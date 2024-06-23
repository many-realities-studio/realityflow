using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealityFlow.NodeUI
{
    public class GraphView : MonoBehaviour, IPointerDownHandler
    {
        Graph graph;
        public Graph Graph
        {
            get
            {
                if (graph is null)
                    Debug.LogError("Attempted to access Graph of GraphView before initialization");
                return graph;
            }
            set
            {
                graph = value;
                EnableVariableButtons();
                AddInitialVariables();
                templateToggle.ForceSetToggled(currentObject.isTemplate);
                Render();
            }
        }

        public GameObject nodeUIPrefab;
        public GameObject edgeUIPrefab;

        bool dirty;
        VisualScript currentObject;
        public VisualScript CurrentObject { get => currentObject; set => currentObject = value; }
        string selectedVariable;
        Dictionary<NodeIndex, NodeView> nodeUis = new();
        Dictionary<(PortIndex, PortIndex), EdgeView> dataEdgeUis = new();
        Dictionary<(PortIndex, NodeIndex), EdgeView> execEdgeUis = new();

        [SerializeField]
        private Custom_MRTK_InputField variableNameField;
        [SerializeField]
        private GameObject variableItemPrefab;
        [SerializeField]
        private Transform variableContent;
        [SerializeField]
        private PressableButton addVariableButton;
        [SerializeField]
        private TMP_Dropdown variableTypeDropdown;
        [SerializeField]
        private PressableButton removeVariableButton;
        [SerializeField]
        private PressableButton templateToggle;

        public PortIndex? selectedInputEdgePort;
        public PortIndex? selectedOutputEdgePort;
        public NodeIndex? selectedInputExecEdgePort;
        public PortIndex? selectedOutputExecEdgePort;

        void Start()
        {
            variableTypeDropdown.ClearOptions();
            variableTypeDropdown.AddOptions(
                NodeValue.valueTypes.Select(type => type.ToString()).ToList()
            );
        }

        void Update()
        {
            if (dirty)
            {
                Render();
                dirty = false;
            }
        }

        public void MarkDirty()
        {
            dirty = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ClearSelectedEdgeEnds();
            SetPortsActive();
        }

        public void Render()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            nodeUis.Clear();
            dataEdgeUis.Clear();
            execEdgeUis.Clear();

            foreach ((NodeIndex index, Node node) in graph.Nodes)
            {
                if (node.Definition == null)
                    RealityFlowAPI.Instance.RemoveNodeFromGraph(graph, index);

                GameObject nodeUi = Instantiate(nodeUIPrefab, transform);
                NodeView view = nodeUi.GetComponent<NodeView>();

                view.NodeInfo = (index, node);

                nodeUis.Add(index, view);
            }

            foreach ((PortIndex from, PortIndex to) in graph.Edges)
            {
                if (!nodeUis.TryGetValue(from.Node, out NodeView fromView))
                    continue;
                if (!nodeUis.TryGetValue(to.Node, out NodeView toView))
                    continue;

                GameObject edgeUi = Instantiate(edgeUIPrefab, transform);
                EdgeView view = edgeUi.GetComponent<EdgeView>();

                OutputPortView fromPortView = fromView.outputPortViews[from.Port];
                InputPortView toPortView = toView.inputPortViews[to.Port];

                view.from = from;
                view.toNode = to.Node;
                view.toPort = to.Port;
                view.target1 = fromPortView.edgeTarget;
                view.target2 = toPortView.edgeTarget;

                dataEdgeUis.Add((from, to), view);
            }

            foreach ((PortIndex from, ImmutableList<NodeIndex> targets) in graph.ExecutionEdges)
                foreach (NodeIndex to in targets)
                {
                    if (!nodeUis.TryGetValue(from.Node, out NodeView fromView))
                        continue;
                    if (!nodeUis.TryGetValue(to, out NodeView toView))
                        continue;

                    GameObject edgeUi = Instantiate(edgeUIPrefab, transform);
                    EdgeView view = edgeUi.GetComponent<EdgeView>();

                    OutputExecutionPort fromPortView = fromView.outputExecutionPorts[from.Port];
                    InputExecutionPort toPortView = toView.inputExecutionPort;

                    view.from = from;
                    view.toNode = to;
                    view.target1 = fromPortView.edgeTarget;
                    view.target2 = toPortView.edgeTarget;

                    execEdgeUis.Add((from, to), view);
                }

            SetPortsActive();

            EnableVariableButtons();
        }

        void ClearSelectedEdgeEnds()
        {
            selectedInputEdgePort = null;
            selectedOutputEdgePort = null;
            selectedInputExecEdgePort = null;
            selectedOutputExecEdgePort = null;
        }

        /// <summary>
        /// Reasons a port will be disabled:
        /// <list>
        /// <item>
        /// The selected port isn't also data/execution
        /// </item>
        /// <item>
        /// Side mismatch (in/out)
        /// </item>
        /// <item>
        /// Data type mismatch
        /// </item>
        /// <item>
        /// The combined port edge already exists
        /// </item>
        /// <item>
        /// The edge would form a cycle
        /// </item>
        /// </list>
        /// </summary>
        void SetPortsActive()
        {
            if (selectedInputEdgePort is PortIndex selectedInData && !graph.ContainsNode(selectedInData.Node))
                selectedInputEdgePort = null;
            if (selectedOutputEdgePort is PortIndex selectedOutData && !graph.ContainsNode(selectedOutData.Node))
                selectedOutputEdgePort = null;
            if (selectedInputExecEdgePort is NodeIndex selectedInExec && !graph.ContainsNode(selectedInExec))
                selectedInputExecEdgePort = null;
            if (selectedOutputExecEdgePort is PortIndex selectedOutExec && !graph.ContainsNode(selectedOutExec.Node))
                selectedOutputExecEdgePort = null;

            bool noneSelected =
                selectedInputEdgePort == null &&
                selectedOutputEdgePort == null &&
                selectedInputExecEdgePort == null &&
                selectedOutputExecEdgePort == null;
            foreach ((NodeIndex index, NodeView view) in nodeUis)
            {
                foreach (InputPortView port in view.inputPortViews)
                    port.GetComponent<PressableButton>().enabled =
                        noneSelected ||
                        (
                            selectedOutputEdgePort is PortIndex from
                            && graph.PortsCompatible(from, port.port)
                            && !graph.TryGetOutputPortOf(port.port, out _)
                            && !graph.EdgeExists(from, port.port)
                            && !graph.EdgeWouldFormCycle(from.Node, port.port.Node)
                        );

                foreach (var port in view.outputPortViews)
                    port.GetComponent<PressableButton>().enabled =
                        noneSelected ||
                        (
                            selectedInputEdgePort is PortIndex to
                            && graph.PortsCompatible(port.port, to)
                            && !graph.EdgeWouldFormCycle(port.port.Node, to.Node)
                            && !graph.EdgeExists(port.port, to)
                        );

                if (view.inputExecutionPort)
                    view.inputExecutionPort.GetComponent<PressableButton>().enabled =
                        noneSelected ||
                        (
                            selectedOutputExecEdgePort is PortIndex from
                            && !graph.EdgeWouldFormCycle(from.Node, index)
                            && !graph.ExecEdgeExists(from, index)
                        );

                foreach (var port in view.outputExecutionPorts)
                    port.GetComponent<PressableButton>().enabled =
                        noneSelected ||
                        (
                            selectedInputExecEdgePort is NodeIndex node
                            && !graph.EdgeWouldFormCycle(port.port.Node, node)
                            && !graph.ExecEdgeExists(port.port, node)
                        );
            }
        }

        public void SelectInputPort(PortIndex port)
        {
            if (selectedOutputEdgePort is PortIndex otherPort)
            {
                DataEdgeConnected(otherPort, port);
                return;
            }

            ClearSelectedEdgeEnds();

            selectedInputEdgePort = port;

            SetPortsActive();
        }

        public void SelectOutputPort(PortIndex port)
        {
            if (selectedInputEdgePort is PortIndex otherPort)
            {
                DataEdgeConnected(port, otherPort);
                return;
            }

            ClearSelectedEdgeEnds();

            selectedOutputEdgePort = port;

            SetPortsActive();
        }

        public void SelectInputExecutionPort(NodeIndex node)
        {
            if (selectedOutputExecEdgePort is PortIndex port)
            {
                ExecEdgeConnected(port, node);
                return;
            }

            ClearSelectedEdgeEnds();

            selectedInputExecEdgePort = node;

            SetPortsActive();
        }

        public void SelectOutputExecutionPort(PortIndex port)
        {
            if (selectedInputExecEdgePort is NodeIndex node)
            {
                ExecEdgeConnected(port, node);
                return;
            }

            ClearSelectedEdgeEnds();

            selectedOutputExecEdgePort = port;

            SetPortsActive();
        }

        public void DataEdgeConnected(PortIndex from, PortIndex to)
        {
            RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, from, to);
            ClearSelectedEdgeEnds();
            SetPortsActive();
            MarkDirty();
        }

        public void ExecEdgeConnected(PortIndex from, NodeIndex to)
        {
            RealityFlowAPI.Instance.AddExecEdgeToGraph(graph, from, to);
            ClearSelectedEdgeEnds();
            SetPortsActive();
            MarkDirty();
        }

        public void SetTemplate(bool isTemplate)
        {
            if (!CurrentObject)
                return;

            CurrentObject.isTemplate = isTemplate;
        }

        public void SetSelectedVariable(string variable)
        {
            selectedVariable = variable;

            EnableVariableButtons();
        }

        public void VariableNameChanged(string name)
        {
            EnableVariableButtons();
        }

        void EnableVariableButtons()
        {
            addVariableButton.enabled =
                !string.IsNullOrEmpty(variableNameField.text)
                && !Graph.TryGetVariableType(variableNameField.text, out _);

            removeVariableButton.enabled =
                selectedVariable != null
                && Graph.TryGetVariableType(selectedVariable, out _);
        }

        void AddVariableItem(string name, NodeValueType type)
        {
            GameObject item = Instantiate(variableItemPrefab, variableContent);
            VariableItem varItem = item.GetComponent<VariableItem>();
            varItem.title.text = string.Format(varItem.title.text, name, type.ToString());
            varItem.varName = name;
            varItem.type = type;
            varItem.view = this;
        }

        void ClearVariableItems()
        {
            foreach (Transform transform in variableContent)
                Destroy(transform.gameObject);
        }

        public void AddInitialVariables()
        {
            ClearVariableItems();

            foreach ((string name, NodeValueType type) in Graph.Variables)
                AddVariableItem(name, type);

            MarkDirty();
        }

        public void AddVariable()
        {
            string name = variableNameField.text;
            if (string.IsNullOrEmpty(name))
                return;

            if (Graph.TryGetVariableType(name, out _))
                return;

            NodeValueType type = NodeValue.valueTypes[variableTypeDropdown.value];

            RealityFlowAPI.Instance.AddVariableToGraph(Graph, name, type);

            AddVariableItem(name, type);

            MarkDirty();
        }

        public void RemoveVariable()
        {
            if (selectedVariable == null)
                return;

            foreach (Transform trans in variableContent)
                if (trans.GetComponent<VariableItem>().varName == selectedVariable)
                    Destroy(trans.gameObject);

            if (!Graph.TryGetVariableType(selectedVariable, out _))
                return;

            RealityFlowAPI.Instance.RemoveVariableFromGraph(Graph, selectedVariable);

            selectedVariable = null;

            MarkDirty();
        }

        public void AddGetVariableNode(string varName, NodeValueType type)
        {
            string name = $"Get{type}Variable";
            NodeDefinition def = RealityFlowAPI.Instance.NodeDefinitionDict[name];
            NodeIndex node = RealityFlowAPI.Instance.AddNodeToGraph(Graph, def);
            RealityFlowAPI.Instance.SetNodeFieldValue(Graph, node, 0, new VariableValue(varName));
            MarkDirty();
        }

        public void AddSetVariableNode(string varName, NodeValueType type)
        {
            string name = $"Set{type}Variable";
            NodeDefinition def = RealityFlowAPI.Instance.NodeDefinitionDict[name];
            NodeIndex node = RealityFlowAPI.Instance.AddNodeToGraph(Graph, def);
            RealityFlowAPI.Instance.SetNodeFieldValue(Graph, node, 0, new VariableValue(varName));
            MarkDirty();
        }
    }
}