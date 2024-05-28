using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class PaletteToggleManager must be provided an index through the Unity inspector to toggle off all toggle buttons besides the one located by the index.
/// </summary>
public class PaletteToggleManager : MonoBehaviour
{
    [SerializeField] private NetworkedPalette networkedPalette;

    public void toggleOffAllExceptThisOne(int index) 
    {
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            for (int i = 0; i < networkedPalette.toggleStates.Length; i++)
            {
                if (i != index)
                    networkedPalette.toggleStates[i].ForceSetToggled(false);
            }
        }
    }
}
