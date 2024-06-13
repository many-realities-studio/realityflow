using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

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
            }
        }

        [SerializeField]
        Transform editorParent;
        public Transform edgeTarget;

        [NonSerialized]
        public PortIndex port;
        [NonSerialized]
        IValueEditor editor;
        public IValueEditor Editor => editor;

        bool init;

        public void Select()
        {
            GraphView view = GetComponentInParent<GraphView>();
            view.SelectInputPort(port);
        }

        public void Init(NodePortDefinition definition, PortIndex port, GraphView view, NodeValue defaultValue)
        {
            def = definition;
            this.port = port;

            GameObject editorPrefab = NodeView.FieldPrefabs[def.Type];
            editor = Instantiate(editorPrefab, editorParent).GetComponent<IValueEditor>();
            editor.NodeValue = defaultValue;
            editor.OnTick += value =>
            {
                RealityFlowAPI.Instance.SetNodeInputConstantValue(view.Graph, port.Node, port.Port, value);
            };

            init = true;

            Render();
        }

        void Render()
        {
            Assert.IsTrue(init, "Must call Init() when creating input port views");

            editor.Name.text = Definition.Name;
        }
    }
}