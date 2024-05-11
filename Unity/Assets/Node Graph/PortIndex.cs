using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public struct PortIndex
    {
        [SerializeReference]
        public Node Node;
        public int Port;

        public readonly InputNodePort AsInput => Node.GetInput(Port);
        public readonly OutputNodePort AsOutput => Node.GetOutput(Port);

        public PortIndex(Node node, int port)
        {
            Node = node;
            Port = port;
        }
    }
}