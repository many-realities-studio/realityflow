using UnityEngine;
using System.Collections;
using Graph = RealityFlow.NodeGraph.Graph;
using RealityFlow.NodeGraph;

public class SpawnCube
{
    public static void Execute()
    {
        Vector3 position = new Vector3(-2.70f, -0.05f, -1.16f);
        RealityFlowAPI.Instance.SpawnObject("Cube", position, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);
    }
}