using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class InputExecutionPort : MonoBehaviour
    {
        [NonSerialized]
        public NodeIndex node;

        public Transform edgeTarget;

        public void Select()
        {
            GraphView view = GetComponentInParent<GraphView>();
            view.SelectInputExecutionPort(node);
        }
    }
}