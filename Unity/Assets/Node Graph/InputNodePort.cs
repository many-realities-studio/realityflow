using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class InputNodePort
    {
        [SerializeReference]
        public NodeValue ConstantValue;
    }
}