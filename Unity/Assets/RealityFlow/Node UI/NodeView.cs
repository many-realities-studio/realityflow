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
        static Dictionary<NodeValueType, GameObject> FieldPrefabs
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
            };
        }

        static Dictionary<NodeValueType, GameObject> _inputPortPrefabs;
        static Dictionary<NodeValueType, GameObject> InputPortPrefabs
        {
            get
            {
                if (_inputPortPrefabs is null)
                    InitInputPortPrefabs();
                return _inputPortPrefabs;
            }
        }

        static void InitInputPortPrefabs()
        {
            const string basePath = "NodeUI/InputPorts/";
            _inputPortPrefabs = new()
            {
                [NodeValueType.Int] = Resources.Load<GameObject>(basePath + "Integer Port"),
                [NodeValueType.Float] = Resources.Load<GameObject>(basePath + "Float Port"),
                [NodeValueType.Vector2] = Resources.Load<GameObject>(basePath + "Vector2 Port"),
                [NodeValueType.Vector3] = Resources.Load<GameObject>(basePath + "Vector3 Port"),
                [NodeValueType.String] = Resources.Load<GameObject>(basePath + "String Port"),
                [NodeValueType.GameObject] = Resources.Load<GameObject>(basePath + "GameObject Port"),
                [NodeValueType.Any] = Resources.Load<GameObject>(basePath + "Any Port"),
            };
        }

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

            foreach (var def in Node.Definition.Inputs)
            {
                GameObject port = Instantiate(InputPortPrefabs[def.Type], inputPorts);

                InputPortView view = port.GetComponent<InputPortView>();
                view.Definition = def;

                inputPortViews.Add(view);
            }

            foreach (var def in Node.Definition.Outputs)
            {
                GameObject port = Instantiate(outputPortPrefab, outputPorts);

                OutputPortView view = port.GetComponent<OutputPortView>();
                view.Definition = def;

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
