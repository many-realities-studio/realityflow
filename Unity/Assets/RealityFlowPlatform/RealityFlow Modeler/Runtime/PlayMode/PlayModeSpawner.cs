using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;

/// <summary>
/// Class PlayModeSpawner spawns the PlayModeManager if it is not in the scene. Additionally, it is responsible for switching
/// the current state of the room to Play or Edit mode.
/// </summary>
public class PlayModeSpawner : MonoBehaviour
{
    [SerializeField] private NetworkSpawnManager networkSpawnManager;
    [SerializeField] private PaletteSwitcher paletteSwitcher;
    [SerializeField] private GameObject managerPrefab;
    private GameObject playModeManager;
    private bool spawnManager;
    private bool refSet;

    public bool isActive;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelaySearchForPlayModeManager(1));

        // If the PlayModeManager is not in the scene, then spawn it in
        // if(playModeManager == null)
        // {
        //     spawnManager = true;
        // }
    }

    // When the flag is sent, spawn the manager. We do this in Update() since it will spawn it in when it is ready as spawning in Start() does not work.
    // void Update()
    // {
    //     if (spawnManager)
    //     {
    //         spawnManager = false;

    //         StartCoroutine(DelaySearchForPlayModeManager(1));
    //         networkSpawnManager.SpawnWithRoomScopeWithReturn(managerPrefab);
    //         // paletteSwitcher.SetPlayModeManagerRef(playModeManager.GetComponent<NetworkedPlayManager>());
    //         // Debug.Log("playModeManager.name = " + playModeManager.name + " in scene " + playModeManager.transform.parent.parent.parent.name);
    //     }

    //     // if (playModeManager != null && playModeManager.GetComponent<NetworkedPlayManager>().HasContext() && !refSet)
    //     // {
    //     //     refSet = true;
    //     //     paletteSwitcher.SetPlayModeManagerRef(playModeManager.GetComponent<NetworkedPlayManager>());
    //     //     Debug.Log("Has context so set the reference");
    //     //     Debug.Log("playModeManager.name = " + playModeManager.name + " in scene " + playModeManager.transform.parent.parent.parent.name);
    //     // }
    // }

    // After a short delay, find the PlayModeManager as it should now be successfully created
    IEnumerator DelaySearchForPlayModeManager(int secs)
    {
        yield return new WaitForSeconds(secs);
        FindPlayModeManager();
    }

    public void Activate(int tool, bool status)
    {
        // If Play tool
        if(tool == 11)
        {
            isActive = status;

            if (playModeManager != null)
            {
                playModeManager.GetComponent<NetworkedPlayManager>().playMode = isActive;
                playModeManager.GetComponent<NetworkedPlayManager>().hasContext = true;
            }
        }

        // If Exit tool
        if(tool == -1)
        {
            isActive = status;

            if (playModeManager != null)
            {
                playModeManager.GetComponent<NetworkedPlayManager>().playMode = isActive;
                playModeManager.GetComponent<NetworkedPlayManager>().hasContext = true;
            }
        }
    }

    // Iterate through all gameobjects to see if a PlayModeManager is already in the room
    private void FindPlayModeManager()
    {
        // A PlayModeManager is already in the room
        if (FindObjectOfType<NetworkedPlayManager>() != null)
        {
            playModeManager = FindObjectOfType<NetworkedPlayManager>().gameObject;
            paletteSwitcher.SetPlayModeManagerRef(playModeManager.GetComponent<NetworkedPlayManager>());
            // Debug.Log("playModeManager.name = " + playModeManager.name + " in scene " + playModeManager.transform.parent.parent.parent.name);
        }
        else
        {
            networkSpawnManager.SpawnWithRoomScopeWithReturn(managerPrefab);
            playModeManager = FindObjectOfType<NetworkedPlayManager>().gameObject;
            paletteSwitcher.SetPlayModeManagerRef(playModeManager.GetComponent<NetworkedPlayManager>());
        }
        // else if (playModeManager != null)
        // {
        //     Debug.Log("There is already a PlayModeManager.\n" + "playModeManager.name = " + playModeManager.name + " in scene " + playModeManager.transform.parent.parent.parent.name);
        // }
        // else
        // {
        //     // Debug.Log("There is no PlayModeManager. Spawning soon...");
        // }
        
    }
}
