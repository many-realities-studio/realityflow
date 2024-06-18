using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class IntegerEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        Custom_InputField input;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        public int Value
        {
            get => int.TryParse(input.text, out int val) ? val : default;
            set => input.text = value.ToString();
        }

        public NodeValue NodeValue
        {
            set
            {
                if (value.TryGetValue(out int val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to IntEditor");
            }
        }

        public void Tick()
        {
            if (!int.TryParse(input.text, out _))
                input.text = "0";

            if (OnTick != null)
                OnTick(new(Value));
        }
    }
}