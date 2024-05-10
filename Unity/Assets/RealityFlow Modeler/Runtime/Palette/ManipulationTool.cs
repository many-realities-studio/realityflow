using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationTool : MonoBehaviour
{
    public bool isActive;

    public void Activate(int tool, bool status)
    {
        if(tool == 7)
        {
            isActive = status;
        }
    }
}
