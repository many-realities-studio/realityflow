using System.Collections.Generic;
using System.Linq;
using RealityFlow.NodeGraph;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Data;

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
        Dictionary<NodeIndex, GameObject> nodeUis = new();
        Dictionary<(PortIndex, PortIndex), GameObject> dataEdgeUis = new();
        Dictionary<(PortIndex, NodeIndex), GameObject> execEdgeUis = new();

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

            foreach (KeyValuePair<NodeIndex, Node> kv in graph.Nodes)
            {
                NodeIndex key = kv.Key;
                Node node = kv.Value;
                GameObject nodeUi = Instantiate(nodeUIPrefab, transform);
                nodeUi.GetComponent<NodeView>().Node = node;

                nodeUis.Add(key, nodeUi);
            }

            foreach (KeyValuePair<PortIndex, PortIndex> edge in graph.Edges)
            {
                GameObject edgeUi = Instantiate(edgeUIPrefab, transform);
                EdgeView view = edgeUi.GetComponent<EdgeView>();

                dataEdgeUis.Add((edge.Key, edge.Value), edgeUi);
            }
        }
    }
}