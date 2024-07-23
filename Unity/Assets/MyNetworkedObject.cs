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
        if (!context.Id.Valid)
            context = NetworkScene.Register(this);
        else
            Debug.Log("ID is already valid");

        Debug.Log("[NETOBJECT]Context ID: " + context.Id);

    }

    // Update is called once per frame 
    // You want to update to send the transform data to the server every frame
    void Update()
    {
        if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
        {   
            // If the transform has changed, send the update
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;

            Debug.Log("Sending Update: Position=" + lastPosition + ", Scale=" + lastScale + ", Rotation=" + lastRotation);

            // Send the transform data to the server
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation
            });
        }
    }

    public void UpdateRfObject(RfObject rfObj)
    {
        StartCoroutine(UpdateRfObjectCoroutine(rfObj));
    }

    private IEnumerator UpdateRfObjectCoroutine(RfObject rfObj)
    {
        while (context.Scene == null || !context.Id.Valid)
        {
            Debug.LogWarning("[MyNetworkedObject] Waiting for NetworkContext to be initialized...");
            yield return new WaitForSeconds(0.1f);
        }

        this.rfObj = rfObj;

        CacheObjectData cacheObjectData = GetComponent<CacheObjectData>();
        if (cacheObjectData != null)
        {
            cacheObjectData.rfObj = rfObj;
        }
        else
        {
            Debug.LogError("[MyNetworkedObject] CacheObjectData component is missing.");
        }

        // context.SendJson(new Message()
        // {
        //     rfObj = this.rfObj
        // });
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
                // owner = false,
                // isSelected = true,
                // isKinematic = rb.isKinematic
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
                // owner = false,
                // isSelected = false,
                // isKinematic = rb.isKinematic
            });
        }
    }
    
    public void StartHold()
    {
        if (!owner && isHeld)
            return;

        if (!rb)
            rb = GetComponent<Rigidbody>();

        owner = true;
        isHeld = true;


        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
        }
        else
        {
            rb.useGravity = true;
        }

        RealityFlowAPI.Instance.actionLogger.LogAction(
            nameof(RealityFlowAPI.UpdateObjectTransform),
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
            //owner = false,
            //isHeld = true,
            //isSelected = isSelected,
            //isKinematic = true,//,
            //color = gameObject.GetComponent<Renderer>().material.color
            //gravity = rb.useGravity
        });
    }

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
                    rb.useGravity = true;
                    //rb.useGravity = false;
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
                // owner = false,
                // isHeld = false,
                // isKinematic = true,
                // color = gameObject.GetComponent<Renderer>().material.color
                // gravity = rb.useGravity
            });

            //rb.useGravity = true;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        RealityFlowAPI.Instance.UpdateObjectTransform(rfObj.id, transform.localPosition, transform.localRotation, transform.localScale);

        // UpdateTransform();

        // Save the object's transform to the database
        TransformData transformData = new TransformData()
        {
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale
        };

        RealityFlowAPI.Instance.SaveObjectTransformToDatabase(rfObj.id, transformData);
    }

    #endregion
    
    // THE MESSAGE STRUCTURE
    private struct Message
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        // public bool owner;
        // public bool isHeld;
        // public bool isKinematic;
        // public Color color;
        // public bool gravity;
    }

    // THE MESSAGE PROCESSOR
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        Debug.Log("Received Message: Position=" + m.position + ", Scale=" + m.scale + ", Rotation=" + m.rotation);

        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;

        // Update last known transform to avoid feedback loop
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
    }


}