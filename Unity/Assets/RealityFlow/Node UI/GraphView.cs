using System.Linq;
using RealityFlow.NodeGraph;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    class GraphView : MonoBehaviour
    {
        Graph graph;
        public Graph Graph
        {
            get => graph;
            set
            {
                graph = value;
                Render();
            }
        }

        public GameObject nodeUIPrefab;

        void Start()
        {
            graph ??= new();
        }

        void Render()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (Node node in graph.Nodes)
            {
                GameObject nodeUi = Instantiate(nodeUIPrefab, transform);
                nodeUi.GetComponent<NodeView>().Node = node;
            }
        }
    }
}