using UnityEngine;

namespace RealityFlow.NodeGraph.Testing
{
    public static class TestingUtil
    {
        public static Graph SerializationRoundTrip(Graph graph)
        {
            string json = JsonUtility.ToJson(graph);
            return JsonUtility.FromJson<Graph>(json);
        }
    }
}