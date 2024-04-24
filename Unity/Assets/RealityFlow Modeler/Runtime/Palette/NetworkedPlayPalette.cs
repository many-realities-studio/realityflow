using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Spawning;
using Ubiq.Messaging;
using UnityEngine.Animations;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Avatars;

[System.Serializable]
public class RealityFlowPlayToolEvent : UnityEvent<int, bool>
{
}

/// <summary>
/// Class NetworkedPlayPalette provides a menu which offers exiting of Play Mode and is connected to the Ubiq Network library.
/// </summary>
public class NetworkedPlayPalette : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;

    public RealityFlowPlayToolEvent m_ToolEvent;
    private GameObject realityFlowTools;

    // Used to untick the Parent Constraint component on server-side palettes and access client-side palette
    private ParentConstraint parentConstraint;
    private ParentConstraint otherPaletteParentConstraint;
    NetworkedPalette[] editPalettes;
    NetworkedPalette myEditPalette;

    [Tooltip("Displays the owner's UUID and should not be manually changed")]
    public string ownerName;

    // The owner of a palette is the user that has spawned it
    public bool owner;

    // Have access to all button states
    [SerializeField] private StateVisualizer[] buttonStates;
    [Header("Toggle States that should not toggle off other toggles")]
    [SerializeField] private PressableButton handToggleState;

    // Button states (which one is currently being hovered by the owner)
    private List<bool> lastButtonStates = new List<bool>();
    private bool lastHandToggleState;

    Vector3 lastPosition, lastScale;
    Quaternion lastRotation;

    public void ExitTool(bool status)
    {
        m_ToolEvent.Invoke(-1, status);
    }

    void Awake()
    {
        if (this.NetworkId == null)
            Debug.Log("Networked Object " + gameObject.name + " Network ID is null");
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        // These may not be necessary unless a user joins the room during a owner's interaction of their palette
        // Initialize "last" button states to that of the owner
        for (int i = 0; i < buttonStates.Length; i++)
        {
            lastButtonStates.Add(buttonStates[i].Interactable.IsActiveHovered);
        }

        lastHandToggleState = handToggleState.IsToggled;

        if (owner)
        {
            if(m_ToolEvent == null)
            {
                m_ToolEvent = new RealityFlowPlayToolEvent();
            }
            realityFlowTools = GameObject.Find("RealityFlow Editor");
            PlayModeSpawner exitTool = realityFlowTools.GetComponentInChildren<PlayModeSpawner>();
            m_ToolEvent.AddListener(exitTool.Activate);

            Ubiq.Avatars.Avatar[] avatars = context.Scene.GetComponentsInChildren<Ubiq.Avatars.Avatar>();

            // Go through every avatar looking for the palette owner's avatar.
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].ToString().Contains(AvatarManager.UUID))
                {
                    ownerName = AvatarManager.UUID;
                }
            }

            UpdateDominantHand();
        }
        // Users should not be able to interact with others' palettes
        else if (!owner)
        {
            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);

            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }
    }

    public void UpdateDominantHand()
    {
        // Debug.Log("Running dominant hand in play palette");
        parentConstraint = gameObject.GetComponent<ParentConstraint>();
        
        Ubiq.Avatars.Avatar[] avatars = context.Scene.GetComponentsInChildren<Ubiq.Avatars.Avatar>();

        if (myEditPalette != null)
        {
            UpdateEditPaletteHand();
        }
        else
        {
            // Find this owners play palette
            editPalettes = FindObjectsOfType<NetworkedPalette>();
            foreach (NetworkedPalette editPalette in editPalettes)
            {
                if (editPalette.ownerName == ownerName && editPalette.owner)
                {
                    myEditPalette = editPalette;

                    if (myEditPalette != null)
                    {
                        otherPaletteParentConstraint = myEditPalette.gameObject.GetComponent<ParentConstraint>();
                    }
                    // Debug.Log("Found edit palette.name = " + myEditPalette.name);
                }
            }

            // When UpdateDominantHand() is called in Start then the play palette will be null, afterwards once this method gets called again will it be found
            if (myEditPalette != null)
            {
                UpdateEditPaletteHand();
            }
        }

        // Initialize which hand the palette owner has for their dominant (default is right hand)
        gameObject.GetComponent<PaletteHandManager>().UpdateHand(context, parentConstraint, otherPaletteParentConstraint, avatars, "PlayPalette", handToggleState.IsToggled);
    }

    private void UpdateEditPaletteHand()
    {
        myEditPalette.ForceHandToggle(handToggleState.IsToggled);
        //myEditPalette.UpdateDominantHand();
    }

    /// <summary>
    /// Method ForceHandToggle is used to force the Switch Buttons toggle to reflect that of the other palette.
    /// </summary>
    public void ForceHandToggle(bool toggleState)
    {
        handToggleState.ForceSetToggled(toggleState);
    }

    // Update is called once per frame
    void Update()
    {
        if (owner)
        {
            if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation
                || lastHandToggleState != handToggleState.IsToggled)
            {
                lastPosition = transform.localPosition;
                lastScale = transform.localScale;
                lastRotation = transform.localRotation;

                lastHandToggleState = handToggleState.IsToggled;

                BroadcastPlayPaletteInfo();
            }
        }
    }

    private void BroadcastPlayPaletteInfo()
    {
        context.SendJson(new Message()
        {
            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,

            owner = false,
            ownerName = AvatarManager.UUID,

            exitButtonStateVisualizer = buttonStates[0].enabled,
            exitButtonActiveHover = buttonStates[0].Interactable.IsActiveHovered,
            exitButtonHover = buttonStates[0].Interactable.IsRayHovered,

            // Switch hands button
            switchHandsButtonStateVisualizer = buttonStates[1].enabled,
            switchHandsButtonActiveHover = buttonStates[1].Interactable.IsActiveHovered,
            switchHandsButtonHover = buttonStates[1].Interactable.IsRayHovered,

            // Control unique toggle states (those that do not toggle off when another button is toggled)
            handState = handToggleState.IsToggled,
        });
    }

    private struct Message
    {
        public Vector3 position, scale;
        public Quaternion rotation;

        // The user that spawns the palette will always be the owner. This variable will always be set to false
        public bool owner;

        public string ownerName;

        // Control the button animation states
        public bool exitButtonStateVisualizer; public bool exitButtonActiveHover; public bool exitButtonHover;

        // Switch hands button
        public bool switchHandsButtonStateVisualizer; public bool switchHandsButtonActiveHover; public bool switchHandsButtonHover;

        // Control unique toggle states (those that do not toggle off when another button is toggled)
        public bool handState;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Debug.Log("Getting message from scene = " + gameObject.transform.parent.parent.parent.name);

        // Parse the message
        var m = message.FromJson<Message>();

        // Put the m properties in an array to simplify below code
        // Arrays are in the order of { { StateVisualizer, ButtonActiveHover, ButtonHover } }
        bool [,] buttonProperties = new bool[,]
        { 
            { m.exitButtonStateVisualizer, m.exitButtonActiveHover, m.exitButtonHover }, { m.switchHandsButtonStateVisualizer, m.switchHandsButtonActiveHover, m.switchHandsButtonHover },
        };

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localScale = m.scale;
        transform.localRotation = m.rotation;

        owner = m.owner;
        ownerName = m.ownerName;

        // Control the button animation states
        for (int i = 0; i < buttonStates.Length; i++)
        {
            buttonStates[i].enabled = buttonProperties[i, 0];
            buttonStates[i].Interactable.IsActiveHovered.Initialize(buttonProperties[i, 1]);
            buttonStates[i].Interactable.IsRayHovered.Initialize(buttonProperties[i, 2]);

            lastButtonStates[i] = buttonStates[i].Interactable.IsActiveHovered;
        }

        handToggleState.ForceSetToggled(m.handState);

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;
        lastScale = transform.localScale;
        lastRotation = transform.localRotation;

        lastHandToggleState = handToggleState.IsToggled;
    }
}
