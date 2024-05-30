//using Microsoft.MixedReality.Toolkit.UI;
//using Microsoft.MixedReality.Toolkit.Utilities;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviours
{
    public static class BehaviourEventManager
    {
        public static event Action<string> SendEventDown;

        public static SerializableDictionary<string, GameObject> GoIds = null;
        public static SerializableDictionary<string, FlowBehaviour> BehaviourList;
        public static string PreviousBehaviourId = null;

        public static void Initialize()
        {
            GoIds = new SerializableDictionary<string, GameObject>();
            BehaviourList = new SerializableDictionary<string, FlowBehaviour>();
        }

        public static void Clear()
        {
            GoIds = null;
            BehaviourList = null;
        }

        /// <summary>
        /// Adds the FlowBehaviour to the BehaviourList and makes each object Interactable
        /// </summary>
        /// <param name="flowBehaviour"></param>
        public static void CreateNewBehaviour(FlowBehaviour flowBehaviour)
        {
            // Add the behaviour to the list of behaviours
            BehaviourList[flowBehaviour.Id] = flowBehaviour;
            PreviousBehaviourId = flowBehaviour.Id;

            // Make both objects interactable
            ObjectIsInteractable oIsIFirst = FindAndMakeInteractable(flowBehaviour.TriggerObjectId);
            ObjectIsInteractable oIsISecond = FindAndMakeInteractable(flowBehaviour.TargetObjectId);

            oIsIFirst.AddInteractableEvent(flowBehaviour);

            if (oIsIFirst == null || oIsISecond == null)
            {
                Debug.LogWarning("There is a missing gameobject. Failed to make Interaction.");
                return;
            }
        }

        /// <summary>
        /// Finds the gameobject associated with objectId, and adds an ObjectIsInteractable component to it.
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static ObjectIsInteractable FindAndMakeInteractable(string objectId)
        {
            if (FlowTObject.idToGameObjectMapping.TryGetValue(objectId, out FlowTObject foundObject))
            {
                Debug.Log("Found object " + foundObject.Id + " ... making interactable");
                ObjectIsInteractable oIsI = MakeObjectInteractable(foundObject.AttachedGameObject, objectId);
                return oIsI;
            }
            else
            {
                Debug.Log("Cannot make object interactable. Object not found in project.");
                return null;
            }
        }

        /// <summary>
        /// Adds an ObjectIsInteractable component to the game object and adds its' id to the list
        /// of game objects (GoIds)
        /// </summary>
        /// <param name="go"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public static ObjectIsInteractable MakeObjectInteractable(GameObject go, string objectId)
        {
            ObjectIsInteractable oisI = go.GetComponent<ObjectIsInteractable>();

            if (oisI == null)
            {
                oisI = go.AddComponent<ObjectIsInteractable>();
                oisI.Initialize(objectId);
            }

            return oisI;
        }

        /// <summary>
        /// Adds the childFlowBehaviourId into the parent's NextBehaviour list
        /// </summary>
        /// <param name="childFlowBehaviourId"></param>
        /// <param name="parentFlowBehaviourId"></param>
        public static void LinkBehaviours(string childFlowBehaviourId, string parentFlowBehaviourId)
        {
            if (BehaviourList.TryGetValue(parentFlowBehaviourId, out FlowBehaviour parentFlowBehaviour))
            {
                if (!parentFlowBehaviour.NextBehaviour.Contains(childFlowBehaviourId))
                {
                    parentFlowBehaviour.NextBehaviour.Add(childFlowBehaviourId);
                }
            }
            else
            {
                Debug.LogWarning("Unable to find ParentBehaviour.");
            }
        }

        public static void UpdateBehaviour(FlowBehaviour flowBehaviour)
        {
            // Add the behaviour to the list of behaviours
            BehaviourList.Add(flowBehaviour.Id, flowBehaviour);

            // Make both objects interactable
            ObjectIsInteractable oIsIFirst = FindAndMakeInteractable(flowBehaviour.TriggerObjectId);
            ObjectIsInteractable oIsISecond = FindAndMakeInteractable(flowBehaviour.TargetObjectId);

            if (oIsIFirst == null || oIsISecond == null)
            {
                Debug.LogWarning("There is a missing gameobject. Failed to make Interaction.");
                return;
            }
        }

        /// <summary>
        /// Adds the FlowBehaviour fb2 to fb1's NextBehaviour list
        /// </summary>
        /// <param name="fb1"></param>
        /// <param name="fb2"></param>
        /// <returns></returns>
        public static FlowBehaviour AddChain(FlowBehaviour fb1, FlowBehaviour fb2)
        {
            if (!fb1.NextBehaviour.Contains(fb2.Id))
            {
                fb1.NextBehaviour.Add(fb2.Id);
            }

            return fb1;
        }

        /// <summary>
        /// Removes the FlowBehaviour from go1 and go2's list of interactable events
        /// </summary>
        /// <param name="go1"></param>
        /// <param name="go2"></param>
        /// <param name="flowBehaviour"></param>
        public static void DeleteFlowBehaviour(string go1, string go2, FlowBehaviour flowBehaviour)
        {
            GameObject g1 = FlowTObject.idToGameObjectMapping[go1].AttachedGameObject;
            ObjectIsInteractable interactScript = g1.GetComponent<ObjectIsInteractable>();

            if (interactScript == null)
                return;

            interactScript.RemoveInteractableEvent(flowBehaviour, go2);
            BehaviourList.Remove(flowBehaviour.Id);
        }

        /// <summary>
        /// Registers when a BehaviourEvent has been called - sends up to server communicator
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="go1"></param>
        /// <param name="go2"></param>
        /// <param name="chain"></param>
        public static void ListenToEvents(string eventName, string go1, string go2, List<string> nextBehaviours)
        {
            // sends updated coordinates
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>The GameObject associated with the guid. Returns null if object is not found</returns>
        public static GameObject GetGoFromGuid(string guid)
        {
            GameObject temp;

            if (GoIds == null)
            {
                GoIds = new SerializableDictionary<string, GameObject>();
            }

            if (GoIds.TryGetValue(guid, out temp))
            {
                Debug.Log(temp);
                return temp;
            }

            Debug.LogWarning("The game object could not be found in the GoIds list.");
            return null;
        }
    }
}