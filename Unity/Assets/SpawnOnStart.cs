using System.Collections;
using System.Collections.Generic;
using Ubiq.Spawning;
using UnityEngine;

public class SpawnOnStart : MonoBehaviour
{

    public GameObject objectPrefabToSpawn;
    // Start is called before the first frame update
    void Start()
    {
        NetworkSpawnManager.Find(this).SpawnWithPeerScope(objectPrefabToSpawn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
