using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeshSelectionManager : MonoBehaviour
{
    public static MeshSelectionManager Instance { get; private set; }

    private List<GameObject> selectedMeshes = new List<GameObject>();
    public GameObject lastSelection;

    public UnityEvent<GameObject> ObjectSelected;

    private SelectTool selectTool;

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

        selectedMeshes = new List<GameObject>();

        try
        {
            GameObject tools = GameObject.Find("RealityFlow Editor");
            selectTool = tools.GetComponent<SelectTool>();
        }
        catch
        {
            Debug.LogError("Unable to find select tool!");
        }
    }

    public void SelectMesh(GameObject go)
    {
        selectedMeshes.Add(go);
        lastSelection = go;
        ObjectSelected.Invoke(go);
        try
        {
            EditableMesh em = go.GetComponent<EditableMesh>();
        }
        catch
        {
            Debug.LogError(go.name + " doesn't have an EditableMesh!");
        }
        Debug.Log("SelectedMeshes: " + selectedMeshes.Count);
    }

    public void DeselectMesh(GameObject go)
    {
        try
        {
            selectedMeshes.Remove(go);
            UpdateLastSelected(go);
        }
        catch
        {
            Debug.LogError(go.name + " was not selected");
        }
    }

    private void UpdateLastSelected(GameObject go)
    {
        if (lastSelection == go)
        {
            if(selectedMeshes.Count == 0)
                lastSelection = null;
            else
                lastSelection = selectedMeshes[selectedMeshes.Count - 1];
        }
    }

    private void ClearSelectedMeshes()
    {
        selectedMeshes.Clear();
        lastSelection = null;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     Debug.Log("Selected meshes count: " + selectedMeshes.Count);
    // }
}
