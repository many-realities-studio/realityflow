using UnityEngine;

public class CharacterReset : MonoBehaviour
{
    // The GameObject to reset (in this case, Rf MRTK Rig)
    [SerializeField] public GameObject targetObject;

    // The position to reset the character to
    [SerializeField] private Vector3 safePosition = new Vector3(0, 5, 0);

    // The y-coordinate threshold below which the character will be reset
    [SerializeField] private float fallThreshold = -30f;

    void Update()
    {
        // Check if the target object has fallen below the threshold
        if (targetObject.transform.position.y < fallThreshold)
        {
            // Reset the target object's position to the safe position
            targetObject.transform.position = safePosition;
        }
    }
}
