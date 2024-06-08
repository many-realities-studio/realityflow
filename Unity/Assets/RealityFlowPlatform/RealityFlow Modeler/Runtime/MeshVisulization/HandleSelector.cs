using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class HandleSelector handles spawning handles on a mesh
/// </summary>
public class HandleSelector : MonoBehaviour
{
    [SerializeField] public GameObject vertexHandle;
    [SerializeField] public GameObject edgeHandle;
    [SerializeField] public GameObject faceHandle;
    public bool hanldeSelectorIsActive;

    private EditableMesh em;
    private List<Handle> handles;

    public ManipulationMode mode { get; private set; }
    private SelectToolManager selectToolManager;

    private GameObject leftHand;
    private GameObject rightHand;
    private XRRayInteractor rayInteractor;
    private RaycastHit currentHitResult;

    // Start is called before the first frame update
    void Start()
    {
        currentHitResult = new RaycastHit();
        if(rightHand == null) {
            rightHand = GameObject.Find("MRTK XR Rig/Camera Offset/MRTK RightHand Controller");
        }
        // Debug.Log(rightHand);
        if(leftHand == null) {
            leftHand = GameObject.Find("MRTK XR Rig/Camera Offset/MRTK LeftHand Controller");
        }
        // leftHand = GameObject.Find("MRTK LeftHand Controller");
        // rightHand = GameObject.Find("MRTK RightHand Controller");
        Debug.Log(rightHand);
        rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();

        handles = new List<Handle>();

        PaletteHandManager.OnHandChange += SwitchHands;
        MeshManipulationModes.OnManipulationModeChange += SetManipulationMode;
    }

    void OnDestroy()
    {
        PaletteHandManager.OnHandChange -= SwitchHands;
        MeshManipulationModes.OnManipulationModeChange -= SetManipulationMode;
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

    private void SetManipulationMode(ManipulationMode mode)
    {
        InvalidateHandleCache();
        this.mode = mode;

        if(em != null)
        {
            em.gameObject.GetComponent<MeshCollider>().enabled = true;
            em.gameObject.GetComponent<BoundsControl>().HandlesActive = false;
            em.gameObject.GetComponent<NetworkedMesh>().ControlSelection();
            em = null;
        }
    }

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

        if (mode == ManipulationMode.vertex)
        {
            DisplayVertexHandles();
        }
        else if (mode == ManipulationMode.edge)
        {
            DisplayEdgeHandles();
        }
        else if (mode == ManipulationMode.face)
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

        foreach (var edge in edges)
        {
            positions[0] = em.GetVertexInWorldSpace(em.sharedVertices[edge.A].vertices[0]);
            positions[1] = em.GetVertexInWorldSpace(em.sharedVertices[edge.B].vertices[0]);

            //positions[0] = em.positions[em.sharedVertices[edge.A].vertices[0]];
            //positions[1] = em.positions[em.sharedVertices[edge.B].vertices[0]];

            GameObject go = GameObject.Instantiate(edgeHandle, Vector3.zero, Quaternion.identity);
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
            FaceHandle fh = go.GetComponent<FaceHandle>();
            fh.mesh = em;
            fh.faceIndex = i;
            fh.SetPositions(vertexIndicies, indices);

            handles.Add(fh);
        }
    }

    public void UpdateHandlePositions()
    {
        for(int i = 0; i < handles.Count; i++)
        {
            handles[i].UpdateHandlePosition();
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
                Debug.Log("adding mesh " + selectedMesh.gameObject.name + " to visualization");
                em = selectedMesh;
                SpawnHandles();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == ManipulationMode.mObject)
            return;

        GetRayCollision();
    }

    private void SwitchHands(bool isLeftHandDominant)
    {
        // Switch the interactor rays and triggers depending on the dominant hand
        if (isLeftHandDominant)
        {
            rayInteractor = leftHand.GetComponentInChildren<XRRayInteractor>();
        }
        else
        {
            rayInteractor = rightHand.GetComponentInChildren<XRRayInteractor>();
        }
    }
}
