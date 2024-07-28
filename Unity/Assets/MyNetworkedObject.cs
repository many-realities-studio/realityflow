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
using UnityEngine.InputSystem.LowLevel;


public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    // Ubiq ID and the Network Context
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;

    // For Tracking Transform Changes
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;
    public float lastSize = 0.1f;

    // Color and Gravity
    Color lastColor;
    bool lastGravity;

    // Ownership and Manipulation
    public bool owner;
    bool lastOwner;
    public bool isHeld;
    public bool isSelected;
    private CustomObjectManipulator manipulator;

    // Networked Play Manager
    public NetworkedPlayManager networkedPlayManager;
    private bool lastPlayModeState;

    // RealityFlow Object References and Components
    public RfObject rfObj;
    private Rigidbody rb;
    private BoxCollider boxCol;

    // Error Handling
    private bool compErr = false;

    void Start()
    {      
        // finds The Networked Play Manager
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();

        // Initialize the Network Context
        if (!context.Id.Valid)
        {
            context = NetworkScene.Register(this);
            Debug.Log("[START][NET-PREFAB]Context ID: " + context.Id);
        }
        else
            Debug.Log("[START][NET-PREFAB]ID is already valid");


        // Get the Custom Object Manipulator
        if (gameObject.GetComponent<CustomObjectManipulator>() != null)
        {
            manipulator = GetComponent<CustomObjectManipulator>();
        }
        else
        {
            // Send error?
            compErr = true;
        }

        // Get the Rigidbody components
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

        // Get the Box Collider components
        if (gameObject.GetComponent<BoxCollider>() != null)
        {
            boxCol = gameObject.GetComponent<BoxCollider>();
        }
        else
        {
            compErr = true;
        }
    }

    // Update is called once per frame 
    // You want to update to send the transform data to the server every frame
    void Update()
    {   
        // debug log to see if we are the owner 
        // delay it so it only logs every few seconds
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log("[UPDATE][NET-PREFAB]OWNER IS: " + owner);
        }

        if (owner)
        {
            // If the transform has changed, send the update
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;

            if (Time.frameCount % 300 == 0)
            {
                Debug.Log("Sending Update: Position=" + lastPosition + ", Scale=" + lastScale + ", Rotation=" + lastRotation);
            }
            
            // Send the transform data to the server
            SendTransformData();
        }   
    }

    public void SendTransformData()
    {
        // Debug.Log("[SEND][NET-PREFAB]SendTransformData() was called");

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation, 
            rfObj = rfObj,
        });
    }

    public void InitializePrefab(bool isOwner, Vector3 prefabPosition, Vector3 prefabScale, Quaternion prefabRotation, RfObject passedRfObj)
    {
        // Set the RealityFlow Object
        rfObj = passedRfObj;

        // Change the object's name to the rf object's name
        if (gameObject.name != rfObj.id)
        {
            gameObject.name = rfObj.id;
        }
        
        // Set the owner of the object
        owner = isOwner; // IsOwner should be true
        Debug.Log("[INIT-PREFAB][NET-PREFAB]OWNER IS: " + owner);

        // Set the transform of the object
        transform.localPosition = prefabPosition;
        transform.localScale = prefabScale;
        transform.localRotation = prefabRotation;

        // Update the last known transform to avoid feedback loop
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;

        // Initialize the Network Context
        if (!context.Id.Valid)
        {
            context = NetworkScene.Register(this);
            Debug.Log("[START][NET-PREFAB]Context ID: " + context.Id);
        }
        else
            Debug.Log("[START][NET-PREFAB]ID is already valid");

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            rfObj = rfObj,
            owner = false,
            // isHeld = isHeld,
            // isSelected = isSelected,
        });

        Debug.Log("[INIT-PREFAB][NET-PREFAB]INIT MESSAGE SENT");
        Debug.Log("[INIT-PREFAB][NET-PREFAB]OWNER IS: " + owner);
    }

    #region Selection and Holding   
    public void ControlSelection()
    {
        BoundsControl boundsControl = GetComponent<BoundsControl>();
        if (boundsControl != null && boundsControl.HandlesActive)
        {
            if (!owner && isSelected)
                return;

            owner = true;
            isSelected = true;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                rfObj = rfObj
            });
        }
        else
        {
            isSelected = false;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                rfObj = rfObj,
            });
        }
    }
    
    public void StartHold()
    {   
        // Debug Saying that we are holding the object
        Debug.Log("[START-HOLD][PREFAB]Started Holding Prefab");
        
        // Change the object's name to the rf object's name
        if (gameObject.name != rfObj.id)
        {
            gameObject.name = rfObj.id;
        }

        // The Moved Object needs to be the owner in order to send Updates
        owner = true; // Person who starts holding the object becomes owner
        isHeld = true;


        // Get the rigid Body Component
        if (!rb)
            rb = GetComponent<Rigidbody>();


        // Determines the behaivior of the object depending on play and edit mode
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

        // Log the transformation at the start of holding
        RealityFlowAPI.Instance?.actionLogger?.LogAction(
            nameof(RealityFlowAPI.UpdateObjectTransform), // Action name to match the API
            rfObj.id,
            transform.localPosition,
            transform.localRotation,
            transform.localScale
        ); 
        // debug for the rf obj id
        Debug.Log("[START-HOLD][PREFAB]RF OBJECT ID: " + rfObj.id);

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            rfObj = rfObj,
            owner = false,
        });

    }

    public void EndHold()
    {
        // Debug Saying that we are holding the object
        Debug.Log("[END-HOLD][PREFAB]Ended Holding Prefab");

        // The Moved Object needs to be the owner in order to send Updates
        owner = true; // Person who starts holding the object becomes owner
        isHeld = false;

        // Get the rigid Body Component
        if (!rb)
            rb = GetComponent<Rigidbody>();


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

            //rb.useGravity = true;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        
        //Updates the object's transform
        RealityFlowAPI.Instance.UpdatePrefab(gameObject);

        //Sends Ubiq Update
        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            rfObj = rfObj,
            owner = false, //Set Others to Not Owner
        });
    }

    #endregion
    
    // THE MESSAGE STRUCTURE
    public struct Message
    {
        
        public Vector3 position;      // Transform Data
        public Vector3 scale;
        public Quaternion rotation;
        public RfObject rfObj;         // RfObject Data
        public bool owner;             // Ownership Data
    }

    // THE MESSAGE PROCESSOR
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log("Received Message: Position=" + m.position + ", Scale=" + m.scale + ", Rotation=" + m.rotation);
        }

        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;

        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;

        // Update the RealityFlow Object
        rfObj = m.rfObj;

        //Update Ownership
        owner = m.owner;
    }
}