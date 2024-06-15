using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RealityFlow.NodeUI
{
    public class NodeWhiteboard : MonoBehaviour
    {
        [SerializeField]
        GraphView topLevelGraphView;

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
    }
}