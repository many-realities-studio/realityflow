using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using Samples.Whisper;
using Org.BouncyCastle.Crypto.Modes;
using Ubiq.Samples;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UX;
using Samples.Whisper;
using Ubiq.Samples;
using System.IO;
public class ChangeTextOnButtonPress : MonoBehaviour
{
    public StatefulInteractable[] PressableButtons; // Array of pressable buttons
    public TMP_Text TextDisplay; // Reference to the TMP text UI element
    public Whisper whisperRoot; // Reference to the Whisper script

    private int currentRecordingIndex = -1; // To keep track of the current recording button
    private AudioClip currentAudioClip; // To store the current audio clip
    private byte[] data;
    private int questionNumber = 0; // To track the question number

    private string[] TextArray = new string[] {
        "I liked the possibilities given by the system. ",
        "I felt immersed in the environment. ",
        "It was simple creating a behavior for an object. ",
        "It was simple to create a system composed of multiple objects. ",
        "Visual appearance properties are simpler to add than changing the kinematics of the object.",
        "The behaviors I added agreed with my description. ",
        "The system was responsive, and the behavior was added in an acceptable time. ",
        "I liked the overall experience."
    };

    private string[] fileNames = new string[]
    {
        "recording1.wav",
        "recording2.wav",
        "recording3.wav",
        "recording4.wav",
        "recording5.wav",
        "recording6.wav",
        "recording7.wav",
        "recording8.wav"
    };

    private void Start()
    {
        for (int i = 0; i < PressableButtons.Length; i++)
        {
            int index = i; // Capture the current index for the lambda
            PressableButtons[index].OnClicked.AddListener(() => OnButtonClicked(index));
        }
    }

    private void OnButtonClicked(int index)
    {
        if (index >= 0 && index < TextArray.Length)
        {
            TextDisplay.text = TextArray[index];

            // Stop the current recording if another button is pressed or the same button is pressed again
            if (currentRecordingIndex != -1 && Microphone.IsRecording(null))
            {
                StopRecording(currentRecordingIndex);
            }

            // Start recording for the new button if a different button is pressed
            if (currentRecordingIndex != index)
            {
                StartRecording(index);
                currentRecordingIndex = index;
                questionNumber++; // Increment the question number
            }
            else
            {
                currentRecordingIndex = -1; // Reset if the same button is pressed again
            }
        }
        else
        {
            Debug.LogError("Index out of range.");
        }
    }

    private void StartRecording(int index)
    {
        string micName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        currentAudioClip = Microphone.Start(micName, false, 30, 44100); // Start recording with a maximum duration of 30 seconds
        Debug.Log("Recording started for button index: " + index);
    }

    private void StopRecording(int index)
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null); // Stop the recording
            SaveRecording(currentAudioClip, fileNames[index], index);
            Debug.Log("Recording stopped and saved for button index: " + index);
        }
    }

    private void SaveRecording(AudioClip clip, string fileName, int index)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null. Recording might not have been started correctly.");
            return;
        }

        // Use the provided SaveWav class to save the audio clip
        data = SaveWav.Save(fileName, clip);
        Debug.Log("Audio saved to: " + fileName);
        TranscribeRecording(data, fileName, index, questionNumber);
    }

    private async void TranscribeRecording(byte[] audioData, string fileName, int index, int questionNumber)
    {
        var res = await whisperRoot.TranscribeRecordingAsync(audioData);
        RealityFlowAPI.Instance.LogActionToServer("ExitSurvey", new { transcription = res, questionNumber, rating = index, fileName });
        Debug.Log("Transcription result: " + res);
    }
}