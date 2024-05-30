// DraggableVisuals.cs
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

[RequireComponent(typeof(DraggableNode))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
[AddComponentMenu("MRTK/UX/Canvas DraggableNode Visuals")]
public class DraggableVisuals : MonoBehaviour
{
  [SerializeField]
  [Tooltip("The transform of the handle.")]
  private RectTransform handle;

  public RectTransform Handle
  {
    get => handle;
    set => handle = value;
  }

  private DraggableNode draggableNodeState;

  protected DraggableNode DraggableNodeState
  {
    get
    {
      if (draggableNodeState == null)
      {
        draggableNodeState = GetComponent<DraggableNode>();
      }

      return draggableNodeState;
    }
  }

  private RectTransformColliderFitter touchableFitter;
  private UGUIInputAdapter uguiInputAdapter;
  private BoxCollider parentCollider;

  void OnEnable()
  {
    uguiInputAdapter = GetComponent<UGUIInputAdapter>();

    DraggableNodeState.OnValueUpdated.AddListener(UpdateHandle);

    // Ensure the parent collider is correctly referenced
    DraggableParent parent = GetComponentInParent<DraggableParent>();
    if (parent != null)
    {
      parentCollider = parent.GetBoxCollider();
    }
    else
    {
      Debug.LogError("DraggableVisuals: No DraggableParent found in parent hierarchy.");
    }

    UpdateHandle(DraggableNodeState.DraggableNodeValue);
  }

  void OnDisable()
  {
    DraggableNodeState.OnValueUpdated.RemoveListener(UpdateHandle);
  }

#if UNITY_EDITOR

  void Update()
  {
    if (Application.isPlaying)
    {
      return;
    }

    UpdateHandle(DraggableNodeState.DraggableNodeValue);
  }
#endif // UNITY_EDITOR

  void UpdateHandle(DraggableNodeEventData data)
  {
    UpdateHandle(data.NewValue);
  }

  void UpdateHandle(Vector2 value)
  {
    if (parentCollider == null)
    {
      // Debug.LogError("DraggableVisuals: parentCollider is not assigned.");
      return;
    }

    Vector3 newPosition = new Vector3(
        Mathf.Lerp(parentCollider.bounds.min.x, parentCollider.bounds.max.x, value.x),
        Mathf.Lerp(parentCollider.bounds.min.y, parentCollider.bounds.max.y, value.y),
        parentCollider.transform.position.z // Assuming Z is constant or not relevant for 2D dragging
    );

    handle.position = ClampPositionToBounds(newPosition, parentCollider);
  }

  private Vector3 ClampPositionToBounds(Vector3 position, BoxCollider bounds)
  {
    Vector3 min = bounds.bounds.min;
    Vector3 max = bounds.bounds.max;

    return new Vector3(
        Mathf.Clamp(position.x, min.x, max.x),
        Mathf.Clamp(position.y, min.y, max.y),
        position.z // Assuming Z is constant or not relevant for 2D dragging
    );
  }
}
