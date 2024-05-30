using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Logging;
using UnityEngine.Events;

// NOTE: This file has been modified for RealityFlow, comments with "RF" are
// placed by all the changes made.

namespace Ubiq.Spawning
{
    public enum NetworkSpawnOrigin
    {
        Local,
        Remote
    }

    public interface INetworkSpawnable
    {
        NetworkId NetworkId { get; set; }
    }

    public class NetworkSpawner : IDisposable
    {
        public NetworkScene scene { get; private set; }
        public RoomClient roomClient { get; private set; }
        public PrefabCatalogue catalogue { get; private set; }
        public string propertyPrefix { get; private set; }

        public delegate void SpawnEventHandler(GameObject gameObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin);
        public event SpawnEventHandler OnSpawned = delegate { };
        public delegate void DespawnEventHandler(GameObject gameObject, IRoom room, IPeer peer);
        public event DespawnEventHandler OnDespawned = delegate { };

        private IRoom prevSpawnerRoom;
        private Dictionary<string, GameObject> spawnedForRoom = new Dictionary<string, GameObject>();
        private Dictionary<IPeer, Dictionary<string, GameObject>> spawnedForPeers = new Dictionary<IPeer, Dictionary<string, GameObject>>();

        private List<string> tmpStrings = new List<string>();
        private List<INetworkSpawnable> tmpSpawnables = new List<INetworkSpawnable>();
        private List<NetworkId> tmpNetworkIds = new List<NetworkId>();

        // REALITYFLOW CHANGE: Value to aid in making unique object names:
        int j = 0; // RF

        [Serializable]
        private struct Message
        {
            public NetworkId creatorPeer;
            public int catalogueIndex;
        }

        public NetworkSpawner(NetworkScene scene, RoomClient roomClient,
            PrefabCatalogue catalogue, string propertyPrefix = "ubiq.spawner.")
        {
            this.scene = scene;
            this.roomClient = roomClient;
            this.catalogue = catalogue;
            this.propertyPrefix = propertyPrefix;

            roomClient.OnJoinedRoom.AddListener(OnJoinedRoom);
            roomClient.OnRoomUpdated.AddListener(OnRoomUpdated);

            roomClient.OnPeerAdded.AddListener(OnPeerAdded);
            roomClient.OnPeerUpdated.AddListener(OnPeerUpdated);
            roomClient.OnPeerRemoved.AddListener(OnPeerRemoved);
        }

        public void Dispose()
        {
            if (roomClient)
            {
                roomClient.OnJoinedRoom.RemoveListener(OnJoinedRoom);
                roomClient.OnRoomUpdated.RemoveListener(OnRoomUpdated);

                roomClient.OnPeerAdded.RemoveListener(OnPeerAdded);
                roomClient.OnPeerUpdated.RemoveListener(OnPeerUpdated);
                roomClient.OnPeerRemoved.RemoveListener(OnPeerRemoved);
            }
        }

        private void OnJoinedRoom(IRoom room)
        {
            UpdateRoom(room);
        }

        static private NetworkSpawnOrigin GetOrigin(bool local)
        {
            return local ? NetworkSpawnOrigin.Local : NetworkSpawnOrigin.Remote;
        }

        private void OnRoomUpdated(IRoom room)
        {
            UpdateRoom(room);
        }

