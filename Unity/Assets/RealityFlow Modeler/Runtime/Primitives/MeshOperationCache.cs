using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class MeshOperationCache stores the operations that modified a mesh. Transforms can be though of
/// as a sum of transforms for each vertex.
/// </summary>
public class MeshOperationCache
{
    public bool isEmpty;

    public List<MeshOperation> operations;

    private EditableMesh mesh;

    public MeshOperationCache(EditableMesh em)
    {
        operations = new List<MeshOperation>();
        isEmpty = true;

        mesh = em;
    }

    public void CacheOperation(MeshOperation operation)
    {
        operations.Add(operation);
        if(operations.Count > 0)
            isEmpty = false;
    }
}
