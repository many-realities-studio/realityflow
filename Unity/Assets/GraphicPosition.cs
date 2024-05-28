using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GraphicPosition : TrackedDeviceGraphicRaycaster
{
	public Image reticle;
	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		base.Raycast(eventData, resultAppendList);
		// Vector3 loc = transform.position - eventData.pointerCurrentRaycast.worldPosition;
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(gameObject.GetComponent<RectTransform>(), 
        eventData.pointerCurrentRaycast.screenPosition, 
        eventData.enterEventCamera, out localPoint);
		//Debug.Log(localPoint);
		// Debug.Log(loc.x / 4.29f + " " + loc.y / 4.27f);
		reticle.GetComponent<RectTransform>().anchoredPosition = new Vector3(localPoint.x, localPoint.y, 0f);
		//reticle2.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(localPoint.x, localPoint.y, 0f);
		// gameObject.GetComponentInChildren<Image>().mainTexture.height;
		// loc1 = new Vector3((localPoint.x / gameObject.GetComponentInChildren<Image>().mainTexture.width / 2f), (localPoint.y / gameObject.GetComponentInChildren<Image>().mainTexture.height / 2f), 0f);
		//Debug.Log(gameObject.GetComponentInChildren<Image>().mainTexture.height);
		//Debug.Log(gameObject.GetComponentInChildren<Image>().mainTexture.width);
		// loc2.z = (float)(localPoint.y / 426f) * 32.76923f / 2f;
		// loc2.x = (float)(localPoint.x / 426f) * 32.76923f / 2f;
		// loc2.y = 0.62f;

		//var pointerEventData = new pointerEventData{ position = touchPos};
		//var raycastResults = new List<RaycastResult>();
	}
}
