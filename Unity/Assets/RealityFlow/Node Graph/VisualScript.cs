using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace RealityFlow.NodeGraph
{
    public class VisualScript : MonoBehaviour
    {
        public Graph graph;
        readonly EvalContext ctx = new();

        public bool IsTemplate => RealityFlowAPI.Instance.SpawnedObjects[gameObject].isTemplate;

        ObjectManipulator interactable;

        NetworkedPlayManager _playManager;
        NetworkedPlayManager PlayManager
        {
            get
            {
                if (!_playManager)
                {
                    _playManager = FindObjectOfType<NetworkedPlayManager>();
                    if (_playManager)
                    {
                        _playManager.enterPlayMode.AddListener(OnEnterPlayMode);
                        _playManager.exitPlayMode.AddListener(OnExitPlayMode);
                    }
                }
                
                return _playManager;
            }
        }

        void Start()
        {
            interactable = GetComponent<ObjectManipulator>();
            if (interactable)
            {
                interactable.activated.AddListener(OnActivate);
                interactable.selectEntered.AddListener(OnSelect);
                interactable.selectExited.AddListener(OnSelectExit);
            }
            else
                Debug.LogError("Couldn't find an object manipulator!");
        }

        public void OnEnterPlayMode()
        {
            if (IsTemplate)
                gameObject.SetActive(false);

            if (graph is null)
                return;

            foreach (NodeIndex node in graph.NodesOfType("OnStart"))
                // TODO: Instead of playing from each root individually, add all to queue and evaluate
                // once.
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);

            ctx.ClearVariables();
        }

        public void OnExitPlayMode()
        {
            if (IsTemplate || graph is null)
                gameObject.SetActive(true);

            ctx.ClearVariables();
        }

        void OnActivate(ActivateEventArgs args)
        {
            if (!PlayManager.playMode || graph is null)
                return;

            foreach (NodeIndex node in graph.NodesOfType("OnActivate"))
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);
        }

        void OnSelect(SelectEnterEventArgs args)
        {
            if (!PlayManager.playMode || graph is null)
                return;

            foreach (NodeIndex node in graph.NodesOfType("OnSelect"))
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);
        }

        void OnSelectExit(SelectExitEventArgs args)
        {
            if (!PlayManager.playMode || graph is null)
                return;

            foreach (NodeIndex node in graph.NodesOfType("OnSelectExit"))
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);
        }

        void OnCollisionEnter(Collision col)
        {
            if (!PlayManager.playMode || graph is null)
                return;

            foreach (NodeIndex node in graph.NodesOfType("OnCollision"))
                ctx.EvaluateGraphFromRoot(
                    gameObject,
                    new(graph), 
                    node, 
                    (
                        "collidedWith", 
                        RealityFlowAPI.Instance.SpawnedObjects.ContainsKey(col.gameObject) 
                            ? new GameObjectValue(col.gameObject) 
                            : null
                    )
                );
        }
    }
}