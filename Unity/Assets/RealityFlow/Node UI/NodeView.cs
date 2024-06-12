using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace RealityFlow.NodeUI
{
    public class NodeView : MonoBehaviour
    {
        (NodeIndex index, Node value) nodeInfo;
        public (NodeIndex, Node) NodeInfo
        {
            get => nodeInfo;
            set
            {
                nodeInfo = value;
                Render();
            }
        }

        NodeIndex Index => nodeInfo.index;
        Node Node => nodeInfo.value;

        static Dictionary<NodeValueType, GameObject> _fieldPrefabs;
        public static Dictionary<NodeValueType, GameObject> FieldPrefabs
        {
            get
            {
                if (_fieldPrefabs is null)
                    InitFieldPrefabs();
                return _fieldPrefabs;
            }
        }

        static void InitFieldPrefabs()
        {
            const string basePath = "NodeUI/ValueEditors/";
            _fieldPrefabs = new()
            {
                [NodeValueType.Int] = Resources.Load<GameObject>(basePath + "Integer Editor"),
                [NodeValueType.Float] = Resources.Load<GameObject>(basePath + "Float Editor"),
                [NodeValueType.Vector2] = Resources.Load<GameObject>(basePath + "Vector2 Editor"),
                [NodeValueType.Vector3] = Resources.Load<GameObject>(basePath + "Vector3 Editor"),
                [NodeValueType.String] = Resources.Load<GameObject>(basePath + "String Editor"),
                [NodeValueType.GameObject] = Resources.Load<GameObject>(basePath + "GameObject Editor"),
                [NodeValueType.Any] = Resources.Load<GameObject>(basePath + "Any Editor"),
                [NodeValueType.Bool] = Resources.Load<GameObject>(basePath + "Bool Editor"),
            };
        }

        [SerializeField]
        GameObject inputPortPrefab;
        [SerializeField]
        GameObject outputPortPrefab;
        [SerializeField]
        GameObject executionInputPrefab;
        [SerializeField]
        GameObject executionOutputPrefab;

        [SerializeField]
        TextMeshProUGUI title;
        [SerializeField]
        Transform fields;
        [SerializeField]
        Transform inputPorts;
        [SerializeField]
        Transform outputPorts;

        [NonSerialized]
        public List<InputPortView> inputPortViews = new();
        [NonSerialized]
        public List<OutputPortView> outputPortViews = new();
        [NonSerialized]
        public InputExecutionPort inputExecutionPort;
        [NonSerialized]
        public List<OutputExecutionPort> outputExecutionPorts = new();

        public void Move()
        {
            RectTransform rect = (RectTransform)transform;
            Vector2 pos = rect.localPosition;
            GraphView view = GetComponentInParent<GraphView>();
            RealityFlowAPI.Instance.SetNodePosition(view.Graph, Index, pos);
        }

        void Render()
        {
            ClearChildren(fields);
            ClearChildren(inputPorts);
            ClearChildren(outputPorts);

            inputPortViews.Clear();
            outputPortViews.Clear();
            inputExecutionPort = null;
            outputExecutionPorts.Clear();

            title.text = Node.Definition.Name;
            transform.localPosition = new(
                Node.Position.x,
                Node.Position.y,
                transform.localPosition.z
            );

            for (int i = 0; i < Node.Definition.Fields.Count; i++)
            {
                NodeFieldDefinition def = Node.Definition.Fields[i];
                GameObject field = Instantiate(FieldPrefabs[def.Default.Type], fields);

                IValueEditor editor = field.GetComponent<IValueEditor>();
                editor.NodeValue = def.Default;
                editor.OnTick += value =>
                {
                    Assert.IsTrue(Node.TrySetField(i, value));
                };
            }

            if (Node.Definition.ExecutionInput)
            {
                GameObject port = Instantiate(executionInputPrefab, inputPorts);
                InputExecutionPort portScript = port.GetComponent<InputExecutionPort>();
                portScript.node = Index;

                inputExecutionPort = portScript;
            }

            for (int i = 0; i < Node.Definition.ExecutionOutputs.Count; i++)
            {
                string name = Node.Definition.ExecutionOutputs[i];
                GameObject port = Instantiate(executionOutputPrefab, outputPorts);
                OutputExecutionPort portScript = port.GetComponent<OutputExecutionPort>();
                portScript.DisplayName.text = name;
                portScript.port = new(Index, i);

                outputExecutionPorts.Add(portScript);
            }

            for (int i = 0;  i < Node.Definition.Inputs.Count;  i++)
            {
                var def = Node.Definition.Inputs[i];
                GameObject port = Instantiate(inputPortPrefab, inputPorts);

                InputPortView view = port.GetComponent<InputPortView>();
                view.Definition = def;
                view.port = new(Index, i);

                inputPortViews.Add(view);
            }

            for (int i = 0; i < Node.Definition.Outputs.Count; i++)
            {
                var def = Node.Definition.Outputs[i];
                GameObject port = Instantiate(outputPortPrefab, outputPorts);

                OutputPortView view = port.GetComponent<OutputPortView>();
                view.Definition = def;
                view.port = new(Index, i);

                outputPortViews.Add(view);
            }
        }

        void ClearChildren(Transform tf)
        {
            foreach (Transform child in tf)
                Destroy(child.gameObject);
        }
    }
}
