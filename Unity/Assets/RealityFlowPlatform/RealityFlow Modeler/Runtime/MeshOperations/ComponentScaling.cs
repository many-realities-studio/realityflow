using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentScaling : MeshOperation
{
    public int[] indices { get; private set; }
    public Vector3 scale { get; private set; }

    public bool isObject => indices.Length == 0;

    public ComponentScaling()
    {
        scale = Vector3.one;
    }

    public ComponentScaling(int[] selectedIndices)
    {
        scale = Vector3.one;
        indices = new int[selectedIndices.Length];

        selectedIndices.CopyTo(indices, 0);
    }

    public void AddOffsetAmount(Vector3 offset)
    {
        scale = offset;
    }

    public MeshOperations GetOperationType()
    {
        return MeshOperations.Scale;
    }

    public void Execute(EditableMesh em)
    {
        if(isObject)
        {
            em.gameObject.transform.localScale += scale;
        }
        else
        {
            em.ScaleVertices(indices, scale);
        }
    }
}
