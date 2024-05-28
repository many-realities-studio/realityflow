using Behaviours;
using Newtonsoft.Json;
using Packages.realityflow_package.Runtime.scripts.Messages;
using Packages.realityflow_package.Runtime.scripts.Structures.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityFlow.Plugin.Scripts
{
    /// <summary>
    /// The purpose of this class is to hold all the information that is necessary to define a behaviour.
    /// The information from this class gets serialized and sent to the server.
    /// </summary>
    public class FlowBehaviour
    {
        [JsonIgnore]
        private dynamic _flowAction;

        [JsonProperty("TypeOfTrigger")]
        public string TypeOfTrigger { get; set; } // The trigger type

        [JsonProperty("Id")]
        public string Id { get; set; } // The ID of this behaviour

        [JsonProperty("TriggerObjectId")]
        public string TriggerObjectId { get; set; } // The first object ID - typically the trigger object

        [JsonProperty("TargetObjectId")]
        public string TargetObjectId { get; set; } // The second object ID - typically the target object

        [JsonProperty("NextBehaviour")]
        public List<string> NextBehaviour { get; set; } // The chain behaviour

        [JsonIgnore]
        public string BehaviourName { get; set; } // the behaviour name

        public event Action<string, string, string, List<string>> EventCalled;

        [JsonProperty("Action")]
        public dynamic flowAction
        {
            get => _flowAction;
            set
            {
                if (value == null || (value.GetType() == typeof(string) && value == "null"))
                {
                    _flowAction = new FlowAction(true);
                }
                else if (value.GetType().IsSubclassOf(typeof(FlowAction)) == false && value.GetType() != typeof(FlowAction))
                {
                    FlowAction baseAction = MessageSerializer.DesearializeObject(value, typeof(FlowAction));
                    _flowAction = FlowAction.ConvertToChildClass(value, baseAction.ActionType);
                }
                else
                {
                    _flowAction = value;
                }
            }
        }

        [JsonConstructor]
        public FlowBehaviour(string typeOfTrigger, string id, string triggerObjectId, string targetObjectId, List<string> nextBehaviour, dynamic flowAction)
        {
            TypeOfTrigger = typeOfTrigger;
            Id = id;
            TriggerObjectId = triggerObjectId;
            TargetObjectId = targetObjectId;
            NextBehaviour = nextBehaviour;

            this.flowAction = flowAction;
            EventCalled += BehaviourEventManager.ListenToEvents;
            BehaviourName = typeOfTrigger;
            if (TypeOfTrigger.Equals("Immediate"))
            {
                BehaviourName = this.flowAction.ActionType;
            }
        }

        public FlowBehaviour(string typeOfTrigger, string id, string triggerObjectId, string targetObjectId, dynamic flowAction)
        {
            TypeOfTrigger = typeOfTrigger;
            Id = id;
            TriggerObjectId = triggerObjectId;
            TargetObjectId = targetObjectId;
            NextBehaviour = new List<string>();
            this.flowAction = flowAction;
        }

        public void EventTrigger()
        {
            OnCallDown(BehaviourName);
        }

        /// <summary>
        /// Invokes event for this behaviour - subscribed to by BEM
        /// </summary>
        public void CallBehaviourEvent()
        {
            EventCalled.Invoke(BehaviourName, TriggerObjectId, TargetObjectId, NextBehaviour);
        }

        public string GetBehaviourName()
        {
            string flowBehaviourName = TypeOfTrigger;

            if (TypeOfTrigger.Equals("Immediate"))
            {
                flowBehaviourName = flowAction.ActionType;
            }

            return flowBehaviourName;
        }

        /// <summary>
        /// Determines logic based on scriptName to decide on what should be done on event call
        /// </summary>
        /// <param name="scriptName"></param>
        private void OnCallDown(string scriptName)
        {
            Debug.Log("Inside OnCallDown");

            // If it's a click event, just trigger the behaviours in its next behaviour list
            if (scriptName.Equals("Click"))
            {
                if (NextBehaviour.Count > 0)
                {
                    foreach (string chainedBehaviourId in NextBehaviour)
                    {
                        if (BehaviourEventManager.BehaviourList.TryGetValue(chainedBehaviourId, out FlowBehaviour chainedBehaviour))
                        {
                            chainedBehaviour.EventTrigger();
                        }

                        Debug.Log("execute chained events");
                    }
                }

                return;
            }

            GameObject triggerObject = FlowTObject.idToGameObjectMapping[TriggerObjectId].AttachedGameObject;
            GameObject targetObject = FlowTObject.idToGameObjectMapping[TargetObjectId].AttachedGameObject;

            // figure out how to add specific code to each script here
            switch (scriptName)
            {
                case "Teleport":
                    // take in input from colliding object that determines teleport coordinates (teleport nodes)
                    Vector3 coords = flowAction.teleportCoordinates.GetCoordinates();

                    triggerObject.transform.position = coords;
                    return;

                case "SnapZone":

                    // take in more info than teleport, but basically acts as a teleport within the other object
                    if (flowAction.teleportCoordinates.IsSnapZone)
                    {
                        triggerObject.transform.position = targetObject.transform.position + flowAction.teleportCoordinates.GetCoordinates();
                        triggerObject.transform.localScale = flowAction.teleportCoordinates.GetScale();
                        triggerObject.transform.rotation = flowAction.teleportCoordinates.GetRotation();
                    }
                    return;

                case "Enable":
                    // enable second object and all related scripts
                    targetObject.SetActive(true);
                    return;

                case "Disable":
                    // disable second object and all related scripts
                    targetObject.SetActive(false);
                    return;

                default:
                    Debug.LogError("BehaviourEvent.OnCallDown returned no matching script name for " + scriptName);
                    return;
            }
        }
    }
}