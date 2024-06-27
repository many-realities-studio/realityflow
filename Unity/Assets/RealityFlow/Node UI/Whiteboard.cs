using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealityFlow.NodeGraph;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class Whiteboard : MonoBehaviour
    {
        public static Whiteboard Instance { get; private set; }

        [SerializeField]
        private GraphView topLevelGraphView;

        public GraphView TopLevelGraphView => topLevelGraphView;

        public bool DoNotShow { get; set; }

        GraphView selectedGraphView;
        public GraphView SelectedGraphView
        {
            get => selectedGraphView;
            set => selectedGraphView = value;
        }

        void Start()
        {
            selectedGraphView = topLevelGraphView;
        }

        public void Init()
        {
            if (Instance)
                Destroy(gameObject);
            else
                Instance = this;

            RealityFlowAPI.Instance.OnLeaveRoom += () => gameObject.SetActive(false);
        }

        public void ShowForObject(VisualScript obj)
        {
            if (DoNotShow)
                return;

            gameObject.SetActive(true);
            // TODO: Probably use API for this later
            transform.position = obj.transform.position + Vector3.up * 1.0f;
            if (RealityFlowAPI.Instance.SpawnedObjects[obj.gameObject].graphId == null)
            {
                obj.graph = RealityFlowAPI.Instance.CreateNodeGraphAsync();
                RealityFlowAPI.Instance.AssignGraph(obj.graph, obj.gameObject);
            }
            topLevelGraphView.CurrentObject = obj;
            topLevelGraphView.Graph = obj.graph;
        }
    }
}