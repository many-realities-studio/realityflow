using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Spawning;
using UnityEngine;

public enum PrimitiveSpawningMode
{
    Move,
    Scale
}

/// <summary>
/// Class PrimitiveSpawner handles user spawning and placing primitives into the world.
/// Should be attached to the hand controller of the user.
/// </summary>
public class PrimitiveSpawner : MonoBehaviour
{
    [SerializeField] public GameObject primitive;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject resizeMeshPlane;

    public static PrimitiveSpawner instance;
    private GameObject attachedObject;
    private GameObject spawnedMesh;
    private NetworkedMesh attachedObjectNetworkMesh;
    private PrimitiveSpawningMode primitiveSpawningMode;
    private GameObject XZplane;
    private GameObject Yplane;
    private SnapGrid grid;

    public bool active;
    private Vector3 spawnPos;
    private Vector3 gridSpawnPos;
    private ShapeType currentShapeType;
    private RaycastHit currentHitResult;

    private bool resizingMesh;
    // private Slider snappingMode;
    private GridTool gridTool;
    private float lastSnappingMode;
    private float offMode = 0f;
    private float surfaceMode = 1f;
    private bool leftHandDominant = false;

    public void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

        grid = FindObjectOfType<SnapGrid>();
        gridTool = FindObjectOfType<GridTool>();
        rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();

        if (rayInteractor == null)
        {
            Debug.LogWarning("No ray interactor found!");
        }
        
        active = false;
        primitiveSpawningMode = PrimitiveSpawningMode.Scale;

        PrimitiveMenu.OnEnterMeshSpawn += EnterMeshSpawningMode;
        PrimitiveMenu.OnExitMeshSpawn += ExitMeshSpawningMode;

