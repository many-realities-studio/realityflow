//using GraphProcessor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine;
using Ubiq.Rooms;
using UnityEngine.Events;
// using static UnityEditor.Progress;

//using Ubiq.Samples;
//using UnityEngine.XR.ARSubsystems;


// uuid generation -- create a file with that name (json format)
// create a project file with the array of objects (each corresponding to a uuid)
// Save and load all objects to the file and the project file

// 1. Push the entire project serialized as a JSON to the d[Serializable]

//atabase every change. Expensive and potentially laggy.

// 2. Push the changes incrementally, and expect the scene to be stored on server in such a way to be edited by object and not be scene.
// Incorporates Ubiq API

public class ProjectContentManager : MonoBehaviour
{
    //public UnityEvent projectLoad = new UnityEvent();
    public List<IRealityFlowObject> sceneObjects = new List<IRealityFlowObject>();
    public static ProjectContentManager instance;
    public RealityFlowClient rfClient;
    public const string objectPath = "/savefile";
    public const string objectPathCount = "/savefile.count";
    public string server = "http://localhost:4000/graphql";
    public string defaultProjectId;
    private bool _editMode = true;
    private RoomClient client;
    public NetworkContext context;
    public GameObject mainMenu;
    public string accessToken;

    // Each of these working with just JSON serialization...
    // Loads all of the objects from the JSON file.
    // Replace with getting the content of the project from the server (when creating a new room from a project)
    // Otherwise, joining a room may involve getting a synced version of this component.
    public bool editMode
    {
        get { return _editMode; }
        set
        {
            // Sync edit state with other clients, possibly by sending a message
            // Check if player is a co-owner of project before permitting changing edit mode
            _editMode = value;

        }
    }

    private void Awake()
    {
        client = GetComponentInParent<RoomClient>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //Try to acces the list of ID

        //IF list exist, populate the scene with saved objects

        //If list does not exist, create a new string list to save IDs to, and upload to the net

        if (!instance)
        {
            instance = this;
        }
        accessToken = PlayerPrefs.GetString("accessToken");

        rfClient = RealityFlowClient.Find(this);
    }

    public IRealityFlowObject GetObject(GameObject obj)
    {
        return sceneObjects.Find((IRealityFlowObject ro) => ro.transform == obj.transform);
    }

    public void LoadObject(IRealityFlowObject em)
    {
        // String token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY0MWM0ODAyNmU2ZjhjZjkzNWNkNTZlMCIsInVzZXJuYW1lIjoiSmFuZURvZSIsImVtYWlsIjoibmF0aGFuaWVsQHNoYXBlZGN2LmNvbSIsImZpcnN0TmFtZSI6IkphbmUiLCJsYXN0TmFtZSI6IkRvZSIsImlhdCI6MTY4MzI0NDkzOCwiZXhwIjoxNjgzMzMxMzM4fQ.4m-yLOAfXW6qzK9hZyTScT2BseJQOp6IragpvCdwoqY";
        // graphQLC.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        // graphQLClient = graphQLC;
        var getProjectObjects = new GraphQLRequest
        {
            Query = @"
                query Query($getProjectByIdId: String) {
                    getProjectById(id: $getProjectByIdId) {
                       objects {
                            id
                            objectJson
                       }
                     }
               }
            ",
            OperationName = "Query",
            Variables = new { getProjectByIdId = defaultProjectId }
        };
        var queryResult = rfClient.SendQueryBlocking(getProjectObjects);
        var data = queryResult["data"];
        var errors = queryResult["errors"];
        Debug.Log("helloyes");
        //var data;
        if (data != null)
        {
            Debug.Log("hello");
            //data = (JArray)queryResult.Data["getProjectById"];
            //var data2 = data["getProjectById"]["objects"];
            //Debug.Log("uuid1: "+data2[0]["id"]);
            //Debug.Log("object1: " + data2[0]["objectJson"]);
            //Debug.Log("uuid2: " + data2[1]["id"]);
            //Debug.Log("object2: " + data2[1]["objectJson"]);
            /* can retrieve list of objects in data["objects"] (this return an array) */

            //SerializableMeshInfo smi =  data2[0]["objectJson"];

            GameObject primitive = PrimitiveSpawner.instance.primitive;

            JsonSerializer serializer = new JsonSerializer();
            SerializableMeshInfo smi = new SerializableMeshInfo();
            foreach (JToken meshData in data)
            {
                smi = (SerializableMeshInfo)serializer.Deserialize(meshData["objectJson"].CreateReader(), typeof(SerializableMeshInfo));
                GameObject go = NetworkSpawnManager.Find(this).SpawnWithPeerScope(primitive);

                go.GetComponent<EditableMesh>().smi = smi;


            }

            //object obj = data2[0]["objectJson"];
            //JsonSerializer serializer = new JsonSerializer();
            //SerializableMeshInfo smi = (SerializableMeshInfo)serializer.Deserialize(data[0]["objectJson"].CreateReader(), typeof(SerializableMeshInfo));

            //em.smi = smi;
            ////SerializeMesh.instance.acceptSmi(smi);

            //IEnumerable<object> collection = (IEnumerable<object>)obj;
            //foreach(object item in collection)
            //{
            //    Debug.Log("uuid2: " + item.ToString());
            //}
        }
        else if (errors != null)
        {

            Debug.Log(errors[0]["message"]);
        }

        //projectLoad.Invoke();
    }

