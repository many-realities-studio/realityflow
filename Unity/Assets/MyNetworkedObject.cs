using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
//using Ubiq.Rooms;
using Ubiq.NetworkedBehaviour;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;
using System;


public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;
    public bool owner;
    public bool isHeld;
    public bool isSelected;
    bool lastOwner;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;
    Color lastColor;
    bool lastGravity;
    public NetworkedPlayManager networkedPlayManager;
    public RfObject rfObj;
    private CustomObjectManipulator manipulator;
    private Rigidbody rb;
    private BoxCollider boxCol;
    private bool lastPlayModeState;

    private bool compErr = false;

    void Start()
    {
        rfObj = RealityFlowAPI.Instance.SpawnedObjects[gameObject];
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();

        // If our context is invalid, register this instance of myNetworkedObject with the scene.
        if (!context.Id.Valid)
        {
            try
            {
                context = NetworkScene.Register(this);
                Debug.Log(context.Scene.Id);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        else
            Debug.Log("ID is already valid");

        owner = false;
        isHeld = false;

        if (gameObject.GetComponent<CustomObjectManipulator>() != null)
        {
            manipulator = GetComponent<CustomObjectManipulator>();
        }
        else
        {
            compErr = true;
        }

        // These should throw errors on failure (object doesn't have these components) TODO some other time:
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            compErr = true;
        }

        if (gameObject.GetComponent<BoxCollider>() != null)
        {
            boxCol = gameObject.GetComponent<BoxCollider>();
        }
        else
        {
            compErr = true;
        }

        RequestRfObject();
        //color = obj.GetComponent<Renderer>().material.color;
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
        isSelected = false;
        // if(lastSize ! = 0.1f;
        //boundsControl = gameObject.GetComponent<BoundsControl>();
        //meshMaterial = gameObject.GetComponent<MeshRenderer>().material;
        //boundsControl.HandlesActive = false;

        if (NetworkId == null)
            Debug.Log("Networked Object " + gameObject.name + " Network ID is null");
    }

    void Update()
    {
        if (owner)
        {
            if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
            {
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;
                lastOwner = owner;

                SendTransformData();
            }
        }
    }

    public void SendTransformData()
    {
        // Debug.Log("SendTransformData() was called");

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = false,
            isHeld = isHeld,
            isKinematic = rb.isKinematic
            //color = GetComponent<Renderer>().material.color
            //gravity = rb.useGravity
        });

        //wasBake = false;
    }

    // Set object owner to whoever picks the object up, and set isHeld to true for every user in scene since object is being held
    public void StartHold()
    {
        if (!owner && isHeld)
            return;

        if (!rb)
            rb = GetComponent<Rigidbody>();

        owner = true;
        isHeld = true;


        // If we are not in play mode, have no gravity and allow the object to move while held,
        // similarly allow thw object to be moved in playmode without gravity on hold.
        if (!networkedPlayManager.playMode)
        {
            //rb.useGravity = false;
            rb.isKinematic = false;

            rb.constraints = RigidbodyConstraints.None;

            // This would also be a place to change to boxcolliders collider interaction masks so that
            // the object can be placed within others to prevent it from colliding with UI.
            // TODO: 

        }
        else
        {
            //rb.useGravity = true;
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
            isKinematic = true,//,
            //color = gameObject.GetComponent<Renderer>().material.color
            gravity = rb.useGravity
        });
    }

    // Set isHeld to false for all users when object is no longer currently being held
    public void EndHold()
    {
        if (!rb)
            rb = GetComponent<Rigidbody>();

        owner = false;
        isHeld = false;

        // When we are not in play mode, have the object remain where you let it go, otherwise, follow what is the property of
        // the rf obj for play mode.
        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = true;

            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            // If we are in play mode, we will want to have the object return to it's object property behaviors.

            // if we have are missing a component don't mess with the object's physics
            if (!compErr)
            {
                Debug.Log("WE DID NOT RUN INTO A COMP ERROR");
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
                    //rb.useGravity = true;
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

            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                owner = false,
                isHeld = false,
                isKinematic = true,//,
                //color = gameObject.GetComponent<Renderer>().material.color
                gravity = rb.useGravity
            });

        }

        //RealityFlowAPI.Instance.UpdateObjectTransform(rfObj.id, transform.localPosition, transform.localRotation, transform.localScale);

        //UpdateTransform();

        // Save the object's transform to the database
        TransformData transformData = new TransformData()
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };

        RealityFlowAPI.Instance.SaveObjectTransformToDatabase(rfObj.id, transformData);
    }

    // Update method to be called within StartHold and EndHold
    public void UpdateTransform()
    {
        if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
        {
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;
            //lastColor = GetComponent<Renderer>().material.color;

            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                owner = owner,
                isHeld = isHeld,
                isKinematic = rb.isKinematic
                //color = GetComponent<Renderer>().material.color
            });
        }
    }


    // Update is called once per frame
    //void Update()
    //{
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

            // Send position details to the rest of the users in the lobby
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                owner = false, 
                isHeld = isHeld,
                isKinematic = true,
                color = obj.GetComponent<Renderer>().material.color
                // gravity = obj.GetComponent<Rigidbody>().useGravity
            });
        }
    }
    */
    //}

    public void UpdateRfObject(RfObject rfObj)
    {
        this.rfObj = rfObj;
        GetComponent<CacheObjectData>().rfObj = rfObj;

        // Sometimes, such as when spawned from the mesh menu, this object will not have run Start()
        // yet and end up failing to have a networkcontext; in this case yield until it can send the 
        // message
        IEnumerator SendRfObjUpdate()
        {
            while (!context.Scene)
                yield return null;

            context.SendJson(new Message()
            {
                rfObj = this.rfObj
            });
        }

        StartCoroutine(SendRfObjUpdate());
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
        public bool gravity;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();
        if (m.rfObj != null)
        {
            Debug.Log("Received rfObject");
            rfObj = m.rfObj;
            GetComponent<CacheObjectData>().rfObj = rfObj;
            RealityFlowAPI.Instance.RegisterPeerSpawnedObject(gameObject, rfObj);
            return;
        }
        if (m.needsRfObject && rfObj != null)
        {
            context.SendJson(new Message()
            {
                rfObj = rfObj
            });
        }

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;
        owner = m.owner;
        isHeld = m.isHeld;
        rb.isKinematic = m.isKinematic;
        //GetComponent<Renderer>().material.color = m.color;
        GetComponent<Rigidbody>().useGravity = m.gravity;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        lastOwner = owner;
        //lastColor = GetComponent<Renderer>().material.color;
        lastGravity = GetComponent<Rigidbody>().useGravity;
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