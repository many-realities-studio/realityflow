using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Spawning;
using TransformTypes;
using UnityEngine;

public class NetworkedOperationCache : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;
    private EditableMesh em;

    private int cacheSize;
    private bool hasStarted;
    private bool awaitingData;

    // Start is called before the first frame update
    void Start()
    {
        if (hasStarted)
            return;

        hasStarted = true;
        context = NetworkScene.Register(this);

        em = gameObject.GetComponent<EditableMesh>();
        cacheSize = 0;

        awaitingData = false;
        RequestCacheData();
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (em != null && em.meshOperationCache != null && cacheSize != em.meshOperationCache.operations.Count)
            {
                cacheSize = em.meshOperationCache.operations.Count;
                SendMostRecentCacheData();
            }
        }
        catch { }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SendAllOperationsCacheData();
        }
    }

    #region sending
    public void RequestCacheData()
    {
        if (cacheSize > 0)
            return;

        //Debug.Log("Request Data");
        try
        {
            context.SendJson(new Message()
            {
                cacheEmpty = true
            });
        }
        catch { return; }
        awaitingData = true;
    }

    public void SendMostRecentCacheData()
    {
        if (cacheSize == 0)
            return;

        MeshOperation operation = em.meshOperationCache.operations[cacheSize - 1];
        MeshOperations type = operation.GetOperationType();
        switch (type)
        {
            case MeshOperations.Translate:
                SendTranslationData((ComponentTranslation)operation, true);
                break;
            case MeshOperations.Rotate:
                SendRotationData((ComponentRotation)operation, true);
                break;
            case MeshOperations.Scale:
                SendScalingData((ComponentScaling)operation, true);
                break;
            default:
                Debug.Log("Shouldn't reach here");
                break;
        }
    }

    public void SendAllOperationsCacheData()
    {
        // REMOVE THIS BEFORE RELEASE!!!!
        // This prevents the cache from being sent and executed multiple times, due to the testing environment
        if (!gameObject.transform.parent.parent.parent.name.Equals("Forest 1"))
            return;

        Debug.Log("Send Data");
        for(int i = 0; i < em.meshOperationCache.operations.Count; i++)
        {
            MeshOperation operation = em.meshOperationCache.operations[i];
            MeshOperations type = operation.GetOperationType();
            switch(type)
            {
                case MeshOperations.Translate:
                    SendTranslationData((ComponentTranslation)operation, false);
                    break;
                case MeshOperations.Rotate:
                    SendRotationData((ComponentRotation)operation, false);
                    break;
                case MeshOperations.Scale:
                    SendScalingData((ComponentScaling)operation, false);
                    break;
                default:
                    Debug.Log("Shouldn't reach here");
                    break;
            }
        }

        context.SendJson(new Message()
        {
            endOfData = true
        });
    }

    private void SendTranslationData(ComponentTranslation translation, bool shouldStore)
    {
        context.SendJson(new Message()
        {
            transformType = translation.GetOperationType(),
            storeOperation = shouldStore,
            indices = translation.indices,
            translation = translation.amount
        });
    }

    private void SendRotationData(ComponentRotation rotation, bool shouldStore)
    {
        context.SendJson(new Message()
        {
            transformType = rotation.GetOperationType(),
            storeOperation = shouldStore,
            indices = rotation.indices,
            rotation = rotation.amount
        });
    }

    private void SendScalingData(ComponentScaling scaling, bool shouldStore)
    {
        context.SendJson(new Message()
        {
            transformType = scaling.GetOperationType(),
            storeOperation = shouldStore,
            indices = scaling.indices,
            scale = scaling.scale
        });
    }
    #endregion

    #region recieving
    private void CacheTranslationOperation(int[] indices, Vector3 amount)
    {
        ComponentTranslation operation = new ComponentTranslation(indices);
        operation.AddOffsetAmount(amount);
        em.CacheOperation(operation);
        cacheSize++;
    }

    private void CacheRotationOperation(int[] indices, Quaternion amount)
    {
        ComponentRotation operation = new ComponentRotation(indices);
        operation.AddOffsetAmount(amount);
        em.CacheOperation(operation);
        cacheSize++;
    }

    private void CacheScalingOperation(int[] indices, Vector3 amount)
    {
        ComponentScaling operation = new ComponentScaling(indices);
        operation.AddOffsetAmount(amount);
        em.CacheOperation(operation);
        cacheSize++;
    }

    private void CacheAndExectueTranslationOperation(int[] indices, Vector3 amount)
    {
        CacheTranslationOperation(indices, amount);
        em.meshOperationCache.operations[cacheSize - 1].Execute(em);
    } 

    private void CacheAndExecuteRotationOperation(int[] indices, Quaternion amount)
    {
        CacheRotationOperation(indices, amount);
        em.meshOperationCache.operations[cacheSize - 1].Execute(em);
    }

    private void CacheAndExecuteScalingOperation(int[] indices, Vector3 amount)
    {
        CacheScalingOperation(indices, amount);
        em.meshOperationCache.operations[cacheSize - 1].Execute(em);
    }

    #endregion
    public struct Message
    {
        public MeshOperations transformType;

        public bool storeOperation;
        public bool cacheEmpty;
        public bool cacheBacklog;

        public int[] indices;
        public Vector3 translation;
        public Quaternion rotation;
        public Vector3 scale;

        public bool endOfData;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();

        if(m.cacheEmpty)
        {
            if(cacheSize > 0)
            {
                SendAllOperationsCacheData();
            }
            return;
        }

        if(m.storeOperation)
        {
            if(m.transformType == MeshOperations.Translate)
                CacheTranslationOperation(m.indices, m.translation);
            else if(m.transformType == MeshOperations.Rotate)
                CacheRotationOperation(m.indices, m.rotation);
            else if(m.transformType == MeshOperations.Scale)
                CacheScalingOperation(m.indices, m.scale);

            awaitingData = false;
        }
        else
        {
            if (!awaitingData)
                return;

            if (m.transformType == MeshOperations.Translate)
                CacheAndExectueTranslationOperation(m.indices, m.translation);
            else if (m.transformType == MeshOperations.Rotate)
                CacheAndExecuteRotationOperation(m.indices, m.rotation);
            else if (m.transformType == MeshOperations.Scale)
                CacheAndExecuteScalingOperation(m.indices, m.scale);

        }

        if (m.endOfData)
        {
            awaitingData = false;
        }
    }

}
