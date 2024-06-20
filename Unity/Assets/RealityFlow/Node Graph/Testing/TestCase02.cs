using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace RealityFlow.NodeGraph.Testing
{
    /// <summary>
    /// Ensure that the type checker prevents constructing a graph that 
    /// </summary>
    public class TestCase02 : MonoBehaviour
    {
        public NodeDefinition OnInteract;
        public NodeDefinition Add;
        public NodeDefinition Integer;
        public NodeDefinition Print;

        NodeIndex start;

        public void Start()
        {
            Graph graph = new("");
            start = graph.AddNode(OnInteract);
            NodeIndex print = graph.AddNode(Print);
            Assert.IsTrue(graph.TryAddExecutionEdge(start, 0, print));
            NodeIndex twelve = graph.AddNode(Integer);
            Assert.IsTrue(graph.GetNode(twelve).TrySetField(0, 12));
            NodeIndex add1 = graph.AddNode(Add);
            Assert.IsTrue(graph.TryAddEdge(twelve, 0, add1, 0));
            NodeIndex add2 = graph.AddNode(Add);
            Assert.IsTrue(graph.TryAddEdge(add1, 0, add2, 0));
            Assert.IsTrue(graph.TryAddEdge(add1, 0, add2, 1));
            Assert.IsTrue(graph.TryAddEdge(add2, 0, print, 0));
            Assert.IsFalse(graph.TryAddEdge(add2, 0, add1, 1));
        }
    }
}