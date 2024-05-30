using System;
using UnityEngine;
using Ubiq.Spawning;


// Test easy way to test primitive generation
// Menu will probably implement something similar for this
public class CubeCreationTest : MonoBehaviour
{

    [SerializeField]
    GameObject cubePrefab;

    private GameObject spawnedObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            // Have to use prefabs for network spawning
            // For now we generate the mesh locally, then copy it over to the networked one
            GameObject go = NetworkSpawnManager.Find(this).SpawnWithPeerScope(cubePrefab);

            // Probably should modify this to just return verts/faces 
            EditableMesh mesh = PrimitiveGenerator.CreatePlane(new Vector3(2, 2, 2));

            EditableMesh em = go.GetComponent<EditableMesh>();
            spawnedObject = go;
            em.CreateMesh(mesh);

            // Delete local mesh
            Destroy(mesh.gameObject);

            //PrimitiveCreationParams prim = new PrimitiveCreationParams();
        }

        if(Input.GetKeyDown(KeyCode.K))
        {
            spawnedObject.GetComponent<MeshVisulization>().DisplayFaceHandles();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            spawnedObject.GetComponent<MeshVisulization>().DisplayVertexHandles();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            spawnedObject.GetComponent<MeshVisulization>().DisplayEdgeHandles();
        }
    }
}
