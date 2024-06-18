using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.Collections;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class VariableEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        TMP_Dropdown dropdown;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        Whiteboard whiteboard;
        readonly List<string> variables = new();

        void OnEnable()
        {
            whiteboard = GetComponentInParent<Whiteboard>();
        }

        public string Value
        {
            get
            {
                if (dropdown.@value.In(0..variables.Count))
                    return variables[dropdown.@value];
                else
                {
                    dropdown.@value = -1;
                    return null;
                }
            }
            set
            {
                variables.Clear();
                variables.AddRange(whiteboard.TopLevelGraphView.Graph.Variables.Keys);
                dropdown.ClearOptions();
                dropdown.AddOptions(variables);
                dropdown.@value = variables.IndexOf(value);
            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (value.TryGetValue(out string val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to VariableEditor");
            }
        }

        public void Tick()
        {
            if (Value == null)
                return;
            OnTick(NodeValue.Variable(Value));
        }
    }
}