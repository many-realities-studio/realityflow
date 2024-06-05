using System.Collections.Generic;
using System.Linq;
using RealityFlow.NodeGraph;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Data;

namespace RealityFlow.NodeUI
{
    class GraphView : DataSourceGOBase
    {
        Graph graph;
        public Graph Graph
        {
            get => graph;
            set
            {
                graph = value;
                if (source != null)
                {
                    source.Graph = graph;
                    source.NotifyAllChanged();
                }
                Render();
            }
        }

        GraphViewSource source;

        public GameObject nodeUIPrefab;
        public GameObject edgeUIPrefab;

        Dictionary<NodeIndex, GameObject> nodeUis = new();
        Dictionary<(PortIndex, PortIndex), GameObject> dataEdgeUis = new();
        Dictionary<(PortIndex, NodeIndex), GameObject> execEdgeUis = new();

        public override IDataSource AllocateDataSource()
        {
            return new GraphViewSource { Graph = graph };
        }

        void Render()
        {
            // foreach (Transform child in transform)
            //     Destroy(child.gameObject);

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

                // Node 
            }
        }
    }
}