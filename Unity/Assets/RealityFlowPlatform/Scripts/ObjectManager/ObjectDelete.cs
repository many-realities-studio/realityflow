using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;

public class ObjectDelete : MonoBehaviour
{
    private RealityFlowClient realityFlowClient;

    void Start()
    {
        // Initialize RealityFlowClient
        realityFlowClient = FindObjectOfType<RealityFlowClient>();
        if (realityFlowClient == null)
        {
            Debug.LogError("RealityFlowClient is not found in the scene.");
            return;
        }
    }

    public void DeleteObject(string objectId)
    {
        var deleteObject = new GraphQLRequest
        {
            Query = @"
            mutation DeleteObject($input: DeleteObjectInput!) {
                deleteObject(input: $input) {
                    id
                }
            }",
            Variables = new
            {
                input = new
                {
                    objectId = objectId
                }
            }
        };

        try
        {
            var graphQLResponse = realityFlowClient.SendQueryAsync(deleteObject);
            var data = graphQLResponse["data"];
            var errors = graphQLResponse["errors"];
            if (data != null)
            {
                Debug.Log("Object deleted from the database successfully.");

                // Find and delete the object in the scene
                GameObject objectToDelete = GameObject.Find(objectId);
                if (objectToDelete != null)
                {
                    Destroy(objectToDelete);
                    Debug.Log($"Deleted object with ID: {objectId} from the scene.");
                }
                else
                {
                    Debug.LogError("Could not find the object to delete in the scene.");
                }
            }
            else
            {
                Debug.LogError("Failed to delete object from the database.");
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

    // Update is called once per frame
    void Update()
    {
        
    }
}