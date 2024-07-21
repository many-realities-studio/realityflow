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
    public RfObject rfObj;
    public bool isSelected;
    bool lastOwner;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;
    Color lastColor;
    public NetworkedPlayManager networkedPlayManager;
    private bool compErr = false;

    void Start()
    {
        Debug.Log("Starting networked object");

        // Register context
        context = NetworkScene.Register(this);

        // Initialize components
        owner = false;
        isHeld = false;

        manipulator = GetComponent<ObjectManipulator>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        boxCol = GetComponent<BoxCollider>();
        if (boxCol == null)
        {
            boxCol = gameObject.AddComponent<BoxCollider>();
        }
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();

        if (networkedPlayManager == null)
        {
            Debug.LogError("[MyNetworkedObject] NetworkedPlayManager not found in the scene");
        }

        RequestRfObject();
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdateTransform();
    }

    public void RequestRfObject()
    {
        Message msg = new Message();
        msg.needsRfObject = true;
        context.SendJson(msg);
    }

    public void StartHold()
    {
        Debug.Log("StartHold called");

        // Ensure all necessary components are available
        if ( rb == null || networkedPlayManager == null || RealityFlowAPI.Instance == null)
        {
            Debug.LogError("StartHold failed due to missing components");
            return;
        }

        owner = true;
        isHeld = true;
        rb.isKinematic = false;

        UpdateTransform();

        if (!networkedPlayManager.playMode)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
        }
        else
        {
            rb.useGravity = false;
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
            owner = false,
            isHeld = true,
            isKinematic = true,
            isSelected = isSelected
        });
    }

    public void EndHold()
    {
        isHeld = false;
        rb.isKinematic = true;

        RealityFlowAPI.Instance.UpdateObjectTransform(rfObj.id, transform.localPosition, transform.localRotation, transform.localScale);

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = true,
            isHeld = false,
            isKinematic = true,
            isSelected = isSelected
        });

        UpdateTransform();
    }

    public void UpdateTransform()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation || lastColor != renderer.material.color)
            {
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;
                lastColor = renderer.material.color;

                context.SendJson(new Message()
                {
                    position = transform.localPosition,
                    scale = transform.localScale,
                    rotation = transform.localRotation,
                    owner = owner,
                    isHeld = isHeld,
                    isKinematic = rb.isKinematic,
                    color = renderer.material.color,
                    isSelected = isSelected
                });
            }
        }
        else
        {
            Debug.LogWarning($"[MyNetworkedObject] No Renderer component found on {gameObject.name}");
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

        context.SendJson(new Message()
        {
            rfObj = this.rfObj
        });
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
        public bool isSelected;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        if (m.rfObj != null)
        {
            rfObj = m.rfObj;
            CacheObjectData cacheObjectData = GetComponent<CacheObjectData>();
            if (cacheObjectData != null)
            {
                cacheObjectData.rfObj = rfObj;
            }
            else
            {
                Debug.LogError("[MyNetworkedObject] CacheObjectData component is missing.");
            }
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
        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;
        owner = m.owner;
        isHeld = m.isHeld;
        rb.isKinematic = m.isKinematic;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = m.color;
        }
        else
        {
            Debug.LogWarning($"[MyNetworkedObject] No Renderer component found on {gameObject.name}");
        }

        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        lastOwner = owner;
        lastColor = renderer != null ? renderer.material.color : Color.white;
    }

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
                owner = false,
                isSelected = true,
                isKinematic = rb.isKinematic
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
                owner = false,
                isSelected = false,
                isKinematic = rb.isKinematic
            });
        }
    }
}