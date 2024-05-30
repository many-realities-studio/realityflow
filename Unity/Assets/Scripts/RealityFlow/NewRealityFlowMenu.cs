using Behaviours;
using Packages.realityflow_package.Runtime.scripts;
using Packages.realityflow_package.Runtime.scripts.Structures.Actions;
using RealityFlow.Plugin.Scripts;
using System;
//using System.Web.Security;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;


public class NewRealityFlowMenu : MonoBehaviour
{
   //private string _Url = "ws://plato.mrl.ai:8999";
    private string _Url = "wss://api.realityflow.io"; // the one we are using
    private const string Url = "ws://a73c9fa8.ngrok.io";

    // Keyboard
    public TouchScreenKeyboard keyboard;
    // View parameters
    private string uName;
    private string pWord;
    private string tempUName;
    private string tempPWord;
    private string projectName;
    private string userToInvite;
    private bool userIsGuest = false;

    private string openProjectId;
    public delegate void VRPhotonJoinHandler(string projectId);

    public delegate void VRPhotonLeaveHandler();

    public static event VRPhotonJoinHandler vrJoinProjectEvent;

    public static event VRPhotonLeaveHandler vrLeaveProjectEvent;

    //private Dictionary<EWindowView, ChangeView> _ViewDictionary = new Dictionary<EWindowView, ChangeView>();

    //private delegate void ChangeView();

    private IList<FlowProject> _ProjectList = null;
    public List<FlowVSGraph> VSGraphList;
    private string newVSGraphName;
    public Dropdown vsDeleteGraphDropdown; 

    [Header("Panels")]
    public GameObject realityFlowMenuPanel;
    public GameObject guestUserComfirmationPanel;
    public GameObject guestUserHubPanel;
    public GameObject guestUserProjectPanel;
    public GameObject createVSGraphView;
    public GameObject deleteVSGraphView;
    public GameObject createObjectView;
    public GameObject deleteObjectView;
    
    [Header("GameObject Parents")]
    public GameObject deleteVSGraphButtonCollection;

    [Header("Prefabs")]
    // Prefabs for instantiation
    public GameObject vsGraphDeleteButton;

    [Header("TextFields")]
    TMP_InputField inputField = null;
         
    public GameObject FirstObjectCreationStep;
    public GameObject FinalObjectCreationStep;
    private string ObjectName;
    private Vector3 ObjectPosition = new Vector3(0, 0, 0);
    private Vector3 ObjectScale = new Vector3(1, 1, 1);
    private Vector4 ObjectRotation = new Vector4(0, 0, 0, 1);
    private string ObjectPrefab;

    [Header("Object Deletion")]
    public List<FlowTObject> objectList;
    public Dropdown deleteObjectDropdown;

    public Transform spawnRelativeTransform;
    public GameObject VRWhiteBoard;

    public void OpenSystemKeyboard()
    {
        Debug.Log("Open Keyboard");
        keyboard =  TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }
    

    public void Request()
    {
        var cam = Camera.main.transform;

        // NetworkSpawnManager.Find(this).SpawnWithRoomScope(VRWhiteBoard);
        // transform.position = cam.TransformPoint(spawnRelativeTransform.localPosition);
        // transform.rotation = cam.rotation * spawnRelativeTransform.localRotation;
        gameObject.SetActive(true);
    }
    public void Update() 
    {
        if (keyboard != null)
        {
            openProjectId = keyboard.text;
            // Do stuff with keyboardText
        }
        else
        {
            inputField = (TMP_InputField)FindObjectOfType(typeof(TMP_InputField));
            if(inputField != null)
                openProjectId = inputField.text.ToString();
        }

        // if (FlowWebsocket.websocket != null)
        // {
        //     Unity.EditorCoroutines.Editor.EditorCoroutineUtility.StartCoroutine(Operations._FlowWebsocket.ReceiveMessage(), Operations._FlowWebsocket);
        // }
    }

    // This function setups the guest user account by creating random information for it.
    public void SetupGuest()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var uNameString = new char[8];
        var pWordString = new char[8];

        var rand = new System.Random();
        
        for (int i = 0; i < uNameString.Length; i++)
        {
            uNameString[i] = chars[rand.Next(chars.Length)];
            pWordString[i] = chars[rand.Next(chars.Length)];
        }

        tempUName = ("Guest-" + new string(uNameString));
        tempPWord = (new string(pWordString));
        //tempPWord = GeneratePassword(8, 0);

        Debug.Log("Generated username: " + tempUName);
        Debug.Log("Generated password: " + tempPWord);

        Operations.Register(tempUName, tempPWord, _Url, (sender, e) => { Debug.Log(e.message); });

        userIsGuest = true;
        
