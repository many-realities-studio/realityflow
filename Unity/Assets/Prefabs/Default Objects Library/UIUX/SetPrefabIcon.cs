using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPrefabIcon : MonoBehaviour
{
    public GameObject prefab { get; set; }

    // Instantiates an special prefab version of the object that has special components removed
    // Said instantiated prefab acts as the icon for that objects button in the toolbox
    public void Start()
    {
        GameObject newButtonIcon = Instantiate(prefab, transform.position + new Vector3(0f, 0.025f, 0f), Quaternion.identity, this.gameObject.transform);
        newButtonIcon.transform.localScale *= 15f;
    }
}
