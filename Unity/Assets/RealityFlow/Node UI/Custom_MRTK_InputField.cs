using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

namespace RealityFlow.NodeUI
{
    [AddComponentMenu("MRTK/UX/Custom - MRTK Input Field")]
    public class Custom_MRTK_InputField : Custom_InputField
    {
        public GameObject nonNativeKeyboardPrefab;

        protected override void Start()
        {
            base.Start();
            onSelect.AddListener(OnSelected); // Hook into the onSelect event
            //onDeselect.AddListener(OnDeselected); // Hook into the onDeselect event

            // Ensure NonNativeKeyboard is instantiated
            //EnsureNonNativeKeyboard();

            // Ensure OpenKeyboardOnButtonPress script is attached and configured
            EnsureOpenKeyboardOnButtonPress();

            // Log to confirm start method is called
            Debug.Log("Custom_MRTK_InputField Start method called.");
        }

        private void EnsureNonNativeKeyboard()
        {
            if (NonNativeKeyboard.Instance == null)
            {
                Debug.Log("NonNativeKeyboard instance is null, instantiating from prefab.");
                if (nonNativeKeyboardPrefab != null)
                {
                    //NonNativeKeyboard.InitializeInstance(nonNativeKeyboardPrefab);
                }
                else
                {
                    Debug.LogError("NonNativeKeyboard prefab is not assigned.");
                }
            }
        }

        /// <summary>
        /// Called when the input field is selected.
        /// </summary>
        /// <param name="text">The current text of the input field.</param>
        private void OnSelected(string text)
        {
            if (NonNativeKeyboard.Instance != null)
            {
                Debug.Log("Input field selected, presenting keyboard.");
                NonNativeKeyboard.Instance.PresentKeyboard(this);
            }
            else
            {
                Debug.LogError("NonNativeKeyboard instance is null. Ensure it is in the scene and properly initialized.");
            }
        }

        /// <summary>
        /// Called when the input field is deselected.
        /// </summary>
        /// <param name="text">The current text of the input field.</param>

        /// <summary>
        /// Activate the input field.
        /// </summary>
        public void ActivateMRTKTMPInputField()
        {
            MRTKInputFieldManager.SetCurrentInputField(this);
            ActivateInputField();
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>Override OnDeselect such that the base is only called when the call comes from the TMP_InputField/MRTKInputFieldManager scripts or we are not using an HMD.
        /// When using HMD we don't want the input field to be deselected just because someone did a pinch or another gesture that triggers this function.
        /// We also call the base when we are selecting another input field so that we don't have multiple ones being selected at once.</para>
        /// </remarks>
        public override void OnDeselect(BaseEventData eventData)
        {
            if (eventData == null || XRSubsystemHelpers.DisplaySubsystem == null)
            {
                base.OnDeselect(eventData);
                MRTKInputFieldManager.RemoveCurrentInputField(this);

                /*
                if (NonNativeKeyboard.Instance != null)
                {
                    Debug.Log("OnDeselect called, closing keyboard.");
                    NonNativeKeyboard.Instance.Close(); // Close the keyboard when the input field is deselected
                }
                else
                {
                    Debug.LogError("NonNativeKeyboard instance is null.");
                }
               */
            }
        }

        private void EnsureOpenKeyboardOnButtonPress()
        {
            OpenKeyboardOnButtonPress openKeyboardScript = GetComponent<OpenKeyboardOnButtonPress>();
            if (openKeyboardScript == null)
            {
                openKeyboardScript = gameObject.AddComponent<OpenKeyboardOnButtonPress>();
            }

            // Set the necessary fields for OpenKeyboardOnButtonPress script
            openKeyboardScript.InputField = this;

            // Find the PressableButton in the parent hierarchy
            openKeyboardScript.PressableButton = FindInParents<PressableButton>(this.transform);

            if (openKeyboardScript.PressableButton == null)
            {
                Debug.LogError("PressableButton not found in parent hierarchy.");
            }
        }

        private T FindInParents<T>(Transform child) where T : Component
        {
            Transform t = child;
            while (t != null)
            {
                T component = t.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                t = t.parent;
            }
            return null;
        }
    }
}
