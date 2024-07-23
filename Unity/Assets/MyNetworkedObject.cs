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

    void Start()
    {
        if (!context.Id.Valid)
            context = NetworkScene.Register(this);
        else
            Debug.Log("ID is already valid");

        Debug.Log("[NETPREFAB]Context ID: " + context.Id);

    }

    void Update()
    {
        if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
        {
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;

            Debug.Log("Sending Update: Position=" + lastPosition + ", Scale=" + lastScale + ", Rotation=" + lastRotation);

            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation
            });
        }
    }

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

    private struct Message
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
    }
}