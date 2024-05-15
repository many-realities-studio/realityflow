using System;

namespace RealityFlow.NodeGraph
{
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
    }
}