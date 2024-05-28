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
    [Serializable]
    public struct ReadonlyGraph : IEquatable<ReadonlyGraph>
    {
        [SerializeField]
        Graph graph;

        public ReadonlyGraph(Graph graph)
        {
            this.graph = graph;
        }

        public Node GetNode(NodeIndex index)
        {
            return graph.GetNode(index);
        }

        public List<NodeValueType> OutputPorts => graph.OutputPorts;

        public int ExecutionInputs => graph.ExecutionInputs;

        public List<NodeIndex> InputExecutionEdges(int index)
            => graph.InputExecutionEdges(index);

        public bool TryGetOutputPortOf(PortIndex input, out PortIndex output)
            => graph.TryGetOutputPortOf(input, out output);

        public bool TryGetGraphOutputSource(int outputIndex, out PortIndex port)
            => graph.TryGetGraphOutputSource(outputIndex, out port);

        public List<NodeIndex> GetExecutionInputPortsOf(PortIndex outputPort)
            => graph.GetExecutionInputPortsOf(outputPort);

        public override readonly bool Equals(object obj)
        {
            if (obj is ReadonlyGraph view)
                return Equals(view);
            return false;
        }

        public readonly bool Equals(ReadonlyGraph other)
        {
            return graph == other.graph;
        }

        public readonly override int GetHashCode()
        {
            return graph.GetHashCode();
        }

        public static bool operator ==(ReadonlyGraph left, ReadonlyGraph right) => left.Equals(right);
        public static bool operator !=(ReadonlyGraph left, ReadonlyGraph right) => !(left == right);
    }
}