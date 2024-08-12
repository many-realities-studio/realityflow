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
using RealityFlow.Collections;
using RealityFlow.NodeGraph;
using RealityFlow.NodeUI;
using RealityFlow.Scripting;

public class StructuredAction
{
    public string Action { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
}

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
    };

    // New queue to store ChatGPT messages
    private Queue<string> chatGPTMessageQueue = new Queue<string>();

    public string ChatGPTMessage => lastChatGPTResponseCache?.Choices?.FirstOrDefault()?.Message?.content ?? string.Empty;

    private void Awake()
    {
    }

    public void Execute()
    {
        var originalReminders = chatGPTQuestion.reminders.ToArray();

        LLMPromptToBePassed = $"{chatGPTQuestion.promptPrefixConstant} {UserWhisperInput.text}";

        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        LoadReminders(); // Load reminders from the Reminders class
        ReplacePromptPlaceholders();
        AddNodeDefinitionsToContext();
        AddPrefabNamesToContext();
        AddSelectedObjectToContext();
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
                reminderMessage += $"\n-------------------------------------------------------------------------\n\n\nUse the visualIndicator location {indicatorPosition} as the position data when you spawn any and all objects. Also unless specified otherwise use this location as the updateobjecttransform position";
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
                Vector3 objectPosition = selectedObject.transform.position;
                string positionString = $"({objectPosition.x}, {objectPosition.y}, {objectPosition.z})";
                if (visualScript != null)
                {
                    string graphJson = null;
                    if (visualScript.graph != null)
                        graphJson = JsonUtility.ToJson(visualScript.graph);
                    var selectedObjectReminder = $"\n-------------------------------------------------------------------------\n\n\nVery important!! Use the object {selectedObjectName} to do anything that the user requests if no other object name is given also use this as the objectID for nodes. Put that identifier as the objectId, objectName, or any other related identifier field.  Even if there is a object being referenced don't do anything the prompt doesn't say for instance if it says spawn a cube only spawn a cube don't make a node on the graph even if an object's graph is referenced. If requests have no object name use the object {selectedObjectName}. The current graph for this object is: {graphJson} once again ONLY USE IT IF THE USER SPECIFICALLY ASKS FOR NODES OR GRAPH MANIPULATIONS OR SOMETHING RELATED TO NODES EVEN IF AN OBJECT IS REFERENCED also this is the object's current location: {positionString} don't move the object to its current location unless specified by visualIndicatorLocation ";
                    Debug.Log($"Selected object: {selectedObjectName}, Graph: {graphJson}");
                    AddTemporaryReminder(selectedObjectReminder);
                }
            }
        }
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

    private void LoadReminders()
    {
        foreach (var reminder in Reminders.reminderTexts)
        {
            AddTemporaryReminder(reminder);
        }
    }

    private void AddTemporaryReminder(string newReminder)
    {
        var remindersList = chatGPTQuestion.reminders.ToList();
        remindersList.Add(newReminder);
        chatGPTQuestion.reminders = remindersList.ToArray();
    }

    private IEnumerator SendRequestToChatGPT(string[] originalReminders)
    {
        yield return StartCoroutine(ChatGPTClient.Instance.Ask(LLMPromptToBePassed, response =>
         {
             lastChatGPTResponseCache = response;
             ChatGPTProgress.Instance.StopProgress();

             // Enqueue the ChatGPT message instead of processing it immediately
             chatGPTMessageQueue.Enqueue(ChatGPTMessage);
             WriteResponseToFile(ChatGPTMessage);
             Debug.Log("ChatGPT message added to queue.");

             // Optionally start processing the queue immediately if needed
             if (immediateCompilation)
             {
                 StartCoroutine(ProcessAndExecuteResponses());
             }
         }));

        chatGPTQuestion.reminders = originalReminders;
    }

    // New method to process the queue and execute responses
    public IEnumerator ProcessAndExecuteResponses()
    {
        while (chatGPTMessageQueue.Count > 0)
        {
            string messageToProcess = chatGPTMessageQueue.Dequeue();
            Debug.Log("Processing message from queue.");
            ProcessAndExecuteResponse(messageToProcess);

            // Yield to ensure other coroutines and Unity engine have time to process
            yield return null;
        }
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

        try
        {
#if UNITY_EDITOR
            File.WriteAllText(localPath, response);
            Debug.Log("Response written to file: " + localPath);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write response to file: " + e.Message);
        }
    }

    private void ProcessAndExecuteResponse(string jsonResponse)
    {
        try
        {
            // Clean up the JSON response by removing markdown code block formatting
            if (jsonResponse.Contains("```"))
            {
                int startIndex = jsonResponse.IndexOf('['); // Change to '[' for list
                int endIndex = jsonResponse.LastIndexOf(']');
                jsonResponse = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
            }

            // Deserialize the cleaned JSON response into a list of StructuredAction objects
            var structuredResponses = JsonConvert.DeserializeObject<List<StructuredAction>>(jsonResponse);

            // Process each action in the list
            foreach (var action in structuredResponses)
            {
                ExecuteAction(action);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process and execute the response: {ex.Message}");
        }
    }


    private void ExecuteAction(StructuredAction action)
    {
        switch (action.Action)
        {
            case "SpawnObject":
                string prefabName = action.Parameters["prefabName"].ToString();
                var position = JsonConvert.DeserializeObject<Vector3>(action.Parameters["spawnPosition"].ToString());
                var rotation = JsonConvert.DeserializeObject<Quaternion>(action.Parameters["spawnRotation"].ToString());
                var scale = JsonConvert.DeserializeObject<Vector3>(action.Parameters["scale"].ToString());

                RealityFlowAPI.Instance.SpawnObject(prefabName, position, scale, rotation, RealityFlowAPI.SpawnScope.Room);
                break;

            case "DespawnObject":
                string objectNameToDespawn = action.Parameters["objectName"].ToString();
                GameObject objectToDespawn = GameObject.Find(objectNameToDespawn);
                if (objectToDespawn != null)
                {
                    RealityFlowAPI.Instance.DespawnObject(objectToDespawn);
                }
                break;

            case "UpdateObjectTransform":
                string objectNameToUpdate = action.Parameters["objectName"].ToString();
                var newPosition = JsonConvert.DeserializeObject<Vector3>(action.Parameters["position"].ToString());
                var newRotation = JsonConvert.DeserializeObject<Quaternion>(action.Parameters["rotation"].ToString());
                var newScale = JsonConvert.DeserializeObject<Vector3>(action.Parameters["scale"].ToString());

                RealityFlowAPI.Instance.UpdateObjectTransform(objectNameToUpdate, newPosition, newRotation, newScale);
                break;

            case "AddNodeToGraph":
                string objectId = action.Parameters["objectId"].ToString();
                GameObject obj = GameObject.Find(objectId);

                if (obj == null)
                {
                    Debug.LogError($"Object with ID {objectId} not found.");
                    return;
                }

                var visualScript = obj.GetComponent<VisualScript>();
                if (visualScript == null)
                {
                    Debug.LogError("VisualScript component not found on the object.");
                    return;
                }

                Graph graph = visualScript.graph;
                if (graph == null)
                {
                    Debug.LogError("Graph not found on the VisualScript component.");
                    return;
                }

                string nodeName = action.Parameters["nodeName"].ToString();
                NodeDefinition nodeDef = RealityFlowAPI.Instance.NodeDefinitionDict[nodeName];

                NodeIndex newNode = RealityFlowAPI.Instance.AddNodeToGraph(graph, nodeDef);
                if (newNode != null)
                {
                    Debug.Log($"{nodeName} node added to the graph.");
                }
                break;
        }
    }

}