    public void AddObject(GameObject go)
    {
        if (editMode) // And user has permission to edit
        {
            // This is null potentially? It's saying it's null there. 


            //SerializableMeshInfo smi = new SerializableMeshInfo(go);

            //Add Id of the object being saved to the DataServer

            //Eliminate current List of object
            //Serialize and save newly made list
            EditableMesh em = go.GetComponent<EditableMesh>();
            SaveObject(em);

            //Vector3 prefabPos = go.transform.position;

            em.type = "mesh";
            sceneObjects.Add(em);
        }

        // Create a new RealityFlow object and add it to the list.
        // Calls an "AddObject route on the server"
    }

    public void SaveObject(IRealityFlowObject em)
    {
        //Send the Json file to the dataserver

        Debug.Log("Hi");

        // var graphQLC = new GraphQLHttpClient(server, new NewtonsoftJsonSerializer());
        // String token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY0MWM0ODAyNmU2ZjhjZjkzNWNkNTZlMCIsInVzZXJuYW1lIjoiSmFuZURvZSIsImVtYWlsIjoibmF0aGFuaWVsQHNoYXBlZGN2LmNvbSIsImZpcnN0TmFtZSI6IkphbmUiLCJsYXN0TmFtZSI6IkRvZSIsImlhdCI6MTY4MzI0NDkzOCwiZXhwIjoxNjgzMzMxMzM4fQ.4m-yLOAfXW6qzK9hZyTScT2BseJQOp6IragpvCdwoqY";
        // graphQLC.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        // this.graphQLClient = graphQLC;
        var getUserInfoRequest = new GraphQLRequest
        {
            Query = @"
               mutation SaveObject($input: SaveObjectInput) {
                  saveObject(input: $input) {
                    id
                  }
                }
            ",
            OperationName = "SaveObject",
            Variables = new { input = new { objectJson = em.smi, projectId = defaultProjectId } }
        };

        var queryResult = rfClient.SendQueryBlocking(getUserInfoRequest);
        var queryData = queryResult["data"];

        if (queryData != null)
        {
            Debug.Log("successfully call api");
        }
        else if (queryResult["errors"] != null)
        {

            Debug.Log(queryResult["errors"][0]["message"]);
        }

        Debug.Log(queryData["saveObject"]["id"]);


        em.uuid = (string)queryData["saveObject"]["id"];
    }

    public void UpdateObject(GameObject go)
    {
        if (editMode)
        {
            // And user has permission to edit
            // Update the object position in list
            IRealityFlowObject robj = sceneObjects.Find((robj) => robj.uuid == go.GetComponent<EditableMesh>().uuid);

            // var graphQLC = new GraphQLHttpClient(server, new NewtonsoftJsonSerializer());
            // String token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY0MWM0ODAyNmU2ZjhjZjkzNWNkNTZlMCIsInVzZXJuYW1lIjoiSmFuZURvZSIsImVtYWlsIjoibmF0aGFuaWVsQHNoYXBlZGN2LmNvbSIsImZpcnN0TmFtZSI6IkphbmUiLCJsYXN0TmFtZSI6IkRvZSIsImlhdCI6MTY4MzI0NDkzOCwiZXhwIjoxNjgzMzMxMzM4fQ.4m-yLOAfXW6qzK9hZyTScT2BseJQOp6IragpvCdwoqY";
            // graphQLC.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            // this.graphQLClient = graphQLC;
            var getUserInfoRequest = new GraphQLRequest
            {
                Query = @"
                   mutation EditObject($input: EditObjectInput) {
                      editObject(input: $input) {
                        id
                      }
                    }
                ",
                OperationName = "EditObject",
                Variables = new { input = new { objectId = go.GetComponent<EditableMesh>().uuid, objectJson = go } }
            };

            var queryResult = rfClient.SendQueryBlocking(getUserInfoRequest);
        }
    }

    public void RemoveObject(GameObject go)
    {
        if (editMode) // And user has permission to edit
        {
            IRealityFlowObject robj = sceneObjects.Find((robj) => robj.uuid == go.GetComponent<EditableMesh>().uuid);
            sceneObjects.Remove(robj);

            // var graphQLC = new GraphQLHttpClient(server, new NewtonsoftJsonSerializer());
            // String token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY0MWM0ODAyNmU2ZjhjZjkzNWNkNTZlMCIsInVzZXJuYW1lIjoiSmFuZURvZSIsImVtYWlsIjoibmF0aGFuaWVsQHNoYXBlZGN2LmNvbSIsImZpcnN0TmFtZSI6IkphbmUiLCJsYXN0TmFtZSI6IkRvZSIsImlhdCI6MTY4MzI0NDkzOCwiZXhwIjoxNjgzMzMxMzM4fQ.4m-yLOAfXW6qzK9hZyTScT2BseJQOp6IragpvCdwoqY";
            // graphQLC.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            // this.graphQLClient = graphQLC;
            var getUserInfoRequest = new GraphQLRequest
            {
                Query = @"
                   mutation DeleteObject($input: DeleteObjectInput) {
                      deleteObject(input: $input) {
                        id
                      }
                    }
                ",
                OperationName = "DeleteObject",
                Variables = new { input = new { objectId = go.GetComponent<EditableMesh>().uuid } }
            };

            var queryResult = rfClient.SendQueryBlocking(getUserInfoRequest);
        }
    }


    void LoadSceneContent(string content)
    {


        // Takes in data either from a file or from a string
        // Called once the scene is loaded (or the project is loaded).


        // Does load scene get called only when a room is created for the first time? Probably....


        // How do you handle joining a room after its created with this logic? Do you communicate this "ProjectContentManager" through Ubiq's serialization?

        // Creates the objects through instantiation and then populates the List with those object details (sceneObjects).


        ///LoadObject();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
