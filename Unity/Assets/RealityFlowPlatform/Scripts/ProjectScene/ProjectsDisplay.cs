using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Text;

public class ProjectsDisplay : MonoBehaviour
{
    // Project UI elements
    public GameObject myProjectsPanel;
    public GameObject projectsCoOwnedPanel;
    public GameObject projectsJoinedPanel;
    public GameObject projectPrefab;
    public GameObject projectDetailPanel;
    public GameObject projectTitle;
    public GameObject projectDescription;
    public GameObject projectOwner;

    // Room UI elements
    public Transform roomsContent;
    public GameObject roomUI;
    public Button createRoomBtn;
    public Button refreshRoomsBtn;

    // GraphQL client and access token variables
    private RealityFlowClient rfClient;

    void Awake()
    {
        rfClient = RealityFlowClient.Find(this);
        if (rfClient == null)
        {
            Debug.LogError("RealityFlowClient not found.");
            return;
        }

        rfClient.LoginSuccess += OnLoginSuccess; // Subscribe to the login success event
    }

    private void OnLoginSuccess(bool success)
    {
        if (success)
        {
            Initialize();
        }
        else
        {
            Debug.LogError("Login failed.");
        }
    }

    private void Initialize()
    {
        if (rfClient.userDecoded == null)
        {
            Debug.LogError("userDecoded is null.");
            return;
        }

        if (!rfClient.userDecoded.ContainsKey("id"))
        {
            Debug.LogError("userDecoded does not contain 'id'.");
            return;
        }

        rfClient.OnRoomsReceived += DisplayRooms;
        rfClient.OnProjectUpdated += UpdateProject;

        if (createRoomBtn != null)
        {
            createRoomBtn.onClick.AddListener(rfClient.CreateRoom);
        }
        else
        {
            Debug.LogError("CreateRoomBtn is not assigned.");
        }

        GetProjectsData();
    }

    #region Projects
    private void GetProjectsData()
    {
        if (rfClient == null)
        {
            Debug.LogError("rfClient is null.");
            return;
        }

        if (rfClient.userDecoded == null)
        {
            Debug.LogError("userDecoded is null.");
            return;
        }

        if (!rfClient.userDecoded.ContainsKey("id"))
        {
            Debug.LogError("userDecoded does not contain 'id'.");
            return;
        }

        var userId = rfClient.userDecoded["id"];
        var projectsQuery = new GraphQLRequest
        {
            Query = @"
                 query GetUserProjects($getUserByIdId: String!) {
                    getUserById(id: $getUserByIdId) {
                        projectsOwned {
                            categories
                            id
                            isPublic
                            projectName
                        }
                        projectsJoined {
                            id
                            categories
                            isPublic
                            projectName
                        }
                        projectsCoOwned {
                            id
                            categories
                            isPublic
                            projectName
                        } 
                    }
                }
            ",
            OperationName = "GetUserProjects",
            Variables = new { getUserByIdId = userId }
        };

        var queryResult = rfClient.SendQueryBlocking(projectsQuery);
        var data = queryResult["data"];
        if (data != null)
        {
            var myProjects = (JArray)data["getUserById"]["projectsOwned"];
            var projectsCoOwned = (JArray)data["getUserById"]["projectsCoOwned"];
            var projectsJoined = (JArray)data["getUserById"]["projectsJoined"];

            DisplayProjects(myProjects, myProjectsPanel);
            DisplayProjects(projectsCoOwned, projectsCoOwnedPanel);
            DisplayProjects(projectsJoined, projectsJoinedPanel);
        }
        if (queryResult["errors"] != null)
        {
            Debug.Log(queryResult["errors"][0]["message"]);
        }
    }

    private void DisplayProjects(JArray projects, GameObject parentPanel)
    {
        if (projects.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < projects.Count; i++)
        {
            var project = GameObject.Instantiate(projectPrefab, parentPanel.transform, false);
            var children = new List<GameObject>();
            project.GetChildGameObjects(children);
            foreach (var child in children)
            {
                if (child.name == "ProjectInfo")
                {
                    var children2 = new List<GameObject>();
                    child.GetChildGameObjects(children2);
                    foreach (var c in children2)
                    {
                        if (c.name == "ProjectTitle")
                        {
                            c.GetComponent<TextMeshProUGUI>().text = (string)projects[i]["projectName"];
                        }
                    }
                    int x2 = i;
                    child.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        OpenProject((string)projects[x2]["id"]);
                    });
                }
            }
        }
    }

    public void OpenProject(string id)
    {
        rfClient.SetCurrentProject(id);
        projectDetailPanel.SetActive(true);

        var getProjectData = new GraphQLRequest
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
                }
            }",
            OperationName = "GetProjectById",
            Variables = new { getProjectByIdId = id }
        };

        var graphQL = rfClient.SendQueryBlocking(getProjectData);
        var projectdata = graphQL["data"];
        if (projectdata != null)
        {
            projectTitle.GetComponent<TextMeshProUGUI>().text = (string)projectdata["getProjectById"]["projectName"];
            projectDescription.GetComponent<TextMeshProUGUI>().text = (string)projectdata["getProjectById"]["description"];
            projectOwner.GetComponent<TextMeshProUGUI>().text = "by " + (string)projectdata["getProjectById"]["projectOwner"]["username"];

            rfClient.GetRoomsByProjectId(id);
        }
        else
        {
            Debug.LogError("Failed to fetch project data");
        }
    }

    private void UpdateProject(JObject project)
    {
        projectDetailPanel.SetActive(true);
        projectTitle.GetComponent<TextMeshProUGUI>().text = (string)project["getProjectById"]["projectName"];
        projectDescription.GetComponent<TextMeshProUGUI>().text = (string)project["getProjectById"]["description"];
        projectOwner.GetComponent<TextMeshProUGUI>().text = "by " + (string)project["getProjectById"]["projectOwner"]["username"];
    }
    #endregion

    #region Rooms
    private void DisplayRooms(JArray rooms)
    {
        foreach (Transform child in roomsContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            var room = GameObject.Instantiate(roomUI, roomsContent, false);
            var children = new List<GameObject>();
            room.GetChildGameObjects(children);
            foreach (var child in children)
            {
                if (child.name == "JoinButton")
                {
                    int x = i;
                    string joinCode = (string)rooms[x]["joinCode"];
                    child.GetComponent<Button>().onClick.AddListener(() => rfClient.JoinRoom(joinCode));
                }
            }
        }
    }
    #endregion

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
}