using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using Unity.VisualScripting;

public class PopulateObjectLibrary : MonoBehaviour
{
    // This should be set to the Object button prefab
    public GameObject buttonPrefab;

    // This should be set to the SpawnObjectAtRay component atttached to one of the hands
    public SpawnObjectAtRay spawnScript;

    // These lists should be populated with all of the objects that are expected to appear
    // in the toolbox along with their icon prefabs
    public List<GameObject> objectPrefabs = new List<GameObject>();
    public List<GameObject> iconPrefabs = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < objectPrefabs.Count; i++)
            InstantiateButton(buttonPrefab, objectPrefabs[i], iconPrefabs[i], this.gameObject.transform);
    }

    // Instantiate a button and set it's prefab
    private void InstantiateButton(GameObject buttonPrefab, GameObject objectPrefab, GameObject iconPrefab, Transform parent)
    {
        // Instantiate the new button, set the text, and set the icon prefab
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(objectPrefab.name);
        newButton.GetComponentInChildren<SetPrefabIcon>().prefab = iconPrefab;

        // Create a new Unity action and add it as a listener to the buttons OnClicked event
        UnityAction<GameObject> action = new UnityAction<GameObject>(TriggerObjectSpawn);
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(objectPrefab));
    }

    // OnClicked event that triggers when the button is pressed
    // Sends the object prefab for the new buttons object to SpawnObjectAtRay when pressed
    void TriggerObjectSpawn(GameObject objectPrefab)
    {
      Debug.Log("TriggerObjectSpawn");
      Debug.Log(objectPrefab);
        spawnScript.RaySpawnToggle(Instantiate(objectPrefab));
    }
}
