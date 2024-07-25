using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;
using System;

public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;

    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;

    public bool owner;
    public bool isHeld;
    public bool isSelected;
    private CustomObjectManipulator manipulator;

    public NetworkedPlayManager networkedPlayManager;

    private Rigidbody rb;
    private BoxCollider boxCol;

    private bool compErr = false;

    void Awake()
    {
        Debug.Log("[NETOBJECT] Awake is called");
        owner = false;
        isHeld = false;
        isSelected = false;

        // Initialization that does not depend on other scripts
        InitializeComponents();
    }

    void Start()
    {
        Debug.Log("[NETOBJECT] Start is called");

        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();

        context = NetworkScene.Register(this);

        Debug.Log("[NETOBJECT] Context ID: " + context.Id);

        // Initialization that might depend on other scripts or components being initialized
        SendInitialTransformData();
    }

    void InitializeComponents()
    {
        if (gameObject.GetComponent<CustomObjectManipulator>() != null)
        {
            manipulator = GetComponent<CustomObjectManipulator>();
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

    void Update()
    {   
        if (owner)
        {
            if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
            {   
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;

                Debug.Log("Sending Update: Position=" + lastPosition + ", Scale=" + lastScale + ", Rotation=" + lastRotation);

                SendTransformData();
            }
        }   
    }

    public void SendTransformData()
    {
        Debug.Log("[NETOBJECT] SendTransformData is called");
        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = owner,
            isHeld = isHeld,
            isSelected = isSelected
        });
    }

    void SendInitialTransformData()
    {
        Debug.Log("[NETOBJECT] SendInitialTransformData is called");
        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = owner,
            isHeld = isHeld,
            isSelected = isSelected
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        Debug.Log("[NETOBJECT] ProcessMessage received: Position=" + m.position + ", Scale=" + m.scale + ", Rotation=" + m.rotation);

        // Prevent ownership changes if already owned
        if (!owner)
        {
            transform.localPosition = m.position;
            transform.localScale = m.scale;
            transform.localRotation = m.rotation;
            owner = m.owner;

            isHeld = m.isHeld;
            isSelected = m.isSelected;

            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;
        }
    }

    public struct Message
    {
        public bool owner;
        public bool isHeld;
        public bool isSelected;
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
    }

    public void StartHold()
    {
        Debug.Log("[NETOBJECT] StartHold is called");

        if (!owner)
        {
            return;
        }

        owner = true;
        isHeld = true;

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = owner,
            isHeld = isHeld,
            isSelected = isSelected
        });
    }

    public void EndHold()
    {
        Debug.Log("[NETOBJECT] EndHold is called");

        owner = false;
        isHeld = false;

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = owner,
            isHeld = isHeld,
            isSelected = isSelected
        });
    }
}