        private void UpdateRoom(IRoom room)
        {
            // Debug.Log("Updating Room " + room.UUID); // RF
            // Spawn all unspawned objects for this room
            foreach (var item in room)
            {
                if (item.Key.StartsWith(propertyPrefix))
                {
                    if (!spawnedForRoom.ContainsKey(item.Key))
                    {
                        var msg = JsonUtility.FromJson<Message>(item.Value);
                        var local = msg.creatorPeer == roomClient.Me.networkId;
                        var go = InstantiateAndSetIds(item.Key, msg.catalogueIndex, local);

                        spawnedForRoom.Add(item.Key, go);
                        OnSpawned(go, room, peer: null, GetOrigin(local));
                    }
                }
            }

            // Remove any spawned items no longer present in properties
            tmpStrings.Clear();
            foreach (var item in spawnedForRoom)
            {
                if (room[item.Key] == string.Empty)
                {
                    tmpStrings.Add(item.Key);
                }
            }

            foreach (var key in tmpStrings)
            {
                var go = spawnedForRoom[key];
                if (go)
                {
                    OnDespawned(go, room, peer: null);
                    GameObject.Destroy(go);
                }
                spawnedForRoom.Remove(key);
            }
        }

        private void OnPeerAdded(IPeer peer)
        {
            UpdatePeer(peer);
        }

        private void OnPeerUpdated(IPeer peer)
        {
            UpdatePeer(peer);
        }

        private void UpdatePeer(IPeer peer)
        {
            spawnedForPeers.TryGetValue(peer, out var spawnedForPeer);

            // Spawn all unspawned objects for this peer
            foreach (var item in peer)
            {
                if (item.Key.StartsWith(propertyPrefix))
                {
                    if (spawnedForPeer == null)
                    {
                        spawnedForPeer = new Dictionary<string, GameObject>();
                        spawnedForPeers.Add(peer, spawnedForPeer);
                    }

                    if (!spawnedForPeer.ContainsKey(item.Key))
                    {
                        var msg = JsonUtility.FromJson<Message>(item.Value);
                        var local = msg.creatorPeer == roomClient.Me.networkId;
                        var go = InstantiateAndSetIds(item.Key, msg.catalogueIndex, local);

                        spawnedForPeer.Add(item.Key, go);
                        OnSpawned(go, room: null, peer, GetOrigin(local));
                    }
                }
            }

            // Remove any spawned items no longer present in properties
            if (spawnedForPeer == null)
            {
                return;
            }

            tmpStrings.Clear();
            foreach (var item in spawnedForPeer)
            {
                if (peer[item.Key] == string.Empty)
                {
                    tmpStrings.Add(item.Key);
                }
            }

            foreach (var key in tmpStrings)
            {
                var go = spawnedForPeer[key];
                if (go)
                {
                    OnDespawned(go, room: null, peer);
                    GameObject.Destroy(go);
                }

                spawnedForPeer.Remove(key);
            }
        }

        private void OnPeerRemoved(IPeer peer)
        {
            // Remove all objects spawned for this peer
            if (spawnedForPeers.TryGetValue(peer, out var spawned))
            {
                foreach (var go in spawned.Values)
                {
                    GameObject.Destroy(go);
                }
                spawnedForPeers.Remove(peer);
            }
        }
        public void Despawn(GameObject gameObject)
        {
            string key = null;
            foreach (var kvp in spawnedForRoom)
            {
                //kvp.Value.name = kvp.Value.name.Replace("(Clone)", "").Trim();
                if (kvp.Value == gameObject)
                {
                    key = kvp.Key;
                    break;
                }
            }

            // For room scope objects, despawn when we hear back from server
            if (key != null)
            {
                roomClient.Room[key] = string.Empty;
                return;
            }
            if (key == null && spawnedForPeers.TryGetValue(roomClient.Me, out var spawned))
            {
                foreach (var kvp in spawned)
                {
                    if (kvp.Value == gameObject)
                    {
                        key = kvp.Key;
                        break;
                    }
                }
                // For (local) peer scope objects, despawn immediately
                if (key != null)
                {
                    OnDespawned(gameObject, room: null, peer: roomClient.Me);
                    GameObject.Destroy(gameObject);
                    spawned.Remove(key);

                    roomClient.Me[key] = null;
                }
            }
        }


