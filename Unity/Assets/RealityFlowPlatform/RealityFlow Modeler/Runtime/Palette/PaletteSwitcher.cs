using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class PaletteSwitcher switches the palette between its Play and Edit variants depending on the state of PlayModeManager.
/// </summary>
public class PaletteSwitcher : MonoBehaviour
{
    private PaletteSpawner paletteSpawner;
    public NetworkedPlayManager networkedPlayManager;

    public static event Action<bool> OnPlayModeChange;

    // Start is called before the first frame update
    void Start()
    {
        paletteSpawner = gameObject.GetComponent<PaletteSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkedPlayManager != null)
        {
            // Debug.Log("playModeManager.playMode = " + networkedPlayManager.playMode);
        }
    }

    public void SetPlayModeManagerRef(NetworkedPlayManager managerReference)
    {
        networkedPlayManager = managerReference;
        OnPlayModeChange?.Invoke(networkedPlayManager.playMode);
    }
}
