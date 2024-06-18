using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class PrimitiveSpawner interfaces with the palette to handle spawning primitives
/// </summary>
public class PrimitiveMenu : MonoBehaviour
{
    public PrimitiveSpawningMode spawningMode;

    public static event Action<ShapeType> OnEnterMeshSpawn;
    public static event Action OnExitMeshSpawn;

    public void SpawnPlane()
    {
        SpawnPrimitive(ShapeType.Plane);
    }

    public void SpawnCube()
    {
        SpawnPrimitive(ShapeType.Cube);
    }

    public void SpawnWedge()
    {
        SpawnPrimitive(ShapeType.Wedge);
    }

    public void SpawnCylinder()
    {
        SpawnPrimitive(ShapeType.Cylinder);
    }

    public void SpawnCone()
    {
        SpawnPrimitive(ShapeType.Cone);
    }

    public void SpawnUVSphere()
    {
        SpawnPrimitive(ShapeType.Sphere);
    }

    public void SpawnTorus()
    {
        SpawnPrimitive(ShapeType.Torus);
    }

    public void SpawnPipe()
    {
        SpawnPrimitive(ShapeType.Pipe);
    }
    
    public void ExitMeshMode()
    {
        // Only run this script if you are the owner of the palette
        if (gameObject.transform.parent.GetComponent<NetworkedPalette>().owner)
        {
            OnExitMeshSpawn?.Invoke();
        }
    }

    private void SpawnPrimitive(ShapeType type)
    {
        // Only run this script if you are the owner of the palette
        if (gameObject.transform.parent.GetComponent<NetworkedPalette>().owner)
        {
            OnEnterMeshSpawn?.Invoke(type);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
