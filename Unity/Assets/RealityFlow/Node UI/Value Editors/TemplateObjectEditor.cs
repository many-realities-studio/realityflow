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
    public class TemplateObjectEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        TMP_Dropdown dropdown;
        [SerializeField]
        TMP_Text title;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        readonly List<string> dropdownIds = new();
        readonly List<string> noneList = new[] { "None" }.ToList();

        void Awake()
        {
            RealityFlowAPI.Instance.OnTemplatesChanged += UpdateList;
        }

        void OnDestroy()
        {
            RealityFlowAPI.Instance.OnTemplatesChanged -= UpdateList;
        }

        public GameObject Value
        {
            get
            {
                if (dropdown.@value == 0)
                    return null;

                if ((dropdown.@value - 1).In(0..dropdownIds.Count))
                {
                    string id = dropdownIds[dropdown.@value - 1];
                    if (RealityFlowAPI.Instance.SpawnedObjectsById.TryGetValue(id, out GameObject obj))
                        return obj;
                    else
                        Debug.LogError($"Failed to get object for template id {id}");

                    return null;
                }
                else
                    return null;
            }
            set
            {
                UpdateList();

                if (value == null)
                    dropdown.SetValueWithoutNotify(0);
                else if (RealityFlowAPI.Instance.SpawnedObjects.TryGetValue(value, out RfObject obj))
                    dropdown.SetValueWithoutNotify(dropdownIds.IndexOf(obj.id) + 1);
                else
                    Debug.LogError($"Failed to get RfObject for template {value.name}");
            }
        }

        public NodeValue NodeValue
        {
            set
            {
                if (value == null)
                    Value = null;
                else if (NodeValue.TryGetValue(value, out GameObject val))
                    Value = val;
                else
                    Debug.LogError("incorrect value type assigned to TemplateObject Editor");
            }
        }

        public void Tick()
        {
            OnTick(new TemplateObjectValue(Value));
        }

        public void UpdateList()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(noneList);
            dropdown.AddOptions(
                RealityFlowAPI.Instance.Templates.Select(
                    go => RealityFlowAPI.Instance.SpawnedObjects[go].name
                ).ToList()
            );

            dropdownIds.Clear();
            dropdownIds.AddRange(
                RealityFlowAPI.Instance.Templates.Select(
                    go => RealityFlowAPI.Instance.SpawnedObjects[go].id
                )
            );
        }
    }
}