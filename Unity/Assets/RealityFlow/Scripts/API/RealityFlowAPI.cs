using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Spawning;
using System;
using System.Linq;
using RealityFlow.NodeGraph;
using UnityEngine.Assertions;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class RealityFlowAPI : MonoBehaviour, INetworkSpawnable
{
    private NetworkSpawnManager spawnManager;
    private ActionLogger actionLogger = new ActionLogger();
    private NetworkContext networkContext;

    public NetworkId NetworkId { get; set; }

    // Singleton instance
    private static RealityFlowAPI _instance;
    private static readonly object _lock = new object();

    public enum SpawnScope
    {
        Room,
        Peer
    }

    public static RealityFlowAPI Instance
    {
        get
        {
            lock (_lock)
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
            spawnManager.roomClient.OnRoomUpdated.AddListener(OnRoomUpdated);
        }
    }

    void Start()
    {

    }

    public void ProcessTransformUpdate(string propertyKey, string jsonMessage)
    {
        var transformMessage = JsonUtility.FromJson<TransformMessage>(jsonMessage);
        Debug.Log($"Received transform update: {transformMessage.ObjectName}, Pos: {transformMessage.Position}, Rot: {transformMessage.Rotation}, Scale: {transformMessage.Scale}");
        GameObject obj = FindSpawnedObject(transformMessage.ObjectName);
        if (obj != null)
        {
            obj.transform.position = transformMessage.Position;
            obj.transform.rotation = transformMessage.Rotation;
            obj.transform.localScale = transformMessage.Scale;
        }
        else
        {
            Debug.LogError($"Object named {transformMessage.ObjectName} not found in ProcessTransformUpdate.");
        }
    }

    private void OnRoomUpdated(IRoom room)
    {
        foreach (var property in room)
        {
            if (property.Key.StartsWith("transform."))
            {
                ProcessTransformUpdate(property.Key, property.Value);
            }
        }
    }

    public GameObject GetPrefabByName(string name)
    {
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

    public GameObject SpawnObject(string prefabName, Vector3 position, Vector3 scale = default, Quaternion rotation = default, SpawnScope scope = SpawnScope.Room)
    {
        GameObject newObject = spawnManager.catalogue.prefabs.Find(prefab => prefab.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase));
        if (newObject == null)
        {
            Debug.LogError($"Prefab not found: {prefabName}");
            return null;
        }

        switch (scope)
        {
            case SpawnScope.Room:
                spawnManager.SpawnWithRoomScope(newObject);
                Debug.Log("Spawned with Room Scope");
                break;
            case SpawnScope.Peer:
                spawnManager.SpawnWithPeerScope(newObject);
                Debug.Log("Spawned with Peer Scope");
                break;
            default:
                Debug.LogError("Unknown spawn scope");
                break;
        }

        if (newObject != null)
        {
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.transform.localScale = scale;
        }

        actionLogger.LogAction(nameof(SpawnObject), prefabName, position, scale, rotation, scope);
        return newObject;
    }

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

    public GameObject FindSpawnedObject(string objectName)
    {
        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager is not initialized.");
            return null;
        }

        // Search in the spawnedForRoom dictionary
        foreach (var kvp in spawnManager.GetSpawnedForRoom())
        {
            if (kvp.Value.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // Search in the spawnedForPeers dictionary
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

    // Method to update the transform of a networked object
    public void UpdateObjectTransform(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = FindSpawnedObject(objectName);
        if (obj == null)
        {
            obj = GetPrefabByName(objectName);
        }
        if (obj != null)
        {
            // Log the current transform before making changes
            actionLogger.LogAction(nameof(UpdateObjectTransform), objectName, obj.transform.position, obj.transform.rotation, obj.transform.localScale);
            Debug.Log("The object's current location is: position: " + obj.transform.position + " Object rotation: " + obj.transform.rotation + " Object scale: " + obj.transform.localScale);
            Debug.Log("The object's desired location is: position: " + position + " Object rotation: " + rotation + " Object scale: " + scale);

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = scale;

            // Serialize and send the transform update
            var message = new TransformMessage(objectName, position, rotation, scale);
            Debug.Log($"Sending transform update: {message.ObjectName}, Pos: {message.Position}, Rot: {message.Rotation}, Scale: {message.Scale}");
            var jsonMessage = JsonUtility.ToJson(message);
            var propertyKey = $"transform.{objectName}";
            spawnManager.roomClient.Room[propertyKey] = jsonMessage;
        }
        else
        {
            Debug.LogError($"Object named {objectName} not found.");
        }
    }

    public void AddNodeToGraph(Graph graph, NodeDefinition def)
    {
        // TODO: Add node to GraphQL
        NodeIndex index = graph.AddNode(def);
        actionLogger.LogAction(nameof(AddNodeToGraph), graph, def, index);
        Debug.Log($"Adding node {def.Name} to graph at index {index}");
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


    private void UndoSingleAction(ActionLogger.LoggedAction action)
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
                GameObject respawnedObject = SpawnObject(objName, position, scale, rotation, SpawnScope.Peer);
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

            case nameof(RemoveNodeFromGraph):
                graph = (Graph)action.Parameters[0];
                Graph.NodeMemory mem = (Graph.NodeMemory)action.Parameters[1];
                List<(PortIndex, PortIndex)> data = (List<(PortIndex, PortIndex)>)action.Parameters[2];
                List<(PortIndex, NodeIndex)> exec = (List<(PortIndex, NodeIndex)>)action.Parameters[3];

                graph.RememberNode(mem, out _);
                foreach ((PortIndex fromData, PortIndex toData) in data)
                    if (!graph.TryAddEdge(fromData.Node, fromData.Port, toData.Node, toData.Port))
                        Debug.LogError("Failed to restore edge of deleted node");
                foreach ((PortIndex fromExec, NodeIndex toExec) in exec)
                    if (!graph.TryAddExecutionEdge(fromExec.Node, fromExec.Port, toExec))
                        Debug.LogError("Failed to restore execution edge of deleted node");

                break;

            case nameof(AddDataEdgeToGraph):
                graph = (Graph)action.Parameters[0];
                (PortIndex from, PortIndex to) = ((PortIndex, PortIndex))action.Parameters[1];

                graph.RemoveDataEdge(from, to);
                break;

            case nameof(RemoveDataEdgeFromGraph):
                graph = (Graph)action.Parameters[0];
                from = (PortIndex)action.Parameters[1];
                to = (PortIndex)action.Parameters[2];

                if (!graph.TryAddEdge(from.Node, from.Port, to.Node, to.Port))
                    Debug.LogError($"Failed to undo delete edge from {from} to {to}");
                break;

            case nameof(AddExecEdgeToGraph):
                graph = (Graph)action.Parameters[0];
                (PortIndex port, NodeIndex node) = ((PortIndex, NodeIndex))action.Parameters[1];

                graph.RemoveExecutionEdge(port, node);
                break;

            case nameof(RemoveExecEdgeFromGraph):
                graph = (Graph)action.Parameters[0];
                from = (PortIndex)action.Parameters[1];
                NodeIndex toNode = (NodeIndex)action.Parameters[2];

                if (!graph.TryAddExecutionEdge(from.Node, from.Port, toNode))
                    Debug.LogError($"Failed to undo delete exec edge from {from} to {toNode}");
                break;

            case nameof(SetNodePosition):
                graph = (Graph)action.Parameters[0];
                node = (NodeIndex)action.Parameters[1];
                Vector2 prevPos = (Vector2)action.Parameters[2];
                Vector2 pos = (Vector2)action.Parameters[3];

                graph.GetNode(node).Position = prevPos;
                break;

            case nameof(SetNodeFieldValue):
                graph = (Graph)action.Parameters[0];
                node = (NodeIndex)action.Parameters[1];
                int field = (int)action.Parameters[2];
                NodeValue oldValue = (NodeValue)action.Parameters[3];

                if (!graph.GetNode(node).TrySetField(field, oldValue))
                    Debug.LogError("Failed to undo set field");

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