using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class SelectTool grabs a reference to the active state of the select tool.
/// </summary>
public class SelectTool : MonoBehaviour
{
    public bool isActive;
    [Tooltip("Displays the owner's UUID and should not be manually changed")]
    public string ownerName;

    public void Activate(int tool, bool status)
    {
        if(tool == 0)
        {
            isActive = status;
        }
    }

    public void AssignName(string name)
    {
        ownerName = name;
    }
}
