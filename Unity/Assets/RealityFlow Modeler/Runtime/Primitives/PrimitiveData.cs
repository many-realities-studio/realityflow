using UnityEngine;

public struct PrimitiveData
{
    public Vector3[] positions;
    public EMFace[] faces;
    public ShapeType type;

    public PrimitiveData(Vector3[] positions, EMFace[] faces)
    {
        this.type = ShapeType.NoShape;
        this.positions = positions;
        this.faces = faces;
    }

    public PrimitiveData(ShapeType type, Vector3[] positions, EMFace[] faces)
    {
        this.type = type;
        this.positions = positions;
        this.faces = faces;
    }
}
