using Microsoft.MixedReality.Toolkit.SpatialManipulation;
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
    /// <param name="position"> translation amount </param>
    public static void TransformVertices(this EditableMesh mesh, int[] indices, Vector3 position)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            TransformVertex(mesh, indices[i], position);
        }

        mesh.RefreshMesh();
    }

    public static void TransformVertices(this EditableMesh mesh, int[] indices, Vector3[] positions)
    {
        if (indices.Length != positions.Length)
            return;

        for (int i = 0; i < indices.Length; i++)
        {
            TransformVertex(mesh, indices[i], positions[i]);
        }

        mesh.RefreshMesh();
    }

    public static void TranslateVerticesWithNetworking(this EditableMesh mesh, int[] indices, Vector3 offset)
    {
        TransformVertices(mesh, indices, offset);
        //for (int i = 0; i < indices.Length; i++) { mesh.CacheOperation(indices[i], offset); }
        NetworkVertexPosition(mesh, TransformType.Translate, indices, offset, Quaternion.identity, Vector3.one);
    }


    /// <summary>
    /// Translates the position of all conicident vertices
    /// </summary>
    public static void TransformVertex(this EditableMesh mesh, int sharedVertIndex, Vector3 offset)
    {
        int[] vertIndices = mesh.sharedVertices[sharedVertIndex].vertices;

        for(int i = 0; i < vertIndices.Length; i++)
        {
            // mesh.positions[vertIndices[i]] += position;
            mesh.positions[vertIndices[i]] += offset;
        }
    }

    /// <summary>
    /// Directly sets a vertex position in local space
    /// </summary>
    public static void SetVertexPosition(this EditableMesh mesh, int sharedVertIndex, Vector3 position)
    {
        int[] vertIndices = mesh.sharedVertices[sharedVertIndex].vertices;
        Vector3 startingPos = mesh.positions[vertIndices[0]];
        Vector3 offset = position - startingPos;

        for (int i = 0; i < vertIndices.Length; i++)
        {
            mesh.positions[vertIndices[i]] = position;
        }
    }

    public static void RotateVerticesWithNetworking(this EditableMesh mesh, int[] indices, Quaternion newRot)
    {
        Vector3[] offsets = RotateVertices(mesh, indices, newRot);
        NetworkVertexPosition(mesh, TransformType.Rotate, indices, Vector3.zero, newRot, Vector3.one);
    }

    public static void ScaleVerticesWithNetworking(this EditableMesh mesh, int[] indices, Vector3 newScale)
    {
        Vector3[] offsets = ScaleVertices(mesh, indices, newScale);
        NetworkVertexPosition(mesh, TransformType.Scale, indices, Vector3.zero, Quaternion.identity, newScale);
    }

    public static void BakeVerticesWithNetworking(this EditableMesh mesh)
    {
        // should be better way to do this!
        if (mesh == null)
            return;
        BakeVertices(mesh);
        NetworkVertexPosition(mesh, TransformType.Bake, new int[0], Vector3.zero, mesh.transform.rotation, mesh.transform.localScale);
    }

    /// <summary>
    /// Rotates a set of vertices around centroid
    /// </summary>
    public static Vector3[] RotateVertices(this EditableMesh mesh, int[] indices, Quaternion newRot)
    {
        Vector3 center = FindCentroidFromVertices(mesh, indices);
        Vector3[] offsets = new Vector3[indices.Length];
        int index;
        for(int i = 0; i < indices.Length; i++)
        {
            index = indices[i];
            Vector3 relativePos = mesh.positions[mesh.sharedVertices[index].vertices[0]] - center;
            Vector3 rotatedPos = newRot * relativePos;
            rotatedPos += center;

            offsets[i] = rotatedPos - mesh.GetPositionFromSharedVertIndex(index);
            SetVertexPosition(mesh, index, rotatedPos);
        }

        mesh.RefreshMesh();
        return offsets;
    }

    /// <summary>
    /// Scales a set of vertices around centroid
    /// </summary>
    public static Vector3[] ScaleVertices(this EditableMesh mesh, int[] indices, Vector3 newScale)
    {
        Vector3 center = FindCentroidFromVertices(mesh, indices);
        Vector3[] offsets = new Vector3[indices.Length];
        int index;
        for (int i = 0; i < indices.Length; i++)
        {
            index = indices[i];
            Vector3 pos = mesh.positions[mesh.sharedVertices[indices[i]].vertices[0]];
            Vector3 direction = pos - center;
            Vector3 newPos = Vector3.Scale(newScale, direction) + center;
            offsets[i] = newPos - mesh.GetPositionFromSharedVertIndex(index);
            SetVertexPosition(mesh, index, newPos);
        }

        mesh.RefreshMesh();
        return offsets;
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

    public static void BakeVertices(this EditableMesh mesh)
    {
        Transform meshTransform = mesh.gameObject.transform;
        EMSharedVertex[] sharedVertices = mesh.sharedVertices;
        int[] indices = new int[sharedVertices.Length];

        for (int i = 0; i < sharedVertices.Length; i++)
        {
            indices[i] = i;

            Vector3 relativePos = mesh.positions[sharedVertices[i].vertices[0]];
            Vector3 scalePos = Vector3.Scale(relativePos, meshTransform.localScale);
            Vector3 rotatedPos = meshTransform.rotation * scalePos;

            SetVertexPosition(mesh, i, rotatedPos);
        }

        mesh.RefreshMesh();
        mesh.gameObject.transform.rotation = Quaternion.identity;
        mesh.gameObject.transform.transform.localScale = Vector3.one;
        mesh.GetComponent<NetworkedMesh>().wasBake = true;
        // should be better way to do this
        mesh.gameObject.GetComponent<BoundsControl>().RecomputeBounds();
    }
}
