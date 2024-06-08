using UnityEngine;

class SpawnObject : ActionLogger.ILogAction
{
    public GameObject spawned;

    public string Name() => "SpawnObject";

    public void Undo()
    {
        RealityFlowAPI.Instance.DespawnObject(spawned, log: false);
    }
}