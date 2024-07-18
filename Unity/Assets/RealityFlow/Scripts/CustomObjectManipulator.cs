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
        }
    }
}