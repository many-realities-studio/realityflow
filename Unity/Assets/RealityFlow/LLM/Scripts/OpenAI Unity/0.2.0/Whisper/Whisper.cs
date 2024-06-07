using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Ubiq.Voip;
using Ubiq;
using Microsoft.MixedReality.Toolkit.UX;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private PressableButton recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private MRTKTMPInputField message;
        [SerializeField] private MRTKTMPInputField apiKeyInputField;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private PressableButton submitButton;

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

            string apiKey = EnvConfigManager.Instance.OpenAIApiKey;
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = apiKeyInputField.text;
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                InitializeOpenAI(apiKey);
            }
            else
            {
                Debug.LogError("OpenAI API key is not set. Please provide it through the environment variable or the input field.");
            }

            RefreshMicrophoneList();

            recordButton.OnClicked.AddListener(ToggleRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            submitButton.OnClicked.AddListener(SubmitApiKey);

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
            Debug.Log("Refreshing microphone list...");
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
            dropdown.RefreshShownValue(); // Ensure the dropdown is updated
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        private void ToggleRecording()
        {
            if (isRecording)
            {
                EndRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            isRecording = true;
            recordButton.enabled = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");

            Debug.Log($"Starting recording with device: {dropdown.options[index].text}");
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);

            voipPeerConnectionManager?.MuteAll(); // Mute the VoIP microphone
        }

        private async void EndRecording()
        {
            isRecording = false;
            recordButton.enabled = true;

            message.text = "Transcripting...";

            Microphone.End(null);

            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };

            Debug.Log("Using API key: " + currentApiKey);
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            message.text = res.Text;

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
                    EndRecording();
                }
            }
        }
    }
}
