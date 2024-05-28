using RealityFlow.Plugin.Scripts;
using System;
using UnityEngine;

namespace Packages.realityflow_package.Runtime.scripts
{
    [ExecuteAlways]
    [Serializable]
    public class FlowObject_Monobehaviour : MonoBehaviour
    {
        public Rigidbody objectRigidbody;
        public Color latestColor;
        public FlowTObject underlyingFlowObject;

        private bool constraintsFrozen;


        private void OnEnable()
        {
            constraintsFrozen = true;
        }

        private void OnValidate()
        {
        }

        // In update, objects are frozen locally if a user is not allowed to modify them. As soon as an object is checked out,
        // the object will be unfrozen, and frozen again after being checked in.
        public void Update()
        {
            if (underlyingFlowObject != null)
            {
                underlyingFlowObject.UpdateObjectGlobally(underlyingFlowObject);
            }

            if (underlyingFlowObject.CanBeModified == true && constraintsFrozen)
            {
                constraintsFrozen = false;
                objectRigidbody.constraints = RigidbodyConstraints.None;
            }
            
            if (underlyingFlowObject.CanBeModified == false && constraintsFrozen == false)
            {
                constraintsFrozen = true;
                objectRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }
}