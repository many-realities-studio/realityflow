using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.Rooms;
using UnityEngine;


/// <summary>
/// Class PeerDictionary tracks when users join or leave the room and updates a dictionary with uuid as key.
/// This is experimental for now, but it could be useful for tools that would need to know what other users are
/// interacting with when they're run. Probably not the greatest in component form and should be implemented into
/// whatever tool/feature would need this.
/// </summary>
public class PeerDictionary : MonoBehaviour
{
    public Dictionary<string, List<GameObject>> peers = new Dictionary<string, List<GameObject>>();

    public RoomClient room { get; private set; }

    void Start()
    {
        room = RoomClient.Find(this);

        if (room == null)
            Debug.Log("No RoomCLient found!");

        AddCurrentPeers();

        room.OnPeerAdded.AddListener(OnPeerAdded);
        room.OnPeerRemoved.AddListener(OnPeerRemoved);
    }
    void OnDestroy()
    {
        room.OnPeerAdded.RemoveListener(OnPeerAdded);
        room.OnPeerRemoved.RemoveListener(OnPeerRemoved);
    }

    private void AddCurrentPeers()
    {
        // When called at startup it doesn't account for the first user to join the room.
        // We have to add it manually
        foreach (var peer in room.Peers)
        {
            peers.Add(peer.uuid, new List<GameObject>());
        }
    }

    void OnPeerAdded(IPeer peer)
    {
        try
        {
            Debug.Log("Peer joined" + peer.uuid);
            peers.Add(peer.uuid, new List<GameObject>());
        }
        catch
        {
            Debug.Log("Peer already in room");
        }
    }

    void OnPeerRemoved(IPeer peer)
    {
        Debug.Log("Peer left" + peer.uuid);
        peers.Remove(peer.uuid);
    }
}
