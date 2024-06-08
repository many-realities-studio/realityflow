#if UNITY_WEBRTC || UNITY_WEBRTC_UBIQ_FORK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Ubiq.Voip.Implementations.Unity
{
    public class PeerConnectionMicrophone : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Starting,
            Running
        }

        public State state
        {
            get
            {
                if (!audioSource || audioSource.clip == null)
                {
                    return State.Idle;
                }
                if (audioSource.isPlaying)
                {
                    return State.Running;
                }
                return State.Starting;
            }
        }

        public AudioStreamTrack audioStreamTrack { get; private set; }
        public event Action<AudioStats> statsPushed;

        private AudioSource audioSource;
        private List<GameObject> users = new List<GameObject>();
        private bool microphoneAuthorized;
        private AudioStatsFilter statsFilter;

        //RealityFlow added flag to track mute state
        private bool isMuted = false;

        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#endif
        }

        private void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Wait for microphone permissions before processing any audio
            if (!microphoneAuthorized)
            {
                microphoneAuthorized = Permission.HasUserAuthorizedPermission(Permission.Microphone);

                if (!microphoneAuthorized)
                {
                    return;
                }
            }
#endif
            //RealityFlow added logic
            if (isMuted)
            {
                // If muted, stop the audio source if it's running
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log("Microphone is muted. Audio source stopped.");
                }
                return;
            }

            if (state == State.Idle && users.Count > 0)
            {
                RequireAudioSource();
                audioSource.clip = Microphone.Start("", true, 1, AudioSettings.outputSampleRate);
            }

            if (state == State.Starting)
            {
                if (Microphone.GetPosition("") > audioSource.clip.frequency / 8.0f)
                {
                    audioSource.loop = true;
                    audioSource.Play();
                    audioStreamTrack = new AudioStreamTrack(audioSource);
                }
            }

            if (state == State.Running && users.Count == 0)
            {
                audioSource.Stop();
                Microphone.End("");
                Destroy(audioSource.clip);
                audioSource.clip = null;
                audioStreamTrack.Dispose();
                audioStreamTrack = null;
            }
        }

        //Mute and Unmute are RealityFlow added functions

        public void Mute()
        {
            isMuted = true;
            Debug.Log("Microphone muted.");
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Audio source stopped due to muting.");
            }
        }

        public void Unmute()
        {
            isMuted = false;
            Debug.Log("Microphone unmuted.");
            if (audioSource != null)
            {
                if (!Microphone.IsRecording(null))
                {
                    Debug.Log("Restarting microphone.");
                    audioSource.clip = Microphone.Start("", true, 1, AudioSettings.outputSampleRate);
                    while (!(Microphone.GetPosition(null) > 0)) { } // Wait until the recording has started
                    audioSource.Play();
                }
                else
                {
                    audioSource.Play();
                    Debug.Log("Audio source started playing due to unmuting.");
                }
            }
            else
            {
                Debug.LogError("Audio source is null in Unmute.");
            }
        }

        private void StatsFilter_StatsPushed(AudioStats stats)
        {
            statsPushed?.Invoke(stats);
        }

        private void RequireAudioSource()
        {
            if (!audioSource)
            {
                audioSource = GetComponent<AudioSource>();

                if (!audioSource)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    statsFilter = gameObject.AddComponent<AudioStatsFilter>();
                    statsFilter.hideFlags = HideFlags.HideInInspector;
                    statsFilter.SetStatsPushedCallback(StatsFilter_StatsPushed);
                }
            }
        }

        public IEnumerator AddUser(GameObject user)
        {
            if (!users.Contains(user))
            {
                users.Add(user);
            }

            while (state != State.Running)
            {
                yield return null;
            }
        }

        public void RemoveUser(GameObject user)
        {
            users.Remove(user);
        }
    }
}
#endif
