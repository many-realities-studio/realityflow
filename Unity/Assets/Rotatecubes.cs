using UnityEngine;
using System.Collections;

public class RotateCubes : MonoBehaviour
{
    public GameObject cubePrefab;
    private GameObject[] cubes = new GameObject[20];
    
    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            cubes[i] = GameObject.Instantiate(cubePrefab, new Vector3(i * 2, 0, 0), Quaternion.identity);
        }
    }

    void Update()
    {
        foreach (GameObject cube in cubes)
        {
            if (cube != null)
            {
                cube.transform.Rotate(new Vector3(0, Time.deltaTime * 1000, 0));
            }
        }
    }
}