using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Spawning;
using System.IO;
using System.Collections;
using Newtonsoft.Json;

public class ChatGPTTester : MonoBehaviour
{
    [SerializeField]
    private ChatGPTQuestion chatGPTQuestion;

    private string LLMPromptToBePassed;

    [SerializeField]
    private MRTKTMPInputField UserWhisperInput;

    [SerializeField]
    private ChatGPTResponse lastChatGPTResponseCache;

    [SerializeField]
    private bool immediateCompilation = false;

    public RaycastLogger raycastLogger;

    private static readonly Dictionary<string, string> apiFunctionDescriptions = new Dictionary<string, string>
    {
        { "SpawnObject", "Create an object(s): {0}" },
        { "DespawnObject", "Remove the object(s): {0}" },
        { "UpdateObjectTransform", "Move the object(s): {0}" },
        { "AddNodeToGraph", "Add a node(s) to the graph: {0}" },
        // Add more mappings as needed
    };

    public string ChatGPTMessage => lastChatGPTResponseCache?.Choices?.FirstOrDefault()?.Message?.content ?? string.Empty;

    private void Awake()
    {
    }

    public void Execute()
    {
        var originalReminders = chatGPTQuestion.reminders.ToArray();

        LLMPromptToBePassed = $"{chatGPTQuestion.promptPrefixConstant} {UserWhisperInput.text}";

        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        ReplacePromptPlaceholders();

        AddNodeDefinitionsToContext();

        AddPrefabNamesToContext();

        AddSelectedObjectToContext();

        AddGraphManipulationReminder();

        AppendRemindersToPrompt();

        StartCoroutine(SendRequestToChatGPT(originalReminders));
    }

    private void ReplacePromptPlaceholders()
    {
        foreach (var r in chatGPTQuestion.replacements)
        {
            LLMPromptToBePassed = LLMPromptToBePassed.Replace($"{{{r.replacementType}}}", r.value);
        }
    }

    private void AddNodeDefinitionsToContext()
    {
        var nodeDefinitionsJson = GetNodeDefinitionsJson();
        LLMPromptToBePassed += $"\n\nThe available node definition types are: {nodeDefinitionsJson}\n\n";
    }

    private void AddPrefabNamesToContext()
    {
        List<string> prefabNames = RealityFlowAPI.Instance.GetPrefabNames();
        if (prefabNames.Count > 0)
        {
            var reminderMessage = "\n-------------------------------------------------------------------------\n\n\nOnly use the following prefabs when spawning: " + string.Join(", ", prefabNames);
            Debug.Log("Only use the following prefabs when generating code, also if the object in the prompt is not present in the prefab list choose the closest applicable one: " + string.Join(", ", prefabNames));

            Vector3 indicatorPosition = raycastLogger.GetVisualIndicatorPosition();
            if (indicatorPosition != Vector3.zero)
            {
                reminderMessage += $"\n-------------------------------------------------------------------------\n\n\nUse the location {indicatorPosition} as the position data when you spawn any and all objects. Also unless specified otherwise use this location as the updateobjecttransform position";
                Debug.LogError("Visual Indicator Location: " + indicatorPosition);
            }

            AddTemporaryReminder(reminderMessage);
        }
    }

    private void AddSelectedObjectToContext()
    {
        string selectedObjectName = raycastLogger.GetSelectedObjectName();
        if (!string.IsNullOrEmpty(selectedObjectName))
        {
            GameObject selectedObject = GameObject.Find(selectedObjectName);
            if (selectedObject != null)
            {
                var visualScript = selectedObject.GetComponent<RealityFlow.NodeGraph.VisualScript>();
                if (visualScript != null && visualScript.graph != null)
                {
                    string graphJson = JsonUtility.ToJson(visualScript.graph);
                    var selectedObjectReminder = $"\n-------------------------------------------------------------------------\n\n\nVery important!! Use the object {selectedObjectName} to do anything that the user requests if no other object name is given also use this as the objectID for nodes. Even if there is a object being referenced don't do anything the prompt doesn't say for instance if it says spawn a cube only spawn a cube don't make a node on the graph even if an object's graph is referenced. If requests have no object name use the object {selectedObjectName}. The current graph for this object is: {graphJson} once again ONLY USE IT IF THE USER SPECIFICALLY ASKS FOR NODES OR GRAPH MANIPULATIONS OR SOMETHING RELATED TO NODES EVEN IF AN OBJECT IS REFERENCED";
                    Debug.Log($"Selected object: {selectedObjectName}, Graph: {graphJson}");
                    AddTemporaryReminder(selectedObjectReminder);
                }
            }
        }
    }

    private void AddGraphManipulationReminder()
    {
        var graphManipulationReminder = "\n-------------------------------------------------------------------------\n\n\n DO WHAT THE PROMPT SAYS, for example If it says to make a cube don't make nodes on the graph unless it says to. Ignore the following message unless specified: When making a node or manipulating a graph, only do it in the way you see here. Do not deviate from how this file is set up. Your responses should be structured very similarly to this example. Don't try to create a new graph, don't try to use JSON, don't try to update the database, do only what the user says.Do it like you see in this file: \n\n\n\n" +
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
                                "    };\n" +
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
                                "to spawn an object do it like this and structure the code exactly like this, of course replace the object name with the actual object from the prompt: using UnityEngine;\n" +
                               "using System.Collections;\n" +
                                "using Graph = RealityFlow.NodeGraph.Graph;\n" +
                                "using RealityFlow.NodeGraph;\n" +
                                "\n" +
                                "public class SpawnCube\n" +
                                "{\n" +
                                "    public static void Execute()\n" +
                                "    {\n" +
                                "        Vector3 spawnPosition = new Vector3(-0.24f, -0.05f, 1.82f);\n" +
                                "        RealityFlowAPI.Instance.SpawnObject(\"Cube\", spawnPosition, Vector3.one, Quaternion.identity, RealityFlowAPI.SpawnScope.Room);\n" +
                                "    }\n" +
                                "}";


        AddTemporaryReminder(graphManipulationReminder);
    }