        /// <summary>
        /// Spawn an entity with peer scope. All Components implementing the
        /// INetworkSpawnable interface will have their NetworkIDs set and
        /// synced. Should a peer leave scope for any other peer, accompanying
        /// objects for the leaving peer will be removed for the other peer. As
        /// only the local peer can set values in their own properties, the
        /// object is created and immediately accessible.
        /// </summary>
        public GameObject SpawnWithPeerScope(GameObject gameObject)
        {


            var key = $"{propertyPrefix}{NetworkId.Unique()}"; // Uniquely id the whole object
            var catalogueIdx = ResolveIndex(gameObject);

            var go = InstantiateAndSetIds(key, catalogueIdx, local: true);
            if (!spawnedForPeers.ContainsKey(roomClient.Me))
            {
                spawnedForPeers.Add(roomClient.Me, new Dictionary<string, GameObject>());
            }
            spawnedForPeers[roomClient.Me].Add(key, go);
            OnSpawned(go, room: null, roomClient.Me, GetOrigin(local: true));

            roomClient.Me[key] = JsonUtility.ToJson(new Message()
            {
                creatorPeer = roomClient.Me.networkId,
                catalogueIndex = catalogueIdx,
            });

            return go;
        }

        // RF: There was a commented out section on an old version with a SpawnWithPeerScopeCast Method
        // Refer to Fixing-references-old-vs branch on realityflow-new repository.
        //
        //

        /// <summary>
        /// Spawn an entity with room scope. All Components implementing the
        /// INetworkSpawnable interface will have their NetworkIDs set and
        /// synced. On leaving the room, objects spawned this way will be
        /// removed locally but will persist within the room. To avoid race
        /// conditions in the shared room dictionary the object is not
        /// immediately accessible.
        /// </summary>
        public void SpawnWithRoomScope(GameObject gameObject)
        {
            var key = $"{propertyPrefix}{NetworkId.Unique()}"; // Uniquely id the whole object
            var catalogueIdx = ResolveIndex(gameObject);

            var go = InstantiateAndSetIds(key, catalogueIdx, local: true);
            spawnedForRoom.Add(key, go);

            roomClient.Room[key] = JsonUtility.ToJson(new Message()
            {
                creatorPeer = roomClient.Me.networkId,
                catalogueIndex = catalogueIdx,
            });

            OnSpawned(go, roomClient.Room, null, GetOrigin(local: true));

        }

        // RF (adds a return to above method)
        public GameObject SpawnWithRoomScopeWithReturn(GameObject gameObject)
        {
            var key = $"{ propertyPrefix }{ NetworkId.Unique() }"; // Uniquely id the whole object
            var catalogueIdx = ResolveIndex(gameObject);

            var go = InstantiateAndSetIds(key, catalogueIdx, local: true);
            if (!spawnedForRoom.ContainsKey(key))
            {
                spawnedForRoom.Add(key, go);
            }
            OnSpawned(go, roomClient.Room, peer: null, GetOrigin(local: true));

            roomClient.Room[key] = JsonUtility.ToJson(new Message()
            {
                creatorPeer = roomClient.Me.networkId,
                catalogueIndex = catalogueIdx,
            });

            return go;
        }
    public void SpawnWithRoomScope(GameObject gameObject)
    {
        var key = $"{ propertyPrefix }{ NetworkId.Unique() }"; // Uniquely id the whole object
        var catalogueIdx = ResolveIndex(gameObject);

        var go = InstantiateAndSetIds(key, catalogueIdx, local: true);
        go.name = gameObject.name; // Removing the "(Clone)" suffix

        if (!spawnedForPeers.ContainsKey(roomClient.Me))
        {
            spawnedForPeers.Add(roomClient.Me, new Dictionary<string, GameObject>());
        }
        spawnedForPeers[roomClient.Me].Add(key, go);

        // Debug log to display the network ID of the spawned object.
        var networkableComponents = go.GetComponentsInChildren<INetworkSpawnable>(true);
        foreach (var component in networkableComponents)
        {
            Debug.Log($"Spawned Object Network ID: {component.NetworkId}");
        }

        OnSpawned(go, room: null, roomClient.Me, GetOrigin(local: true));

        roomClient.Me[key] = JsonUtility.ToJson(new Message()
        {
            creatorPeer = roomClient.Me.networkId,
            catalogueIndex = catalogueIdx,
        });
    }


