using System;
using RealityFlow.NodeUI;
using Ubiq.Rooms;
using Ubiq.Spawning;
using Unity.VisualScripting;
using UnityEngine;

namespace RealityFlow.API.Actions
{
    public class SpawnObject : IAction
    {
        public string prefabName;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        public RealityFlowAPI.SpawnScope scope = RealityFlowAPI.SpawnScope.Room;
        public Action<GameObject> onSpawn;

        GameObject spawned;

        public string Name => nameof(SpawnObject);

        public System.Diagnostics.StackTrace StackSource { get; set; }

        public bool IsPersistent { get; set; } = false;

        public static bool CanBePersistent => true;

        public void Do()
        {
            GameObject newObject = RealityFlowAPI.Instance.GetPrefabByName(prefabName);

            if (newObject != null && RealityFlowAPI.Instance.SpawnManager != null)
            {
                switch (scope)
                {
                    case RealityFlowAPI.SpawnScope.Room:
                        RealityFlowAPI.Instance.SpawnManager.OnSpawned.AddListener(SpawnCallback);
                        RealityFlowAPI.Instance.SpawnManager.SpawnWithRoomScope(newObject);
                        Debug.Log("Spawned with Room Scope");
                        break;
                    case RealityFlowAPI.SpawnScope.Peer:
                        spawned = RealityFlowAPI.Instance.SpawnManager.SpawnWithPeerScope(newObject);
                        Debug.Log("Spawned with Peer Scope");
                        SpawnCallback(spawned, null, null, NetworkSpawnOrigin.Local);
                        break;
                    default:
                        Debug.LogError("Unknown spawn scope");
                        break;
                }
            }
            else
            {
                Debug.LogError("Prefab not found or NetworkSpawnManager is not initialized.");
            }
        }

        public void Undo()
        {
            if (IsPersistent)
                spawned.SetActive(false);
            else
                UnityEngine.Object.Destroy(spawned);
        }

        void SpawnCallback(GameObject spawned, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
        {
            this.spawned = spawned;

            if (spawned && scope == RealityFlowAPI.SpawnScope.Room)
            {
                SetupNewObject(spawned);

                if (IsPersistent)
                    // also calls onSpawn after completing
                    PersistObject(spawned);
                else
                    onSpawn?.Invoke(spawned);
            }
            else
            {
                Debug.LogError("Could not find the spawned object in the scene or the object was spawned with peer scope.");
            }

            RealityFlowAPI.Instance.SpawnManager.OnSpawned.RemoveListener(SpawnCallback);
        }

        void SetupNewObject(GameObject spawned)
        {
            spawned.transform.SetPositionAndRotation(position, rotation);
            spawned.transform.localScale = scale;

            Rigidbody rb = spawned.GetOrAddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Add BoxCollider based on bounds
            BoxCollider box = spawned.GetOrAddComponent<BoxCollider>();

            if (spawned.TryGetComponent(out Renderer boundsSource) == false)
                boundsSource = spawned.GetComponentInChildren<Renderer>();

            if (boundsSource != null)
            {
                box.center = boundsSource.bounds.center - spawned.transform.position;
                box.size = boundsSource.bounds.size;
            }

            // Add whiteboard attach
            if (spawned.GetComponent<AttachedWhiteboard>() == null)
                spawned.AddComponent<AttachedWhiteboard>();
        }

        void PersistObject(GameObject spawned)
        {
            TransformData transformData = new()
            {
                position = spawned.transform.position,
                rotation = spawned.transform.rotation,
                scale = spawned.transform.localScale
            };

            RfObject rfObject = new()
            {
                name = spawned.name,
                type = "Prefab",
                graphId = null,
                transformJson = JsonUtility.ToJson(transformData),
                meshJson = "{}",
                projectId = RealityFlowAPI.Instance.client.GetCurrentProjectId(),
                originalPrefabName = prefabName
            };

            RealityFlowAPI.Instance.CreateObjectQuery(rfObject, id =>
            {
                Debug.Log("Object saved to the database successfully.");

                rfObject.id = id;
                Debug.Log($"Assigned ID from database: {rfObject.id}");

                RealityFlowAPI.Instance.SpawnedObjects[spawned] = rfObject;
                RealityFlowAPI.Instance.SpawnedObjectsById[id] = spawned;

                RealityFlowAPI.Instance.LogActionToServer("SpawnObject", new { rfObject });

                spawned.name = rfObject.id;

                onSpawn?.Invoke(spawned);
            });
        }
    }
}