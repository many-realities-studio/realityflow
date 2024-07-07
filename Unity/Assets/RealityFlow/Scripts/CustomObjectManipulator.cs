using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using RealityFlow; // Ensure RealityFlow namespace is correct
using UnityEngine.XR.Interaction.Toolkit;

public class CustomObjectManipulator : ObjectManipulator
{
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {

        base.OnSelectEntered(args);

        // Log the transformation when manipulation starts (object is grabbed)
        if (HostTransform != null)
        {
            var rfObj = RealityFlowAPI.Instance.FindSpawnedObject(HostTransform.gameObject.name);
            Debug.Log("Entered on select should log " + rfObj.name);
            RealityFlowAPI.Instance.actionLogger.LogAction(
                            nameof(RealityFlowAPI.UpdateObjectTransform),
                            rfObj.name,
                            HostTransform.position,
                            HostTransform.rotation,
                            HostTransform.localScale
                        );
        }
        else
            Debug.Log("HostTransform is null");

        // Debug log to print the current location
        Debug.Log("Current location of HostTransform: position: " + HostTransform.position +
                  " rotation: " + HostTransform.rotation +
                  " scale: " + HostTransform.localScale);
    }

    /*
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Log the transformation when manipulation ends (object is released)
        if (HostTransform != null)
        {
            var rfObj = RealityFlowAPI.Instance.SpawnedObjects[HostTransform.gameObject];
            RealityFlowAPI.Instance.actionLogger.LogAction(
                nameof(RealityFlowAPI.UpdateObjectTransform),
                rfObj.id,
                HostTransform.position,
                HostTransform.rotation,
                HostTransform.localScale
            );
        }
    }
    */
}
