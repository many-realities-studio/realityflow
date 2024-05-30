// DraggableNode.cs
using Microsoft.MixedReality.Toolkit;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using DraggableNodeEvent = UnityEngine.Events.UnityEvent<DraggableNodeEventData>;

[HelpURL("https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/draggableNodes")]
[AddComponentMenu("XR/DraggableNode")]
public class DraggableNode : StatefulInteractable, ISnapInteractable
{
  #region Serialized Fields and Public Properties

  [Header("DraggableNode Options")]

  [SerializeField]
  [Tooltip("Whether or not this draggableNode is manipulatable by IPokeInteractors.\nIf true, IGrabInteractors will have no effect.")]
  private bool isTouchable;

  public bool IsTouchable
  {
    get => isTouchable;
    set => isTouchable = value;
  }

  [SerializeField]
  [Tooltip("Whether or not this draggableNode snaps to the designated position on the draggableNode.\nGrab interactions will not snap, regardless of the value of this property.")]
  private bool snapToPosition;

  public bool SnapToPosition
  {
    get => snapToPosition;
    set => snapToPosition = value;
  }

  [SerializeField]
  [Tooltip("Transform of the handle affordance")]
  private Transform handleTransform;

  public Transform HandleTransform => handleTransform;

  [SerializeField]
  private Vector2 draggableNodeValue = new Vector2(0.5f, 0.5f);

  public Vector2 DraggableNodeValue
  {
    get => draggableNodeValue;
    set
    {
      var oldDraggableNodeValue = draggableNodeValue;
      draggableNodeValue = value;
      OnValueUpdated.Invoke(new DraggableNodeEventData(oldDraggableNodeValue, value));
    }
  }

  [SerializeField]
  [Tooltip("Controls whether this draggableNode is increments in steps or continuously.")]
  private bool useDraggableNodeStepDivisions;

  public bool UseDraggableNodeStepDivisions
  {
    get => useDraggableNodeStepDivisions;
    set => useDraggableNodeStepDivisions = value;
  }

  [SerializeField]
  [Min(1)]
  [Tooltip("Number of subdivisions the draggableNode is split into.")]
  private int draggableNodeStepDivisions = 1;

  public int DraggableNodeStepDivisions
  {
    get => draggableNodeStepDivisions;
    set => draggableNodeStepDivisions = value;
  }

  #endregion

  #region Event Handlers

  [Header("DraggableNode Events")]
  public DraggableNodeEvent OnValueUpdated = new DraggableNodeEvent();

  #endregion

  #region Private Fields

  private Vector2 DraggableNodeStepVal => new Vector2((maxVal - minVal) / draggableNodeStepDivisions, (maxVal - minVal) / draggableNodeStepDivisions);

  #endregion

  #region Protected Properties

  protected Vector2 StartDraggableNodeValue { get; private set; }
  protected Vector3 StartInteractionPoint { get; private set; }
  private BoxCollider handleCollider;
  private Vector3 originalHandleColliderSize;

  #endregion

  #region Constants

  private const float minVal = 0.0f;
  private const float maxVal = 1.0f;

  #endregion

  #region Unity methods

  protected override void Awake()
  {
    base.Awake();
    ApplyRequiredSettings();
    parentCollider = GetComponentInParent<DraggableParent>().GetBoxCollider();
    handleCollider = handleTransform.GetComponent<BoxCollider>();

    if (handleCollider != null)
    {
      originalHandleColliderSize = handleCollider.size;
    }
  }

  protected override void Reset()
  {
    base.Reset();
    ApplyRequiredSettings();
  }

  protected virtual void Start()
  {
    SnapToPosition = snapToPosition;

    // if (handleTransform == null)
    // {
      // Debug.LogWarning("DraggableNode " + name + " has no handle transform. Please fix! Using primary collider instead.");
      handleTransform = colliders[0].transform;
    // }

    if (useDraggableNodeStepDivisions)
    {
      InitializeStepDivisions();
    }

    OnValueUpdated.Invoke(new DraggableNodeEventData(draggableNodeValue, draggableNodeValue));
  }

  private void OnValidate()
  {
    ApplyRequiredSettings();
  }

  #endregion

  #region Private Methods

  protected virtual void ApplyRequiredSettings()
  {
    selectMode = InteractableSelectMode.Single;
  }

  private void InitializeStepDivisions()
  {
    DraggableNodeValue = SnapDraggableNodeToStepPositions(DraggableNodeValue);
  }

  private Vector2 SnapDraggableNodeToStepPositions(Vector2 value)
  {
    var stepCountX = value.x / DraggableNodeStepVal.x;
    var stepCountY = value.y / DraggableNodeStepVal.y;
    var snappedValueX = DraggableNodeStepVal.x * Mathf.RoundToInt(stepCountX);
    var snappedValueY = DraggableNodeStepVal.y * Mathf.RoundToInt(stepCountY);
    return new Vector2(Mathf.Clamp(snappedValueX, minVal, maxVal), Mathf.Clamp(snappedValueY, minVal, maxVal));
  }

  private void UpdateDraggableNodeValue()
  {
    if (interactorsSelecting.Count == 0)
    {
      return;
    }

    Vector3 interactionPoint = interactorsSelecting[0].GetAttachTransform(this).position;
    Vector3 interactorDelta = interactionPoint - StartInteractionPoint;

    Vector3 newPosition = StartInteractionPoint + interactorDelta;
    newPosition = ClampPositionToBounds(newPosition, parentCollider);

    float unsnappedValueX = Mathf.Clamp01((newPosition.x - parentCollider.bounds.min.x) / parentCollider.bounds.size.x);
    float unsnappedValueY = Mathf.Clamp01((newPosition.y - parentCollider.bounds.min.y) / parentCollider.bounds.size.y);

    DraggableNodeValue = useDraggableNodeStepDivisions
        ? SnapDraggableNodeToStepPositions(new Vector2(unsnappedValueX, unsnappedValueY))
        : new Vector2(unsnappedValueX, unsnappedValueY);
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

  #endregion

  #region XRI methods

  protected override void OnSelectEntered(SelectEnterEventArgs args)
  {
    base.OnSelectEntered(args);

    StartInteractionPoint = args.interactorObject.GetAttachTransform(this).position;
    StartDraggableNodeValue = draggableNodeValue;

    if (handleCollider != null)
    {
      handleCollider.size = originalHandleColliderSize * 1.5f; // Increase the size by 50%
    }
  }

  protected override void OnSelectExited(SelectExitEventArgs args)
  {
    base.OnSelectExited(args);

    if (handleCollider != null)
    {
      handleCollider.size = originalHandleColliderSize; // Revert to original size
    }
  }

  public override bool IsSelectableBy(IXRSelectInteractor interactor)
  {
    if (isSelected)
    {
      return base.IsSelectableBy(interactor) && interactor == interactorsSelecting[0];
    }

    if (interactor is IGrabInteractor)
    {
      return !isTouchable && base.IsSelectableBy(interactor);
    }

    if (interactor is IPokeInteractor)
    {
      return isTouchable && base.IsSelectableBy(interactor);
    }

    return base.IsSelectableBy(interactor);
  }

  private static readonly ProfilerMarker ProcessInteractablePerfMarker =
      new ProfilerMarker("[MRTK] DraggableNode.ProcessInteractable");

  public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
  {
    using (ProcessInteractablePerfMarker.Auto())
    {
      base.ProcessInteractable(updatePhase);

      if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
      {
        UpdateDraggableNodeValue();
      }
    }
  }
  #endregion
  private BoxCollider parentCollider;
}
