using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ubiq.Rooms;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Networking;


// Structure for GraphQL Requests
public class GraphQLRequest : System.Object
{
    [JsonPropertyAttribute(PropertyName = "query")]
    public string Query;
    [JsonPropertyAttribute(PropertyName = "operationName")]
    public string OperationName;
    [JsonPropertyAttribute(PropertyName = "variables")]
    public System.Object Variables;
}

public class RealityFlowClient : MonoBehaviour
{
    public string accessToken = null;
    public RoomClient roomClient;
    private string currentProjectId;

    // GraphQL client and access token variables
    public Dictionary<string, string> userDecoded;
#if REALITYFLOW_LIVE
    public string server = @"https://reality.gaim.ucf.edu/";
#else
    public string server = @"http://localhost:4000/";
#endif
    public string graphQLRoute = "graphql"; 
    public event Action<bool> LoginSuccess;
    public event Action<JArray> OnRoomsReceived;
    public event Action<JObject> OnProjectUpdated;

    private void Awake()
    {
        //Debug.Log("RealityFlowClient Awake");
        // Ensure only one instance
               if (transform.parent == null)
        {
            if (rootRealityFlowClient == null)
            {
                rootRealityFlowClient = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            foreach (var item in GetComponents<RealityFlowClient>())
            {
                if (item != this)
                {
                    Destroy(item);
                }
            }
        }
        // Attempt to find RoomClient manually if not assigned
        if (roomClient == null)
        {
            // Debug.LogWarning("RoomClient is not assigned in the inspector, trying to find it in the scene.");
            roomClient = NetworkScene.Find(this)?.GetComponent<RoomClient>();
            if (roomClient == null)
            {
                //Debug.LogWarning("Attempting to find RoomClient directly in the hierarchy.");
                roomClient = FindObjectOfType<RoomClient>();
            }
        }

        if (roomClient == null)
        {
            Debug.LogError("RoomClient component not found on this GameObject or in the scene.");
            return;
        }

        roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
    }
    private static RealityFlowClient rootRealityFlowClient;
    public async void Login(string inputAccessToken) {
        Debug.Log("Logging in....");
        userDecoded = DecodeJwt(inputAccessToken);
        // Debug.Log("User decoded: " + userDecoded);
                // Create a new room using the GraphQL API
        var verifyToken = new GraphQLRequest
        {
            Query = @"
            query VerifyAccessToken($input: String) {
                verifyAccessToken(accessToken: $input) {
                    id
                }
            }
        ",
            OperationName = "VerifyAccessToken",
            Variables = new
            {
                input = inputAccessToken
            }
        };
        var graphQL = await SendQueryAsync(verifyToken);
        if (graphQL["data"] != null)
        {
            Debug.Log("User Logged In successfully");
            accessToken = inputAccessToken;
            PlayerPrefs.SetString("accessToken", accessToken);
            LoginSuccess.Invoke(true);
        }
        else
        {
            Debug.LogError("Failed to log in");
            accessToken = "";
            PlayerPrefs.SetString("accessToken", "");
            LoginSuccess.Invoke(false);
        }

        // Debug.Log("RoomClient successfully initialized and listener added.");
    }

    public void CreateRoom(string ProjectId)
    {
        Debug.Log("Creating room for project: " + currentProjectId); // Log the project ID
        currentProjectId = ProjectId;

        // Check if a project is selected
        if (string.IsNullOrEmpty(currentProjectId))
        {
            Debug.LogError("No project selected");
            return;
        }

        // !!LOAD OBJECTS FROM PROJECT HERE!!
        roomClient.OnJoinedRoom.AddListener(OnJoinedRoomCreate);
        roomClient.Join("test-room", false); // Name: Test-Room, Publish: false
        RealityFlowAPI.Instance.FetchAndPopulateObjects();

    }

    private async void OnJoinedRoomCreate(IRoom room)
    {
        Debug.Log("Created Room: " + room.Name);
        Debug.Log(room.Name + " JoinCode: " + room.JoinCode);
        Debug.Log(room.Name + " UUID: " + room.UUID);
        Debug.Log(room.Name + " Publish: " + room.Publish);

        // Create a new room using the GraphQL API
        var addRoom = new GraphQLRequest
        {
            Query = @"
            mutation AddRoom($input: AddRoomInput!) {
                addRoom(input: $input) {
                    id
                }
            }
        ",
            OperationName = "AddRoom",
            Variables = new
            {
                input = new
                {
                    projectId = currentProjectId,
                    creatorId = userDecoded["id"],
                    roomId = room.UUID,
                    joinCode = room.JoinCode,
                    isEditable = true
                }
            }
        };
        var graphQL = await SendQueryAsync(addRoom);
        if (graphQL["data"] != null)
        {
            Debug.Log("Room created successfully");
            GetRoomsByProjectId(currentProjectId);
        }
        else
        {
            Debug.LogError("Failed to create room");
        }
        roomClient.OnJoinedRoom.RemoveListener(OnJoinedRoomCreate);
    }

    public void JoinRoom(string joinCode)
    {
        Debug.Log("Joining room found in the Database. . .");


        // Join the room using the roomClient
        roomClient.Join(joinCode);

        // !!LOAD OBJECTS FROM PROJECT HERE!!
        //Call FetchAndPopulateObjects from API
        //RealityFlowAPI.Instance.FetchAndPopulateObjects();
    }

    private void OnJoinedRoom(IRoom room)
    {
        Debug.Log("Joined room: " + room.Name);
        Debug.Log(room.Name + " JoinCode: " + room.JoinCode);
        Debug.Log(room.Name + " UUID: " + room.UUID);
        Debug.Log(room.Name + " Publish: " + room.Publish);
    }

    public async Task<JObject> SendQueryAsync(GraphQLRequest payload)
    {
        // Describe request
        UnityWebRequest request = UnityWebRequest.Post(server + graphQLRoute,
            JsonConvert.SerializeObject(payload), "application/json");
        if (accessToken != null && accessToken != "")
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        // Send request
        bool waiting = true;
        request.SendWebRequest().completed += _ => waiting = false;
        await Task.Run(async () => {
            while (waiting)
                await Task.Delay(3);
        });
        // Handle response
        JObject response = null;

        // Handle network issues
        if (request.result != UnityWebRequest.Result.Success
                && request.downloadHandler.text == "") {
            response = new JObject();
            var errors = new JArray();
            var error = new JObject();
            var errorExtensions = new JObject();

            error.Add("message", request.error);
            errorExtensions.Add("code", "INTERNAL_SERVER_ERROR");
            error.Add("extensions", errorExtensions);
            errors.Add(error);

            error = new JObject();
            error.Add("message", request.downloadHandler.text);
            errors.Add(error);

            response.Add("errors", errors);
        }
        
        // Handle response
        if (response == null) {
            try {
                Debug.Log(request.downloadHandler.text);
                response = JsonConvert.DeserializeObject<JObject>(
                    request.downloadHandler.text // This is failing
                );
            } catch(Exception e) {
                Debug.Log(response);
                Debug.LogError(e);
                return new JObject();
            }
        }


        // Display any errors
        if (response["errors"] != null) {
            Debug.LogError("GraphQL Errors");
            foreach (JObject error in response["errors"]) {
                Debug.LogError(error["message"]);
            }
        }

        return response;
    }

    public void LeaveRoom()
    {
        // Implementation for leaving a room

    }
    
    public string GetCurrentProjectId()
    {
        return currentProjectId;
    }

    public void SetCurrentProject(string projectId)
    {
        //Debug.Log("Setting current project ID to: " + projectId);
        currentProjectId = projectId;
        PlayerPrefs.SetString("currentProjectId", currentProjectId);
    }
    // Function to decode the JWT token
    public static Dictionary<string, string> DecodeJwt(string jwt)
    {
        string[] jwtParts = jwt.Split('.');
        byte[] decodedPayload = FromBase64Url(jwtParts[1]);
        string decodedText = Encoding.UTF8.GetString(decodedPayload);

        Dictionary<string, string> jwtPayload = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedText);

        return jwtPayload;
    }

