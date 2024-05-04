using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Ubiq.Samples;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ProjectDisplay : MonoBehaviour
{
    public GameObject parentContent;
    public GameObject projectPrefab;
    public GameObject projectDetailPanel;
    

    public GameObject projectTitle;
    public GameObject projectDescription;
    public GameObject projectOwner;
    public GameObject createRoomBtn;
    public GameObject refreshRoomsBtn;

    public SocialMenu mainMenu;

    public GraphQLHttpClient graphQLClient;

    private string accessToken;
    private JObject data;
    private Dictionary<string, string> userDecoded;
    // Start is called before the first frame update
    /**/
    void Start()
    {
        accessToken = PlayerPrefs.GetString("accessToken");
        userDecoded = DecodeJwt(accessToken);
        var graphQLC = new GraphQLHttpClient("http://localhost:4000/graphql", new NewtonsoftJsonSerializer());
        graphQLC.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        graphQLClient = graphQLC;
        getProjectsData();
       

    }
    private async void getProjectsData()
    {
        var publicProjectsQuery = new GraphQLRequest
        {
            Query = @"
                  query Query {
                        getPublicProjects {
                            isPublic
                            id
                            gallery
                            projectName
                            categories
                        }
                }
            ",
            OperationName = "Query"
        };
        var queryResult = await graphQLClient.SendQueryAsync<JObject>(publicProjectsQuery);
        data = queryResult.Data;
        if (data != null)
        {
        
            var projects = (JArray)data["getPublicProjects"];
            for (int i = 0; i < projects.Count; i++)
            {
                var project = GameObject.Instantiate(projectPrefab);
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
                    else if (child.name == "ProjectBtn")
                    {
                        int x = i;
                        child.GetComponent<Button>().onClick.AddListener(delegate { openProject((string)projects[x]["id"]); });
                    }

                }

                project.transform.SetParent(parentContent.transform);
            }
            int numRows = (projects.Count / 4) + 1;
            parentContent.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1001, (numRows > 2) ? 168 * numRows : 168 * 2);

        }
        if (queryResult.Errors != null)
        {
            Debug.Log(queryResult.Errors[0].Message);
        }

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
        getRoomInstances(id);
        refreshRoomsBtn.GetComponent<Button>().onClick.AddListener(delegate { getRoomInstances(id); });
        createRoomBtn.GetComponent<Button>().onClick.AddListener(delegate { createRoomInstance(id); });
    }

    public async void createRoomInstance(string projectId)
    {
        Debug.Log("Creating room with id: " + userDecoded["username"] + '@' + projectId);
            
        Debug.Log(userDecoded["username"] + '@' + projectId);
        mainMenu.roomClient.Join(
            name: userDecoded["username"] + '@' + projectId,
            publish: true) ;
    // Create a mutation that adds a room using C#
        var addRoom = new GraphQLRequest
        {
            Query = @"
            mutation($input: AddRoomInput!) {
                addRoom(input: $input) {
                    id
                }
            }
        ",
            OperationName = "addRoom",
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
        Debug.Log(graphQL);
        if (graphQL.Data != null)
        {
            Debug.Log(graphQL.Data);
        } 
        if(graphQL.Errors != null) {
            Debug.Log(graphQL.Errors);
        }
    }

    public async void getRoomInstances(string id)
    {
        var getProjectData = new GraphQLRequest
        {
            Query = @"
                query GetProjectById($getProjectByIdId: String) {
                  getProjectById(id: $getProjectByIdId) {
                    rooms {
                      joinCode
                      roomId
                    }
                }
                ",
            OperationName = "GetProjectById",
            Variables = new { getProjectByIdId = id }
        };

        var graphQL = await graphQLClient.SendQueryAsync<JObject>(getProjectData);
        if (graphQL.Data != null)
        {
            foreach (var room in graphQL.Data["getProjectById"])
            {
                Debug.Log(room["roomId"]);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
