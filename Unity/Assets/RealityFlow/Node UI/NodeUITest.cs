using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using UnityEngine;
using UnityEngine.Assertions;

namespace RealityFlow.NodeUI
{
    public class NodeUITest : MonoBehaviour
    {
        public NodeDefinition Activate;
        public NodeDefinition Print;
        public NodeDefinition AddDef;
        public NodeDefinition Integer;
        public NodeDefinition This;
        public NodeDefinition Name;

        GraphView view;

        Graph ConstructGraph()
        {
            Graph graph = new();
            NodeIndex start = graph.AddNode(Activate);
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

        void Start()
        {
            view = GetComponent<GraphView>();

            Graph graph = ConstructGraph();
            view.Graph = graph;
        }
    }
}