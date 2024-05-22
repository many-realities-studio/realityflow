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



namespace Ubiq
{
public class PaletteMenuInvoker : MonoBehaviour
    {
    public MenuRequestSource source;

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
                /*var node = wrist == Wrist.Left
                    ? AvatarHints.NodePosRot.LeftWrist
                    : AvatarHints.NodePosRot.RightWrist;
                if (AvatarHints.TryGet(node, XRPlayerController.Singleton, out var positionRotation))
                {
                    transform.position = positionRotation.position;
                    transform.rotation = positionRotation.rotation;
                }*/
            }
    }
}
