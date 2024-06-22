using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.UX;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class BoolEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        PressableButton button;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        public bool Value
        {
            get => button.IsToggled;
            set
            {
                button.ForceSetToggled(value);
            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (NodeValue.TryGetValue(value, out bool val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to BoolEditor");
            }
        }

        public void Tick()
        {
            OnTick(new BoolValue(Value));
        }
    }
}