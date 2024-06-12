using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;

/// <summary>
/// Class UpdateGridToggles updates the toggles across all menus to match what is currently selected.
/// </summary>
public class UpdateGridToggles : MonoBehaviour
{
    private StatefulInteractable gridModeToggle;
    [Tooltip("Check = Grid mode on\nUnchecked = Grid mode off")]
    public bool lastSnapGridValue;
    private UpdateGridToggles[] snapTogglesScripts;

    void Start()
    {
        gridModeToggle = gameObject.GetComponent<StatefulInteractable>();

        // Get all references of any snap unit sliders on the owner's palette
        snapTogglesScripts = NetworkedPalette.reference.GetComponentsInChildren<UpdateGridToggles>(true);
    }

    void Update()
    {
        // Update values only for the owner
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            if (lastSnapGridValue != gridModeToggle.IsToggled)
            {
                lastSnapGridValue = gridModeToggle.IsToggled;
                UpdateGridModes(gridModeToggle.IsToggled);
            }
        }
    }

    private void UpdateGridModes(bool gridModeValue)
    {
        // Assign all other snap grid modes the same value for the owner
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            for (int i = 0; i < snapTogglesScripts.Length; i++)
            {
                // Ensure that we do not assign this game object's snap grid value but rather only all other occurrences of UpdateGridToggles
                if (snapTogglesScripts[i].transform.parent.parent.parent.parent.name != gameObject.transform.parent.parent.parent.parent.name)
                {
                    snapTogglesScripts[i].GetComponent<StatefulInteractable>().ForceSetToggled(gridModeValue);
                    snapTogglesScripts[i].GetComponent<UpdateGridToggles>().lastSnapGridValue = gridModeValue;
                }
            }
        }
    }
}