    // Function to convert the base64 URL to byte array
    static byte[] FromBase64Url(string base64Url)
    {
        string padded = base64Url.Length % 4 == 0
        ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
        string base64 = padded.Replace("_", "/").Replace("-", "+");
        return Convert.FromBase64String(base64);
    }
    public static RealityFlowClient Find(MonoBehaviour component)
    {
        return Find(component.transform);
    }

    public static RealityFlowClient Find(Transform component)
    {
        // Check if the scene is simply a parent, or if we can find a root scene.
        // var scene = component.GetComponentInParent<RealityFlowClient>();
        var scene = NetworkScene.Find(component);
        var rootRealityFlowClient = scene.GetComponentInChildren<RealityFlowClient>();
        if (rootRealityFlowClient != null)
        {
            return rootRealityFlowClient;
        }
        return null;
    }

    public async void OpenProject(string id)
    {
        Debug.Log("Opening project with ID: " + id);
        
        // Create a new GraphQL query request to get the project details by ID.
        var GetProjectData = new GraphQLRequest
        {
            Query = @"
                query GetProjectById($getProjectByIdId: String) {
                    getProjectById(id: $getProjectByIdId) {
                        projectName
                        gallery
                        description
                        projectOwner {
                            username
                        }
                        rooms {
                            id
                            udid
                            joinCode
                            isEditable
                            creatorId
                        }
                    }
                }
                ",
            OperationName = "GetProjectById",
            Variables = new { getProjectByIdId = id }
        };

        // Send the query request asynchronously and wait for the response.
        var graphQL = await SendQueryAsync(GetProjectData);
        var projectdata = graphQL["data"];
        if (projectdata != null)
        {
            Debug.Log("Fetched project data: " + projectdata.ToString());
            
            // Set the project details in the UI
            OnProjectUpdated.Invoke((JObject)projectdata);
            var roomsData = projectdata["getProjectById"]["rooms"];
            if (roomsData != null)
            {
                Debug.Log("Fetched rooms data: " + roomsData.ToString());
                Debug.Log("Rooms: " + roomsData.ToString());
                OnRoomsReceived.Invoke(roomsData as JArray);
            }
            else
            {
                Debug.LogError("Failed to fetch rooms data");
            }
            // GetRoomsByProjectId(id);
        }
        else
        {
            Debug.LogError("Failed to fetch project data");
        }
    }

