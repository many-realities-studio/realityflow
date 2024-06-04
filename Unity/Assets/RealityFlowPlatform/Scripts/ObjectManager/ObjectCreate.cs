using UnityEngine;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;
using Ubiq.Spawning;
using System;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;

public class ObjectSpawn : MonoBehaviour
{
    private GraphQLHttpClient graphQLClient;
    private NetworkSpawnManager networkSpawnManager;
    private RealityFlowClient realityFlowClient;
    private RfObjectManager rfObjectManager; // Add reference to RfObjectManager
    private string projectId;
    void Start()
    {
        // Initialize NetworkSpawnManager (UBIQ)
        networkSpawnManager = FindObjectOfType<NetworkSpawnManager>();
        if (networkSpawnManager == null)
        {
            Debug.LogError("NetworkSpawnManager is not found in the scene.");
            return;
        }

        // Initialize RealityFlowClient  (RealityFlow)
        realityFlowClient = FindObjectOfType<RealityFlowClient>();
        if (realityFlowClient == null)
        {
            Debug.LogError("RealityFlowClient is not found in the scene.");
            return;
        }

        // Initialize RfObjectManager
        rfObjectManager = FindObjectOfType<RfObjectManager>();
        if (rfObjectManager == null)
        {
            Debug.LogError("RfObjectManager is not found in the scene.");
            return;
        }

        // Get projectId from RfObjectManager
        projectId = rfObjectManager.projectId;

        // For Testing Purpses the project ID is a string field in Editor
        // NOTE: Ideally this function should get the current projectID from the RealityFlowClient
        // Obtain the Access Token from realityFlowClient
        InitializeGraphQLClient();
        // Debug.Log("ACESS TOKEN: " + realityFlowClient.accessToken);
        // Debug.Log("SERVER: " + realityFlowClient.server + "/graphql");
        // Debug.Log("PROJECT ID: " + projectId);
    }
    
    private void InitializeGraphQLClient()
    {
        if (realityFlowClient == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            return;
        }

        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(realityFlowClient.server + "/graphql")
        };

        graphQLClient = new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {realityFlowClient.accessToken}");
    }

    public void SpawnObjectWithRoomScope(string prefabName)
    {
        GameObject prefab = GetPrefabByName(prefabName);

        if (prefab != null && networkSpawnManager != null)
        {
            // Spawn the object with room scope
            networkSpawnManager.SpawnWithRoomScope(prefab);
            Debug.Log($"Spawned {prefab.name} with room scope.");

            // Find the spawned object (assuming it has the same name as the prefab)
            GameObject spawnedObject = GameObject.Find(prefab.name);
            if (spawnedObject != null)
            {
                // Create a TransformData object and populate it
                TransformData transformData = new TransformData
                {
                    position = spawnedObject.transform.position,
                    rotation = spawnedObject.transform.rotation,
                    scale = spawnedObject.transform.localScale
                };

                // Create a RfObject and populate it
                RfObject rfObject = new RfObject
                {
                    projectId = projectId,
                    name = spawnedObject.name,
                    type = "Prefab",
                    transformJson = JsonUtility.ToJson(transformData),
                    meshJson = GetMeshJson(spawnedObject)
                };

                // Log the JSON to verify it
                Debug.Log("transformJson: " + rfObject.transformJson);
                Debug.Log("meshJson: " + rfObject.meshJson);
                SaveObjectToDatabase(rfObject);
            }
            else
            {
                Debug.LogError("Could not find the spawned object in the scene.");
            }
        }
        else
        {
            Debug.LogError("Prefab not found or NetworkSpawnManager is not initialized.");
        }
    }

    private GameObject GetPrefabByName(string prefabName)
    {
        if (networkSpawnManager != null && networkSpawnManager.catalogue != null)
        {
            foreach (var prefab in networkSpawnManager.catalogue.prefabs)
            {
                if (prefab.name == prefabName)
                {
                    return prefab;
                }
            }
        }
        Debug.LogError($"No prefab found with the name: {prefabName}");
        return null;
    }

    private string GetMeshJson(GameObject spawnedObject)
    {
        MeshFilter meshFilter = spawnedObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.mesh;
            MeshData meshData = new MeshData
            {
                vertices = mesh.vertices,
                triangles = mesh.triangles,
                normals = mesh.normals,
                uv = mesh.uv
            };
            return JsonUtility.ToJson(meshData);
        }
        return "{}";
    }

    private async void SaveObjectToDatabase(RfObject rfObject)
    {
        if (realityFlowClient == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            return;
        }

        var saveObject = new GraphQLRequest
        {
            Query = @"
            mutation SaveObject($input: SaveObjectInput!) {
                saveObject(input: $input) {
                    id
                }
            }",
            OperationName = "SaveObject",
            Variables = new
            {
                input = new
                {
                    projectId = rfObject.projectId,
                    name = rfObject.name,
                    type = rfObject.type,
                    meshJson = rfObject.meshJson,
                    transformJson = rfObject.transformJson
                }
            }
        };

        try
        {
            Debug.Log("Sending GraphQL request to: " + realityFlowClient.server + "/graphql");
            Debug.Log("Request: " + JsonUtility.ToJson(saveObject));

            var graphQLResponse = await graphQLClient.SendMutationAsync<JObject>(saveObject);
            if (graphQLResponse.Data != null)
            {
                Debug.Log("Object saved to the database successfully.");
            }
            else
            {
                Debug.LogError("Failed to save object to the database.");
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
        }
    }
}
