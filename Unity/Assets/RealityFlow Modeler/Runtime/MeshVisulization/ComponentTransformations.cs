using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ComponentTransformations
{
    public static void TranslateVertices(EditableMesh mesh, Vector3 offset)
    {
        List<int> vertices = HandleSelectionManager.Instance.indicies;
        mesh.TranslateVerticesWithNetworking(vertices.Distinct().ToArray(), offset);
    }
    
    public static void RotateVertices(EditableMesh mesh, Quaternion offset)
    {
        List<int> vertices = HandleSelectionManager.Instance.indicies;
        mesh.RotateVerticesWithNetworking(vertices.Distinct().ToArray(), offset);
    }

    public static void ScaleVertices(EditableMesh mesh, Vector3 newScale)
    {
        List<int> vertices = HandleSelectionManager.Instance.indicies;
        mesh.ScaleVerticesWithNetworking(vertices.Distinct().ToArray(), newScale);
    }
}
