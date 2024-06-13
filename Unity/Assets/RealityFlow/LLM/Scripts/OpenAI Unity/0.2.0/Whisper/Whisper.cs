using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Ubiq.Voip;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private MRTKTMPInputField message;
        [SerializeField] private MRTKTMPInputField apiKeyInputField;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button muteButton;

        private readonly string fileName = "output.wav";
        private readonly int duration = 5;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai;
        private MuteManager muteManager;
        private string currentApiKey;

        private void Start()
        {
            recordButton.onClick.AddListener(ToggleRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            submitButton.onClick.AddListener(SubmitApiKey);
            muteButton.onClick.AddListener(() => muteManager?.ToggleMute());

            RefreshMicrophoneList();

            muteManager = FindObjectOfType<MuteManager>(); // Initialize MuteManager
            if (muteManager == null)
            {
                Debug.LogError("MuteManager not found in the scene.");
            }

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

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
        }

        private void InitializeOpenAI(string apiKey)
        {
            currentApiKey = apiKey;
            openai = new OpenAIApi(apiKey);
            Debug.Log("OpenAI API initialized with provided key.");
        }

        public void SubmitApiKey()
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
                dropdown.options.Add(new TMP_Dropdown.OptionData("No devices found"));
            }
            else
            {
                foreach (var device in devices)
                {
                    Debug.Log("Found device: " + device);
                    dropdown.options.Add(new TMP_Dropdown.OptionData(device));
                }
            }
            dropdown.RefreshShownValue();
        }

        private void ChangeMicrophone(int index)
        {
            Debug.Log($"Microphone changed to: {dropdown.options[index].text}");
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        public void ToggleRecording()
        {
            if (isRecording)
            {
                Debug.Log("ToggleRecording called but already recording, stopping now.");
                EndRecording();
            }
            else
            {
                Debug.Log("ToggleRecording called and starting recording now.");
                StartRecording();
            }
        }

        public void StartRecording()
        {
            if (isRecording) return; // Prevent starting if already recording
            if (!recordButton)
                Debug.Log("The record button is null StartRecording function");
            isRecording = true;
            recordButton.enabled = false;

            time = 0; // Reset time when starting a new recording
            SetProgressBarColor(new Color(0.847f, 1.0f, 0.824f)); // Set color to #D8FFD2

            var index = PlayerPrefs.GetInt("user-mic-device-index");

            if (index >= dropdown.options.Count)
            {
                Debug.LogError("Invalid microphone index.");
                return;
            }

            var micName = dropdown.options[index].text;
            if (string.IsNullOrEmpty(micName))
            {
                Debug.LogError("Selected microphone name is null or empty.");
                return;
            }

            Debug.Log($"Starting recording with device: {micName}");
            clip = Microphone.Start(micName, false, duration, 44100);
            Debug.Log(clip + " Is the clip");

            muteManager?.Mute(); // Mute the VoIP microphone
        }

        public async void EndRecording()
        {
            if (!recordButton)
                Debug.Log("The record button is null EndRecording function");

            if (!isRecording) return; // Ensure this method is only called once
            Debug.Log("In the EndRecording method");
            isRecording = false;
            recordButton.enabled = true;

            Debug.Log("Beginning transcription it should say transcribing");
            message.text = "Transcripting...";

            Microphone.End(null);

            if (clip == null)
            {
                Debug.LogError("AudioClip is null. Recording might not have been started correctly.");
                return;
            }

            byte[] data = SaveWav.Save(fileName, clip);

            if (data == null)
            {
                Debug.LogError("Failed to save audio data.");
                return;
            }

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };

            if (openai == null)
            {
                Debug.LogError("OpenAI API is not initialized.");
                return;
            }

            Debug.Log("Using API key: " + currentApiKey);
            var res = await openai.CreateAudioTranscription(req);

            StartCoroutine(AnimateProgressBarFill(0));
            SetProgressBarColor(new Color(0.314f, 0.388f, 0.835f)); // Set color to #5063D5 after recording
            message.text = res.Text;

            Debug.Log("Unmuting all microphones after Whisper recording.");
            muteManager?.UnMute(); // Unmute the VoIP microphone
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                float fillAmount = time / duration;
                StartCoroutine(AnimateProgressBarFill(fillAmount));

                if (time >= duration)
                {
                    Debug.Log("Recording duration reached, ending recording.");
                    time = 0;
                    EndRecording();
                }
            }
        }

        private void SetProgressBarColor(Color color)
        {
            // Set color for the progress bar
            progressBar.color = color;

            // Set color for all child components of the progress bar
            foreach (Transform child in progressBar.transform)
            {
                var childImage = child.GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.color = color;
                }
            }
        }

        private IEnumerator AnimateProgressBarFill(float targetFillAmount)
        {
            float startFillAmount = progressBar.fillAmount;
            float elapsed = 0f;
            float duration = 0.5f; // Animation duration

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                progressBar.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, elapsed / duration);
                yield return null;
            }

            progressBar.fillAmount = targetFillAmount;
        }
    }
}
