using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public struct NodeFieldDefinition
    {
        public string Name;
        public NodeValue Default;
    }
}