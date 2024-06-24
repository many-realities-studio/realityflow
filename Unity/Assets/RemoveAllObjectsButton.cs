using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAllObjectsButton : MonoBehaviour
{
    // Start is called before the first frame update
    public void deleteAllObj()
    {
        List<GameObject> objectsToDespawn = new List<GameObject>(RealityFlowAPI.Instance.SpawnedObjects.Keys);

        // Include peer-scoped objects
        foreach (GameObject obj in objectsToDespawn)
        {
            if (obj != null)
            {
                RealityFlowAPI.Instance.DespawnObject(obj);
            }
        }
    }
}
