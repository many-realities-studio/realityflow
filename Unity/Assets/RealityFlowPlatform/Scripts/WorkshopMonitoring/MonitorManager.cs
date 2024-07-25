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
  #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Awake();
#else
    private void Awake() {}
#endif

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

        var getUser = await rfClient.SendQueryAsync(new GraphQLRequest
        {
            OperationName = "GetUser",
            Query = @"query GetUser($id: String!) {
                getUserById(id: $id) {
                    currentRoomId
                }
            }",
            Variables = new { id = userId }
        });



        if (getUser["data"] == null || getUser["data"]["getUserById"] == null) {
            OnJoinRejected(new Rejection() { reason = "Invalid user id" }) ;
            return;
        }

        var getRoom = await rfClient.SendQueryAsync(new GraphQLRequest()
        {
            OperationName = "GetRoom",
            Query = @"query GetRoom($id: String!) {
                getRoom(id: $id) {
                    udid
                }
            }",
            Variables = new { id = getUser["data"]["getUserById"]["currentRoomId"]}
        })  ;

        if (getRoom["data"] == null || getRoom["data"]["getRoom"] == null) {
            OnJoinRejected(new Rejection() { reason = "Invalid room id"});
            return;
        }
        
        roomClient.Join((string)getRoom["data"]["getRoom"]["udid"]);
        selectedUserId = userId;
    }
}
