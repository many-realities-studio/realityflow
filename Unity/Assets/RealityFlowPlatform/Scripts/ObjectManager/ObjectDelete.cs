using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;

public class ObjectDelete : MonoBehaviour
{
    private GraphQLHttpClient graphQLClient;
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

        InitializeGraphQLClient();
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

    public async void DeleteObject(string objectId)
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
            var graphQLResponse = await graphQLClient.SendMutationAsync<JObject>(deleteObject);
            if (graphQLResponse.Data != null)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}