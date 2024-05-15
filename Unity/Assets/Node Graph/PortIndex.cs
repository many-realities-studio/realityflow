using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public struct PortIndex
    {
        public NodeIndex Node;
        public int Port;

        public PortIndex(NodeIndex node, int port)
        {
            Node = node;
            Port = port;
        }

        public Node GetNode(Graph graph) => graph.GetNode(Node);

        public InputNodePort AsInput(Graph graph) => GetNode(graph).GetInput(Port);
    }
}