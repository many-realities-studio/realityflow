using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
            if (Instance && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            RealityFlowAPI.Instance.OnLeaveRoom += () => gameObject.SetActive(false);
        }

        public void Show()
        {
            if (DoNotShow)
                return;

            gameObject.SetActive(true);
            // TODO: Probably use API for this later
            Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            transform.position = Camera.main.transform.position + camForward;
            transform.forward = camForward;
        }

        public void SetAttachedObj(VisualScript obj)
        {
            if (RealityFlowAPI.Instance.SpawnedObjects[obj.gameObject].graphId == null)
            {
                obj.graph = RealityFlowAPI.Instance.CreateNodeGraphAsync();
                RealityFlowAPI.Instance.AssignGraph(obj.graph, obj.gameObject);
            }
            topLevelGraphView.CurrentObject = obj;
            topLevelGraphView.Graph = obj.graph;

            WhiteboardIndicatorLine line = topLevelGraphView.GetComponent<WhiteboardIndicatorLine>();
            if (line)
                line.target = obj.transform;
        }
    }
}