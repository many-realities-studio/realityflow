using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;

public class CubeSpawnerTest : MonoBehaviour
{
    public GameObject CubePrefab;

    public void SpawnCube()
    {
        NetworkSpawnManager.Find(this).SpawnWithPeerScope(CubePrefab);
        //Debug.Log("Cube spawned");
    }
}
