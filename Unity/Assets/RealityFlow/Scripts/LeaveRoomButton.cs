using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaveRoomButton : MonoBehaviour
{
    public void LeaveRoom()
    {
        RealityFlowAPI.Instance.LogActionToServer("Leave Room", new {});
        Debug.Log("Clicked");
        RealityFlowAPI.Instance.LeaveRoom();
    }
}