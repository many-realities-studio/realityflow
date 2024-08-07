using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class xrayTool handles toggling the XrayTool for handles and meshes
/// </summary>
public class XrayTool : MonoBehaviour
{
    //HandleSpawner spawner;

    public bool isActive;

    void Start()
    {
        //spawner = HandleSpawner.Instance;
    }

    private void EnterXrayMode()
    {
        /*if (spawner == null)
            spawner = HandleSpawner.Instance;

        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            spawner.EnterXrayMode();
        }*/
    }

    private void ExitXrayMode()
    {
        /*
        if (spawner == null)
            spawner = HandleSpawner.Instance;

        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            spawner.ExitXrayMode();
        }
        */
    }

    public void Activate(int tool, bool status)
    {
        if (tool == 9)
        {
            isActive = status;

            if (isActive)
            {
                EnterXrayMode();
            }
            else if (!isActive)
            {
                ExitXrayMode();
            }
        }
    }
}
