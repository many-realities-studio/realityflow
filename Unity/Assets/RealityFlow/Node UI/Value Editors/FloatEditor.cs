using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class FloatEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        Custom_InputField input;
        [SerializeField]
        TMP_Text title;

        public Action<NodeValue> OnTick { get; set; }

        public float Value
        {
            get => float.TryParse(input.text, out float val) ? val : default;
            set => input.text = value.ToString();
        }

        public NodeValue NodeValue
        {
            set
            {
                if (value.TryGetValue(out float val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to FloatEditor");
            }
        }

        public string Name
        {
            get => title.text;
            set => title.text = value;
        }

        public void Tick()
        {
            if (!float.TryParse(input.text, out _))
                input.text = "0";

            OnTick(new(Value));
        }
    }
}