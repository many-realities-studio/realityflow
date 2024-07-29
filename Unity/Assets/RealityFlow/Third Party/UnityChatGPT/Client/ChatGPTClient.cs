using DilmerGames.Core.Singletons;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Samples.Whisper;
using System.Linq;
using System.Text;

public class ChatGPTClient : Singleton<ChatGPTClient>
{
    [SerializeField]
    private ChatGTPSettings chatGTPSettings;
    private Whisper whisper;
    private string logFilePath;

    private void Start()
    {
        whisper = FindObjectOfType<Whisper>();
        if (whisper == null)
        {
            Debug.LogError("Whisper instance not found in the scene.");
        }

        logFilePath = Path.Combine(Application.persistentDataPath, "ChatGPTLogs.txt");
    }

    public IEnumerator Ask(string prompt, Action<ChatGPTResponse> callBack)
    {
        DateTime startTime = DateTime.Now;
        yield return StartCoroutine(CaptureAndEncodeScreenshot());
        DateTime screenshotCapturedTime = DateTime.Now;

        string base64Image = EncodeScreenshotToBase64();
        DateTime imageEncodedTime = DateTime.Now;

        var url = chatGTPSettings.debug ? $"{chatGTPSettings.apiURL}?debug=true" : chatGTPSettings.apiURL;

        var requestParams = new ChatGPTRequest
        {
            Model = chatGTPSettings.apiModel,
            Messages = new ChatGPTChatMessage[]
            {
                new ChatGPTChatMessage
                {
                    role = "user",
                    content = prompt
                },
                new ChatGPTChatMessage
                {
                    role = "user",
                    content = "Here is an image for analysis, use it as context, however if you are unable to process it just process the prompt instead: data:image/png;base64," + base64Image
                }
            }
        };

        string requestBody = JsonConvert.SerializeObject(requestParams);
        Debug.Log("Request Payload: " + requestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.disposeDownloadHandlerOnDispose = true;
            request.disposeUploadHandlerOnDispose = true;
            request.disposeCertificateHandlerOnDispose = true;

            request.SetRequestHeader("Content-Type", "application/json");

            if (whisper != null)
            {
                string apiKey = whisper.GetCurrentApiKey();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                }
                else
                {
                    Debug.LogError("API key is null or empty.");
                }
            }
            else
            {
                Debug.LogError("Whisper reference is null.");
                yield break;
            }

            DateTime requestStartTime = DateTime.Now;
            yield return request.SendWebRequest();
            DateTime requestEndTime = DateTime.Now;

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Request error: " + request.error);
            }
            else
            {
                string responseInfo = request.downloadHandler.text;
                Debug.Log("API Response: " + responseInfo);

                var response = JsonConvert.DeserializeObject<ChatGPTResponse>(responseInfo);
                if (response != null && response.Choices != null && response.Choices.Any())
                {
                    response = response.CodeCleanUp();
                    response.ResponseTotalTime = (requestEndTime - requestStartTime).TotalMilliseconds;

                    LogDetails(startTime, screenshotCapturedTime, imageEncodedTime, requestStartTime, requestEndTime, response);
                    callBack(response);
                }
                else
                {
                    Debug.LogError("No choices found in the response or response is null.");
                }
            }
        }
    }

    private void LogDetails(DateTime startTime, DateTime screenshotCapturedTime, DateTime imageEncodedTime, DateTime requestStartTime, DateTime requestEndTime, ChatGPTResponse response)
    {
        string model = chatGTPSettings.apiModel;
        StringBuilder logBuilder = new StringBuilder();

        logBuilder.AppendLine($"Model: {model}");
        logBuilder.AppendLine($"Start Time: {startTime}");
        logBuilder.AppendLine($"Screenshot Captured Time: {screenshotCapturedTime} (Duration: {(screenshotCapturedTime - startTime).TotalMilliseconds} ms)");
        logBuilder.AppendLine($"Image Encoded Time: {imageEncodedTime} (Duration: {(imageEncodedTime - screenshotCapturedTime).TotalMilliseconds} ms)");
        logBuilder.AppendLine($"Request Start Time: {requestStartTime} (Duration: {(requestStartTime - imageEncodedTime).TotalMilliseconds} ms)");
        logBuilder.AppendLine($"Request End Time: {requestEndTime} (Duration: {(requestEndTime - requestStartTime).TotalMilliseconds} ms)");
        logBuilder.AppendLine($"Total Time: {(requestEndTime - startTime).TotalMilliseconds} ms");
        logBuilder.AppendLine($"Response Total Time: {response.ResponseTotalTime} ms");
        logBuilder.AppendLine($"Response: {response.Choices[0].Message.content}");
        logBuilder.AppendLine("----------------------------------------------------");

        File.AppendAllText(logFilePath, logBuilder.ToString());
    }

    private IEnumerator CaptureAndEncodeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenTexture.Apply();

        byte[] imageBytes = screenTexture.EncodeToJPG(20);
        encodedScreenshot = Convert.ToBase64String(imageBytes);

        Destroy(screenTexture);
    }

    private string EncodeScreenshotToBase64()
    {
        return encodedScreenshot;
    }

    private string encodedScreenshot;
}
