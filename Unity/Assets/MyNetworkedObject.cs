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
    bool lastOwner;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;
    Color lastColor;

    // variables for entering and exiting playmode
    public NetworkedPlayManager networkedPlayManager;
    private bool lastPlayModeState;
    private RfObject rfObj;
    private bool compErr = false;

    void Start()
    {
        rfObj = RealityFlowAPI.Instance.SpawnedObjects[gameObject];
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();

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

        if (gameObject.GetComponent<ObjectManipulator>() != null)
        {
            manipulator = GetComponent<ObjectManipulator>();
        }
        else
        {
            compErr = true;
        }

        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
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

        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
        }
        else
        {
            rb.useGravity = false;
        }

        Debug.Log("Started hold the action is now being logged to the ActionLogger");
        RealityFlowAPI.Instance.actionLogger.LogAction(
            nameof(RealityFlowAPI.UpdateObjectTransform), // Action name to match the API
            rfObj.id,
            transform.localPosition,
            transform.localRotation,
            transform.localScale
        );

        UpdateTransform();
    }

    // Set isHeld to false for all users when object is no longer currently being held
    public void EndHold()
    {
        if (!rb)
            rb = GetComponent<Rigidbody>();

        isHeld = false;

        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        else
        {
            if (!compErr)
            {
                if (rfObj.isStatic)
                {
                    rb.isKinematic = true;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                else
                {
                    rb.isKinematic = false;
                }

                if (rfObj.isGravityEnabled)
                {
                    rb.useGravity = true;
                }
                else
                {
                    rb.useGravity = false;
                }

                if (rfObj.isCollidable)
                {
                    boxCol.enabled = true;
                }
                else
                {
                    boxCol.enabled = false;
                }
            }
        }

        UpdateTransform();

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
    private void UpdateTransform()
    {
        if (!owner && isHeld)
            this.gameObject.GetComponent<ObjectManipulator>().enabled = false;
        else
            this.gameObject.GetComponent<ObjectManipulator>().enabled = true;

        if (owner)
        {
            if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation || lastColor != gameObject.GetComponent<Renderer>().material.color)
            {
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;
                lastOwner = owner;
                lastColor = gameObject.GetComponent<Renderer>().material.color;

                context.SendJson(new Message()
                {
                    position = transform.localPosition,
                    scale = transform.localScale,
                    rotation = transform.localRotation,
                    owner = false,
                    isHeld = isHeld,
                    isKinematic = true,
                    color = gameObject.GetComponent<Renderer>().material.color
                });
            }
        }
    }

    public struct Message
    {
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
        var m = message.FromJson<Message>();

        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;
        owner = m.owner;
        isHeld = m.isHeld;
        rb.isKinematic = m.isKinematic;
        gameObject.GetComponent<Renderer>().material.color = m.color;

        lastPosition = gameObject.transform.localPosition;
        lastScale = gameObject.transform.localScale;
        lastRotation = gameObject.transform.localRotation;
        lastOwner = owner;
        lastColor = gameObject.GetComponent<Renderer>().material.color;
    }
}