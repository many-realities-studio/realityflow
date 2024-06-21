using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RaycastLogger : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    private RealityFlowAPI realityFlowAPI;

    // Input action for the trigger button
    public RealityFlowActions inputActions;

    // Prefab for the visual indicator
    public GameObject visualIndicatorPrefab;
    private GameObject visualIndicatorInstance;

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
            Debug.Log($"Raycast hit at position: {hitPosition}");

            // Log the action in RealityFlowAPI
            realityFlowAPI.actionLogger.LogAction(nameof(LogRaycastHitLocation), hitPosition);

            // Activate and move the visual indicator to the hit position
            if (visualIndicatorInstance != null)
            {
                visualIndicatorInstance.transform.position = hitPosition;
                visualIndicatorInstance.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Visual indicator instance is not available.");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
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
}
