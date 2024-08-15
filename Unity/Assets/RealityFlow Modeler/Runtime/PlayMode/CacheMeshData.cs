using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Org.BouncyCastle.Crypto.Engines;

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

    // Object with information containing collidable, grav, static
    private RfObject rfObj;
    private Rigidbody rb;
    private bool compErr = false;
    private BoxCollider boxCol;
    private MeshCollider meshCol;
    private EditableMesh em;
    private EditableMesh cachedMesh;
    
    public void SetRfObject(RfObject rfObj)
    {
        this.rfObj = rfObj;
    }
    
    void Start()
    {
        networkedPlayManager = FindObjectOfType<NetworkedPlayManager>();
        lastPlayModeState = networkedPlayManager.playMode;

        em = gameObject.GetComponent<EditableMesh>();

        cachedPosition = transform.localPosition;
        cachedScale = transform.localScale;
        cachedRotation = transform.localRotation;
        cachedMesh = em;

         // These should throw errors on failure (object doesn't have these components) TODO some other time:
        if(gameObject.GetComponent<Rigidbody>() != null)
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
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

        if(gameObject.GetComponent<MeshCollider>() != null)
        {
            meshCol = gameObject.GetComponent<MeshCollider>();
        } else
        {
            compErr = true;
        }
    }

    void Update()
    {
        // When Play mode is toggled either on or off
        // gameObject.transform.parent.parent.parent.name == "Forest 1"
        if (lastPlayModeState != networkedPlayManager.playMode)
        {
            lastPlayModeState = networkedPlayManager.playMode;
            // Cache values when Play mode is entered
            if (networkedPlayManager.playMode)
            {
                cachedPosition = transform.localPosition;
                cachedScale = transform.localScale;
                cachedRotation = transform.localRotation;
                cachedMesh = em;

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
                        rb.constraints = RigidbodyConstraints.None;
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
                        meshCol.enabled = true;
                        boxCol.enabled = true;
                    } else
                    {
                        meshCol.enabled = false;
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
                em = cachedMesh;
                em.RefreshMesh();

                //Debug.Log("there is a compErr: " + compErr);

                // if we have are missing a component don't mess with the object's physics
                if(!compErr)
                {
                    // Depending on the rf obj properties, behave appropraitely in play mode
                    // TODO: Move to it's own component (Like RFobject manager or something)
                    //       Include the playmode switch stuff
                    // if static, remove the added constraints
                    if(rfObj.isStatic)
                    {
                        //rb.constraints = RigidbodyConstraints.None;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    } else
                    {
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }

                    // all objects should float and be still
                    rb.useGravity = false;
                    rb.isKinematic = true;

                    // the object needs to be selectable
                    boxCol.enabled = true;
                }


                //RealityFlowAPI.Instance.UpdatePrimitive(gameObject);
                RealityFlowAPI.Instance.UpdateObjectTransform(gameObject.name);
                // All meshes should be selectable after Play mode is exited
                //gameObject.GetComponent<ObjectManipulator>().enabled = true;
            }
        }
    }
}
