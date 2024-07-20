using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using Ubiq.Rooms;
using Ubiq.Messaging;
using UnityEngine;

public class MonitorManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Awake();


    public RoomClient roomClient;

    public GameObject UI;
    public GameObject LoginLabel;
    public GameObject LoginFailedLabel;
    public GameObject PickUserLabel;
    public GameObject JoiningLabel;
    public GameObject JoiningFailedLabel;
    public TextMeshProUGUI JoiningFailedLabelReason;

    
    RealityFlowClient rfClient;
    GameObject LastLabel;
    
    string selectedUserId;
    bool isReady = false;

    private void ShowLabel(GameObject label) {
        if (LastLabel != null)
            LastLabel.SetActive(false);
        if (label != null) {
            label.SetActive(true);
            UI.SetActive(true);
            LastLabel = label;
        } else {
            UI.SetActive(false);
        }
    }

    void Start() {
        LastLabel = LoginLabel;
        rfClient = FindObjectOfType<RealityFlowClient>();
        rfClient.LoginSuccess += OnLogin;
        
        roomClient.OnJoinedRoom.AddListener(OnJoin);
        roomClient.OnJoinRejected.AddListener(OnJoinRejected);

#if UNITY_WEBGL && !UNITY_EDITOR
        Awake();
#endif
    }

    void OnLogin(bool successful) {
        if (successful) {
            ShowLabel(PickUserLabel);
            isReady = true;
        } else {
            ShowLabel(LoginFailedLabel);
        }
    }

    void OnJoin(IRoom room) {
        ShowLabel(null);
    }

    void OnJoinRejected(Rejection rejection) {
        JoiningFailedLabelReason.text = rejection.reason;
        ShowLabel(JoiningFailedLabel);
        selectedUserId = null;
    }

    async Task Follow(string userId)
    {
        // Check that credentials are correct
        if (!isReady) {
            return;
        } else if (string.IsNullOrEmpty(userId)) {
            OnJoinRejected(new Rejection() { reason = "Invalid user id" });
            return;
        }

        ShowLabel(JoiningLabel);

        var result = await rfClient.SendQueryAsync(new GraphQLRequest
        {
            OperationName = "GetUser",
            Query = @"query GetUser($id: String!) {
                getUserById(id: $id) {
                    currentRoomId
                }
            }",
            Variables = new { id = userId }
        });

        if (result["data"] == null || result["data"]["getUserById"] == null) {
            OnJoinRejected(new Rejection() { reason = "Invalid user id" }) ;
            return;
        }
        
        roomClient.Join((string)result["data"]["getUserById"]["currentRoomId"]);
        selectedUserId = userId;
    }
}
