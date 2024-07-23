using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Ubiq.Rooms;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Collections;
using Samples.Whisper;
using TMPro;


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
    public Transform projectsPanel;
    public GameObject projectUIPrefab; 
    public GameObject DiscoveryPanelDetail;
    public GameObject RoomDescriptionPanel;

    private static RealityFlowClient rootRealityFlowClient;

    #if REALITYFLOW_LIVE
        public string server = @"https://reality.gaim.ucf.edu/";
    #else
        public string server = @"http://localhost:4000/";
    #endif

    private void Awake()
    {
        // Debug.Log(" === RealityFlowClient Awake === ");

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

        accessToken = PlayerPrefs.GetString("accessToken");

    }

    public void Start()
    {
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

    public JObject SendQueryAsync(GraphQLRequest payload)
    {
        // Describe request
        UnityWebRequest request = UnityWebRequest.Post(server + graphQLRoute,
        JsonConvert.SerializeObject(payload), "application/json");
        if (accessToken != null && accessToken != "")
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        // Send request
        // TODO: Actually make this occur over multiple frames by way of coroutines. 
        // Seems to take 50-100ms on a decent connection, which will drop multiple frames if not
        // asynchronous.
        double start = Time.realtimeSinceStartupAsDouble;
        UnityWebRequestAsyncOperation task = request.SendWebRequest();
        while (!task.isDone)
            Thread.Sleep(1);
        double end = Time.realtimeSinceStartupAsDouble;
        Debug.Log($"Query took {(end - start) * 1000}ms to complete");

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

    public static Dictionary<string, string> DecodeJwt(string jwt)
    {
        string[] jwtParts = jwt.Split('.');
        byte[] decodedPayload = FromBase64Url(jwtParts[1]);
        string decodedText = Encoding.UTF8.GetString(decodedPayload);

        Dictionary<string, string> jwtPayload = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedText);

        return jwtPayload;
    }

    static byte[] FromBase64Url(string base64Url)
    {
        string padded = base64Url.Length % 4 == 0
        ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
        string base64 = padded.Replace("_", "/").Replace("-", "+");
        return Convert.FromBase64String(base64);
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

    private static JObject ProcessQueryResponse(UnityWebRequest request, System.Diagnostics.StackTrace stacktrace)
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
                response = JsonConvert.DeserializeObject<JObject>(
                    request.downloadHandler.text
                );
            }
            catch (Exception e)
            {
                Debug.Log(response);
                Debug.LogException(e);
                Debug.LogError($"Stacktrace: {stacktrace}");
                return new JObject();
            }
        }

        // Display any errors
        if (response["errors"] != null)
        {
            Debug.LogError("GraphQL Errors");
            Debug.LogError($"Stacktrace: {stacktrace}");
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

    public JObject SendQueryBlocking(GraphQLRequest payload)
    {
        UnityWebRequest request = CreateWebRequest(payload);

        // Send request
        double start = Time.realtimeSinceStartupAsDouble;
        UnityWebRequestAsyncOperation task = request.SendWebRequest();
        while (!task.isDone)
            Thread.Sleep(1);
        double end = Time.realtimeSinceStartupAsDouble;
        // Debug.Log($"Blocking query took {(end - start) * 1000}ms to complete");

        return ProcessQueryResponse(request, new System.Diagnostics.StackTrace());
    }

    public void SendQueryFireAndForget(GraphQLRequest payload, Action<UnityWebRequest> onComplete = null)
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

            ProcessQueryResponse(request, trace);

            onComplete?.Invoke(request);
        }

        StartCoroutine(WaitForRequestCompletion(
            task, 
            request, 
            onComplete, 
            new System.Diagnostics.StackTrace(true)
        ));
    }
    #endregion

    #region Login Methods
    private void ShowOTP()
    {
        projectManager.SetActive(false);
        loginMenu.gameObject.SetActive(true);
        loginMenu.onOTPSubmitted += SubmitOTP;
    }

    public void SubmitOTP(string otp)
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
        var queryResult = SendQueryBlocking(verifyOTP);
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

    public void Login(string inputAccessToken)
    {
        Debug.Log("Logging in....");

        userDecoded = DecodeJwt(inputAccessToken);

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
        var graphQL = SendQueryBlocking(verifyToken);
        if (graphQL["errors"] == null && graphQL["data"]["verifyAccessToken"] != null)
        {
            accessToken = inputAccessToken;
            PlayerPrefs.SetString("accessToken", accessToken);

            if (Whisper.rootWhisper != null)
            {
                Whisper.rootWhisper.InitializeGPT((string)graphQL["data"]["verifyAccessToken"]["apiKey"]);
            }
            LoginSuccess?.Invoke(true); // Notify that login was successful
        }
        else
        {
            Debug.LogError("Failed to log in");
            accessToken = "";
            PlayerPrefs.SetString("accessToken", "");
            LoginSuccess?.Invoke(false); // Notify that login failed
        }
    }
    #endregion

    #region Project Methods
    public void CreateProject()
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
        var graphQL = SendQueryBlocking(createProject);
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

    public void SetCurrentProject(string projectId)
    {
        //Debug.Log("Setting current project ID to: " + projectId);
        currentProjectId = projectId;
        PlayerPrefs.SetString("currentProjectId", currentProjectId);
    }

    public string GetCurrentProjectId()
    {
        return currentProjectId;
    }

    #endregion

    #region Room Methods
    public void GetRoomsByProjectId(string projectId)
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

        var graphQL = SendQueryBlocking(getRooms);
        var roomsData = graphQL["data"];
        JArray rooms = null;
        if (roomsData != null)
        {
            rooms = (JArray)roomsData["getRoomsByProjectId"];
        }
        else
        {
            Debug.LogError("Failed to fetch rooms data");
        }
        OnRoomsReceived?.Invoke(rooms);
    }

    public void CreateRoom()
    {
        Debug.Log("Creating room for project: " + currentProjectId); // Log the project ID

        // Check if a project is selected
        if (string.IsNullOrEmpty(currentProjectId))
        {
            Debug.LogError("No project selected");
            return;
        }

        // Hide Project and Login Menus, show Level Editor
        projectManager.SetActive(false);

        roomClient.OnJoinedRoom.AddListener(OnJoinCreatedRoom); //!ON CREATE ROOM SHOULD TRIGGER!

        // Create a new room using the RoomClient
        roomClient.Join("User Created Room", false); // Name: , Publish: false

        RealityFlowAPI.Instance.FetchAndPopulateObjects();
    }

    public void OnJoinCreatedRoom(IRoom room)
    {
        Debug.Log("[CREATED ROOM]");
        // Debug.Log("Created Room: " + room.Name);// ON CREATE ROOM SHOULD TRIGGER!

        Debug.Log(room.Name + " JoinCode: " + room.JoinCode);
        Debug.Log(room.Name + " UUID: " + room.UUID);
        Debug.Log(room.Name + " Publish: " + room.Publish);

        levelEditor.SetActive(true);

        //RealityFlowAPI.Instance.FetchAndPopulateObjects();

        // Create a new room using the GraphQL API
        var addRoom = new GraphQLRequest
        {
            Query = @"
            mutation AddRoom($input: AddRoomInput!, $input2: UpdateExpiredTimeInput!, $input3: UpdateUserRoomIdInput!) {
                addRoom(input: $input) {
                    id
                }
                updateExpiredTime(input: $input2) {
                    id
                }
                updateUserRoomId(input: $input3) {
                    id
                }
            }
        ",
            OperationName = "AddRoom",
            Variables = new
            {
                input3 = new
                {
                    userId = userDecoded["id"],
                    defaultProjectId = currentProjectId,
                    newRoomId = room.UUID
                },
                input2 = new
                {
                    userId = userDecoded["id"]
                },
                input = new
                {
                    projectId = currentProjectId,
                    // creatorId = userDecoded["id"], seems to have been removed from schema
                    roomId = room.UUID,
                    joinCode = room.JoinCode,
                    isEditable = true
                }
            }
        };
        var graphQL = SendQueryBlocking(addRoom);
        if (graphQL["data"] != null)
        {
            Debug.Log("Room created successfully");
            GetRoomsByProjectId(currentProjectId);
        }
        else
        {
            Debug.LogError("Failed to create room: Room may already exist.");
        }

        // Run KeepRoomAlive every 30 seconds
        InvokeRepeating("KeepRoomAlive", 0, 30);


        roomClient.OnJoinedRoom.RemoveListener(OnJoinCreatedRoom);

        OnRoomCreated?.Invoke();
    }

    private void KeepRoomAlive()
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
        var graphQL = SendQueryBlocking(updateExpiredTime);
        if (graphQL["data"] != null)
        {
            //Debug.Log("Room alive");
        }
        else
        {
            Debug.LogError("Failed to keep room alive");
        }
    }
     
    public void JoinRoom(string joinCode)
    {
        Debug.Log("Joining room for project: " + currentProjectId); // Log the project ID
        //Debug.Log("Joining room with join code: " + joinCode); // Log the join code

        // Check if a project is selected
        if (string.IsNullOrEmpty(currentProjectId))
        {
            Debug.LogError("No project selected");
            return;
        }
        projectManager.SetActive(false);  // MAYBE CHECK FOR SUCCESSFUL JOIN BEFORE HIDING

        roomClient.OnJoinedRoom.AddListener(OnJoinedExistingRoom);

        roomClient.Join(joinCode); // Join Room Based on Room Code

    }

    private void OnJoinedExistingRoom(IRoom room)
    {
        Debug.Log("[JOINED CREATED ROOM]");
        Debug.Log("Joined room: " + room.Name);
        Debug.Log(room.Name + " JoinCode: " + room.JoinCode);
        Debug.Log(room.Name + " UUID: " + room.UUID);
        Debug.Log(room.Name + " Publish: " + room.Publish);

        levelEditor.SetActive(true); 

        roomClient.OnJoinedRoom.RemoveListener(OnJoinedExistingRoom);
    }

    public void LeaveRoom()
    {
        Debug.Log("=== LEAVING ROOM ==="); // Log the project ID

        roomClient.Join("", false);

        RealityFlowAPI.Instance.DespawnAllObjectsInBothDictionarys();

        projectManager.SetActive(true);


    }

    #endregion
}