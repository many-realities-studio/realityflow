using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.Scripting;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// Node fields are UI-centered values inherent to a node that cannot be passed in by a port.
    /// For example, the integer value in an integer node is a field.
    /// </summary>
    [Serializable]
    public struct NodeFieldDefinition
    {
        public string Name;
        public NodeValueType DefaultType;
        [SerializeReference]
        [DiscUnion]
        public NodeValue DefaultValue;

        public readonly string GetDescriptor()
        {
            return $@"  {{
                    name: ""{Name}"",
                    defaultType: ""{DefaultType}"",
                    defaultValue: ""{DefaultValue}"",
                }}";
        }
    }
}