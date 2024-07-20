using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using Ubiq.Spawning;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using GLTFast;
using UnityEngine.XR.Interaction.Toolkit;

public class ModelData
{
    public string id;
    public string name;
    public int triangles;
    public string downloadURL;
    public string thumbnailURL;
}

// Serializable class for Mesh data
[Serializable]
public class SerializableMeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Vector2[] uv;
}

public class PopulateObjectLibrary : MonoBehaviour
{
    public GameObject buttonPrefab;
    public RaycastLogger spawnScript;
    [SerializeField] private NetworkSpawnManager networkSpawnManager;
    public List<GameObject> objectPrefabs = new List<GameObject>();
    public List<GameObject> iconPrefabs = new List<GameObject>();
    public List<ModelData> modelCatalogue = new List<ModelData>();
    public GameObject modelsButton;
    public GameObject objectsButton;
    public Transform contentGrid;
    private RealityFlowClient rfClient;

    void Awake()
    {
        rfClient = RealityFlowClient.Find(this);
        if (rfClient != null && rfClient.userDecoded != null && rfClient.userDecoded.ContainsKey("id"))
        {
            string userId = rfClient.userDecoded["id"];
            GetUserName(userId);
            GetUserOwnedModels(userId);
        }
        else
        {
            Debug.LogError("User ID not found in RealityFlowClient.");
        }
        for (int i = 0; i < objectPrefabs.Count; i++)
            InstantiateButton(buttonPrefab, objectPrefabs[i], iconPrefabs[i], this.gameObject.transform);

        modelsButton.GetComponent<PressableButton>().OnClicked.AddListener(OnModelsButtonClicked);
        objectsButton.GetComponent<PressableButton>().OnClicked.AddListener(OnObjectsButtonClicked);
    }

    private void ClearObjectGrid()
    {
        foreach (Transform child in contentGrid)
        {
            Destroy(child.gameObject);
        }
    }

    private void GetUserName(string userId)
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
            var graphQLResponse = rfClient.SendQueryBlocking(getUserRequest);
            var data = graphQLResponse["data"];

            if (data != null && data["getUserById"] != null)
            {
                string username = data["getUserById"]["username"].ToString();
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

    #region Populate with Objects
    void OnObjectsButtonClicked()
    {   
        Debug.Log("Populating Buttons for Objects...");
        ClearObjectGrid();
        for (int i = 0; i < objectPrefabs.Count; i++)
        {
            InstantiateButton(buttonPrefab, objectPrefabs[i], iconPrefabs[i], this.gameObject.transform);
        }
    }

    private void InstantiateButton(GameObject buttonPrefab, GameObject objectPrefab, GameObject iconPrefab, Transform parent)
    {
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(objectPrefab.name);
        newButton.GetComponentInChildren<SetPrefabIcon>().prefab = iconPrefab;
        UnityAction<GameObject> action = new UnityAction<GameObject>(TriggerObjectSpawn);
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(objectPrefab));
    }

    void TriggerObjectSpawn(GameObject objectPrefab)
    {
        Debug.Log("TriggerObjectSpawn");
        Debug.Log(spawnScript.GetVisualIndicatorPosition());

        // Default rotation
        Quaternion defaultRotation = objectPrefab.transform.rotation;

        Debug.Log("[POL]Spawning Prefab from Catalog: " + objectPrefab.name);

        // Spawn the object
        GameObject spawnedObject = RealityFlowAPI.Instance.SpawnObject(objectPrefab.name, spawnScript.GetVisualIndicatorPosition() + new Vector3(0, 0.25f, 0), objectPrefab.transform.localScale, defaultRotation, RealityFlowAPI.SpawnScope.Room);

        Debug.Log("[POL]Object Spawned: " + spawnedObject.name);

        // After Spawn Object, log the action to the server
        RealityFlowAPI.Instance.LogActionToServer("Add Prefab" + spawnedObject.name.ToString(), new { prefabTransformPosition = spawnedObject.transform.localPosition, prefabTransformRotation = spawnedObject.transform.localRotation, prefabTransformScale = spawnedObject.transform.localEulerAngles });


        // Add Rigidbody and MeshCollider
        if (spawnedObject.GetComponent<Rigidbody>() != null)
        {
            spawnedObject.GetComponent<Rigidbody>().useGravity = true;
            StartCoroutine("setObjectToBeStill", spawnedObject);
        }
    }

