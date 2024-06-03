using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Ubiq.Voip;
using Ubiq;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private TMP_InputField message;
        [SerializeField] private TMP_InputField apiKeyInputField;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private Button submitButton; // Add reference to the Submit button

        private readonly string fileName = "output.wav";
        private readonly int duration = 5;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai;
        private VoipPeerConnectionManager voipPeerConnectionManager;
        private string currentApiKey;

        private void Start()
        {
            voipPeerConnectionManager = FindObjectOfType<VoipPeerConnectionManager>();

            // Initialize the OpenAI API if the environment variable is set
            string apiKey = EnvConfigManager.Instance.OpenAIApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                InitializeOpenAI(apiKey);
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            Debug.Log("WebGL platform detected, microphone not supported.");
#else
            RefreshMicrophoneList();
#endif

            recordButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            submitButton.onClick.AddListener(SubmitApiKey); // Add listener to Submit button

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
        }

        private void InitializeOpenAI(string apiKey)
        {
            currentApiKey = apiKey;
            openai = new OpenAIApi(apiKey);
            Debug.Log("OpenAI API initialized with provided key.");
        }

        private void SubmitApiKey()
        {
            string apiKey = apiKeyInputField.text;
            if (!string.IsNullOrEmpty(apiKey))
            {
                EnvConfigManager.Instance.UpdateApiKey(apiKey);
                InitializeOpenAI(apiKey);
                Debug.Log("API key submitted: " + apiKey);
            }
            else
            {
                Debug.LogError("API key is empty. Please enter a valid API key.");
            }
        }

        private void RefreshMicrophoneList()
        {
            dropdown.ClearOptions();
            List<string> devices = new List<string>(Microphone.devices);
            if (devices.Count == 0)
            {
                Debug.LogError("No microphone devices found.");
                dropdown.options.Add(new Dropdown.OptionData("No devices found"));
            }
            else
            {
                foreach (var device in devices)
                {
                    Debug.Log("Found device: " + device);
                    dropdown.options.Add(new Dropdown.OptionData(device));
                }
            }
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        private void StartRecording()
        {
            isRecording = true;
            recordButton.enabled = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");

#if !UNITY_WEBGL
            Debug.Log($"Starting recording with device: {dropdown.options[index].text}");
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
#endif

            voipPeerConnectionManager?.MuteAll(); // Mute the VoIP microphone
        }

        private async void EndRecording()
        {
            message.text = "Transcripting...";

#if !UNITY_WEBGL
            Microphone.End(null);
#endif

            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };

            Debug.Log("Using API key: " + currentApiKey); // Add this log to confirm the API key being used
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            message.text = res.Text;
            recordButton.enabled = true;

            Debug.Log("Unmuting all microphones after Whisper recording.");
            voipPeerConnectionManager?.UnmuteAll(); // Unmute the VoIP microphone
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progressBar.fillAmount = time / duration;

                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }
    }
}
