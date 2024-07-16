using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using RealityFlow; // Ensure RealityFlow namespace is correct
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class CustomObjectManipulator : ObjectManipulator
{
    private MyNetworkedObject networkedObject;
    private Coroutine updateTransformCoroutine;

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

            // Call the API method to update the object transform
            RealityFlowAPI.Instance.UpdateObjectTransform(
                            rfObj.name,
                            HostTransform.position,
                            HostTransform.rotation,
                            HostTransform.localScale
                        );

            networkedObject = HostTransform.GetComponent<MyNetworkedObject>();
            if (networkedObject != null)
            {
                // Start coroutine to update transform every second
                updateTransformCoroutine = StartCoroutine(UpdateTransformPeriodically());
            }
        }
        else
        {
            Debug.Log("HostTransform is null");
        }

        // Debug log to print the current location
        Debug.Log("Current location of HostTransform: position: " + HostTransform.position +
                  " rotation: " + HostTransform.rotation +
                  " scale: " + HostTransform.localScale);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Log the transformation when manipulation ends (object is released)
        if (HostTransform != null)
        {
            var rfObj = RealityFlowAPI.Instance.FindSpawnedObject(HostTransform.gameObject.name);
            Debug.Log("Exited on select should log " + rfObj.name);
            RealityFlowAPI.Instance.actionLogger.LogAction(
                            nameof(RealityFlowAPI.UpdateObjectTransform),
                            rfObj.name,
                            HostTransform.position,
                            HostTransform.rotation,
                            HostTransform.localScale
                        );

            // Call the API method to update the object transform
            RealityFlowAPI.Instance.UpdateObjectTransform(
                            rfObj.name,
                            HostTransform.position,
                            HostTransform.rotation,
                            HostTransform.localScale
                        );

            if (networkedObject != null)
            {
                networkedObject.UpdateTransform();
                StopCoroutine(updateTransformCoroutine);
            }
        }
        else
        {
            Debug.Log("HostTransform is null");
        }

        // Debug log to print the final location
        Debug.Log("Final location of HostTransform: position: " + HostTransform.position +
                  " rotation: " + HostTransform.rotation +
                  " scale: " + HostTransform.localScale);
    }

    private IEnumerator UpdateTransformPeriodically()
    {
        while (true)
        {
            if (networkedObject != null)
            {
                networkedObject.UpdateTransform();
            }
            yield return new WaitForSeconds(1);
        }
    }
}