using UnityEngine;

[System.Serializable]
public class RfObject
{
    public string id; // Unique ID for each object
    public string projectId;
    public string name;
    public string type;
    public string transformJson;
    public string meshJson;
}

[System.Serializable]
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Vector2[] uv;
}

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

