//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/RealityFlow/RealityFlow Actions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @RealityFlowActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @RealityFlowActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""RealityFlow Actions"",
    ""maps"": [
        {
            ""name"": ""RealityFlowXRActions"",
            ""id"": ""1d0e97c5-fcec-4033-9a16-aa55ea99f9c1"",
            ""actions"": [
                {
                    ""name"": ""ToggleRecording"",
                    ""type"": ""Button"",
                    ""id"": ""5456f297-fbe0-45bf-aebf-67bd27bf636c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""OpenLLMMenu"",
                    ""type"": ""Button"",
                    ""id"": ""3db29b92-65c3-41ce-9f49-779fd3fc7c4e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Execute"",
                    ""type"": ""Button"",
                    ""id"": ""273a882f-2067-4c24-a4d1-c145e03ac09f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""820f9921-3903-457a-89aa-e02739e701ab"",
                    ""path"": ""<XRController>{LeftHand}/secondaryButton"",
                    ""interactions"": ""Hold(duration=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleRecording"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ea831c0e-fe08-4173-a9c9-3ae88ecb5744"",
                    ""path"": ""<Keyboard>/#(J)"",
                    ""interactions"": ""Hold"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleRecording"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bc8cfafc-186d-4c3b-b35c-8f6395f10455"",
                    ""path"": ""<XRController>{LeftHand}/secondaryButton"",
                    ""interactions"": ""Press"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OpenLLMMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""fc725d5f-60ab-4afc-b725-a66083e154d5"",
                    ""path"": ""<XRController>{LeftHand}/primaryButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Execute"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // RealityFlowXRActions
        m_RealityFlowXRActions = asset.FindActionMap("RealityFlowXRActions", throwIfNotFound: true);
        m_RealityFlowXRActions_ToggleRecording = m_RealityFlowXRActions.FindAction("ToggleRecording", throwIfNotFound: true);
        m_RealityFlowXRActions_OpenLLMMenu = m_RealityFlowXRActions.FindAction("OpenLLMMenu", throwIfNotFound: true);
        m_RealityFlowXRActions_Execute = m_RealityFlowXRActions.FindAction("Execute", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // RealityFlowXRActions
    private readonly InputActionMap m_RealityFlowXRActions;
    private List<IRealityFlowXRActionsActions> m_RealityFlowXRActionsActionsCallbackInterfaces = new List<IRealityFlowXRActionsActions>();
    private readonly InputAction m_RealityFlowXRActions_ToggleRecording;
    private readonly InputAction m_RealityFlowXRActions_OpenLLMMenu;
    private readonly InputAction m_RealityFlowXRActions_Execute;
    public struct RealityFlowXRActionsActions
    {
        private @RealityFlowActions m_Wrapper;
        public RealityFlowXRActionsActions(@RealityFlowActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @ToggleRecording => m_Wrapper.m_RealityFlowXRActions_ToggleRecording;
        public InputAction @OpenLLMMenu => m_Wrapper.m_RealityFlowXRActions_OpenLLMMenu;
        public InputAction @Execute => m_Wrapper.m_RealityFlowXRActions_Execute;
        public InputActionMap Get() { return m_Wrapper.m_RealityFlowXRActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(RealityFlowXRActionsActions set) { return set.Get(); }
        public void AddCallbacks(IRealityFlowXRActionsActions instance)
        {
            if (instance == null || m_Wrapper.m_RealityFlowXRActionsActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_RealityFlowXRActionsActionsCallbackInterfaces.Add(instance);
            @ToggleRecording.started += instance.OnToggleRecording;
            @ToggleRecording.performed += instance.OnToggleRecording;
            @ToggleRecording.canceled += instance.OnToggleRecording;
            @OpenLLMMenu.started += instance.OnOpenLLMMenu;
            @OpenLLMMenu.performed += instance.OnOpenLLMMenu;
            @OpenLLMMenu.canceled += instance.OnOpenLLMMenu;
            @Execute.started += instance.OnExecute;
            @Execute.performed += instance.OnExecute;
            @Execute.canceled += instance.OnExecute;
        }

        private void UnregisterCallbacks(IRealityFlowXRActionsActions instance)
        {
            @ToggleRecording.started -= instance.OnToggleRecording;
            @ToggleRecording.performed -= instance.OnToggleRecording;
            @ToggleRecording.canceled -= instance.OnToggleRecording;
            @OpenLLMMenu.started -= instance.OnOpenLLMMenu;
            @OpenLLMMenu.performed -= instance.OnOpenLLMMenu;
            @OpenLLMMenu.canceled -= instance.OnOpenLLMMenu;
            @Execute.started -= instance.OnExecute;
            @Execute.performed -= instance.OnExecute;
            @Execute.canceled -= instance.OnExecute;
        }

        public void RemoveCallbacks(IRealityFlowXRActionsActions instance)
        {
            if (m_Wrapper.m_RealityFlowXRActionsActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IRealityFlowXRActionsActions instance)
        {
            foreach (var item in m_Wrapper.m_RealityFlowXRActionsActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_RealityFlowXRActionsActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public RealityFlowXRActionsActions @RealityFlowXRActions => new RealityFlowXRActionsActions(this);
    public interface IRealityFlowXRActionsActions
    {
        void OnToggleRecording(InputAction.CallbackContext context);
        void OnOpenLLMMenu(InputAction.CallbackContext context);
        void OnExecute(InputAction.CallbackContext context);
    }
}
