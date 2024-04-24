using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

/// <summary>
/// Class EditableMesh stores all the data and methods for creating and modifying meshes
/// at runtime.
/// </summary>
public class EditableMesh : MonoBehaviour
{
    public Mesh mesh { get; set; }

    MeshFilter meshFilter;

    new MeshRenderer renderer;

    [SerializeField]
    public EMFace[] faces { get; private set; }

    public Vector3[] positions;

    public Vector3[] normals;

    public EMSharedVertex[] sharedVertices;
    internal Dictionary<int, int> sharedVertexLookup;

    internal MeshOperationCache meshOperationCache;

    public ShapeType baseShape;
    public bool isEmpty = true;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<MeshRenderer>();

        if (CheckForExistingMesh())
        {
            LoadMeshData();
        }

        baseShape = ShapeType.NoShape;
    }

    public void CreateMesh(EditableMesh otherMesh)
    {
        positions = new Vector3[otherMesh.positions.Length];
        Array.Copy(otherMesh.positions, positions, otherMesh.positions.Length);

        faces = new EMFace[otherMesh.faces.Length];
        Array.Copy(otherMesh.faces, faces, otherMesh.faces.Length);

        baseShape = otherMesh.baseShape;
        sharedVertices = EMSharedVertex.GetSharedVertices(positions);
        sharedVertexLookup = EMSharedVertex.CreateSharedVertexDict(sharedVertices);
        meshOperationCache = new MeshOperationCache(this);

        FinalizeMesh();
    }

    public void CreateMesh(PrimitiveData input)
    {
        positions = new Vector3[input.positions.Length];
        Array.Copy(input.positions, positions, input.positions.Length);

        faces = new EMFace[input.faces.Length];
        Array.Copy(input.faces, faces, input.faces.Length);

        baseShape = input.type;
        sharedVertices = EMSharedVertex.GetSharedVertices(positions);
        sharedVertexLookup = EMSharedVertex.CreateSharedVertexDict(sharedVertices);
        meshOperationCache = new MeshOperationCache(this);

        FinalizeMesh();
    }

    public static EditableMesh CreateMesh(Vector3[] positions, EMFace[] faces)
    {
        GameObject go = new GameObject();

        EditableMesh em = go.AddComponent<EditableMesh>();
        em.positions = positions;
        em.faces = faces;
        em.sharedVertices = EMSharedVertex.GetSharedVertices(positions);
        em.sharedVertexLookup = EMSharedVertex.CreateSharedVertexDict(em.sharedVertices);
        em.FinalizeMesh();
        return em;
    }

    public static EditableMesh CreateMeshFromVertices(Vector3[] points)
    {
        GameObject go = new GameObject();

        EditableMesh em = go.AddComponent<EditableMesh>();

        EMFace[] f = new EMFace[points.Length / 4];

        for (int i = 0; i < points.Length; i += 4)
        {
            f[i/4] = new EMFace(new int[6]
            {
                i + 0, i + 1, i + 2,
                i + 1, i + 3, i + 2
            });
        }

        em.positions = points;
        em.faces = f;

        em.sharedVertices = EMSharedVertex.GetSharedVertices(em.positions);
        em.sharedVertexLookup = EMSharedVertex.CreateSharedVertexDict(em.sharedVertices);

        em.FinalizeMesh();

        return em;
    }

    public Vector3 GetPositionFromSharedVertIndex(int index)
    {
        return positions[sharedVertices[index].vertices[0]];
    }

    public Vector3[] GetUniqueVertexPositions()
    {
        Vector3[] pos = new Vector3[sharedVertices.Length];

        for(int i = 0; i < sharedVertices.Length; i++)
        {
            pos[i] = positions[sharedVertices[i].vertices[0]];
        }

        return pos;
    }

    public void CacheOperation(MeshOperation operation)
    {
        if (meshOperationCache == null)
            meshOperationCache = new MeshOperationCache(this);

        meshOperationCache.CacheOperation(operation);
    }

    public void FinalizeMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        mesh.vertices = positions;

        List<int>[] tris = new List<int>[1];
        tris[0] = new List<int>();

        for (int i = 0; i < faces.Length; i++)
        {
            tris[0].AddRange(faces[i].indicies);
        }

        int[] indicies = new int[tris[0].Count];
        for (int i = 0; i<tris[0].Count; i++)
            indicies[i] = tris[0][i];

        mesh.SetIndices(indicies, MeshTopology.Triangles, 0, false);

        RefreshMesh();

        isEmpty = false;
    }

    /// <summary>
    /// Updates the mesh normals, bounds, and updates the meshFilter.
    /// </summary>
    public void RefreshMesh()
    {
        mesh.vertices = positions;
        

        var collider = GetComponent<MeshCollider>();
        if (collider != null)
        {
            collider.sharedMesh = mesh;
        }

        meshFilter.mesh = mesh;

        mesh.RecalculateNormals();
        RecalculateBoundsSafe();
    }

    /// <summary>
    /// Recalculates the bounds of a mesh and adds some padding if they're 0
    /// this prevents errors with MRTK bounding boxes in BoundsControl
    /// </summary>
    public void RecalculateBoundsSafe()
    {
        mesh.RecalculateBounds();
        Bounds mb = mesh.bounds;

        if (mesh.bounds.size.x < 0.001f)
        {
            mb.Expand(new Vector3(0.05f, 0.0f, 0.0f));
        }

        if (mesh.bounds.size.y < 0.001f)
        {
            mb.Expand(new Vector3(0.0f, 0.05f, 0.0f));
        }

        if (mesh.bounds.size.z < 0.001f)
        {
            mb.Expand(new Vector3(0.0f, 0.0f, 0.05f));
        }

        mesh.bounds = mb;
    }

    /// <summary>
    /// Checks if this component has been attached to an object with an existing mesh, useful for
    /// importing meshes
    /// </summary>
    private bool CheckForExistingMesh()
    {
        if (meshFilter == null)
            return false;

        if (meshFilter.mesh == null)
            return false;
        else
            return true;
    }

    /// <summary>
    /// Uses existing mesh data to populate the fields of Editable Mesh
    /// </summary>
    private void LoadMeshData()
    {
        Mesh m = meshFilter.mesh;

        if (m.vertices.Length <= 0)
        {
            return;
        }

        positions = m.vertices;
        normals = m.normals;

        List<EMFace> f = new List<EMFace>();

        // Convert mesh triangle array to faces, unfortunately can't really extrapolate quads vs
        // triangles since unity treats everything as a triangle.
        for (int i = 0; i < m.subMeshCount; i++)
        {
            for (int j = 0; j < m.GetTriangles(i).Length; j += 3)
            {
                //Debug.Log("Triangle " + j/3 + " {" + m.GetTriangles(i)[j] + " " + m.GetTriangles(i)[j + 1]
                //   + " " + m.GetTriangles(i)[j + 2]);
                f.Add(new EMFace(new int[]
                {
                    m.GetTriangles(i)[j],
                    m.GetTriangles(i)[j + 1],
                    m.GetTriangles(i)[j + 2]
                }));
            }
        }

        faces = f.ToArray();
        sharedVertices = EMSharedVertex.GetSharedVertices(positions);

        /*
        for(int i = 0; i < sharedVertices.Length; i++)
        {
            string s = "";
            for(int j = 0; j < sharedVertices[i].vertices.Length; j++)
            {
                s += sharedVertices[i].vertices[j];
                s += " ";
            }

            Debug.Log(s);
        */
    }

}
