using UnityEngine;
using Newtonsoft.Json.Linq;
using Ubiq.Spawning;
using System;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;

public class ObjectSpawn : MonoBehaviour
{
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
        // Debug.Log("ACESS TOKEN: " + realityFlowClient.accessToken);
        // Debug.Log("SERVER: " + realityFlowClient.server + "/graphql");
        // Debug.Log("PROJECT ID: " + projectId);
    }

    public void SpawnObjectWithRoomScope(string prefabName)
    {
        GameObject prefab = GetPrefabByName(prefabName);

        if (prefab != null && networkSpawnManager != null)
        {
            networkSpawnManager.SpawnWithRoomScope(prefab);
            Debug.Log($"Spawned {prefab.name} with room scope.");
            
            GameObject spawnedObject = GameObject.Find(prefab.name);
            if (spawnedObject != null)
            {
                TransformData transformData = new TransformData
                {
                    position = spawnedObject.transform.position,
                    rotation = spawnedObject.transform.rotation,
                    scale = spawnedObject.transform.localScale
                };

                RfObject rfObject = new RfObject
                {
                    projectId = projectId,
                    name = spawnedObject.name,
                    type = "Prefab",
                    graphId = null,
                    transformJson = JsonUtility.ToJson(transformData),
                    meshJson = GetMeshJson(spawnedObject)
                };

                CreateObjectInDatabase(rfObject);

                // Log the JSON to verify it
                Debug.Log("transformJson: " + rfObject.transformJson);
                Debug.Log("meshJson: " + rfObject.meshJson);
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

    private async void CreateObjectInDatabase(RfObject rfObject)
    {
        if (realityFlowClient == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            return;
        }

        var createObject = new GraphQLRequest
        {
            Query = @"
            mutation CreateObject($input: SaveObjectInput!) {
                createObject(input: $input) {
                    id
                }
            }",
            OperationName = "CreateObject",
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
            Debug.Log("Request: " + JsonUtility.ToJson(createObject));

            var graphQLResponse = await realityFlowClient.SendQueryAsync(createObject);
            var data = graphQLResponse["data"];
            var errors = graphQLResponse["errors"];
            if (data != null)
            {
                Debug.Log("Object saved to the database successfully.");
                
                // Extract the ID from the response and assign it to the rfObject
                var returnedId = data["saveObject"]["id"].ToString();
                rfObject.id = returnedId;
                Debug.Log($"Assigned ID from database: {rfObject.id}");
                
                // Update the name of the spawned object in the scene
                GameObject spawnedObject = GameObject.Find(rfObject.name);
                if (spawnedObject != null)
                {
                    spawnedObject.name = rfObject.id;
                    Debug.Log($"Updated spawned object name to: {spawnedObject.name}");
                }
                else
                {
                    Debug.LogError("Could not find the spawned object to update its name.");
                }
            }
            else
            {
                Debug.LogError("Failed to save object to the database.");
                foreach (var error in errors)
                {
                    Debug.LogError($"GraphQL Error: {error["message"]}");
                    if (error["Extensions"] != null)
                    {
                        Debug.LogError($"Error Extensions: {error["Extensions"]}");
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
