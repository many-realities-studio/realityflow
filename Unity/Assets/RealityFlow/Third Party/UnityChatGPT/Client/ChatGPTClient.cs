using DilmerGames.Core.Singletons;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ChatGPTClient : Singleton<ChatGPTClient>
{
    [SerializeField]
    private ChatGTPSettings chatGTPSettings;

    private List<ChatGPTChatMessage> conversationHistory = new List<ChatGPTChatMessage>();
    private string screenshotPath;
    public bool IsScreenshotReady { get; private set; }
    private string base64Screenshot;

    // Method to capture and encode screenshot
    public void CaptureScreenshot()
    {
        IsScreenshotReady = false;
        screenshotPath = Path.Combine(Application.persistentDataPath, "screenshot.png");
        Debug.Log("CaptureScreenshot called. Path: " + screenshotPath);
        ScreenCapture.CaptureScreenshot(screenshotPath);
        Debug.Log("Screenshot capture requested: " + screenshotPath);
        StartCoroutine(WaitForScreenshot());
    }

    private IEnumerator WaitForScreenshot()
    {
        Debug.Log("WaitForScreenshot coroutine started.");
        yield return new WaitForEndOfFrame(); // Ensure the screenshot is fully captured
        Debug.Log("End of frame reached, attempting to load and encode screenshot...");
        LoadAndEncodeScreenshot();
    }

    private void LoadAndEncodeScreenshot()
    {
        Debug.Log("LoadAndEncodeScreenshot called. Path: " + screenshotPath);
        if (File.Exists(screenshotPath))
        {
            Debug.Log("Screenshot file found at: " + screenshotPath);
            byte[] screenshotBytes = File.ReadAllBytes(screenshotPath);
            base64Screenshot = Convert.ToBase64String(screenshotBytes);
            Debug.Log("Screenshot encoded to Base64.");
            IsScreenshotReady = true;
        }
        else
        {
            Debug.LogError("Screenshot file not found: " + screenshotPath);
            IsScreenshotReady = true; // Set to true to prevent hanging in case of error
        }
    }

    public IEnumerator Ask(string prompt, Action<ChatGPTResponse> callBack)
    {
        yield return AskInternal(prompt, base64Screenshot, callBack);
    }

    private IEnumerator AskInternal(string prompt, string base64Screenshot, Action<ChatGPTResponse> callBack)
    {
        var url = chatGTPSettings.debug ? $"{chatGTPSettings.apiURL}?debug=true" : chatGTPSettings.apiURL;

        // Add the user message to the conversation history
        conversationHistory.Add(new ChatGPTChatMessage
        {
            role = "user",
            content = prompt
        });

        // Optionally add the screenshot context
        if (!string.IsNullOrEmpty(base64Screenshot))
        {
            conversationHistory.Add(new ChatGPTChatMessage
            {
                role = "system",
                content = base64Screenshot
            });
        }

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            var requestParams = JsonConvert.SerializeObject(new ChatGPTRequest
            {
                Model = chatGTPSettings.apiModel,
                Messages = conversationHistory.ToArray() // Pass the conversation history
            });

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestParams);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;
            request.disposeCertificateHandlerOnDispose = true;

            request.SetRequestHeader("Content-Type", "application/json");

            // Retrieve the API key from the environment manager
            string apiKey = EnvConfigManager.Instance.OpenAIApiKey;
            string organization = EnvConfigManager.Instance.OpenAIOrganization;

            // Set the request headers
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
            else
            {
                Debug.LogError("API key is null or empty.");
            }

            if (!string.IsNullOrEmpty(organization))
            {
                request.SetRequestHeader("OpenAI-Organization", organization);
            }
            else
            {
                Debug.LogWarning("OpenAI-Organization is null or empty.");
            }

            var requestStartDateTime = DateTime.Now;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Request error: " + request.error);
            }
            else
            {
                string responseInfo = request.downloadHandler.text;
                var response = JsonConvert.DeserializeObject<ChatGPTResponse>(responseInfo)
                    .CodeCleanUp();

                response.ResponseTotalTime = (DateTime.Now - requestStartDateTime).TotalMilliseconds;

                // Add the assistant message to the conversation history
                conversationHistory.Add(new ChatGPTChatMessage
                {
                    role = "assistant",
                    content = response.Choices[0].Message.content
                });

                callBack(response);
            }
        }
    }
}