        private static NetworkId ParseNetworkId(string key, string propertyPrefix)
        {
            if (key.Length > propertyPrefix.Length)
            {
                return new NetworkId(key.Substring(propertyPrefix.Length));
            }
            return NetworkId.Null;
        }

        // RF - This method has a small modification for object naming
        private GameObject InstantiateAndSetIds(string key, int catalogueIdx, bool local)
        {
            var networkId = ParseNetworkId(key, propertyPrefix);
            if (networkId == NetworkId.Null)
            {
                return null;
            }

            var go = GameObject.Instantiate(catalogue.prefabs[catalogueIdx]);
            tmpSpawnables.Clear();
            go.GetComponentsInChildren<INetworkSpawnable>(true, tmpSpawnables);

            for (int i = 0; i < tmpSpawnables.Count; i++)
            {
                tmpSpawnables[i].NetworkId = NetworkId.Create(networkId, (uint)(i + 1));
            }

            go.name = go.name + j++; // RF change
            return go;
        }

        // RF - new method that helps with instantiating objects
        private GameObject InstantiateObject(string key, int catalogueIdx, bool local)
        {
            var networkId = ParseNetworkId(key, propertyPrefix);
            if (networkId == NetworkId.Null)
            {
                return null;
            }

            var go = GameObject.Instantiate(catalogue.prefabs[catalogueIdx]);
            go.name = go.name + j++;
            return go;
        }


        /*private int ResolveIndex(GameObject gameObject)
        {
           for (int curr = 0; curr < catalogue.prefabs.Count; curr++){
            Debug.Log($"Prefab {curr}: {catalogue.prefabs[curr].name}");
            }
            
            var i = catalogue.IndexOf(gameObject);
            Debug.Assert(i >= 0, $"Could not find {gameObject.name} in Catalogue. Ensure that you've added your new prefab to the Catalogue on NetworkSpawner before trying to instantiate it.");
            return i;
        }*/

        //This function was edited by RealityFlow the commented out function above was the original
        private int ResolveIndex(GameObject gameObject)
        {

            // Debug log each prefab in the catalogue for verification
            for (int curr = 0; curr < catalogue.prefabs.Count; curr++)
            {
                //Debug.Log($"Prefab {curr}: {catalogue.prefabs[curr].name}");
            }

            // Find the index of the prefab by name
            for (int i = 0; i < catalogue.prefabs.Count; i++)
            {
                if (catalogue.prefabs[i].name == gameObject.name)
                {
                    return i;
                }
            }

            Debug.LogError($"Could not find {gameObject.name} in Catalogue. Ensure that you've added your new prefab to the Catalogue on NetworkSpawner before trying to instantiate it.");
            return -1; // Return -1 if the prefab is not found
        }

        public Dictionary<string, GameObject> GetSpawnedForRoom()
        {
            return spawnedForRoom;
        }

        public Dictionary<IPeer, Dictionary<string, GameObject>> GetSpawnedForPeers()
        {
            return spawnedForPeers;
        }

    }

    public class NetworkSpawnManager : MonoBehaviour
    {
        public RoomClient roomClient;
        public PrefabCatalogue catalogue;

        public class OnSpawnedEvent : UnityEvent<GameObject, IRoom, IPeer, NetworkSpawnOrigin> { }
        public class OnDespawnedEvent : UnityEvent<GameObject, IRoom, IPeer> { }

        public OnSpawnedEvent OnSpawned = new OnSpawnedEvent();
        public OnDespawnedEvent OnDespawned = new OnDespawnedEvent();

        private NetworkSpawner spawner;

