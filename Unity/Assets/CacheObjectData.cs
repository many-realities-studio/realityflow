using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Org.BouncyCastle.Crypto.Engines;

/// <summary>
/// Class CacheMeshData is attached to every networked mesh and caches it's information when Play mode is entered. Upon exit of Play mode
/// the mesh is reverted back to its original state prior to entering Play mode.
/// </summary>
public class CacheObjectData : MonoBehaviour
{
    public NetworkedPlayManager networkedPlayManager;
    private bool lastPlayModeState;

    // As play mode grows in functionality, additional values and states should be cached here
    private Vector3 cachedPosition, cachedScale;
    private Quaternion cachedRotation;

    // Object with information containing collidable, grav, static
    private RfObject rfObj;
    private Rigidbody rb;

    private BoxCollider boxCol;

    // component errors
    private bool compErr = false;

    void Start()
    {
        rfObj = RealityFlowAPI.Instance.SpawnedObjects[gameObject];
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();
        lastPlayModeState = networkedPlayManager.playMode;

        cachedPosition = transform.localPosition;
        cachedScale = transform.localScale;
        cachedRotation = transform.localRotation;


        // These should throw errors on failure (object doesn't have these components) TODO some other time:
        if(gameObject.GetComponent<Rigidbody>() != null)
        {
            rb = gameObject.GetComponent<Rigidbody>();
        } else
        {
            compErr = true;
        }

        if(gameObject.GetComponent<BoxCollider>() != null)
        {
            boxCol = gameObject.GetComponent<BoxCollider>();
        } else
        {
            compErr = true;
        }
    }

    void Update()
    {
        // When Play mode is toggled either on or off
        // gameObject.transform.parent.parent.parent.name == "Forest 1"
        
        // TODO: ?Because we are constantly checking if play mode is on or off, set the proper gravity/properties relating to the rf obj?
        if (lastPlayModeState != networkedPlayManager.playMode)
        {
            lastPlayModeState = networkedPlayManager.playMode;
            // Cache values when Play mode is entered
            if (networkedPlayManager.playMode)
            {
                cachedPosition = transform.localPosition;
                cachedScale = transform.localScale;
                cachedRotation = transform.localRotation;

                Debug.Log("These are my rfProperties, Static: " + rfObj.isStatic + " Collidable: " + rfObj.isCollidable + " GavityEnb: " + rfObj.isGravityEnabled);

                // if we have are missing a component don't mess with the object's physics
                if(!compErr)
                {
                    // Depending on the rf obj properties, behave appropraitely in play mode
                    // TODO: Move to it's own component (Like RFobject manager or something)
                    //       Include the playmode switch stuff
                    // if static, be still on play
                    if(rfObj.isStatic)
                    {
                        rb.isKinematic = true;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    } else
                    {
                        rb.isKinematic = false;
                    }

                    // if has gravity, apply in play mode
                    if(rfObj.isGravityEnabled)
                    {
                        rb.useGravity = true;
                    } else
                    {
                        rb.useGravity = false;
                    }

                    // if the object is collide enabled, keep that otherwise turn off the collider
                    if(rfObj.isCollidable)
                    {
                        boxCol.enabled = true;
                    } else
                    {
                        boxCol.enabled = false;
                    }
                } 
            }
            // Revert values back to cached information upon leaving Play mode
            else
            {
                transform.localPosition = cachedPosition;
                transform.localScale = cachedScale;
                transform.localRotation = cachedRotation;

                Debug.Log("there is a compErr: " + compErr);

                // if we have are missing a component don't mess with the object's physics
                if(!compErr)
                {
                    // Depending on the rf obj properties, behave appropraitely in play mode
                    // TODO: Move to it's own component (Like RFobject manager or something)
                    //       Include the playmode switch stuff
                    // if static, remove the added constraints
                    if(rfObj.isStatic)
                    {
                        rb.constraints = RigidbodyConstraints.None;
                    }

                    // all objects should float and be still
                    rb.useGravity = false;
                    rb.isKinematic = true;

                    // the object needs to be selectable
                    boxCol.enabled = true;
                }

                

                // All meshes should be selectable after Play mode is exited
                //gameObject.GetComponent<ObjectManipulator>().enabled = true;
            }
        }
    }
}