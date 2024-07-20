using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ubiq.Rooms;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Threading;
using System.Collections;
using Unity.VisualScripting;
using Samples.Whisper;


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
    public GameObject projectManager;
    public OTPVerification loginMenu;
    public GameObject levelEditor;
    public RoomClient roomClient;
    private string currentProjectId;
    public Dictionary<string, string> userDecoded;
    public string graphQLRoute = "graphql";
    public event Action<bool> LoginSuccess;
    public event Action<JArray> OnRoomsReceived;
    public event Action<JObject> OnProjectUpdated;
    public event Action OnRoomCreated;
    
    UnityAction<IRoom> OnJoinCreatedRoomAction;

    private static RealityFlowClient rootRealityFlowClient;

    #if REALITYFLOW_LIVE
        public string server = @"https://reality.gaim.ucf.edu/";
    #else
        public string server = @"http://localhost:4000/";
    #endif

    private void Awake()
    {
        Debug.Log(" === RealityFlowClient Awake === ");

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

        if (projectManager != null && loginMenu != null) {
            LoginSuccess += (result) =>
            {
                if (result)
                {
                    Debug.Log("Login successful");
                    projectManager.SetActive(true);
                    loginMenu.gameObject.SetActive(false);
                }
                else
                {
                    loginMenu.gameObject.SetActive(true);
                    Debug.Log("Login is unsuccessful, proceeding with setup.");
                }
            };
        }

        accessToken = PlayerPrefs.GetString("accessToken");
    }

    public void Start()
    {
        OnJoinCreatedRoomAction = room => { OnJoinCreatedRoom(room); };
        // Check to see if PlayerPrefs already has an access token
        if (string.IsNullOrEmpty(accessToken))
        {
            // Debug.Log("Access token is null or empty.");
            ShowOTP();
        }
        else
        {
            // Debug.Log("Access token is valid.");
            Login(accessToken);
        }

    }

    public void OnApplicationQuit()
    {
        SendQueryAsync(new GraphQLRequest
        {
            Query = @"
                mutation UpdateRoom {
                    updateUserRoomId {
                        username
                    }
                }
            ",
            OperationName = "UpdateRoom"
        });
    }

    #region Utility Methods
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

    public async Task<JObject> SendQueryAsync(GraphQLRequest payload)
    {
        // Describe request
        UnityWebRequest request = UnityWebRequest.Post(server + graphQLRoute,
        JsonConvert.SerializeObject(payload), "application/json");
        if (accessToken != null && accessToken != "")
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        // Send request
        double start = Time.realtimeSinceStartupAsDouble;
        await request.SendWebRequest();
        double end = Time.realtimeSinceStartupAsDouble;
        Debug.Log($"Query '{payload.OperationName}' took {(end - start) * 1000}ms to complete");

        // Handle response
        JObject response = null;

        // Handle network issues
        if (request.result != UnityWebRequest.Result.Success
                && request.downloadHandler.text == "")
        {
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
        if (response == null)
        {
            try
            {
                // Debug.Log(request.downloadHandler.text);
                response = JsonConvert.DeserializeObject<JObject>(
                    request.downloadHandler.text // This is failing
                );
            }
            catch (Exception e)
            {
                Debug.Log(response);
                Debug.LogError(e);
                return new JObject();
            }
        }

        // Display any errors
        if (response["errors"] != null)
        {
            Debug.LogError("GraphQL Errors");
            foreach (JObject error in response["errors"])
            {
                Debug.LogError(error["message"]);
            }
        }

        return response;
    }

    #endregion

    #region Login Methods
    private void ShowOTP()
    {
        if (projectManager)
            projectManager.SetActive(false);
        
        if (loginMenu) {
            loginMenu.gameObject.SetActive(true);
            loginMenu.onOTPSubmitted += SubmitOTP;
        }
    }

    public async void SubmitOTP(string otp)
    {

        // Create a new GraphQL mutation request to verify the OTP provided by the user.
        var verifyOTP = new GraphQLRequest
        {
            Query = @"
                   mutation VerifyOTP($input: VerifyOTPInput!) {
                        verifyOTP(input: $input) {
                            accessToken
                         }
                   }
            ",
            OperationName = "VerifyOTP",
            Variables = new { input = new { otp } }
        };

        // Send the mutation request asynchronously and wait for the response.
        var queryResult = await SendQueryAsync(verifyOTP);
        var data = queryResult["data"];
        var errors = queryResult["errors"];
        if (data != null && errors == null)  // Success in retrieving Data
        {
            Debug.Log(data);
            string accessToken = (string)data["verifyOTP"]["accessToken"];
            PlayerPrefs.SetString("accessToken", accessToken);
            Login(accessToken);
            projectManager.SetActive(true);
            loginMenu.gameObject.SetActive(false);

        }
        else if (errors != null) // Failure to retrieve data
        {
            Debug.Log(errors[0]["message"]);
        }
    }

    public async Task CreateProject()
    {
        /* Create project input
        input CreateProjectInput{
        projectName: String,
        projectOwnerId: String,
        description: String,
        details: String,
        thumbnailImg: String,
        categories: [String]
        isPublic: Boolean
        gallery: [String]
        publicUrl: String,
        globalId: String

        createProject(input: CreateProjectInput) :Project!

        */
        // Create a new project with reasonable defaults and then open it
        var createProject = new GraphQLRequest
        {
            Query = @"
                mutation CreateProject($input: CreateProjectInput!) {
                    createProject(input: $input) {
                        id
                    }
                }
            ",
            OperationName = "CreateProject",
            Variables = new
            {
                input = new
                {
                    projectName = "New Project " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    projectOwnerId = userDecoded["id"],
                    description = "A new project",
                    details = "A new project",
                    thumbnailImg = "https://via.placeholder.com/150",
                    categories = new string[] { "Education" },
                    isPublic = false,
                    gallery = new string[] { "https://via.placeholder.com/150" },
                    publicUrl = "https://reality.gaim.ucf.edu/",
                    globalId = "New Project"
                }
            }
        };
        var graphQL = await SendQueryAsync(createProject);
        if (graphQL["data"] != null)
        {
            Debug.Log("Room created successfully");
            SetCurrentProject((string)graphQL["data"]["createProject"]["id"]);
            CreateRoom();
        }
        else
        {
            Debug.LogError("Failed to create room: Room may already exist.");
        }
    }

    public async Task Login(string inputAccessToken)
    {
        Debug.Log("Logging in....");
        Debug.Log(inputAccessToken);


        userDecoded = DecodeJwt(inputAccessToken);
        // Debug.Log("User decoded: " + userDecoded);

        // Create a new room using the GraphQL API
        var verifyToken = new GraphQLRequest
        {
            Query = @"
            query VerifyAccessToken($input: String) {
                verifyAccessToken(accessToken: $input) {
                    apiKey
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
        if (graphQL["errors"] == null && graphQL["data"]["verifyAccessToken"] != null)
        {
            // Debug.Log("User Logged In successfully");
            accessToken = inputAccessToken;
            PlayerPrefs.SetString("accessToken", accessToken);

            if (Whisper.rootWhisper)
                Whisper.rootWhisper.InitializeGPT((string)graphQL["data"]["verifyAccessToken"]["apiKey"]);    
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

    #endregion

    #region Project Methods

    public string GetCurrentProjectId()
    {
        return currentProjectId;
    }

    #endregion

    #region Room Methods
    public void CreateRoom()
    {
        // Debug.Log("Creating room for project: " + currentProjectId); // Log the project ID

        // Check if a project is selected
        if (string.IsNullOrEmpty(currentProjectId))
        {
            Debug.LogError("No project selected");
            return;
        }

        // Hide Project and Login Menus, show Level Editor
        projectManager.SetActive(false);
        // Put loading here?

        // !!LOAD OBJECTS FROM PROJECT HERE!!
        roomClient.OnJoinedRoom.AddListener(OnJoinCreatedRoomAction);
        roomClient.Join("test-room", false); // Name: Test-Room, Publish: false


    }

    public async Task OnJoinCreatedRoom(IRoom room)
    {
        //Debug.Log("Created Room: " + room.Name);
        // Debug.Log(room.Name + " JoinCode: " + room.JoinCode);
        // Debug.Log(room.Name + " UUID: " + room.UUID);
        // Debug.Log(room.Name + " Publish: " + room.Publish);

        levelEditor.SetActive(true);

        RealityFlowAPI.Instance.FetchAndPopulateObjects();
        // Create a new room using the GraphQL API
        var addRoom = new GraphQLRequest
        {
            Query = @"
            mutation AddRoom($input: AddRoomInput!, $input2: UpdateExpiredTimeInput!, $input3: UpdateUserRoomIdInput!) {
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
            Debug.Log(graphQL["data"]);
            Debug.Log("Room created successfully");
            GetRoomsByProjectId(currentProjectId);
        }
        else
        {
            Debug.LogError("Failed to create room: Room may already exist.");
        }

        // Run KeepRoomAlive every 30 seconds
        InvokeRepeating("KeepRoomAlive", 0, 30);


        roomClient.OnJoinedRoom.RemoveListener(OnJoinCreatedRoomAction);
        OnRoomCreated?.Invoke();
    }

    private async Task KeepRoomAlive()
    {
        // Create a new room using the GraphQL API
        var updateExpiredTime = new GraphQLRequest
        {
            Query = @"
            mutation UpdateExpiredTime($input: UpdateExpiredTimeInput!) {
                updateExpiredTime(input: $input) {
                    id
                }
            }
        ",
            OperationName = "UpdateExpiredTime",
            Variables = new
            {
                input = new
                {
                    userId = userDecoded["id"]
                }
            }
        };
        var graphQL = await SendQueryAsync(updateExpiredTime);
        if (graphQL["data"] != null)
        {
            Debug.Log("Room alive");
        }
        else
        {
            Debug.LogError("Failed to keep room alive");
        }
    }

    //  ==============   JOIN ROOM   ======================
    public void JoinRoom(string joinCode)
    {
        Debug.Log("Joining room for project: " + currentProjectId); // Log the project ID
        Debug.Log("Joining room with join code: " + joinCode); // Log the join code

        // Check if a project is selected
        if (string.IsNullOrEmpty(currentProjectId))
        {
            Debug.LogError("No project selected");
            return;
        }
        projectManager.SetActive(false);  // MAYBE CHECK FOR SUCCESSFUL JOIN BEFORE HIDING

        roomClient.OnJoinedRoom.AddListener(OnJoinedExistingRoom);

        Debug.Log("-RIGHT BEFORE EVENT CALL-");
        roomClient.Join(joinCode); // Join Room Based on Room Code
        Debug.Log("-RIGHT AFTER EVENT CALL-");
    }
    private void OnJoinedExistingRoom(IRoom room)
    {
        Debug.Log("Joined room: " + room.Name);
        Debug.Log(room.Name + " JoinCode: " + room.JoinCode);
        Debug.Log(room.Name + " UUID: " + room.UUID);
        Debug.Log(room.Name + " Publish: " + room.Publish);

        roomClient.OnJoinedRoom.RemoveListener(OnJoinedExistingRoom);
    }

    // ========= LEAVE ROOM =========
    public void LeaveRoom()
    {
        Debug.Log("=== LEAVING ROOM ==="); // Log the project ID
        roomClient.Join("", false);

        RealityFlowAPI.Instance.DespawnAllObjectsInBothDictionarys();
        // levelEditor.SetActive(false);
        projectManager.SetActive(true);


    }

    private UnityWebRequest CreateWebRequest(GraphQLRequest payload)
    {
        // Describe request
        UnityWebRequest request = UnityWebRequest.Post(server + graphQLRoute,
        JsonConvert.SerializeObject(payload), "application/json");
        if (accessToken != null && accessToken != "")
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        return request;
    }

    private static JObject ProcessQueryResponse(UnityWebRequest request)
    {
        // Handle response
        JObject response = null;

        // Handle network issues
        if (request.result != UnityWebRequest.Result.Success
                && request.downloadHandler.text == "")
        {
            response = new JObject();
            var errors = new JArray();
            var error = new JObject();
            var errorExtensions = new JObject();

            error.Add("message", request.error);
            errorExtensions.Add("code", "INTERNAL_SERVER_ERROR");
            error.Add("extensions", errorExtensions);
            errors.Add(error);

            response.Add("errors", errors);
        }

        // Handle response
        if (response == null)
        {
            try
            {
                // Debug.Log(request.downloadHandler.text);
                response = JsonConvert.DeserializeObject<JObject>(
                    request.downloadHandler.text // This is failing
                );
            }
            catch (Exception e)
            {
                Debug.Log(response);
                Debug.LogException(e);
                return new JObject();
            }
        }

        // Display any errors
        if (response["errors"] != null)
        {
            Debug.LogError("GraphQL Errors");
            foreach (JObject error in response["errors"])
            {
                StringBuilder err = new(); 
                err.Append(error["message"].ToString() ?? "unknown error");
                if (error["extensions"] is JObject exts)
                    err.Append($": {exts["code"]}");
                Debug.LogError(err);
            }
        }

        return response;
    }

    // public JObject await SendQueryAsync(GraphQLRequest payload)
    // {
    //     UnityWebRequest request = CreateWebRequest(payload);

    //     // Send request
    //     double start = Time.realtimeSinceStartupAsDouble;
    //     UnityWebRequestAsyncOperation task = request.SendWebRequest();
    //     while (!task.isDone)
    //         Thread.Sleep(1);
    //     double end = Time.realtimeSinceStartupAsDouble;
    //     Debug.Log($"Blocking query took {(end - start) * 1000}ms to complete");

    //     return ProcessQueryResponse(request);
    // }

    public void SendQueryCoroutine(GraphQLRequest payload, Action<UnityWebRequest> onComplete = null)
    {
        UnityWebRequest request = CreateWebRequest(payload);

        // Send request
        UnityWebRequestAsyncOperation task = request.SendWebRequest();
        static IEnumerator WaitForRequestCompletion(
            UnityWebRequestAsyncOperation task, 
            UnityWebRequest request, 
            Action<UnityWebRequest> onComplete,
            System.Diagnostics.StackTrace trace
        )
        {
            double start = Time.realtimeSinceStartupAsDouble;
            yield return task;
            double end = Time.realtimeSinceStartupAsDouble;
            Debug.Log($"Fire & Forget query took {(end - start) * 1000}ms to complete");

            ProcessQueryResponse(request);

            onComplete?.Invoke(request);
        }

        StartCoroutine(WaitForRequestCompletion(
            task, 
            request, 
            onComplete, 
            new System.Diagnostics.StackTrace(true)
        ));
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

    public async Task OpenProject(string id)
    {
        Debug.Log("Opening project with ID: " + id);
        SetCurrentProject(id);
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
            //Debug.Log("Fetched project data: " + projectdata.ToString());

            // Set the project details in the UI
            OnProjectUpdated.Invoke((JObject)projectdata);
            var roomsData = projectdata["getProjectById"]["rooms"];
            if (roomsData != null)
            {
                //Debug.Log("Fetched rooms data: " + roomsData.ToString());
                //Debug.Log("Rooms: " + roomsData.ToString());
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
    public async Task GetRoomsByProjectId(string projectId)
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
            // Debug.Log("Fetched rooms data: " + roomsData.ToString());
            rooms = (JArray)roomsData["getRoomsByProjectId"];
            // Debug.Log("Rooms: " + rooms.ToString());
        }
        else
        {
            Debug.LogError("Failed to fetch rooms data");
        }
        OnRoomsReceived?.Invoke(rooms);
    }

    #endregion
}