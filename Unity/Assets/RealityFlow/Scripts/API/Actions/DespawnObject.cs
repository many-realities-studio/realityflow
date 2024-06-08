public class DespawnObject : ActionLogger.ILogAction
{
    public string Name() => "DespawnObject";

    public void Undo()
    {
        // RealityFlowAPI.Instance.SpawnObject();
    }
}