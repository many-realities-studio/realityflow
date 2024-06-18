using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentTranslation : MeshOperation
{
    public int[] indices { get; private set; }
    public Vector3 amount { get; private set; }

    public bool isObject => indices.Length == 0;

    public ComponentTranslation()
    {
        amount = Vector3.zero;
    }

    public ComponentTranslation(int[] selectedIndices)
    {
        amount= Vector3.zero;
        indices = new int[selectedIndices.Length];

        selectedIndices.CopyTo(indices, 0);
    }

    public void AddOffsetAmount(Vector3 offset)
    {
        amount += offset;
    }

    public MeshOperations GetOperationType()
    {
        return MeshOperations.Translate;
    }

    public void Execute(EditableMesh em)
    {
        if(isObject)
        {
            em.gameObject.transform.position += amount;
        }
        else
        {
            em.TransformVertices(indices, amount);
        }
    }
}
