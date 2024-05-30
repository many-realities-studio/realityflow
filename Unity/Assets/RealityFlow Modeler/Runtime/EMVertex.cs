using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class Vertex stores data for vertices of a mesh
/// </summary>
public class EMVertex : MonoBehaviour
{
    [SerializeField]
    Vector3 m_Position;

    [SerializeField]
    Vector3 m_Normal;

    [SerializeField]
    Vector4 m_Tangent;

    [SerializeField]
    Vector2 m_UV0;

    public Vector3 position
    {
        get { return m_Position; }
        set { m_Position = value; }
    }

    public Vector3 normal
    {
        get { return m_Normal; }
        set { m_Normal = value; }
    }

    public Vector4 tangent
    {
        get { return m_Tangent; }
        set { m_Tangent = value; }
    }

    public Vector2 uv0
    {
        get { return m_UV0; }
        set { m_UV0 = value; }
    }
}
