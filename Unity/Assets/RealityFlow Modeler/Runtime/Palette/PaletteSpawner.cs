using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;

/// <summary>
/// Class PaletteSpawner spawns the palette if it is not in the scene. If the palette is in the scene then this class will toggle the visibility of it.
/// </summary>
public class PaletteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject palettePrefab;
    [SerializeField] private NetworkSpawnManager networkSpawnManager;
    private StatefulInteractable isLeftHandDominant;
    private GameObject palette;
    private bool paletteShown;

    public void SpawnPalette()
    {
        Vector3 paletteSize = palettePrefab.transform.localScale;

        // If the palette is not in the current scene, then spawn it in
        if(palette == null)
        {
            // Debug.Log("SpawnPalette() was triggered to spawn a palette");
            palette = NetworkSpawnManager.Find(this).SpawnWithPeerScope(palettePrefab);

            paletteShown = true;

            // Set the ownership of the spawned palette to the user who spawned it
            palette.GetComponent<NetworkedPalette>().owner = true;
            //palette.GetComponent<NetworkedPalette>().needData = true;

            try
            {
                isLeftHandDominant = GameObject.FindGameObjectsWithTag("DominantHandManager")[0].GetComponent<PressableButton>();
                Debug.Log(GameObject.FindGameObjectsWithTag("DominantHandManager")[0]);
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log(e + " Has the palette been spawned in your scene?");
            }
        }
        // We set the scale of the palette to 0 to effectively hide it. We do not disable it in the hierarchy as this will turn off
        // all scripts which would not allow the user to use any functions from the palette.
        else if (paletteShown)
        {
            palette.transform.localScale = new Vector3(0, 0, 0);
            paletteShown = false;
        }
        else if (!paletteShown)
        {
            palette.transform.localScale = paletteSize;
            paletteShown = true;
        }
    }
}
