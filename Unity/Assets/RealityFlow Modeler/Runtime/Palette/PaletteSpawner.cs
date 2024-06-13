using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;


/// <summary>
/// Class PaletteSpawner spawns the palette if it is not in the scene. If the palette is in the scene then this class will toggle the visibility of it.
/// </summary>
public class PaletteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject palettePrefab;
    [SerializeField] private GameObject playPalettePrefab;
    [SerializeField] private NetworkSpawnManager networkSpawnManager;
    private NetworkedPalette networkedPalette;
    private PaletteSwitcher paletteSwitcher;
    private StatefulInteractable isLeftHandDominant;
    private GameObject palette;
    private GameObject playPalette;
    private bool paletteShown;
    private bool lastPlayState;

    private Vector3 paletteSize;
    private Vector3 playPaletteSize;

    /*public void SpawnPalette()
    {
        Vector3 paletteSize = palettePrefab.transform.localScale;

        // If the palette is not in the current scene, then spawn it in
        if(palette == null)
        {
            // Debug.Log("SpawnPalette() was triggered to spawn a palette");
            palette = NetworkSpawnManager.Find(this).SpawnWithPeerScope(palettePrefab);
            PaletteHandManager phm = palette.GetComponent<PaletteHandManager>();
            phm.paletteManager = gameObject;
            phm.left = GetComponents<OnButtonPress>()[0];
            phm.right = GetComponents<OnButtonPress>()[1];
            phm.left.enabled = false;
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
    }*/

    void Start()
    {
        paletteSwitcher = gameObject.GetComponent<PaletteSwitcher>();

        PaletteHandManager.OnHandChange += SwitchHands;

        // Ensure users do not attempt to spawn in a palette immediately on spawn to avoid race conditions
        // with setting up references for paletteSwitcher.networkedPlayManager.
        StartCoroutine(AllowSpawnMechanics(1));
    }

    void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
    }

    IEnumerator AllowSpawnMechanics(int secs)
    {
        yield return new WaitForSeconds(secs);
        gameObject.GetComponents<OnButtonPress>()[0].enabled = true;
    }

    public void ControlPalette()
    {
        paletteSize = palettePrefab.transform.localScale;
        playPaletteSize = playPalettePrefab.transform.localScale;

        // If the palette is not in the current scene, then spawn it in
        if(palette == null)
        {
            // Debug.Log("ControlPalette() was triggered to spawn a palette");
            palette = networkSpawnManager.SpawnWithPeerScope(palettePrefab);
            playPalette = networkSpawnManager.SpawnWithPeerScope(playPalettePrefab);

            // responsible for palette being attached to hand.
            PaletteHandManager phm = palette.GetComponent<PaletteHandManager>();
            phm.paletteManager = gameObject;
            phm.left = GetComponents<OnButtonPress>()[0];
            phm.right = GetComponents<OnButtonPress>()[1];
            phm.left.enabled = false;

            paletteShown = true;

            networkedPalette = palette.GetComponent<NetworkedPalette>();

            // Set the ownership of the spawned palette to the user who spawned it
            networkedPalette.owner = true;
            playPalette.GetComponent<NetworkedPlayPalette>().owner = true;

            try
            {
                isLeftHandDominant = GameObject.FindGameObjectsWithTag("DominantHandManager")[0].GetComponent<PressableButton>();
                Debug.Log(GameObject.FindGameObjectsWithTag("DominantHandManager")[0]);
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log(e + " Has the palette been spawned in your scene?");
            }

            // Hide the opposite palette when spawning in palette for the first time
            if (paletteSwitcher.networkedPlayManager.playMode)
            {
                Debug.Log("Paly mode is on when you joined");
                palette.transform.localScale = new Vector3(0, 0, 0);
            }
            else
            {
                playPalette.transform.localScale = new Vector3(0, 0, 0);
            }
        }
        // We set the scale of the palette to 0 to effectively hide it. We do not disable it in the hierarchy as this will turn off
        // all scripts which would not allow the user to use any functions from the palette.
        else if (paletteShown)
        {
            // Hide the current palette that is being used
            if (paletteSwitcher.networkedPlayManager.playMode)
            {
                playPalette.transform.localScale = new Vector3(0, 0, 0);
            }
            else
            {
                palette.transform.localScale = new Vector3(0, 0, 0);
            }

            paletteShown = false;
        }
        else if (!paletteShown)
        {
            // Hide the current palette that is being used
            if (paletteSwitcher.networkedPlayManager.playMode)
            {
                playPalette.transform.localScale = playPaletteSize;
            }
            else
            {
                palette.transform.localScale = paletteSize;
            }
            
            paletteShown = true;
        }
    }

    void Update()
    {
        if (paletteSwitcher.networkedPlayManager != null)
        {
            if (lastPlayState != paletteSwitcher.networkedPlayManager.playMode)
            {
                lastPlayState = paletteSwitcher.networkedPlayManager.playMode;

                if (palette != null)
                {
                    SwitchPalettes(paletteSwitcher.networkedPlayManager.playMode);
                }
            }
        }
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the grip buttons depending on the dominant hand
        if(isLeftHandDominant)
        {
            gameObject.GetComponents<OnButtonPress>()[0].enabled = false;
            gameObject.GetComponents<OnButtonPress>()[1].enabled = true;
        }
        else
        {
            gameObject.GetComponents<OnButtonPress>()[0].enabled = true;
            gameObject.GetComponents<OnButtonPress>()[1].enabled = false;
        }
    }

    private void SwitchPalettes(bool isPlayMode)
    {
        // Switch the palettes depending on whether the room is in Play or Edit mode
        if (isPlayMode)
        {
            palette.transform.localScale = new Vector3(0, 0, 0);

            // Turn off all buttons on the Edit palette so users using any tool should now be out of it upon entering Play mode
            foreach (StatefulInteractable button in palette.GetComponentsInChildren<StatefulInteractable>(true))
            {
                // Toggle off the button if it is a toggle button and not an undesirable button
                if ((int)button.ToggleMode == 1 && button.gameObject.name != "Switch Hands Button" && button.gameObject.name != "Play Button")
                {
                    // Debug.Log("button.gameObject.name = " + button.gameObject.name + " with toggle state = " + button.ToggleMode);
                    button.ForceSetToggled(false);
                }
            }

            // Revert palette back to Home Menu
            networkedPalette.homeMenu.SetActive(true);
            networkedPalette.meshMenu.SetActive(false);
            networkedPalette.path3DMenu.SetActive(false);
            networkedPalette.manipulateMenu.SetActive(false);
            networkedPalette.transformMenu.SetActive(false);
            networkedPalette.editMenu.SetActive(false);
            networkedPalette.colorsMenu.SetActive(false);
            networkedPalette.shadersMenu.SetActive(false);

            playPalette.transform.localScale = playPaletteSize;
        }
        else
        {
            playPalette.transform.localScale = new Vector3(0, 0, 0);
            palette.transform.localScale = paletteSize;
        }
    }
}
