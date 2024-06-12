using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class VertexHandle spawns vertex handles for manipulating the vertices of a mesh
/// </summary>
public class VertexHandle : Handle
{
    public int sharedVertexIndex;

    public override void Awake()
    {
        base.Awake();
        if(meshRenderer == null)
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        mode = ManipulationMode.vertex;
    }

    public override int[] GetSharedVertexIndicies()
    {
        int[] vertices = {sharedVertexIndex};
        return vertices;
    }

    public override void UpdateHandlePosition()
    {
        Vector3 pos = mesh.GetPositionFromSharedVertIndex(sharedVertexIndex);
        pos = mesh.transform.TransformPoint(pos);
        transform.position = pos;
    }
}
