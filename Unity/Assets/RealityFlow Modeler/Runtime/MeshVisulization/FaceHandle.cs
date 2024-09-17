using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FaceHandle : Handle
{
    public int faceIndex;

    private MeshFilter meshFilter;
    private Mesh faceMesh;

    public Vector3[] vpositions;
    public int[] uniquePositionIndicies;
    public int[] indicies;

    public override void Awake()
    {
        base.Awake();
        if(meshFilter == null)
            meshFilter = gameObject.GetComponent<MeshFilter>(); 

        faceMesh = new Mesh();
        mode = ManipulationMode.face;
    }

    public void SetPositions(int[] uniqueIndicies, int[] indicies)
    {
        if (isSelected)
            return;

        meshFilter = GetComponent<MeshFilter>();
        faceMesh.Clear();
        uniquePositionIndicies = uniqueIndicies.Distinct().ToArray();
        Vector3[] pos = new Vector3[uniqueIndicies.Length];

        for(int i = 0; i < uniquePositionIndicies.Length; i++)
        {
            pos[i] = mesh.positions[uniquePositionIndicies[i]];
        }

        vpositions = pos;
        this.indicies = indicies;

        faceMesh.vertices = vpositions;
        faceMesh.triangles = indicies;
        meshFilter.mesh = faceMesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider)
        {
            collider.sharedMesh = faceMesh;
        }
    }

    public void UpdateFacePosition()
    {
        int[] index = uniquePositionIndicies;

        for (int i = 0; i < index.Length; i++)
        {
            vpositions[i] = mesh.positions[index[i]];
        }

        faceMesh.vertices = vpositions;
        faceMesh.triangles = indicies;

        meshFilter.mesh = faceMesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if(collider)
        {
            collider.sharedMesh = faceMesh;
        }
    }

    public override void UpdateHandlePosition()
    {
        int[] index = uniquePositionIndicies;

        for (int i = 0; i < index.Length; i++)
        {
            vpositions[i] = mesh.positions[index[i]];
        }

        faceMesh.vertices = vpositions;
        faceMesh.triangles = indicies;

        meshFilter.mesh = faceMesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider)
        {
            collider.sharedMesh = faceMesh;
        }
    }

    public override int[] GetSharedVertexIndicies()
    {
        int[] faceIndicies = mesh.faces[faceIndex].GetUniqueIndicies();

        for(int i = 0; i < faceIndicies.Length; i++)
        {
            faceIndicies[i] = mesh.sharedVertexLookup[faceIndicies[i]];
        }

        return faceIndicies; 
    }
}
