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

    void Start()
    {
        context = NetworkScene.Register(this);
        
        owner = false;
        isHeld = false;

        manipulator = GetComponent<ObjectManipulator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        boxCol = GetComponent<BoxCollider>();
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
        rb.isKinematic = true;

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

    public struct Message
    {
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
}