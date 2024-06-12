using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json.Linq;

public class RetrieveModel : MonoBehaviour
{
    public ModelUI modelUI; // Reference to the ModelUI script

    private RealityFlowClient rfClient;

    void Start()
    {
        rfClient = RealityFlowClient.Find(this); // Find the RealityFlow client
        if (rfClient != null && rfClient.userDecoded != null && rfClient.userDecoded.ContainsKey("id"))
        {
            string userId = rfClient.userDecoded["id"];
            Debug.Log("User ID found: " + userId);
            GetUserName(userId);
            GetModelsData(userId); // Fetch models data with user ID
        }
        else
        {
            Debug.LogError("User ID not found in RealityFlowClient.");
        }
    }

    private async void GetUserName(string userId)
    {
        var getUserRequest = new GraphQLRequest
        {
            Query = @"
            query GetUserName($id: String!) {
                getUserById(id: $id) {
                    username
                }
            }",
            Variables = new { id = userId }
        };

        try
        {
            var graphQLResponse = await rfClient.graphQLClient.SendQueryAsync<JObject>(getUserRequest);

            if (graphQLResponse.Data != null && graphQLResponse.Data["getUserById"] != null)
            {
                string username = graphQLResponse.Data["getUserById"]["username"].ToString();
                Debug.Log("Username for user ID " + userId + ": " + username);
            }
            else
            {
                Debug.LogError("Failed to fetch username for user ID: " + userId);
                if (graphQLResponse.Errors != null)
                {
                    foreach (var error in graphQLResponse.Errors)
                    {
                        Debug.LogError("GraphQL Error: " + error.Message);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception in GetUserName: " + ex.Message);
        }
    }

    private async void GetModelsData(string userId)
    {
        var getModelsRequest = new GraphQLRequest
        {
            Query = @"
            query GetUserModelsForUnity($input: UserIdInput!) {
                getUserModelsForUnity(input: $input) {
                    id
                    name
                    triangles
                    downloadURL
                    thumbnailURL
                }
            }",
            Variables = new { input = new { userId = userId } }
        };

        try
        {
            var graphQLResponse = await rfClient.graphQLClient.SendQueryAsync<JObject>(getModelsRequest);

            if (graphQLResponse.Data != null && graphQLResponse.Data["getUserModelsForUnity"] != null)
            {
                JArray models = (JArray)graphQLResponse.Data["getUserModelsForUnity"];
                if (models.Count > 0)
                {
                    Debug.Log("Models found for user ID: " + userId);

                    List<ModelData> modelDataList = new List<ModelData>();
                    foreach (var model in models)
                    {
                        ModelData modelData = new ModelData
                        {
                            id = model["id"].ToString(),
                            name = model["name"].ToString(),
                            triangles = int.Parse(model["triangles"].ToString()),
                            downloadURL = model["downloadURL"].ToString(),
                            thumbnailURL = model["thumbnailURL"].ToString()
                        };
                        modelDataList.Add(modelData);
                    }

                    modelUI.PopulateModels(modelDataList);
                }
                else
                {
                    Debug.LogError("No models found for user ID: " + userId);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch models for user ID: " + userId);
                if (graphQLResponse.Errors != null)
                {
                    foreach (var error in graphQLResponse.Errors)
                    {
                        Debug.LogError("GraphQL Error: " + error.Message);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception in GetModelsData: " + ex.Message);
        }
    }
}