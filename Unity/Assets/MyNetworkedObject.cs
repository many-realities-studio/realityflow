using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    // Ubiq ID and the Network Context
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;

    // For Tracking Transform Changes
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Quaternion lastRotation;

    // Ownership and Manipulation
    public bool owner;
    public bool isHeld;
    public bool isSelected;

    // Rigidbody and Collider
    private Rigidbody rb;
    private BoxCollider boxCol;

    // Error Handling
    private bool compErr = false;

    void Start()
    {      
        // Initialize the Network Context
        if (!context.Id.Valid)
            context = NetworkScene.Register(this);
        else
            Debug.Log("ID is already valid");

        // Get the Rigidbody components
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
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

    void Awake()
    {
        owner = false;
        isHeld = false;
        isSelected = false;
    }

    // Update is called once per frame 
    void Update()
    {   
        if (owner)
        {
            if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
            {   
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;

                SendTransformData();
            }
        }
    }

    private void SendTransformData()
    {
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

    public void ControlSelection()
    {
        if (owner)
        {
            isSelected = !isSelected;
            SendTransformData();
        }
    }
    
    public void StartHold()
    {   
        if (!owner)
            return;

        if (!rb)
            rb = GetComponent<Rigidbody>();

        isHeld = true;
        rb.isKinematic = !rb.useGravity;
        SendTransformData();
    }

    public void EndHold()
    {
        if (!rb)
            rb = GetComponent<Rigidbody>();

        isHeld = false;
        SendTransformData();
    }

    // Message structure for network communication
    private struct Message
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public bool owner;
        public bool isHeld;
        public bool isSelected;
    }

    // Process incoming network messages
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();

        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;

        // Update last known transform to avoid feedback loop
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
    }
}