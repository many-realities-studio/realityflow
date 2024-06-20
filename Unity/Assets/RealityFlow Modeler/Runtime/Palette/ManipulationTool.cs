using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationTool : MonoBehaviour
{
    public bool isActive;

    public ManipulationMode mode { get; private set; }
    public int otherMode;

    public void SetManipulationMode(int newMode)
    {
        if (NetworkedPalette.reference != null && NetworkedPalette.reference.owner)
        {
            otherMode = newMode;
            if (newMode == 1)
                mode = ManipulationMode.vertex;
            else if (newMode == 2)
                mode = ManipulationMode.edge;
            else if (newMode == 3)
                mode = ManipulationMode.face;
            else
            {
                if(NetworkedPalette.reference != null)
                {
                   if(!NetworkedPalette.reference.toggleStates[11].IsToggled &&
                      !NetworkedPalette.reference.toggleStates[12].IsToggled &&
                      !NetworkedPalette.reference.toggleStates[13].IsToggled )
                    {
                        mode = ManipulationMode.mObject;
                    }
                }
            }

            //HandleSpawner.Instance.SetManipulationMode(mode);
        }
    }

    public void Activate(int tool, bool status)
    {
        if(tool == 7)
        {
            isActive = status;
        }
    }
}
