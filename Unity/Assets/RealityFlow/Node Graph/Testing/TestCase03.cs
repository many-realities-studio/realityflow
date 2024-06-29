using UnityEngine;
using UnityEngine.Assertions;

namespace RealityFlow.NodeGraph.Testing
{
    /// <summary>
    /// Test a while loop printing from 1 to 10.
    /// </summary>
    public class TestCase03 : MonoBehaviour
    {
        public NodeDefinition OnInteract;
        public NodeDefinition Integer;
        public NodeDefinition Add;
        public NodeDefinition While;
        public NodeDefinition Print;

        NodeIndex start;

        public Graph ConstructGraph()
        {
            Graph graph = new("");
            start = graph.AddNode(OnInteract);

            return graph;
        }

        public void Start()
        {
            Graph graph = TestingUtil.SerializationRoundTrip(ConstructGraph());
            EvalContext ctx = new();
            ctx.EvaluateGraphFromRoot(gameObject, new(graph), start);

            string json = JsonUtility.ToJson(graph, true);
            Debug.Log(json);
        }
    }
}