using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeHandle : Handle
{
    // index of position in sharedVertex array
    public int A { get; private set; }
    public int B { get; private set; }

    //private GameObject spawnedCylinder;

    public override void Awake()
    {
        base.Awake();

        mode = ManipulationMode.edge;
    }

    public void SetIndicies(int A, int B)
    {
        if(A == B)
        {
            Debug.LogError("Edge indicies should not be equal!");
        }

        this.A = A;
        this.B = B;
    }

    public void UpdateMeshTransform()
    {
        Vector3 pos1 = mesh.GetPositionFromSharedVertIndex(A);
        Vector3 pos2 = mesh.GetPositionFromSharedVertIndex(B);

        pos1 = mesh.transform.TransformPoint(pos1);
        pos2 = mesh.transform.TransformPoint(pos2);
        Vector3 center = (pos1 + pos2) / 2;

        float distance = Vector3.Distance(center, pos2);
        Vector3 direction = center - pos2;
        Quaternion toRotation = Quaternion.FromToRotation(Vector3.up, direction);
        transform.position = center;
        transform.rotation = toRotation;
        transform.localScale = new Vector3(0.1f, distance, 0.1f);
    }

    public override void UpdateHandlePosition()
    {
        Vector3 pos1 = mesh.GetPositionFromSharedVertIndex(A);
        Vector3 pos2 = mesh.GetPositionFromSharedVertIndex(B);

        pos1 = mesh.transform.TransformPoint(pos1);
        pos2 = mesh.transform.TransformPoint(pos2);
        Vector3 center = (pos1 + pos2) / 2;

        float distance = Vector3.Distance(center, pos2);
        Vector3 direction = center - pos2;
        Quaternion toRotation = Quaternion.FromToRotation(Vector3.up, direction);
        transform.position = center;
        transform.rotation = toRotation;
        transform.localScale = new Vector3(0.1f, distance, 0.1f);
    }

    public override int[] GetSharedVertexIndicies()
    {
        int[] vertices = { A, B };
        return vertices;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
