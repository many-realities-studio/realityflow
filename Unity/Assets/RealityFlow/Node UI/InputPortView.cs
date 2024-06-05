using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class InputPortView : MonoBehaviour
    {
        NodePortDefinition def;
        public NodePortDefinition Definition
        {
            get => def;
            set
            {
                def = value;
                Render();
            }
        }

        [SerializeField]
        IntegerEditor editor;

        void Render()
        {
            // editor.Name = Definition.Name;
        }
    }
}