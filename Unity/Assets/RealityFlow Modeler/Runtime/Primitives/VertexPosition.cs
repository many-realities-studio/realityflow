using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TransformTypes;
using UnityEngine;

/// <summary>
/// Class VertexPosition, handles transforming vertex positions from local space to world space
/// and handles manipulations on sets of vertices
/// </summary>
public static class VertexPosition
{
    public static Vector3 GetVertexInWorldSpace(this EditableMesh mesh, int index)
    {
        return mesh.gameObject.transform.TransformPoint(mesh.positions[index]);
    }

    public static Vector3[] GetVerticesInWorldSpace(this EditableMesh mesh, Vector3[] positions)
    {
        if (mesh == null)
            return null;

        Vector3[] worldPositions = new Vector3[positions.Length];
        for(int i = 0; i < positions.Length; i++)
        {
            worldPositions[i] = mesh.transform.TransformPoint(positions[i]);
        }

        return worldPositions;
    }

    public static Vector3[] GetVertices(this EditableMesh mesh, EMFace face)
    {
        if (mesh == null)
            return null;

        int[] verts = face.GetUniqueIndicies();
        Vector3[] localPositions = new Vector3[verts.Length];

        for (int i = 0; i < localPositions.Length; i++)
        {
            localPositions[i] = mesh.positions[verts[i]];
        }

        return localPositions;
    }

    /// <summary>
    /// Translates a set of targeted vertices
    /// </summary>
    /// <param name="mesh"> target mesh</param>
    /// <param name="indicies"> indicies in shared vertex array </param>
    /// <param name="positions"> translation amount </param>
    public static void TransformVertices(this EditableMesh mesh, int[] indicies, Vector3 positions)
    {
        for (int i = 0; i < indicies.Length; i++)
        {
            TransformVertex(mesh, indicies[i], positions);
        }

        mesh.RefreshMesh();
    }

    public static void TranslateVerticesWithNetworking(this EditableMesh mesh, int[] indicies, Vector3 offset)
    {
        TransformVertices(mesh, indicies, offset);

        NetworkVertexPosition(mesh, TransformType.Translate, indicies, offset, Quaternion.identity, Vector3.one);
    }


    /// <summary>
    /// Translates the position of all conicident vertices
    /// </summary>
    public static void TransformVertex(this EditableMesh mesh, int sharedVertIndex, Vector3 position)
    {
        int[] vertIndices = mesh.sharedVertices[sharedVertIndex].vertices;

        for(int i = 0; i < vertIndices.Length; i++)
        {
            mesh.positions[vertIndices[i]] += position;
        }
    }

    /// <summary>
    /// Directly sets a vertex position in local space
    /// </summary>
    public static void SetVertexPosition(this EditableMesh mesh, int sharedVertIndex, Vector3 position)
    {
        int[] vertIndices = mesh.sharedVertices[sharedVertIndex].vertices;

        for (int i = 0; i < vertIndices.Length; i++)
        {
            mesh.positions[vertIndices[i]] = position;
        }
    }

    public static void RotateVerticesWithNetworking(this EditableMesh mesh, int[] indicies, Quaternion newRot)
    {
        RotateVertices(mesh, indicies, newRot);
        NetworkVertexPosition(mesh, TransformType.Rotate, indicies, Vector3.zero, newRot, Vector3.one);
    }

    public static void ScaleVerticesWithNetworking(this EditableMesh mesh, int[] indicies, Vector3 newScale)
    {
        ScaleVertices(mesh, indicies, newScale);
        NetworkVertexPosition(mesh, TransformType.Scale, indicies, Vector3.zero, Quaternion.identity, newScale);
    }

    /// <summary>
    /// Rotates a set of vertices around centroid
    /// </summary>
    public static void RotateVertices(this EditableMesh mesh, int[] indicies, Quaternion newRot)
    {
        Vector3 center = FindCentroidFromVertices(mesh, indicies);
        int index;
        for(int i = 0; i < indicies.Length; i++)
        {
            //index = mesh.sharedVertexLookup[indicies[i]];
            index = indicies[i];
            //Vector3 relativePos = mesh.positions[indicies[i]] - center;
            Vector3 relativePos = mesh.positions[mesh.sharedVertices[index].vertices[0]] - center;
            Vector3 rotatedPos = newRot * relativePos;
            rotatedPos += center;
            SetVertexPosition(mesh, index, rotatedPos);
        }

        mesh.RefreshMesh();
    }

    /// <summary>
    /// Scales a set of vertices around centroid
    /// </summary>
    public static void ScaleVertices(this EditableMesh mesh, int[] indicies, Vector3 newScale)
    {
        Vector3 center = FindCentroidFromVertices(mesh, indicies);
        int index;
        for (int j = 0; j < indicies.Length; j++)
        {
            //index = mesh.sharedVertexLookup[indicies[j]];
            index = indicies[j];
            Vector3 pos = mesh.positions[mesh.sharedVertices[indicies[j]].vertices[0]];
            //Vector3 direction = mesh.positions[indicies[j]] - center;
            Vector3 direction = pos - center;
            Vector3 newPos = Vector3.Scale(newScale, direction) + center;
            SetVertexPosition(mesh, index, newPos);
        }

        mesh.RefreshMesh();
    }


    /// <summary>
    /// Computes the centroid of a set of vertices
    /// </summary>
    public static Vector3 FindCentroidFromVertices(EditableMesh mesh, int[] indicies)
    {
        Vector3 centroid = Vector3.zero;
      
        for (int i = 0; i < indicies.Length; i++)
        {
            Vector3 pos = mesh.positions[mesh.sharedVertices[indicies[i]].vertices[0]];
            centroid += pos;
        }

        return centroid / indicies.Length;
    }

    public static Vector3 FindCentroidFromEdges(EditableMesh mesh, EdgeHandle[] edges)
    {
        Vector3 centroid = Vector3.zero;
        int[] positions = new int[2];
        for(int i =0; i < edges.Length; i++)
        {
            positions[0] = mesh.sharedVertices[edges[i].A].vertices[0];
            positions[1] = mesh.sharedVertices[edges[i].B].vertices[0];

            centroid += FindCentroidFromVertices(mesh, positions);
        }


        return centroid / edges.Length;
    }

    public static Vector3 FindCentroidFromFaces(EditableMesh mesh, int[] indicies)
    {
        Vector3 centroid = Vector3.zero;

        // Calculate centroid for each face
        for (int i = 0; i < indicies.Length; i++)
        {
            EMFace face = mesh.faces[indicies[i]];
            int[] positions = face.GetUniqueIndicies();
            centroid += FindCentroidFromVertices(mesh, positions);
        }

        return centroid / indicies.Length;
    }


    /// <summary>
    /// Facilitates networking any mesh changes
    /// </summary>
    private static void NetworkVertexPosition(EditableMesh mesh, TransformType type, int[] indicies, 
        Vector3 position, Quaternion rotation, Vector3 scale)
    {
        mesh.GetComponent<NetworkedMesh>().SendVertexTransformData(
            type,
            indicies,
            position,
            rotation,
            scale
        );
    }
}
