using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit;

/// <summary>
/// Class SwitchText swaps the text of the Cancel Mesh Creation popup to the appropriate primary button.
/// </summary>
public class SwitchText : MonoBehaviour
{
    [Tooltip("Used only for the initial call when the Mesh Menu is entered to determine which hand is dominant.")]
    [SerializeField] private StatefulInteractable dominantHandButton;

    void Start()
    {
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            SwapText(dominantHandButton.IsToggled);
            PaletteHandManager.OnHandChange += SwapText;
        }
    }

    public void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwapText;
    }

    private void SwapText(bool isLeftHandDominant)
    {
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            if(isLeftHandDominant)
            {
                gameObject.GetComponent<TMP_Text>().text = "Exit mesh creation mode by\npressing 'X'";
            }
            else
            {
                gameObject.GetComponent<TMP_Text>().text = "Exit mesh creation mode by\npressing 'A'";
            }
        }
    }
}
