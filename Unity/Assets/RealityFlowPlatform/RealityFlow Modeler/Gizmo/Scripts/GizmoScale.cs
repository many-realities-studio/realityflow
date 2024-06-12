using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The class GizmoScale scales the size of the gizmo to take the same relative screen space regardless of distance to user
/// </summary>
public class GizmoScale : MonoBehaviour
{
    float initDistance;
    // Start is called before the first frame update
    void Start()
    {
        initDistance = 6.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float curDistance = distance(Camera.main.transform.position, transform.position);
        float scale = curDistance / initDistance;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Finds the distance between two points in 3D space
    /// </summary>
    /// <param name="a">The first point</param>
    /// <param name="b">The second point</param>
    /// <returns>The distance between <paramref name="a"/> and <paramref name="b"/></returns>
    float distance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.x - b.x, 2f)
            + Mathf.Pow(a.y - b.y, 2f)
            + Mathf.Pow(a.z - b.z, 2f)
            );
    }
}
