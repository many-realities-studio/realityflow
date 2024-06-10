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
                graph ??= new();
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
            ClearSelectedEdgeEnds();

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

                    view.target1 = fromPortView.edgeTarget;
                    view.target2 = toPortView.edgeTarget;
                }
        }

        void ClearSelectedEdgeEnds()
        {
            selectedInputEdgePort = null;
            selectedOutputEdgePort = null;
            selectedInputExecEdgePort = null;
            selectedOutputExecEdgePort = null;

            EnableAllPorts();
        }

        void EnableAllPorts()
        {
            foreach ((NodeIndex index, NodeView view) in nodeUis)
            {
                // foreach (var port in view.inputPortViews)
                //     port.GetComponent<PressableButton>().enabled = true;
                // foreach (var port in view.outputPortViews)
                //     port.GetComponent<PressableButton>().enabled = true;
                if (view.inputExecutionPort)
                    view.inputExecutionPort.GetComponent<PressableButton>().enabled = true;
                foreach (var p in view.outputExecutionPorts)
                    p.GetComponent<PressableButton>().enabled = true;
            }
        }

        public void SelectInputPort(PortIndex port)
        {
            // TODO:
            // selectedInputEdgePort = port;
        }

        public void SelectOutputPort(PortIndex port)
        {

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
            foreach ((NodeIndex index, NodeView view) in nodeUis)
            {
                // foreach (var p in view.inputPortViews)
                //     p.GetComponent<PressableButton>().enabled = false;
                // foreach (var p in view.outputPortViews)
                //     p.GetComponent<PressableButton>().enabled = false;
                if (view.inputExecutionPort)
                    view.inputExecutionPort.GetComponent<PressableButton>().enabled = false;
            }
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
            foreach ((NodeIndex index, NodeView view) in nodeUis)
            {
                // foreach (var p in view.inputPortViews)
                //     p.GetComponent<PressableButton>().enabled = false;
                // foreach (var p in view.outputPortViews)
                //     p.GetComponent<PressableButton>().enabled = false;
                foreach (var p in view.outputExecutionPorts)
                    p.GetComponent<PressableButton>().enabled = false;
            }
        }

        public void ExecEdgeConnected(PortIndex from, NodeIndex to)
        {
            RealityFlowAPI.Instance.AddExecEdgeToGraph(graph, from, to);
            ClearSelectedEdgeEnds();
            MarkDirty();
        }
    }
}