        PaletteHandManager.OnHandChange += SwapHands;
    }

    public void OnDestroy()
    {
        PrimitiveMenu.OnEnterMeshSpawn -= EnterMeshSpawningMode;
        PrimitiveMenu.OnExitMeshSpawn -= ExitMeshSpawningMode;

        PaletteHandManager.OnHandChange -= SwapHands;
    }

    /// <summary>
    /// Function creates a mesh proxy and attaches it to the end of the ray.
    /// </summary>
    public void EnterMeshSpawningMode(ShapeType type)
    {
        // Only find a reference to this owner's snapping mode once
        // if (snappingMode == null)
        // {
        //     FindSnappingModeReference();
        // }

        // if (snappingMode != null)
        // {
            ChangeSnappingModes();
        // }

        // if (gridTool != null)
        // {
        //     Debug.Log("Grid mode is on? " + gridTool.isActive);
        // }

        currentShapeType = type;
        CreateMeshProxy();
        active = true;
    }

    public void ExitMeshSpawningMode()
    {
        DestroyProxy();
        active = false;
        // Default ray cast distance when not in mesh creation mode
        rayInteractor.maxRaycastDistance = 10f;
        rayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;
    }

    public void OnSpawnButtonRelease()
    {
        if (!active)
            return;

        // Mesh has been created, owner can now exit mesh creation at any time
        ReEnableCancelButton(leftHandDominant);

        resizingMesh = false;
        CreateMeshProxy();
        Destroy(XZplane);
        Destroy(Yplane);

        spawnedMesh.GetComponent<MeshCollider>().enabled = true;
    }

    private void DestroyProxy()
    {
        if (attachedObject != null)
        {
            NetworkSpawnManager.Find(this).Despawn(attachedObject);
        }
    }

    private void CreateMeshProxy()
    {
        if(attachedObject != null)
        {
            DestroyProxy();
        }

        attachedObject = NetworkSpawnManager.Find(this).SpawnWithPeerScope(primitive);
        EditableMesh mesh = PrimitiveGenerator.CreatePrimitive(currentShapeType);
        EditableMesh em = attachedObject.GetComponent<EditableMesh>();

        em.CreateMesh(mesh);

        // Disable mesh collision so the ray doesn't interact with it
        attachedObject.GetComponent<MeshCollider>().enabled = false;

        attachedObjectNetworkMesh = attachedObject.GetComponent<NetworkedMesh>();

        Destroy(mesh.gameObject);
    }

    /// <summary>
    /// Grabs a reference to the snapping mode for use in determining which snapping mode the owner is in.
    /// </summary>
    private void FindSnappingModeReference()
    {
        // DELETEME: First try the following code of grabbing a reference. Here we are seeing if we can get away with this approach
        // as it is simple. If this approach fails when testing with two people in the same room then try the approach used by the 
        // Select Tool by grabbing the list of users in the room and assigning the reference to the user with the corresponding AvatarManager.UUID

        // snappingMode = GameObject.Find("Snapping Backplate").GetComponentInChildren<Slider>();
        // gridTool = GameObject.Find("Snapping Backplate").GetComponentInChildren<PressableButton>();
    }

    private void SwapHands(bool isLeftHandDominant)
    {
        // Assign for use in the enabling and disabling of the cancel mesh creation button
        leftHandDominant = isLeftHandDominant;

        // Switch the interactor rays
        if(isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
        }

        ChangeInputButtons(isLeftHandDominant);
    }

    private void ChangeInputButtons(bool isLeftHandDominant)
    {
        try
        {
            if(isLeftHandDominant)
            {
                // Switch mesh creation buttons
                gameObject.GetComponents<OnButtonPress>()[0].enabled = false;
                gameObject.GetComponents<OnButtonPress>()[1].enabled = true;

                // Switch cancel mesh creation buttons
                gameObject.GetComponents<OnButtonPress>()[2].enabled = false;
                gameObject.GetComponents<OnButtonPress>()[3].enabled = true;
            }
            else
            {
                // Switch mesh creation buttons
                gameObject.GetComponents<OnButtonPress>()[0].enabled = true;
                gameObject.GetComponents<OnButtonPress>()[1].enabled = false;

                // Switch cancel mesh creation buttons
                gameObject.GetComponents<OnButtonPress>()[2].enabled = true;
                gameObject.GetComponents<OnButtonPress>()[3].enabled = false;
            }
        }
        catch (NullReferenceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// Used for disabling the cancel mesh creation button to avoid users from getting out of mesh creation mode during it.
    /// </summary>
    private void DisableCancelButton(bool isLeftHandDominant)
    {
      // This is the wrong way to do it.

        try
        {
            if(isLeftHandDominant)
            {
                gameObject.GetComponents<OnButtonPress>()[3].enabled = false;
            }
            else
            {
                gameObject.GetComponents<OnButtonPress>()[2].enabled = false;
            }
        }
        catch (NullReferenceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// Used for re-enabling the cancel mesh creation button once a user is exited from mesh creation mode.
    /// </summary>
    private void ReEnableCancelButton(bool isLeftHandDominant)
    {
        try
        {
            if(isLeftHandDominant)
            {
                gameObject.GetComponents<OnButtonPress>()[3].enabled = true;
            }
            else
            {
                gameObject.GetComponents<OnButtonPress>()[2].enabled = true;
            }
        }
        catch (NullReferenceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void ChangeSnappingModes()
    {
        if (gridTool.currentSnapModeValue == offMode)
        {
            rayInteractor.maxRaycastDistance = 0.15f;
            rayInteractor.GetComponent<XRInteractorLineVisual>().enabled = false;
        }
        else if (gridTool.currentSnapModeValue == surfaceMode)
        {
            rayInteractor.maxRaycastDistance = 10f;
            rayInteractor.GetComponent<XRInteractorLineVisual>().enabled = true;
        }
        else
        {
            Debug.LogError("Could not change snapping modes.");
        }

    }

    /// <summary>
    /// Spawns mesh at target ray position. Reference to function stored in Spawn Manager.
    /// </summary>
    public void SpawnMesh()
    {
        if (!active)
            return;

        spawnedMesh = NetworkSpawnManager.Find(this).SpawnWithRoomScopeWithReturn(primitive);
        EditableMesh em = spawnedMesh.GetComponent<EditableMesh>();

        if (attachedObject == null) return; 

        em.CreateMesh(attachedObject.GetComponent<EditableMesh>());
        spawnedMesh.transform.position = attachedObject.transform.position;

        TryEnterResizeMode();
    }

    private void TryEnterResizeMode()
    {
        if(primitiveSpawningMode == PrimitiveSpawningMode.Scale)
        {
            // We no longer want the owner to exit mesh mode during mesh creation
            DisableCancelButton(leftHandDominant);

            resizingMesh = true;
            // Destroy the proxy so the user doesn't see it while resizing the object
            DestroyProxy();

            spawnCollisionPlanes();

        }
    }

    private void GetRayCollision()
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out currentHitResult);
    }

    private void TryMoveMesh()
    {
        // Hold object if it's not held so it changes can be seen by others
        if(!attachedObjectNetworkMesh.isHeld)
        {
            if(attachedObjectNetworkMesh.HasContext())
                attachedObjectNetworkMesh.StartHold();
        }

        if (!gridTool.isActive)
        {
            if (currentHitResult.collider == null)
            {
                // If no hit, spawn object at the end of the ray
                spawnPos = rayInteractor.rayOriginTransform.position + (rayInteractor.rayOriginTransform.forward * rayInteractor.maxRaycastDistance);
                attachedObject.transform.position = spawnPos;
                return;
            }

            // Check if we're hitting a UI component
            if (currentHitResult.collider.gameObject.GetComponentInParent<CanvasRenderer>())
            {
                return;
            }

            spawnPos = currentHitResult.point;
        }
        else if (gridTool.isActive && gridTool.currentSnapModeValue == offMode)
        {
            spawnPos = gridSpawnPos;
        }
        else if (gridTool.isActive && gridTool.currentSnapModeValue == surfaceMode)
        {
            if (currentHitResult.collider == null)
            {
                // If no hit, spawn object at the end of the ray based off the grid
                spawnPos = grid.GetNearestPointOnGrid(rayInteractor.rayOriginTransform.position + (rayInteractor.rayOriginTransform.forward * rayInteractor.maxRaycastDistance));
                attachedObject.transform.position = spawnPos;
                return;
            }

            // Check if we're hitting a UI component
            if (currentHitResult.collider.gameObject.GetComponentInParent<CanvasRenderer>())
            {
                return;
            }

            spawnPos = gridSpawnPos;
        }

        // Offset spawn position so it doesn't intersect with ground
        if(currentShapeType != ShapeType.Cone)
        {
            // Elevate primitive plane to account for snapping point but only if not at the lowest snap unit value
            if(currentShapeType == ShapeType.Plane && grid.size != 0.05f)
            {
                spawnPos.y += 0.01f;
                
                if (gridTool.isActive)
                {
                    spawnPos.y += 0.04f;
                }
            }
            else
            {
                Bounds objBounds = attachedObject.GetComponent<EditableMesh>().mesh.bounds;
                spawnPos += new Vector3(0, objBounds.extents.y, 0);
            }
        }

        attachedObject.transform.position = spawnPos;
    }

    private void ScaleMesh()
    {
        if (!resizingMesh)
            return;

        float size = calculateSizeFromRayDistance();

        PrimitiveRebuilder.RebuildMesh(spawnedMesh.GetComponent<EditableMesh>(), size);
        NetworkedMesh nm = spawnedMesh.GetComponent<NetworkedMesh>();
        if(nm.HasContext())
        {
            nm.UpdateAndSendMeshResizeData(size);
        }

        spawnedMesh.GetComponent<MeshCollider>().enabled = false;
    }

    private float calculateSizeFromRayDistance()
    {
        if (currentHitResult.collider == null)
        {
            return Mathf.Max(0.1f, Vector3.Distance(spawnPos, (rayInteractor.rayOriginTransform.position + (rayInteractor.rayOriginTransform.forward * rayInteractor.maxRaycastDistance))));
        }
        else
        {
            return Mathf.Max(0.1f, Vector3.Distance(spawnPos, currentHitResult.point));
        }
    }

    private void spawnCollisionPlanes()
    {
        XZplane = GameObject.Instantiate(resizeMeshPlane, spawnPos, Quaternion.identity);

        // Rotate the Ysomething plane so it's up vector faces the user who is spawning the mesh
        Vector3 rotVector = Quaternion.LookRotation(rayInteractor.rayOriginTransform.forward).eulerAngles;
        rotVector.x = -90.0f;
        rotVector.z = 0.0f;
        Yplane = GameObject.Instantiate(resizeMeshPlane, spawnPos, Quaternion.Euler(rotVector));
    }

    public void Update()
    {
        // if (snappingMode != null)
        // {
            // If the snapping mode is changed during mesh creation, then change it
            if (lastSnappingMode != gridTool.currentSnapModeValue && active)
            {
                lastSnappingMode = gridTool.currentSnapModeValue;
                ChangeSnappingModes();
            }
        // }

        if (!active)
        {
            return;
        }

        if (!gridTool.isActive)
        {
            GetRayCollision();
        }
        else if (gridTool.isActive && gridTool.currentSnapModeValue == offMode)
        {
            GetRayCollision();
            gridSpawnPos = grid.GetNearestPointOnGrid(rayInteractor.transform.position);
        }
        else if (gridTool.isActive && gridTool.currentSnapModeValue == surfaceMode)
        {
            GetRayCollision();
            gridSpawnPos = grid.GetNearestPointOnGrid(currentHitResult.point);
        }

        if (!resizingMesh)
        {
            TryMoveMesh();
        }
        else
        {
            ScaleMesh();
        }
    }
}
