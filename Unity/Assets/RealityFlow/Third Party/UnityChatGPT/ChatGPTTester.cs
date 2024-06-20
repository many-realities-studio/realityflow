using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Spawning;
using System.IO;

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
        { "UpdateObjectTransform", "Update the transform of object: {0}" },
        { "AddNodeToGraph", "Add a node to the graph: {0}" },
        // Add more mappings as needed
    };

    private void Awake()
    {

    }

    public void Execute()
    {
        LLMPromptToBePassed

 = $"{chatGPTQuestion.promptPrefixConstant} {UserWhisperInput.text}";


        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        Array.ForEach(chatGPTQuestion.replacements, r =>
        {
            LLMPromptToBePassed

     = LLMPromptToBePassed

    .Replace("{" + $"{r.replacementType}" + "}", r.value);
        });

        List<string> prefabNames = RealityFlowAPI.Instance.GetPrefabNames();
        if (prefabNames.Count > 0)
        {
            var reminderMessage = "\n-------------------------------------------------------------------------\n\n\nOnly use the following prefabs when spawning: " + string.Join(", ", prefabNames);
            Debug.Log("Only use the following prefabs when generating code: " + string.Join(", ", prefabNames));
            AddOrUpdateReminder(reminderMessage);
        }

        //string spawnedObjectsData = RealityFlowAPI.Instance.ExportSpawnedObjectsData();
        //if (!string.IsNullOrEmpty(spawnedObjectsData))
        //{
        //  var reminderMessage = "\n --------------------------------------------------------------\n\n\nCurrent spawned objects data: " + spawnedObjectsData;
        //Debug.Log("Current spawned objects data: " + spawnedObjectsData);
        //AddOrUpdateReminder(reminderMessage);
        //}

        if (chatGPTQuestion.reminders.Length > 0)
        {
            LLMPromptToBePassed

     += $", {string.Join(',', chatGPTQuestion.reminders)}";
            Debug.Log("The complete reminders are: " + LLMPromptToBePassed

    );
        }

        StartCoroutine(ChatGPTClient.Instance.Ask(LLMPromptToBePassed

, (response) =>
        {
            lastChatGPTResponseCache = response;
            ChatGPTProgress.Instance.StopProgress();

            WriteResponseToFile(ChatGPTMessage);
            // Log the API calls in plain English
            LogApiCalls(ChatGPTMessage);
            //Logger.Instance.LogInfo(ChatGPTMessage);

            // Log the generated code instead of executing it immediately
            RealityFlowAPI.Instance.actionLogger.LogGeneratedCode(ChatGPTMessage);

            if (immediateCompilation)
            {
                ExecuteLoggedActions();
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
        // This method should extract the object name or relevant parameter from the generated code
        // You can implement this based on the expected structure of the generated code
        // For example, if the generated code is like "SpawnObject('Ladder', ...)", you can extract 'Ladder'

        // Here's a simple example assuming the object name is always the first parameter
        int startIndex = code.IndexOf(functionName) + functionName.Length + 1;
        int endIndex = code.IndexOf(',', startIndex);
        if (endIndex == -1) endIndex = code.IndexOf(')', startIndex);
        if (startIndex >= 0 && endIndex > startIndex)
        {
            return code.Substring(startIndex, endIndex - startIndex).Trim(' ', '\'', '\"');
        }
        return "Unknown Object";
    }

    public void ExecuteLoggedActions()
    {
        // Execute all logged actions (code snippets) sequentially
        RealityFlowAPI.Instance.actionLogger.ExecuteLoggedCode();
    }

    public void ProcessAndCompileResponse()
    {
        RoslynCodeRunner.Instance.RunCode(ChatGPTMessage);
    }

    private void WriteResponseToFile(string response)
    {
        string path = Application.persistentDataPath + "/ChatGPTResponse.json";
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
