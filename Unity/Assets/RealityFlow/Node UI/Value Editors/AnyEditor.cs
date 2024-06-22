using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class AnyEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        public object Value
        {
            get => null;
            set { }
        }

        public NodeValue NodeValue { set { } }

        public void Tick()
        {
            OnTick(null);
        }
    }
}