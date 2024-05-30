using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;

/// <summary>
/// Class PaletteBackButtonToggle should toggle off all buttons in the current menu.
/// </summary>
public class PaletteBackButtonToggle : MonoBehaviour
{
    [SerializeField] private PressableButton[] menuButtons;

    public void ToggleOffAllMenuButtons()
    {
        foreach (PressableButton button in menuButtons)
        {
            button.ForceSetToggled(false);
        }
    }
}
