using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class MeshVisualization manipulates the mesh material
/// </summary>
public class MeshVisulization : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private SelectToolManager selectToolManager;

    [SerializeField, Range(0f, 1f)]
    private float alphaAmount = 0.4f;

    private float currentMetallicValue;
    private float currentGlossyValue;

    void Awake()
    {
        TryGetMeshRenderer();
    }

    private void TryGetMeshRenderer()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void CacheMetallicAndGlossyValues()
    {
        Material mat = gameObject.GetComponent<MeshRenderer>().material;

        currentMetallicValue = mat.GetFloat("_Metallic");
        currentGlossyValue = mat.GetFloat("_Glossiness");
    }

    /// <summary>
    /// Sets the material of the EditableMesh to be opaque
    /// </summary>
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
        mat.SetFloat("_Metallic", currentMetallicValue);
        mat.SetFloat("_Glossiness", currentGlossyValue);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 2000;

        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    /// <summary>
    /// Sets the material of the EditableMesh to be transparent
    /// </summary>
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
        mat.SetFloat("_Metallic", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color newColor = mat.color;
        newColor.a = alphaAmount;
        mat.color = newColor;
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }
}
