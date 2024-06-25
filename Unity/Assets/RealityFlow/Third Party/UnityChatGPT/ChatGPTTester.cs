using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Spawning;
using System.IO;
using System.Collections;

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
        { "SpawnObject", "Create an object: {0}" },
        { "DespawnObject", "Remove the object: {0}" },
        { "UpdateObjectTransform", "Move the object: {0}" },
        { "AddNodeToGraph", "Add a node to the graph: {0}" },
        // Add more mappings as needed
    };

    private void Awake()
    {
    }

    public void Execute()
    {
        LLMPromptToBePassed = $"{chatGPTQuestion.promptPrefixConstant} {UserWhisperInput.text}";

        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        Array.ForEach(chatGPTQuestion.replacements, r =>
        {
            LLMPromptToBePassed = LLMPromptToBePassed.Replace("{" + $"{r.replacementType}" + "}", r.value);
        });

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

            AddOrUpdateReminder(reminderMessage);
        }

        // Add reminder for selected object if it exists
        string selectedObjectName = raycastLogger.GetSelectedObjectName();
        if (!string.IsNullOrEmpty(selectedObjectName))
        {
            var selectedObjectReminder = $"\n-------------------------------------------------------------------------\n\n\nVery important!! Use the object {selectedObjectName} to do anything that the user requests if no other object name is given. If requests have no object name use the object {selectedObjectName} ";
            Debug.Log($"Selected object: {selectedObjectName}");
            AddOrUpdateReminder(selectedObjectReminder);
        }

        if (chatGPTQuestion.reminders.Length > 0)
        {
            LLMPromptToBePassed += $", {string.Join(',', chatGPTQuestion.reminders)}";
            Debug.Log("The complete reminders are: " + LLMPromptToBePassed);
        }

        StartCoroutine(ChatGPTClient.Instance.Ask(LLMPromptToBePassed, (response) =>
        {
            lastChatGPTResponseCache = response;
            ChatGPTProgress.Instance.StopProgress();

            WriteResponseToFile(ChatGPTMessage);
            //Log the API calls in plain English
            Debug.Log("Logging message in plain English");
            LogApiCalls(ChatGPTMessage);

            //If you want to see the code produced by ChatGPT uncomment out the line below
            //Logger.Instance.LogInfo(ChatGPTMessage);
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

        // Clear reminders after use
        chatGPTQuestion.reminders = chatGPTQuestion.reminders.Where(r => !IsTemporaryReminder(r)).ToArray();
    }

    private void AddOrUpdateReminder(string newReminder)
    {
        var remindersList = chatGPTQuestion.reminders.ToList();
        var existingReminderIndex = remindersList.FindIndex(r => r.Contains(newReminder.Split(':')[0]));

        if (existingReminderIndex != -1)
        {
            remindersList[existingReminderIndex] = newReminder;
        }
        else
        {
            remindersList.Add(newReminder);
        }

        chatGPTQuestion.reminders = remindersList.ToArray();
    }

    private bool IsTemporaryReminder(string reminder)
    {
        // Identify temporary reminders based on specific keywords or patterns
        return reminder.StartsWith("Only use the following prefabs") || reminder.StartsWith("Current spawned objects data");
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