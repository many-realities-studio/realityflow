using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParameterSpawn : MonoBehaviour
{
    private SpawnParameterAtRay handReference;
    private string currentNode;

    void Start()
    {
        currentNode = this.gameObject.GetComponent<ButtonActionReference>().nodeName;
        handReference = GameObject.Find("GraphContent").GetComponent<SpawnParameterAtRay>();
    }
    
    public void TriggerNodeSpawnProcess()
    {
        handReference.RaySpawnToggle(currentNode);
    }
}
