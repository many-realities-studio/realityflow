using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;

/// <summary>
/// Class NetworkedPlayManager provides the current state of the room and is connected to the Ubiq Network library.
/// </summary>
public class NetworkedPlayManager : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;

    public bool playMode;

    // Users that are in the room when Play mode is turned on or off has context. Users that join the room during Play mode will not have context, thus will need it.
    public bool hasContext;

    private bool lastPlayModeState;

    void Awake()
    {
        if (this.NetworkId == null)
            Debug.Log("Networked Object " + gameObject.name + " Network ID is null");
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);

        RequestPlayState();
    }

    private void RequestPlayState()
    {
        if (hasContext)
        {
            return;
        }

        // Debug.Log("Requested Play data");
        Message msg = new Message();
        msg.needsData = true;

        context.SendJson(msg);
    }

    // Update is called once per frame
    void Update()
    {
        if (lastPlayModeState != playMode)
        {
            lastPlayModeState = playMode;

            BroadcastPlayState();
        }
    }

    private void BroadcastPlayState()
    {
        context.SendJson(new Message()
        {
            play = playMode,
        });
    }

    private void SendContextData()
    {
        context.SendJson(new Message()
        {
            needsContext = true,
        });
    }

    private struct Message
    {
        public bool needsData;
        public bool play;
        public bool needsContext;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Debug.Log("Getting message from scene = " + gameObject.transform.parent.parent.parent.name);

        // Parse the message
        var m = message.FromJson<Message>();

        if(m.needsData && hasContext)
        {
            // Debug.Log("We need data for the mesh from scene " + gameObject.transform.parent.parent.parent.name);
            BroadcastPlayState();
            SendContextData();
            return;
        }

        if (m.needsContext)
        {
            hasContext = true;
            return;
        }

        playMode = m.play;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPlayModeState = playMode;
    }
}
