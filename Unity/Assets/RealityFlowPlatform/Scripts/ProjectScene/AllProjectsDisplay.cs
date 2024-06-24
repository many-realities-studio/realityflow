// Purpose: This script is responsible for displaying all the projects available on the platform.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ProjectDisplay : MonoBehaviour
{
    // UI elements
    public GameObject projectPrefab;       
    public GameObject projectDetailPanel;  
    public GameObject projectTitle;        
    public GameObject projectDescription;  
    public GameObject projectOwner; 
    public Transform parentContent; 
    private JObject data;
    public bool tutorialsOnly;

    // GraphQL client and access token variables
    private RealityFlowClient rfClient;
    public Transform roomsContent;
    public GameObject roomUI;
    public Button createRoomBtn;


    void Start()
    {
        rfClient = RealityFlowClient.Find(this);
        GetProjectsData();
        rfClient.OnProjectUpdated += UpdateProject;
    }

    private void UpdateProject(JObject project) {
        projectDetailPanel.SetActive(true); // Set the project detail panel to active
        projectTitle.GetComponent<TextMeshProUGUI>().text = (string)project["getProjectById"]["projectName"];
        projectDescription.GetComponent<TextMeshProUGUI>().text = (string)project["getProjectById"]["description"];
        projectOwner.GetComponent<TextMeshProUGUI>().text = "by " + (string)project["getProjectById"]["projectOwner"]["username"];
    }

    // Function to get the projects data
    private void GetProjectsData()
    {
        Debug.Log("Getting public projects");
        // Create a new GraphQL query request to get the public projects available on the platform.
        var publicProjectsQuery = new GraphQLRequest
        {
            Query = @"
                  query Query {
                        getPublicProjects {
                            isPublic
                            isTutorial
                            id
                            gallery
                            projectName
                            categories
                        }
                }
            ",
            OperationName = "Query"
        };

        // Send the query request asynchronously and wait for the response.
        var queryResult = rfClient.SendQueryAsync(publicProjectsQuery);
        var data = queryResult["data"];  // Store the data received from the query

        // If the data is not null, display the projects on the platform.
        if (data != null)
        {
            Debug.Log("data isn't null");
                        // Get the list of projects from the data
            var projects = (JArray)data["getPublicProjects"];
            for (int i = 0; i < projects.Count; i++)
            {
              if(tutorialsOnly && !projects[i]["isTutorial"]) {
                continue;
              }
                var project = GameObject.Instantiate(projectPrefab, transform.GetChild(0).GetChild(0).transform);  // Instantiate the project prefab
                var children = new List<GameObject>();
                project.GetChildGameObjects(children);  // Get the child game objects of the project prefab
                foreach (var child in children)             
                {
                    if (child.name == "ProjectInfo")
                    {
                        var children2 = new List<GameObject>();
                        child.GetChildGameObjects(children2);
                        foreach (var c in children2)
                        {
                            Debug.Log(child.name);
                            if (c.name == "ProjectTitle")
                            {
                                int x = i;
                                c.GetComponent<TextMeshProUGUI>().text = (string)projects[x]["projectName"];
                            }
                            else if (c.name == "Categories")
                            {
                                int x = i;
                                c.GetComponent<TextMeshProUGUI>().text = string.Join(" | ",projects[x]["categories"]);
                            }
    
                            else if (c.name == "Publicity")
                            {
                                int x = i;
                                bool isPublic = (bool)projects[x]["isPublic"];
                                c.GetComponent<TextMeshProUGUI>().text = isPublic ? "public" : "private";
                            }
                        }

                    }
                    else if (child.name == "OpenProjectBtn")
                    {
                        int x = i;
                        child.GetComponent<Button>().onClick
                            .AddListener(delegate { 
                                rfClient.OpenProject((string)projects[x]["id"]); 
                            });
                    }

                }
            }
            int numRows = (projects.Count / 4) + 1;
            // transform.GetChild(0).GetChild(0).transform
            //     .GetComponent<RectTransform>().sizeDelta = 
            //         new Vector2(1001, (numRows > 2) ? 168 * numRows : 168 * 2);
        }
        if (queryResult["errors"] != null)
        {
            Debug.Log(queryResult["errors"][0]["message"]);
        }

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

        // Function to display the rooms in the project
    private void displayRooms(JArray rooms)
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
                    child.GetComponent<Button>().onClick.AddListener(() => CallJoinRoom(joinCode));
                }
            }
        }
    }
    // Wrapper method to call RoomManager's JoinRoom
    public void CallJoinRoom(string joinCode)
    {  
        Debug.Log("[JOIN ROOM]Join code!: " + joinCode);
        rfClient.roomClient.Join(joinCode);
    }
}
