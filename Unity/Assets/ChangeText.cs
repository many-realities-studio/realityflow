using System.Collections;
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

public class ChangeTextOnButtonPress : MonoBehaviour
{
    public StatefulInteractable[] PressableButtons; // Array of pressable buttons
    public string[] TextArray; // Array of text messages
    public TMP_Text TextDisplay; // Reference to the TMP text UI element

    private int currentRecordingIndex = -1; // To keep track of the current recording button
    private AudioClip currentAudioClip; // To store the current audio clip

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

    private void OnButtonClicked(int index)
    {
        if (index >= 0 && index < TextArray.Length)
        {
            TextDisplay.text = TextArray[index];

            // Stop the current recording if another button is pressed or the same button is pressed again
            if (currentRecordingIndex != -1)
            {
                StopRecording(currentRecordingIndex);
            }

            // Start recording for the new button if a different button is pressed
            if (currentRecordingIndex != index)
            {
                StartRecording(index);
                currentRecordingIndex = index;
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
        currentAudioClip = Microphone.Start(null, false, 300, 44100); // Start recording with a maximum duration of 300 seconds
        Debug.Log("Recording started for button index: " + index);
    }

    private void StopRecording(int index)
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null); // Stop the recording
            SaveRecording(currentAudioClip, fileNames[index]);
            Debug.Log("Recording stopped and saved for button index: " + index);
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
        SaveWav.Save(fileName, clip);
        Debug.Log("Audio saved to: " + fileName);
    }
}