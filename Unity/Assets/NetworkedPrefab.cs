using System;
using UnityEngine;
using Ubiq.Messaging;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;

public class NetworkedPrefab : MonoBehaviour
{
    // -- The NetworkId and Context are required for the NetworkScene to function
    public NetworkId NetworkId { get; set; }
    NetworkContext context;
    public bool owner;

    // -- Select Tool variables
    public bool isHeld;
    public bool isSelected;
    private BoundsControl boundsControl;
    private GameObject boundsVisuals;
    private Material meshMaterial;
    private Material boundsMaterial;
    private ObjectManipulator objectManipulator;
    private EraserTool eraser;
    bool lastOwner;

    // -- The Information we want to synchronize
    Vector3 lastPosition;
    Vector3 lastScale;
    Quaternion lastRotation;


    // Start is called before the first frame update
    void Start()
    {
        // -- Register the object with the NetworkScene
        if (!context.Id.Valid)
            context = NetworkScene.Register(this);
        else
            Debug.Log("ID is already valid");

        // EraserTool Init
        eraser = FindObjectOfType<EraserTool>();

        // Find the child game object of this mesh that draws the bounds visuals
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.name.Contains("BoundingBox"))
            {
                boundsVisuals = child.gameObject;
                //Debug.Log("boundsVisuals.name = " + boundsVisuals.name + " in the scene: " + gameObject.transform.parent.parent.parent.name);

                // Find the immediate child game object of the bounding box that contains the instanced material
                foreach (Transform child2 in boundsVisuals.transform)
                {
                    try
                    {
                        boundsMaterial = child2.gameObject.GetComponent<MeshRenderer>().material;
                        // Debug.Log("child2.gameObject.name = " + child2.gameObject.name + " child2.gameObject.GetComponent<MeshRenderer>().material.HasProperty('Color') = "
                        // + child2.gameObject.GetComponent<MeshRenderer>().material.HasProperty("_Color_"));

                        // if (boundsMaterial.HasProperty("_Color_"))
                        // {
                        //     Debug.Log("The current color of bounds visuals for mesh: " + child2.gameObject.name + " in the scene: "
                        //     + child2.gameObject.transform.parent.parent.parent.parent.parent.name + " is the color: " + boundsMaterial.GetColor("_Color_"));
                        // }
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e + " Does " + child2.gameObject.name + " have the ThickerSqueezableBox material?");
                    }
                    break;
                }
            }
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
                lastOwner = owner;

                SendTransformData();
            }
        }
    }

    public struct Message
    {
        // -- The information we want to synchronize --
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public bool owner;
        // public string mOwnerName;
        public bool isHeld;
        public bool isSelected;
        public bool handlesActive;
        public bool boundsVisuals;
        public Color meshColor;
        public float meshMetallic;
        public float meshSmoothness;
        public Color boundsColor;
        public bool objectManipulator;


    }
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use ther parsed message to update the object
        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;

        owner = m.owner;
        // ownerName = m.mOwnerName;
        isHeld = m.isHeld;
        isSelected = m.isSelected;
        boundsControl.HandlesActive = m.handlesActive;
        boundsVisuals.SetActive(m.boundsVisuals);
        meshMaterial.SetColor("_Color", m.meshColor);
        meshMaterial.SetFloat("_Metallic", m.meshMetallic);
        meshMaterial.SetFloat("_Glossiness", m.meshSmoothness);
        boundsMaterial.SetColor("_Color_", m.boundsColor);


        // Make sure update doesn't trigger as a result of the message
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;
        lastOwner = owner;
    }

    public void SendTransformData()
    {
        // Debug.Log("SendTransformData() was called");

        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,
            owner = false,
            isHeld = isHeld,
            isSelected = isSelected,
            handlesActive = boundsControl.HandlesActive,
            boundsVisuals = boundsVisuals.activeInHierarchy,
            meshColor = meshMaterial.color,
            meshMetallic = meshMaterial.GetFloat("_Metallic"),
            meshSmoothness = meshMaterial.GetFloat("_Glossiness"),
            boundsColor = new Color(1f, 0.21f, 0.078f, 1f),
            
        });
    }
}
