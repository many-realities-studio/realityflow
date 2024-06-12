using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CentroidCalculator
{
    public static Vector3 FindCentroidFromSharedVertices(EditableMesh mesh, int[] indicies)
    {
        Vector3 centroid = Vector3.zero;
        int index = 0;
        for(int i = 0; i < indicies.Length; i++)
        {
            index = mesh.sharedVertices[indicies[i]].vertices[0];
            centroid += mesh.positions[index];
        }

        return centroid / indicies.Length;
    }

    /// <summary>
    /// Computes the centroid of a set of vertices
    /// </summary>
    public static Vector3 FindCentroidFromVertices(EditableMesh mesh, int[] indicies)
    {
        Vector3 centroid = Vector3.zero;

        for (int i = 0; i < indicies.Length; i++)
        {
            centroid += mesh.positions[indicies[i]];
        }

        return centroid / indicies.Length;
    }

    public static Vector3 FindCentroidFromEdges(EditableMesh mesh, EdgeHandle[] edges)
    {
        Vector3 centroid = Vector3.zero;
        int[] positions = new int[2];
        for (int i = 0; i < edges.Length; i++)
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
}
