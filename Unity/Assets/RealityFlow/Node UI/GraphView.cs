using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using RealityFlow.NodeGraph;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class GraphView : MonoBehaviour
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
                Render();
            }
        }

        public GameObject nodeUIPrefab;
        public GameObject edgeUIPrefab;

        bool dirty;
        Dictionary<NodeIndex, NodeView> nodeUis = new();
        Dictionary<(PortIndex, PortIndex), EdgeView> dataEdgeUis = new();
        Dictionary<(PortIndex, NodeIndex), EdgeView> execEdgeUis = new();

        public PortIndex? selectedInputEdgePort;
        public PortIndex? selectedOutputEdgePort;
        public NodeIndex? selectedInputExecEdgePort;
        public PortIndex? selectedOutputExecEdgePort;

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

        public void Render()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            nodeUis.Clear();
            dataEdgeUis.Clear();
            execEdgeUis.Clear();

            foreach ((NodeIndex index, Node node) in graph.Nodes)
            {
                GameObject nodeUi = Instantiate(nodeUIPrefab, transform);
                NodeView view = nodeUi.GetComponent<NodeView>();
                view.NodeInfo = (index, node);

                nodeUis.Add(index, view);
            }

            foreach ((PortIndex from, PortIndex to) in graph.Edges)
            {
                GameObject edgeUi = Instantiate(edgeUIPrefab, transform);
                EdgeView view = edgeUi.GetComponent<EdgeView>();

                NodeView fromView = nodeUis[from.Node];
                NodeView toView = nodeUis[to.Node];

                OutputPortView fromPortView = fromView.outputPortViews[from.Port];
                InputPortView toPortView = toView.inputPortViews[to.Port];

                view.from = from;
                view.toNode = to.Node;
                view.toPort = to.Port;
                view.target1 = fromPortView.edgeTarget;
                view.target2 = toPortView.edgeTarget;

                dataEdgeUis.Add((from, to), view);
            }

            foreach ((PortIndex from, List<NodeIndex> targets) in graph.ExecutionEdges)
                foreach (NodeIndex to in targets)
                {
                    GameObject edgeUi = Instantiate(edgeUIPrefab, transform);
                    EdgeView view = edgeUi.GetComponent<EdgeView>();

                    NodeView fromView = nodeUis[from.Node];
                    NodeView toView = nodeUis[to];

                    OutputExecutionPort fromPortView = fromView.outputExecutionPorts[from.Port];
                    InputExecutionPort toPortView = toView.inputExecutionPort;

                    view.from = from;
                    view.toNode = to;
                    view.target1 = fromPortView.edgeTarget;
                    view.target2 = toPortView.edgeTarget;

                    execEdgeUis.Add((from, to), view);
                }

            SetPortsActive();
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
                foreach (var port in view.inputPortViews)
                    port.GetComponent<PressableButton>().enabled =
                        noneSelected ||
                        (
                            selectedOutputEdgePort is PortIndex from
                            && graph.PortsCompatible(from, port.port)
                            && !graph.EdgeWouldFormCycle(from.Node, port.port.Node)
                            && !graph.EdgeExists(from, port.port)
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
    }
}