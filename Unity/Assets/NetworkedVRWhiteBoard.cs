using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.NetworkedBehaviour;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkedVRWhiteBoard : MonoBehaviour, INetworkSpawnable
{
   public NetworkId NetworkId {get; set;}
    NetworkContext context;
    public bool owner;
    private GameObject obj;
    private ObjectManipulator manipulator;
    private NetworkedVRWhiteBoard whiteBoard;
    bool lastOwner;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;

    // Start is called before the first frame update
    void Start()
    {
        // this.NetworkId = NetworkId.Unique();
        context = NetworkScene.Register(this);
        
        // Debug.Log("Connected to Server");
    }

    void Awake()
    {
        
        owner = false;
        obj = this.gameObject;
        manipulator = GetComponent<ObjectManipulator>();
        whiteBoard = GetComponent<NetworkedVRWhiteBoard>();
        if(this.NetworkId == null)
        {
            Debug.Log("Network ID is null");
        }
        
    }

    public void SetOwner()
    {
        if(!owner)
        {
            owner = true;
            Debug.Log("Owner set");
            obj = this.gameObject;
        }
        else
        {
            // make object unmovable
            manipulator.AllowedManipulations = Microsoft.MixedReality.Toolkit.TransformFlags.None;
        }
    }

    public void RemoveOwner()
    {
        owner = false;
        context.SendJson(new Message()
        {
            position = obj.transform.localPosition,
            scale = obj.transform.localScale,
            rotation = obj.transform.localRotation,
            owner = false
        }); 
        Debug.Log("Owner removed");
    }

    // Update is called once per frame
    void Update()
    {
        // update position, scale, and rotation
        if(owner)
        {
            if(lastPosition != obj.transform.localPosition || lastScale != obj.transform.localScale || lastRotation != obj.transform.localRotation)
            {
                lastPosition = obj.transform.localPosition;
                lastScale = obj.transform.localScale;
                lastRotation = obj.transform.localRotation;
                lastOwner = whiteBoard.owner;

                context.SendJson(new Message()
                {
                    position = obj.transform.localPosition,
                    scale = obj.transform.localScale,
                    rotation = obj.transform.localRotation,
                    owner = true
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
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        obj.transform.localPosition = m.position;
        obj.transform.localScale = m.scale;
        obj.transform.localRotation = m.rotation;
        whiteBoard.owner = m.owner;
        

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = obj.transform.localPosition;
        lastScale = obj.transform.localScale;
        lastRotation = obj.transform.localRotation;
        lastOwner = whiteBoard.owner;
    }
}
