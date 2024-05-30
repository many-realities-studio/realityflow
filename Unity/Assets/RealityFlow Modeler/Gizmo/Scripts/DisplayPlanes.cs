using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The DisplayPlanes class sets the the corrent position of the plane translate objects base on camera position
/// </summary>
public class DisplayPlanes : MonoBehaviour
{
    [SerializeField]
    GameObject gizmo;
    Vector3 cameraQuad;
    Vector3 intialLocalPos;

    // Start is called before the first frame update
    void Start()
    {
        intialLocalPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        cameraQuad = gizmo.GetComponent<DisplayAxes>().GetCameraQuad();

        Vector3 newPos = this.transform.localPosition;

        newPos.y = intialLocalPos.y * cameraQuad.y;
        newPos.x = intialLocalPos.x * cameraQuad.x;
        newPos.z = intialLocalPos.z * cameraQuad.z;

        this.transform.localPosition = newPos;
    }
}
