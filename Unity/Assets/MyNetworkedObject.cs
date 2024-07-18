using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System;

public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;
    public bool owner;
    public bool isHeld;
    private ObjectManipulator manipulator;
    private Rigidbody rb;
    private BoxCollider boxCol;
    public RfObject rfObj;
    public bool isSelected;
    bool lastOwner;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;
    Color lastColor;
    // variables for entering and exiting playmode
    public NetworkedPlayManager networkedPlayManager;
    private ObjectManipulator manipulator;
    private Rigidbody rb;
    private BoxCollider boxCol;
    private bool lastPlayModeState;
    private RfObject rfObj;
    //bool lastGravity;
    private bool compErr = false;

    void Start()
    {
        Debug.Log("Starting networked object");
        context = NetworkScene.Register(this);
        
        owner = false;
        isHeld = false;

        manipulator = GetComponent<ObjectManipulator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        boxCol = GetComponent<BoxCollider>();
        RequestRfObject();
    }

    public void RequestRfObject()
    {
        Message msg = new Message();
        msg.needsRfObject = true;
        context.SendJson(msg);
    }

    void Awake()
    {
        owner = false;
        isHeld = false;
        //isSelected = false;
        // if(lastSize ! = 0.1f;
        //boundsControl = gameObject.GetComponent<BoundsControl>();
        //meshMaterial = gameObject.GetComponent<MeshRenderer>().material;
        //boundsControl.HandlesActive = false;

        if (NetworkId == null)
            Debug.Log("Networked Object " + gameObject.name + " Network ID is null");
    }

    // Set object owner to whoever picks the object up, and set isHeld to true for every user in scene since object is being held
    public void StartHold()
    {
        owner = true;
        isHeld = true;
        rb.isKinematic = false;

        UpdateTransform();
    }

    // Set isHeld to false for all users when object is no longer currently being held
    public void EndHold()
    {
        isHeld = false;

        RealityFlowAPI.Instance.UpdateObjectTransform(rfObj.id, transform.localPosition, transform.localRotation, transform.localScale);

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = true,
            isHeld = false,
            isKinematic = true//,
            //color = gameObject.GetComponent<Renderer>().material.color
            // gravity = obj.GetComponent<Rigidbody>().useGravity
        });

        // When we are not in play mode, have the object remain where you let it go, otherwise, follow what is the property of
        // the rf obj for play mode.
        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        else
        {
            // If we are in play mode, we will want to have the object return to it's object property behaviors.

            // if we have are missing a component don't mess with the object's physics
            if (!compErr)
            {
                // Depending on the rf obj properties, behave appropraitely in play mode
                // TODO: Move to it's own component (Like RFobject manager or something)
                //       Include the playmode switch stuff
                // if static, be still on play
                if (rfObj.isStatic)
                {
                    rb.isKinematic = true;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                else
                {
                    rb.isKinematic = false;
                }

                // if has gravity, apply in play mode
                if (rfObj.isGravityEnabled)
                {
                    rb.useGravity = true;
                }
                else
                {
                    rb.useGravity = false;
                }

                // if the object is collidable
                if (rfObj.isCollidable)
                {
                    boxCol.enabled = true;
                }
                else
                {
                    boxCol.enabled = false;
                }
            }

            //rb.useGravity = true;
            //rb.isKinematic = false;
        }

        // Save the object's transform to the database
        TransformData transformData = new TransformData()
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };

        // SaveObjectTransformToDatabase(rfObj.id, transformData);
    }


    // Update is called once per frame
    void Update()
    {
        /*
        // If currently not the owner and the object is being held by someone, disable ObjectManipulator so it can be moved
        if (!owner && isHeld)
            this.gameObject.GetComponent<ObjectManipulator>().enabled = false;
        else
            this.gameObject.GetComponent<ObjectManipulator>().enabled = true;

        // Update object positioning if the object is owned
        // If you currently own the object, physics calculations are made on your device and transmitted to the rest for that object
        if(owner)
        {
            if(lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation || lastColor != obj.GetComponent<Renderer>().material.color)
            {
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;
                lastOwner = myObject.owner;
                lastColor = obj.GetComponent<Renderer>().material.color;
                // lastGravity = obj.GetComponent<Rigidbody>().useGravity;

            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                owner = owner,
                isHeld = isHeld,
                isKinematic = rb.isKinematic,
                color = GetComponent<Renderer>().material.color
            });
        }
    }

    public void UpdateRfObject(RfObject rfObj)
    {
        this.rfObj = rfObj;
        GetComponent<CacheObjectData>().rfObj = rfObj;
        context.SendJson(new Message()
        {
            rfObj = this.rfObj
        });
    }

    

    public struct Message
    {
        public bool needsRfObject;
        public RfObject rfObj;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public bool owner;
        public bool isHeld;
        public bool isKinematic;
        public Color color;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        if(m.rfObj != null)
        {
            Debug.Log("Received rfObject");
            rfObj = m.rfObj;
            GetComponent<CacheObjectData>().rfObj = rfObj;
            RealityFlowAPI.Instance.RegisterPeerSpawnedObject(gameObject, rfObj);
            return;
        }
        if(m.needsRfObject && rfObj != null) {
            context.SendJson(new Message()
            {
                rfObj = rfObj
            });
        }
        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;
        owner = m.owner;
        isHeld = m.isHeld;
        rb.isKinematic = m.isKinematic;
        GetComponent<Renderer>().material.color = m.color;

        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        lastOwner = owner;
        lastColor = GetComponent<Renderer>().material.color;
    }

    public void ControlSelection()
    {
        //Debug.Log("ControlSelection() was called");
        // If the mesh is selected, then start the selection
        if (gameObject.GetComponent<BoundsControl>().HandlesActive)
        {
            if (!owner && isSelected)
            return;

            //Debug.Log("This mesh is now selected");
            owner = true;
            // ownerName = selectTool.ownerName;
            isSelected = true;
            //boundsControl.HandlesActive = true;
            //boundsVisuals.SetActive(true);
        }
    }

    public void ControlSelection()
    {
        //Debug.Log("ControlSelection() was called");
        // If the mesh is selected, then start the selection
        if (gameObject.GetComponent<BoundsControl>().HandlesActive)
        {
            if (!owner && isSelected)
            return;

            //Debug.Log("This mesh is now selected");
            owner = true;
            // ownerName = selectTool.ownerName;
            isSelected = true;
            //boundsControl.HandlesActive = true;
            //boundsVisuals.SetActive(true);
        }
    }
}