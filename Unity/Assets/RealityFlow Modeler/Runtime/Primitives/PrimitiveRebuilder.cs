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
        PrimitiveData data;
        size = Mathf.Max(minimumMeshSize, size);

        switch (target.baseShape)
        {
            case ShapeType.Plane:
                data = PrimitiveGenerator.CreatePlane(new Vector3(size, size, size));
                break;
            case ShapeType.Cube:
                data = PrimitiveGenerator.CreateCube(new Vector3(size, size, size));
                break;
            case ShapeType.Wedge:
                data = PrimitiveGenerator.CreateWedge(new Vector3(size, size, size));
                break;
            case ShapeType.Cylinder:
                data = PrimitiveGenerator.CreateCylinder(16, 1, size);
                break;
            case ShapeType.Cone:
                data = PrimitiveGenerator.CreateCone(16, size);
                break;
            case ShapeType.Sphere:
                data = PrimitiveGenerator.CreateUVSphere(8, 8, size);
                break;
            case ShapeType.Torus:
                data = PrimitiveGenerator.CreateTorus(8, 8, size * 2, size);
                break;
            case ShapeType.Pipe:
                data = PrimitiveGenerator.CreatePipe(8, 0, 0.5f * size, size, 0.2f * size);
                break;
            default:
                return;
        }

        UpdateMesh(target, data);
    }

    private static void UpdateMesh(EditableMesh target, PrimitiveData newMesh)
    {
        target.CreateMesh(newMesh);

        // Adjust bounds visuals to the finalized size
        target.GetComponent<BoundsControl>().RecomputeBounds();
    }
}
