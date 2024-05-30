// Attach this script to the parent GameObject
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DraggableParent : MonoBehaviour
{
  public BoxCollider boxCollider;

  void Awake()
  {
    boxCollider = GetComponent<BoxCollider>();
    boxCollider.isTrigger = true; // Ensure the collider is a trigger to allow interactions
    // Resize the collider to match the bounds
    // GetComponent<RectTransformColliderFitter>().enabled = true;
    // GetComponent<RectTransformColliderFitter>();
  }

  public BoxCollider GetBoxCollider()
  {
    return boxCollider;
  }
}