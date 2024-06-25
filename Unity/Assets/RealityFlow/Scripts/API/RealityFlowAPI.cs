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
using Graph = RealityFlow.NodeGraph.Graph;
using Ubiq.Rooms;
using UnityEngine.Events;
using RealityFlow.NodeUI;
using UnityEngine.Rendering;
using Microsoft.MixedReality.GraphicsTools;
using System.Collections.Immutable;
using System.Reflection;
using UnityEngine.UI; // For Selectable and Navigation
using UnityEngine.EventSystems; // For UGUIInputAdapterDraggable
using Microsoft.MixedReality.Toolkit.Input; // For ObjectManipulator
using Microsoft.MixedReality.Toolkit.SpatialManipulation; // For ConstraintManager and TetheredPlacement
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Unity.VisualScripting;
using System.Collections;
using TMPro;
using RealityFlow.Collections;
using RealityFlow.Scripting;






#if UNITY_EDITOR
using UnityEditor;
#endif

public class RealityFlowAPI : MonoBehaviour, INetworkSpawnable
{
    private string objectId;
    [SerializeField] private NetworkSpawnManager spawnManager;
    private GameObject selectedObject;
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 previousScale;
    public Material outlineMaterial;
    public ActionLogger actionLogger;
    private NetworkContext networkContext;
    public NetworkId NetworkId { get; set; }
    private static RealityFlowAPI _instance;                // SINGLE INSTANCE OF THE API
    private static readonly object _lock = new object();   // ENSURES THREAD SAFETY
    public PrefabCatalogue catalogue; // Prefab Catalog
    public GameObject whiteboardPrefab;
    public static GameObject NearMenuToolbox;
    public GameObject nearMenuReference;

    public static GameObject DeleteMenu;
    public GameObject delteMenuReference;

    ImmutableDictionary<string, NodeDefinition> nodeDefinitionDict;
    public ImmutableDictionary<string, NodeDefinition> NodeDefinitionDict
    {
        get
        {
            nodeDefinitionDict ??= GetAvailableNodeDefinitions()
                    .ToDictionary(def => def.Name)
                    .ToImmutableDictionary();

            return nodeDefinitionDict;
        }
    }

    private RealityFlowClient client;
    public bool isUndoing = false;
    readonly Dictionary<GameObject, RfObject> spawnedObjects = new();
    public ImmutableDictionary<GameObject, RfObject> SpawnedObjects
        => spawnedObjects.ToImmutableDictionary();
    readonly Dictionary<string, GameObject> spawnedObjectsById = new();
    public ImmutableDictionary<string, GameObject> SpawnedObjectsById
        => spawnedObjectsById.ToImmutableDictionary();

    readonly Dictionary<GameObject, int> templatesDict = new();
    public ImmutableDictionary<GameObject, int> TemplatesDict => templatesDict.ToImmutableDictionary();
    readonly List<GameObject> templates = new();
    public IEnumerable<GameObject> Templates => templates;
    public Action OnTemplatesChanged;
    [SerializeField]
    public GameObject audioPlayer; 

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
        try
        {
            ScriptUtilities.Init();
        }
        catch
        {
            Debug.LogError("Failed to initialize script utils");
        }

        AudioClipNames = AudioClips.Select(kv => kv.Key).ToList();

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            spawnManager = NetworkSpawnManager.Find(this);
            if (spawnManager == null)
            {
                Debug.LogError("NetworkSpawnManager not found on the network scene!");
            }
            spawnManager.roomClient.OnRoomUpdated.AddListener(OnRoomUpdated); // Add listener for room updates

