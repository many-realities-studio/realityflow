using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Animations;
using Ubiq.Messaging;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Avatars;
using Ubiq.Spawning;
using Slider = Microsoft.MixedReality.Toolkit.UX.Slider;

[System.Serializable]
public class RealityFlowToolEvent : UnityEvent<int, bool>
{
}

[System.Serializable]
public class RealityFlowManipulationEvent : UnityEvent<int>
{
}

[System.Serializable]
public class RealityFlowGizmoEvent : UnityEvent<int>
{
}

/// <summary>
/// Class NetworkedPalette provides a set of menus and is connected to the Ubiq Network library.
/// </summary>
public class NetworkedPalette : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public static NetworkedPalette reference;

    // Used to untick the Parent Constraint component on server-side palettes and access client-side palette
    private ParentConstraint parentConstraint;
    private ParentConstraint otherPaletteParentConstraint;

    public RealityFlowToolEvent m_ToolEvent; 
    ConstraintSource constraintSource;
    NetworkedPlayPalette[] playPalettes;
    NetworkedPlayPalette myPlayPalette;

    public RealityFlowManipulationEvent m_ManipulationEvent;
    public RealityFlowGizmoEvent m_GizmoEvent;

    private NetworkedPalette myPalette;

    [Tooltip("Displays the owner's UUID and should not be manually changed")]
    public string ownerName;

    // The owner of a palette is the user that has spawned it
    public bool owner;

    // Have access to all palette menu states
    [Header("Menu States")]
    // Code should probably be updated to following:
    /*[SerializeField] private GameObject homeMenu;
    [SerializeField] private GameObject meshMenu;
    [SerializeField] private GameObject path3DMenu;
    [SerializeField] private GameObject manipulateMenu;
    [SerializeField] private GameObject transformMenu;
    [SerializeField] private GameObject editMenu;
    [SerializeField] private GameObject colorsMenu;
    [SerializeField] private GameObject shadersMenu;*/
    public GameObject homeMenu;
    public GameObject meshMenu;
    public GameObject path3DMenu;
    public GameObject manipulateMenu;
    public GameObject transformMenu;
    public GameObject editMenu;
    public GameObject colorsMenu;
    public GameObject shadersMenu;

    // Boolean values to determine which menu is currently active by the owner
    private bool lastHomeMenuState;
    private bool lastMeshMenuState;
    private bool lastPath3DMenuState;
    private bool lastManipulateMenuState;
    private bool lastTransformMenuState;
    private bool lastEditMenuState;
    private bool lastColorsMenuState;
    private bool lastShadersMenu;

    // Have access to all button states
    [SerializeField] private StateVisualizer[] buttonStates;
    public StatefulInteractable[] toggleStates;
    [Header("Toggle States that should not toggle off other toggles")]
    [SerializeField] private StatefulInteractable editToggleState;
    [SerializeField] private StatefulInteractable path3DToggleState;
    [SerializeField] private PressableButton combineToolToggleState;
    [SerializeField] private PressableButton handToggleState;
    [SerializeField] private PressableButton xrayToggleState;

    // Button states (which one is currently being hovered by the owner)
    private List<bool> lastButtonStates = new List<bool>();
    private List<bool> lastToggleStates = new List<bool>();
    private bool lastEditToggleState;
    private bool lastPath3DToggleState;
    private bool lastCombineToolToggleState;
    private bool lastHandToggleState;
    private bool lastXrayToggleState;

    [Header("Mesh Mode States")]
    // Have access to snap mode and snap unit states
    [SerializeField] private Slider snapModeState;
    [SerializeField] private Slider snapUnitState;
    [SerializeField] private Slider transformSnapUnitState;
    [SerializeField] private Slider manipulateSnapUnitState;
    [SerializeField] private StatefulInteractable meshGridModeToggleState;
    [SerializeField] private StatefulInteractable transformGridModeToggleState;
    [SerializeField] private StatefulInteractable manipulateGridModeToggleState;

    // Have access to all color menu states
    [Header("Color States")]
    [SerializeField] private ColorPickerControl colorPickerControl;
    [SerializeField] private Slider hSlider;
    [SerializeField] private Slider metallicSlider;
    [SerializeField] private Slider smoothnessSlider;
    [SerializeField] private GameObject svPicker;

    private RectTransform svPickerTransform;
    private Vector3 lastSvPickerTransform;
    private Image svPickerImage;

    // Snap mode and snap unit states (current values that the owner has set)
    private float lastSnapModeState;
    private float lastSnapUnitState;
    private float lastTransformSnapUnitState;
    private float lastManipulateSnapUnitState;
    private bool lastMeshGridModeToggleState;
    private bool lastTransformGridModeToggleState;
    private bool lastManipulateGridModeToggleState;

    // Color menu states (current values that the owner has set)
    private float lastHueSliderState;
    private float lastMetallicSliderState;
    private float lastSmoothnessSliderState;

    public NetworkContext context;
    Vector3 lastPosition, lastScale;
    Quaternion lastRotation;
    private GameObject realityFlowTools;
    
    public void SelectTool(bool status)
    {
        m_ToolEvent.Invoke(0, status);
    }

    public void EraserTool(bool status)
    {
        m_ToolEvent.Invoke(1, status);
    }

    public void ColorTool(bool status)
    {
        m_ToolEvent.Invoke(2, status);
    }

    public void MetallicTool(bool status)
    {
        m_ToolEvent.Invoke(3, status);
    }

    public void SmoothnessTool(bool status)
    {
        m_ToolEvent.Invoke(4, status);
    }

    public void GridTool(bool status)
    {
        m_ToolEvent.Invoke(6, status);
    }

    public void ManipulationTool(bool status)
    {
        m_ToolEvent.Invoke(7, status);
    }

    public void CopyTool(bool status)
    {
        m_ToolEvent.Invoke(8, status);
    }

    public void PlayTool(bool status)
    {
        m_ToolEvent.Invoke(11, status);
    }

    public void XrayTool(bool status)
    {
        m_ToolEvent.Invoke(9, status);
    }
    public void GizmoTool(bool status)
    {
        m_ToolEvent.Invoke(10, status);
    }

    public void SetManipulationTool(int mode)
    {
        m_ManipulationEvent.Invoke(mode);
    }

    public void SetGizmoTool(int mode)
    {
        m_GizmoEvent.Invoke(mode);
    }

    public void Start()
    {
        context = NetworkScene.Register(this);
        
        // By default, all button StateVisualizer's are inactive until this start method is called.
        // This is to prevent a bug where buttons are being visualized in their hover states over
        // the server even though the owner is not hovering over them.
        foreach (StateVisualizer state in buttonStates)
        {
            state.enabled = true;
        }

        // These may not be necessary unless a user joins the room during a owner's interaction of their palette
        // Initialize "last" button states to that of the owner
        for (int i = 0; i < buttonStates.Length; i++)
        {
            lastButtonStates.Add(buttonStates[i].Interactable.IsActiveHovered);
        }

        // Initialize "last" toggle states to that of the owner
        for (int i = 0; i < toggleStates.Length; i++)
        {
            lastToggleStates.Add(toggleStates[i].IsToggled);
        }

        // Initialize picker states
        svPickerTransform = svPicker.GetComponent<RectTransform>();
        svPickerImage = svPicker.GetComponent<Image>();

        // Initialize "last" unique toggle states (those that do not toggle off when another button is toggled)
        lastEditToggleState = editToggleState.IsToggled;
        lastPath3DToggleState = path3DToggleState.IsToggled;
        lastCombineToolToggleState = combineToolToggleState.enabled;
        lastHandToggleState = handToggleState.IsToggled;
        lastXrayToggleState = xrayToggleState.IsToggled;

        // Initialize "last" slider values and toggle state of any grid-related tools
        lastSnapModeState = snapModeState.SliderValue;
        lastSnapUnitState = snapUnitState.SliderValue;
        lastTransformSnapUnitState = transformSnapUnitState.SliderValue;
        lastManipulateSnapUnitState = manipulateSnapUnitState.SliderValue;
        lastMeshGridModeToggleState = meshGridModeToggleState.IsToggled;
        lastTransformGridModeToggleState = transformGridModeToggleState.IsToggled;
        lastManipulateGridModeToggleState = manipulateGridModeToggleState.IsToggled;

        if (owner)
        {
            // Get a reference to this palette for use in scripts that need to check for ownership
            reference = gameObject.GetComponent<NetworkedPalette>();
            
            Ubiq.Avatars.Avatar[] avatars = context.Scene.GetComponentsInChildren<Ubiq.Avatars.Avatar>();
        
            // Go through every avatar looking for the palette owner's avatar.
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].ToString().Contains(AvatarManager.UUID))
                {
                    ownerName = AvatarManager.UUID;
                }
            }

            if(m_ToolEvent == null)
            {
                m_ToolEvent = new RealityFlowToolEvent();
            }
            realityFlowTools = GameObject.Find("RealityFlow Editor");
            SelectTool selectTool = realityFlowTools.GetComponent<SelectTool>();
            m_ToolEvent.AddListener(selectTool.Activate);
            try
            {
                selectTool.AssignName(ownerName);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
            EraserTool eraserTool = realityFlowTools.GetComponent<EraserTool>();
            m_ToolEvent.AddListener(eraserTool.Activate);
            ColorTool colorTool = realityFlowTools.GetComponent<ColorTool>();
            m_ToolEvent.AddListener(colorTool.Activate);
            GridTool gridTool = realityFlowTools.GetComponent<GridTool>();
            m_ToolEvent.AddListener(gridTool.Activate);
            ManipulationTool manipulationTool = realityFlowTools.GetComponent<ManipulationTool>();
            m_ToolEvent.AddListener(manipulationTool.Activate);
            CopyTool copyTool = realityFlowTools.GetComponent<CopyTool>();
            m_ToolEvent.AddListener(copyTool.Activate);
            PlayModeSpawner playTool = realityFlowTools.GetComponentInChildren<PlayModeSpawner>();
            m_ToolEvent.AddListener(playTool.Activate);
            XrayTool xrayTool = realityFlowTools.GetComponent<XrayTool>();
            //m_ToolEvent.AddListener(xrayTool.Activate);
            GizmoTool gizmoTool = realityFlowTools.GetComponent<GizmoTool>();
            //m_ToolEvent.AddListener(gizmoTool.Activate);

            m_ManipulationEvent.AddListener(manipulationTool.SetManipulationMode);
            //m_GizmoEvent.AddListener(gizmoTool.SetGizmoMode);

            /*
            Ubiq.Avatars.Avatar[] avatars = context.Scene.GetComponentsInChildren<Ubiq.Avatars.Avatar>();
        
            // Go through every avatar looking for the palette owner's avatar.
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].ToString().Contains(AvatarManager.UUID))
                {
                    ownerName = AvatarManager.UUID;
                }
            }
            */

            UpdateDominantHand();
        }
        // Users should not be able to interact with others' palettes
        else if (!owner)
        {
            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true);
            Slider[] sliders = gameObject.GetComponentsInChildren<Slider>(true);
            UpdateSnapMode[] snapModes = gameObject.GetComponentsInChildren<UpdateSnapMode>(true);
            UpdateSnapUnits[] snapUnits = gameObject.GetComponentsInChildren<UpdateSnapUnits>(true);
            UpdateGridToggles[] gridToggles = gameObject.GetComponentsInChildren<UpdateGridToggles>(true);

            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            foreach (Slider slider in sliders)
            {
                slider.enabled = false;
            }

            foreach (UpdateSnapMode snapMode in snapModes)
            {
                snapMode.enabled = false;
            }

            foreach (UpdateSnapUnits snapUnit in snapUnits)
            {
                snapUnit.enabled = false;
            }

            foreach (UpdateGridToggles gridToggle in gridToggles)
            {
                gridToggle.enabled = false;
            }

            RequestPaletteData();
        }
    }

    private void Awake()
    {
        if (this.NetworkId == null)
            Debug.Log("Networked Object " + gameObject.name + " Network ID is null");
    }

    /// <summary>
    /// When a user joins the room, they need to know the current state of all palettes in the room.
    /// </summary>
    private void RequestPaletteData()
    {
        // Debug.Log("Attempt to request palette data...");

        // Request palette transform data
        TransformMessage transformMsg = new TransformMessage();
        transformMsg.needsData = true;

        context.SendJson(transformMsg);

        // Request palette info
        InfoMessage infoMsg = new InfoMessage();
        infoMsg.needsData = true;

        context.SendJson(infoMsg);
    }

    public void UpdateDominantHand()
    {
        parentConstraint = gameObject.GetComponent<ParentConstraint>();
        
        Ubiq.Avatars.Avatar[] avatars = context.Scene.GetComponentsInChildren<Ubiq.Avatars.Avatar>();

        if (myPlayPalette != null)
        {
            UpdatePlayPaletteHand();
        }
        else
        {
            // Find this owners play palette
            playPalettes = FindObjectsOfType<NetworkedPlayPalette>();
            foreach (NetworkedPlayPalette playPalette in playPalettes)
            {
                if (playPalette.ownerName == ownerName && playPalette.owner)
                {
                    myPlayPalette = playPalette;

                    if (myPlayPalette != null)
                    {
                        otherPaletteParentConstraint = myPlayPalette.gameObject.GetComponent<ParentConstraint>();
                    }
                    // Debug.Log("Found play palette.name = " + myPlayPalette.name);
                }
            }

            // When UpdateDominantHand() is called in Start then the play palette will be null, afterwards once this method gets called again will it be found
            if (myPlayPalette != null)
            {
                UpdatePlayPaletteHand();
            }
        }

        // Initialize which hand the palette owner has for their dominant (default is right hand)
        gameObject.GetComponent<PaletteHandManager>().UpdateHand(context, parentConstraint, avatars, otherPaletteParentConstraint, "Palette", handToggleState.IsToggled);
    }

    private void UpdatePlayPaletteHand()
    {
        myPlayPalette.ForceHandToggle(handToggleState.IsToggled);
        //myPlayPalette.UpdateDominantHand();
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
            for (int i = 0, j = 0; i < lastButtonStates.Count; i++, j++)
            {
                // Ensure that j not does go out of bounds. This works because there will not be no case where there are more toggle buttons than total buttons.
                if (j >= toggleStates.Length)
                {
                    j = toggleStates.Length - 1;
                }

                // If any action is taken that should be reflected over the server, then fire the message. This futureproofs the palette if it will ever be stationary in the future
                if (lastPosition != transform.localPosition || lastScale != transform.localScale || lastRotation != transform.localRotation)
                {
                    lastPosition = transform.localPosition;
                    lastScale = transform.localScale;
                    lastRotation = transform.localRotation;

                    BroadcastPaletteTransform();
                }

                if (lastHomeMenuState != homeMenu.activeInHierarchy || lastMeshMenuState != meshMenu.activeInHierarchy || lastPath3DMenuState != path3DMenu.activeInHierarchy || lastManipulateMenuState != manipulateMenu.activeInHierarchy
                || lastTransformMenuState != transformMenu.activeInHierarchy || lastEditMenuState != editMenu.activeInHierarchy || lastColorsMenuState != colorsMenu.activeInHierarchy
                || lastShadersMenu != shadersMenu.activeInHierarchy || lastButtonStates[i] != buttonStates[i].Interactable.IsActiveHovered || lastToggleStates[j] != toggleStates[j].IsToggled
                || lastEditToggleState != editToggleState.IsToggled || lastPath3DToggleState != path3DToggleState.IsToggled || lastCombineToolToggleState != combineToolToggleState.enabled
                || lastHandToggleState != handToggleState.IsToggled || lastXrayToggleState != xrayToggleState.IsToggled || lastSnapModeState != snapModeState.SliderValue || lastSnapUnitState != snapUnitState.SliderValue
                || lastTransformSnapUnitState != transformSnapUnitState.SliderValue || lastManipulateSnapUnitState != manipulateSnapUnitState.SliderValue || lastMeshGridModeToggleState != meshGridModeToggleState.IsToggled
                || lastTransformGridModeToggleState != transformGridModeToggleState.IsToggled || lastManipulateGridModeToggleState != manipulateGridModeToggleState.IsToggled
                || lastHueSliderState != hSlider.SliderValue || lastMetallicSliderState != metallicSlider.SliderValue || lastSmoothnessSliderState != smoothnessSlider.SliderValue || lastSvPickerTransform != svPickerTransform.localPosition)
                {
                    // Control the active menu states
                    lastHomeMenuState = homeMenu.activeInHierarchy;
                    lastMeshMenuState = meshMenu.activeInHierarchy;
                    lastPath3DMenuState = path3DMenu.activeInHierarchy;
                    lastManipulateMenuState = manipulateMenu.activeInHierarchy;
                    lastTransformMenuState = transformMenu.activeInHierarchy;
                    lastEditMenuState = editMenu.activeInHierarchy;
                    lastColorsMenuState = colorsMenu.activeInHierarchy;
                    lastShadersMenu = shadersMenu.activeInHierarchy;

                    // Control all button animation states
                    lastButtonStates[i] = buttonStates[i].Interactable.IsActiveHovered;

                    // Control all toggle animation states
                    lastToggleStates[j] = toggleStates[j].IsToggled;

                    // Control unique toggle states (those that do not toggle off when another button is toggled)
                    lastEditToggleState = editToggleState.IsToggled;
                    lastPath3DToggleState = path3DToggleState.IsToggled;
                    lastCombineToolToggleState = combineToolToggleState.enabled;
                    lastHandToggleState = handToggleState.IsToggled;
                    lastXrayToggleState = xrayToggleState.IsToggled;

                    // Control the snap mode and snap unit states
                    lastSnapModeState = snapModeState.SliderValue;
                    lastSnapUnitState = snapUnitState.SliderValue;
                    lastTransformSnapUnitState = transformSnapUnitState.SliderValue;
                    lastManipulateSnapUnitState = manipulateSnapUnitState.SliderValue;
                    lastMeshGridModeToggleState = meshGridModeToggleState.IsToggled;
                    lastTransformGridModeToggleState = transformGridModeToggleState.IsToggled;
                    lastManipulateGridModeToggleState = manipulateGridModeToggleState.IsToggled;

                    // Control the color menu states
                    lastHueSliderState = hSlider.SliderValue;
                    lastMetallicSliderState = metallicSlider.SliderValue;
                    lastSmoothnessSliderState = smoothnessSlider.SliderValue;
                    lastSvPickerTransform = svPickerTransform.localPosition;

                    BroadcastPaletteInfo();
                }
            }
        }

    }

    private void BroadcastPaletteTransform()
    {
        // Debug.Log("Sending network palette info in scene " + gameObject.transform.parent.parent.parent + " from user ID " + ownerName);
        context.SendJson(new TransformMessage()
        {
            type = "transform",

            position = transform.localPosition,
            scale = transform.localScale,
            rotation = transform.localRotation,

            // owner = false,
        });
    }

    private void BroadcastPaletteInfo()
    {
        // Debug.Log("Sending network palette info in scene " + gameObject.transform.parent.parent.parent + " from user ID " + ownerName);
        context.SendJson(new InfoMessage()
        {
            type = "info",

            owner = false,

            ownerName = AvatarManager.UUID,

            // Control the active menu states
            homeMenuState = homeMenu.activeInHierarchy,
            meshMenuState = meshMenu.activeInHierarchy,
            path3DMenuState = path3DMenu.activeInHierarchy,
            manipulateMenuState = manipulateMenu.activeInHierarchy,
            transformMenuState = transformMenu.activeInHierarchy,
            editMenuState = editMenu.activeInHierarchy,
            colorsMenuState = colorsMenu.activeInHierarchy,
            shadersMenuState = shadersMenu.activeInHierarchy,

            // Control the button animation states
            // Mesh related buttons
            meshButtonStateVisualizer = buttonStates[0].enabled,
            meshButtonActiveHover = buttonStates[0].Interactable.IsActiveHovered,
            meshButtonHover = buttonStates[0].Interactable.IsRayHovered,

            cubeButtonStateVisualizer = buttonStates[1].enabled,
            cubeButtonActiveHover = buttonStates[1].Interactable.IsActiveHovered,
            cubeButtonHover = buttonStates[1].Interactable.IsRayHovered,

            sphereButtonStateVisualizer = buttonStates[2].enabled,
            sphereButtonActiveHover = buttonStates[2].Interactable.IsActiveHovered,
            sphereButtonHover = buttonStates[2].Interactable.IsRayHovered,

            cylinderButtonStateVisualizer = buttonStates[3].enabled,
            cylinderButtonActiveHover = buttonStates[3].Interactable.IsActiveHovered,
            cylinderButtonHover = buttonStates[3].Interactable.IsRayHovered,

            coneButtonStateVisualizer = buttonStates[4].enabled,
            coneButtonActiveHover = buttonStates[4].Interactable.IsActiveHovered,
            coneButtonHover = buttonStates[4].Interactable.IsRayHovered,

            torusButtonStateVisualizer = buttonStates[34].enabled,
            torusButtonActiveHover = buttonStates[34].Interactable.IsActiveHovered,
            torusButtonHover = buttonStates[34].Interactable.IsRayHovered,

            planeButtonStateVisualizer = buttonStates[6].enabled,
            planeButtonActiveHover = buttonStates[6].Interactable.IsActiveHovered,
            planeButtonHover = buttonStates[6].Interactable.IsRayHovered,

            path3DButtonStateVisualizer = buttonStates[5].enabled,
            path3DButtonActiveHover = buttonStates[5].Interactable.IsActiveHovered,
            path3DButtonHover = buttonStates[5].Interactable.IsRayHovered,

            polygon2DButtonStateVisualizer = buttonStates[35].enabled,
            polygon2DButtonActiveHover = buttonStates[35].Interactable.IsActiveHovered,
            polygon2DButtonHover = buttonStates[35].Interactable.IsRayHovered,

            cubeTubeButtonStateVisualizer = buttonStates[7].enabled,
            cubeTubeButtonActiveHover = buttonStates[7].Interactable.IsActiveHovered,
            cubeTubeButtonHover = buttonStates[7].Interactable.IsRayHovered,

            sphereTubeButtonStateVisualizer = buttonStates[8].enabled,
            sphereTubeButtonActiveHover = buttonStates[8].Interactable.IsActiveHovered,
            sphereTubeButtonHover = buttonStates[8].Interactable.IsRayHovered,

            cylinderTubeButtonStateVisualizer = buttonStates[9].enabled,
            cylinderTubeButtonActiveHover = buttonStates[9].Interactable.IsActiveHovered,
            cylinderTubeButtonHover = buttonStates[9].Interactable.IsRayHovered,

            meshBackButtonStateVisualizer = buttonStates[10].enabled,
            meshBackButtonActiveHover = buttonStates[10].Interactable.IsActiveHovered,
            meshBackButtonHover = buttonStates[10].Interactable.IsRayHovered,

            wedgeButtonStateVisualizer = buttonStates[45].enabled,
            wedgeButtonActiveHover = buttonStates[45].Interactable.IsActiveHovered,
            wedgeButtonHover = buttonStates[45].Interactable.IsRayHovered,

            pipeButtonStateVisualizer = buttonStates[46].enabled,
            pipeButtonActiveHover = buttonStates[46].Interactable.IsActiveHovered,
            pipeButtonHover = buttonStates[46].Interactable.IsRayHovered,
            // End of mesh related buttons

            teleportButtonStateVisualizer = buttonStates[11].enabled,
            teleportButtonActiveHover = buttonStates[11].Interactable.IsActiveHovered,
            teleportButtonHover = buttonStates[11].Interactable.IsRayHovered,

            selectButtonStateVisualizer = buttonStates[12].enabled,
            selectButtonActiveHover = buttonStates[12].Interactable.IsActiveHovered,
            selectButtonHover = buttonStates[12].Interactable.IsRayHovered,

            // Manipulate related buttons
            manipulateButtonStateVisualizer = buttonStates[13].enabled,
            manipulateButtonActiveHover = buttonStates[13].Interactable.IsActiveHovered,
            manipulateButtonHover = buttonStates[13].Interactable.IsRayHovered,

            manipulateFaceButtonStateVisualizer = buttonStates[14].enabled,
            manipulateFaceButtonActiveHover = buttonStates[14].Interactable.IsActiveHovered,
            manipulateFaceButtonHover = buttonStates[14].Interactable.IsRayHovered,

            manipulateEdgeButtonStateVisualizer = buttonStates[15].enabled,
            manipulateEdgeButtonActiveHover = buttonStates[15].Interactable.IsActiveHovered,
            manipulateEdgeButtonHover = buttonStates[15].Interactable.IsRayHovered,

            manipulateVertexButtonStateVisualizer = buttonStates[16].enabled,
            manipulateVertexButtonActiveHover = buttonStates[16].Interactable.IsActiveHovered,
            manipulateVertexButtonHover = buttonStates[16].Interactable.IsRayHovered,

            xrayButtonStateVisualizer = buttonStates[17].enabled,
            xrayButtonActiveHover = buttonStates[17].Interactable.IsActiveHovered,
            xrayButtonHover = buttonStates[17].Interactable.IsRayHovered,

            tessellationButtonStateVisualizer = buttonStates[32].enabled,
            tessellationButtonActiveHover = buttonStates[32].Interactable.IsActiveHovered,
            tessellationButtonHover = buttonStates[32].Interactable.IsRayHovered,

            extrudeButtonStateVisualizer = buttonStates[36].enabled,
            extrudeButtonActiveHover = buttonStates[36].Interactable.IsActiveHovered,
            extrudeButtonHover = buttonStates[36].Interactable.IsRayHovered,

            manipulateBackButtonStateVisualizer = buttonStates[18].enabled,
            manipulateBackButtonActiveHover = buttonStates[18].Interactable.IsActiveHovered,
            manipulateBackButtonHover = buttonStates[18].Interactable.IsRayHovered,
            // End of manipulate related buttons

            // Transform related buttons
            transformButtonStateVisualizer = buttonStates[19].enabled,
            transformButtonActiveHover = buttonStates[19].Interactable.IsActiveHovered,
            transformButtonHover = buttonStates[19].Interactable.IsRayHovered,

            translateButtonStateVisualizer = buttonStates[20].enabled,
            translateButtonActiveHover = buttonStates[20].Interactable.IsActiveHovered,
            translateButtonHover = buttonStates[20].Interactable.IsRayHovered,

            rotateButtonStateVisualizer = buttonStates[21].enabled,
            rotateButtonActiveHover = buttonStates[21].Interactable.IsActiveHovered,
            rotateButtonHover = buttonStates[21].Interactable.IsRayHovered,

            scaleButtonStateVisualizer = buttonStates[22].enabled,
            scaleButtonActiveHover = buttonStates[22].Interactable.IsActiveHovered,
            scaleButtonHover = buttonStates[22].Interactable.IsRayHovered,

            transformAllButtonStateVisualizer = buttonStates[23].enabled,
            transformAllButtonActiveHover = buttonStates[23].Interactable.IsActiveHovered,
            transformAllButtonHover = buttonStates[23].Interactable.IsRayHovered,

            transformBackButtonStateVisualizer = buttonStates[24].enabled,
            transformBackButtonActiveHover = buttonStates[24].Interactable.IsActiveHovered,
            transformBackButtonHover = buttonStates[24].Interactable.IsRayHovered,
            // End of transform related buttons

            eraserButtonStateVisualizer = buttonStates[25].enabled,
            eraserButtonActiveHover = buttonStates[25].Interactable.IsActiveHovered,
            eraserButtonHover = buttonStates[25].Interactable.IsRayHovered,

            copyButtonStateVisualizer = buttonStates[43].enabled,
            copyButtonActiveHover = buttonStates[43].Interactable.IsActiveHovered,
            copyButtonHover = buttonStates[43].Interactable.IsRayHovered,

            undoButtonStateVisualizer = buttonStates[26].enabled,
            undoButtonActiveHover = buttonStates[26].Interactable.IsActiveHovered,
            undoButtonHover = buttonStates[26].Interactable.IsRayHovered,

            redoButtonStateVisualizer = buttonStates[27].enabled,
            redoButtonActiveHover = buttonStates[27].Interactable.IsActiveHovered,
            redoButtonHover = buttonStates[27].Interactable.IsRayHovered,

            combineButtonStateVisualizer = buttonStates[28].enabled,
            combineButtonActiveHover = buttonStates[28].Interactable.IsActiveHovered,
            combineButtonHover = buttonStates[28].Interactable.IsRayHovered,

            playButtonStateVisualizer = buttonStates[44].enabled,
            playButtonActiveHover = buttonStates[44].Interactable.IsActiveHovered,
            playButtonHover = buttonStates[44].Interactable.IsRayHovered,
            
            leaveButtonStateVisualizer = buttonStates[47].enabled,
            leaveButtonActiveHover = buttonStates[47].Interactable.IsActiveHovered,
            leaveButtonHover = buttonStates[47].Interactable.IsRayHovered,

            EnableNearMenuToolboxButtonStateVisualizer = buttonStates[47].enabled,
            EnableNearMenuToolboxButtonActiveHover = buttonStates[47].Interactable.IsActiveHovered,
            EnableNearMenuToolboxButtonHover = buttonStates[47].Interactable.IsRayHovered,

            // Edit related buttons
            editButtonStateVisualizer = buttonStates[29].enabled,
            editButtonActiveHover = buttonStates[29].Interactable.IsActiveHovered,
            editButtonHover = buttonStates[29].Interactable.IsRayHovered,

            editConfirmButtonStateVisualizer = buttonStates[30].enabled,
            editConfirmButtonActiveHover = buttonStates[30].Interactable.IsActiveHovered,
            editConfirmButtonHover = buttonStates[30].Interactable.IsRayHovered,

            editCancelButtonStateVisualizer = buttonStates[31].enabled,
            editCancelButtonActiveHover = buttonStates[31].Interactable.IsActiveHovered,
            editCancelButtonHover = buttonStates[31].Interactable.IsRayHovered,
            // End of edit related buttons

            // Color related buttons
            colorsButtonStateVisualizer = buttonStates[37].enabled,
            colorsButtonActiveHover = buttonStates[37].Interactable.IsActiveHovered,
            colorsButtonHover = buttonStates[37].Interactable.IsRayHovered,

            colorsBackButtonStateVisualizer = buttonStates[38].enabled,
            colorsBackButtonActiveHover = buttonStates[38].Interactable.IsActiveHovered,
            colorsBackButtonHover = buttonStates[38].Interactable.IsRayHovered,

            moreOptionsButtonStateVisualizer = buttonStates[39].enabled,
            moreOptionsButtonActiveHover = buttonStates[39].Interactable.IsActiveHovered,
            moreOptionsButtonHover = buttonStates[39].Interactable.IsRayHovered,

            metallicButtonStateVisualizer = buttonStates[40].enabled,
            metallicButtonActiveHover = buttonStates[40].Interactable.IsActiveHovered,
            metallicButtonHover = buttonStates[40].Interactable.IsRayHovered,

            smoothnessButtonStateVisualizer = buttonStates[41].enabled,
            smoothnessButtonActiveHover = buttonStates[41].Interactable.IsActiveHovered,
            smoothnessButtonHover = buttonStates[41].Interactable.IsRayHovered,

            colorButtonStateVisualizer = buttonStates[42].enabled,
            colorButtonActiveHover = buttonStates[42].Interactable.IsActiveHovered,
            colorButtonHover = buttonStates[42].Interactable.IsRayHovered,

            satValue = colorPickerControl.currentSat,
            valValue = colorPickerControl.currentVal,
            hueSliderValue = hSlider.SliderValue,
            metallicSliderValue = metallicSlider.SliderValue,
            smoothnessSliderValue = smoothnessSlider.SliderValue,
            satValPicker = svPickerTransform,
            //satValPickerTransform = svPickerTransform.localPosition,
            svPickerColor = svPickerImage.color,
            // End of color related buttons

            // Switch hands button
            switchHandsButtonStateVisualizer = buttonStates[33].enabled,
            switchHandsButtonActiveHover = buttonStates[33].Interactable.IsActiveHovered,
            switchHandsButtonHover = buttonStates[33].Interactable.IsRayHovered,

            // Control all toggle states
            teleportToolState = toggleStates[0].IsToggled,
            selectToolState = toggleStates[1].IsToggled,
            eraserToolState = toggleStates[2].IsToggled,
            cubeState = toggleStates[3].IsToggled,
            sphereState = toggleStates[4].IsToggled,
            cylinderState = toggleStates[5].IsToggled,
            coneState = toggleStates[6].IsToggled,
            planeState = toggleStates[7].IsToggled,
            cubeTubeState = toggleStates[8].IsToggled,
            sphereTubeState = toggleStates[9].IsToggled,
            coneTubeState = toggleStates[10].IsToggled,
            faceSelectionState = toggleStates[11].IsToggled,
            edgeSelectionState = toggleStates[12].IsToggled,
            vertexSelectionState = toggleStates[13].IsToggled,
            // manipulateAllState = toggleStates[14].IsToggled,
            tessellationState = toggleStates[14].IsToggled,
            translateToolState = toggleStates[15].IsToggled,
            rotateToolState = toggleStates[16].IsToggled,
            scaleToolState = toggleStates[17].IsToggled,
            transformAllState = toggleStates[18].IsToggled,
            torusState = toggleStates[19].IsToggled,
            polygon2DState = toggleStates[20].IsToggled,
            extrudeState = toggleStates[21].IsToggled,
            moreOptionsState = toggleStates[22].IsToggled,
            metallicState = toggleStates[23].IsToggled,
            smoothnessState = toggleStates[24].IsToggled,
            colorState = toggleStates[25].IsToggled,
            copyState = toggleStates[26].IsToggled,
            wedgeState = toggleStates[27].IsToggled,
            pipeState = toggleStates[28].IsToggled,

            // Control unique toggle states (those that do not toggle off when another button is toggled)
            editState = editToggleState.IsToggled,
            path3DState = path3DToggleState.IsToggled,
            combineToolState = combineToolToggleState.enabled,
            handState = handToggleState.IsToggled,
            xrayState = xrayToggleState.IsToggled,

            // Control the snap mode and snap unit states
            snapModeValue = snapModeState.SliderValue,
            snapUnitValue = snapUnitState.SliderValue,
            transformSnapUnitValue = transformSnapUnitState.SliderValue,
            manipulateSnapUnitValue = manipulateSnapUnitState.SliderValue,
            meshGridModeState = meshGridModeToggleState.IsToggled,
            transformGridModeState = transformGridModeToggleState.IsToggled,
            manipulateGridModeState = manipulateGridModeToggleState.IsToggled
        });
    }

    private struct TransformMessage
    { 
        public string type;
        public bool needsData;
        public Vector3 position, scale;
        public Quaternion rotation;
    }

    private struct InfoMessage
    { 
        public string type;
        public bool needsData;

        // The user that spawns the palette will always be the owner. This variable will always be set to false
        public bool owner;

        public string ownerName;

        // Control the active menu states
        public bool homeMenuState;
        public bool meshMenuState;
        public bool path3DMenuState;
        public bool manipulateMenuState;
        public bool transformMenuState;
        public bool editMenuState;
        public bool colorsMenuState;
        public bool shadersMenuState;

        // Control the button animation states
        // Mesh related buttons
        public bool meshButtonStateVisualizer; public bool meshButtonActiveHover; public bool meshButtonHover;
        public bool cubeButtonStateVisualizer; public bool cubeButtonActiveHover; public bool cubeButtonHover; 
        public bool sphereButtonStateVisualizer; public bool sphereButtonActiveHover; public bool sphereButtonHover;
        public bool cylinderButtonStateVisualizer; public bool cylinderButtonActiveHover; public bool cylinderButtonHover;
        public bool coneButtonStateVisualizer; public bool coneButtonActiveHover; public bool coneButtonHover;
        public bool torusButtonStateVisualizer; public bool torusButtonActiveHover; public bool torusButtonHover;
        public bool wedgeButtonStateVisualizer; public bool wedgeButtonActiveHover; public bool wedgeButtonHover;
        public bool pipeButtonStateVisualizer; public bool pipeButtonActiveHover; public bool pipeButtonHover;
        public bool planeButtonStateVisualizer; public bool planeButtonActiveHover; public bool planeButtonHover;
        public bool path3DButtonStateVisualizer; public bool path3DButtonActiveHover; public bool path3DButtonHover;
        public bool polygon2DButtonStateVisualizer; public bool polygon2DButtonActiveHover; public bool polygon2DButtonHover;
        public bool cubeTubeButtonStateVisualizer; public bool cubeTubeButtonActiveHover; public bool cubeTubeButtonHover; 
        public bool sphereTubeButtonStateVisualizer; public bool sphereTubeButtonActiveHover; public bool sphereTubeButtonHover;
        public bool cylinderTubeButtonStateVisualizer; public bool cylinderTubeButtonActiveHover; public bool cylinderTubeButtonHover;
        public bool copyButtonStateVisualizer; public bool copyButtonActiveHover; public bool copyButtonHover;
        public bool meshBackButtonStateVisualizer; public bool meshBackButtonActiveHover; public bool meshBackButtonHover;

        // Non-parent related buttons (those that do not open up a menu upon click)
        public bool teleportButtonStateVisualizer; public bool teleportButtonActiveHover; public bool teleportButtonHover;
        public bool selectButtonStateVisualizer; public bool selectButtonActiveHover; public bool selectButtonHover;
        public bool eraserButtonStateVisualizer; public bool eraserButtonActiveHover; public bool eraserButtonHover;
        public bool undoButtonStateVisualizer; public bool undoButtonActiveHover; public bool undoButtonHover;
        public bool redoButtonStateVisualizer; public bool redoButtonActiveHover; public bool redoButtonHover;
        public bool combineButtonStateVisualizer; public bool combineButtonActiveHover; public bool combineButtonHover;
        public bool playButtonStateVisualizer; public bool playButtonActiveHover; public bool playButtonHover;
        public bool leaveButtonStateVisualizer; public bool leaveButtonActiveHover; public bool leaveButtonHover;
        public bool EnableNearMenuToolboxButtonStateVisualizer; public bool EnableNearMenuToolboxButtonActiveHover; public bool EnableNearMenuToolboxButtonHover;

        // Manipulate related buttons
        public bool manipulateButtonStateVisualizer; public bool manipulateButtonActiveHover; public bool manipulateButtonHover;
        public bool manipulateFaceButtonStateVisualizer; public bool manipulateFaceButtonActiveHover; public bool manipulateFaceButtonHover;
        public bool manipulateEdgeButtonStateVisualizer; public bool manipulateEdgeButtonActiveHover; public bool manipulateEdgeButtonHover;
        public bool manipulateVertexButtonStateVisualizer; public bool manipulateVertexButtonActiveHover; public bool manipulateVertexButtonHover;
        public bool xrayButtonStateVisualizer; public bool xrayButtonActiveHover; public bool xrayButtonHover;
        public bool tessellationButtonStateVisualizer; public bool tessellationButtonActiveHover; public bool tessellationButtonHover;
        public bool manipulateBackButtonStateVisualizer; public bool manipulateBackButtonActiveHover; public bool manipulateBackButtonHover;
        public bool extrudeButtonStateVisualizer; public bool extrudeButtonActiveHover; public bool extrudeButtonHover;

        // Transform related buttons
        public bool transformButtonStateVisualizer; public bool transformButtonActiveHover; public bool transformButtonHover;
        public bool translateButtonStateVisualizer; public bool translateButtonActiveHover; public bool translateButtonHover;
        public bool rotateButtonStateVisualizer; public bool rotateButtonActiveHover; public bool rotateButtonHover;
        public bool scaleButtonStateVisualizer; public bool scaleButtonActiveHover; public bool scaleButtonHover;
        public bool transformAllButtonStateVisualizer; public bool transformAllButtonActiveHover; public bool transformAllButtonHover;
        public bool transformBackButtonStateVisualizer; public bool transformBackButtonActiveHover; public bool transformBackButtonHover;

        // Edit related buttons
        public bool editButtonStateVisualizer; public bool editButtonActiveHover; public bool editButtonHover;
        public bool editConfirmButtonStateVisualizer; public bool editConfirmButtonActiveHover; public bool editConfirmButtonHover;
        public bool editCancelButtonStateVisualizer; public bool editCancelButtonActiveHover; public bool editCancelButtonHover;
        
        // Color related buttons
        public bool colorsButtonStateVisualizer; public bool colorsButtonActiveHover; public bool colorsButtonHover;
        public bool colorsBackButtonStateVisualizer; public bool colorsBackButtonActiveHover; public bool colorsBackButtonHover;
        public bool moreOptionsButtonStateVisualizer; public bool moreOptionsButtonActiveHover; public bool moreOptionsButtonHover;
        public bool metallicButtonStateVisualizer; public bool metallicButtonActiveHover; public bool metallicButtonHover;
        public bool smoothnessButtonStateVisualizer; public bool smoothnessButtonActiveHover; public bool smoothnessButtonHover;
        public bool colorButtonStateVisualizer; public bool colorButtonActiveHover; public bool colorButtonHover;
        public float satValue;
        public float valValue;
        public float hueSliderValue;
        public float metallicSliderValue;
        public float smoothnessSliderValue;
        public RectTransform satValPicker;
        // public Vector3 satValPicker;
        public Color svPickerColor;

        // Switch hands button
        public bool switchHandsButtonStateVisualizer; public bool switchHandsButtonActiveHover; public bool switchHandsButtonHover;

        // Control all toggle states
        public bool teleportToolState;
        public bool selectToolState;
        public bool eraserToolState;
        public bool cubeState;
        public bool sphereState;
        public bool cylinderState;
        public bool coneState;
        public bool planeState;
        public bool wedgeState;
        public bool pipeState;
        public bool cubeTubeState;
        public bool sphereTubeState;
        public bool coneTubeState;
        public bool faceSelectionState;
        public bool edgeSelectionState;
        public bool vertexSelectionState;
        //public bool manipulateAllState;
        public bool tessellationState;
        public bool translateToolState;
        public bool rotateToolState;
        public bool scaleToolState;
        public bool transformAllState;
        public bool torusState;
        public bool polygon2DState;
        public bool extrudeState;
        public bool moreOptionsState;
        public bool metallicState;
        public bool smoothnessState;
        public bool colorState;
        public bool copyState;

        // Control unique toggle states (those that do not toggle off when another button is toggled)
        public bool editState;
        public bool path3DState;
        public bool combineToolState;
        public bool handState;
        public bool xrayState;

        // Control the snap mode and snap unit states
        public float snapModeValue;
        public float snapUnitValue;
        public float transformSnapUnitValue;
        public float manipulateSnapUnitValue;
        public bool meshGridModeState;
        public bool transformGridModeState;
        public bool manipulateGridModeState;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {  
        // Debug.Log("Getting message from scene = " + gameObject.transform.parent.parent.parent.name + " from avatar " + ownerName);
        // Parse the message
        var tm = message.FromJson<TransformMessage>();

        if (tm.type == "transform")
        {
            // Debug.Log("Transform message received");
            transform.localPosition = tm.position;
            transform.localScale = tm.scale;
            transform.localRotation = tm.rotation;
            return;
        }

        var m = message.FromJson<InfoMessage>();
        if (m.type == "info")
        {
            // Debug.Log("Info message received");
            // Put the m properties in an array to simplify below code
            // Arrays are in the order of { { StateVisualizer, ButtonActiveHover, ButtonHover, ButtonTouch } }
            bool [,] buttonProperties = new bool[,]
            { 
                { m.meshButtonStateVisualizer, m.meshButtonActiveHover, m.meshButtonHover }, { m.cubeButtonStateVisualizer, m.cubeButtonActiveHover, m.cubeButtonHover },
                { m.sphereButtonStateVisualizer, m.sphereButtonActiveHover, m.sphereButtonHover }, { m.cylinderButtonStateVisualizer, m.cylinderButtonActiveHover, m.cylinderButtonHover },
                { m.coneButtonStateVisualizer, m.coneButtonActiveHover, m.coneButtonHover }, { m.path3DButtonStateVisualizer, m.path3DButtonActiveHover, m.path3DButtonHover },
                { m.planeButtonStateVisualizer, m.planeButtonActiveHover, m.planeButtonHover }, { m.cubeTubeButtonStateVisualizer, m.cubeTubeButtonActiveHover, m.cubeTubeButtonHover },
                { m.sphereTubeButtonStateVisualizer, m.sphereTubeButtonActiveHover, m.sphereTubeButtonHover }, { m.cylinderTubeButtonStateVisualizer, m.cylinderTubeButtonActiveHover, m.cylinderTubeButtonHover },
                { m.meshBackButtonStateVisualizer, m.meshBackButtonActiveHover, m.meshBackButtonHover }, { m.teleportButtonStateVisualizer, m.teleportButtonActiveHover, m.teleportButtonHover },
                { m.selectButtonStateVisualizer, m.selectButtonActiveHover, m.selectButtonHover }, { m.manipulateButtonStateVisualizer, m.manipulateButtonActiveHover, m.manipulateButtonHover },
                { m.manipulateFaceButtonStateVisualizer, m.manipulateFaceButtonActiveHover, m.manipulateFaceButtonHover },
                { m.manipulateEdgeButtonStateVisualizer, m.manipulateEdgeButtonActiveHover, m.manipulateEdgeButtonHover },
                { m.manipulateVertexButtonStateVisualizer, m.manipulateVertexButtonActiveHover, m.manipulateVertexButtonHover },
                // { m.manipulateAllButtonStateVisualizer, m.manipulateAllButtonActiveHover, m.manipulateAllButtonHover },
                { m.xrayButtonStateVisualizer, m.xrayButtonActiveHover, m.xrayButtonHover },
                { m.manipulateBackButtonStateVisualizer, m.manipulateBackButtonActiveHover, m.manipulateBackButtonHover },
                { m.transformButtonStateVisualizer, m.transformButtonActiveHover, m.transformButtonHover }, { m.translateButtonStateVisualizer, m.translateButtonActiveHover, m.translateButtonHover },
                { m.rotateButtonStateVisualizer, m.rotateButtonActiveHover, m.rotateButtonHover }, { m.scaleButtonStateVisualizer, m.scaleButtonActiveHover, m.scaleButtonHover },
                { m.transformAllButtonStateVisualizer, m.transformAllButtonActiveHover, m.transformAllButtonHover }, { m.transformBackButtonStateVisualizer, m.transformBackButtonActiveHover, m.transformBackButtonHover },
                { m.eraserButtonStateVisualizer, m.eraserButtonActiveHover, m.eraserButtonHover }, { m.undoButtonStateVisualizer, m.undoButtonActiveHover, m.undoButtonHover },
                { m.redoButtonStateVisualizer, m.redoButtonActiveHover, m.redoButtonHover }, { m.combineButtonStateVisualizer, m.combineButtonActiveHover, m.combineButtonHover },
                { m.editButtonStateVisualizer, m.editButtonActiveHover, m.editButtonHover }, { m.editConfirmButtonStateVisualizer, m.editConfirmButtonActiveHover, m.editConfirmButtonHover },
                { m.editCancelButtonStateVisualizer, m.editCancelButtonActiveHover, m.editCancelButtonHover }, { m.tessellationButtonStateVisualizer, m.tessellationButtonActiveHover, m.tessellationButtonHover },
                { m.switchHandsButtonStateVisualizer, m.switchHandsButtonActiveHover, m.switchHandsButtonHover }, { m.torusButtonStateVisualizer, m.torusButtonActiveHover, m.torusButtonHover },
                { m.polygon2DButtonStateVisualizer, m.polygon2DButtonActiveHover, m.polygon2DButtonHover }, { m.extrudeButtonStateVisualizer, m.extrudeButtonActiveHover, m.extrudeButtonHover },
                { m.colorsButtonStateVisualizer, m.colorsButtonActiveHover, m.colorsButtonHover }, { m.colorsBackButtonStateVisualizer, m.colorsBackButtonActiveHover, m.colorsBackButtonHover },
                { m.moreOptionsButtonStateVisualizer, m.moreOptionsButtonActiveHover, m.moreOptionsButtonHover }, { m.metallicButtonStateVisualizer, m.metallicButtonActiveHover, m.metallicButtonHover },
                { m.smoothnessButtonStateVisualizer, m.smoothnessButtonActiveHover, m.smoothnessButtonHover }, { m.colorButtonStateVisualizer, m.colorButtonActiveHover, m.colorButtonHover },
                { m.copyButtonStateVisualizer, m.copyButtonActiveHover, m.copyButtonHover }, { m.playButtonStateVisualizer, m.playButtonActiveHover, m.playButtonHover }, { m.leaveButtonStateVisualizer, m.leaveButtonActiveHover, m.leaveButtonHover },
                {m.wedgeButtonStateVisualizer, m.wedgeButtonActiveHover, m.wedgeButtonHover }, {m.pipeButtonStateVisualizer, m.pipeButtonActiveHover, m.pipeButtonHover}, {m.EnableNearMenuToolboxButtonStateVisualizer, m.EnableNearMenuToolboxButtonActiveHover, m.EnableNearMenuToolboxButtonHover}
            };

            bool [] toggleProperties = new bool[]
            {
                m.teleportToolState, m.selectToolState, m.eraserToolState, m.cubeState, m.sphereState, m.cylinderState, m.coneState, m.planeState, m.cubeTubeState, m.sphereTubeState, m.coneTubeState,
                m.faceSelectionState, m.edgeSelectionState, m.vertexSelectionState, /*m.manipulateAllState,*/ m.tessellationState, m.translateToolState, m.rotateToolState,
                m.scaleToolState, m.transformAllState, m.torusState, m.polygon2DState, m.extrudeState, m.moreOptionsState, m.metallicState, m.smoothnessState, m.colorState, m.copyState,
                m.wedgeState, m.pipeState
            };

            // Use the message to update the Component
            owner = m.owner;
            ownerName = m.ownerName;

            // Control the active menu states
            homeMenu.SetActive(m.homeMenuState);
            meshMenu.SetActive(m.meshMenuState);
            path3DMenu.SetActive(m.path3DMenuState);
            manipulateMenu.SetActive(m.manipulateMenuState);
            transformMenu.SetActive(m.transformMenuState);
            editMenu.SetActive(m.editMenuState);
            colorsMenu.SetActive(m.colorsMenuState);
            shadersMenu.SetActive(m.shadersMenuState);

            // Control the button animation states
            for (int i = 0; i < buttonStates.Length; i++)
            {
                buttonStates[i].enabled = buttonProperties[i, 0];
                buttonStates[i].Interactable.IsActiveHovered.Initialize(buttonProperties[i, 1]);
                buttonStates[i].Interactable.IsRayHovered.Initialize(buttonProperties[i, 2]);

                lastButtonStates[i] = buttonStates[i].Interactable.IsActiveHovered;
            }

            // Control all toggle states
            for (int i = 0; i < toggleStates.Length; i++)
            { 
                toggleStates[i].ForceSetToggled(toggleProperties[i]);

                lastToggleStates[i] = toggleStates[i].IsToggled;
            }

            // Control unique toggle states (those that do not toggle off when another button is toggled)
            editToggleState.IsToggled.Initialize(m.editState);
            path3DToggleState.IsToggled.Initialize(m.path3DState);
            combineToolToggleState.enabled = m.combineToolState;
            handToggleState.ForceSetToggled(m.handState);
            xrayToggleState.ForceSetToggled(m.xrayState);

            // Control the snap mode and snap unit states
            snapModeState.SliderValue = m.snapModeValue;
            snapUnitState.SliderValue = m.snapUnitValue;
            transformSnapUnitState.SliderValue = m.transformSnapUnitValue;
            manipulateSnapUnitState.SliderValue = m.manipulateSnapUnitValue;
            meshGridModeToggleState.ForceSetToggled(m.meshGridModeState);
            transformGridModeToggleState.ForceSetToggled(m.transformGridModeState);
            manipulateGridModeToggleState.ForceSetToggled(m.manipulateGridModeState);

            // Control the color states
            colorPickerControl.currentSat = m.satValue;
            colorPickerControl.currentVal = m.valValue;
            hSlider.SliderValue = m.hueSliderValue;
            metallicSlider.SliderValue = m.metallicSliderValue;
            smoothnessSlider.SliderValue = m.smoothnessSliderValue;
            if (svPickerTransform != null && m.satValPicker != null)
            {
                svPickerTransform.anchoredPosition3D = m.satValPicker.anchoredPosition3D;
            }
            //svPickerTransform.localPosition = m.satValPickerTransform;
            
            svPickerImage.color = m.svPickerColor;

            // Make sure the logic in Update doesn't trigger as a result of this message
            lastPosition = transform.localPosition;
            lastScale = transform.localScale;
            lastRotation = transform.localRotation;

            lastHomeMenuState = homeMenu.activeInHierarchy;
            lastMeshMenuState = meshMenu.activeInHierarchy;
            lastPath3DMenuState = path3DMenu.activeInHierarchy;
            lastManipulateMenuState = manipulateMenu.activeInHierarchy;
            lastTransformMenuState = transformMenu.activeInHierarchy;
            lastEditMenuState = editMenu.activeInHierarchy;
            lastColorsMenuState = colorsMenu.activeInHierarchy;
            lastShadersMenu = shadersMenu.activeInHierarchy;

            // Control unique toggle states (those that do not toggle off when another button is toggled)
            lastEditToggleState = editToggleState.IsToggled;
            lastPath3DToggleState = path3DToggleState.IsToggled;
            lastCombineToolToggleState = combineToolToggleState.enabled;
            lastHandToggleState = handToggleState.IsToggled;
            lastXrayToggleState = xrayToggleState.IsToggled;

            // Control the snap mode and snap unit states
            lastSnapModeState = snapModeState.SliderValue;
            lastSnapUnitState = snapUnitState.SliderValue;
            lastTransformSnapUnitState = transformSnapUnitState.SliderValue;
            lastManipulateSnapUnitState = manipulateSnapUnitState.SliderValue;
            lastMeshGridModeToggleState = meshGridModeToggleState.IsToggled;

            // Control the color menu states
            lastHueSliderState = hSlider.SliderValue;
            lastMetallicSliderState = metallicSlider.SliderValue;
            lastSmoothnessSliderState = smoothnessSlider.SliderValue;
            lastSvPickerTransform = svPickerTransform.localPosition;
        }

        if (tm.needsData == true && owner)
        {
            BroadcastPaletteTransform();
        }

        if (m.needsData == true && owner)
        {
            BroadcastPaletteInfo();
        }
    }
}