    // Function to get the rooms associated with the project
    public async void GetRoomsByProjectId(string projectId)
    {
        var getRooms = new GraphQLRequest
        {
            Query = @"
                query GetRoomsByProjectId($projectId: String!) {
                    getRoomsByProjectId(projectId: $projectId) {
                        id
                        udid
                        joinCode
                        isEditable
                    }
                }
            ",
            OperationName = "GetRoomsByProjectId",
            Variables = new { projectId = projectId }
        };

        var graphQL = await SendQueryAsync(getRooms);
        var roomsData = graphQL["data"];
        JArray rooms = null;
        if (roomsData != null)
        {
            Debug.Log("Fetched rooms data: " + roomsData.ToString());
            rooms = (JArray)roomsData["getRoomsByProjectId"];
            Debug.Log("Rooms: " + rooms.ToString());
        }
        else
        {
            Debug.LogError("Failed to fetch rooms data");
        }
        OnRoomsReceived?.Invoke(rooms);
    }
    // Wrapper method to call RoomManager's CreateRoom
    public void CallCreateRoom()
    {
        CreateRoom(currentProjectId);
    }

    // Wrapper method to call RoomManager's JoinRoom
    public void CallJoinRoom(string joinCode)
    {  
        Debug.Log("[JOIN ROOM]Join code!: " + joinCode);
        roomClient.Join(joinCode);
    }

}