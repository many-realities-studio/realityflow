using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using Ubiq.Spawning;

public class ChatGPTTester : MonoBehaviour
{
    [SerializeField]
    private PressableButton askButton;

    [SerializeField]
    private PressableButton compilerButton;

    [SerializeField]
    private TextMeshProUGUI responseTimeText;

    [SerializeField]
    private TextMeshProUGUI chatGPTAnswer;

    [SerializeField]
    private TextMeshProUGUI chatGPTQuestionText;

    [SerializeField]
    private ChatGPTQuestion chatGPTQuestion;

    private string gptPrompt;

    [SerializeField]
    private TextMeshProUGUI scenarioTitleText;

    [SerializeField]
    private MRTKTMPInputField promptText;

    [SerializeField]
    private MRTKTMPInputField scenarioQuestionText;

    [SerializeField]
    private ChatGPTResponse lastChatGPTResponseCache;

    [SerializeField]
    private bool immediateCompilation = false;

    [SerializeField]
    private GameObject whisperCanvasHolder;

    [SerializeField]
    private PressableButton whisperToggleButton;

    [SerializeField]
    private PressableButton undoButton;

    [SerializeField]
    private TextMeshProUGUI progressText;

    public string ChatGPTMessage
    {
        get
        {
            return (lastChatGPTResponseCache.Choices.FirstOrDefault()?.Message?.content ?? null) ?? string.Empty;
        }
    }

    public Color CompileButtonColor
    {
        set
        {
            var renderer = compilerButton.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = value;
            }
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
        progressText.text = "RealityGPT";
        responseTimeText.text = string.Empty;
        SetButtonEnabled(compilerButton, false);
        SetButtonEnabled(undoButton, false);

        askButton.OnClicked.AddListener(() =>
        {
            SetButtonEnabled(compilerButton, false);
            CompileButtonColor = Color.white;

            Execute();
        });

        compilerButton.OnClicked.AddListener(() =>
        {
            ExecuteLoggedActions();
            SetButtonEnabled(undoButton, true); // Enable Undo button after compiling
        });

        whisperToggleButton.OnClicked.AddListener(() =>
        {
            ToggleWhisperCanvas();
        });

        undoButton.OnClicked.AddListener(() =>
        {
            RealityFlowAPI.Instance.UndoLastAction();
            CheckUndoButtonState();
        });
    }

    public void Execute()
    {
        gptPrompt = $"{chatGPTQuestion.promptPrefixConstant} {promptText.text}";

        scenarioTitleText.text = chatGPTQuestion.scenarioTitle;

        SetButtonEnabled(askButton, false);

        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        Array.ForEach(chatGPTQuestion.replacements, r =>
        {
            gptPrompt = gptPrompt.Replace("{" + $"{r.replacementType}" + "}", r.value);
        });

        List<string> prefabNames = RealityFlowAPI.Instance.GetPrefabNames();
        if (prefabNames.Count > 0)
        {
            var reminderMessage = "Only use the following prefabs when spawning: " + string.Join(", ", prefabNames);
            chatGPTQuestion.reminders = chatGPTQuestion.reminders.Concat(new[] { reminderMessage }).ToArray();
        }

        string spawnedObjectsData = RealityFlowAPI.Instance.ExportSpawnedObjectsData();
        if (!string.IsNullOrEmpty(spawnedObjectsData))
        {
            var reminderMessage = "Current spawned objects data: " + spawnedObjectsData;
            chatGPTQuestion.reminders = chatGPTQuestion.reminders.Concat(new[] { reminderMessage }).ToArray();
        }

        if (chatGPTQuestion.reminders.Length > 0)
        {
            gptPrompt += $", {string.Join(',', chatGPTQuestion.reminders)}";
        }

        scenarioQuestionText.text = gptPrompt;

        StartCoroutine(ChatGPTClient.Instance.Ask(gptPrompt, (response) =>
        {
            SetButtonEnabled(askButton, true);

            CompileButtonColor = Color.blue;

            SetButtonEnabled(compilerButton, true);
            lastChatGPTResponseCache = response;
            responseTimeText.text = $"Time: {response.ResponseTotalTime} ms";

            ChatGPTProgress.Instance.StopProgress();

            // Log the API calls in plain English
            LogApiCalls(ChatGPTMessage);

            // Log the generated code instead of executing it immediately
            RealityFlowAPI.Instance.actionLogger.LogGeneratedCode(ChatGPTMessage);

            if (immediateCompilation)
            {
                ExecuteLoggedActions();
                SetButtonEnabled(undoButton, true); // Enable Undo button after compiling
            }
        }));
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

    private void SetButtonEnabled(PressableButton button, bool isEnabled)
    {
        button.enabled = isEnabled;
    }

    private void ToggleWhisperCanvas()
    {
        bool isActive = whisperCanvasHolder.activeSelf;
        whisperCanvasHolder.SetActive(!isActive);

        if (!isActive)
        {
            progressText.text = "In the Whisper menu";
        }
        else
        {
            progressText.text = "RealityGPT";
        }

        responseTimeText.gameObject.SetActive(isActive);
        chatGPTAnswer.gameObject.SetActive(isActive);
        chatGPTQuestionText.gameObject.SetActive(isActive);
        promptText.gameObject.SetActive(isActive);
        scenarioQuestionText.gameObject.SetActive(isActive);
        scenarioTitleText.gameObject.SetActive(isActive);
    }

    private void CheckUndoButtonState()
    {
        SetButtonEnabled(undoButton, RealityFlowAPI.Instance.actionLogger.GetActionStackCount() > 0);
    }
}