    private IEnumerator setObjectToBeStill(GameObject spawnedObject)
    {
        yield return new WaitForSeconds(2);
        spawnedObject.GetComponent<Rigidbody>().useGravity = false;
        spawnedObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    #endregion

    #region Populate with Models
    public void GetUserOwnedModels(string userId)
    {
        //Debug.Log("Starting GetUserOwnedModels for user ID: " + userId);

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
            var graphQLResponse = rfClient.SendQueryBlocking(getModelsRequest);
            Debug.Log("GraphQL response received: " + graphQLResponse);

            if (graphQLResponse == null)
            {
                Debug.LogError("GraphQL response is null.");
                return;
            }

            var data = graphQLResponse["data"];
            if (data != null && data["getUserModelsForUnity"] != null)
            {
                JArray models = (JArray)data["getUserModelsForUnity"];
                Debug.Log("Number of models retrieved: " + models.Count);

                if (models.Count > 0)
                {
                    // Debug.Log("Models found for user ID: " + userId);

                    modelCatalogue.Clear();
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
                        modelCatalogue.Add(modelData);
                    }
                }
                else
                {
                    Debug.LogError("No models found for user ID: " + userId);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch models for user ID: " + userId);
                Debug.Log("GraphQL Response: " + graphQLResponse);

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
            Debug.LogError("Exception in GetUserOwnedModels: " + ex.Message);
        }
    }

    void OnModelsButtonClicked()
    {   
        Debug.Log("Populating Buttons for Models...");
        ClearObjectGrid();
        foreach (var modelData in modelCatalogue)
        {
            InstantiateButton(buttonPrefab, modelData, iconPrefabs[0], this.gameObject.transform);
        }
    }

    private void InstantiateButton(GameObject buttonPrefab, ModelData modelData, GameObject iconPrefab, Transform parent)
    {
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(modelData.name);
        newButton.GetComponentInChildren<SetPrefabIcon>().prefab = iconPrefab;
        UnityAction<string> action = new UnityAction<string>(TriggerModelSpawn);
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(modelData.downloadURL));
    }

    private void TriggerModelSpawn(string downloadUrl)
    {
        Debug.Log("TriggerModelSpawn with Download URL: " + downloadUrl);
        StartCoroutine(DownloadAndSpawnModel(downloadUrl));
    }

    private IEnumerator DownloadAndSpawnModel(string url)
    {
        // Download the model from the URL
        Debug.Log("Downloading model from URL: " + url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download model. Error: " + www.error);
            yield break;
        }

        byte[] data = www.downloadHandler.data;
        Debug.Log("Successfully downloaded model. Size: " + data.Length + " bytes.");

        // Load the model
        var gltf = new GltfImport();
        var successTask = gltf.LoadGltfBinary(data, null);
        yield return new WaitUntil(() => successTask.IsCompleted);

        if (!successTask.Result)
        {
            Debug.LogError("Failed to parse the downloaded model.");
            yield break;
        }

        Debug.Log("Successfully parsed the model.");

        // Instantiate the model
        var sceneTask = gltf.InstantiateMainSceneAsync(transform);
        yield return sceneTask;

        if (sceneTask.Result)
        {
            Debug.Log("Model instantiated successfully.");
        
            var instantiatedModel = transform.GetChild(transform.childCount - 1).gameObject;

            // Set the model's position and scale
            instantiatedModel.transform.SetParent(null);
            instantiatedModel.transform.position = spawnScript.GetVisualIndicatorPosition() + new Vector3(0, 0.25f, 0);
            instantiatedModel.transform.localScale = Vector3.one;

            // Add Rigidbody and MeshCollider
            Rigidbody rb = instantiatedModel.AddComponent<Rigidbody>();
            MeshCollider meshCollider = instantiatedModel.AddComponent<MeshCollider>();
            meshCollider.convex = true;

            // Set Rigidbody properties
            rb.mass = 1.0f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // Extract mesh data and serialize it
            Mesh mesh = meshCollider.sharedMesh;
            SerializableMeshData serializableMeshData = new SerializableMeshData
            {
                vertices = mesh.vertices,
                triangles = mesh.triangles,
                normals = mesh.normals,
                uv = mesh.uv
            };
            string meshJson = JsonUtility.ToJson(serializableMeshData);

            // Use the RealityFlowAPI to save the model to the server
            ModelData modelData = new ModelData
            {
                id = Guid.NewGuid().ToString(),  // Generate a unique ID for the model
                name = instantiatedModel.name,
                triangles = mesh.triangles.Length / 3,
                downloadURL = url,  // Assuming the URL can be used for future downloads
                thumbnailURL = ""   // Add logic to generate or fetch a thumbnail URL if needed
            };

            string projectId = "your_project_id";  // Replace with actual project ID

            RealityFlowAPI.Instance.SaveModelToDatabase(instantiatedModel, modelData, projectId, meshJson);

            Debug.Log("Model saved to the database successfully.");
        }
        else
        {
            Debug.LogError("Failed to instantiate the model.");
        }
    }
    #endregion
}