using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;

public class NetworkedPrefab : MonoBehaviour
{
    NetworkContext context;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;

    void Start()
    {
        context = NetworkScene.Register(this);
        Debug.Log("Registered with NetworkScene, Context ID: " + context.Id);
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