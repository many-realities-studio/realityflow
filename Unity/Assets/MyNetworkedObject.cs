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

        // If we are not in play mode, have no gravity and allow the object to move while held,
        // similarly allow thw object to be moved in playmode without gravity on hold.
        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = false;

            // This would also be a place to change to boxcolliders collider interaction masks so that
            // the object can be placed within others to prevent it from colliding with UI.
            // TODO: 

        }
        else
        {
            rb.useGravity = false;
        }

        Debug.Log("Started hold the action is now being logged to the ActionLogger");
        // Log the transformation at the start of holding
        RealityFlowAPI.Instance.actionLogger.LogAction(
        nameof(RealityFlowAPI.UpdateObjectTransform), // Action name to match the API
        rfObj.id,
        transform.localPosition,
        transform.localRotation,
        transform.localScale
        );



        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = false,
            isHeld = true,
            //isSelected = isSelected,
            isKinematic = true//,
            //color = gameObject.GetComponent<Renderer>().material.color
            // gravity = obj.GetComponent<Rigidbody>().useGravity
        });
    }

    // Set isHeld to false for all users when object is no longer currently being held
    public void EndHold()
    {
        isHeld = false;
        rb.isKinematic = true;

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

        UpdateTransform();
    }

    // Update method to be called within StartHold and EndHold
    public void UpdateTransform()
    {
        if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation || lastColor != GetComponent<Renderer>().material.color)
        {
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;
            lastColor = GetComponent<Renderer>().material.color;

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
}