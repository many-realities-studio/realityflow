using System;
using UnityEngine;

namespace RealityFlow.NodeGraph
{
    [Serializable]
    public class InputNodePort
    {
        /// <summary>
        /// The constant value is the inline value you can fill in on ports that have no input.
        /// Not all types can be used in a constant value, since ones like Graphs are too complex
        /// to write out in that UI.
        /// </summary>
        [SerializeReference]
        [DiscUnion]
        public NodeValue ConstantValue;
    }
}