        private void Reset()
        {
            if (roomClient == null)
            {
                roomClient = GetComponentInParent<RoomClient>();
            }
#if UNITY_EDITOR
            if (catalogue == null)
            {
                try
                {
                    var asset = UnityEditor.AssetDatabase.FindAssets("Prefab Catalogue").FirstOrDefault();
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(asset);
                    catalogue = UnityEditor.AssetDatabase.LoadAssetAtPath<PrefabCatalogue>(path);
                }
                catch
                {
                    // if the default prefab has gone away, no problem
                }
            }
#endif
        }

        private void Start()
        {
            spawner = new NetworkSpawner(NetworkScene.Find(this), roomClient, catalogue);
            spawner.OnSpawned += Spawner_OnSpawned;
            spawner.OnDespawned += Spawner_OnDespawned;
        }

        private void OnDestroy()
        {
            spawner.OnSpawned -= Spawner_OnSpawned;
            spawner.OnDespawned -= Spawner_OnDespawned;
            spawner.Dispose();
            spawner = null;
        }

        /// <summary>
        /// Spawn an entity with peer scope. All Components implementing the
        /// INetworkSpawnable interface will have their NetworkIDs set and
        /// synced. Should a peer leave scope for any other peer, accompanying
        /// objects for the leaving peer will be removed for the other peer. As
        /// only the local peer can set values in their own properties, the
        /// object is created and immediately accessible.
        /// </summary>

        public GameObject SpawnWithPeerScope(GameObject gameObject)
        {
            if (spawner != null)
            {
                return spawner.SpawnWithPeerScope(gameObject);
            }
            return null;
        }

        /// <summary>
        /// Spawn an entity with room scope. All Components implementing the
        /// INetworkSpawnable interface will have their NetworkIDs set and
        /// synced. On leaving the room, objects spawned this way will be
        /// removed locally but will persist within the room. To avoid race
        /// conditions in the shared room dictionary the object is not
        /// immediately accessible.
        /// </summary>
        public void SpawnWithRoomScope(GameObject gameObject)
        {
            if (spawner != null)
            {
                spawner.SpawnWithRoomScope(gameObject);
            }
        }

        public void Despawn(GameObject gameObject)
        {
            if (spawner != null)
            {
                spawner.Despawn(gameObject);
            }
        }

        // RF - new method that returns the game object it spawns
        // Weird and recursive and not sure if it works - May go unused?
        public GameObject SpawnWithRoomScopeWithReturn(GameObject gameObject)
        {
            if(spawner != null)
            {
               return spawner.SpawnWithRoomScopeWithReturn(gameObject);
            }
            return null;
        }

        private void Spawner_OnSpawned(GameObject gameObject, IRoom room,
            IPeer peer, NetworkSpawnOrigin origin)
        {
            gameObject.transform.parent = transform;
            OnSpawned.Invoke(gameObject, room, peer, origin);
        }

        private void Spawner_OnDespawned(GameObject gameObject, IRoom room,
            IPeer peer)
        {
            OnDespawned.Invoke(gameObject, room, peer);
        }

        public static NetworkSpawnManager Find(MonoBehaviour component)
        {
            var scene = NetworkScene.Find(component);
            if (scene)
            {
                return scene.GetComponentInChildren<NetworkSpawnManager>();
            }
            return null;
        }

        public Dictionary<string, GameObject> GetSpawnedForRoom()
        {
            if (spawner != null)
            {
                return spawner.GetSpawnedForRoom();
            }
            return null;
        }

        public Dictionary<IPeer, Dictionary<string, GameObject>> GetSpawnedForPeers()
        {
            if (spawner != null)
            {
                return spawner.GetSpawnedForPeers();
            }
            return null;
        }

        public void UpdatePeerPublic(IPeer peer)
        {
            //spawner.UpdatePeer(peer);
        }

        public void UpdateRoomPublic(IRoom room)
        {
            //spawner.UpdateRoom(room);
        }
    }

}