            if (actionLogger == null)
            {
                actionLogger = gameObject.AddComponent<ActionLogger>();
            }
        }

        client = RealityFlowClient.Find(this);

        IEnumerator HookNetworkedPlayManager()
        {
            while (!NetworkedPlayManager.Instance)
                yield return null;

            NetworkedPlayManager.Instance.exitPlayMode.AddListener(ClearNonPersisted);
        }

        StartCoroutine(HookNetworkedPlayManager());

        // assign the near menu toolbox
        NearMenuToolbox = nearMenuReference;
        DeleteMenu = delteMenuReference;
    }

    // ===== SUPPORT FUNCTIONS =====
    public string ExportSpawnedObjectsData()
    {
        StringBuilder sb = new StringBuilder();

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
    public void AssignGraph(Graph newGraph, GameObject obj)
    {
        // Adds the graph ID to the object's graph property as an update.
        spawnedObjects[obj].graphId = newGraph.Id;
        SaveObjectToDatabase(spawnedObjects[obj]);
    }


    #region Graph Functions
    public Graph CreateNodeGraphAsync()
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
                    graphJson = "{}",
                }
            };

            var queryObject = new GraphQLRequest
            {
                Query = query,
                OperationName = "CreateGraph",
                Variables = variables
            };
            var graphQLResponse = client.SendQueryAsync(queryObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Graph saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["createGraph"]["id"].ToString();
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
            if (!isUndoing)
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

    public void SaveGraphAsync(Graph toSave) // Saves the graph to the database
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

            var graphQLResponse = client.SendQueryAsync(queryObject);
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

    #endregion

    #region Visual Scripting (Template, Static, Collide, Grav, Text)

    public void SetTemplate(VisualScript obj, bool becomeTemplate)
    {
        bool contains = templatesDict.TryGetValue(obj.gameObject, out int index);
        if (contains && !becomeTemplate)
        {
            templatesDict.Remove(obj.gameObject);
            templates.RemoveAt(index);
        }
        else if (!contains && becomeTemplate)
        {
            templatesDict.Add(obj.gameObject, templates.Count);
            templates.Add(obj.gameObject);
        }

        RfObject rfObj = SpawnedObjects[obj.gameObject];
        rfObj.isTemplate = becomeTemplate;

        SaveObjectToDatabase(rfObj);

        LogActionToServer("SetTemplate", new { objId = rfObj.id, become = becomeTemplate });

        OnTemplatesChanged?.Invoke();
    }

    public void SetStatic(VisualScript obj, bool becomeStatic)
    {
        RfObject rfObj = SpawnedObjects[obj.gameObject];
        rfObj.isStatic = becomeStatic;

        LogActionToServer("SetStatic", new { objId = rfObj.id, become = becomeStatic });

        SaveObjectToDatabase(rfObj);
    }

    public void SetCollidable(VisualScript obj, bool becomeCollidable)
    {
        RfObject rfObj = SpawnedObjects[obj.gameObject];
        rfObj.isCollidable = becomeCollidable;

        LogActionToServer("SetCollidable", new { objId = rfObj.id, become = becomeCollidable });

        SaveObjectToDatabase(rfObj);
    }

    public void SetGravity(VisualScript obj, bool becomeGravityEnabled)
    {
        RfObject rfObj = SpawnedObjects[obj.gameObject];
        rfObj.isGravityEnabled = becomeGravityEnabled;

        LogActionToServer("SetGravity", new { objId = rfObj.id, become = becomeGravityEnabled });

        SaveObjectToDatabase(rfObj);
    }

    public void SetUIText(GameObject textComp, string text)
    {
        // TODO: Network
        
        textComp.GetComponent<TMP_Text>().text = text;
    }

    [SerializeField]
    private SerializableDict<string, AudioClip> AudioClips = new();
    public List<string> AudioClipNames { get; private set; }

    public void PlaySound(string clip, Vector3 position)
    {
        GameObject player = Instantiate(audioPlayer, position, Quaternion.identity);
        AudioSource source = player.GetComponent<AudioSource>();
        source.clip = AudioClips[clip];
        source.Play();
        player.GetComponent<DestroyObject>().lifeTime = AudioClips[clip].length + 1f;
    }

    // -- EDIT GRAPH FUNCTIONS --
    public void SendGraphUpdateToDatabase(string graphJson, string graphId)
    {
        var query = @"
            mutation UpdateGraph($input: UpdateGraphInput!) {
                updateGraph(input: $input) {
                    id
                    graphJson
                }
            }
        ";

        var variables = new
        {
            input = new
            {
                id = graphId,
                graphJson = graphJson
            }
        };

        var queryObject = new GraphQLRequest
        {
            Query = query,
            OperationName = "UpdateGraph",
            Variables = variables
        };

        try
        {
            var graphQLResponse = client.SendQueryAsync(queryObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Graph updated in the database successfully.");

                // Extract the ID from the response
                var returnedId = graphQLResponse["data"]["updateGraph"]["id"].ToString();
                Debug.Log($"Assigned ID from database: {returnedId}");
            }
            else
            {
                Debug.LogError("Failed to update the graph in the database.");
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

    public NodeIndex AddNodeToGraph(Graph graph, NodeDefinition def)
    {
        // add the node to the graph
        NodeIndex index = graph.AddNode(def);
        if (!isUndoing)
            actionLogger.LogAction(nameof(AddNodeToGraph), graph, def, index);

        // Serialize the graph object to JSON
        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Adding node {def.Name} to graph at index {index}");

        LogActionToServer("AddNode", new { graphId = graph.Id, defName = def.Name, index });

        SendGraphUpdateToDatabase(graphJson, graph.Id);
        return index;
    }

    public void RemoveNodeFromGraph(Graph graph, NodeIndex node)
    {
        Graph.NodeMemory nodeMem = graph.GetMemory(node);
        List<(PortIndex, PortIndex)> dataEdges = new();
        List<(PortIndex, NodeIndex)> execEdges = new();
        graph.EdgesOf(node, dataEdges, execEdges);
        graph.RemoveNode(node);
        if (!isUndoing)
            actionLogger.LogAction(nameof(RemoveNodeFromGraph), graph, nodeMem, dataEdges, execEdges);
        Debug.Log("Removed node from graph");

        // Serialize the graph object to JSON
        string graphJson = JsonUtility.ToJson(graph);
        // Debug.Log($"Adding node {def} to graph at index {index}");

        LogActionToServer("RemoveNode", new { graphId = graph.Id, node });

        SendGraphUpdateToDatabase(graphJson, graph.Id);
    }

    public void AddDataEdgeToGraph(Graph graph, PortIndex from, PortIndex to)
    {
        if (!graph.TryAddEdge(from.Node, from.Port, to.Node, to.Port))
        {
            Debug.LogError("Failed to add edge");
            return;
        }
        if (!isUndoing)
            actionLogger.LogAction(nameof(AddDataEdgeToGraph), graph, (from, to));
        Debug.Log($"Adding edge at {from}:{to}");

        // MUTATIONS TO UPDATE JSON STRING
        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Adding edge {from}:{to} to graph");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("AddDataEdge", new { graphId = graph.Id, fromNode = from.Node, fromPort = from.Port, toNode = to.Node, toPort = to.Port });
    }

    public void RemoveDataEdgeFromGraph(Graph graph, PortIndex from, PortIndex to)
    {
        graph.RemoveDataEdge(from, to);
        if (!isUndoing)
            actionLogger.LogAction(nameof(RemoveDataEdgeFromGraph), graph, from, to);
        Debug.Log($"Deleted edge from {from} to {to}");

        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Deleting edge {from}:{to} to graph");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("RemoveDataEdge", new { graphId = graph.Id, fromNode = from.Node, fromPort = from.Port, toNode = to.Node, toPort = to.Port });
    }

    public void AddExecEdgeToGraph(Graph graph, PortIndex from, NodeIndex to)
    {
        if (!graph.TryAddExecutionEdge(from.Node, from.Port, to))
        {
            Debug.LogError("Failed to add edge");
            return;
        }
        if (!isUndoing)
            actionLogger.LogAction(nameof(AddExecEdgeToGraph), graph, (from, to));
        Debug.Log($"Adding edge at {from}:{to}");

        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Adding exec edge {from}:{to} to graph");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("AddExecEdge", new { graphId = graph.Id, fromNode = from.Node, fromPort = from.Port, toNode = to });
    }

    public void RemoveExecEdgeFromGraph(Graph graph, PortIndex from, NodeIndex to)
    {
        graph.RemoveExecutionEdge(from, to);
        if (!isUndoing)
            actionLogger.LogAction(nameof(RemoveExecEdgeFromGraph), graph, from, to);
        Debug.Log($"Deleted exec edge from {from} to {to}");

        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Removing exec edge {from}:{to} to graph");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("RemoveExecEdge", new { graphId = graph.Id, fromNode = from.Node, fromPort = from.Port, toNode = to });
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
        if (!isUndoing)
            actionLogger.LogAction(nameof(SetNodePosition), graph, node, prevPosition, position);
        Debug.Log($"Moved node {node} to {position}");

        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Moving node {node} to {position}");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("MoveNode", new { graphId = graph.Id, node, fromPosition = prevPosition, toPosition = position });
    }

    public void SetNodeFieldValue(Graph graph, NodeIndex node, int field, NodeValue value)
    {
        Node nodeData = graph.GetNode(node);
        if (!nodeData.TryGetField(field, out NodeValue oldValue))
        {
            Debug.LogError("Failed to get old field value when setting node field");
            oldValue = null;
        }
        if (!nodeData.TrySetFieldValue(field, value))
        {
            Debug.LogError("Failed to set node field value");
            return;
        }
        if (!isUndoing)
            actionLogger.LogAction(nameof(SetNodeFieldValue), graph, node, field, oldValue);
        Debug.Log($"Set node {node} field {field} to {value}");

        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Setting node {node} field {field} to value {value}");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("SetNodeField", new { graphId = graph.Id, node, field, oldValue, newValue = value });
    }

    public void SetNodeInputConstantValue(Graph graph, NodeIndex node, int port, NodeValue value)
    {
        Node nodeData = graph.GetNode(node);
        if (!nodeData.TryGetInputValue(port, out NodeValue oldValue))
        {
            Debug.LogError("Failed to get old input port constant value when setting input port constant");
            oldValue = null;
        }
        if (!nodeData.TrySetInputValue(port, value))
        {
            Debug.LogError("Failed to set node input port constant value");
            return;
        }
        if (!isUndoing)
            actionLogger.LogAction(nameof(SetNodeFieldValue), graph, node, port, oldValue);
        Debug.Log($"Set node {node} input port {port} to {value}");

        string graphJson = JsonUtility.ToJson(graph);
        Debug.Log($"Setting node {node} port {port} constant to {value}");

        SendGraphUpdateToDatabase(graphJson, graph.Id);

        LogActionToServer("SetNodePortConstant", new { graphId = graph.Id, node, port, oldValue, newValue = value });
    }

    public void AddVariableToGraph(Graph graph, string name, NodeValueType type)
    {
        graph.AddVariable(name, type);

        LogActionToServer("AddVariable", new { graphId = graph.Id, name, type = type.ToString() });
    }

    public void RemoveVariableFromGraph(Graph graph, string name)
    {
        graph.RemoveVariable(name);

        LogActionToServer("RemoveVariable", new { graphId = graph.Id, name });
    }

    public void GameObjectAddLocalImpulse(GameObject obj, Vector3 dirMag)
    {
        // TODO: NETWORK IT AAAAAA
        obj.GetComponent<Rigidbody>().AddRelativeForce(dirMag, ForceMode.Impulse);
    }
    #endregion

    #region Update and Spawn primitive
    // // ---Spawn/Save Object---
    public GameObject UpdatePrimitive(GameObject spawnedMesh)
    {
        Debug.Log("Updating primitive...");
        EditableMesh em = spawnedMesh.GetComponent<EditableMesh>();
        TransformData transformData = new TransformData
        {
            position = spawnedMesh.transform.position,
            rotation = spawnedMesh.transform.rotation,
            scale = spawnedMesh.transform.localScale
        };

        PrimitiveRebuilder.RebuildMesh(spawnedMesh.GetComponent<EditableMesh>(), spawnedMesh.GetComponent<NetworkedMesh>().lastSize);
        SerializableMeshInfo smi = spawnedMesh.GetComponent<EditableMesh>().smi;

        RfObject rfObject = spawnedObjects[spawnedMesh];
        rfObject.transformJson = JsonUtility.ToJson(transformData);
        rfObject.meshJson = JsonUtility.ToJson(smi);
        // Manually serialize faces into a json array of arrays
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < smi.faces.Length; i++)
        {
            sb.Append("[");
            for (int j = 0; j < smi.faces[i].Length; j++)
            {
                sb.Append(smi.faces[i][j]);
                if (j < smi.faces[i].Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("]");
            if (i < smi.faces.Length - 1)
            {
                sb.Append(",");
            }
        }
        sb.Append("]");
        Debug.Log(sb.ToString());
        // Add sb.ToString() as the value of the faces property, adding it instead of replacing it.
        rfObject.meshJson = rfObject.meshJson.Insert(rfObject.meshJson.Length - 1, $",\"faces\":{sb.ToString()}");

        var createObject = new GraphQLRequest
        {
            Query = @"
            mutation UpdateObject($input: UpdateObjectInput!) {
                updateObject(input: $input) {
                    id
                }
            }",
            OperationName = "UpdateObject",
            Variables = new
            {
                input = new
                {
                    id = rfObject.id,
                    name = rfObject.name,
                    graphId = rfObject.graphId,
                    meshJson = rfObject.meshJson,
                    transformJson = rfObject.transformJson
                }
            }
        };

        try
        {
            var graphQLResponse = client.SendQueryAsync(createObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Object saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["updateObject"]["id"].ToString();
                rfObject.id = returnedId;
                Debug.Log($"Assigned ID from database: {rfObject.id}");
                spawnedObjects[spawnedMesh] = rfObject;
                spawnedObjectsById[returnedId] = spawnedMesh;

                // Update the name of the spawned object in the scene
                if (spawnedMesh != null)
                {
                    spawnedMesh.name = rfObject.id;
                    Debug.Log($"Updated spawned object name to: {spawnedMesh.name}");
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
            //if (!isUndoing)
            //actionLogger.LogAction(nameof(SpawnPrimitive), position, rotation, scale, inputMesh, type);
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

        return spawnedMesh;
    }

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

        // Set the Primitive's transform Data
        spawnedMesh.transform.position = position;
        spawnedMesh.transform.rotation = rotation;
        spawnedMesh.transform.localScale = scale;
        TransformData transformData = new TransformData
        {
            position = position,
            rotation = rotation,
            scale = scale
        };
        // Generate faces
        spawnedMesh.GetComponent<EditableMesh>().LoadMeshData();
        SerializableMeshInfo smi = spawnedMesh.GetComponent<EditableMesh>().smi;
        RfObject rfObject = new RfObject
        {
            name = "PrimitiveBase",
            type = "Primitive",
            graphId = null,
            transformJson = JsonUtility.ToJson(transformData),
            meshJson = JsonUtility.ToJson(smi),
            projectId = client.GetCurrentProjectId()
        };
        // Manually serialize faces into a json array of arrays
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < smi.faces.Length; i++)
        {
            sb.Append("[");
            for (int j = 0; j < smi.faces[i].Length; j++)
            {
                sb.Append(smi.faces[i][j]);
                if (j < smi.faces[i].Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("]");
            if (i < smi.faces.Length - 1)
            {
                sb.Append(",");
            }
        }
        sb.Append("]");
        Debug.Log(sb.ToString());
        // Add sb.ToString() as the value of the faces property, adding it instead of replacing it.
        rfObject.meshJson = rfObject.meshJson.Insert(rfObject.meshJson.Length - 1, $",\"faces\":{sb.ToString()}");
        Debug.Log(rfObject);

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
            var graphQLResponse = client.SendQueryAsync(createObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Object saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["createObject"]["id"].ToString();
                rfObject.id = returnedId;
                Debug.Log($"Assigned ID from database: {rfObject.id}");
                spawnedObjects[spawnedMesh] = rfObject;
                spawnedObjectsById[returnedId] = spawnedMesh;

                // Update the name of the spawned object in the scene
                if (spawnedMesh != null)
                {
                    spawnedMesh.name = rfObject.id;
                    Debug.Log($"Updated spawned object name to: {spawnedMesh.name}");
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
            if (!isUndoing)
                actionLogger.LogAction(nameof(SpawnPrimitive), position, rotation, scale, inputMesh, type);
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

        return spawnedMesh;
    }
    #endregion

    public void LogActionToServer(string action, object data)
    {
      var createObject = new GraphQLRequest
        {
            Query = @"
        mutation LogAction($input2: LogEntryInput!) {
            addLogEntry(input: $input2) {
                id  
            }
        }",
        OperationName = "LogAction",
        Variables = new
        {
            input2 = new
            {
                eventType = action,
                eventData = JsonUtility.ToJson(data),
            }
        }
    };
    try
    {
        var graphQLResponse = client.SendQueryAsync(createObject);
        if (graphQLResponse["data"] != null)
        {
            // Extract the ID from the response and assign it to the rfObject
            var returnedId = graphQLResponse["data"]["createObject"]["id"].ToString();
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
    #region Spawn Object
    public GameObject SpawnObject(string prefabName, Vector3 spawnPosition,
        Vector3 scale = default, Quaternion spawnRotation = default, SpawnScope scope = SpawnScope.Room)
    {
        //Search for Object in the catalogue
        GameObject newObject = GetPrefabByName(prefabName);
        GameObject spawnedObject = null;
        Debug.Log(newObject);
        Debug.Log(spawnManager);
        UnityAction<GameObject, IRoom, IPeer, NetworkSpawnOrigin> action = null;
        Debug.Log("####### The spawn scope is " + scope + " ############");
        action = (GameObject go, IRoom room, IPeer peer, NetworkSpawnOrigin origin) =>
            {
                Debug.Log("inside the action with the scope" + scope + " ############");
                spawnedObject = go;
                if (spawnedObject != null)
                {
                    spawnedObject.transform.position = spawnPosition;
                    spawnedObject.transform.rotation = spawnRotation;
                    spawnedObject.transform.localScale = scale;


                    // Add Rigidbody
                    if (spawnedObject.GetComponent<Rigidbody>() == null)
                    {
                        var rigidbody = spawnedObject.AddComponent<Rigidbody>();
                        rigidbody.useGravity = false;
                        rigidbody.isKinematic = true;
                    }

                    // Add BoxCollider based on bounds
                    if (spawnedObject.GetComponent<BoxCollider>() == null)
                    {
                        BoxCollider boxCollider = spawnedObject.AddComponent<BoxCollider>();
                        Renderer renderer = spawnedObject.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            boxCollider.center = renderer.bounds.center - spawnedObject.transform.position;
                            boxCollider.size = renderer.bounds.size;
                        }
                        else
                        {
                            // Handle case where mesh is on a child object
                            Renderer childRenderer = spawnedObject.GetComponentInChildren<Renderer>();
                            if (childRenderer != null)
                            {
                                boxCollider.center = childRenderer.bounds.center - spawnedObject.transform.position;
                                boxCollider.size = childRenderer.bounds.size;
                            }
                        }
                    }

                    // Add UGUIInputAdapterDraggable
                    // var draggableAdapter = spawnedObject.AddComponent<UGUIInputAdapterDraggable>();
                    // draggableAdapter.interactable = true;
                    // draggableAdapter.transition = Selectable.Transition.None;
                    // draggableAdapter.navigation = new Navigation { mode = Navigation.Mode.Automatic };

                    // Add TetheredPlacement script with Distance Threshold set to 20
                    // var tetheredPlacement = spawnedObject.AddComponent<TetheredPlacement>();
                    // tetheredPlacement.GetType().GetField("distanceThreshold", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tetheredPlacement, 20.0f);

                    

                    // Add NetworkedOperationCache script
                    // if (spawnedObject.GetComponent<NetworkedOperationCache>() == null)
                    // {
                    //     spawnedObject.AddComponent<NetworkedOperationCache>();
                    // }

                    // Add ObjectManipulator
                        
                    // Add whiteboard attatch
                    if(spawnedObject.GetComponent<AttachedWhiteboard>() == null)
                    {
                        spawnedObject.AddComponent<AttachedWhiteboard>();
                    }
                    

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
                        projectId = client.GetCurrentProjectId(),
                        originalPrefabName = prefabName
                    };

                    var createObject = new GraphQLRequest
                    {
                        Query = @"
                    mutation CreateObject($input: CreateObjectInput!, $input2: LogEntryInput!) {
                        createObject(input: $input) {
                            id
                        }
                        addLogEntry(input: $input2) {
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
                            },
                            input2 = new
                            {
                                eventType = "Create Object",
                                name = rfObject
                            }
                        }
                    };
                    try
                    {
                        Debug.Log("Sending GraphQL request to: " + client.server + "/graphql");
                        Debug.Log("Request: " + JsonUtility.ToJson(createObject));
                        var graphQLResponse = client.SendQueryAsync(createObject);
                        if (graphQLResponse["data"] != null)
                        {
                            Debug.Log("Object saved to the database successfully.");

                            // Extract the ID from the response and assign it to the rfObject
                            var returnedId = graphQLResponse["data"]["createObject"]["id"].ToString();
                            rfObject.id = returnedId;
                            Debug.Log($"Assigned ID from database: {rfObject.id}");
                            spawnedObjects[spawnedObject] = rfObject;
                            spawnedObjectsById[returnedId] = spawnedObject;
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
                        if (!isUndoing)
                            actionLogger.LogAction(nameof(SpawnObject), spawnedObject.name, spawnPosition, scale, spawnRotation, scope);
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
                    Debug.LogWarning("Could not find the spawned object in the scene or the object was spawned with peer scope.");
                    if (!isUndoing)
                        actionLogger.LogAction(nameof(SpawnObject), prefabName, spawnPosition, scale, spawnRotation, scope);
                    return;
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
                    // Directly call the action for Peer scope
                    action.Invoke(spawnedObject, null, null, NetworkSpawnOrigin.Local);
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

        return spawnedObject;
    }
    #endregion

    readonly HashSet<GameObject> nonPersistentObjects = new();

    public void InstantiateNonPersisted(GameObject obj, Vector3 position, Quaternion rotation)
    {
        try
        {
            RfObject objectDetails = SpawnedObjects[obj];
            string prefabName = objectDetails.name;
            GameObject prefab = GetPrefabByName(prefabName);

            GameObject spawned = spawnManager.SpawnWithPeerScope(prefab);
            spawned.transform.SetPositionAndRotation(position, rotation);
            spawned.SetActive(true);

            if (obj.GetComponent<VisualScript>() is VisualScript script)
            {
                VisualScript newScript = spawned.AddComponent<VisualScript>();
                newScript.graph = script.graph;

                // TODO: If in play mode call OnEnterPlayMode
            }

            nonPersistentObjects.Add(spawned);
            spawnedObjects.Add(spawned, objectDetails);

            if (!isUndoing)
                actionLogger.LogAction(nameof(InstantiateNonPersisted), spawned);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void DestroyNonPersisted(GameObject obj)
    {
        bool nonPersistent = nonPersistentObjects.Contains(obj);
        if (nonPersistent)
        {
            spawnedObjects.Remove(obj);
            spawnManager.Despawn(obj);
        }
        else
            obj.SetActive(false);

        if (!isUndoing)
            actionLogger.LogAction(nameof(DestroyNonPersisted), obj, nonPersistent);
    }

    public void ClearNonPersisted()
    {
        foreach (GameObject obj in nonPersistentObjects)
        {
            Destroy(obj);
        }
        nonPersistentObjects.Clear();
    }

    private void SaveObjectToDatabase(RfObject rfObject)
    {
        if (client == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            return;
        }

        var saveObject = new GraphQLRequest
        {
            Query = @"
            mutation UpdateObject($input: UpdateObjectInput!) {
                updateObject(input: $input) {
                    id
                }
            }",
            OperationName = "UpdateObject",
            Variables = new
            {
                input = new
                {
                    id = rfObject.id,
                    name = rfObject.name,
                    graphId = rfObject.graphId,
                    meshJson = rfObject.meshJson,
                    transformJson = rfObject.transformJson,
                    isTemplate = rfObject.isTemplate,
                    isStatic = rfObject.isStatic,
                    isCollidable = rfObject.isCollidable,
                    isGravityEnabled = rfObject.isGravityEnabled,
                }
            }
        };

        try
        {
            Debug.Log("Sending GraphQL request to: " + client.server + "/graphql");
            Debug.Log("Request: " + JsonUtility.ToJson(saveObject));

            var graphQLResponse = client.SendQueryAsync(saveObject);
            if (graphQLResponse["data"] != null && graphQLResponse["errors"]==null)
            {
                Debug.Log("Object saved to the database successfully.");

                // Extract the ID from the response and assign it to the rfObject
                var returnedId = graphQLResponse["data"]["updateObject"]["id"].ToString();
                rfObject.id = returnedId;
                Debug.Log($"Assigned ID from database: {rfObject.id}");

        // Update the name of the spawned object in the scene
                GameObject spawnedObject = spawnedObjectsById[rfObject.id];//GameObject.Find(rfObject.name);
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
    public void FetchAndPopulateObjects()
    {
        var objectsInDatabase = FetchObjectsByProjectId(client.GetCurrentProjectId());
        if (objectsInDatabase != null)
        {
            //ObjectUI.PopulateUI(contentContainer, objectPrefab, objectsInDatabase); USED TO POPULATE UI
            PopulateRoom(objectsInDatabase);
        }
    }
    private List<RfObject> FetchObjectsByProjectId(string projectId)
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
                    graphId
                    isTemplate
                    isStatic
                    isCollidable
                    isGravityEnabled
                }
            }",
            Variables = new { projectId = projectId }
        };

        try
        {
            var graphQLResponse = client.SendQueryAsync(getObjectsQuery);
            if (graphQLResponse["data"] != null)
            {
                var data = graphQLResponse["data"]["getObjectsByProjectId"];
                if (data == null)
                {
                    Debug.LogWarning("No objects found for the given project ID.");
                    return null;
                }

                if (data is JArray objectsArray)
                {
                    var objectsInDatabase = objectsArray.ToObject<List<RfObject>>();
                    if (objectsInDatabase == null)
                    {
                        Debug.LogWarning("Deserialized objects are null.");
                        return null;
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

        // Clear the current dictionary
        spawnedObjects.Clear();
        spawnedObjectsById.Clear();

        var getGraphsQuery = new GraphQLRequest
        {
            Query = @"
            query GetGraphsByProjectId($projectId: String!) {
                getGraphsByProjectId(projectId: $projectId) {
                    id
                    graphJson
                }
            }",
            Variables = new { projectId = client.GetCurrentProjectId() }
        };

        List<GraphData> graphsInDatabase = null;

        try
        {
            var graphQLResponse = client.SendQueryAsync(getGraphsQuery);
            if (graphQLResponse["data"] != null)
            {
                var data = graphQLResponse["data"]["getGraphsByProjectId"];
                if (data == null)
                    Debug.LogWarning("No graphs found for the given project ID.");

                if (data is JArray graphsArray)
                {
                    graphsInDatabase = graphsArray.ToObject<List<GraphData>>();
                    if (graphsInDatabase == null)
                        Debug.LogWarning("Deserialized objects are null.");
                }
                else
                    Debug.LogWarning("Data is not a JArray.");
            }
            else
            {
                Debug.LogError("Failed to retrieve graphs. Response data is null.");
                var errors = graphQLResponse["errors"];
                if (errors != null)
                    foreach (var error in errors)
                    {
                        Debug.LogError($"GraphQL Error: {error["message"]}");
                        if (error["Extensions"] != null)
                            Debug.LogError($"Error Extensions: {error["Extensions"]}");
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
            Debug.LogError("Exception stack trace: " + ex.StackTrace);
        }

        Dictionary<string, GraphData> graphData = graphsInDatabase?.ToDictionary(graph => graph.id);

        foreach (RfObject obj in objectsInDatabase)
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
                var spawnedObject = spawnManager.SpawnWithRoomScopeWithReturn(prefab);
                if (spawnedObject.GetComponent<EditableMesh>() != null)
                {
                    Debug.Log("Primitive Base");

                    var serializableMesh = JsonUtility.FromJson<SerializableMeshInfo>(obj.meshJson);
                    // Deserialize the two dimensional array of integers from the json string and assign it to serializableMesh.faces

                    // StringBuilder sb = new StringBuilder();
                    // sb.Append("[");
                    // for (int i = 0; i < smi.faces.Length; i++)
                    // {
                    //     sb.Append("[");
                    //     for (int j = 0; j < smi.faces[i].Length; j++)
                    //     {
                    //         sb.Append(smi.faces[i][j]);
                    //         if (j < smi.faces[i].Length - 1)
                    //         {
                    //             sb.Append(",");
                    //         }
                    //     }
                    //     sb.Append("]");
                    //     if (i < smi.faces.Length - 1)
                    //     {
                    //         sb.Append(",");
                    //     }
                    // }
                    // sb.Append("]");

                    // Reverse the above serialization for the faces property of the string contained in obj.meshJson and assign it to serializableMesh.faces
                    //
                    obj.meshJson = obj.meshJson.Remove(obj.meshJson.Length - 1);
                    int start = obj.meshJson.LastIndexOf("\"faces\":") + 9;
                    int end = obj.meshJson.Length;
                    string faces = obj.meshJson.Substring(start, end - start - 1);
                    string[] faceArray = faces.Split('[');
                    int[][] facesArray = new int[faceArray.Length - 1][];
                    for (int i = 1; i < faceArray.Length; i++)
                    {
                        // If the last character is a , remove it
                        faceArray[i] = faceArray[i].TrimEnd(',');
                        string[] face = faceArray[i].Split(',');
                        // Remove any "]" characters from the last element of the array
                        facesArray[i - 1] = new int[face.Length];
                        for (int j = 0; j < face.Length; j++)
                        {
                            face[j] = face[j].Replace("]", "");
                            facesArray[i - 1][j] = int.Parse(face[j]);
                        }
                    }
                    serializableMesh.faces = facesArray;
                    Debug.Log(serializableMesh.lastSize);
                    // Error can't deserialize here for some reason. Can check with team or investigate 
                    spawnedObject.GetComponent<EditableMesh>().smi = serializableMesh;
                    Debug.Log(spawnedObject.GetComponent<EditableMesh>().baseShape);
                    Debug.Log(spawnedObject.GetComponent<NetworkedMesh>().lastSize);

                }
                Debug.Log("Spawned object with room scope");
                if (spawnedObject == null)
                {
                    Debug.LogError("Spawned object is null.");
                    return;
                }

                spawnedObject.AddComponent<AttachedWhiteboard>();
                if (obj.graphId != null && graphData.TryGetValue(obj.graphId, out GraphData graph))
                {
                    Debug.Log($"Attaching graphdata `{graph.graphJson}` to object {spawnedObject}");
                    Graph graphObj = JsonUtility.FromJson<Graph>(graph.graphJson);
                    spawnedObject.EnsureComponent<VisualScript>().graph = graphObj;
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

                spawnedObjects.Add(spawnedObject, obj);
                spawnedObjectsById.Add(obj.id, spawnedObject);

                Debug.Log($"Added object with ID: {obj.id}, Name: {obj.name} to dictionary");
                // Find the spawned object in the scene (assuming it's named the same as the prefab)
            }
            catch (Exception ex)
            {
                Debug.LogError("Error spawning object with NetworkSpawnManager: " + ex.Message);
                Debug.LogError("Exception stack trace: " + ex.StackTrace);
            }
        }

        // Debug log to show the dictionary was populated
        Debug.Log("Dictionary populated with " + spawnedObjects.Count + " objects.");
        foreach (var kvp in spawnedObjects)
            Debug.Log("GameObject: " + kvp.Key.name + ", RfObject: " + JsonUtility.ToJson(kvp.Value));

        Debug.Log("Room population complete.");
    }

    #region FindSpawnedObject By ID
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
    #endregion

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

    #region UpdateObjectTransform (Move Object)
    // Method to update the transform of a networked object
    public void UpdateObjectTransform(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = FindSpawnedObject(objectName);
        if (obj != null)
        {
            // Update the peer object transform
            UpdatePeerObjectTransform(obj, position, rotation, scale);

            // Prepare the transform data for the database update
            TransformData transformData = new TransformData
            {
                position = position,
                rotation = rotation,
                scale = scale
            };

            // Send the updated transform to the database
            SaveObjectTransformToDatabase(objectName, transformData);
        }
        else
        {
            Debug.LogWarning($"Object with name {objectName} not found.");
        }
    }
    public void SaveObjectTransformToDatabase(string objectId, TransformData transformData)
    {
        Debug.Log("Inside the save object transform to database function called from the update object transform function");
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
            OperationName = "UpdateObject",
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
            var graphQLResponse = client.SendQueryAsync(saveObject);
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
    #endregion

    #region Delete Functions
    // ---Despawn/Delete--
    public void DespawnAllObjectsInBothDictionarys()
    {
        // Despawn objects in spawnedObjects dictionary
        foreach (var kvp in spawnedObjects)
        {
            Destroy(kvp.Key);
        }
        spawnedObjects.Clear();

        // Despawn objects in spawnedObjectsById dictionary
        foreach (var kvp in spawnedObjectsById)
        {
            Destroy(kvp.Value);
        }
        spawnedObjectsById.Clear();
    }

    //This function is primarily for peer scope
    public void DespawnObject(GameObject objectToDespawn, SpawnScope scope = SpawnScope.Room)
    {
        if (objectToDespawn != null)
        {
            string objectId = objectToDespawn.name;
            //Set the originalPrefabName to the object's name by default in case of peer scope
            string originalPrefabName = objectId;
            if (spawnedObjects.TryGetValue(objectToDespawn, out RfObject rfObject))
                originalPrefabName = rfObject.originalPrefabName; // Ensure this field exists and is set correctly when spawning objects

            if (!isUndoing)
                actionLogger.LogAction(nameof(DespawnObject), originalPrefabName, objectToDespawn.transform.position, objectToDespawn.transform.rotation, objectToDespawn.transform.localScale, scope);

            if (scope == SpawnScope.Room)
            {
                // Remove object from the database
                RemoveObjectFromDatabase(objectId, () =>
                {
                    // Only despawn the object if it was successfully removed from the database
                    spawnManager.Despawn(objectToDespawn);
                    Debug.Log("Despawned: " + objectToDespawn.name);

                    // Remove the object from local dictionaries
                    spawnedObjects.Remove(objectToDespawn);
                    spawnedObjectsById.Remove(objectId);
                });
            }
            else
            {
                // Directly despawn the object if it's not room-scoped
                spawnManager.Despawn(objectToDespawn);
                Debug.Log("Despawned: " + objectToDespawn.name);

                // Remove the object from local dictionaries
                spawnedObjects.Remove(objectToDespawn);
                spawnedObjectsById.Remove(objectId);
            }

        }
        else
        {
            Debug.LogError("Object to despawn is null");
        }
    }

    private void RemoveObjectFromDatabase(string objectId, Action onSuccess)
    {
        if (client == null)
        {
            Debug.LogError("RealityFlowClient is not initialized.");
            return;
        }

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
            Debug.Log("Sending GraphQL request to: " + client.server + "/graphql");
            Debug.Log("Request: " + JsonUtility.ToJson(deleteObject));
            var graphQLResponse = client.SendQueryAsync(deleteObject);
            if (graphQLResponse["data"] != null)
            {
                Debug.Log("Object removed from the database successfully.");

                // Extract the ID from the response and ensure it matches the requested ID
                //var returnedId = graphQLResponse.Data.deleteObject.id;
                var returnedId = graphQLResponse["data"]["deleteObject"]["id"].ToString();
                if (returnedId == objectId)
                {
                    Debug.Log($"Successfully removed object with ID: {objectId} from the database.");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"Mismatch in deleted object ID. Requested: {objectId}, Returned: {returnedId}");
                }
            }
            else
            {
                Debug.LogError("Failed to remove object from the database.");
                if (graphQLResponse["errors"] != null)
                {
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
    #endregion
    private bool IsRoomScoped(GameObject obj)
    {
        // Replace with your actual logic to determine if the object is room-scoped
        return true; // Assuming all objects are room-scoped for now
    }

    #region Catalogue Code
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
    #endregion

    #region Undo Functionality
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
        isUndoing = true;
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
        isUndoing = false;
    }

    private void UndoSingleAction(ActionLogger.LoggedAction action)
    {
        switch (action.FunctionName)
        {
            case nameof(SpawnObject):
                string prefabName = (string)action.Parameters[0];
                Debug.Log("The spawned object's name is " + prefabName);
                GameObject spawnedObject = FindSpawnedObject(prefabName);
                if (spawnedObject != null)
                {
                    Debug.Log(prefabName + " existed, despawning it now");
                    DespawnObject(spawnedObject);
                }
                break;

            case nameof(DespawnObject):
                string objectId = action.Parameters[0] as string;
                if (spawnedObjectsById.ContainsKey(objectId))
                {
                    RfObject rfObject = spawnedObjects[spawnedObjectsById[objectId]];
                    string originalPrefabName = rfObject.originalPrefabName;
                    Vector3 position = (Vector3)action.Parameters[1];
                    Quaternion rotation = (Quaternion)action.Parameters[2];
                    Vector3 scale = (Vector3)action.Parameters[3];
                    SpawnScope scope = (SpawnScope)action.Parameters[4];
                    SpawnObject(originalPrefabName, position, scale, rotation, scope);
                }
                else
                {
                    string objName = action.Parameters[0] as string;
                    Debug.Log("Undoing the despawn of object named " + objName);
                    Vector3 position = (Vector3)action.Parameters[1];
                    Quaternion rotation = (Quaternion)action.Parameters[2];
                    Vector3 scale = (Vector3)action.Parameters[3];
                    SpawnScope scope = (SpawnScope)action.Parameters[4]; // Ensure the scope is logged during the initial action and passed here.

                    GameObject respawnedObject = SpawnObject(objName, position, scale, rotation, scope);
                    if (respawnedObject != null)
                    {
                        respawnedObject.transform.localScale = scale;
                    }
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

            case nameof(InstantiateNonPersisted):
                GameObject spawned = (GameObject)action.Parameters[0];

                Destroy(spawned);

                break;

            case nameof(DestroyNonPersisted):
                GameObject destroyed = (GameObject)action.Parameters[0];
                bool nonPersistent = (bool)action.Parameters[1];

                if (!nonPersistent)
                    destroyed.SetActive(true);

                break;

                // Add cases for other functions...
        }
    }
    #endregion
    public List<string> GetPrefabNames()
    {
        if (spawnManager != null && spawnManager.catalogue != null)
        {
            return spawnManager.catalogue.prefabs.Select(prefab => prefab.name).ToList();
        }
        return new List<string>();
    }

    private List<NodeDefinition> GetAvailableNodeDefinitions()
    {
        List<NodeDefinition> defs = new();

        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/Actions/"));
        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/Control Flow/"));
        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/Functional/"));
        defs.AddRange(Resources.LoadAll<NodeDefinition>("NodeGraph/Nodes/State/"));

        // In the future, this can query the DB to access online node definitions

        return defs;
    }

    public string[] GetNodeWhitelist()
    {
        // TODO: Access graphql DB to get whitelist for current project

        return null;
    }

    public void StartCompoundAction()
    {
        actionLogger.StartCompoundAction();
    }

    public void EndCompoundAction()
    {
        actionLogger.EndCompoundAction();
    }

    public void UpdatePeerObjectTransform(GameObject obj, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (obj != null)
        {
            // Log the current transform before making changes
            if (!isUndoing)
                actionLogger.LogAction(nameof(UpdatePeerObjectTransform), obj.name, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
            Debug.Log("The object's current location is: position: " + obj.transform.position + " Object rotation: " + obj.transform.rotation + " Object scale: " + obj.transform.localScale);
            Debug.Log("The object's desired location is: position: " + position + " Object rotation: " + rotation + " Object scale: " + scale);

            // Apply the transform changes
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = scale;

            // Serialize and send the transform update
            var message = new TransformMessage(obj.name, position, rotation, scale);
            Debug.Log($"Sending transform update: {message.ObjectName}, Pos: {message.Position}, Rot: {message.Rotation}, Scale: {message.Scale}");
            var jsonMessage = JsonUtility.ToJson(message);
            var propertyKey = $"transform.{obj.name}";
            spawnManager.roomClient.Room[propertyKey] = jsonMessage;
        }
        else
        {
            Debug.LogWarning("UpdatePeerObjectTransform: The provided GameObject is null.");
        }
    }

    // Method to process incoming peer transform updates
    public void ProcessPeerTransformUpdate(string propertyKey, string jsonMessage)
    {
        var transformMessage = JsonUtility.FromJson<TransformMessage>(jsonMessage);
        Debug.Log($"Received transform update: {transformMessage.ObjectName}, Pos: {transformMessage.Position}, Rot: {transformMessage.Rotation}, Scale: {transformMessage.Scale}");
        GameObject obj = FindSpawnedObjectByName(transformMessage.ObjectName);
        if (obj != null)
        {
            obj.transform.position = transformMessage.Position;
            obj.transform.rotation = transformMessage.Rotation;
            obj.transform.localScale = transformMessage.Scale;
        }
        else
        {
            Debug.LogError($"Object named {transformMessage.ObjectName} not found in ProcessPeerTransformUpdate.");
        }
    }

    // Method to handle room updates and process peer transform updates for all objects
    private void OnRoomUpdated(IRoom room)
    {
        foreach (var property in room)
        {
            if (property.Key.StartsWith("transform."))
            {
                ProcessPeerTransformUpdate(property.Key, property.Value);
            }
        }
    }

    // Method to find a spawned object by its name

    public GameObject FindSpawnedObjectByName(string objectName)
    {
        Debug.Log("In the FindSpawnedByName method");
        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager is not initialized.");
            return null;
        }

        foreach (var kvp in spawnManager.GetSpawnedForRoom())
        {
            if (kvp.Value.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        foreach (var peerDict in spawnManager.GetSpawnedForPeers())
        {
            foreach (var kvp in peerDict.Value)
            {
                if (kvp.Value.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
        }

        Debug.LogWarning($"Object named {objectName} not found in the spawned objects.");
        return null;
    }
    // TODO: Refactor this method to improve performance
}

// ===== RF Object Class =====
[Serializable]
public class RfObject
{
    public string id; // Unique ID for each object
    public string projectId;
    public string name;
    public string graphId;
    public string type;
    public string transformJson;
    public string meshJson;
    public string originalPrefabName;
    public bool isTemplate;
    public bool isStatic;
    public bool isCollidable = true;
    public bool isGravityEnabled = true;
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
[Serializable]
public class TransformMessage
{
    public string ObjectName;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public TransformMessage(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        ObjectName = objectName;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}

[Serializable]
public class GraphData
{
    public string id;
    public string graphJson;
}
