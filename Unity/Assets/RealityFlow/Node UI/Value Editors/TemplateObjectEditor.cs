using System;
using System.Collections;
using System.Collections.Generic;
using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RealityFlow.NodeUI
{
    public class TemplateObjectEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        TMP_Dropdown dropdown;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        readonly List<GameObject> templates = new();

        GameObject value;
        public GameObject Value
        {
            get => value;
            set
            {

            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (NodeValue.TryGetValue(value, out GameObject val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to Vector3Editor");
            }
        }

        public void Tick()
        {
            OnTick(new TemplateObjectValue(value));
        }
    }
}