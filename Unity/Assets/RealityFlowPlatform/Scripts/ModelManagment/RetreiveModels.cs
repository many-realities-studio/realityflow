using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
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

    private async Task GetUserName(string userId)
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
            var graphQLResponse = await rfClient.SendQueryAsync(getUserRequest);
            var data = graphQLResponse["data"];

            if (data != null && data["getUserById"] != null)
            {
                string username = data["getUserById"]["username"].ToString();
                Debug.Log("Username for user ID " + userId + ": " + username);
            }
            else
            {
                Debug.LogError("Failed to fetch username for user ID: " + userId);
                var errors = graphQLResponse["errors"];
                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        Debug.LogError("GraphQL Error: " + error["message"]);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception in GetUserName: " + ex.Message);
        }
    }

    private async Task GetModelsData(string userId)
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
            var graphQLResponse = await rfClient.SendQueryAsync(getModelsRequest);
            var data = graphQLResponse["data"];
            if (data != null && data["getUserModelsForUnity"] != null)
            {
                JArray models = (JArray)data["getUserModelsForUnity"];
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
                Debug.Log(graphQLResponse);
                var errors = graphQLResponse["errors"];
                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        Debug.LogError("GraphQL Error: " + error["message"]);
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