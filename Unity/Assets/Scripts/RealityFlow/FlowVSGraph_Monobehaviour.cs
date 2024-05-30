using RealityFlow.Plugin.Scripts;
using System;
using UnityEngine;

namespace Packages.realityflow_package.Runtime.scripts
{
    [ExecuteAlways]
    [Serializable]
    public class FlowVSGraph_Monobehaviour : MonoBehaviour
    {
        // This MonoBehaviour handles attempting to update a FlowVSGraph every frame through its representative GameObject in the scene, but 
        // will only succeed once a graph's IsUpdated flag gets set through a graph change.
        public FlowVSGraph underlyingFlowVSGraph;

        private void OnEnable()
        {
        }

        private void OnValidate()
        {
            //Debug.Log("OnValidate");
        }

        public void Update()
        {
            if (underlyingFlowVSGraph != null)
            {
                underlyingFlowVSGraph.UpdateFlowVSGraphGlobally(underlyingFlowVSGraph);
            }
        }
    }
}