using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RealityFlow.NodeGraph;
using UnityEngine;
using UnityEngine.Assertions;

namespace RealityFlow.NodeUI
{
    public class NodeUITest : MonoBehaviour
    {
        NodeDefinition Activate;
        NodeDefinition Print;
        NodeDefinition AddDef;
        NodeDefinition Integer;
        NodeDefinition This;
        NodeDefinition Name;

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
            List<NodeDefinition> nodes = RealityFlowAPI.Instance.GetAvailableNodeDefinitions();
            Dictionary<string, NodeDefinition> dict = nodes.ToDictionary(def => def.Name);
            Activate = dict["Activate"];
            Print = dict["Print"];
            AddDef = dict["IntAdd"];
            Integer = dict["Integer"];
            This = dict["ThisObject"];
            Name = dict["ObjectName"];

            view = GetComponent<GraphView>();

            Graph graph = ConstructGraph();
            view.Graph = graph;
        }
    }
}