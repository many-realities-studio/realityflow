using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class OutputExecutionPort : MonoBehaviour
    {
        [SerializeField]
        TMP_Text displayName;
        public TMP_Text DisplayName => displayName;

        [NonSerialized]
        public PortIndex port;

        public Transform edgeTarget;

        public void Select()
        {
            GraphView view = GetComponentInParent<GraphView>();
            view.SelectOutputExecutionPort(port);
        }
    }
}