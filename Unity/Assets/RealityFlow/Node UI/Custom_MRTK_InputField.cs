using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;

namespace RealityFlow.NodeUI
{
    /// <summary>   
    /// See Custom_InputField.
    /// Replaces MRTK's specialized InputField implementation.
    /// </summary>
    [AddComponentMenu("MRTK/UX/Custom - MRTK Input Field")]
    public class Custom_MRTK_InputField : Custom_InputField
    {
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
            }
        }
    }

}