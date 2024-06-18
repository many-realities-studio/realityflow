using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class Vector3Editor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        FloatEditor xInput;
        [SerializeField]
        FloatEditor yInput;
        [SerializeField]
        FloatEditor zInput;

        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        Vector3 value;
        public Vector3 Value
        {
            get => value;
            set
            {
                xInput.Value = value.x;
                yInput.Value = value.y;
                zInput.Value = value.z;
            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (value.TryGetValue(out Vector3 val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to Vector3Editor");
            }
        }

        public void Tick()
        {
            xInput.Tick();
            yInput.Tick();
            zInput.Tick();

            value = new(xInput.Value, yInput.Value, zInput.Value);

            OnTick(new(value));
        }
    }
}