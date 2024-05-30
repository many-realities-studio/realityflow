using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ubiq.Examples
{
    public class _04_JoinRoomClients : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("Started!");
            var guid = Guid.NewGuid();
            foreach (var roomClient in GetComponentsInChildren<RoomClient>())
            {
                Debug.Log("Joining...");
                roomClient.OnJoinedRoom.AddListener((roomClient) => {
                    Debug.Log("Joined");
                });
                roomClient.Join(guid);
            }
        }
    }

}