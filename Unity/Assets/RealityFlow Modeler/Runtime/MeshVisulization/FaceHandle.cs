using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class FaceHandle provides methods to create handles used for manipulating faces of a mesh
/// </summary>
public class FaceHandle : Handle
{
    public int faceIndex;

    private MeshFilter meshFilter;
    public Mesh faceMesh { get; private set; }

    public Vector3[] vpositions;

    [SerializeField]
    private int[] uniquePositionIndicies;
    public int[] indicies;
    private int[] sharedVertIndex;

    public override void Awake()
    {
        base.Awake();
        if(meshFilter == null)
            meshFilter = gameObject.GetComponent<MeshFilter>(); 

        faceMesh = new Mesh();
        mode = ManipulationMode.face;
    }

    /// <summary>
    /// Sets the initial parameters of the face mesh
    /// </summary>
    /// <param name="uniqueIndicies"> the indicies of the vertices in the possitions array </param>
    /// <param name="indicies"> the indicies of the triangle should be [0, 1, 2, ...]</param>
    public void SetPositions(int[] uniqueIndicies, int[] indicies)
    {
        if (isSelected)
            return;

        meshFilter = GetComponent<MeshFilter>();
        faceMesh.Clear();
        uniquePositionIndicies = uniqueIndicies.Distinct().ToArray();
        Vector3[] pos = new Vector3[uniquePositionIndicies.Length];

        for(int i = 0; i < uniquePositionIndicies.Length; i++)
        {
            pos[i] = Vector3.Scale(mesh.positions[uniquePositionIndicies[i]], mesh.gameObject.transform.localScale);
        }

        vpositions = pos;
        this.indicies = indicies;

        faceMesh.vertices = vpositions;
        faceMesh.triangles = indicies;
        faceMesh.RecalculateNormals(); 
        meshFilter.mesh = faceMesh;

        CacheSharedVertIndex();

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider)
        {
            collider.sharedMesh = faceMesh;
        }

        if(HandleSpawner.Instance.xrayActive)
            ReverseFaceWindingOrder();
    }

    /// <summary>
    /// Reverse the winding order of the faces
    /// </summary>
    public void ReverseFaceWindingOrder()
    {
        int[] tris = meshFilter.mesh.GetIndices(0);
        System.Array.Reverse(tris);

        // Mesh collider use backface culling, if it hits a backface it ignores it
        meshFilter.mesh.triangles = tris;
        MeshCollider collider = GetComponent<MeshCollider>();
        if(collider != null)
        {
            collider.sharedMesh = meshFilter.mesh;
        }
    }

    private void CacheSharedVertIndex()
    {
        sharedVertIndex = new int[uniquePositionIndicies.Length];
        for(int i =0; i < sharedVertIndex.Length; i++)
        {
            sharedVertIndex[i] = mesh.sharedVertexLookup[uniquePositionIndicies[i]];
        }
    }

    public override void UpdateHandlePosition()
    {
        int[] index = uniquePositionIndicies;

        for (int i = 0; i < index.Length; i++)
        {
            vpositions[i] = Vector3.Scale(mesh.positions[uniquePositionIndicies[i]], mesh.gameObject.transform.localScale);
        }

        faceMesh.vertices = vpositions;
        faceMesh.triangles = indicies;

        meshFilter.mesh = faceMesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider)
        {
            collider.sharedMesh = faceMesh;
        }

        if(HandleSpawner.Instance.xrayActive)
        {
            ReverseFaceWindingOrder();
        }    
    }

    public override int[] GetSharedVertexIndicies()
    {
        return sharedVertIndex; 
    }
}
