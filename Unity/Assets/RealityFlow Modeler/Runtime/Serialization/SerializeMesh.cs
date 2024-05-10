using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Spawning;
using Newtonsoft.Json;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;

[RequireComponent(typeof(MeshFilter))]
public class SerializeMesh : MonoBehaviour//, IRealityFlowObject
{
    //private static readonly JsonSerializerOptions options = new JsonSerializerOptions(){ DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    public static SerializeMesh instance;
    [SerializeField] public GameObject primitive;
    private GameObject go;
    private Mesh myMesh;

    private EditableMesh em;

    private SerializableMeshInfo smi;
    private IRealityFlowObject ro;
    private XRRayInteractor interactor;
    private RaycastHit currentHitResult;
    private bool lookingAtObject;
    private Vector3 spawnPos;
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;
    private float lastSize;
    private Color color;
    private float metalFactor;
    private float glossFactor;
   
    /*temp*/
    private ShapeType baseShape;

    // Start is called before the first frame update
    void Start()
    {
        //ProjectContentManager.instance.projectLoad.AddListener(ImportMesh);

        interactor = GameObject.Find("Right Controller").GetComponentInChildren<XRRayInteractor>();
        lookingAtObject = false;
    }

    // Update is called once per frame
    void Update()
    {
        GetRayCollision();


        if (currentHitResult.collider != null)
        {
            lookingAtObject = true;
        }
        else
        {
            lookingAtObject = false;
        }

    }

    private void GetRayCollision()
    {
        interactor.TryGetCurrent3DRaycastHit(out currentHitResult);
    }

    public void ExportMesh()
    {
        if (lookingAtObject)
        {
            /*GameObject go = currentHitResult.collider.gameObject;

            if (System.IO.File.Exists(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\meshFile.json"))
            {
                System.IO.File.Delete(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\meshFile.json");
            }

            //ProjectContentManager.instance.AddObject(go);

            smi = new SerializableMeshInfo(go);

            File.WriteAllText(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\meshFile.json", JsonConvert.SerializeObject(smi));

            */
        }
        if (!System.IO.File.Exists(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\meshFile.json"))
        {
            Debug.LogError("meshFile.json file does not exist.");
            return;
        }

        StreamReader file = File.OpenText(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\meshFile.json");

        JsonSerializer serializer = new JsonSerializer();
        smi = (SerializableMeshInfo)serializer.Deserialize(file, typeof(SerializableMeshInfo));

        GameObject go = NetworkSpawnManager.Find(this).SpawnWithPeerScope(primitive);

        go.GetComponent<EditableMesh>().smi = smi;

        ProjectContentManager.instance.AddObject(go);
        

        file.Close();
    }

    //public void acceptSmi(SerializableMeshInfo smi)
    //{
    //    GameObject go = NetworkSpawnManager.Find(this).SpawnWithPeerScope(primitive);

    //    go.GetComponent<EditableMesh>().smi = smi;
    //}

    public void ImportMesh()
    {
        GameObject go = NetworkSpawnManager.Find(this).SpawnWithPeerScope(primitive);
        ProjectContentManager.instance.LoadObject(go.GetComponent<EditableMesh>());

        //GameObject go = currentHitResult.collider.gameObject;

        if (System.IO.File.Exists(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\retrievedMesh.json"))
        {
            System.IO.File.Delete(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\retrievedMesh.json");
        }

        ////get smi from server through load, and then load editable mesh into scene
        /// smi = loadobject?
        /// File.WriteAllText(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\retrievedMesh.json", JsonConvert.SerializeObject(smi));
        ///
        /// GameObject go = ProjectContentManager.instance.AddObject(primitive);
        /// go.GetComponent<EditableMesh>().smi = smi;


        //ProjectContentManager.instance.AddObject(go);

        //smi = new SerializableMeshInfo(go);

        //File.WriteAllText(Application.dataPath + "\\RealityFlow Modeler\\Runtime\\Serialization\\retrievedMesh.json", JsonConvert.SerializeObject(smi));

        //Debug.Log("Mesh dumped");
        
    }
   


}
