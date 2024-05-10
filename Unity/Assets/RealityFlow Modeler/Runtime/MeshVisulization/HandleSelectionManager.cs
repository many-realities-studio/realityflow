using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandleSelectionManager : MonoBehaviour
{
    public static HandleSelectionManager Instance { get; private set; }
    public GameObject manipulator;
    public EditableMesh mesh;

    private List<Handle> selectedHandles;
    public static List<int> selectedIndices;
    public HandleSelector handleSelector { get; private set; }
    private GameObject spawnedManipulator;
    public List<int> indicies() { return selectedIndices; }

    public AttachGizmoState gizmoTool;
    private Vector3 selectionCentroid;

    public Color defaultColor;
    public Color OnHoverColor;
    public Color OnSelectColor;

    private void Start()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        selectedHandles = new List<Handle>();
        selectedIndices = new List<int>();

        GameObject tools = GameObject.Find("RealityFlow Editor");
        gizmoTool = tools.GetComponent<AttachGizmoState>();

        GameObject selectManager = GameObject.Find("Component Select Manager");
        handleSelector = selectManager.gameObject.GetComponent<HandleSelector>();
        
        if(handleSelector == null)
        {
            Debug.Log("Handle selector not found");
        }
    }

    public void SelectHandle(Handle handle)
    {
        mesh = handle.mesh;
        selectedHandles.Add(handle);
        AppendSelectedVertices(handle);
        UpdateCentroidPosition();
    }

    private void AppendSelectedVertices(Handle handle)
    {
        int[] indicies = handle.GetSharedVertexIndicies();
        selectedIndices.AddRange(indicies);
    }

    public void RemoveSelectedHandle(Handle handle)
    {
        try
        {
            selectedHandles.Remove(handle);
            RemoveSelectedVertices(handle);
            UpdateCentroidPosition();
            if(selectedHandles.Count <= 0)
            {
                if (spawnedManipulator != null)
                {
                    spawnedManipulator.GetComponent<ComponentSelectManipulator>().RemoveGizmo();
                }
            }
        }
        catch
        {
            Debug.LogError("Handle not in list");
        }

    }

    private void RemoveSelectedVertices(Handle handle)
    {
        int[] indicies = handle.GetSharedVertexIndicies();
        try
        {
            for(int i = 0; i < indicies.Length; i++)
            {
                selectedIndices.Remove(indicies[0]);
            }
        }
        catch
        {
            Debug.LogError("Vertex not in list!");
        }
    }

    private void UpdateManipulatorPosition()
    {
        if(spawnedManipulator == null)
        {
            spawnedManipulator = GameObject.Instantiate(manipulator);
        }

        spawnedManipulator.GetComponent<ComponentSelectManipulator>().SafeUpdatePosition(selectionCentroid);
    }

    private void UpdateCentroidPosition()
    {
        if(selectedHandles.Count <= 0)
        {
            selectionCentroid = Vector3.zero;
            return;    
        }

        Vector3 center = CalculateCentroidPosition();
        selectionCentroid = mesh.transform.TransformPoint(center);
        UpdateManipulatorPosition();
    }

    private Vector3 CalculateCentroidPosition()
    {
        Vector3 centroid = Vector3.zero;
        for(int i = 0; i < selectedIndices.Count; i++)
        {
            int index = mesh.sharedVertices[selectedIndices[i]].vertices[0];
            centroid += mesh.positions[index];
        }

        for(int i = 0; i < selectedHandles.Count; i++)
        {
            FaceHandle fh = selectedHandles[i].GetComponent<FaceHandle>();
        }

        return centroid / selectedIndices.Count;
    }

    public void ClearSelectedHandlesAndVertices()
    {
        mesh = null;
        selectedHandles.Clear();
        selectedIndices.Clear();
        selectionCentroid = Vector3.zero;
        if(spawnedManipulator != null)
        {
            spawnedManipulator.GetComponent<ComponentSelectManipulator>().RemoveGizmo();
        }
    }

    private void Update()
    {

    }
}
