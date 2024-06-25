using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UX;
using Samples.Whisper;
using System.IO;
using Ubiq.Samples;
using System.Threading.Tasks;

public class ChangeTextOnButtonPress : MonoBehaviour
{
    public PressableButton[] PressableButtons; // Changed to StatefulButton for interaction
    public TMP_Text TextDisplay; // Reference to the TMP text UI element
    public GameObject container;
    public Whisper whisperRoot;


    private int currentRecordingIndex = -1; // To keep track of the current recording button
    private AudioClip currentAudioClip; // To store the current audio clip
    private byte[] data;
    private string[] TextArray = new string[] {
"I liked the possibilities given by the system. ",
"I felt immersed in the environment. ",
"It was simple creating a behavior for an object. ",
"It was simple to create a system composed of multiple objects. ",
"Visual appearance properties are simpler to add than changing the kinematics of the object.",
"The behaviors I added agreed with my description. ",
"The system was responsive, and the behavior was added in an acceptable time. ",
"I liked the overall experience."};
    private string[] fileNames = new string[]
    {
        "recording1.wav",
        "recording2.wav",
        "recording3.wav",
        "recording4.wav",
        "recording5.wav",
        "recording6.wav",
        "recording7.wav"
    };
    private int questionNumber = 0;

    private void Start()
    {
        if (PressableButtons.Length != TextArray.Length || PressableButtons.Length != 7)
        {
            Debug.LogError("Ensure there are 7 pressable buttons and 7 corresponding text messages.");
            return;
        }

        for (int i = 0; i < PressableButtons.Length; i++)
        {
            int index = i; // Capture the current index for the lambda
            PressableButtons[index].OnClicked.AddListener(() => OnButtonClicked(index));
        }
    }

    private async void OnButtonClicked(int index)
    {
        if (questionNumber >= 0 && questionNumber < TextArray.Length)
        {
            TextDisplay.text = TextArray[questionNumber];

            // Stop the current recording if another button is pressed or the same button is pressed again
            if (currentRecordingIndex != -1 && currentRecordingIndex != index)
            {
                StopRecording(currentRecordingIndex);
            }

            if (Microphone.IsRecording(null))
            {
                Microphone.End(null); // Stop recording if still ongoing
            }

            if (currentRecordingIndex != questionNumber)
            {
                StartRecording(questionNumber++);
            }
            else
            {
                container.SetActive(false); // Reset the container UI
                container.SetActive(true);
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
        currentAudioClip = Microphone.Start(null, false, 300, 44100); // Start recording with a maximum duration of 300 seconds
        Debug.Log("Recording started for question index: " + index);
    }

    private void StopRecording(int index)
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null); // Stop the recording
            SaveRecording(currentAudioClip, fileNames[index]);
            Debug.Log("Recording stopped and saved for question index: " + index);
        }
    }

    private void SaveRecording(AudioClip clip, string fileName)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null. Recording might not have been started correctly.");
            return;
        }

        // Use the provided SaveWav class to save the audio clip
        data = SaveWav.Save(fileName, clip);
        Debug.Log("Audio saved to: " + fileName);
        var res = whisperRoot.TranscribeRecording(data);
        RealityFlowAPI.Instance.LogActionToServer("ExitSurvey", new { transcription = res, questionNumber });
    }
}
