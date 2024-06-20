using OpenAI;
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
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private MRTKTMPInputField message;
        [SerializeField] private MRTKTMPInputField apiKeyInputField;
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button muteButton;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private GameObject flashingLight; // Reference to the flashing light GameObject
        [SerializeField] private GameObject nearmenutoolbox; // Reference to the nearmenutoolbox
        [SerializeField] private GameObject LLMWindow; // Reference to the LLMWindow

        private readonly string fileName = "output.wav";
        private readonly int maxDuration = 5;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai;
        private MuteManager muteManager;
        private string currentApiKey;

        private RealityFlowActions inputActions;
        private ChatGPTTester chatGPTTester; // Reference to the ChatGPTTester

        private Coroutine countdownCoroutine;
        private bool isCountdownActive;

        private void Awake()
        {
            inputActions = new RealityFlowActions();
            chatGPTTester = FindObjectOfType<ChatGPTTester>(); // Initialize the ChatGPTTester reference
        }

        private void OnEnable()
        {
            inputActions.RealityFlowXRActions.ToggleRecording.performed += OnRecordingStarted;
            inputActions.RealityFlowXRActions.ToggleRecording.canceled += OnRecordingCanceled;
            inputActions.RealityFlowXRActions.Execute.started += OnExecute; // Register the Execute action
            inputActions.RealityFlowXRActions.OpenLLMMenu.started += OnOpenLLMMenu; // Register the OpenLLMMenu action
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.RealityFlowXRActions.ToggleRecording.performed -= OnRecordingStarted;
            inputActions.RealityFlowXRActions.ToggleRecording.canceled -= OnRecordingCanceled;
            inputActions.RealityFlowXRActions.Execute.started -= OnExecute; // Unregister the Execute action
            inputActions.RealityFlowXRActions.OpenLLMMenu.started -= OnOpenLLMMenu; // Unregister the OpenLLMMenu action
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

        private void Start()
        {
            recordButton.onClick.AddListener(ToggleRecording);
            //dropdown.onValueChanged.AddListener(ChangeMicrophone);
            submitButton.onClick.AddListener(SubmitApiKey);
            muteButton.onClick.AddListener(() => muteManager?.ToggleMute());

            // Comment out the microphone list refresh and selection
            //RefreshMicrophoneList();

            muteManager = FindObjectOfType<MuteManager>();
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
                Debug.Log("In whisper API key is :" + apiKey);
                InitializeOpenAI(apiKey);
            }
            else
            {
                Debug.LogError("OpenAI API key is not set. Please provide it through the environment variable or the input field.");
            }

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);

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

        /* 
        // Commented out the microphone list refresh and selection
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
        */

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
            SetProgressBarColor(new Color(0.847f, 1.0f, 0.824f)); // Set color to #D8FFD2

            progressText.text = "Talking to Whisper"; // Update progress text

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
            progressText.text = "Transcripting..."; // Update progress text

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
            progressText.text = "In the Whisper menu"; // Update progress text

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

        private void SetProgressBarColor(Color color)
        {
            progressBar.color = color;

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
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                progressBar.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, elapsed / duration);
                yield return null;
            }

            progressBar.fillAmount = targetFillAmount;
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
    }
}
