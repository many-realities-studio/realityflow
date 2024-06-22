using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class Vector2Editor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        FloatEditor xInput;
        [SerializeField]
        FloatEditor yInput;

        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        Vector2 value;
        public Vector2 Value
        {
            get => value;
            set
            {
                xInput.Value = value.x;
                yInput.Value = value.y;
            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (NodeValue.TryGetValue(value, out Vector2 val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to Vector2Editor");
            }
        }

        public void Tick()
        {
            xInput.Tick();
            yInput.Tick();

            value = new(xInput.Value, yInput.Value);

            OnTick(new Vector2Value(Value));
        }
    }
}
