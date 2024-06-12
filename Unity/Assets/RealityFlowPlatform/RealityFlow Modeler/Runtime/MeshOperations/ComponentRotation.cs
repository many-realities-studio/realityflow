using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentRotation : MeshOperation
{
    public int[] indices { get; private set; }
    public Quaternion amount { get; private set; }

    public bool isObject => indices.Length == 0;

    public ComponentRotation()
    {
        amount = Quaternion.identity;
    }

    public ComponentRotation(int[] selectedIndices)
    {
        amount= Quaternion.identity;
        indices = new int[selectedIndices.Length];

        selectedIndices.CopyTo(indices, 0);
    }

    public void AddOffsetAmount(Quaternion offset)
    {
        amount = offset;
    }

    public MeshOperations GetOperationType()
    {
        return MeshOperations.Rotate;
    }

    public void Execute(EditableMesh em)
    {
        if(isObject)
        {
            em.gameObject.transform.rotation = amount * em.gameObject.transform.rotation;
        }
        else
        {
            em.RotateVertices(indices, amount);
        }
    }
}
