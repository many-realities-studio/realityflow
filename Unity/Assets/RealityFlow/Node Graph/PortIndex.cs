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

        public InputNodePort AsInput(Graph graph) => graph.GetNode(Node).GetInput(Port);
        public InputNodePort AsInput(ReadonlyGraph graph) => graph.GetNode(Node).GetInput(Port);
    }
}