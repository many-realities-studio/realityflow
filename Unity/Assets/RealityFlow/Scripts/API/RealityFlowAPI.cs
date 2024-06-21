using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;
using Ubiq.Spawning;
using System;
using System.Linq;
using System.Threading.Tasks;
using RealityFlow.NodeGraph;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;
using Unity.VisualScripting;
using Graph = RealityFlow.NodeGraph.Graph;
using Ubiq.Rooms;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class RealityFlowAPI : MonoBehaviour, INetworkSpawnable
{
    private string objectId;
    private NetworkSpawnManager spawnManager;
    private GameObject selectedObject;
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 previousScale;
    public Material outlineMaterial;
    public ActionLogger actionLogger = new ActionLogger();
    private NetworkContext networkContext;
    public NetworkId NetworkId { get; set; }
    private static RealityFlowAPI _instance;                // SINGLE INSTANCE OF THE API
    private static readonly object _lock = new object();   // ENSURES THREAD SAFETY
    public PrefabCatalogue catalogue; // Prefab Catalog
    /// </summary>

    private RealityFlowClient client;
    public Dictionary<GameObject, RfObject> spawnedObjects = new Dictionary<GameObject, RfObject>();
    public enum SpawnScope
    {
        Room,
        Peer
    }

    public static RealityFlowAPI Instance
    {
        get
        {
            lock (_lock)   // ENSURE THREAD SAFETY 
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RealityFlowAPI>() ?? new GameObject("RealityFlowAPI").AddComponent<RealityFlowAPI>();
                }
                return _instance;
            }
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Makes the object persistent across scenes
            spawnManager = NetworkSpawnManager.Find(this);
            if (spawnManager == null)
            {
                Debug.LogError("NetworkSpawnManager not found on the network scene!");
            }
        }

        client = RealityFlowClient.Find(this);
    }

    // ===== SUPPORT FUNCTIONS =====
    public string ExportSpawnedObjectsData()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var kvp in spawnManager.GetSpawnedForRoom())
        {
            var obj = kvp.Value;
            if (obj != null)
            {
                sb.AppendLine("Object: " + obj.name);
                Component[] components = obj.GetComponents<Component>();
                foreach (Component component in components)
                {
                    sb.AppendLine("  Component: " + component.GetType().Name);
                    if (component is Transform transform)
                    {
                        sb.AppendLine("    Position: " + transform.position);
                        sb.AppendLine("    Rotation: " + transform.rotation);
                        sb.AppendLine("    Scale: " + transform.localScale);
                    }
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public GameObject GetPrefabByName(string name)
    {
        // This function searches the catalogue for a prefab with the given name
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            foreach (var prefab in spawnManager.catalogue.prefabs)
            {
                if (prefab.name == name)
                    return prefab;
            }
        }
        Debug.LogWarning($"Prefab named {name} not found in function GetPrefabByName.");
        return null;
    }

    private string GetMeshJson(GameObject spawnedObject)
    {
        // This function serializes the mesh data of the spawned object
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

    void OutlineEffect(GameObject obj)
    {
        // Apply the outline effect (using material or component)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Store the original material if not already stored
                if (!originalMaterials.ContainsKey(renderer.gameObject))
                {
                    originalMaterials[renderer.gameObject] = renderer.material;
                }
                // Apply the outline material
                renderer.material = outlineMaterial;
            }
        }
    }

    void RemoveOutlineEffect(GameObject obj)
    {
        // Remove the outline effect (restore original material)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer.gameObject))
            {
                renderer.material = originalMaterials[renderer.gameObject];
                originalMaterials.Remove(renderer.gameObject); // Optionally remove the entry if it's no longer needed
            }
        }
    }

    // ===== API FUNCTIONS =====

    // -- CREATE GRAPH FUNCTIONS --

    // Update the object with the new graph id
    public async Task AssignGraph(Graph newGraph, GameObject obj)
    {
        // Adds the graph ID to the object's graph property as an update.
        spawnedObjects[obj].graphId = newGraph.Id;
        await SaveObjectToDatabase(spawnedObjects[obj]);
    }
    public async Task<Graph> CreateNodeGraphAsync()
    {
        string name = "New Graph";

        // Ensure client is initialized
        if (client == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            throw new Exception("RealityFlowClient is not initialized.");
        }

        // Get the current project ID from the client
        string projectId = client.GetCurrentProjectId();
        if (string.IsNullOrEmpty(projectId))
        {
            Debug.LogError("Current project ID is required.");
            throw new ArgumentException("Current project ID is required.");
        }
        Graph newGraph = null;
        try
        {
            // Serialize the Graph object to JSON
            string graphJson = JsonUtility.ToJson(new { nodes = Array.Empty<Node[]>(), edges = Array.Empty<Edge[]>() });
            // Call the GraphQL resolver to save the graph and retrieve its ID
            var query = @"
            mutation CreateGraph($input: CreateGraphInput!) {
                createGraph(input: $input) {
                    id
                }
            }
        ";

            var variables = new
            {
                input = new
                {
                    projectId = projectId,
                    name = name,
                    graphJson = graphJson
                }
            };

            var queryObject = new GraphQLRequest
            {
                Query = query,
                OperationName = "CreateGraph",
                Variables = variables
            };
            var graphQLResponse = await client.SendQueryAsync(queryObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Graph saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["id"].ToString();
                Debug.Log($"Assigned ID from database: {returnedId}");
                newGraph = new Graph(returnedId);
                newGraph.name = name;
            }
            else
            {
                Debug.LogError("Failed to save graph to the database.");
                foreach (var error in graphQLResponse["errors"])
                {
                    Debug.LogError($"GraphQL Error: {error["message"]}");
                    if (error["extensions"] != null)
                    {
                        Debug.LogError($"Error Extensions: {error["extensions"]}");
                    }
                }
            }

            actionLogger.LogAction(nameof(CreateNodeGraphAsync), newGraph.Id);
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
        return newGraph;
    }
    public async Task SaveGraphAsync(Graph toSave) // Saves the graph to the database
    {
        var query = @"
            mutation SaveGraph($input: SaveGraphInput!) {
                saveGraph(input: $input) {
                    id
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                projectId = client.GetCurrentProjectId(),
                name = toSave.name,
                graphJson = JsonUtility.ToJson(toSave)
            }
        };

        var queryObject = new GraphQLRequest
        {
            Query = query,
            OperationName = "SaveGraph",
            Variables = variables
        };
        Graph newGraph;
        try
        {

            var graphQLResponse = await client.SendQueryAsync(queryObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Graph saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["id"].ToString();
                newGraph = new Graph(returnedId);
                Debug.Log($"Assigned ID from database: {returnedId}");
            }
            else
            {
                Debug.LogError("Failed to save object to the database.");
                foreach (var error in graphQLResponse["errors"])
                {
                    Debug.LogError($"GraphQL Error: {error["message"]}");
                    if (error["extensions"] != null)
                    {
                        Debug.LogError($"Error Extensions: {error["extensions"]}");
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
        return;
    }

    // -- EDIT GRAPH FUNCTIONS --

    public void AddNodeToGraph(Graph graph, NodeDefinition def)
    {
        // TODO: Add node to GraphQL
        NodeIndex index = graph.AddNode(def);
        actionLogger.LogAction(nameof(AddNodeToGraph), graph, def, index);
        Debug.Log($"Adding node {def.Name} to graph at index {index}");

        // MUTATIONS TO UPDATE JSON STRING
    }

    public void RemoveNodeFromGraph(Graph graph, NodeIndex node)
    {
        Graph.NodeMemory nodeMem = graph.GetMemory(node);
        List<(PortIndex, PortIndex)> dataEdges = new();
        List<(PortIndex, NodeIndex)> execEdges = new();
        graph.EdgesOf(node, dataEdges, execEdges);
        graph.RemoveNode(node);
        actionLogger.LogAction(nameof(RemoveNodeFromGraph), graph, nodeMem, dataEdges, execEdges);
        Debug.Log("Removed node from graph");

        // MUTATIONS TO UPDATE JSON STRING
    }

    public void AddDataEdgeToGraph(Graph graph, PortIndex from, PortIndex to)
    {
        if (!graph.TryAddEdge(from.Node, from.Port, to.Node, to.Port))
        {
            Debug.LogError("Failed to add edge");
            return;
        }
        actionLogger.LogAction(nameof(AddDataEdgeToGraph), graph, (from, to));
        Debug.Log($"Adding edge at {from}:{to}");

        // MUTATIONS TO UPDATE JSON STRING
    }

    public void RemoveDataEdgeFromGraph(Graph graph, PortIndex from, PortIndex to)
    {
        graph.RemoveDataEdge(from, to);
        actionLogger.LogAction(nameof(RemoveDataEdgeFromGraph), graph, from, to);
        Debug.Log($"Deleted edge from {from} to {to}");
    }

    public void AddExecEdgeToGraph(Graph graph, PortIndex from, NodeIndex to)
    {
        if (!graph.TryAddExecutionEdge(from.Node, from.Port, to))
        {
            Debug.LogError("Failed to add edge");
            return;
        }
        actionLogger.LogAction(nameof(AddExecEdgeToGraph), graph, (from, to));
        Debug.Log($"Adding edge at {from}:{to}");
    }

    public void RemoveExecEdgeFromGraph(Graph graph, PortIndex from, NodeIndex to)
    {
        graph.RemoveExecutionEdge(from, to);
        actionLogger.LogAction(nameof(RemoveExecEdgeFromGraph), graph, from, to);
        Debug.Log($"Deleted exec edge from {from} to {to}");
    }
    public void SetNodePosition(Graph graph, NodeIndex node, Vector2 position)
    {
        if (!graph.ContainsNode(node))
        {
            Debug.LogError("Failed to move node because it does not exist");
            return;
        }
        Vector2 prevPosition = graph.GetNode(node).Position;
        graph.GetNode(node).Position = position;
        actionLogger.LogAction(nameof(SetNodePosition), graph, node, prevPosition, position);
        Debug.Log($"Moved node {node} to {position}");
    }

    public void SetNodeFieldValue(Graph graph, NodeIndex node, int field, NodeValue value)
    {
        Node nodeData = graph.GetNode(node);
        if (!nodeData.TryGetField(field, out NodeValue oldValue))
        {
            Debug.LogError("Failed to get old field value when setting node field");
            oldValue = NodeValue.Null;
        }
        if (!nodeData.TrySetFieldValue(field, value))
        {
            Debug.LogError("Failed to set node field value");
            return;
        }
        actionLogger.LogAction(nameof(SetNodeFieldValue), graph, node, field, oldValue);
        Debug.Log($"Set node {node} field {field} to {value}");
    }

    public void SetNodeInputConstantValue(Graph graph, NodeIndex node, int port, NodeValue value)
    {
        Node nodeData = graph.GetNode(node);
        if (!nodeData.TryGetInputValue(port, out NodeValue oldValue))
        {
            Debug.LogError("Failed to get old input port constant value when setting input port constant");
            oldValue = NodeValue.Null;
        }
        if (!nodeData.TrySetInputValue(port, value))
        {
            Debug.LogError("Failed to set node input port constant value");
            return;
        }
        actionLogger.LogAction(nameof(SetNodeFieldValue), graph, node, port, oldValue);
        Debug.Log($"Set node {node} input port {port} to {value}");
    }

    public void GameObjectAddLocalImpulse(GameObject obj, Vector3 dirMag)
    {
        // TODO: Punted implementation until rewrite for less to rewrite
        // ^ also punting undo functionality
    }

    // ---Spawn/Save Object---
    public GameObject SpawnPrimitive(Vector3 position, Quaternion rotation, Vector3 scale, EditableMesh inputMesh = null, ShapeType type = ShapeType.Cube)
    {
        var spawnedMesh = NetworkSpawnManager.Find(this).SpawnWithRoomScopeWithReturn(PrimitiveSpawner.instance.primitive);
        EditableMesh em = spawnedMesh.GetComponent<EditableMesh>();
        if (inputMesh == null)
        {
            // Based on the shape
            EditableMesh newMesh = PrimitiveGenerator.CreatePrimitive(type);
            em.CreateMesh(newMesh);
            Destroy(newMesh.gameObject);
        }
        else if (inputMesh != null)
        {
            em.CreateMesh(inputMesh);
        }
        else
        {
            Debug.LogError("Error!!");
            return null;
        }
        spawnedMesh.transform.position = position;
        spawnedMesh.transform.rotation = rotation;
        spawnedMesh.transform.localScale = scale;
        return spawnedMesh;
    }

    public Task<GameObject> SpawnObject(string prefabName, Vector3 spawnPosition,
        Vector3 scale = default, Quaternion spawnRotation = default, SpawnScope scope = SpawnScope.Room)
    {
        var tcs = new TaskCompletionSource<GameObject>();
        //Search for Object in the catalogue
        GameObject newObject = GetPrefabByName(prefabName);
        GameObject spawnedObject = null;
        Debug.Log(newObject);
        Debug.Log(spawnManager);
        UnityAction<GameObject, IRoom, IPeer, NetworkSpawnOrigin> action = null;
        action = async (GameObject go, IRoom room, IPeer peer, NetworkSpawnOrigin origin) =>
            {
                spawnedObject = go;
                if (spawnedObject != null)
                {
                    spawnedObject.transform.position = spawnPosition;
                    spawnedObject.transform.rotation = spawnRotation;
                    spawnedObject.transform.localScale = scale;
                }
                if (scope == SpawnScope.Room)
                {
                    // Serialize the object's transform
                    TransformData transformData = new TransformData
                    {
                        position = spawnedObject.transform.position,
                        rotation = spawnedObject.transform.rotation,
                        scale = spawnedObject.transform.localScale
                    };

                    RfObject rfObject = new RfObject
                    {
                        // projectId = projectId,  (!!!)
                        name = spawnedObject.name,
                        type = "Prefab",
                        graphId = null,
                        transformJson = JsonUtility.ToJson(transformData),
                        meshJson = "{}",
                        projectId = client.GetCurrentProjectId()
                    };

                    var createObject = new GraphQLRequest
                    {
                        Query = @"
                    mutation CreateObject($input: CreateObjectInput!) {
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
                                graphId = rfObject.graphId,
                                type = rfObject.type,
                                meshJson = rfObject.meshJson,
                                transformJson = rfObject.transformJson
                            }
                        }
                    };
                    try
                    {
                        Debug.Log("Sending GraphQL request to: " + client.server + "/graphql");
                        Debug.Log("Request: " + JsonUtility.ToJson(createObject));
                        var graphQLResponse = await client.SendQueryAsync(createObject);
                        if (graphQLResponse["data"] != null)
                        {
                            Debug.Log("Object saved to the database successfully.");

                            // Extract the ID from the response and assign it to the rfObject
                            var returnedId = graphQLResponse["data"]["saveObject"]["id"].ToString();
                            rfObject.id = returnedId;
                            Debug.Log($"Assigned ID from database: {rfObject.id}");
                            spawnedObjects[spawnedObject] = rfObject;
                            // Update the name of the spawned object in the scene
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
                            foreach (var error in graphQLResponse["errors"])
                            {
                                Debug.LogError($"GraphQL Error: {error["message"]}");
                                if (error["extensions"] != null)
                                {
                                    Debug.LogError($"Error Extensions: {error["extensions"]}");
                                }
                            }
                        }
                        actionLogger.LogAction(nameof(SpawnObject), prefabName, spawnPosition, scale, spawnRotation, scope);
                        tcs.SetResult(newObject);
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
                else
                {
                    Debug.LogError("Could not find the spawned object in the scene.");
                    tcs.SetResult(null);
                }
                spawnManager.OnSpawned.RemoveListener(action);
            };
        if (newObject != null && spawnManager != null)
        {
            // Spawn the object with the given scope
            switch (scope)
            {
                case SpawnScope.Room:
                    spawnManager.OnSpawned.AddListener(action);
                    spawnManager.SpawnWithRoomScope(newObject);
                    Debug.Log("Spawned with Room Scope");
                    break;
                case SpawnScope.Peer:
                    spawnedObject = spawnManager.SpawnWithPeerScope(newObject);
                    Debug.Log("Spawned with Peer Scope");
                    break;
                default:
                    Debug.LogError("Unknown spawn scope");
                    break;
            }
        }
        else
        {
            Debug.LogError("Prefab not found or NetworkSpawnManager is not initialized.");
            return null;
        }
        return tcs.Task;
    }

    private async Task SaveObjectToDatabase(RfObject rfObject)
    {
        if (client == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            return;
        }

        var saveObject = new GraphQLRequest
        {
            Query = @"
            mutation UpdateObject($input: SaveObjectInput!) {
                updateObject(input: $input) {
                    id
                }
            }",
            OperationName = "UpdateObject",
            Variables = new
            {
                input = new
                {
                    projectId = rfObject.projectId,
                    name = rfObject.name,
                    graphId = rfObject.graphId,
                    type = rfObject.type,
                    meshJson = rfObject.meshJson,
                    transformJson = rfObject.transformJson
                }
            }
        };

        try
        {
            Debug.Log("Sending GraphQL request to: " + client.server + "/graphql");
            Debug.Log("Request: " + JsonUtility.ToJson(saveObject));

            var graphQLResponse = await client.SendQueryAsync(saveObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Object saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["saveObject"]["id"].ToString();
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
                foreach (var error in graphQLResponse["errors"])
                {
                    Debug.LogError($"GraphQL Error: {error["message"]}");
                    if (error["extensions"] != null)
                    {
                        Debug.LogError($"Error Extensions: {error["extensions"]}");
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

    // --- Fetch/Populate Room---
    public async void FetchAndPopulateObjects()
    {
        var objectsInDatabase = await FetchObjectsByProjectId(client.GetCurrentProjectId());
        if (objectsInDatabase != null)
        {
            //ObjectUI.PopulateUI(contentContainer, objectPrefab, objectsInDatabase); USED TO POPULATE UI
            PopulateRoom(objectsInDatabase);
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
            var graphQLResponse = await client.SendQueryAsync(getObjectsQuery);
            if (graphQLResponse["data"] != null)
            {
                //Debug.Log("GraphQL Response Data: " + graphQLResponse.Data.ToString());

                var data = graphQLResponse["data"]["getObjectsByProjectId"];
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
                var errors = graphQLResponse["errors"];
                if (errors != null)
                {
                    foreach (var error in errors)  // Log any errors in the response
                    {
                        Debug.LogError($"GraphQL Error: {error["message"]}");
                        if (error["Extensions"] != null)
                        {
                            Debug.LogError($"Error Extensions: {error["Extensions"]}");
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

        if (catalogue == null || spawnManager == null)
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
                spawnManager.SpawnWithRoomScope(prefab);

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

    // ---Select/Edit---
    public GameObject FindSpawnedObject(string id)
    {
        // !I dont know what this method is used for but I wont delete it!
        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager is not initialized.");
            return null;
        }

        // Search in the spawnedForRoom dictionary (What are the Dictonarys For?)
        foreach (var kvp in spawnManager.GetSpawnedForRoom())
        {
            if (kvp.Value.name.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Search in the spawnedForPeers dictionary (What are the Dictonarys For?)
        foreach (var peerDict in spawnManager.GetSpawnedForPeers())
        {
            foreach (var kvp in peerDict.Value)
            {
                if (kvp.Value.name.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
        }

        Debug.LogWarning($"Object named {id} not found in the spawned objects.");
        return null;
    }

    public void SelectAndOutlineObject(string id)
    {
        // Find the object with the given ID
        GameObject objectToSelect = GameObject.Find(id);

        if (objectToSelect != null)
        {
            // Apply outline effect to the object
            OutlineEffect(objectToSelect);

            // If there is already a selected object, remove the outline
            if (selectedObject != null && selectedObject != objectToSelect)
            {
                RemoveOutlineEffect(selectedObject);
            }

            // Update the reference to the currently selected object
            selectedObject = objectToSelect;

            // Initialize previous transform values
            previousPosition = selectedObject.transform.position;
            previousRotation = selectedObject.transform.rotation;
            previousScale = selectedObject.transform.localScale;

            // Set the objectId for database updates
            objectId = id;
        }
        else
        {
            Debug.LogWarning("Object with ID " + id + " not found.");
        }
    }

    // Method to update the transform of a networked object
    public async Task UpdateObjectTransform(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (selectedObject != null)
        {
            if (selectedObject.transform.position != previousPosition ||
                selectedObject.transform.rotation != previousRotation ||
                selectedObject.transform.localScale != previousScale)
            {
                // Update the previous transform values
                previousPosition = selectedObject.transform.position;
                previousRotation = selectedObject.transform.rotation;
                previousScale = selectedObject.transform.localScale;

                // Send updated transform to the database
                TransformData transformData = new TransformData
                {
                    position = selectedObject.transform.position,
                    rotation = selectedObject.transform.rotation,
                    scale = selectedObject.transform.localScale
                };

                // Await SaveObjectTransformToDatabase
                await SaveObjectTransformToDatabase(objectId, transformData);
            }
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
            mutation UpdateObject($input: UpdateObjectInput!) {
                updateObject(input: $input) {
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
            var graphQLResponse = await client.SendQueryAsync(saveObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Object transform updated in the database successfully.");
            }
            else
            {
                Debug.LogError("Failed to update object transform in the database.");
                foreach (var error in graphQLResponse["errors"])
                {
                    Debug.LogError($"GraphQL Error: {error["message"]}");
                    if (error["Extensions"] != null)
                    {
                        Debug.LogError($"Error Extensions: {error["Extensions"]}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception: " + ex.Message);
        }
    }

    // ---Despawn/Delete---
    public void DespawnObject(GameObject objectToDespawn)
    {
        if (objectToDespawn != null)
        {
            actionLogger.LogAction(nameof(DespawnObject), objectToDespawn.name, objectToDespawn.transform.position, objectToDespawn.transform.rotation, objectToDespawn.transform.localScale);
            spawnManager.Despawn(objectToDespawn);
            Debug.Log("Despawned: " + objectToDespawn.name);
        }
        else
        {
            Debug.LogError("Object to despawn is null");
        }
    }

#if UNITY_EDITOR
    public void AddPrefabToCatalogue(GameObject prefab)
    {
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            if (!spawnManager.catalogue.prefabs.Contains(prefab))
            {
                spawnManager.catalogue.prefabs.Add(prefab);
                Debug.Log($"Prefab {prefab.name} added to catalogue.");
                SaveCatalogue();
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} is already in the catalogue.");
            }
        }
        else
        {
            Debug.LogError("NetworkSpawnManager or its catalogue is null.");
        }
    }

    public void AddGameObjectToCatalogue(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("Cannot add null GameObject to catalogue.");
            return;
        }

        string localPath = "Assets/Prefabs/" + gameObject.name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, localPath);
        if (prefab != null)
        {
            AddPrefabToCatalogue(prefab);
        }
        else
        {
            Debug.LogError("Failed to create prefab from GameObject.");
        }
    }

    private void SaveCatalogue()
    {
        EditorUtility.SetDirty(spawnManager.catalogue);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Catalogue saved.");
    }
#endif

    public void UndoLastAction()
    {
        Debug.Log("Attempting to undo last action.");
        Debug.Log($"Action stack count before undo: {actionLogger.GetActionStackCount()}");

        actionLogger.StartUndo();
        var lastAction = actionLogger.GetLastAction();
        actionLogger.EndUndo();

        if (lastAction == null)
        {
            Debug.Log("No actions to undo.");
            return;
        }

        if (lastAction is ActionLogger.CompoundAction compoundAction)
        {
            foreach (var action in compoundAction.Actions)
            {
                UndoSingleAction(action);
            }
        }
        else
        {
            UndoSingleAction(lastAction);
        }

        // Clear the action stack after undo
        //actionLogger.ClearActionStack();
        Debug.Log($"Action stack after undo: {actionLogger.GetActionStackCount()}");
        //StopAllCoroutines();
    }

    private async void UndoSingleAction(ActionLogger.LoggedAction action)
    {
        switch (action.FunctionName)
        {
            case nameof(SpawnObject):
                string prefabName = (string)action.Parameters[0] + "(Clone)";
                Debug.Log("The spawned object's name is " + prefabName);
                GameObject spawnedObject = FindSpawnedObject(prefabName);
                if (spawnedObject != null)
                {
                    DespawnObject(spawnedObject);
                }
                break;

            case nameof(DespawnObject):
                string objName = ((string)action.Parameters[0]).Replace("(Clone)", "").Trim();
                Debug.Log("Undoing the despawn of object named " + objName);
                Vector3 position = (Vector3)action.Parameters[1];
                Quaternion rotation = (Quaternion)action.Parameters[2];
                Vector3 scale = (Vector3)action.Parameters[3];
                GameObject respawnedObject = await SpawnObject(objName, position, scale, rotation, SpawnScope.Peer);
                if (respawnedObject != null)
                {
                    respawnedObject.transform.localScale = scale;
                }
                break;

            case nameof(UpdateObjectTransform):
                string objectName = (string)action.Parameters[0];
                Vector3 oldPosition = (Vector3)action.Parameters[1];
                Quaternion oldRotation = (Quaternion)action.Parameters[2];
                Vector3 oldScale = (Vector3)action.Parameters[3];
                Debug.Log("Undoing the transform of object named " + objectName);
                GameObject obj = FindSpawnedObject(objectName);
                if (obj != null)
                {
                    UpdateObjectTransform(objectName, oldPosition, oldRotation, oldScale);
                }
                else
                {
                    Debug.LogError($"Object named {objectName} not found during undo transform.");
                }
                break;

            case nameof(AddNodeToGraph):
                Graph graph = (Graph)action.Parameters[0];
                NodeIndex index = (NodeIndex)action.Parameters[2];

                graph.RemoveNode(index);

                break;

                // Add cases for other functions...
        }
    }

    public List<string> GetPrefabNames()
    {
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            return spawnManager.catalogue.prefabs.Select(prefab => prefab.name).ToList();
        }
        return new List<string>();
    }

    public List<NodeDefinition> GetAvailableNodeDefinitions()
    {
        List<NodeDefinition> defs = new();

        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/Actions/"));
        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/Control Flow/"));
        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/Functional/"));
        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/State/"));

        // In the future, this can query the DB to access online node definitions

        return defs;
    }

    public void StartCompoundAction()
    {
        actionLogger.StartCompoundAction();
    }

    public void EndCompoundAction()
    {
        actionLogger.EndCompoundAction();
    }

    // Add these using directives if not already present

    public void LogRaycastHitLocation(XRRayInteractor rayInteractor)
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hitResult))
        {
            Vector3 hitPosition = hitResult.point;
            Debug.Log($"Raycast hit at position: {hitPosition}");

            // Log the action
            actionLogger.LogAction(nameof(LogRaycastHitLocation), hitPosition);
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
        }
    }
}

// ===== RF Object Class =====
[System.Serializable]
public class RfObject
{
    public string id; // Unique ID for each object
    public string projectId;
    public string name;
    public string graphId;
    public string type;
    public string transformJson;
    public string meshJson;
}

[System.Serializable]
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Vector2[] uv;
}

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

