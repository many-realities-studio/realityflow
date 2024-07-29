using System.Linq;
using DilmerGames.Core.Singletons;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Samples.Whisper;
public static class ChatGPTExtensions
{
    public const string KEYWORD_USING = "using UnityEngine";
    public const string KEYWORD_PUBLIC_CLASS = "public class";
    public static readonly string[] filters = { "C#", "c#", "csharp", "CSHARP" };

    public static ChatGPTResponse CodeCleanUp(this ChatGPTResponse chatGPTResponse)
    {
        if (chatGPTResponse?.Choices == null || !chatGPTResponse.Choices.Any())
        {
            Debug.LogError("No choices found in the response.");
            return chatGPTResponse;
        }

        var messageContent = chatGPTResponse.Choices.FirstOrDefault()?.Message?.content;
        if (string.IsNullOrEmpty(messageContent))
        {
            Debug.LogError("Message content is null or empty.");
            return chatGPTResponse;
        }

        // Apply filters
        filters.ToList().ForEach(f =>
        {
            messageContent = messageContent.Replace(f, string.Empty);
        });

        // Split due to explanations
        var codeLines = messageContent.Split("```");

        // Extract code
        messageContent = codeLines.FirstOrDefault(c => c.Contains(KEYWORD_USING) ||
            c.Contains(KEYWORD_PUBLIC_CLASS))?.Trim();

        if (messageContent == null)
        {
            Debug.LogError("No valid code block found in the message content.");
            return chatGPTResponse;
        }

        // Update the response content
        chatGPTResponse.Choices[0].Message.content = messageContent;

        return chatGPTResponse;
    }
}
