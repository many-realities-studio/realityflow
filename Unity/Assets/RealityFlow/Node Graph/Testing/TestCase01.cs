using UnityEngine;
using UnityEngine.Assertions;

namespace RealityFlow.NodeGraph.Testing
{
    /// <summary>
    /// Test a simple graph with an integer, print, and onInteract node which will print the integer.
    /// </summary>
    public class TestCase01 : MonoBehaviour
    {
        public NodeDefinition OnInteract;
        public NodeDefinition Integer;
        public NodeDefinition Print;
        public NodeDefinition This;
        public NodeDefinition Name;

        NodeIndex start;

        public Graph ConstructGraph()
        {
            Graph graph = new();
            start = graph.AddNode(OnInteract);
            NodeIndex print = graph.AddNode(Print);
            Assert.IsTrue(graph.TryAddExecutionEdge(start, 0, print));
            NodeIndex twelve = graph.AddNode(Integer);
            Assert.IsTrue(graph.GetNode(twelve).TrySetField(0, 12));
            Assert.IsTrue(graph.TryAddEdge(twelve, 0, print, 0));
            NodeIndex printName = graph.AddNode(Print);
            NodeIndex thisObj = graph.AddNode(This);
            NodeIndex objName = graph.AddNode(Name);
            Assert.IsTrue(graph.TryAddEdge(thisObj, 0, objName, 0));
            Assert.IsTrue(graph.TryAddEdge(objName, 0, printName, 0));
            Assert.IsTrue(graph.TryAddExecutionEdge(start, 0, printName));

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