        // Show Guest Confirm Window and turn off reality flow panel
        guestUserComfirmationPanel.SetActive(!guestUserComfirmationPanel.activeInHierarchy);
        realityFlowMenuPanel.SetActive(!realityFlowMenuPanel.activeInHierarchy);   
    }

    public void _ConfirmGuestLogin()
    {
        // Send the user to the Guest User Hub screen by logging them into their guest account
        ConfigurationSingleton.SingleInstance.CurrentUser = new FlowUser(tempUName, tempPWord);
        Operations.Login(ConfigurationSingleton.SingleInstance.CurrentUser, _Url, (_, e) =>
        {
            Debug.Log("login callback: " + e.message.WasSuccessful.ToString());
            if (e.message.WasSuccessful == true)
            {
                Operations.GetAllUserProjects(ConfigurationSingleton.SingleInstance.CurrentUser, (__, _e) =>
                {
                    // Project list should be empty, not sure if this is necessary.
                    _ProjectList = _e.message.Projects;
                    // Turn off confirmation panel and turn on guest hub view
                    guestUserHubPanel.SetActive(!guestUserHubPanel.activeInHierarchy);
                    guestUserComfirmationPanel.SetActive(!guestUserComfirmationPanel.activeInHierarchy);
                    Debug.Log("HIIIII");
                });
            }
            else
            {
                //Operations.DeleteUser(new FlowUser(tempUName, tempPWord));
                ConfigurationSingleton.SingleInstance.CurrentUser = null;
            }
        });

    }

    public void GuestUserJoinProject()
    {
        openProjectId = KeyboardManager.Instance.InputField.text.ToString();
        Debug.Log("Open Project Id: " + openProjectId);
        if(openProjectId != null) // let this be Join button if we have an openProjectId
        {
            Operations.OpenProject(openProjectId, ConfigurationSingleton.SingleInstance.CurrentUser, (_, e) =>
            {
                if (e.message.WasSuccessful == true)
                {
                    Debug.Log(e.message);
                    if (e.message.WasSuccessful == true)
                    {
                        ConfigurationSingleton.SingleInstance.CurrentProject = e.message.flowProject;
                        // Show GuestProjectHub


                        
                        guestUserProjectPanel.SetActive(!guestUserProjectPanel.activeInHierarchy);
                        Transform head = GameObject.Find("Main Camera").transform;
                        vrJoinProjectEvent?.Invoke(ConfigurationSingleton.SingleInstance.CurrentProject.Id);
                        
                        guestUserHubPanel.SetActive(!guestUserHubPanel.activeInHierarchy);
                    }
                }
            });
        }
    }
    public void GuestUserLogout()
    {
        // Send logout event to the server
        if (ConfigurationSingleton.SingleInstance.CurrentUser != null)
        {
            // TODO: Logic for deleting user after logout
            //FlowUser toDelete = ConfigurationSingleton.SingleInstance.CurrentUser;
            foreach (FlowTObject obj in FlowTObject.idToGameObjectMapping.Values)
            {
                if (obj.CanBeModified == true)
                {
                    Operations.CheckinObject(obj.Id, ConfigurationSingleton.SingleInstance.CurrentProject.Id, 
                                            ConfigurationSingleton.SingleInstance.CurrentUser.Username, (_, e) => { });
                }
            }
            Operations.DeleteUser(ConfigurationSingleton.SingleInstance.CurrentUser);

            userIsGuest = false;

            ConfigurationSingleton.SingleInstance.CurrentProject = null;
                // Tell Photon to disconnect from the room the user is in.
                vrLeaveProjectEvent?.Invoke();

                GameObject[] wbList;
                wbList = GameObject.FindGameObjectsWithTag("Canvas");

                // Clear all whiteboards in the scene.
                foreach (GameObject wb in wbList)
                {
                    GameObject runeTimeGraphObject = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(2).gameObject;
                    RealityFlowGraphView rfgv = runeTimeGraphObject.GetComponent<RealityFlowGraphView>();
                    rfgv.ClearWhiteBoard();
                }

                FlowTObject.RemoveAllObjectsFromScene();
                FlowVSGraph.RemoveAllGraphsFromScene();
            //window = EWindowView.LOGIN;
            realityFlowMenuPanel.SetActive(!realityFlowMenuPanel.activeInHierarchy);
            guestUserProjectPanel.SetActive(!guestUserProjectPanel.activeInHierarchy);
      
        }
    }

    public void ShowHideCreateVSGraph()
    {
        createVSGraphView.SetActive(!createVSGraphView.activeInHierarchy);
    }

    public void ShowHideDeleteVSGraph()
    {
        deleteVSGraphView.SetActive(!deleteVSGraphView.activeInHierarchy);
        //CreateDeleteVSGraphView();
    }

    public void ShowHideCreateObject()
    {
        createObjectView.SetActive(!createObjectView.activeInHierarchy);
        FirstObjectCreationStep.SetActive(true);
    }

    public void ShowHideDeleteObject()
    {
        deleteObjectView.SetActive(!deleteObjectView.activeInHierarchy);
    }


    public void CreateVSGraph()
    {
        inputField = (TMP_InputField)FindObjectOfType(typeof(TMP_InputField));
        string input = inputField.text.ToString();
        if(input == null)
        {
            Debug.Log("Could not find input field");
        }
        else if(input == "")
        {
            Debug.Log("Input field empty");
        }
        else
        {
            FlowVSGraph graph = new FlowVSGraph(input);
            Debug.Log(graph.Name);
            graph.Name = input;
            Debug.Log(graph);
            Debug.Log(JsonUtility.ToJson(graph));
            // Debug.Log(graph.Id);
            // Debug.Log(graph._id);

            Operations.CreateVSGraph(graph, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => Debug.Log(e.message));   
            createVSGraphView.SetActive(!createVSGraphView.activeInHierarchy);
        }
    }

    // // Delete VSGraph for VR Menu not working due to the created gameobjects not lining up
    // public void CreateDeleteVSGraphView()
    // {   
    //     float column = -0.016f;
    //     float columnWidth = 0.04f; 
    //     // Send the user to the Project Hub screen
    //     // if (userIsGuest)
    //     // {
    //     //     Debug.Log("Guest cannot delete VS Graph");
    //     // }
    //     // else
    //     // {
    //         // TODO: This info is likely incorrect for graphs, needs to be thought about
    //         foreach (FlowVSGraph currentFlowVSGraph in FlowVSGraph.idToVSGraphMapping.Values)
    //         {
    //              GameObject deleteVSGraphButtton = Instantiate(vsGraphDeleteButton, new Vector3(column, 0, 0), Quaternion.identity, deleteVSGraphButtonCollection.transform);
    //              // Figure out how to tie the id to this gameObject;
    //              deleteVSGraphButtton.AddComponent<FlowVSGraph_Monobehaviour>().underlyingFlowVSGraph = currentFlowVSGraph;
    //              deleteVSGraphButtton.name = currentFlowVSGraph.Name;
    //              column += columnWidth;
    //         }
    //     //}
    // }

    public void LoadGraphsToDelete()
    {
        deleteVSGraphView.SetActive(true);
        List<string> options = new List<string> ();
        VSGraphList.Clear();
        foreach (FlowVSGraph graph in FlowVSGraph.idToVSGraphMapping.Values)
        {
            options.Add(graph.Name);
            VSGraphList.Add(graph);
        }
        vsDeleteGraphDropdown.ClearOptions ();
        vsDeleteGraphDropdown.AddOptions(options);
    }

    public void DeleteGraph()
    {
        int index = vsDeleteGraphDropdown.value;
        Debug.Log(index);
        DeleteVSGraph(VSGraphList[index]);
        VSGraphList.Clear();
        deleteVSGraphView.SetActive(false);
    }

    private void DeleteVSGraph(FlowVSGraph currentFlowVSGraph)
    {
        Operations.DeleteVSGraph(currentFlowVSGraph.Id, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => { Debug.Log("Deleted VSGraph " + e.message); });
    }

    public void CreateObjectStep1()
    {
        inputField = (TMP_InputField)FindObjectOfType(typeof(TMP_InputField));
        ObjectName = inputField.text.ToString();
        if(ObjectName == null)
        {
            Debug.Log("Could not find input field");
        }
        else if(ObjectName == "")
        {
            Debug.Log("Input field empty");
        }
        else
        {
            FirstObjectCreationStep.SetActive(false);
            FinalObjectCreationStep.SetActive(true);
        }

    }

    public void CreateObject()
    {
        inputField = (TMP_InputField)FindObjectOfType(typeof(TMP_InputField));
        ObjectPrefab = inputField.text.ToString();
        if(ObjectPrefab == null)
        {
            Debug.Log("Could not find input field");
        }
        else if(ObjectPrefab == "")
        {
            Debug.Log("Input field empty");
        }
        else
        {
            FinalObjectCreationStep.SetActive(false);
            createObjectView.SetActive(!createObjectView.activeInHierarchy);
            FlowTObject createdGameObject = new FlowTObject(ObjectName, ObjectPosition, new Quaternion(ObjectRotation.x, ObjectRotation.y, ObjectRotation.z, ObjectRotation.w), ObjectScale, new Color(), ObjectPrefab);

            Operations.CreateObject(createdGameObject, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => Debug.Log(e.message));
        }
    }

    public void LoadObjectsToDelete()
    {
        deleteObjectView.SetActive(true);
        List<string> options = new List<string> ();
        objectList.Clear();
        foreach (FlowTObject flowTObject in FlowTObject.idToGameObjectMapping.Values)
        {
            options.Add(flowTObject.Name);
            objectList.Add(flowTObject);
        }
        deleteObjectDropdown.ClearOptions ();
        deleteObjectDropdown.AddOptions(options);
    }

    public void DeleteObject()
    {
        int index = deleteObjectDropdown.value;
        Debug.Log(index);
        DeleteObject(objectList[index]);
        objectList.Clear();
        deleteObjectView.SetActive(false);
    }

    private void DeleteObject(FlowTObject currentFlowObject)
    {
        Operations.DeleteObject(currentFlowObject.Id, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => { Debug.Log("Deleted Object " + e.message); });
    }
}
