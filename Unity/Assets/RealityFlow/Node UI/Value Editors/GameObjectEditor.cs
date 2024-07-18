using System;
using RealityFlow.NodeGraph;
using TMPro;
using UnityEngine;

namespace RealityFlow.NodeUI
{
    public class GameObjectEditor : MonoBehaviour, IValueEditor
    {
        [SerializeField]
        TMP_Text title;
        [SerializeField]
        TMP_Text nameText;

        public TMP_Text Name => title;

        public Action<NodeValue> OnTick { get; set; }

        GameObject Value { get; set; }

        public NodeValue NodeValue
        {
            set
            {
                if (value is null)
                {
                    Value = null;
                }
                else if (NodeValue.TryGetValue(value, out GameObject val))
                {
                    Value = val;
                }
                else
                    Debug.LogError("incorrect value type assigned to GameObjectEditor");

                SetName(Value);
            }
        }

        void SetName(GameObject go)
        {
            if (go == null)
                nameText.text = "nothing";
            else if (RealityFlowAPI.Instance.SpawnedObjects.TryGetValue(go, out RfObject rfObj))
                nameText.text = rfObj.name;
            else
                nameText.text = "<could not look up name>";
        }

        public void Tick()
        {
            SetName(Value);

            if (Value == null)
                OnTick?.Invoke(null);
            else
                OnTick?.Invoke(new GameObjectValue(Value));
        }

        public void BeginSelection()
        {
            EventBus<AvatarSelectedObject>.Subscribe(OnSelection);
            nameText.text = "Please select an object...";
        }

        void OnSelection(AvatarSelectedObject obj)
        {
            if (RealityFlowAPI.Instance.SpawnedObjects.ContainsKey(obj.Selected))
            {
                NodeValue = new GameObjectValue(obj.Selected);

                EventBus<AvatarSelectedObject>.Unsubscribe(OnSelection);

                Tick();
            }
            else
            {
                Debug.Log("Attempted to select non-RfObj for gameobject node value; ignoring");
            }
        }

        void OnDestroy()
        {
            EventBus<AvatarSelectedObject>.Unsubscribe(OnSelection);
        }
    }
}