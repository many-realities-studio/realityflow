using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.NetworkedBehaviour;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;
public class MyNetworkedObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId {get; set;}
    NetworkContext context;
    public bool owner;
    public bool isHeld;
    private GameObject obj;
    private ObjectManipulator manipulator;
    private MyNetworkedObject myObject;
    private Rigidbody rb;
    bool lastOwner;
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;
    Color lastColor;
    //bool lastGravity;

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
        Debug.Log(context.Scene.Id);
    }

    void Awake()
    {
        owner = false;
        isHeld = false;
        manipulator = this.GetComponent<ObjectManipulator>();
        myObject = this.GetComponent<MyNetworkedObject>();
        obj = this.gameObject;
        rb = this.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        if(this.NetworkId == null)
            Debug.Log("Networked Object " + obj.name + " Network ID is null");
        //color = obj.GetComponent<Renderer>().material.color;
    }

    // Set object owner to whoever picks the object up, and set isHeld to true for every user in scene since object is being held
    public void StartHold()
    {
            if (!owner && isHeld)
                return;


            owner = true;
            isHeld = true;
            rb.isKinematic = false;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                scale = transform.localScale,
                rotation = transform.localRotation,
                owner = false,
                isHeld = true,
                isKinematic = true,
                color = obj.GetComponent<Renderer>().material.color
                // gravity = obj.GetComponent<Rigidbody>().useGravity
            }); 
    }

    // Set isHeld to false for all users when object is no longer currently being held
    public void EndHold()
    {
        isHeld = false;
        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = true,
            isHeld = false,
            isKinematic = true,
            color = obj.GetComponent<Renderer>().material.color
            // gravity = obj.GetComponent<Rigidbody>().useGravity
        });
    }

    // Update is called once per frame
    void Update()
    {
        // If currently not the owner and the object is being held by someone, disable ObjectManipulator so it can be moved
        // if (!owner && isHeld)
        //     this.gameObject.GetComponent<ObjectManipulator>().enabled = false;
        // else
        //     this.gameObject.GetComponent<ObjectManipulator>().enabled = true;

        // // Update object positioning if the object is owned
        // // If you currently own the object, physics calculations are made on your device and transmitted to the rest for that object
        // if(owner)
        // {
            if(lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation || lastColor != obj.GetComponent<Renderer>().material.color)
            {
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;
                lastOwner = myObject.owner;
                lastColor = obj.GetComponent<Renderer>().material.color;
                // lastGravity = obj.GetComponent<Rigidbody>().useGravity;

                // Send position details to the rest of the users in the lobby
                context.SendJson(new Message()
                {
                    position = transform.localPosition,
                    scale = transform.localScale,
                    rotation = transform.localRotation,
                    owner = false, 
                    isHeld = isHeld,
                    isKinematic = true,
                    color = obj.GetComponent<Renderer>().material.color
                    // gravity = obj.GetComponent<Rigidbody>().useGravity
                });
            }
        //}
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
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;
        myObject.owner = m.owner;
        myObject.isHeld = m.isHeld;
        rb.isKinematic = m.isKinematic;
        obj.GetComponent<Renderer>().material.color = m.color;
        //obj.GetComponent<Rigidbody>().useGravity = m.gravity;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = obj.transform.localPosition;
        lastScale = obj.transform.localScale;
        lastRotation = obj.transform.localRotation;
        lastOwner = myObject.owner;
        lastColor = obj.GetComponent<Renderer>().material.color;
        //lastGravity = obj.GetComponent<Rigidbody>().useGravity;
    }
}