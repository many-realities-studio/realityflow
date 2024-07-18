using System;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class GameObjectEditor : MonoBehaviour, IValueEditor
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