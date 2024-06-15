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
    private EditableMesh objEM;

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
            /*em.CreateMesh(PrimitiveGenerator.CreatePrimitive(ShapeType.Wedge));
            go.GetComponent<NetworkedMesh>().SetSize(2.0f);
            objEM = em;*/
            spawnedObject = go;
            em.CreateMesh(mesh);

            // Delete local mesh
            Destroy(mesh.gameObject);

            //PrimitiveCreationParams prim = new PrimitiveCreationParams();
        }

        if(Input.GetKeyDown(KeyCode.K))
        {
            spawnedObject.GetComponent<MeshVisulization>().DisplayFaceHandles();
            /*int[] arr = { 0 };
            objEM.TranslateVerticesWithNetworking(arr, new Vector3(0.0f, 0.5f, 0.0f));
            objEM.RefreshMesh();*/
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            spawnedObject.GetComponent<MeshVisulization>().DisplayVertexHandles();
            /*objEM.TransformVertex(0, new Vector3(0.0f, -0.5f, 0.0f));
            objEM.RefreshMesh();*/
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            spawnedObject.GetComponent<MeshVisulization>().DisplayEdgeHandles();
            /*int[] indices = { 4, 5, 6, 7 };
            //objEM.ScaleVertices(indices, );*/
        }
    }
}
