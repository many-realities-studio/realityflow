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

public class PopulateObjectLibrary : MonoBehaviour
{
    // This should be set to the Object button prefab
    public GameObject buttonPrefab;

    // This should be set to the SpawnObjectAtRay component attached to one of the hands
    public RaycastLogger spawnScript;

    // Spawn the object as networked
    [SerializeField] private NetworkSpawnManager networkSpawnManager;

    // These lists should be populated with all of the objects that are expected to appear
    // in the toolbox along with their icon prefabs
    public List<GameObject> objectPrefabs = new List<GameObject>();
    public List<GameObject> iconPrefabs = new List<GameObject>();
    public List<ModelData> modelCatalogue = new List<ModelData>();

    // Reference to the Models button
    public GameObject modelsButton;
    public GameObject objectsButton;
    public Transform contentGrid;

    // Reference to the RealityFlowClient
    private RealityFlowClient rfClient;

    // Start is called before the first frame update
    void Awake()
    {
        rfClient = RealityFlowClient.Find(this); // Find the RealityFlow client
        if (rfClient != null && rfClient.userDecoded != null && rfClient.userDecoded.ContainsKey("id"))
        {
            string userId = rfClient.userDecoded["id"];
            Debug.Log("User ID found: " + userId);
            GetUserName(userId);

            // Get models data with actual user ID
            GetModelsData(userId);
        }
        else
        {
            Debug.LogError("User ID not found in RealityFlowClient.");
        }
        for (int i = 0; i < objectPrefabs.Count; i++)
            InstantiateButton(buttonPrefab, objectPrefabs[i], iconPrefabs[i], this.gameObject.transform);

        // Set up listener for the Models button
        modelsButton.GetComponent<PressableButton>().OnClicked.AddListener(OnModelsButtonClicked);
        objectsButton.GetComponent<PressableButton>().OnClicked.AddListener(OnObjectsButtonClicked);
    }

    // Instantiate a button and set its prefab
    private void InstantiateButton(GameObject buttonPrefab, GameObject objectPrefab,
        GameObject iconPrefab, Transform parent)
    {
        // Instantiate the new button, set the text, and set the icon prefab
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(objectPrefab.name);
        newButton.GetComponentInChildren<SetPrefabIcon>().prefab = iconPrefab;

        // Create a new Unity action and add it as a listener to the button's OnClicked event
        UnityAction<GameObject> action = new UnityAction<GameObject>(TriggerObjectSpawn);
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(objectPrefab));
    }

    // Overloaded InstantiateButton method for ModelData
    private void InstantiateButton(GameObject buttonPrefab, ModelData modelData,
        GameObject iconPrefab, Transform parent)
    {
        // Instantiate the new button, set the text, and set the icon prefab
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(modelData.name);
        newButton.GetComponentInChildren<SetPrefabIcon>().prefab = iconPrefab;

        // Create a new Unity action and add it as a listener to the button's OnClicked event
        UnityAction<string> action = new UnityAction<string>(TriggerModelSpawn);
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(modelData.downloadURL));
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

    void TriggerObjectSpawn(GameObject objectPrefab)
    {
        Debug.Log("TriggerObjectSpawn");
        Debug.Log(spawnScript.GetVisualIndicatorPosition());

        // Use the prefab's default rotation
        Quaternion defaultRotation = objectPrefab.transform.rotation;
        // Spawn the object with the default rotation
        GameObject spawnedObject = RealityFlowAPI.Instance.SpawnObject(objectPrefab.name, spawnScript.GetVisualIndicatorPosition() + new Vector3(0, 0.25f, 0), objectPrefab.transform.localScale, defaultRotation, RealityFlowAPI.SpawnScope.Room);
        RealityFlowAPI.Instance.LogActionToServer("Add Prefab" + spawnedObject.name.ToString(), new { prefabTransformPosition = spawnedObject.transform.localPosition, prefabTransformRotation = spawnedObject.transform.localRotation, prefabTransformScale = spawnedObject.transform.localEulerAngles });

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

    // Method called when the Models button is clicked
    void OnModelsButtonClicked()
    {
        Debug.Log("Populating Buttons for Models...");

        ClearObjectGrid();

        foreach (var modelData in modelCatalogue)
        {
            InstantiateButton(buttonPrefab, modelData, iconPrefabs[0], this.gameObject.transform);
        }
    }

    public void GetModelsData(string userId)
    {
        Debug.Log("Starting GetModelsData for user ID: " + userId);

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
                    Debug.Log("Models found for user ID: " + userId);

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
            Debug.LogError("Exception in GetModelsData: " + ex.Message);
        }
    }

    void TriggerModelSpawn(string modelUrl)
    {
        Debug.Log("TriggerModelSpawn with URL: " + modelUrl);
        StartCoroutine(DownloadAndSpawnModel(modelUrl));
    }

    private IEnumerator DownloadAndSpawnModel(string url)
    {
        Debug.Log("Downloading model from URL: " + url);
        UnityWebRequest webRequest = UnityWebRequest.Get(url);

        // Wait for the response
        yield return webRequest.SendWebRequest();

        // Check for errors
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download model. Error: " + webRequest.error);
            yield break;
        }

        // Get the data
        byte[] data = webRequest.downloadHandler.data;
        Debug.Log("Successfully downloaded model. Size: " + data.Length + " bytes.");

        // Parse and instantiate the model
        var gltf = new GltfImport();
        var successTask = gltf.LoadGltfBinary(data, null);
        yield return new WaitUntil(() => successTask.IsCompleted);

        if (!successTask.Result)
        {
            Debug.LogError("Failed to parse the downloaded model.");
            yield break;
        }

        Debug.Log("Successfully parsed the model.");

        // Instantiate the model in the scene
        var sceneTask = gltf.InstantiateMainSceneAsync(transform);
        yield return sceneTask;

        if (sceneTask.Result)
        {
            Debug.Log("Model instantiated successfully.");
            var instantiatedModel = transform.GetChild(transform.childCount - 1);

            // Detach the model from its parent to ensure it does not inherit the button's scale
            instantiatedModel.SetParent(null);

            // Set the position to a specific location (e.g., the center of the level)
            Vector3 spawnPosition = new Vector3(0, 0.25f, 0); // Example position
            instantiatedModel.position = spawnPosition;

            // Adjust the scale to ensure the model is reasonably sized
            instantiatedModel.localScale = Vector3.one; // Reset scale to original size

            // Add Rigidbody component for physics
            Rigidbody rb = instantiatedModel.gameObject.AddComponent<Rigidbody>();

            // Add appropriate collider
            Collider collider = instantiatedModel.gameObject.AddComponent<MeshCollider>();
            ((MeshCollider)collider).convex = true;

            // Optional: Configure Rigidbody properties
            rb.mass = 1.0f; // Example: Set mass to 1
            rb.useGravity = false; // Enable gravity
            rb.isKinematic = false; // Allow physics interactions

            // Add XRGrabInteractable component for XR interaction
            // XRGrabInteractable grabInteractable = instantiatedModel.gameObject.AddComponent<XRGrabInteractable>();

            // Optional: Configure XRGrabInteractable properties
            // grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic; // Example: Set movement type to kinematic
            // grabInteractable.throwOnDetach = true; // Allow throwing the object when released

            Debug.Log("Rigidbody, Collider, and XRGrabInteractable added to the model.");
        }
        else
        {
            Debug.LogError("Failed to instantiate the model.");
        }
    }

    #endregion
}