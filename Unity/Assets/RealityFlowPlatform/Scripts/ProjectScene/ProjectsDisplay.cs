using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ProjectsDisplay : MonoBehaviour
{
    // Project UI elements
    public GameObject myProjectsPanel;
    public GameObject projectsCoOwnedPanel;
    public GameObject projectsJoinedPanel;
    public GameObject activeProjectsPanel;
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

    #region Initialization
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

        GetUserProjectsData();
        GetActiveProjectsData();
    }
    #endregion

    #region Display Projects
    private async Task GetUserProjectsData()
    {
        // Check For Essential Components
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

        // Get the user's projects
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

        var queryResult = await rfClient.SendQueryAsync(projectsQuery);
        var data = queryResult["data"];
        if (data != null)
        {
            var myProjects = (JArray)data["getUserById"]["projectsOwned"];
            var projectsCoOwned = (JArray)data["getUserById"]["projectsCoOwned"];
            var projectsJoined = (JArray)data["getUserById"]["projectsJoined"];

            DisplayUserProjects(myProjects, myProjectsPanel);
            DisplayUserProjects(projectsCoOwned, projectsCoOwnedPanel);
            DisplayUserProjects(projectsJoined, projectsJoinedPanel);
        }
        if (queryResult["errors"] != null)
        {
            Debug.Log(queryResult["errors"][0]["message"]);
        }
    }

    public async Task GetActiveProjectsData()
    {
        // Debug.Log("--- Fetching Active Projects Data ---");

        // Updated Query Call
        var activeProjectsQuery = new GraphQLRequest
        {
            Query = @"
                query {
                    getActiveProjects {
                        id
                        projectName
                        description
                        projectOwner {
                            username
                        }
                        rooms {
                            id
                            joinCode
                            isEditable
                        }
                    }
                }
            ",
            Variables = null
        };

        var graphQL = await rfClient.SendQueryAsync(activeProjectsQuery);
        var projectsData = graphQL["data"]["getActiveProjects"] as JArray;

        if (projectsData != null)
        {
            // Debug.Log($"Fetched {projectsData.Count} active projects.");
            DisplayActiveProjects(projectsData, activeProjectsPanel); // Pass the fetched data to the display method
        }
        else
        {
            Debug.LogError("Failed to fetch active projects");
        }
    }

    private void DisplayUserProjects(JArray projects, GameObject parentPanel)
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
                }
            }

            // Find and set up the join button
            Button joinButton = project.GetComponent<Button>();
            if (joinButton != null)
            {
                string projectId = (string)projects[i]["id"];
                joinButton.onClick.AddListener(() => OpenProject(projectId));
            }
            else
            {
                Debug.LogWarning("Button component not found in the Project prefab.");
            }
        }
    }

    // DisplayActive Projects
    public void DisplayActiveProjects(JArray projectsData, GameObject parentPanel)
    {
        // Debug.Log("--- Displaying Active Projects ---");
        ClearActiveProjects();
        
        if (projectsData != null)
        {
            foreach (var project in projectsData)
            {
                // Debug.Log($"Processing project: {project["projectName"]}");
                // Instantiate a new ProjectUI prefab
                GameObject projectUIInstance = Instantiate(projectPrefab, parentPanel.transform);

                if (projectUIInstance == null)
                {
                    Debug.LogError("Failed to instantiate ProjectUI prefab.");
                    continue;
                }

                // Ensure the instantiated UI element is active
                projectUIInstance.SetActive(true);

                // Find the project info
                Transform projectInfo = projectUIInstance.transform.Find("ProjectInfo");
                if (projectInfo == null)
                {
                    Debug.LogError("ProjectInfo not found in the instantiated ProjectUI prefab.");
                    continue;
                }

                // Find the project title Text component
                TextMeshProUGUI projectTitleText = projectInfo.Find("ProjectTitle")?.GetComponent<TextMeshProUGUI>();
                if (projectTitleText == null)
                {
                    Debug.LogError("ProjectTitle Text component not found in the ProjectInfo.");
                    continue;
                }

                // Set the project title
                projectTitleText.text = project["projectName"].ToString();
                // Debug.Log($"Set project title to: {project["projectName"]}");

                // Find and set up the join button
                Button joinButton = projectUIInstance.GetComponent<Button>();
                if (joinButton != null)
                {
                    string projectId = project["id"].ToString();
                    joinButton.onClick.AddListener(() => OpenProject(projectId));
                    // Debug.Log($"Set up join button for project: {project["projectName"]}");
                }
                else
                {
                    Debug.LogWarning("Button component not found in the ProjectUI prefab.");
                }
            }
        }
        else
        {
            Debug.LogError("No projects data to display");
        }
    }
    
    
    public async Task OpenProject(string id)
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
                        rooms {
                            id
                            udid
                            joinCode
                            isEditable
                            creatorId
                        }
                    }
                }",
            OperationName = "GetProjectById",
            Variables = new { getProjectByIdId = id }
        };

        var graphQL = await rfClient.SendQueryAsync(getProjectData);
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

    #region Display Rooms
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

    private void ClearActiveProjects()
    {
        foreach (Transform child in activeProjectsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

}