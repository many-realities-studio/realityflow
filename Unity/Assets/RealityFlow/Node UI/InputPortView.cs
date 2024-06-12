using System;
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

        [NonSerialized]
        public PortIndex port;

        [SerializeField]
        Transform editorParent;
        public Transform edgeTarget;

        IValueEditor editor;
        bool init;

        public void Select()
        {
            GraphView view = GetComponentInParent<GraphView>();
            view.SelectInputPort(port);
        }

        void Init()
        {
            GameObject editorPrefab = NodeView.FieldPrefabs[def.Type];
            editor = Instantiate(editorPrefab, editorParent).GetComponent<IValueEditor>();

            init = true;
        }

        void Render()
        {
            if (!init)
                Init();

            editor.Name.text = Definition.Name;
        }
    }
}