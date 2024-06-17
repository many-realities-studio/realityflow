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
        public bool isTemplate;

        ObjectManipulator interactable;

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
            if (isTemplate)
                gameObject.SetActive(false);
        }

        void OnActivate(ActivateEventArgs args)
        {
            foreach (NodeIndex node in graph.NodesOfType("OnActivate"))
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);
        }

        void OnSelect(SelectEnterEventArgs args)
        {
            foreach (NodeIndex node in graph.NodesOfType("OnSelect"))
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);
        }

        void OnSelectExit(SelectExitEventArgs args)
        {
            foreach (NodeIndex node in graph.NodesOfType("OnSelectExit"))
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node);
        }

        void OnCollisionEnter(Collision col)
        {
            foreach (NodeIndex node in graph.NodesOfType("OnCollision"))
                // TODO: Replace this with RealityFlowID
                ctx.EvaluateGraphFromRoot(gameObject, new(graph), node, ("collidedWith", new(col.gameObject)));
        }
    }
}