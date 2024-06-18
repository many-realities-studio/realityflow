using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class HandleSpawner handles spawning handles on a mesh
/// </summary>
public class HandleSpawner : MonoBehaviour
{
    public static HandleSpawner Instance { get; private set; }

    [SerializeField] public GameObject vertexHandle;
    [SerializeField] public GameObject edgeHandle;
    [SerializeField] public GameObject faceHandle;
    public bool hanldeSelectorIsActive;

    private EditableMesh em;
    private List<Handle> handles;

    public ManipulationMode mode { get; private set; }

    private XrayTool xrayTool;
    private ManipulationTool manipulationTool;

    public bool xrayActive => xrayTool.isActive;
    public bool manipulationActive => manipulationTool.isActive;

    private GameObject leftHand;
    private GameObject rightHand;
    private XRRayInteractor rayInteractor;
    private RaycastHit currentHitResult;

    

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        currentHitResult = new RaycastHit();
        leftHand = GameObject.Find("MRTK XR Rig/Camera Offset/MRTK LeftHand Controller");
        rightHand = GameObject.Find("MRTK XR Rig/Camera Offset/MRTK RightHand Controller");
        rayInteractor = rightHand.GetComponentInChildren<MRTKRayInteractor>();

        handles = new List<Handle>();

        PaletteHandManager.OnHandChange += SwitchHands;