    private void AppendRemindersToPrompt()
    {
        if (chatGPTQuestion.reminders.Length > 0)
        {
            LLMPromptToBePassed += $", {string.Join(',', chatGPTQuestion.reminders)}";
            Debug.Log("The complete reminders are: " + LLMPromptToBePassed);

#if UNITY_EDITOR
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "LLMPromptToBePassed.txt"), LLMPromptToBePassed);
#endif
        }
    }

    private IEnumerator SendRequestToChatGPT(string[] originalReminders)
    {
        yield return StartCoroutine(ChatGPTClient.Instance.Ask(LLMPromptToBePassed, response =>
        {
            lastChatGPTResponseCache = response;
            ChatGPTProgress.Instance.StopProgress();

            WriteResponseToFile(ChatGPTMessage);

            Debug.Log("Logging message in plain English");
            LogApiCalls(ChatGPTMessage);

            RealityFlowAPI.Instance?.actionLogger?.LogGeneratedCode(ChatGPTMessage);

            if (immediateCompilation)
            {
                StartCoroutine(ExecuteLoggedActionsCoroutine());
            }
        }));

        chatGPTQuestion.reminders = originalReminders;
    }

    private string GetNodeDefinitionsJson()
    {
        var nodeDefinitions = RealityFlowAPI.Instance.NodeDefinitionDict.Values.ToList();
        return JsonConvert.SerializeObject(nodeDefinitions.Select(nd => new
        {
            name = nd.Name,
            inputs = nd.Inputs.Select(i => i.Name).ToArray(),
            outputs = nd.Outputs.Select(o => o.Name).ToArray()
        }).ToList());
    }

    private void AddTemporaryReminder(string newReminder)
    {
        var remindersList = chatGPTQuestion.reminders.ToList();
        remindersList.Add(newReminder);
        chatGPTQuestion.reminders = remindersList.ToArray();
    }

    private void LogApiCalls(string generatedCode)
    {
        bool matched = false;
        foreach (var entry in apiFunctionDescriptions)
        {
            if (generatedCode.Contains(entry.Key))
            {
                string objectName = ExtractObjectName(generatedCode, entry.Key);
                string logMessage = string.Format(entry.Value, objectName);
                Logger.Instance.LogInfo(logMessage);
                matched = true;
            }
        }
        if (!matched)
        {
            Logger.Instance.LogInfo("Added an action.");
        }
    }

    private string ExtractObjectName(string code, string functionName)
    {
        int startIndex = code.IndexOf(functionName) + functionName.Length + 1;
        if (startIndex < functionName.Length + 1)
        {
            return "Unknown Object";
        }

        int endIndex = code.IndexOf(',', startIndex);
        if (endIndex == -1) endIndex = code.IndexOf(')', startIndex);
        if (startIndex >= 0 && endIndex > startIndex)
        {
            string parameter = code.Substring(startIndex, endIndex - startIndex).Trim(' ', '\'', '\"');

            if (parameter.StartsWith("prefabName"))
            {
                int nameStartIndex = code.IndexOf("string prefabName = ") + "string prefabName = ".Length;
                int nameEndIndex = code.IndexOf(';', nameStartIndex);
                if (nameStartIndex >= 0 && nameEndIndex > nameStartIndex)
                {
                    parameter = code.Substring(nameStartIndex, nameEndIndex - nameStartIndex).Trim(' ', '\'', '\"');
                }
            }
            return parameter;
        }
        return "Unknown Object";
    }

    public IEnumerator ExecuteLoggedActionsCoroutine()
    {
        if (RealityFlowAPI.Instance == null)
        {
            Debug.LogError("RealityFlowAPI.Instance is null.");
            yield break;
        }

        if (RealityFlowAPI.Instance.actionLogger == null)
        {
            Debug.LogError("RealityFlowAPI.Instance.actionLogger is null.");
            yield break;
        }

        yield return StartCoroutine(RealityFlowAPI.Instance.actionLogger.ExecuteLoggedCodeCoroutine());
    }

    public void ProcessAndCompileResponse()
    {
        RoslynCodeRunner.Instance.RunCodeCoroutine(ChatGPTMessage);
    }

    private void WriteResponseToFile(string response)
    {
        Debug.Log("Written to " + Application.persistentDataPath + "/ChatGPTResponse.cs");
        string localPath = Application.persistentDataPath + "/ChatGPTResponse.cs";
        //string externalPath = Path.Combine(Application.dataPath, "TestScript.cs");

        try
        {
#if UNITY_EDITOR
            File.WriteAllText(localPath, response);
            Debug.Log("Response written to file: " + localPath);

            //File.WriteAllText(externalPath, response);
            //Debug.Log("Response written to file: " + externalPath);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write response to file: " + e.Message);
        }
    }
}
