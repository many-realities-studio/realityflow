using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class MeshVisualization displays handles for the different components of a mesh.
/// </summary>
public class MeshVisulization : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    [SerializeField]
    public GameObject vertexHandle;

    [SerializeField]
    public GameObject edgeHandle;

    [SerializeField]
    public GameObject faceHandle;

    private EditableMesh em;
    private GameObject[] handles;

    public ManipulationMode mode { get; private set; }
    private ManipulationMode lastMode;
    private bool lastActiveState;


    private SelectToolManager selectToolManager;

    [SerializeField, Range(0f, 1f)]
    private float alphaAmount = 0.4f;

    private float currentMetallicValue;
    private float currentGlossyValue;

    void Awake()
    {
        selectToolManager = gameObject.GetComponent<SelectToolManager>();   
        em = gameObject.GetComponent<EditableMesh>();

        InvalidateHandleCache();

        MeshManipulationModes.OnManipulationModeChange += SetManipulationMode;
        //TryGetMeshRenderer();
    }

    private void TryGetMeshRenderer()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void SetManipulationMode(ManipulationMode mode)
    {
        InvalidateHandleCache();
        this.mode = mode;

        // if (mode == ManipulationMode.mObject)
        // {
        //     gameObject.GetComponent<MeshCollider>().enabled = true;
        // }
    }

    public void OnMeshSelected()
    {
        // Debug.LogError("Mesh Selected Occurs");
        // Freeze mesh transformations for now
        //Debug.Log("Freeze the mesh. The manipulate tool is set to " + selectToolManager.manipulationTool.isActive);
        gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.None;
        //gameObject.GetComponent<MeshCollider>().enabled = false;

        gameObject.GetComponent<MeshCollider>().enabled = false;
        gameObject.GetComponent<BoundsControl>().HandlesActive = true;
        gameObject.GetComponent<NetworkedMesh>().ControlSelection();

        if (mode == ManipulationMode.vertex)
        {
            // if (lastMode != mode)
            // {
            //     Debug.Log("Last mode was not vertex so enable mesh collider");
            //     gameObject.GetComponent<MeshCollider>().enabled = true;
            // }
            lastMode = mode;

            DisplayVertexHandles();
        }
        else if (mode == ManipulationMode.edge)
        {
            // if (lastMode != mode)
            // {
            //     gameObject.GetComponent<MeshCollider>().enabled = true;
            // }
            lastMode = mode;

            DisplayEdgeHandles();
        }
        else if (mode == ManipulationMode.face)
        {
            // if (lastMode != mode)
            // {
            //     gameObject.GetComponent<MeshCollider>().enabled = true;
            // }
            lastMode = mode;

            DisplayFaceHandles();
        }
    }

    #region Display Handles
    public void DisplayVertexHandles()
    {
        if (em == null)
        {
            return;
        }

        InvalidateHandleCache();

        Vector3[] localPos = em.GetUniqueVertexPositions();
        Vector3[] points = em.GetVerticesInWorldSpace(localPos);
        List<GameObject> h = new List<GameObject>();

        for (int i = 0; i < points.Length; i++)
        {
            GameObject go = GameObject.Instantiate(vertexHandle, points[i], Quaternion.identity);
            VertexHandle handle = go.GetComponent<VertexHandle>();
            handle.mesh = em;

            int sharedVertIndex = EMSharedVertex.GetSharedVertexIndexFromPosition(em, localPos[i]);
            handle.sharedVertexIndex = sharedVertIndex;

            h.Add(go);
        }

        handles = h.ToArray();
    }

    public void DisplayEdgeHandles()
    {
        InvalidateHandleCache();

        EMFace[] faces = em.faces;
        EMEdge[] faceEdges;
        HashSet<EMEdge> edges = new HashSet<EMEdge>();
        List<GameObject> h = new List<GameObject>();
        Vector3[] positions = new Vector3[2];

        for (int i = 0; i < faces.Length; i++)
        {
            faceEdges = faces[i].GetExteriorEdgesWithSharedIndicies(em);
            for(int j = 0; j < faceEdges.Length; j++)
            {
                edges.Add(faceEdges[j]);
            }
        }

        foreach(var edge in edges)
        {
            positions[0] = em.GetVertexInWorldSpace(em.sharedVertices[edge.A].vertices[0]);
            positions[1] = em.GetVertexInWorldSpace(em.sharedVertices[edge.B].vertices[0]);

            //positions[0] = em.positions[em.sharedVertices[edge.A].vertices[0]];
            //positions[1] = em.positions[em.sharedVertices[edge.B].vertices[0]];

            GameObject go = GameObject.Instantiate(edgeHandle, Vector3.zero, Quaternion.identity);
            //LineRenderer lr = go.GetComponent<LineRenderer>();

            //lr.SetPositions(positions);
            EdgeHandle handle = go.GetComponent<EdgeHandle>();
            handle.mesh = em;
            handle.SetIndicies(edge.A, edge.B);
            handle.UpdateMeshTransform();

            h.Add(go);
        }
        handles = h.ToArray();
    }

    public void DisplayFaceHandles()
    {
        InvalidateHandleCache();

        EMFace[] faces = em.faces;
        List<GameObject> h = new List<GameObject>();
        int[] vertexIndicies;

        int[] indices;

        for (int i = 0; i < faces.Length; i++)
        {
            vertexIndicies = em.faces[i].indicies;
            indices = faces[i].GetReducedIndicies();

            GameObject go = GameObject.Instantiate(faceHandle, em.transform.position, em.transform.rotation);
            FaceHandle fh = go.GetComponent<FaceHandle>();
            fh.mesh = em;
            fh.faceIndex = i;
            fh.SetPositions(vertexIndicies, indices);

            h.Add(go);
        }

        handles = h.ToArray();
    }
    #endregion

    #region Update Handles
    public void UpdateHandlePositions()
    {
        if (mode == ManipulationMode.vertex)
        {
            UpdateVertexHandles();
        }
        else if (mode == ManipulationMode.edge)
        {
            UpdateEdgeHandles();
        }
        else if (mode == ManipulationMode.face)
        {
            UpdateFaceHandles();
        }
    }

    public void UpdateVertexHandles()
    {
        VertexHandle vh;

        for (int i = 0;  i < handles.Length; i++)
        {
            vh = handles[i].GetComponent<VertexHandle>();
            vh.UpdateHandlePosition();
        }
    }

    private void UpdateEdgeHandles()
    {
        EdgeHandle eh;

        for(int i = 0; i < handles.Length; i++)
        {
            eh = handles[i].GetComponent<EdgeHandle>();
            eh.UpdateHandlePosition();
            //eh.UpdateMeshTransform();
        }
    }

    public void UpdateFaceHandles()
    {
        for (int i = 0; i < handles.Length; i++)
        {
            FaceHandle fh = handles[i].GetComponent<FaceHandle>();
            //fh.UpdateFacePosition();
            fh.UpdateHandlePosition();
        }
    }
    #endregion

    #region Destroy and Invalidate Handles
    private void DestroyHandles()
    {
        if (handles == null)
            return;
        for(int i = 0; i < handles.Length; i++)
        {
            GameObject.Destroy(handles[i].gameObject);
        }
    }

    private void InvalidateHandleCache()
    {
        DestroyHandles();
        handles = null;
    }

    #endregion

    void Update()
    {
        // if (lastMode != mode && selectToolManager.manipulationTool.isActive)
        // {
        //     Debug.Log("Turn handles OFF on mode switch");
        //     gameObject.GetComponent<MeshCollider>().enabled = true;
        //     gameObject.GetComponent<BoundsControl>().HandlesActive = false;
        //     gameObject.GetComponent<NetworkedMesh>().ControlSelection();
        //     lastMode = mode;
        // }

        if (selectToolManager.manipulationTool.isActive && gameObject.GetComponent<MRTKBaseInteractable>().IsRaySelected)
        {
            OnMeshSelected();
            lastActiveState = true;
        }

        if (lastActiveState == true && !selectToolManager.manipulationTool.isActive)
        {
            //Debug.Log("Turn handles OFF");
            lastActiveState = false;
            gameObject.GetComponent<MeshCollider>().enabled = true;
            gameObject.GetComponent<BoundsControl>().HandlesActive = false;
            gameObject.GetComponent<NetworkedMesh>().ControlSelection();
        }
    }

    #region Mesh Material Updates
    private void CacheMetallicAndGlossyValues()
    {
        Material mat = gameObject.GetComponent<MeshRenderer>().material;

        currentMetallicValue = mat.GetFloat("metallicFactor");
        currentGlossyValue = mat.GetFloat("roughnessFactor");
    }

    public void SetMeshMaterialOpaque()
    {
        if (meshRenderer == null)
            TryGetMeshRenderer();

        Material mat = gameObject.GetComponent<MeshRenderer>().material;
        if (mat == null)
            return;

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.SetFloat("metallicFactor", currentMetallicValue);
        mat.SetFloat("roughnessFactor", currentGlossyValue);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 2000;

        gameObject.GetComponent<MeshRenderer>().material = mat;
    }
    public void SetMeshMaterialTransparent()
    {
        if (meshRenderer == null)
            TryGetMeshRenderer();

        Material mat = gameObject.GetComponent<MeshRenderer>().material;
        if (mat == null)
            return;

        CacheMetallicAndGlossyValues();
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.SetFloat("metallicFactor", 0);
        
        // not sure the shader supports the below calls
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color newColor = mat.color;
        newColor.a = alphaAmount;
        mat.color = newColor;
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }
    #endregion
}
