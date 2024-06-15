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

        void Start()
        {
            if (Instance)
                Destroy(gameObject);
            else
                Instance = this;
        }

        public void ShowForObject(VisualScript obj)
        {
            gameObject.SetActive(true);
            topLevelGraphView.Graph = obj.graph;
        }
    }
}