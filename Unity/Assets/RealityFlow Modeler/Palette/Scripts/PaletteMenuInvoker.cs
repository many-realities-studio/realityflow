using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ubiq.Avatars;
using Ubiq.XRI;
using Ubiq.Voip;
using Ubiq.Messaging;
using Avatar = Ubiq.Avatars.Avatar;



namespace Ubiq
{
public class PaletteMenuInvoker : MonoBehaviour
    {
    public MenuRequestSource source;
    public AvatarManager avatarManager;
    public IPeer me;
    public enum Wrist
    {
        Left,
        Right
    }
    public Wrist wrist;
    public void Use(MenuAdapterXRI controller)
    {
        source.Request(gameObject);
    }

    public void Awake() {
      avatarManager = NetworkScene.Find(this).GetComponentInChildren<AvatarManager>();
      NetworkScene.Find(this).GetComponent<RoomClient>().OnJoinedRoom.AddListener((room) => {
        Debug.Log("Joined room");
        me = NetworkScene.Find(this).GetComponent<RoomClient>().Me;
      });
    }

    public void UnUse(MenuAdapterXRI controller) { }

    private void Update()
    {
        UpdatePositionAndRotation();
    }

    private void LateUpdate()
    {
        UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
      if(me != null) {

      // Keep the palette on the user's left or right wrist.
        var nodePosition = wrist == Wrist.Left
            ? "LeftHandPosition"
            : "RightHandPosition";
        var nodeRotation = wrist == Wrist.Left
            ? "LeftHandRotation"
            : "RightHandRotation";
        Quaternion quat;
        Avatar avatar = avatarManager.FindAvatar(me);
        avatar.hints.TryGetQuaternion(nodeRotation, out quat);   
        transform.rotation = quat;
        Vector3 vec3;
        avatarManager.FindAvatar(me).hints.TryGetVector3(nodePosition, out vec3);   
        transform.position = vec3;
      }
    }
    }
}