        GameObject tools = GameObject.Find("RealityFlow Editor");
        xrayTool = tools.GetComponent<XrayTool>();
        manipulationTool = tools.GetComponent<ManipulationTool>();
    }

    void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
    }

    private void DestroyHandles()
    {
        if (handles == null)
            return;
        for (int i = 0; i < handles.Count; i++)
        {
            GameObject.Destroy(handles[i].gameObject);
        }
    }

    private void InvalidateHandleCache()
    {
        DestroyHandles();
        handles.Clear();
        HandleSelectionManager.Instance.ClearSelectedHandlesAndVertices();
    }

    private void OnSelectedMeshChange(EditableMesh selectedMesh)
    {
        if (em == null)
            return;
        if (em == selectedMesh)
            return;

        em.gameObject.GetComponent<MeshCollider>().enabled = true;
        em.gameObject.GetComponent<BoundsControl>().HandlesActive = false;
        em.gameObject.GetComponent<NetworkedMesh>().ControlSelection();
        em.gameObject.GetComponent<MeshVisulization>().SetMeshMaterialOpaque();
    }

    public void SetManipulationMode(ManipulationMode mode)
    {
        InvalidateHandleCache();

        if(em != null)
        {
            if (manipulationTool.mode != ManipulationMode.mObject)
            {
                SpawnHandles();
            }
            else
            { 
                em.gameObject.GetComponent<MeshCollider>().enabled = true;
                em.gameObject.GetComponent<BoundsControl>().HandlesActive = false;
                em.gameObject.GetComponent<NetworkedMesh>().ControlSelection();
                em.gameObject.GetComponent<MeshVisulization>().SetMeshMaterialOpaque();
                em = null;
            }
        }
    }

    #region HandleSpawning
    private void SpawnHandles()
    {
        if(em == null)
        {
            Debug.LogError("Mesh is null!");
            return;
        }

        em.gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.None;
        em.gameObject.GetComponent<MeshCollider>().enabled = false;
        em.gameObject.GetComponent<BoundsControl>().HandlesActive = true;
        em.gameObject.GetComponent<NetworkedMesh>().ControlSelection();

        if (manipulationTool.mode == ManipulationMode.vertex)
        {
            DisplayVertexHandles();
        }
        else if (manipulationTool.mode == ManipulationMode.edge)
        {
            DisplayEdgeHandles();
        }
        else if (manipulationTool.mode == ManipulationMode.face)
        {
            DisplayFaceHandles();
        }
    }

    /// <summary>
    /// Displays vertex handles at each vertex of the mesh
    /// </summary>
    public void DisplayVertexHandles()
    {
        if (em == null)
        {
            return;
        }

        InvalidateHandleCache();

        Vector3[] localPos = em.GetUniqueVertexPositions();
        Vector3[] points = em.GetVerticesInWorldSpace(localPos);

        for (int i = 0; i < points.Length; i++)
        {
            GameObject go = GameObject.Instantiate(vertexHandle, points[i], Quaternion.identity);
            go.name = "Vertex Handle " + i;
            VertexHandle handle = go.GetComponent<VertexHandle>();
            handle.mesh = em;

            int sharedVertIndex = EMSharedVertex.GetSharedVertexIndexFromPosition(em, localPos[i]);
            handle.sharedVertexIndex = sharedVertIndex;

            handles.Add(handle);
        }
    }

    /// <summary>
    /// Displays edge handles using line renderers, two end points are set to world space
    /// positions of two endpoints
    /// </summary>
    public void DisplayEdgeHandles()
    {
        InvalidateHandleCache();

        EMFace[] faces = em.faces;
        EMEdge[] faceEdges;
        HashSet<EMEdge> edges = new HashSet<EMEdge>();
        Vector3[] positions = new Vector3[2];

        for (int i = 0; i < faces.Length; i++)
        {
            faceEdges = faces[i].GetExteriorEdgesWithSharedIndicies(em);
            for (int j = 0; j < faceEdges.Length; j++)
            {
                edges.Add(faceEdges[j]);
            }
        }

        int count = 0;
        foreach (var edge in edges)
        {
            positions[0] = em.GetVertexInWorldSpace(em.sharedVertices[edge.A].vertices[0]);
            positions[1] = em.GetVertexInWorldSpace(em.sharedVertices[edge.B].vertices[0]);

            GameObject go = GameObject.Instantiate(edgeHandle, Vector3.zero, Quaternion.identity);
            go.name = "Edge Handle " + count++;
            EdgeHandle handle = go.GetComponent<EdgeHandle>();
            handle.mesh = em;
            handle.SetIndicies(edge.A, edge.B);
            handle.UpdateMeshTransform();

            handles.Add(handle);
        }
    }

    public void DisplayFaceHandles()
    {
        InvalidateHandleCache();

        EMFace[] faces = em.faces;
        int[] vertexIndicies;

        int[] indices;

        for (int i = 0; i < faces.Length; i++)
        {
            vertexIndicies = faces[i].indicies;
            indices = faces[i].GetReducedIndicies();

            GameObject go = GameObject.Instantiate(faceHandle, em.transform.position, em.transform.rotation);
            go.name = "Face Handle " + i;
            FaceHandle fh = go.GetComponent<FaceHandle>();
            fh.mesh = em;
            fh.faceIndex = i;
            fh.SetPositions(vertexIndicies, indices);

            handles.Add(fh);
        }

    }
    #endregion

    /// <summary>
    /// Updates the position of the handles to account of any transformations on the base mesh
    /// </summary>
    public void UpdateHandlePositions()
    {
        for(int i = 0; i < handles.Count; i++)
        {
            handles[i].UpdateHandlePosition();
        }
    }

    /// <summary>
    /// Reverses the winding order of the face handles 
    /// </summary>
    public void ReverseFaceHandlesDirection()
    {
        if (manipulationTool.mode != ManipulationMode.face)
            return;

        FaceHandle fh;
        for(int i = 0; i < handles.Count; i++)
        {
            fh = handles[i].GetComponent<FaceHandle>();
            fh.ReverseFaceWindingOrder();
        }
    }

    public void EnterXrayMode()
    {
        if (!xrayTool.isActive)
            return;

        ReverseFaceHandlesDirection();
        if(em != null)
        {
            em.gameObject.GetComponent<MeshVisulization>().SetMeshMaterialTransparent();
        }
    }

    public void ExitXrayMode()
    {
        if (xrayTool.isActive)
            return;

        ReverseFaceHandlesDirection();
        if(em!=null)
        {
            em.gameObject.GetComponent<MeshVisulization>().SetMeshMaterialOpaque();
        }
    }

    private void GetRayCollision()
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out currentHitResult);

        if (currentHitResult.collider == null)
            return;

        MRTKBaseInteractable interactable = currentHitResult.transform.gameObject.GetComponent<MRTKBaseInteractable>();
        if (interactable != null && interactable.isSelected)
        {
            EditableMesh selectedMesh = currentHitResult.transform.gameObject.GetComponent<EditableMesh>();
            if (selectedMesh != null)
            {
                OnSelectedMeshChange(selectedMesh);
                em = selectedMesh;
                SpawnHandles();
                if(xrayTool.isActive)
                {
                    em.gameObject.GetComponent<MeshVisulization>().SetMeshMaterialTransparent();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (manipulationTool.mode == ManipulationMode.mObject)
            return;

        GetRayCollision();
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the interactor rays and triggers depending on the dominant hand
        if (isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<MRTKRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<MRTKRayInteractor>();
        }
    }
}
