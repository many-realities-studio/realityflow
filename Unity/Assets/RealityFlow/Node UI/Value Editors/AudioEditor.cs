using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RealityFlow.Collections;
using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RealityFlow.NodeUI
{
    public class AudioEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        TMP_Dropdown dropdown;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        readonly List<string> noneList = new[] { "None" }.ToList();

        void Awake()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(noneList);
            dropdown.AddOptions(RealityFlowAPI.Instance.AudioClipNames);
        }

        public string Value
        {
            get
            {
                if (dropdown.@value == 0)
                    return null;

                if ((dropdown.@value - 1).In(0..RealityFlowAPI.Instance.AudioClipNames.Count))
                {
                    string clip = RealityFlowAPI.Instance.AudioClipNames[dropdown.@value - 1];
                    return clip;
                }
                else
                {
                    Debug.LogError("Out of bounds dropdown value for audio");
                    return null;
                }
            }
            set
            {
                if (value == null)
                    dropdown.SetValueWithoutNotify(0);
                else if (RealityFlowAPI.Instance.AudioClipNames.IndexOf(value) is int index && index != -1)
                    dropdown.SetValueWithoutNotify(index + 1);
                else
                    Debug.LogError($"Failed to get RfObject for audio {value}");
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
                    Debug.LogError("incorrect value type assigned to Audio Editor");
            }
        }

        public void Tick()
        {
            OnTick(new AudioValue(Value));
        }
    }
}