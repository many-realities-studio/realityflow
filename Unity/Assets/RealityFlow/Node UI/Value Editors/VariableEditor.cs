using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                if (dropdown.@value == 0)
                    return null;

                if ((dropdown.@value - 1).In(0..variables.Count))
                    return variables[dropdown.@value - 1];
                else
                    return null;
            }
            set
            {
                variables.Clear();
                variables.AddRange(
                    whiteboard.TopLevelGraphView.Graph.Variables
                    .Where(kv => kv.Value == NodeValueType.Int)
                    .Select(kv => kv.Key)
                );
                dropdown.ClearOptions();
                dropdown.AddOptions(new[] { "None" }.ToList());
                dropdown.AddOptions(variables);
                if (value == null)
                    dropdown.SetValueWithoutNotify(0);
                else
                    dropdown.SetValueWithoutNotify(variables.IndexOf(value) + 1);
            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (value == null)
                    Value = null;
                else if (NodeValue.TryGetValue(value, out string val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to VariableEditor");
            }
        }

        public void Tick()
        {
            OnTick(new VariableValue(Value));
        }
    }
}