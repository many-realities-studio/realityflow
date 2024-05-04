using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class MyProjectsDisplay : MonoBehaviour
{
    public GameObject myProjectsPanel;
    public GameObject projectsCoOwnedPanel;
    public GameObject projectsJoinedPanel;
    public GameObject projectPrefab;
    public GameObject projectDetailPanel;


    public GameObject projectTitle;
    public GameObject projectDescription;
    public GameObject projectOwner;
    public GameObject createRoomBtn;

    public GraphQLHttpClient graphQLClient;

    private string accessToken;
    private JObject data;
    private Dictionary<string, string> userDecoded;
    // Start is called before the first frame update
    void Start()
    {
        accessToken = PlayerPrefs.GetString("accessToken");
        userDecoded = DecodeJwt(accessToken);
        Debug.Log(accessToken);
        var graphQLC = new GraphQLHttpClient("http://localhost:4000/graphql", new NewtonsoftJsonSerializer());
        graphQLC.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        graphQLClient = graphQLC;
        getProjectsData();
    }
    private async void getProjectsData()
    {
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
            Variables = new {getUserByIdId = userDecoded["id"]}
     
        };
        var queryResult = await graphQLClient.SendQueryAsync<JObject>(projectsQuery);
        data = queryResult.Data;
        if (data != null)
        {

            var myProjects = (JArray)data["getUserById"]["projectsOwned"];
            var projectsCoOwned = (JArray)data["getUserById"]["projectsCoOwned"];
            var projectsJoined = (JArray)data["getUserById"]["projectsJoined"];

            
            Debug.Log(myProjects);
            displayProjects(myProjects, myProjectsPanel);
            displayProjects(projectsCoOwned, projectsCoOwnedPanel);
            displayProjects(projectsJoined, projectsJoinedPanel);
            Debug.Log(data["getUserById"]);
        }
        if (queryResult.Errors != null)
        {
            Debug.Log(queryResult.Errors[0].Message);
        }

    }
    private void displayProjects(JArray projects, GameObject parentPanel)
    {
        Debug.Log("Displaying projects...");
        if (projects.Count <= 0)
        {
            Transform parent = parentPanel.transform.parent;
            GameObject textChild = parent.GetChild(1).gameObject;
            textChild.GetComponent<TextMeshProUGUI>().text = "No projects found...";
            return;
        }
        for (int i = 0; i < projects.Count; i++)
        {
            var project = GameObject.Instantiate(projectPrefab, parentPanel.transform,false);
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
                            int x = i;
                            c.GetComponent<TextMeshProUGUI>().text = (string)projects[x]["projectName"];
                        }
                        else if (c.name == "Categories")
                        {
                            int x = i;
                            c.GetComponent<TextMeshProUGUI>().text = string.Join(" | ", projects[x]["categories"]);
                        }

                        else if (c.name == "Publicity")
                        {
                            int x = i;
                            bool isPublic = (bool)projects[x]["isPublic"];
                            c.GetComponent<TextMeshProUGUI>().text = isPublic ? "public" : "private";
                        }
                    }

                }
                else if (child.name == "ProjectBtn")
                {
                    int x = i;
                    child.GetComponent<Button>().onClick.AddListener(delegate { openProject((string)projects[x]["id"]); });
                }

            }

            // project.transform.SetParent(parentPanel.transform);
        }
    }
    public async void openProject(string id)
    {
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
                }
                ",
            OperationName = "GetProjectById",
            Variables = new { getProjectByIdId = id }
        };

        var graphQL = await graphQLClient.SendQueryAsync<JObject>(getProjectData);


        var projectdata = graphQL.Data;
        projectTitle.GetComponent<TextMeshProUGUI>().text = (string)projectdata["getProjectById"]["projectName"];
        projectDescription.GetComponent<TextMeshProUGUI>().text = (string)projectdata["getProjectById"]["description"];
        projectOwner.GetComponent<TextMeshProUGUI>().text = "by " + (string)projectdata["getProjectById"]["projectOwner"]["username"];
        createRoomBtn.GetComponent<Button>().onClick.AddListener(delegate { createRoomInstance(id); });
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
    public async void createRoomInstance(string projectId)
    {
        Debug.Log("Creating room with id: " + projectId);
        Debug.Log("Creating room with id: " + userDecoded["username"] + '@' + projectId);
            
        Debug.Log(userDecoded["username"] + '@' + projectId);
        // mainMenu.roomClient.Join(
        //     name: userDecoded["username"] + '@' + projectId,
        //     publish: true) ;
    // Create a mutation that adds a room using C#
        var addRoom = new GraphQLRequest
        {
            Query = @"
            mutation AddRoom($input: AddRoomInput) {
                addRoom(input: $input) {
                    id
                }
            }
        ",
            OperationName = "AddRoom",
            Variables = new { input = new
            {
                roomId =  userDecoded["username"] + '@' + projectId,
                projectId = projectId,
                creatorId = userDecoded["username"],
                isEditable = true, // or some other value
                joinCode = "AXBO" // function to generate join code
            } }
        };

        var graphQL = await graphQLClient.SendMutationAsync<JObject>(addRoom);
        if (graphQL.Data != null)
        {
            Debug.Log(graphQL.Data);
        }
        if(graphQL.Errors != null) {
            foreach(var error in graphQL.Errors) {
                Debug.Log(error.Message);
            }
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
