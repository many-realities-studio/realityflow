using System;
using System.Collections;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class StringEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        Custom_InputField input;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        public string Value
        {
            get => input.text;
            set => input.SetTextWithoutNotify(value);
        }

        public NodeValue NodeValue
        {
            set
            {
                if (NodeValue.TryGetValue(value, out string val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to StringEditor");
            }
        }
        
        public void Tick()
        {
            OnTick?.Invoke(new StringValue(Value));
        }
    }
}