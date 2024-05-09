using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class NodeValueType 
    {
        [SerializeReference]
        public Kind Data = new Kind.Int();

        [Serializable]
        public class Kind 
        {
            [Serializable]
            public class Int : Kind { }
            [Serializable]
            public class TypeVariable : Kind
            {
                public int Index;
            }
        }
    }
}