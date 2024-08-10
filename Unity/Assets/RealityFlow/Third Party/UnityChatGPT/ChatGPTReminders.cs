public static class Reminders
{

    //Use this script to add reminders that you want to pass to ChatGPT
    public static readonly string[] reminderTexts = new string[]
    {
        //Object Spawn Reminder 
        "The JSON schema is:\n" +
"{\n" +
"  \"type\": \"object\",\n" +
"  \"properties\": {\n" +
"    \"action\": {\n" +
"      \"type\": \"string\",\n" +
"      \"description\": \"The action to be performed, e.g., 'SpawnObject'.\"\n" +
"    },\n" +
"    \"parameters\": {\n" +
"      \"type\": \"object\",\n" +
"      \"properties\": {\n" +
"        \"prefabName\": {\n" +
"          \"type\": \"string\",\n" +
"          \"description\": \"The name of the prefab to spawn.\"\n" +
"        },\n" +
"        \"spawnPosition\": {\n" +
"          \"type\": \"object\",\n" +
"          \"properties\": {\n" +
"            \"x\": { \"type\": \"number\" },\n" +
"            \"y\": { \"type\": \"number\" },\n" +
"            \"z\": { \"type\": \"number\" }\n" +
"          },\n" +
"          \"required\": [\"x\", \"y\", \"z\"]\n" +
"        },\n" +
"        \"scale\": {\n" +
"          \"type\": \"object\",\n" +
"          \"properties\": {\n" +
"            \"x\": { \"type\": \"number\" },\n" +
"            \"y\": { \"type\": \"number\" },\n" +
"            \"z\": { \"type\": \"number\" }\n" +
"          },\n" +
"          \"required\": [\"x\", \"y\", \"z\"]\n" +
"        },\n" +
"        \"spawnRotation\": {\n" +
"          \"type\": \"object\",\n" +
"          \"properties\": {\n" +
"            \"x\": { \"type\": \"number\" },\n" +
"            \"y\": { \"type\": \"number\" },\n" +
"            \"z\": { \"type\": \"number\" },\n" +
"            \"w\": { \"type\": \"number\" }\n" +
"          },\n" +
"          \"required\": [\"x\", \"y\", \"z\", \"w\"]\n" +
"        },\n" +
"        \"scope\": {\n" +
"          \"type\": \"string\",\n" +
"          \"description\": \"The scope of the spawn, e.g., 'Room' or 'Peer'.\"\n" +
"        }\n" +
"      },\n" +
"      \"required\": [\"prefabName\", \"spawnPosition\", \"scale\", \"spawnRotation\", \"scope\"]\n" +
"    }\n" +
"  },\n" +
"  \"required\": [\"action\", \"parameters\"],\n" +
"  \"additionalProperties\": false\n" +
"}",


        //Reminder DespawnObject
"The JSON schema is:\n" +
"{\n" +
"  \"type\": \"object\",\n" +
"  \"properties\": {\n" +
"    \"action\": {\n" +
"      \"type\": \"string\",\n" +
"      \"description\": \"The action to be performed, e.g., 'DespawnObject'.\"\n" +
"    },\n" +
"    \"parameters\": {\n" +
"      \"type\": \"object\",\n" +
"      \"properties\": {\n" +
"        \"objectToDespawn\": {\n" +
"          \"type\": \"string\",\n" +
"          \"description\": \"The ID or name of the object to despawn.\"\n" +
"        },\n" +
"        \"scope\": {\n" +
"          \"type\": \"string\",\n" +
"          \"description\": \"The scope of the despawn, e.g., 'Room' or 'Peer'.\"\n" +
"        }\n" +
"      },\n" +
"      \"required\": [\"objectToDespawn\", \"scope\"]\n" +
"    }\n" +
"  },\n" +
"  \"required\": [\"action\", \"parameters\"],\n" +
"  \"additionalProperties\": false\n" +
"}",


//Reminder Update Object Transform

"The JSON schema is:\n" +
"{\n" +
"  \"type\": \"object\",\n" +
"  \"properties\": {\n" +
"    \"action\": {\n" +
"      \"type\": \"string\",\n" +
"      \"description\": \"The action to be performed, e.g., 'UpdateObjectTransform'.\"\n" +
"    },\n" +
"    \"parameters\": {\n" +
"      \"type\": \"object\",\n" +
"      \"properties\": {\n" +
"        \"objectName\": {\n" +
"          \"type\": \"string\",\n" +
"          \"description\": \"The name of the object to update.\"\n" +
"        },\n" +
"        \"position\": {\n" +
"          \"type\": \"object\",\n" +
"          \"properties\": {\n" +
"            \"x\": { \"type\": \"number\" },\n" +
"            \"y\": { \"type\": \"number\" },\n" +
"            \"z\": { \"type\": \"number\" }\n" +
"          },\n" +
"          \"required\": [\"x\", \"y\", \"z\"]\n" +
"        },\n" +
"        \"rotation\": {\n" +
"          \"type\": \"object\",\n" +
"          \"properties\": {\n" +
"            \"x\": { \"type\": \"number\" },\n" +
"            \"y\": { \"type\": \"number\" },\n" +
"            \"z\": { \"type\": \"number\" },\n" +
"            \"w\": { \"type\": \"number\" }\n" +
"          },\n" +
"          \"required\": [\"x\", \"y\", \"z\", \"w\"]\n" +
"        },\n" +
"        \"scale\": {\n" +
"          \"type\": \"object\",\n" +
"          \"properties\": {\n" +
"            \"x\": { \"type\": \"number\" },\n" +
"            \"y\": { \"type\": \"number\" },\n" +
"            \"z\": { \"type\": \"number\" }\n" +
"          },\n" +
"          \"required\": [\"x\", \"y\", \"z\"]\n" +
"        }\n" +
"      },\n" +
"      \"required\": [\"objectName\", \"position\", \"rotation\", \"scale\"]\n" +
"    }\n" +
"  },\n" +
"  \"required\": [\"action\", \"parameters\"],\n" +
"  \"additionalProperties\": false\n" +
"}",

        //Reminder 1
        "Only use the RealityFlow API which an example of which is referenced here:\n\n" +
        "# RealityFlowAPI Reference Sheet\n\n" +
        "## Overview\n\n" +
        "The `RealityFlowAPI` class provides a set of functions to manage and interact with objects and graphs in a networked Unity environment. This reference sheet details each function and provides short examples of how to use them.\n\n" +
        "## API Functions\n\n" +
        "### 1. `AssignGraph(Graph newGraph, GameObject obj)`\n" +
        "**Description**: Assigns a graph ID to the specified object.\n\n" +
        "**Example**:\n" +
        "```csharp\nGraph newGraph = CreateNodeGraphAsync();\nGameObject obj = FindSpawnedObject(\"exampleObjectId\");\nRealityFlowAPI.Instance.AssignGraph(newGraph, obj);\n```\n\n" +
        "### 2. `CreateNodeGraphAsync()`\n" +
        "**Description**: Creates a new node graph asynchronously.\n\n" +
        "**Example**:\n" +
        "```csharp\nGraph newGraph = RealityFlowAPI.Instance.CreateNodeGraphAsync();\n```\n\n" +
        "### 3. `SaveGraphAsync(Graph toSave)`\n" +
        "**Description**: Saves the specified graph to the database.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.SaveGraphAsync(newGraph);\n```\n\n" +
        "### 4. `SendGraphUpdateToDatabase(string graphJson, string graphId)`\n" +
        "**Description**: Sends the updated graph data to the database.\n\n" +
        "**Example**:\n" +
        "```csharp\nstring updatedGraphJson = JsonUtility.ToJson(newGraph);\nRealityFlowAPI.Instance.SendGraphUpdateToDatabase(updatedGraphJson, newGraph.Id);\n```\n\n" +
        "### 5. `AddNodeToGraph(Graph graph, NodeDefinition def)`\n" +
        "**Description**: Adds a node to the specified graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nNodeDefinition nodeDef = // Get node definition\nNodeIndex index = RealityFlowAPI.Instance.AddNodeToGraph(graph, nodeDef);\n```\n\n" +
        "### 6. `RemoveNodeFromGraph(Graph graph, NodeIndex node)`\n" +
        "**Description**: Removes a node from the specified graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.RemoveNodeFromGraph(graph, nodeIndex);\n```\n\n" +
        "### 7. `AddDataEdgeToGraph(Graph graph, PortIndex from, PortIndex to)`\n" +
        "**Description**: Adds a data edge between two nodes in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.AddDataEdgeToGraph(graph, fromPortIndex, toPortIndex);\n```\n\n" +
        "### 8. `RemoveDataEdgeFromGraph(Graph graph, PortIndex from, PortIndex to)`\n" +
        "**Description**: Removes a data edge between two nodes in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.RemoveDataEdgeFromGraph(graph, fromPortIndex, toPortIndex);\n```\n\n" +
        "### 9. `AddExecEdgeToGraph(Graph graph, PortIndex from, NodeIndex to)`\n" +
        "**Description**: Adds an execution edge between two nodes in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.AddExecEdgeToGraph(graph, fromPortIndex, toNodeIndex);\n```\n\n" +
        "### 10. `RemoveExecEdgeFromGraph(Graph graph, PortIndex from, NodeIndex to)`\n" +
        "**Description**: Removes an execution edge between two nodes in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.RemoveExecEdgeFromGraph(graph, fromPortIndex, toNodeIndex);\n```\n\n" +
        "### 11. `SetNodePosition(Graph graph, NodeIndex node, Vector2 position)`\n" +
        "**Description**: Sets the position of a node in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.SetNodePosition(graph, nodeIndex, newPosition);\n```\n\n" +
        "### 12. `SetNodeFieldValue(Graph graph, NodeIndex node, int field, NodeValue value)`\n" +
        "**Description**: Sets the value of a node field in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.SetNodeFieldValue(graph, nodeIndex, fieldIndex, newValue);\n```\n\n" +
        "### 13. `SetNodeInputConstantValue(Graph graph, NodeIndex node, int port, NodeValue value)`\n" +
        "**Description**: Sets a constant value for a node input port in the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.SetNodeInputConstantValue(graph, nodeIndex, portIndex, newValue);\n```\n\n" +
        "### 14. `AddVariableToGraph(Graph graph, string name, NodeValueType type)`\n" +
        "**Description**: Adds a variable to the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.AddVariableToGraph(graph, variableName, variableType);\n```\n\n" +
        "### 15. `RemoveVariableFromGraph(Graph graph, string name)`\n" +
        "**Description**: Removes a variable from the graph.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.RemoveVariableFromGraph(graph, variableName);\n```\n\n" +
        "### 16. `SpawnObject(string prefabName, Vector3 spawnPosition, Vector3 scale = default, Quaternion spawnRotation = default, SpawnScope scope = SpawnScope.Room)`\n" +
        "**Description**: Spawns an object from the specified prefab at the given position, scale, and rotation.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.SpawnObject(objectPrefab.name, Vector3.zero, objectPrefab.transform.localScale, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);\nGameObject spawnedObject = RealityFlowAPI.Instance.SpawnObject(\"PrefabName\", spawnPosition, spawnScale, spawnRotation);\n```\n\n" +
        "### 17. `DespawnObject(GameObject objectToDespawn)`\n" +
        "**Description**: Despawns the specified object.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.DespawnObject(objectToDespawn);\n```\n\n" +
        "### 18. `FindSpawnedObject(string id)`\n" +
        "**Description**: Finds a spawned object by its ID.\n\n" +
        "**Example**:\n" +
        "```csharp\nGameObject foundObject = RealityFlowAPI.Instance.FindSpawnedObject(\"objectId\");\n```\n\n" +
        "### 19. `SelectAndOutlineObject(string id)`\n" +
        "**Description**: Selects and applies an outline effect to the object with the specified ID.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.SelectAndOutlineObject(\"objectId\");\n```\n\n" +
        "### 20. `UpdateObjectTransform(string objectName, Vector3 position, Quaternion rotation, Vector3 scale)`\n" +
        "**Description**: Updates the transform of the specified object.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.UpdateObjectTransform(\"objectName\", newPosition, newRotation, newScale);\n```\n\n" +
        "### 21. `UndoLastAction()`\n" +
        "**Description**: Undoes the last logged action.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.UndoLastAction();\n```\n\n" +
        "### 22. `StartCompoundAction()`\n" +
        "**Description**: Starts a compound action for batching multiple actions together.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.StartCompoundAction();\n```\n\n" +
        "### 23. `EndCompoundAction()`\n" +
        "**Description**: Ends a compound action.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.EndCompoundAction();\n```\n\n" +
        "## Support Functions\n\n" +
        "### `ExportSpawnedObjectsData()`\n" +
        "**Description**: Exports data for all spawned objects in the room.\n\n" +
        "**Example**:\n" +
        "```csharp\nstring data = RealityFlowAPI.Instance.ExportSpawnedObjectsData();\nDebug.Log(data);\n```\n\n" +
        "### `GetPrefabByName(string name)`\n" +
        "**Description**: Retrieves a prefab by its name from the catalogue.\n\n" +
        "**Example**:\n" +
        "```csharp\nGameObject prefab = RealityFlowAPI.Instance.GetPrefabByName(\"PrefabName\");\n```\n\n" +
        "### `FetchAndPopulateObjects()`\n" +
        "**Description**: Fetches objects from the database and populates the room with them.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.FetchAndPopulateObjects();\n```\n\n" +
        "### `FindSpawnedObjectByName(string objectName)`\n" +
        "**Description**: Finds a spawned object by its name.\n\n" +
        "**Example**:\n" +
        "```csharp\nGameObject foundObject = RealityFlowAPI.Instance.FindSpawnedObjectByName(\"ObjectName\");\n```\n\n" +
        "### `UpdatePeerObjectTransform(GameObject obj, Vector3 position, Quaternion rotation, Vector3 scale)`\n" +
        "**Description**: Updates the transform of a peer object and sends the update to the network.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.UpdatePeerObjectTransform(peerObject, newPosition, newRotation, newScale);\n```\n\n" +
        "### `ProcessPeerTransformUpdate(string propertyKey, string jsonMessage)`\n" +
        "**Description**: Processes a transform update received from a peer.\n\n" +
        "**Example**:\n" +
        "```csharp\nRealityFlowAPI.Instance.ProcessPeerTransformUpdate(\"propertyKey\", jsonMessage);\n```",
    

        //Reminder 2
        "do not include any explanations",

        //Reminder 3
        "aways include code with markdown",

         //Reminder 4
        "do not require any prefabs",

         //Reminder 5
        "do not require any references",

         //Reminder 6
        "all references should not be null",

         //Reminder 7
        "Use the namespaces using UnityEngine; using System.Collections; using Graph = RealityFlow.NodeGraph.Graph;  using RealityFlow.NodeGraph; ",

         //Reminder 8
        "ONLY GENERATE CODE THAT MATCHES WHAT THE USER IS REQUESTING NOTHING ELSE",

        //Reminder 9
        "DO NOT USE COROUTINES",

         //Reminder 10
        "All code should be in a static Execute method",

         //Reminder 11
        "Do not use any methods that you yourself do not create or know you have specific access to such as Invoke()",

         //Reminder 12
        "Remember that the Execute method is static and instance methods of MonoBehaviour cannot be called from a static method. If you want to use one do it like this example:\n\n" +
        "using UnityEngine;\n" +
        "using System.Collections;\n" +
        "using StarterAssets;\n\n" +
        "public class SpawnRectangle : MonoBehaviour\n" +
        "{\n" +
        "    public static void Execute()\n" +
        "    {\n" +
        "        string message = \"This is what we heard you say, is this correct: \\\"Spawn a Rectangle\\\"?\";\n" +
        "        Debug.Log(message);\n\n" +
        "        // Create a new GameObject and add this script to it\n" +
        "        GameObject go = new GameObject(\"SpawnRectangleObject\");\n" +
        "        SpawnRectangle script = go.AddComponent<SpawnRectangle>();\n\n" +
        "        // Start the coroutine to delay sending the confirmation message\n" +
        "        script.StartCoroutine(script.DelayedMessage(1.0f));\n" +
        "    }\n\n" +
        "    private IEnumerator DelayedMessage(float delay)\n" +
        "    {\n" +
        "        yield return new WaitForSeconds(delay);\n\n" +
        "        Debug.Log(\"Message will be sent in 1 seconds unless canceled or re-recorded.\");\n\n" +
        "        // Spawn a Rectangle object after the delay\n" +
        "        RealityFlowAPI.Instance.SpawnObject(\"Rectangle (Horizontal)\", new Vector3(0, 0, 0), Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Peer);\n\n" +
        "        // Optionally, destroy the GameObject after spawning to clean up\n" +
        "        Destroy(gameObject);\n" +
        "    }\n" +
        "}\n",

         //Reminder 13
        "DO NOT USE MONOBEHAVIOURS",

         //Reminder 14
        "If you spawn an object make it have the tag 'Spawned' and let it keep its natural scale which you can get with .transform.localScale",

         //Reminder 15
        //"IF YOU MAKE A NODE GRAPH OR VISUAL SCRIPT YOU MUST DO IT EXACTLY THIS WAY USING THE FUNCTIONS YOU SEE HERE. DO NOT DO IT ANY OTHER WAY:  private void CreateComprehensiveGraphProcedure(string objId, float spacing)     {         // Find the object         GameObject obj = GameObject.Find(objId);          if (obj == null)         {             Debug.LogError($"Object with ID {objId} not found.");             return;         }          // Ensure the object has a VisualScript component         var visualScript = obj.GetComponent<VisualScript>();         if (visualScript == null)         {             Debug.LogError("VisualScript component not found on the object.");             return;         }          // Get the current graph         Graph graph = visualScript.graph;         if (graph == null)         {             Debug.LogError("Graph not found on the VisualScript component.");             return;         }          // Create new node definitions         NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatAdd"];         NodeDefinition floatMultiplyDef = RealityFlowAPI.Instance.NodeDefinitionDict["FloatMultiply"];         NodeDefinition intGreaterOrEqualDef = RealityFlowAPI.Instance.NodeDefinitionDict["IntGreaterOrEqual"];         NodeDefinition setPositionDef = RealityFlowAPI.Instance.NodeDefinitionDict["SetPosition"];         NodeDefinition thisObjectDef = RealityFlowAPI.Instance.NodeDefinitionDict["ThisObject"];         NodeDefinition vector3Def = RealityFlowAPI.Instance.NodeDefinitionDict["Vector3 Right"];         NodeDefinition intAddDef = RealityFlowAPI.Instance.NodeDefinitionDict["IntAdd"];          // Define spacing         spacing = 100.0f;          // Position offset for spacing out the nodes in a rectangular grid pattern         Vector2[] positions = new Vector2[]         {         new Vector2(0, 0),         new Vector2(spacing, 0),         new Vector2(-spacing, 0),         new Vector2(0, spacing),         new Vector2(0, -spacing),         new Vector2(spacing, spacing),         new Vector2(-spacing, -spacing)         };          // Add new nodes to the graph and set their positions         NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);         RealityFlowAPI.Instance.SetNodePosition(graph, floatAddNode, positions[0]);          NodeIndex floatMultiplyNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatMultiplyDef);         RealityFlowAPI.Instance.SetNodePosition(graph, floatMultiplyNode, positions[1]);          NodeIndex greaterOrEqualNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, intGreaterOrEqualDef);         RealityFlowAPI.Instance.SetNodePosition(graph, greaterOrEqualNode, positions[2]);          NodeIndex setPositionNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, setPositionDef);         RealityFlowAPI.Instance.SetNodePosition(graph, setPositionNode, positions[3]);          NodeIndex thisObjectNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, thisObjectDef);         RealityFlowAPI.Instance.SetNodePosition(graph, thisObjectNode, positions[4]);          NodeIndex vector3Node = RealityFlowAPI.Instance.AddNodeToGraph(graph, vector3Def);         RealityFlowAPI.Instance.SetNodePosition(graph, vector3Node, positions[5]);          NodeIndex intAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, intAddDef);         RealityFlowAPI.Instance.SetNodePosition(graph, intAddNode, positions[6]);          // Set the input constant values for the new nodes         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 0, new FloatValue(2.0f));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 1, new FloatValue(3.0f));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatMultiplyNode, 1, new FloatValue(4.0f));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, greaterOrEqualNode, 0, new IntValue(5));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, greaterOrEqualNode, 1, new IntValue(10));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, intAddNode, 0, new IntValue(0));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, intAddNode, 1, new IntValue(1));          // Create connections (edges) between the nodes         // Logical connections         PortIndex addOutputPort = new PortIndex(floatAddNode, 0); // Output of FloatAdd node         PortIndex multiplyInputPort = new PortIndex(floatMultiplyNode, 0); // First input of FloatMultiply node         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, addOutputPort, multiplyInputPort);          // Conditional connections         PortIndex thisObjectOutput = new PortIndex(thisObjectNode, 0);         PortIndex vector3Output = new PortIndex(vector3Node, 0);         PortIndex conditionOutput = new PortIndex(greaterOrEqualNode, 0);         PortIndex setPositionTarget = new PortIndex(setPositionNode, 0);         PortIndex setPositionValue = new PortIndex(setPositionNode, 1);         PortIndex conditionInput = new PortIndex(setPositionNode, 2); // Assuming the condition input is on index 2         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, vector3Output, setPositionValue);         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, conditionOutput, conditionInput);          // Looping connections         PortIndex intAddOutput = new PortIndex(intAddNode, 0);         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, vector3Output, setPositionValue);         RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, intAddOutput, setPositionValue);          Debug.Log($"Added and linked nodes for comprehensive procedure in the graph.");     }",

         //Reminder 16
        "Use the provided object id which is the Object's name and the graph information to do graph manipulations",

         //Reminder 17
        "Do not send the updated graph to the database",

         //Reminder 18
        "Only define Node definitions like this:\n\n" +
        "NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"FloatAdd\"];\n" +
        "NodeDefinition floatMultiplyDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"FloatMultiply\"];\n" +
        "NodeDefinition intGreaterOrEqualDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"IntGreaterOrEqual\"];\n" +
        "NodeDefinition setPositionDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"SetPosition\"];\n" +
        "NodeDefinition thisObjectDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"ThisObject\"];\n" +
        "NodeDefinition vector3Def = RealityFlowAPI.Instance.NodeDefinitionDict[\"Vector3 Right\"];\n" +
        "NodeDefinition intAddDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"IntAdd\"];\n",

         //Reminder 19
        "Set values like this only:  RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 0, new FloatValue(2.0f));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 1, new FloatValue(3.0f));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatMultiplyNode, 1, new FloatValue(4.0f));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, greaterOrEqualNode, 0, new IntValue(5));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, greaterOrEqualNode, 1, new IntValue(10));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, intAddNode, 0, new IntValue(0));         RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, intAddNode, 1, new IntValue(1));",

         //Reminder 20
        "Do not make nodes or graphs any other way than the ways that I showed you above",

         //Reminder 21
        //"Space the nodes out when spawned use 100.0 spacing",

         //Reminder 22
        "Do what the user prompt says don't do any node or graph things unless specified in the prompt. For example: 'Spawn a cube' does not require any node graph code",

         //Reminder 23
        "",
        "-------------------------------------------------------------------------\n\n\n DO WHAT THE PROMPT SAYS, for example If it says to make a cube don't make nodes on the graph unless it says to. Ignore the following message unless specified: When making a node or manipulating a graph, only do it in the way you see here. Do not deviate from how this file is set up. Your responses should be structured very similarly to this example. Don't try to create a new graph, don't try to use JSON, don't try to update the database, do only what the user says. Do it like you see in this file: \n\n\n\n" +
        "Note this is just an example don't actually generate this, just something similar in line with the user prompt.\n" +
        "private void CreateComprehensiveGraphProcedure(string objId, float spacing)\n" +
        "{\n" +
        "    // Find the object\n" +
        "    GameObject obj = GameObject.Find(objId);\n" +
        "    if (obj == null)\n" +
        "    {\n" +
        "        Debug.LogError($\"Object with ID {objId} not found.\");\n" +
        "        return;\n" +
        "    }\n" +
        "    // Ensure the object has a VisualScript component\n" +
        "    var visualScript = obj.GetComponent<VisualScript>();\n" +
        "    if (visualScript == null)\n" +
        "    {\n" +
        "        Debug.LogError(\"VisualScript component not found on the object.\");\n" +
        "        return;\n" +
        "    }\n" +
        "    // Get the current graph\n" +
        "    Graph graph = visualScript.graph;\n" +
        "    if (graph == null)\n" +
        "    {\n" +
        "        Debug.LogError(\"Graph not found on the VisualScript component.\");\n" +
        "        return;\n" +
        "    }\n" +
        "    // Create new node definitions\n" +
        "    NodeDefinition floatAddDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"FloatAdd\"];\n" +
        "    NodeDefinition floatMultiplyDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"FloatMultiply\"];\n" +
        "    NodeDefinition intGreaterOrEqualDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"IntGreaterOrEqual\"];\n" +
        "    NodeDefinition setPositionDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"SetPosition\"];\n" +
        "    NodeDefinition thisObjectDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"ThisObject\"];\n" +
        "    NodeDefinition vector3Def = RealityFlowAPI.Instance.NodeDefinitionDict[\"Vector3 Right\"];\n" +
        "    NodeDefinition intAddDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"IntAdd\"];\n" +
        "    // Define spacing\n" +
        "    spacing = 100.0f;\n" +
        "    // Position offset for spacing out the nodes in a rectangular grid pattern\n" +
        "    Vector2[] positions = new Vector2[]\n" +
        "    {\n" +
        "        new Vector2(0, 0),\n" +
        "        new Vector2(spacing, 0),\n" +
        "        new Vector2(-spacing, 0),\n" +
        "        new Vector2(0, spacing),\n" +
        "        new Vector2(0, -spacing),\n" +
        "        new Vector2(spacing, spacing),\n" +
        "        new Vector2(-spacing, -spacing)\n" +
        "    }\n" +
        "    // Add new nodes to the graph and set their positions\n" +
        "    NodeIndex floatAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatAddDef);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, floatAddNode, positions[0]);\n" +
        "    NodeIndex floatMultiplyNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, floatMultiplyDef);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, floatMultiplyNode, positions[1]);\n" +
        "    NodeIndex greaterOrEqualNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, intGreaterOrEqualDef);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, greaterOrEqualNode, positions[2]);\n" +
        "    NodeIndex setPositionNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, setPositionDef);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, setPositionNode, positions[3]);\n" +
        "    NodeIndex thisObjectNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, thisObjectDef);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, thisObjectNode, positions[4]);\n" +
        "    NodeIndex vector3Node = RealityFlowAPI.Instance.AddNodeToGraph(graph, vector3Def);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, vector3Node, positions[5]);\n" +
        "    NodeIndex intAddNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, intAddDef);\n" +
        "    RealityFlowAPI.Instance.SetNodePosition(graph, intAddNode, positions[6]);\n" +
        "    // Set the input constant values for the new nodes\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 0, new FloatValue(2.0f));\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatAddNode, 1, new FloatValue(3.0f));\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, floatMultiplyNode, 1, new FloatValue(4.0f));\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, greaterOrEqualNode, 0, new IntValue(5));\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, greaterOrEqualNode, 1, new IntValue(10));\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, intAddNode, 0, new IntValue(0));\n" +
        "    RealityFlowAPI.Instance.SetNodeInputConstantValue(graph, intAddNode, 1, new IntValue(1));\n" +
        "    // Create connections (edges) between the nodes\n" +
        "    // Logical connections\n" +
        "    PortIndex addOutputPort = new PortIndex(floatAddNode, 0); // Output of FloatAdd node\n" +
        "    PortIndex multiplyInputPort = new PortIndex(floatMultiplyNode, 0); // First input of FloatMultiply node\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, addOutputPort, multiplyInputPort);\n" +
        "    // Conditional connections\n" +
        "    PortIndex thisObjectOutput = new PortIndex(thisObjectNode, 0);\n" +
        "    PortIndex vector3Output = new PortIndex(vector3Node, 0);\n" +
        "    PortIndex conditionOutput = new PortIndex(greaterOrEqualNode, 0);\n" +
        "    PortIndex setPositionTarget = new PortIndex(setPositionNode, 0);\n" +
        "    PortIndex setPositionValue = new PortIndex(setPositionNode, 1);\n" +
        "    PortIndex conditionInput = new PortIndex(setPositionNode, 2); // Assuming the condition input is on index 2\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, vector3Output, setPositionValue);\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, conditionOutput, conditionInput);\n" +
        "    // Looping connections\n" +
        "    PortIndex intAddOutput = new PortIndex(intAddNode, 0);\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, thisObjectOutput, setPositionTarget);\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, vector3Output, setPositionValue);\n" +
        "    RealityFlowAPI.Instance.AddDataEdgeToGraph(graph, intAddOutput, setPositionValue);\n" +
        "    Debug.Log($\"Added and linked nodes for comprehensive procedure in the graph.\");\n" +
        "}\n" +
        "Another example that you can use and should use is: if the prompt says 'add an impulse node to the graph' you should generate this exact code but of course change the object id to what the actual object is:\n" +
        "using UnityEngine;\n" +
        "using System.Collections;\n" +
        "using Graph = RealityFlow.NodeGraph.Graph;\n" +
        "using RealityFlow.NodeGraph;\n" +
        "\n" +
        "public class AddImpulseNode\n" +
        "{\n" +
        "    public static void Execute()\n" +
        "    {\n" +
        "        string objectId = \"66a95613acd8a0f4a79780e7\"; // Default object ID\n" +
        "        GameObject obj = GameObject.Find(objectId);\n" +
        "\n" +
        "        if (obj == null)\n" +
        "        {\n" +
        "            Debug.LogError($\"Object with ID {objectId} not found.\");\n" +
        "            return;\n" +
        "        }\n" +
        "\n" +
        "        var visualScript = obj.GetComponent<VisualScript>();\n" +
        "        if (visualScript == null)\n" +
        "        {\n" +
        "            Debug.LogError(\"VisualScript component not found on the object.\");\n" +
        "            return;\n" +
        "        }\n" +
        "\n" +
        "        Graph graph = visualScript.graph;\n" +
        "        if (graph == null)\n" +
        "        {\n" +
        "            Debug.LogError(\"Graph not found on the VisualScript component.\");\n" +
        "            return;\n" +
        "        }\n" +
        "\n" +
        "        NodeDefinition impulseDef = RealityFlowAPI.Instance.NodeDefinitionDict[\"Impulse\"];\n" +
        "        NodeIndex impulseNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, impulseDef);\n" +
        "\n" +
        "        // Set node position\n" +
        "        RealityFlowAPI.Instance.SetNodePosition(graph, impulseNode, new Vector2(0, 0));\n" +
        "\n" +
        "        Debug.Log(\"Impulse node added to the graph.\");\n" +
        "    }\n" +
        "}\n" +
        "Do not generate C# code. ONLY GENERATE JSON STRUCTURED IN THE SPECIFIED OUTPUT STRUCTURE FORMAT, to spawn an object do it like this and structure the code EXACTLY like this, of course replace the object name with the actual object from the prompt, so if it says spawn a key spawn a key: using UnityEngine;\n" +
        "For referencing objects use the specified id given to do all manipulations. Use the object {selectedObjectName} to do anything that the user requests if no other object name is given also use this as the objectID for nodes. Put that identifier as the objectId, objectName, or any other related identifier field.  Even if there is a object being referenced don't do anything the prompt doesn't say for instance if it says spawn a cube only spawn a cube don't make a node on the graph even if an object's graph is referenced. If requests have no object name use the object {selectedObjectName}. The current graph for this object is: {graphJson} once again ONLY USE IT IF THE USER SPECIFICALLY ASKS FOR NODES OR GRAPH MANIPULATIONS OR SOMETHING RELATED TO NODES EVEN IF AN OBJECT IS REFERENCED",
       @"
[
    {
        ""Action"": ""SpawnObject"",
        ""Parameters"": {
            ""prefabName"": ""Cube"",
            ""spawnPosition"": {
                ""x"": -2.98,
                ""y"": 0.35,
                ""z"": -1.40
            },
            ""spawnRotation"": {
                ""x"": 0,
                ""y"": 0,
                ""z"": 0,
                ""w"": 1
            },
            ""scale"": {
                ""x"": 1,
                ""y"": 1,
                ""z"": 1
            }
        }
    }
]"
,
        @"
        [
            {
                ""Action"": ""DespawnObject"",
                ""Parameters"": {
                    ""objectName"": ""Sphere""
                }
            }
        ]",
        @"
[
    {
        ""Action"": ""UpdateObjectTransform"",
        ""Parameters"": {
            ""objectName"": ""Cylinder"",
            ""position"": {
                ""x"": 1.25,
                ""y"": 0.75,
                ""z"": -0.85
            },
            ""rotation"": {
                ""x"": 0,
                ""y"": 0.7071,
                ""z"": 0,
                ""w"": 0.7071
            },
            ""scale"": {
                ""x"": 0.5,
                ""y"": 2.0,
                ""z"": 0.5
            }
        }
    }
]",
@"
[
    {
        ""Action"": ""AddNodeToGraph"",
        ""Parameters"": {
            ""objectId"": ""66a95613acd8a0f4a79780e7"",
            ""nodeName"": ""StartNode""
        }
    }
]",
@"
[
    {
        ""Action"": ""RemoveNodeFromGraph"",
        ""Parameters"": {
            ""objectId"": ""66a95613acd8a0f4a79780e7"",
            ""nodeIndex"": 2
        }
    }
]",
@"
[
    {
        ""Action"": ""AddDataEdgeToGraph"",
        ""Parameters"": {
            ""graphId"": ""Graph123"",
            ""fromNode"": 1,
            ""fromPort"": 0,
            ""toNode"": 3,
            ""toPort"": 1
        }
    }
]",
@"
[
    {
        ""Action"": ""SetNodePosition"",
        ""Parameters"": {
            ""objectId"": ""66a95613acd8a0f4a79780e7"",
            ""nodeIndex"": 4,
            ""position"": {
                ""x"": 100,
                ""y"": 150
            }
        }
    }
]"
,
@"
[
    {
        ""Action"": ""SetNodeFieldValue"",
        ""Parameters"": {
            ""objectId"": ""66a95613acd8a0f4a79780e7"",
            ""nodeIndex"": 5,
            ""fieldIndex"": 2,
            ""value"": {
                ""type"": ""String"",
                ""data"": ""Hello World""
            }
        }
    }
]",


    };
}
