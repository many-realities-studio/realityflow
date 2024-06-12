using Ubiq.Voip;
using UnityEngine;

public class MuteManager : MonoBehaviour
{
    private VoipPeerConnectionManager voipPeerConnectionManager;
    private bool isMuted = false;

    private void Start()
    {
        voipPeerConnectionManager = FindObjectOfType<VoipPeerConnectionManager>();
    }

    public void ToggleMute()
    {
        Debug.Log("ToggleMute called. Current isMuted state: " + isMuted);

        if (isMuted)
        {
            UnMute();
        }
        else
        {
            Mute();
        }
        isMuted = !isMuted; // Toggle the mute state
        Debug.Log("Toggle Mute: isMuted = " + isMuted);
    }

    public void Mute()
    {
        voipPeerConnectionManager?.MuteAll(); // Mute the VoIP microphone
        Debug.Log("Muted");
    }

    public void UnMute()
    {
        voipPeerConnectionManager?.UnmuteAll(); // Unmute the VoIP microphone
        Debug.Log("Unmuted");
    }
}
