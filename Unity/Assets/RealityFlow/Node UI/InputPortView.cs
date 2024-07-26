using System;
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
        [NonSerialized]
        TMP_Text nonEditorName;

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

            if (view.Graph == null)
                return;

            // If the port is connected, just display a name.
            if (view.Graph.ReverseEdges.ContainsKey(port))
            {
                GameObject nonEditorObj = new("InputPortName");
                GameObject editorPrefab = NodeView.FieldPrefabs[def.Type];
                nonEditorObj.transform.SetParent(editorParent);
                nonEditorObj.transform.localRotation = Quaternion.identity;
                nonEditorObj.transform.localScale = editorPrefab.transform.localScale;
                // this is just to set z position; x and y are controlled by layout
                nonEditorObj.transform.localPosition = Vector3.zero;

                nonEditorName = nonEditorObj.AddComponent<TextMeshProUGUI>();
                TMP_Text editorPrefabName = editorPrefab.GetComponent<IValueEditor>().Name;
                nonEditorName.font = editorPrefabName.font;
                nonEditorName.fontSize = 8;
                nonEditorName.alignment = TextAlignmentOptions.TopLeft;
                nonEditorName.color = Color.white;
            }
            // Otherwise, display an editor for the input port's constant value.
            else
            {
                GameObject editorPrefab = NodeView.FieldPrefabs[def.Type];
                editor = Instantiate(editorPrefab, editorParent).GetComponent<IValueEditor>();
                editor.NodeValue = defaultValue;
                editor.OnTick += value =>
                {
                    RealityFlowAPI.Instance.SetNodeInputConstantValue(view.Graph, port.Node, port.Port, value);
                };
            }

            init = true;

            Render();
        }

        void Render()
        {
            Assert.IsTrue(init, "Must call Init() when creating input port views");

            if (editor != null)
                editor.Name.text = Definition.Name;
            if (nonEditorName != null)
                nonEditorName.text = Definition.Name;
        }
    }
}