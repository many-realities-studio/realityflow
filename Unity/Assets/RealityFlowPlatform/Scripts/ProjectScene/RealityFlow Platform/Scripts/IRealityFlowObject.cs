using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine;

public interface IRealityFlowObject
{
    string uuid { get; set; }
    // Server-assigned identity for object
    string type { get; set; }
    //float[] position;
    //float[] rotation;

    // Reference to object in the scene, not serialized

    GameObject gameObject { get;  }

    Transform transform { get;  }

    SerializableMeshInfo smi { get; set; }


    //public Save()

}
