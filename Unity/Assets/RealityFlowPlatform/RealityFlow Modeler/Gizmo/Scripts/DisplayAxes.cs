using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The DisplayAxes class sets the proper rotation of the axes based on camera position
/// </summary>
public class DisplayAxes : MonoBehaviour
{
    // names for x, y, and z child comps
    // names must be in xyz order
    private readonly string[] childNameArray = { "X", "Y", "Z"};

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraQuad = GetCameraQuad();

        foreach (string childName in childNameArray)
        {
            if (GetDirectionOfComp(cameraQuad, childName) == 1)
            {
                SetRotation(childName, 0);
            }
            // == -1
            else
            {
                SetRotation(childName, 180);
            }
        }
    }

    /// <summary>
    /// Determines the 3D quardent the camera is located in relative the gizmo
    /// </summary>
    /// <returns>Returns a Vector3 representing if the camera is located in the positive or negative direction for each axes</returns>
    public Vector3 GetCameraQuad()
    {
        Vector3 quad = new Vector3(0, 0, 0);
        Vector3 camPos = Camera.main.transform.position;

        quad.x = camPos.x > transform.position.x ? 1 : -1;
        quad.y = camPos.y > transform.position.y ? 1 : -1;
        quad.z = camPos.z > transform.position.z ? 1 : -1;

        return quad;
    }

    /// <summary>
    /// Gets the positive or negative direction of the result of GetCameraQuad for an axes
    /// </summary>
    /// <param name="cameraQuad">A Vector3 representing the result of GetCameraQuad</param>
    /// <param name="name">The name of axis</param>
    /// <returns>Returns -1 or 1 float to represent the direction in question</returns>
    float GetDirectionOfComp(Vector3 cameraQuad, string name)
    {
        if (name == childNameArray[0])
            return cameraQuad.x;
        else if (name == childNameArray[1])
            return cameraQuad.y;
        return cameraQuad.z;
    }

    /// <summary>
    /// Sets the rotation of a GameObject contained in this.transform
    /// </summary>
    /// <param name="name">The name of the GameObject</param>
    /// <param name="rot">The degrees of rotation to set the GameObject</param>
    void SetRotation(string name, float rot)
    {
        Vector3 newRot = new Vector3(0, 0, 0);

        // == x
        if (name == childNameArray[0])
            newRot = new Vector3(0, rot, 0);

        // == y
        else if (name == childNameArray[1])
            newRot = new Vector3(0, 0, rot);

        // == z
        else if (name == childNameArray[2])
            newRot = new Vector3(rot, 0, 0);

        transform.Find(name).gameObject.transform.rotation = Quaternion.Euler(newRot);
    }
}
