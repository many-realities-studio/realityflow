using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class TrackCursor : MonoBehaviour
{
    public MRTKRayInteractor interactor;
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
        if(args.interactorObject.transform.gameObject.GetComponent<MRTKRayInteractor>() != null) {
            interactor = args.interactorObject.transform.gameObject.GetComponent<MRTKRayInteractor>();
        }
        reticle.gameObject.SetActive(true);
        isHovering = true;
    }

    public void StopHover(HoverExitEventArgs args) {
        interactable = args.interactableObject;
        interactor = null;
        reticle.gameObject.SetActive(false);
        isHovering = false;
    }
    // Update is called once per frame
    void Update()
    {
        
      if(interactor != null) {
            // Vector3 localTouchPositionWorld = interactor.transform.position;
            Vector3 localTouchPosition;
            if (interactor.TryGetCurrent3DRaycastHit(out RaycastHit rh))
            {
              if(rh.collider) {
                localTouchPosition = GetComponent<RectTransform>().InverseTransformPoint(rh.point);
                cursorPosition.x = localTouchPosition.x;
                cursorPosition.y = localTouchPosition.y;
                reticle.GetComponent<RectTransform>().anchoredPosition = cursorPosition;
              }
            }
            // worldReticle.transform.position = localTouchPositionWorld;
        }
        
    }
}
