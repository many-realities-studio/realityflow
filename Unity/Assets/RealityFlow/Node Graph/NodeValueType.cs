using System;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// Represents every possible type in the NodeGraph type system.
    /// </summary>
    [Serializable]
    public enum NodeValueType
    {
        Int,
        Float,
        Vector2,
        Vector3,
        Quaternion,
        Graph,
        Bool,
        Any,
        GameObject,
        Prefab,
        String,
    }
}