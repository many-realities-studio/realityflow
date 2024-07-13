using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Microsoft.MixedReality.Toolkit.Input;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;

public class RaycastLogger : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    private RealityFlowAPI realityFlowAPI;

    // Input action for the trigger button
    public RealityFlowActions inputActions;

    // Prefab for the visual indicator
    public GameObject visualIndicatorPrefab;
    private GameObject visualIndicatorInstance;

    // Glow material and original materials
    public Material glowMaterial;
    private Material originalMaterial;
    private GameObject lastHitObject;

    // Store the selected object's name
    private string selectedObjectName;

    // Variables for object spawning
    private GameObject currentPrefab = null;
    private bool spawnToggle = false;

    private void Awake()
    {
        inputActions = new RealityFlowActions();
    }

    private void Start()
    {
        realityFlowAPI = RealityFlowAPI.Instance; // Ensure you have a reference to the RealityFlowAPI instance

        // Instantiate the visual indicator and disable it
        if (visualIndicatorPrefab != null)
        {
            visualIndicatorInstance = Instantiate(visualIndicatorPrefab);
            visualIndicatorInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Visual indicator prefab is not assigned.");
        }
    }

    private void OnEnable()
    {
        inputActions.RealityFlowXRActions.SelectLocation.performed += LogRaycastHitLocation;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.RealityFlowXRActions.SelectLocation.performed -= LogRaycastHitLocation;
        inputActions.Disable();
    }

    private void LogRaycastHitLocation(InputAction.CallbackContext context)
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitResult))
        {
            Vector3 hitPosition = hitResult.point;
            GameObject hitObject = hitResult.collider.gameObject;

            Debug.Log($"Raycast hit at position: {hitPosition}");
            Debug.Log($"Raycast hit object: {hitObject.name}");

            // Log the action in RealityFlowAPI
            //realityFlowAPI.actionLogger.LogAction(nameof(LogRaycastHitLocation), hitPosition, hitObject.name);

            if (hitObject.CompareTag("Teleport"))
            {
                // Activate and move the visual indicator to the hit position
                if (visualIndicatorInstance != null)
                {
                    visualIndicatorInstance.transform.position = hitPosition;
                    visualIndicatorInstance.SetActive(true);
                    Debug.Log("Visual indicator updated and activated.");
                }
                else
                {
                    Debug.LogWarning("Visual indicator instance is not available.");
                }

                Debug.Log($"Raycast hit object with 'Teleport' tag: {hitObject.name}. Skipping selection.");
                return;
            }

            ApplyGlowEffect(hitObject);
            // Store the selected object's name
            selectedObjectName = hitObject.name;

            // Handle object spawning
            if (spawnToggle && currentPrefab != null)
            {
                SpawnObjectAtHitLocation();
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
        }
    }

    private void ApplyGlowEffect(GameObject hitObject)
    {
        if (hitObject != lastHitObject && lastHitObject != null && !lastHitObject.CompareTag("Floor"))
        {
            // Revert the last hit object's material to its original material
            RevertGlowEffect(lastHitObject);
            lastHitObject = null; // Reset the last hit object if it's not the floor
        }

        // Save the original material of the hit object
        Renderer renderer = hitObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            renderer.material = glowMaterial;
            lastHitObject = hitObject;
        }
    }

    private void RevertGlowEffect(GameObject hitObject)
    {
        Renderer renderer = hitObject.GetComponent<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
        }
    }

    public Vector3 GetVisualIndicatorPosition()
    {
        if (visualIndicatorInstance != null && visualIndicatorInstance.activeSelf)
        {
            return visualIndicatorInstance.transform.position;
        }
        return Vector3.zero;
    }

    public string GetSelectedObjectName()
    {
        return selectedObjectName;
    }

    private void SpawnObjectAtHitLocation()
    {
        // Instantiate the object in the scene and over the network, then set its position to the hit position
        GameObject currentObject = RealityFlowAPI.Instance.SpawnObject(selectedObjectName, visualIndicatorInstance.transform.position, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);

        // Log the spawned object
        if (currentObject != null)
        {
            currentObject.transform.position = visualIndicatorInstance.transform.position;
            Debug.Log("Spawned " + currentObject.name);
        }
        else
        {
            Debug.LogError("Failed to spawn object: " + selectedObjectName);
        }
    }
    // Function to cancel spawning of the current prefab
    public void CancelSpawn()
    {
        if (spawnToggle)
        {
            Debug.Log("Object spawn cancelled: no longer spawning " + currentPrefab.name);
            spawnToggle = false;
            currentPrefab = null;
        }
    }

    // Function called by the UI button in the default object library. Sets spawn toggle and prefab.
    public void RaySpawnToggle(GameObject objectPrefab)
    {
        Debug.Log("Spawning toggled on for object: " + objectPrefab.name);
        spawnToggle = true;
        currentPrefab = objectPrefab;
    }
}
