using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaveRoomButton : MonoBehaviour
{
    public event UnityAction LeaveRoomAction;

    public void LeaveRoom()
    {
        Debug.Log("Clicked");
        RealityFlowClient.Find(this).LeaveRoom();
    }

}