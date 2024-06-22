﻿using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Ubiq.Voip;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using UnityEngine.InputSystem;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private MRTKTMPInputField message;
        [SerializeField] private GameObject flashingLight; // Reference to the flashing light GameObject
        [SerializeField] private GameObject nearmenutoolbox; // Reference to the nearmenutoolbox
        [SerializeField] private GameObject LLMWindow; // Reference to the LLMWindow

        private readonly string fileName = "output.wav";
        private readonly int maxDuration = 30;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai;
        private MuteManager muteManager;
        private string currentApiKey;

        public static Whisper rootWhisper;
        private RealityFlowActions inputActions;
        private ChatGPTTester chatGPTTester; // Reference to the ChatGPTTester

        private Coroutine countdownCoroutine;
        private bool isCountdownActive;

        private void Awake()
        {
            if (rootWhisper == null)
            {
                DontDestroyOnLoad(gameObject);
                rootWhisper = this;
            }
            else
            {
                gameObject.SetActive(false);
                DestroyObject(gameObject);
                return;
            }
            inputActions = new RealityFlowActions();
            chatGPTTester = FindObjectOfType<ChatGPTTester>(); // Initialize the ChatGPTTester reference
        }

        private void OnEnable()
        {
            inputActions.RealityFlowXRActions.ToggleRecording.performed += OnRecordingStarted;
            inputActions.RealityFlowXRActions.ToggleRecording.canceled += OnRecordingCanceled;
            inputActions.RealityFlowXRActions.Execute.started += OnExecute; // Register the Execute action
            inputActions.RealityFlowXRActions.OpenLLMMenu.started += OnOpenLLMMenu; // Register the OpenLLMMenu action
            inputActions.RealityFlowXRActions.StopCountdown.started += OnStopCountdown; // Register the StopCountdown action
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.RealityFlowXRActions.ToggleRecording.performed -= OnRecordingStarted;
            inputActions.RealityFlowXRActions.ToggleRecording.canceled -= OnRecordingCanceled;
            inputActions.RealityFlowXRActions.Execute.started -= OnExecute; // Unregister the Execute action
            inputActions.RealityFlowXRActions.OpenLLMMenu.started -= OnOpenLLMMenu; // Unregister the OpenLLMMenu action
            inputActions.RealityFlowXRActions.StopCountdown.started -= OnStopCountdown; // Unregister the StopCountdown action
            inputActions.Disable();
        }

        private void OnRecordingStarted(InputAction.CallbackContext context)
        {
            Debug.Log("OnRecordingStarted: " + context.control + " at " + Time.time);
            StartRecording();
        }

        private void OnRecordingCanceled(InputAction.CallbackContext context)
        {
            Debug.Log("OnRecordingCanceled: " + context.control + " at " + Time.time);
            EndRecording();
        }

        private void OnExecute(InputAction.CallbackContext context)
        {
            ExecuteLoggedActions();
        }

        private void OnOpenLLMMenu(InputAction.CallbackContext context)
        {
            if (LLMWindow != null)
            {
                LLMWindow.SetActive(!LLMWindow.activeSelf); // Toggle the LLMWindow's active state
            }
        }

        private void OnStopCountdown(InputAction.CallbackContext context)
        {
            StopCountdown();
        }
        //Formerly known as start
        public void InitializeGPT(string apiKey)
        {

            Debug.Log("################################# the apikey is " + apiKey);
            muteManager = FindObjectOfType<MuteManager>();
            if (muteManager == null)
            {
                Debug.LogError("MuteManager not found in the scene.");
            }

            //string apiKey = EnvConfigManager.Instance.OpenAIApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("In whisper API key is :" + apiKey);
                InitializeOpenAI(apiKey);
            }
            else
            {
                Debug.LogError("OpenAI API key is not set. Please provide it through the environment variable or the input field.");
            }

            if (flashingLight != null)
            {
                flashingLight.SetActive(false); // Ensure the flashing light is initially off
            }

            if (nearmenutoolbox != null)
            {
                nearmenutoolbox.SetActive(false); // Ensure the nearmenutoolbox is initially off
            }

            if (LLMWindow != null)
            {
                LLMWindow.SetActive(false); // Ensure the LLMWindow is initially off
            }
        }

        private void InitializeOpenAI(string apiKey)
        {
            currentApiKey = apiKey;
            openai = new OpenAIApi(apiKey);
            Debug.Log("OpenAI API initialized with provided key.");
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
                if (isCountdownActive)
                {
                    Debug.Log("ToggleRecording called during countdown, canceling countdown and restarting recording.");
                    StopCoroutine(countdownCoroutine);
                    isCountdownActive = false;
                }
                Debug.Log("ToggleRecording called and starting recording now.");
                StartRecording();
            }
        }

        public void StartRecording()
        {
            if (isRecording) return;
            isRecording = true;
            //recordButton.enabled = false;

            time = 0;
            // Use the default microphone
            string micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
            if (string.IsNullOrEmpty(micName))
            {
                Debug.LogError("No microphone devices found.");
                return;
            }

            Debug.Log($"Starting recording with default device: {micName}");
            clip = Microphone.Start(micName, false, maxDuration, 44100); // Set duration to 10 minutes
            Debug.Log(clip + " Is the clip");

            muteManager?.Mute();

            if (flashingLight != null)
            {
                StartCoroutine(FlashLight());
            }
        }

        public async void EndRecording()
        {
            if (!isRecording) return;
            isRecording = false;
            // recordButton.enabled = true;

            Debug.Log("In the EndRecording method");

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
            // Write the transcribed text to the MRTKTMPInputField
            //message.text = $"This is what we heard you say, is this correct:\n\n \"{res.Text}\"?";

            // Print the transcribed message to the debug log
            Debug.Log("Transcribed message: " + res.Text);

            Debug.Log("Unmuting all microphones after Whisper recording.");
            muteManager?.UnMute();

            if (flashingLight != null)
            {
                StopCoroutine(FlashLight());
                flashingLight.SetActive(false); // Turn off the flashing light
            }

            if (nearmenutoolbox != null)
            {
                nearmenutoolbox.SetActive(true); // Enable the nearmenutoolbox
                if (countdownCoroutine != null)
                {
                    StopCoroutine(countdownCoroutine);
                }
                countdownCoroutine = StartCoroutine(StartCountdown(res.Text));
            }
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                float fillAmount = time / maxDuration; // Fill amount based on 10 minutes duration
                if (time >= maxDuration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }

        private IEnumerator FlashLight()
        {
            while (isRecording)
            {
                flashingLight.SetActive(!flashingLight.activeSelf); // Toggle light on and off
                yield return new WaitForSeconds(0.5f); // Wait for half a second
            }
        }

        private IEnumerator StartCountdown(string transcribedText)
        {
            isCountdownActive = true;
            int countdown = 5;
            while (countdown > 0)
            {
                message.text = $"This is what we heard you say, is this correct:\n\n \"{transcribedText}\"?\n\nMessage will be sent in {countdown} seconds unless canceled or re-recorded.";
                yield return new WaitForSeconds(1);
                countdown--;
            }
            message.text = $"{transcribedText}";
            isCountdownActive = false;

            SendMessageToChatGPT();
        }

        private void SendMessageToChatGPT()
        {
            // Reference to the ChatGPTTester instance
            ChatGPTTester chatGPTTester = FindObjectOfType<ChatGPTTester>();
            if (chatGPTTester != null)
            {
                chatGPTTester.Execute(); // Call the Execute method from ChatGPTTester
            }

            if (nearmenutoolbox != null)
            {
                nearmenutoolbox.SetActive(false); // Disable the nearmenutoolbox
            }

            if (LLMWindow != null)
            {
                LLMWindow.SetActive(true); // Enable the LLMWindow
            }
        }

        private void ExecuteLoggedActions()
        {
            if (chatGPTTester != null)
            {
                chatGPTTester.ExecuteLoggedActions();
            }
        }

        // New method to stop the countdown
        public void StopCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
                isCountdownActive = false;
                Debug.Log("Countdown stopped.");
            }
        }
    }
}
