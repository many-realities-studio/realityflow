using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using Unity.VisualScripting;
using Ubiq.Spawning;
using Microsoft.MixedReality.GraphicsTools;

public class PopulateObjectLibrary : MonoBehaviour
{
    // This should be set to the Object button prefab
    public GameObject buttonPrefab;

    // This should be set to the SpawnObjectAtRay component atttached to one of the hands
    public RaycastLogger spawnScript;

    // Spawn the object as networked
    [SerializeField] private NetworkSpawnManager networkSpawnManager;

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
    private void InstantiateButton(GameObject buttonPrefab, GameObject objectPrefab,
        GameObject iconPrefab, Transform parent)
    {
        // Instantiate the new button, set the text, and set the icon prefab
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(objectPrefab.name);
        newButton.GetComponentInChildren<SetPrefabIcon>().prefab = iconPrefab;

        // Create a new Unity action and add it as a listener to the buttons OnClicked event
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(
            () => TriggerObjectSpawn(objectPrefab)
        );
        //newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(objectPrefab));
    }

    // OnClicked event that triggers when the button is pressed
    // Sends the object prefab for the new buttons object to SpawnObjectAtRay when pressed
    async Task TriggerObjectSpawn(GameObject objectPrefab)
    {
        Debug.Log("TriggerObjectSpawn");
        Debug.Log(spawnScript.GetVisualIndicatorPosition());

        // Use the prefab's default rotation
        Quaternion defaultRotation = objectPrefab.transform.rotation;
        // Spawn the object with the default rotation
        GameObject spawnedObject = await RealityFlowAPI.Instance.SpawnObject(objectPrefab.name, spawnScript.GetVisualIndicatorPosition() + new Vector3(0, 0.25f, 0), objectPrefab.transform.localScale, defaultRotation, RealityFlowAPI.SpawnScope.Room);
        RealityFlowAPI.Instance.LogActionToServer("Add Prefab" + spawnedObject.name.ToString(), new { prefabTransformPosition = spawnedObject.transform.localPosition, prefabTransformRotation = spawnedObject.transform.localRotation, prefabTransformScale = spawnedObject.transform.localEulerAngles});


        if(spawnedObject.GetComponent<Rigidbody>() != null)
        {
            spawnedObject.GetComponent<Rigidbody>().useGravity = true;

            StartCoroutine("setObjectToBeStill", spawnedObject);
        }
    }

    private IEnumerator setObjectToBeStill(GameObject spawnedObject)
    {
        yield return new WaitForSeconds(2);
        spawnedObject.GetComponent<Rigidbody>().useGravity = false;
        spawnedObject.GetComponent<Rigidbody>().isKinematic = true;
    }
}
