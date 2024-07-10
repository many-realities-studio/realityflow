using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using RealityFlow.NodeUI;

public class OpenKeyboardOnButtonPress : MonoBehaviour
{
    public PressableButton PressableButton; // Reference to the pressable button
    public Custom_MRTK_InputField InputField; // Reference to the MRTK input field

    private void Start()
    {
        if (PressableButton != null)
        {
            PressableButton.OnClicked.AddListener(OpenKeyboard); // Add listener for button click
        }
    }

    private void OpenKeyboard()
    {
        NonNativeKeyboard keyboard = NonNativeKeyboard.Instance;
        if (keyboard != null && InputField != null)
        {
            keyboard.PresentKeyboard(InputField); // Pass the input field to the keyboard
        }
    }
}