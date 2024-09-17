using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

public class Handle : MonoBehaviour
{
    public EditableMesh mesh { get; set; }

    protected MeshRenderer meshRenderer;
    Color defaultMat;

    public bool isSelected;
    private bool deselectOnRelease;

    protected HandleSelectionManager selectionManager;

    public ManipulationMode mode { get; protected set; }

    public virtual void Awake()
    {
        selectionManager = HandleSelectionManager.Instance;

        if (meshRenderer == null)
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        defaultMat = meshRenderer.material.color;
    }

    public void OnHandleSelected()
    {
        if(selectionManager == null)
        {
            Debug.LogError("SelectionManager not found");
        }

        if (!isSelected)
        {
            isSelected = true;
            deselectOnRelease = false;
        }

        if(isSelected && !deselectOnRelease)
        {
            gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.None;
            selectionManager.SelectHandle(this);
            meshRenderer.material.color = selectionManager.OnSelectColor;
        }
    }

    public void OnBeginHover()
    {
        if(!isSelected)
        { 
            meshRenderer.material.color = selectionManager.OnHoverColor;
        }
    }

    public void OnEndHover()
    {
        if(!isSelected)
        {
            meshRenderer.material.color = defaultMat;
        }
    }

    public void OnHandleReleased()
    {
        if(deselectOnRelease)
        {
            isSelected = false;
            deselectOnRelease = false;
            gameObject.GetComponent<ObjectManipulator>().AllowedManipulations = TransformFlags.None;
            selectionManager.RemoveSelectedHandle(this);
            meshRenderer.material.color = defaultMat;
        }
        else
        {
            deselectOnRelease = true;
        }
    }

    public virtual void UpdateHandlePosition()
    {
        return;
    }

    public virtual int[] GetSharedVertexIndicies()
    {
        return null;
    }
}
