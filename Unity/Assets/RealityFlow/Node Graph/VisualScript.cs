using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace RealityFlow.NodeGraph
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VisualScript : MonoBehaviour
    {
        public Graph graph;

        XRGrabInteractable interactable;

        void Start()
        {
            interactable = GetComponent<XRGrabInteractable>();
            interactable.activated.AddListener(OnActivate);
        }

        void OnActivate(ActivateEventArgs args)
        {
            // TODO: Trigger appropriate node defs
        }
    }
}