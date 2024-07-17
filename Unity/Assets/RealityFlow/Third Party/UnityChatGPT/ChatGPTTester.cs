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

    public string ChatGPTMessage
    {
        get
        {
            return (lastChatGPTResponseCache.Choices.FirstOrDefault()?.Message?.content ?? null) ?? string.Empty;
        }
    }

    private static readonly Dictionary<string, string> apiFunctionDescriptions = new Dictionary<string, string>
    {
        { "SpawnObject", "Create an object(s): {0}" },
        { "DespawnObject", "Remove the object(s): {0}" },
        { "UpdateObjectTransform", "Move the object(s): {0}" },
        { "AddNodeToGraph", "Add a node(s) to the graph: {0}" },
        // Add more mappings as needed
    };

    private void Awake()
    {
    }

    public void Execute()
    {
        // Store original reminders list
        var originalReminders = chatGPTQuestion.reminders.ToArray();

        LLMPromptToBePassed = $"{chatGPTQuestion.promptPrefixConstant} {UserWhisperInput.text}";

        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        Array.ForEach(chatGPTQuestion.replacements, r =>
        {
            LLMPromptToBePassed = LLMPromptToBePassed.Replace("{" + $"{r.replacementType}" + "}", r.value);
        });

        // Fetch and add node definitions to the context
        var nodeDefinitionsJson = GetNodeDefinitionsJson();
        var nodeDefinitionsContext = $"The available node definition types are: {nodeDefinitionsJson} \n\n\n\n";
        LLMPromptToBePassed += $"\n\n{nodeDefinitionsContext}";

        List<string> prefabNames = RealityFlowAPI.Instance.GetPrefabNames();
        if (prefabNames.Count > 0)
        {
            var reminderMessage = "\n-------------------------------------------------------------------------\n\n\nOnly use the following prefabs when spawning: " + string.Join(", ", prefabNames);
            Debug.Log("Only use the following prefabs when generating code: " + string.Join(", ", prefabNames));

            Vector3 indicatorPosition = raycastLogger.GetVisualIndicatorPosition();
            if (indicatorPosition != Vector3.zero)
            {
                reminderMessage += $"\n-------------------------------------------------------------------------\n\n\nUse the location {indicatorPosition} as the position data when you spawn any and all objects. Also unless specified otherwise use this location as the updateobjecttransform position";
                Debug.Log("Visual Indicator Location: " + indicatorPosition);
            }

            AddTemporaryReminder(reminderMessage);
        }

        // Add reminder for selected object if it exists
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
                    var selectedObjectReminder = $"\n-------------------------------------------------------------------------\n\n\nVery important!! Use the object {selectedObjectName} to do anything that the user requests if no other object name is given also use this as the objectID for nodes. If requests have no object name use the object {selectedObjectName}. The current graph for this object is: {graphJson}";
                    Debug.Log($"Selected object: {selectedObjectName}, Graph: {graphJson}");
                    AddTemporaryReminder(selectedObjectReminder);
                }
            }
        }
        // Add specific reminder about graph manipulation
        var graphManipulationReminder = "\n-------------------------------------------------------------------------\n\n\nWhen making a node or manipulating a graph, only do it like this. Do not deviate from how this file is set up at all. Don't try to create a new graph, don't try to use JSON, don't try to update the database. Do it like you see in this file, nodes should be organized rectangularly and use 100.0.0 spacing if the number of nodes being created is less than 10, if its more then 10 spacing shoud be used: \n\n\n\n" +
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
                                        "}";
        AddTemporaryReminder(graphManipulationReminder);

        // Add existing reminders only once
        if (chatGPTQuestion.reminders.Length > 0)
        {
            LLMPromptToBePassed += $", {string.Join(',', chatGPTQuestion.reminders)}";
            Debug.Log("The complete reminders are: " + LLMPromptToBePassed);

            // Save the complete list of reminders to a file
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "LLMPromptToBePassed.txt"), LLMPromptToBePassed);
        }

        StartCoroutine(ChatGPTClient.Instance.Ask(LLMPromptToBePassed, (response) =>
        {
            lastChatGPTResponseCache = response;
            ChatGPTProgress.Instance.StopProgress();

            WriteResponseToFile(ChatGPTMessage);
            // Log the API calls in plain English
            Debug.Log("Logging message in plain English");
            LogApiCalls(ChatGPTMessage);

            // If you want to see the code produced by ChatGPT uncomment out the line below
            // Logger.Instance.LogInfo(ChatGPTMessage);
            if (RealityFlowAPI.Instance == null)
            {
                Debug.LogError("RealityFlowAPI.Instance is null.");
                return;
            }

            if (RealityFlowAPI.Instance.actionLogger == null)
            {
                Debug.LogError("RealityFlowAPI.Instance.actionLogger is null.");
                return;
            }

            // Log the generated code instead of executing it immediately
            RealityFlowAPI.Instance.actionLogger.LogGeneratedCode(ChatGPTMessage);

            if (immediateCompilation)
            {
                ExecuteLoggedActionsCoroutine();
            }
        }));

        // Restore the original reminders list
        chatGPTQuestion.reminders = originalReminders;
    }

    private string GetNodeDefinitionsJson()
    {
        var nodeDefinitions = RealityFlowAPI.Instance.NodeDefinitionDict.Values.ToList();
        var nodeDefinitionsJson = JsonConvert.SerializeObject(nodeDefinitions.Select(nd => new
        {
            name = nd.Name,
            inputs = nd.Inputs.Select(i => i.Name).ToArray(),
            outputs = nd.Outputs.Select(o => o.Name).ToArray()
        }).ToList());
        return nodeDefinitionsJson;
    }

    private void AddTemporaryReminder(string newReminder)
    {
        var remindersList = chatGPTQuestion.reminders.ToList();
        remindersList.Add(newReminder);
        chatGPTQuestion.reminders = remindersList.ToArray();
    }

    private bool IsTemporaryReminder(string reminder)
    {
        // Identify temporary reminders based on specific keywords or patterns
        return reminder.StartsWith("Only use the following prefabs") || reminder.StartsWith("Current spawned objects data") ||
               reminder.StartsWith("Use the location");
    }

    private void LogApiCalls(string generatedCode)
    {
        foreach (var entry in apiFunctionDescriptions)
        {
            if (generatedCode.Contains(entry.Key))
            {
                string objectName = ExtractObjectName(generatedCode, entry.Key);
                string logMessage = string.Format(entry.Value, objectName);
                Logger.Instance.LogInfo(logMessage);
            }
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

            // Check if the parameter is a variable name and resolve it if necessary
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

        // Execute all logged actions (code snippets) sequentially
        yield return StartCoroutine(RealityFlowAPI.Instance.actionLogger.ExecuteLoggedCodeCoroutine());
    }

    public void ProcessAndCompileResponse()
    {
        RoslynCodeRunner.Instance.RunCodeCoroutine(ChatGPTMessage);
    }

    private void WriteResponseToFile(string response)
    {
        Debug.Log("Written to " + Application.persistentDataPath + "/ChatGPTResponse.cs");
        string path = Application.persistentDataPath + "/ChatGPTResponse.cs";
        try
        {
            File.WriteAllText(path, response);
            Debug.Log("Response written to file: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write response to file: " + e.Message);
        }
    }
}
