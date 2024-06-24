using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;


public class MyProjectsDisplay : MonoBehaviour
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
    private JObject data;
    public GameObject roomUI;
    public Button createRoomBtn;
    public Button refreshRoomsBtn;

    // GraphQL client and access token variables 
    // public GraphQLHttpClient graphQLClient;
    private RealityFlowClient rfClient;

    void Start()
    {
        rfClient = RealityFlowClient.Find(this);
        rfClient.OnRoomsReceived += DisplayRooms;
        // CREATE ROOM BUTTON LISTENER
        if (rfClient != null)
        {
            // TODO -ReFresh Button-
            // refreshRoomsBtn.onClick.AddListener(rfClient.GetRoomsByProjectId);
            createRoomBtn.onClick.AddListener(rfClient.CreateRoom);
            // Debug.Log("RoomManager found and listener added.");
        }
        else
        {
            Debug.LogError("RoomManager not found.");
        }
        getProjectsData();
    }

    private void getProjectsData()
    {
        if (rfClient == null)
        {
            return;
        }
        var userId = rfClient.userDecoded["id"];
        // Create a new GraphQL query request to get the projects owned, co-owned, and joined by the user.
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

        // Send the query request to the GraphQL server
        var queryResult = rfClient.SendQueryAsync(projectsQuery);
        var data = queryResult["data"];  //Get the data from the query result
        if (data != null)
        {
            // Get the projects owned, co-owned, and joined by the user
            var myProjects = (JArray)data["getUserById"]["projectsOwned"];
            var projectsCoOwned = (JArray)data["getUserById"]["projectsCoOwned"];
            var projectsJoined = (JArray)data["getUserById"]["projectsJoined"];

            // Display the projects owned, co-owned, and joined by the user
            displayProjects(myProjects, myProjectsPanel);
            displayProjects(projectsCoOwned, projectsCoOwnedPanel);
            displayProjects(projectsJoined, projectsJoinedPanel);
        }
        if (queryResult["errors"] != null)
        {
            Debug.Log(queryResult["errors"][0]["message"]);
        }
    }

    // Function to display the projects on the platform
    private void displayProjects(JArray projects, GameObject parentPanel)
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
                        // else if (c.name == "Categories")
                        // {
                        //     int x = i;
                        //     c.GetComponent<TextMeshProUGUI>().text = string.Join(" | ", projects[x]["categories"]);
                        // }

                        // else if (c.name == "Publicity")
                        // {
                        //     int x = i;
                        //     bool isPublic = (bool)projects[x]["isPublic"];
                        //     c.GetComponent<TextMeshProUGUI>().text = isPublic ? "public" : "private";
                        // }
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

    // Function to open the project details panel
    public void OpenProject(string id)
    {
        // Debug.Log("Opening project with ID: " + id);
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

        var graphQL = rfClient.SendQueryAsync(getProjectData);

        var projectdata = graphQL["data"];
        if (projectdata != null)
        {
            // Debug.Log("Fetched project data: " + projectdata.ToString());

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


    // Function to display the rooms in the project
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
}
