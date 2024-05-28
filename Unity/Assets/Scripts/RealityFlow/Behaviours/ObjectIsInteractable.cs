using System;
using System.Linq;
using RealityFlow.Plugin.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Behaviours
{
    /// <summary>
    /// Determines whether the object is interactable and logs which objects and events it is interactable with
    /// </summary>
    public class ObjectIsInteractable : MonoBehaviour, IPointerClickHandler
    {
        private static SerializableDictionary<string, bool> interactableWith;
        private static SerializableDictionary<FlowBehaviour, string> interactableEvents;

        public event Action SetEventTrigger;

// TODO Fix Interactables by creating API between MRTK & RF that is loosely coupled
        // private Interactable interactableScript;

        private string CurrentEvent = null;

        private string objectId;

        #region Monobehaviour Methods

        /// <summary>
        /// Returns assigned guid
        /// </summary>
        public string GetGuid()
        {
            return objectId;
        }
        
        
        public List<FlowBehaviour> GetAllInteractableEvents()
        {
            return interactableEvents.Keys.ToList();
        }

        #endregion Monobehaviour Methods

        #region Trigger Methods

        /// <summary>
        /// Determines logic based on user's click
        /// </summary>
        public void OnSelect()
        {
            Debug.Log("clicked");
            if (interactableEvents.ContainsValue("Click"))
            {
                SetEventTrigger.Invoke();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Pointer clicked");
            if (eventData.pointerPressRaycast.gameObject == gameObject)
            {
                Debug.Log("clicked");
                if (interactableEvents.ContainsValue("Click"))
                {
                    SetEventTrigger.Invoke();
                }
            }
        }

        /// <summary>
        /// Determines logic upon colliding with another object
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            if (CurrentEvent != null)
            {
                return;
            }
            if (interactableEvents.ContainsValue("Teleport"))
            {
                SetEventTrigger.Invoke();
                CurrentEvent = "Teleport";
            }
            else if (interactableEvents.ContainsValue("SnapZone"))
            {
                SetEventTrigger.Invoke();
                CurrentEvent = "SnapZone";
            }
        }

        /// <summary>
        /// Initializes all private variables
        /// </summary>
        /// <param name="objectId"></param>
        public void Initialize(string objectId)
        {
            this.objectId = objectId;
            interactableWith = new SerializableDictionary<string, bool>();
            interactableEvents = new SerializableDictionary<FlowBehaviour, string>();

            if (!(SystemInfo.deviceType == DeviceType.Desktop || SystemInfo.deviceType == DeviceType.Handheld))
            {
                // TODO Disabled
                // interactableScript = gameObject.AddComponent<Interactable>();

                // interactableScript.IsEnabled = true;
                // interactableScript.States = BehaviourEventManager.DefaultInteractableStates;
                // interactableScript.OnClick.AddListener(() => OnSelect());
            }
            else
            {
                if (Camera.main.gameObject.GetComponent<PhysicsRaycaster>() == null)
                {
                    Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
                }
            }
        }

        /// <summary>
        /// Allows all scripts to function normally once again
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionExit(Collision collision)
        {
            CurrentEvent = null;
        }

        #endregion Trigger Methods

        #region Custom Methods

        /// <summary>
        /// Adds Interactable Objects to dictionary of objects interactable with current GameObject
        /// </summary>
        /// <param name="interactables"></param>
        public void InteractableWith(List<string> interactables)
        {
            for (int i = 0; i < interactables.Count; i++)
            {
                if (!interactableWith.ContainsKey(interactables[i]))
                    interactableWith.Add(interactables[i], true);
            }
        }

        /// <summary>
        /// Adds a list of FlowBehaviours to dictionary of events interactable with current GameObject
        /// </summary>
        /// <param name="events"></param>
        public void AddInteractableEvents(List<FlowBehaviour> events)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (!interactableEvents.ContainsKey(events[i]))
                {
                    SetEventTrigger += events[i].EventTrigger;
                    interactableEvents.Add(events[i], events[i].BehaviourName);
                }
            }
        }

        /// <summary>
        /// Adds a FlowBehaviour to dictionary of events interactable with current GameObject
        /// </summary>
        /// <param name="e"></param>
        public void AddInteractableEvent(FlowBehaviour e)
        {
            if (interactableEvents == null)
            {
                interactableEvents = new SerializableDictionary<FlowBehaviour, string>();
            }

            if (!interactableEvents.ContainsKey(e))
            {
                SetEventTrigger += e.EventTrigger;
                interactableEvents.Add(e, e.BehaviourName);
            }
        }

        /// <summary>
        /// Remove a GameObject from the dictionary of interactable objects
        /// </summary>
        /// <param name="go"></param>
        public void RemoveInteractableObject(string go)
        {
            if (interactableWith.ContainsKey(go))
            {
                interactableWith.Remove(go);
            }
        }

        /// <summary>
        /// Disables an interactable object in the dictionary
        /// </summary>
        /// <param name="go"></param>
        public void TurnOffInteractableObject(string go)
        {
            if (interactableWith.ContainsKey(go))
            {
                interactableWith.Remove(go);
                interactableWith.Add(go, false);
            }
        }

        /// <summary>
        /// Removes an event from the dictionary of interactable events
        /// </summary>
        /// <param name="flowBehaviour"></param>
        /// <param name="go2"></param>
        public void RemoveInteractableEvent(FlowBehaviour flowBehaviour, string go2)
        {
            if (interactableEvents.ContainsKey(flowBehaviour))
            {
                if (go2 != null)
                {
                    if (flowBehaviour.TargetObjectId == go2)
                    {
                        interactableEvents.Remove(flowBehaviour);
                        GameObject g2 = FlowTObject.idToGameObjectMapping[go2].AttachedGameObject;
                        if (g2.GetComponent<ObjectIsInteractable>())
                        {
                            g2.GetComponent<ObjectIsInteractable>().RemoveInteractableEvent(flowBehaviour);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes an event from the dictionary of interactable events (dependent GameObject)
        /// </summary>
        /// <param name="flowBehaviour"></param>
        public void RemoveInteractableEvent(FlowBehaviour flowBehaviour)
        {
            if (interactableEvents.ContainsKey(flowBehaviour))
            {
                interactableEvents.Remove(flowBehaviour);
            }
        }

        /// <summary>
        /// Turns off an event in the dictionary
        /// </summary>
        /// <param name="flowBehaviour"></param>
        public void TurnOffInteractableEvent(FlowBehaviour flowBehaviour)
        {
            if (interactableEvents.ContainsKey(flowBehaviour))
            {
                interactableEvents.Remove(flowBehaviour);
                interactableEvents.Add(flowBehaviour, null);
            }
        }

        #endregion Custom Methods

        #region Interactablility Bools

        /// <summary>
        /// Determines whether the GameObject is interactable with this GameObject
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public bool IsInteractableWith(string go)
        {
            if (interactableWith.ContainsKey(go))
                return true;

            return false;
        }

        /// <summary>
        /// Determines if the event is interactable with this GameObject
        /// </summary>
        /// <param name="flowBehaviour"></param>
        /// <returns></returns>
        public bool IsInteractableWithEvent(FlowBehaviour flowBehaviour)
        {
            if (interactableEvents.ContainsKey(flowBehaviour))
                return true;

            return false;
        }

        #endregion Interactablility Bools
    }
}