using UnityEngine;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;
using Ubiq.Spawning;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class RfObjectManager : MonoBehaviour
{
    private GraphQLHttpClient graphQLClient;
    private NetworkSpawnManager networkSpawnManager;
    public PrefabCatalogue catalogue;
    private RealityFlowClient realityFlowClient;
    [SerializeField]
    public string projectId;
    public GameObject objectPrefab;
    public Transform contentContainer;

    void Start()
    {
        // Initialize NetworkSpawnManager (UBIQ)
        networkSpawnManager = FindObjectOfType<NetworkSpawnManager>();
        if (networkSpawnManager == null)
        {
            Debug.LogError("NetworkSpawnManager is not found in the scene.");
            return;
        }

        // Initialize RealityFlowClient (RealityFlow)
        realityFlowClient = FindObjectOfType<RealityFlowClient>();
        if (realityFlowClient == null)
        {
            Debug.LogError("RealityFlowClient is not found in the scene.");
            return;
        }

        InitializeGraphQLClient();  // Initialize GraphQL client with the access token
        FetchAndPopulateObjects();  // Fetch objects from the database and populate the room
    }

    public void InitializeGraphQLClient()
    {
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(realityFlowClient.server + "/graphql") // Server URL found in RealityFlowClient
        };

        graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {realityFlowClient.accessToken}");
    }

    private async void FetchAndPopulateObjects()
    {
        var objectsInDatabase = await FetchObjectsByProjectId(projectId);
        if (objectsInDatabase != null)
        {
            ObjectUI.PopulateUI(contentContainer, objectPrefab, objectsInDatabase);
            PopulateRoom(objectsInDatabase);
        }
    }

    public async Task SaveObjectTransformToDatabase(string objectId, TransformData transformData)
    {
        var rfObject = new RfObject
        {
            id = objectId,
            transformJson = JsonUtility.ToJson(transformData)
        };

        var saveObject = new GraphQLRequest
        {
            Query = @"
            mutation UpdateObjectTransform($input: UpdateObjectTransformInput!) {
                updateObjectTransform(input: $input) {
                    id
                }
            }",
            Variables = new
            {
                input = new
                {
                    id = rfObject.id,
                    transformJson = rfObject.transformJson
                }
            }
        };

        try
        {
            var graphQLResponse = await graphQLClient.SendMutationAsync<JObject>(saveObject);
            if (graphQLResponse.Data != null)
            {
                Debug.Log("Object transform updated in the database successfully.");
            }
            else
            {
                Debug.LogError("Failed to update object transform in the database.");
                foreach (var error in graphQLResponse.Errors)
                {
                    Debug.LogError($"GraphQL Error: {error.Message}");
                    if (error.Extensions != null)
                    {
                        Debug.LogError($"Error Extensions: {error.Extensions}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception: " + ex.Message);
        }
    }

    private async Task<List<RfObject>> FetchObjectsByProjectId(string projectId)
    {
        Debug.Log("Fetching objects by project ID: " + projectId);

        // GraphQL query to get objects by project ID
        var getObjectsQuery = new GraphQLRequest
        {
            Query = @"
            query GetObjectsByProjectId($projectId: String!) {
                getObjectsByProjectId(projectId: $projectId) {
                    id
                    name
                    type
                    meshJson
                    transformJson
                }
            }",
            Variables = new { projectId = projectId }
        };

        try
        {
            var graphQLResponse = await graphQLClient.SendQueryAsync<JObject>(getObjectsQuery);
            if (graphQLResponse.Data != null)
            {
                //Debug.Log("GraphQL Response Data: " + graphQLResponse.Data.ToString());

                var data = graphQLResponse.Data["getObjectsByProjectId"];
                if (data == null)
                {
                    Debug.LogWarning("No objects found for the given project ID.");
                    return null;
                }

                if (data is JArray objectsArray)
                {
                    //Debug.Log("Raw Objects JSON: " + objectsArray.ToString());

                    // Deserialize JSON data into a list of RfObject
                    var objectsInDatabase = objectsArray.ToObject<List<RfObject>>();
                    if (objectsInDatabase == null)
                    {
                        Debug.LogWarning("Deserialized objects are null.");
                        return null;
                    }

                    // Log each object in the list
                    foreach (var obj in objectsInDatabase)
                    {
                        if (obj == null)
                        {
                            Debug.LogWarning("An object in the list is null.");
                            continue;
                        }

                        Debug.Log("Object: " + JsonUtility.ToJson(obj)); 
                    }

                    return objectsInDatabase;
                }
                else
                {
                    Debug.LogWarning("Data is not a JArray.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve objects. Response data is null.");
                if (graphQLResponse.Errors != null)
                {
                    foreach (var error in graphQLResponse.Errors)  // Log any errors in the response
                    {
                        Debug.LogError($"GraphQL Error: {error.Message}");
                        if (error.Extensions != null)
                        {
                            Debug.LogError($"Error Extensions: {error.Extensions}");
                        }
                    }
                }
            }
        }

        // These debug logs should identify exceptions
        catch (HttpRequestException httpRequestException)
        {
            Debug.LogError("HttpRequestException: " + httpRequestException.Message);
        }
        catch (IOException ioException)
        {
            Debug.LogError("IOException: " + ioException.Message);
        }
        catch (SocketException socketException)
        {
            Debug.LogError("SocketException: " + socketException.Message);
        }
        catch (Exception ex)
        {
            Debug.LogError("General Exception: " + ex.Message);
            Debug.LogError("Exception stack trace: " + ex.StackTrace);
        }

        return null;
    }

    private void PopulateRoom(List<RfObject> objectsInDatabase)
    {
        Debug.Log("Populating room with objects from database.");

        if (catalogue == null || networkSpawnManager == null)
        {
            Debug.LogError("PrefabCatalogue or NetworkSpawnManager is not assigned.");
            return;
        }

        foreach (var obj in objectsInDatabase)
        {
            if (obj == null)
            {
                Debug.LogError("RfObject in the list is null.");
                continue;
            }

            // Remove "(Clone)" suffix from the object name if it exists
            string objectName = obj.name.Replace("(Clone)", "");

            // Find the prefab in the catalogue
            GameObject prefab = catalogue.prefabs.Find(p => p.name == objectName);
            if (prefab == null)
            {
                Debug.LogError($"Prefab for object {objectName} not found in catalogue.");
                continue;
            }

            Debug.Log($"Spawning object: {objectName}");

            try
            {
                // Spawn the object using NetworkSpawnManager to ensure it's synchronized across all users
                networkSpawnManager.SpawnWithRoomScope(prefab);

                // Find the spawned object in the scene (assuming it's named the same as the prefab)
                GameObject spawnedObject = GameObject.Find(objectName);
                if (spawnedObject == null)
                {
                    Debug.LogError("Spawned object is null.");
                    continue;
                }

                // Set the name of the spawned object to its ID for unique identification
                spawnedObject.name = obj.id;

                // Apply the transform properties
                TransformData transformData = JsonUtility.FromJson<TransformData>(obj.transformJson);
                if (transformData != null)
                {
                    spawnedObject.transform.position = transformData.position;
                    spawnedObject.transform.rotation = transformData.rotation;
                    spawnedObject.transform.localScale = transformData.scale;
                }

                Debug.Log($"Spawned object with ID: {obj.id}, Name: {obj.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error spawning object with NetworkSpawnManager: " + ex.Message);
                Debug.LogError("Exception stack trace: " + ex.StackTrace);
            }
        }

        Debug.Log("Room population complete.");
    }
}