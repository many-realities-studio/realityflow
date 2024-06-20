using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ubiq.Spawning;
using Ubiq.Messaging;
using Newtonsoft.Json;
using UnityEngine;

public class SerializableMeshInfo
{

    public float[] vertices;

    public int[][] faces;

    public int shapeID;

    public float[] positions;

    public float[] rotation;

    public float[] scale;

    public float lastSize;

    public float[] colors;

    public bool colorFlag;

    public float metalFactor;

    public bool metalFlag;

    public float glossFactor;

    public bool glossFlag;

    public string objectID;

    public SerializableMeshInfo() { }

    public SerializableMeshInfo(GameObject go)
    {
        EditableMesh em = go.GetComponent<EditableMesh>();

        NetworkedMesh nm = go.GetComponent<NetworkedMesh>();

        lastSize = nm.GetLastSize();

        Material m = go.GetComponent<Renderer>().material;

        objectID = ("" + go.GetComponent<NetworkedMesh>().NetworkId);

        //Debug.Log("ID: " + objectID);

        if (m.color != null)
        {
            Debug.Log(m.color);
            colorFlag = true;
            colors = new float[4] { m.color.r, m.color.g, m.color.b, m.color.a };

        }
        else
        {
            colorFlag = false;
        }

        if (m.GetFloat("_Metallic") != 0)
        {
            metalFlag = true;
            metalFactor = m.GetFloat("_Metallic");
            Debug.Log("MetalFactor: " + metalFactor);
        }
        else
        {
            metalFlag = false;
        }

        if (m.GetFloat("_Glossiness") != 0)
        {
            glossFlag = true;
            glossFactor = m.GetFloat("_Glossiness");
        }
        else
        {
            glossFlag = false;
        }

        Transform t = go.transform;

        positions = new float[3] { t.localPosition.x, t.localPosition.y, t.localPosition.z };

        rotation = new float[4] { t.localRotation.x, t.localRotation.y, t.localRotation.z, t.localRotation.w };

        scale = new float[3] { t.localScale.x, t.localScale.y, t.localScale.z };

        vertices = new float[em.positions.Length * 3]; // initialize vertices array.
        for (int i = 0; i < em.positions.Length; i++) // Serialization: Vector3's values are stored sequentially.
        {
            vertices[i * 3] = em.positions[i].x;
            vertices[i * 3 + 1] = em.positions[i].y;
            vertices[i * 3 + 2] = em.positions[i].z;
        }



        faces = new int[em.faces.Length][];

        for (int i = 0; i < em.faces.Length; i++)
        {

            faces[i] = em.faces[i].indicies;
            /*faces[i] = new int[em.faces[i].indicies.Length];
            for (int j = 0; i < em.faces[i].indicies.Length; i++)
            {
                Debug.Log("Before faces");
                faces[i][j] = em.faces[i].indicies[j];
            }*/
        }

        shapeID = getShapeID(em.baseShape);
    }

    /*public void SpawnObject(GameObject go)
    {
        //GameObject go = NetworkSpawnManager.Find(this).SpawnWithPeerScope(primitive);

        EditableMesh mesh = go.GetComponent<EditableMesh>();

        EditableMesh em = GetEM();

        mesh.CreateMesh(em);

        Object.Destroy(em.gameObject);

        go.transform.localPosition = GetPosition();

        go.transform.localRotation = GetRotation();

        go.transform.localScale = GetScale();

        //Debug.Log(colorFlag);

        if (colorFlag)
        {
            go.GetComponent<Renderer>().material.SetColor("_Color", GetColor());//smi.colors.GetColor());
        }

        if (metalFlag)
        {
            go.GetComponent<Renderer>().material.SetFloat("_Metallic", metalFactor);
        }

        if (glossFlag)
        {
            go.GetComponent<Renderer>().material.SetFloat("_Glossiness", glossFactor);
        }

        NetworkedMesh nm = go.GetComponent<NetworkedMesh>();

        nm.SetLastSize(lastSize);

        //return go;
    }*/
    /*
    public EditableMesh GetEM()
    {
        Vector3[] positions = new Vector3[vertices.Length / 3];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector3(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]);
        }

        EMFace[] EMfaces = new EMFace[faces.Length];

        for (int i = 0; i < faces.Length; i++)
        {
            EMfaces[i] = new EMFace(faces[i]);
        }

        ShapeType baseShape = getShape();

        EditableMesh mesh = EditableMesh.CreateMesh(positions, EMfaces);

        mesh.baseShape = baseShape;

        return mesh;
    }*/

    public Vector3 GetPosition()
    {
        return new Vector3(positions[0], positions[1], positions[2]);
    }

    public Quaternion GetRotation()
    {
        return new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);
    }

    public Vector3 GetScale()
    {
        return new Vector3(scale[0], scale[1], scale[2]);
    }

    public Color GetColor()
    {
        return new Color(colors[0], colors[1], colors[2], colors[3]);
    }

    private int getShapeID(ShapeType type)
    {
        switch (type)
        {
            case ShapeType.NoShape:
                return 0;
            case ShapeType.Plane:
                return 1;
            case ShapeType.Cube:
                return 2;
            case ShapeType.Cylinder:
                //return CreateCylinder(20, 1);
                return 3;
            case ShapeType.Cone:
                return 4;
            case ShapeType.Sphere:
                return 5;
            case ShapeType.Torus:
                return 6;
            default:
                Debug.LogError("Invalid ShapeType input!");
                break;
        }
        return 2;
    }

    public ShapeType getShape()
    {
        switch (shapeID)
        {
            case 0:
                return ShapeType.NoShape;
            case 1:
                return ShapeType.Plane;
            case 2:
                return ShapeType.Cube;
            case 3:
                //return CreateCylinder(20, 1);
                return ShapeType.Cylinder;
            case 4:
                return ShapeType.Cone;
            case 5:
                return ShapeType.Sphere;
            case 6:
                return ShapeType.Torus;
            default:
                Debug.LogError("Invalid ShapeType input!");
                break;
        }
        return ShapeType.Cube;
    }

}