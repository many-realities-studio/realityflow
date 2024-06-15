using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class OutputPortView : MonoBehaviour
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

        [NonSerialized]
        public PortIndex port;

        [SerializeField]
        TextMeshProUGUI portName;

        public Transform edgeTarget;

        public void Select()
        {
            GraphView view = GetComponentInParent<GraphView>();
            view.SelectOutputPort(port);
        }

        void Render()
        {
            portName.text = Definition.Name;
        }
    }
}