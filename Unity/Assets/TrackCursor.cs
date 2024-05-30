using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class TrackCursor : MonoBehaviour
{
    // public XRRayInteractor interactor;
    public IXRHoverInteractor interactor;
    public IXRInteractable interactable;
    public Image reticle;
    public Vector2 cursorPosition;
    public SpawnNodeAtRay spawnScript;
    public bool isHovering = false;

    // Start is called before the first frame update
    void Start()
    {
        cursorPosition = new Vector2();
    }

    public void StartHover(HoverEnterEventArgs args) {
        interactable = args.interactableObject;
        if(args.interactorObject.transform.gameObject.GetComponent<XRRayInteractor>() != null) {
            interactor = args.interactorObject.transform.gameObject.GetComponent<XRRayInteractor>();
        } else if (args.interactorObject.transform.gameObject.GetComponent<CanvasProxyInteractor>() != null)
        {
            Debug.Log(args.interactorObject);
            interactor = args.interactorObject;
        }
        reticle.gameObject.SetActive(true);
        isHovering = true;
    Debug.Log("Start hover");
    }

    public void StopHover(HoverExitEventArgs args) {
    Debug.Log("Stop hover");
        interactable = args.interactableObject;
        interactor = null;
        reticle.gameObject.SetActive(false);
        isHovering = false;
    }
  // Update is called once per frame
  Vector3 localTouchPosition;
  void Update()
  {
    // if(isHovering) {

    // if (interactor != null && interactor is XRRayInteractor)
    // {
    //   // Vector3 localTouchPositionWorld = interactor.transform.position;
    //   if ((interactor as XRRayInteractor).TryGetCurrent3DRaycastHit(out RaycastHit rh))
    //   {
    //     if (rh.collider)
    //     {
    //       localTouchPosition = GetComponent<RectTransform>().InverseTransformPoint(rh.point);
    //       cursorPosition.x = localTouchPosition.x;
    //       cursorPosition.y = localTouchPosition.y;
    //       reticle.GetComponent<RectTransform>().anchoredPosition = cursorPosition;
    //     }
    //   }
    //   // worldReticle.transform.position = localTouchPositionWorld;
    // } else if (interactor != null && interactor is CanvasProxyInteractor) {
    //     // (interactor as CanvasProxyInteractor).UpdateSelect(interactable,);
    //   Debug.Log("Canvas Interactor "+ (interactor as CanvasProxyInteractor).attachTransform.position.ToString());
    //   localTouchPosition = GetComponent<RectTransform>().InverseTransformPoint((interactor as CanvasProxyInteractor).attachTransform.position);
    //   cursorPosition.x = localTouchPosition.x;
    //   cursorPosition.y = localTouchPosition.y;
    //   reticle.GetComponent<RectTransform>().anchoredPosition = cursorPosition;
    // } else if(interactor != null) {
    //   Debug.Log(interactor);
    // }
    // }
    }
}
