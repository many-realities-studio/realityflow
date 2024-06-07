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

    private void Awake()
    {
        responseTimeText.text = string.Empty;
        SetButtonEnabled(compilerButton, false);

        askButton.OnClicked.AddListener(() =>
        {
            SetButtonEnabled(compilerButton, false);
            CompileButtonColor = Color.white;

            Execute();
        });

        compilerButton.OnClicked.AddListener(() =>
        {
            ProcessAndCompileResponse();
        });
    }

    public void Execute()
    {
        gptPrompt = $"{chatGPTQuestion.promptPrefixConstant} {promptText.text}";

        scenarioTitleText.text = chatGPTQuestion.scenarioTitle;

        SetButtonEnabled(askButton, false);

        ChatGPTProgress.Instance.StartProgress("Generating source code, please wait");

        // Handle replacements
        Array.ForEach(chatGPTQuestion.replacements, r =>
        {
            gptPrompt = gptPrompt.Replace("{" + $"{r.replacementType}" + "}", r.value);
        });

        // Add prefabs to reminders using RealityFlowAPI
        List<string> prefabNames = RealityFlowAPI.Instance.GetPrefabNames();
        if (prefabNames.Count > 0)
        {
            var reminderMessage = "Only use the following prefabs when spawning: " + string.Join(", ", prefabNames);
            chatGPTQuestion.reminders = chatGPTQuestion.reminders.Concat(new[] { reminderMessage }).ToArray();
        }

        // Handle reminders
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

            Logger.Instance.LogInfo(ChatGPTMessage);

            if (immediateCompilation)
            {
                ProcessAndCompileResponse();
            }
        }));
    }

    public void ProcessAndCompileResponse()
    {
        RoslynCodeRunner.Instance.RunCode(ChatGPTMessage);
    }

    private void SetButtonEnabled(PressableButton button, bool isEnabled)
    {
        button.enabled = isEnabled;
    }
}
