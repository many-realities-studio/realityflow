using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;

/// <summary>
/// Class PrimitiveRebuilder calls methods to rebuild the target mesh based on size parameters
/// </summary>
public static class PrimitiveRebuilder
{
    public static float minimumMeshSize = 0.1f;
    public static void RebuildMesh(EditableMesh target, float size)
    {
        // Debug.Log("Resizing...");
        EditableMesh em;
        // PrimitiveData data; replace all instances of em with data
        size = Mathf.Max(minimumMeshSize, size);

        switch (target.baseShape)
        {
            case ShapeType.Plane:
                em = PrimitiveGenerator.CreatePlane(new Vector3(size, size, size));
                break;
            case ShapeType.Cube:
                em = PrimitiveGenerator.CreateCube(new Vector3(size, size, size));
                break;
            //case ShapeType.Wedge:
            //    em = PrimitiveGenerator.CreateWedge(new Vector3(size, size, size));
                // break;
            case ShapeType.Cylinder:
                em = PrimitiveGenerator.CreateCylinder(16, 1, size);
                break;
            case ShapeType.Cone:
                em = PrimitiveGenerator.CreateCone(16, size);
                break;
            case ShapeType.Sphere:
                em = PrimitiveGenerator.CreateUVSphere(8, 8, size);
                break;
            case ShapeType.Torus:
                em = PrimitiveGenerator.CreateTorus(8, 8, size * 2, size);
                break;
            //case ShapeType.Pipe:
            //   em = PrimitiveGenerator.CreatePipe(8, 0, 0.5f * size, size, 0.2f * size);
                // break;
            default:
                return;
        }

        UpdateMesh(target, em);
    }

    private static void UpdateMesh(EditableMesh target, EditableMesh newMesh)
    {
        newMesh.mesh = target.mesh;
        target.CreateMesh(newMesh);
        //target.RefreshMesh();
        Object.Destroy(newMesh.gameObject);

        // Adjust bounds visuals to the finalized size
        target.GetComponent<BoundsControl>().RecomputeBounds();
    }
}
