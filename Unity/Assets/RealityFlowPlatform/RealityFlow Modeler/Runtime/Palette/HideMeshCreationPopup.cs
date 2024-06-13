using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;

/// <summary>
/// Class HideMeshCreationPopup is attached to each appropriate mesh button in the Mesh Menu.
/// It is used to hide the popup and detoggle the selected mesh button upon the press of the
/// user's primary button.
/// </summary>
public class HideMeshCreationPopup : MonoBehaviour
{
    private PrimitiveSpawner primitiveSpawner;
    private bool lastActiveState;

    void Start()
    {
        primitiveSpawner = GameObject.Find("Primitive Spawn Input Manager").GetComponent<PrimitiveSpawner>();
    }

    void Update()
    {
        if (lastActiveState != primitiveSpawner.active)
        {
            lastActiveState = primitiveSpawner.active;

            // If we are not in mesh creation mode then detoggle the current active button
            if (!primitiveSpawner.active)
            {
                gameObject.GetComponent<StatefulInteractable>().ForceSetToggled(false);
            }
        }
    }
}
