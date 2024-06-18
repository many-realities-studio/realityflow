using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using TransformTypes;

/// <summary>
/// The class RayGizmoInteraction manages changes that should occur on ray interaction with the gizmo
/// </summary>
public class RayGizmoInteraction : MonoBehaviour
{
    float baseAlpha;
    const float onHoverAlpha = 1.0f;
    bool lastUpdateRayHover = false;
    bool lastUpdateRaySelect = false;
    List<GameObject> disabledComponents;

    // Start is called before the first frame update
    void Start()
    {
        baseAlpha = this.GetComponentInChildren<Renderer>().material.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        if (StartOfRayHover())
        {
            SetTransparency(onHoverAlpha);
            lastUpdateRayHover = true;
        }

        else if(EndOfRayHover())
        {
            SetTransparency(baseAlpha);
            lastUpdateRayHover = false;
        }

        if (StartOfRaySelect())
        {
            DisableNonMatchingComponents();
            lastUpdateRaySelect = true;
        }

        else if (EndOfRaySelect())
        {
            ReEnableComponents();
            lastUpdateRaySelect = false;
        }
    }

    /// <summary>
    /// Returns true for the first time the controller ray selecting over the gizmo
    /// </summary>
    /// <returns></returns>
    bool StartOfRaySelect()
    {
        return !lastUpdateRaySelect && this.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Returns true for the first time the controller ray no longer selecting the gizmo
    /// </summary>
    /// <returns></returns>
    bool EndOfRaySelect()
    {
        return lastUpdateRaySelect && !this.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Returns true for the first time the controller ray is hovering over the gizmo
    /// </summary>
    /// <returns></returns>
    bool StartOfRayHover()
    {
        return !lastUpdateRayHover && this.GetComponent<MRTKBaseInteractable>().IsRayHovered;
    }

    /// <summary>
    /// Returns true for the first time the controller ray is no longer hovering over and is not currently selecting the gizmo
    /// </summary>
    /// <returns></returns>
    bool EndOfRayHover()
    {
        return lastUpdateRayHover && !this.GetComponent<MRTKBaseInteractable>().IsRayHovered && !this.GetComponent<MRTKBaseInteractable>().IsRaySelected;
    }

    /// <summary>
    /// Sets the alpha value of each child as the value of <paramref name="alpha"/>
    /// </summary>
    /// <param name="alpha">The alpha value to set each child as</param>
    void SetTransparency(float alpha)
    {
        foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
        {
            Color initColor = renderer.material.color;
            initColor.a = alpha;
            renderer.material.color = initColor;
        }
    }

    /// <summary>
    /// For each object in the gizmo, the object is disabled if does not belong to same type of transformation as the currently selected object
    /// </summary>
    void DisableNonMatchingComponents()
    {
        GameObject gizmo = this.transform.parent.transform.parent.gameObject;
        disabledComponents = new List<GameObject>();

        for (int i = 0; i < gizmo.transform.childCount; i++)
        {
            for (int j = 0; j < gizmo.transform.GetChild(i).childCount; j++)
            {
                GameObject subComponent = gizmo.transform.GetChild(i).transform.GetChild(j).gameObject;
                // Skip if the sumcomponent is not active
                if (!subComponent.activeInHierarchy) continue;

                if (this.name != subComponent.name)
                {
                    subComponent.SetActive(false);
                    disabledComponents.Add(subComponent);
                }
            }
        }
    }

    /// <summary>
    /// For each object disabled by DisableNonMatchingComponents, enable them
    /// </summary>
    void ReEnableComponents()
    {
        foreach (GameObject go in disabledComponents)
        {
            go.SetActive(true);
        }

        disabledComponents.Clear();
    }
}
