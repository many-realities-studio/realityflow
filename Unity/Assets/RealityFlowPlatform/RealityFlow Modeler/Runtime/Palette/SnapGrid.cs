using UnityEngine;

/// <summary>
/// Class SnapGrid creates a grid with a size specified by the Snap Units.
/// </summary>
public class SnapGrid : MonoBehaviour
{
    public float size = 1f;
    public float rotationSize = 1;
    [SerializeField] private bool hideGizmos;

    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        position -= transform.position;

        int xCount = Mathf.RoundToInt(position.x / size);
        int yCount = Mathf.RoundToInt(position.y / size);
        int zCount = Mathf.RoundToInt(position.z / size);

        Vector3 result = new Vector3(
            (float)xCount * size,
            (float)yCount * size,
            (float)zCount * size);

        result += transform.position;

        return result;
    }

    public float GetNearestStretch(float scaleDirection)
    {
        float tempScale = Mathf.Round(scaleDirection / size) * size;

        if (tempScale == 0)
            tempScale = Mathf.Ceil(scaleDirection / size) * size;

        return tempScale;
    }

    public Vector3 GetNearestScale(Vector3 inScale, Vector3 originalScale)
    {
        Vector3 tempScale = inScale;

        tempScale.x = Mathf.Round(tempScale.x / size);
        tempScale.y = Mathf.Round(tempScale.y / size);
        tempScale.z = Mathf.Round(tempScale.z / size);
        tempScale *= size;

        if (tempScale.x == 0 || tempScale.y == 0 || tempScale.z == 0)
        {
            return originalScale;
        }

        if (originalScale.x == tempScale.x || originalScale.y == tempScale.y || originalScale.z == tempScale.z) 
            return originalScale;

        return tempScale;
    }


    public float GetNearestRotation(float rotationDirection)
    {
        return Mathf.Round(rotationDirection / rotationSize) * rotationSize;
    }

    // Used purely to visualize the positions a user can create a mesh (only in Scene view for now)
    private void OnDrawGizmos()
    {
        if (!hideGizmos)
        {
            Gizmos.color = Color.yellow;
            // Gizmos are drawn off the origin point (0, 0, 0). Modify the x, y, z values to increase or decrease the amount of gizmos visualized
            for (float x = -5; x < 6; x += size)
            {
                for (float y = 0; y < 4; y += size)
                {
                    for (float z = -5; z < 6; z += size)
                    {
                        var point = GetNearestPointOnGrid(new Vector3(x, y, z));
                        Gizmos.DrawSphere(point, 0.1f);
                    }
                }                
            }
        }
    }
}