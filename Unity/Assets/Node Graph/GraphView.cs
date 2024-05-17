using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// A read-only view of a graph, usually used by nodes during graph evaluation so as not to 
    /// invalidate the graph mid-evaluation.
    /// </summary>
    public struct GraphView
    {
        Graph graph;

        public GraphView(Graph graph)
        {
            this.graph = graph;
        }  

        public Node GetNode(NodeIndex index)
        {
            return graph.GetNode(index);
        }

        public int ExecutionInputs => graph.ExecutionInputs;

        public List<NodeIndex> InputExecutionEdges(int index)
            => graph.InputExecutionEdges(index);

        public bool TryGetOutputPortOf(PortIndex input, out PortIndex output)
            => graph.TryGetOutputPortOf(input, out output);

        public List<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => graph.GetExecutionInputPortsOf(outputPort);
    }
}