using System.Collections;
using System.Collections.Generic;
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
        }

        public void ShowForObject(VisualScript obj)
        {
            gameObject.SetActive(true);
            // TODO: Probably use API for this later
            transform.position = obj.transform.position + Vector3.up * 0.5f;
            topLevelGraphView.Graph = obj.graph;
        }
    }
}