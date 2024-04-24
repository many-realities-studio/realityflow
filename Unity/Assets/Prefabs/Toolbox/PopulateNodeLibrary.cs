using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;

public class PopulateNodeLibrary : MonoBehaviour
{
    // This should be set to the Object button prefab
    public GameObject buttonPrefab;

    // This should be set to the SpawnObjectAtRay component atttached to one of the hands
    public SpawnNodeAtRay spawnScript;

    // List that will hold each type of node that will be present in the node library
    private List <string> nodeList = new List<string>();
    private string currentNode;

    // Awake is called when object is enabled
    void Awake()
    {
        // For now, add all available nodes the ugly way.
        nodeList.Add("TextNode");
        nodeList.Add("ModalNode");
        nodeList.Add("PrintNode");
        //nodeList.Add("FloatNode");
        //nodeList.Add("IntNode");
        // nodeList.Add("BoolNode");
        nodeList.Add("ConditionalNode");
        nodeList.Add("StartNode");
        //nodeList.Add("GameObjectManipulationNode");
        //nodeList.Add("ColorNode");
        nodeList.Add("GameObjectNode");

        // Instantiate a button for each node in nodeList
        foreach (string node in nodeList)
        {
            InstantiateButton(buttonPrefab, this.gameObject.transform, node);
            buttonPrefab.name = node;
        }
    }

    // Instantiate a button and set the node associated with it
    private void InstantiateButton(GameObject buttonPrefab, Transform parent, string currentNode)
    {
        // Instantiate the new button and set the text
        GameObject newButton = Instantiate(buttonPrefab, parent);
        newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(currentNode);

        // Create a new Unity action and add it as a listener to the buttons OnClicked event
        UnityAction<string> action = new UnityAction<string>(TriggerNodeSpawn);
        Debug.Log("Assigning node action " + currentNode);
        newButton.GetComponent<PressableButton>().OnClicked.AddListener(() => action(currentNode));
    }

    // OnClicked event that triggers when the button is pressed
    // Sends the node associated with the button to SpawnNodeAtRay on the hand
    void TriggerNodeSpawn(string nodeName)
    {
        Debug.Log(nodeName);
        spawnScript.RaySpawnToggle(nodeName);
        GetObjectName.instance.aName = nodeName;
        
    }
}
