using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine;

public class DuplicateProxy : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;
    public GameObject proxy;

    public bool needsData;
    public string parentName;

    void Start()
    {
        context = NetworkScene.Register(this);
        Debug.Log(this.NetworkId);

        RequestOriginalData();
    }

    private void RequestOriginalData()
    {
        if (parentName != "")
            return;
        else
            Debug.Log("Proxy needs data");

        context.SendJson(new Message()
        {
            parentName = "",
            needsData = true
        });
    }

    private void SendMeshData()
    {
        context.SendJson(new Message()
        {
            parentName = parentName,
            needsData = false
        });
    }

    private void ConvertToEditableMesh()
    {
        Debug.Log("Trying to convert to mesh: " + parentName);
        Debug.Log(gameObject.transform.parent.name);

        GameObject mesh = GameObject.Find(parentName);
        GameObject go = GameObject.Instantiate(proxy);

        //go.GetComponent<NetworkedMesh>().context.Id = context.Id;
        //go.GetComponent<NetworkedMesh>().NetworkId = this.NetworkId;
        Debug.Log(context.Id);
        go.GetComponent<NetworkedMesh>().RegisterWithSetID(context.Id);
        go.GetComponent<EditableMesh>().CreateMesh(mesh.GetComponent<EditableMesh>());
        go.transform.parent = gameObject.transform.parent;
        go.transform.position = mesh.transform.position;
        go.transform.rotation = mesh.transform.rotation;
        go.transform.localScale = mesh.transform.localScale;

        //Destroy(this.gameObject);
    }

    public struct Message
    {
        public string parentName;
        public bool needsData;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();

        if(m.needsData && parentName != "")
        {
            SendMeshData();
            ConvertToEditableMesh();
            return;
        }

        if(m.parentName != "")
        {
            parentName = m.parentName;
            ConvertToEditableMesh();
            NetworkSpawnManager.Find(this).Despawn(gameObject);
        }
    }
}
