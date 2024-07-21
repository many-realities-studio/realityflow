using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using TransformTypes;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;

public class NetworkedPrefab : MonoBehaviour
{
    NetworkContext context;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;

    void Start()
    {
        context = NetworkScene.Register(this);
    }

    void Update()
    {
        if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
        {
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;

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