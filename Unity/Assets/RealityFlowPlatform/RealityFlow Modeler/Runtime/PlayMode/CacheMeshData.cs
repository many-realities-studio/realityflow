using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;

/// <summary>
/// Class CacheMeshData is attached to every networked mesh and caches it's information when Play mode is entered. Upon exit of Play mode
/// the mesh is reverted back to its original state prior to entering Play mode.
/// </summary>
public class CacheMeshData : MonoBehaviour
{
    public NetworkedPlayManager networkedPlayManager;
    private bool lastPlayModeState;

    // As play mode grows in functionality, additional values and states should be cached here
    private Vector3 cachedPosition, cachedScale;
    private Quaternion cachedRotation;

    void Start()
    {
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();
        lastPlayModeState = networkedPlayManager.playMode;

        cachedPosition = transform.localPosition;
        cachedScale = transform.localScale;
        cachedRotation = transform.localRotation;
    }

    void Update()
    {
        // When Play mode is toggled either on or off
        if (lastPlayModeState != networkedPlayManager.playMode && gameObject.transform.parent.parent.parent.name == "Forest 1")
        {
            lastPlayModeState = networkedPlayManager.playMode;
            // Cache values when Play mode is entered
            if (networkedPlayManager.playMode)
            {
                cachedPosition = transform.localPosition;
                cachedScale = transform.localScale;
                cachedRotation = transform.localRotation;
            }
            // Revert values back to cached information upon leaving Play mode
            else
            {
                transform.localPosition = cachedPosition;
                transform.localScale = cachedScale;
                transform.localRotation = cachedRotation;

                // All meshes should be selectable after Play mode is exited
                gameObject.GetComponent<ObjectManipulator>().enabled = true;
            }
        }
    }
}
