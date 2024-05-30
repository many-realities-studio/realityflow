using Behaviours;
using Packages.realityflow_package.Runtime.scripts.Messages;
using Packages.realityflow_package.Runtime.scripts.Messages.BehaviourMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.ObjectMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.RoomMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.UserMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;
using System.Threading.Tasks;

// Unity/GraphQL libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Globalization;

namespace Packages.realityflow_package.Runtime.scripts
{
    public delegate void NotifyGraphUpdate();

    public delegate void RunGraphHandler(string vsGraphId);

    public delegate void DeleteGraphHandler(string vsGraphId);

    public delegate void MobileCheckInHandler(string mobileObjectID);

    /// <summary>
    /// The purpose of this class is to provide a wrapper for the UnityPlugin,
    /// allowing for easy use with the networking tools
    /// </summary>
    public static class Operations
    {
        public static FlowWebsocket _FlowWebsocket { get; private set; }

        public static event NotifyGraphUpdate updateRFGV;

        public static event RunGraphHandler runVSGraph;

        public static event DeleteGraphHandler deleteVSGraph;

        public static event MobileCheckInHandler mobileObjectCheckIn;

        static Operations()
        {
            // Set up default behavior response to events

            // Set up Object updates
            ReceivedMessage.AddEventHandler(typeof(CreateObject_Received), false, _CreateObject);
            ReceivedMessage.AddEventHandler(typeof(DeleteObject_Received), false, _DeleteObject);


            // Set up Project updates
            ReceivedMessage.AddEventHandler(typeof(CreateProject_Received), false, _CreateProject);
            ReceivedMessage.AddEventHandler(typeof(OpenProject_Received), false, _OpenProject);
            ReceivedMessage.AddEventHandler(typeof(LeaveProject_Received), false, _LeaveProject);
            ReceivedMessage.AddEventHandler(typeof(DeleteProject_Received), false, _DeleteProject);

            // Set up Room updates
            ReceivedMessage.AddEventHandler(typeof(UserLeftRoom_Received), false, _UserLeftRoom);

            // Set up User updates
            ReceivedMessage.AddEventHandler(typeof(LogoutUser_Received), false, _LogoutUser);
            ReceivedMessage.AddEventHandler(typeof(RegisterUser_Received), false, _RegisterUser);
            ReceivedMessage.AddEventHandler(typeof(DeleteUser_Received), false, _DeleteUser);

            // Set up Behaviour updates
            ReceivedMessage.AddEventHandler(typeof(CreateBehaviour_Received), false, _CreateBehaviour);
            ReceivedMessage.AddEventHandler(typeof(DeleteBehaviour_Received), false, _DeleteBehaviour);
            ReceivedMessage.AddEventHandler(typeof(UpdateBehaviour_Received), false, _UpdateBehaviour);

            // Set up Graph updates
            ReceivedMessage.AddEventHandler(typeof(CreateVSGraph_Received), false, _CreateVSGraph);
            ReceivedMessage.AddEventHandler(typeof(DeleteVSGraph_Received), false, _DeleteVSGraph);
            ReceivedMessage.AddEventHandler(typeof(UpdateVSGraph_Received), false, _UpdateVSGraph);
            ReceivedMessage.AddEventHandler(typeof(FinalizedUpdateVSGraph_Received), false, _FinalizedUpdateVSGraph);
            ReceivedMessage.AddEventHandler(typeof(RunVSGraph_Received), false, _RunVSGraph);
        }

        #region UserOperations

        public static void Login(FlowUser flowUser, string url, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            bool connectionSuccessful = ConnectToServer(url, flowUser);

            if (FlowWebsocket.websocket.ReadyState == WebSocketSharp.WebSocketState.Open)
            {
                Login_SendToServer loginMessage = new Login_SendToServer(flowUser);
                FlowWebsocket.SendMessage(loginMessage);

                ConfigurationSingleton.SingleInstance.CurrentUser = flowUser;

                ReceivedMessage.AddEventHandler(typeof(LoginUser_Received), true, callbackFunction);
            }
        }

        public static void Logout(FlowUser flowUser)
        {
            Logout_SendToServer logoutMessage = new Logout_SendToServer(flowUser);
            FlowWebsocket.SendMessage(logoutMessage);

            _FlowWebsocket.Disconnect();
        }

        public static void Register(string username, string password, string url, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            bool connectionSuccssfull = ConnectToServer(url);

            if (FlowWebsocket.websocket.ReadyState == WebSocketSharp.WebSocketState.Open)
            {
                RegisterUser_SendToServer register = new RegisterUser_SendToServer(new FlowUser(username, password));
                FlowWebsocket.SendMessage(register);

                ReceivedMessage.AddEventHandler(typeof(RegisterUser_Received), true, callbackFunction);
            }
        }

        public static void DeleteUser(FlowUser flowUser)
        {
            DeleteUser_SendToServer deleteUserMessage = new DeleteUser_SendToServer(flowUser);
            FlowWebsocket.SendMessage(deleteUserMessage);
        }

        #endregion UserOperations

        #region ObjectOperations


        /// <summary>
        /// This is a documentation string. 
        /// </summary>
        /// <param name="flowObject">This is the object to be created</param> 
        public static void CreateObject(FlowTObject flowObject, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            //            graphqlClient_Editor createObject = ScriptableObject.CreateInstance<graphqlClient_Editor>();
            //createObject.CreateObject(flowObject, projectId);


            // This was how it used to be done before GRaphQL
            // CreateObject_SendToServer createObject = new CreateObject_SendToServer(flowObject, projectId);
            // FlowWebsocket.SendMessage(createObject);

            ReceivedMessage.AddEventHandler(typeof(CreateObject_Received), true, callbackFunction);
        }

        public static void UpdateObject(FlowTObject flowObject, FlowUser flowUser, string projectId, string username, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            UpdateObject_SendToServer _updateObject = new UpdateObject_SendToServer(flowObject, projectId, username);
            FlowWebsocket.SendMessage(_updateObject);

            ReceivedMessage.AddEventHandler(typeof(UpdateObject_Received), true, callbackFunction);
        }

        public static void DeleteObject(string idOfObjectToDelete, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            //            graphqlClient_Editor deleteObject = ScriptableObject.CreateInstance<graphqlClient_Editor>();
            //deleteObject.DeleteObject(idOfObjectToDelete, projectId);

            // This was how it used to be done before GRaphQL Implementation
            // DeleteObject_SendToServer _deleteObject = new DeleteObject_SendToServer(projectId, idOfObjectToDelete);
            // FlowWebsocket.SendMessage(_deleteObject);

            ReceivedMessage.AddEventHandler(typeof(DeleteObject_Received), true, callbackFunction);
        }

        #endregion ObjectOperations


        #region VSGraphOperations

        public static void CreateVSGraph(FlowVSGraph flowVSGraph, /*FlowUser flowUser,*/ string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {   // All of this branch's vsGraph logic is untouched. We didnt get it work 100%,
            // so check Demo-packageUnstable branch for code changes that we tried to get it to work with

            // graphqlClient_Editor createVSGraph = ScriptableObject.CreateInstance<graphqlClient_Editor>();
            // createVSGraph.CreateVSGraph(flowVSGraph, projectId);

            string message = ("{\"FlowVSGraph\":" + JsonUtility.ToJson(flowVSGraph) + ",\"ProjectId\":\"" + projectId + "\",\"MessageType\":\"CreateVSGraph\"}");

            FlowWebsocket.SendGraphMessage(message);

            ReceivedMessage.AddEventHandler(typeof(CreateVSGraph_Received), true, callbackFunction);
        }

        public static void UpdateVSGraph(FlowVSGraph flowVSGraph, FlowUser flowUser, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            string message = ("{\"FlowVSGraph\":" + JsonUtility.ToJson(flowVSGraph) + ",\"ProjectId\":\"" + projectId + "\",\"MessageType\":\"UpdateVSGraph\"}");
            FlowWebsocket.SendGraphMessage(message);

            ReceivedMessage.AddEventHandler(typeof(UpdateVSGraph_Received), true, callbackFunction);
        }

        public static void FinalizedUpdateVSGraph(FlowVSGraph flowVSGraph, FlowUser flowUser, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            // graphqlClient_Editor finalizedUpdateVSGraph = ScriptableObject.CreateInstance<graphqlClient_Editor>();
            // finalizedUpdateVSGraph.FinalizedUpdateVSGraph(flowVSGraph, projectId);

            //UpdateVSGraph_SendToServer updateVSGraph = new UpdateVSGraph_SendToServer(flowVSGraph, /*flowUser,*/ projectId); // TODO: format string msg
            // Debug.Log(JsonUtility.ToJson(flowVSGraph.edges));
            string message = ("{\"FlowVSGraph\":" + JsonUtility.ToJson(flowVSGraph) + ",\"ProjectId\":\"" + projectId + "\",\"MessageType\":\"FinalizedUpdateVSGraph\"}");
            
            // FlowWebsocket.SendGraphMessage(message);
            // ReceivedMessage.AddEventHandler(typeof(FinalizedUpdateVSGraph_Received), true, callbackFunction);
        }

        public static void DeleteVSGraph(string idOfVSGraphToDelete, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            // graphqlClient_Editor deleteVSGraph = ScriptableObject.CreateInstance<graphqlClient_Editor>();
            // deleteVSGraph.DeleteVSGraph(idOfVSGraphToDelete, projectId);

            //DeleteVSGraph_SendToServer deleteVSGraph = new DeleteVSGraph_SendToServer(projectId, idOfVSGraphToDelete); // TODO: format string msg
            string message = ("{\"VSGraphId\":\"" + idOfVSGraphToDelete + "\",\"ProjectId\":\"" + projectId + "\",\"MessageType\":\"DeleteVSGraph\"}");
            FlowWebsocket.SendGraphMessage(message);

            ReceivedMessage.AddEventHandler(typeof(DeleteVSGraph_Received), true, callbackFunction);
        }

        public static void UpdateNodeView(NodeView nodeView, FlowUser flowUser, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            string boolValue = (nodeView.CanBeModified ? "true" : "false");
            string message = ("{\"FlowNodeView\": {\"CanBeModified\":" + boolValue + ",\"LocalPos\":" + JsonUtility.ToJson(nodeView.localPos) + ",\"NodeGUID\":\"" + nodeView.nodeGUID + "\"},\"ProjectId\":\"" + projectId + "\",\"MessageType\":\"UpdateNodeView\"}");
            FlowWebsocket.SendGraphMessage(message);

            ReceivedMessage.AddEventHandler(typeof(UpdateVSGraph_Received), true, callbackFunction);
        }

        public static void RunVSGraph(string idOfVSGraphToRun, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            string message = ("{\"VSGraphId\":\"" + idOfVSGraphToRun + "\",\"ProjectId\":\"" + projectId + "\",\"MessageType\":\"RunVSGraph\"}");
            FlowWebsocket.SendGraphMessage(message);

            ReceivedMessage.AddEventHandler(typeof(RunVSGraph_Received), true, callbackFunction);
        }

        #endregion VSGraphOperations

        #region BehaviourOperations

        public static async void CreateBehaviour(FlowBehaviour behaviour, string projectId, List<string> behavioursToLinkTo, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            // GraphQL -- Not finished
            //graphqlClient_Editor createBehaviour = new graphqlClient_Editor();
            //createBehaviour.CreateBehaviour(behaviour, projectId, behavioursToLinkTo);
            //string IdToBeLinked = await createBehaviour.CreateBehaviour(behaviour, projectId, behavioursToLinkTo);

            // if(IdToBeLinked != null){
            //     behaviour.BehaviourName = behaviour.flowAction.ActionType;
            //     //behavioursToLinkTo.Add(IdToBeLinked);
            //     __CreateBehaviour(behaviour, projectId, behavioursToLinkTo, IdToBeLinked);

            //}


            CreateBehaviour_SendToServer _createBehaviour = new CreateBehaviour_SendToServer(behaviour, projectId, behavioursToLinkTo);

            FlowWebsocket.SendMessage(_createBehaviour);
            ReceivedMessage.AddEventHandler(typeof(CreateBehaviour_Received), true, callbackFunction);

        }

        public static void DeleteBehaviour(List<string> behaviourIds, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            //GraphQL
            // graphqlClient_Editor deleteBehaviour = new graphqlClient_Editor();
            // deleteBehaviour.DeleteBehaviour(behaviourIds, projectId);
            //__DeleteBehaviour(behaviourIds);
            DeleteBehaviour_SendToServer deleteBehaviour = new DeleteBehaviour_SendToServer(behaviourIds, projectId);
            FlowWebsocket.SendMessage(deleteBehaviour);

            ReceivedMessage.AddEventHandler(typeof(DeleteBehaviour_Received), true, callbackFunction);
        }

        public static void UpdateBehaviour(FlowBehaviour behaviour, string projectId, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            UpdateBehaviour_SendToServer updateBehaviour = new UpdateBehaviour_SendToServer(behaviour, projectId);
            FlowWebsocket.SendMessage(updateBehaviour);

            ReceivedMessage.AddEventHandler(typeof(UpdateBehaviour_Received), true, callbackFunction);
        }

        #endregion BehaviourOperations

        #region ProjectOperations

        public static void CreateProject(FlowProject flowProject, FlowUser flowUser, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            CreateProject_SendToServer createProject = new CreateProject_SendToServer(flowProject, flowUser);
            FlowWebsocket.SendMessage(createProject);

            ReceivedMessage.AddEventHandler(typeof(CreateProject_Received), true, callbackFunction);
        }

        public static void DeleteProject(FlowProject flowProject, FlowUser flowUser, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            DeleteProject_SendToServer deleteProject = new DeleteProject_SendToServer(flowProject, flowUser);
            FlowWebsocket.SendMessage(deleteProject);

            ReceivedMessage.AddEventHandler(typeof(DeleteProject_Received), true, callbackFunction);
        }

        public static void OpenProject(string projectId, FlowUser flowUser, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            OpenProject_SendToServer openProject = new OpenProject_SendToServer(projectId, flowUser);
            FlowWebsocket.SendMessage(openProject);

            ReceivedMessage.AddEventHandler(typeof(OpenProject_Received), true, callbackFunction);
        }

        public static void LeaveProject(string projectId, FlowUser flowUser, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            LeaveProject_SendToServer leaveProject = new LeaveProject_SendToServer(projectId, flowUser);
            FlowWebsocket.SendMessage(leaveProject);

            ReceivedMessage.AddEventHandler(typeof(LeaveProject_Received), true, callbackFunction);
        }

        public static void GetAllUserProjects(FlowUser flowUser, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            GetAllUserProjects_SendToServer getAllUserProjects = new GetAllUserProjects_SendToServer(flowUser);
            FlowWebsocket.SendMessage(getAllUserProjects);

            ReceivedMessage.AddEventHandler(typeof(GetAllUserProjects_Received), true, callbackFunction);
        }

        #endregion ProjectOperations

        #region RoomMessages

        public static void JoinRoom(string projectId, FlowUser flowUser, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            JoinRoom_SendToServer joinRoom = new JoinRoom_SendToServer(projectId, flowUser);
            FlowWebsocket.SendMessage(joinRoom);

            ReceivedMessage.AddEventHandler(typeof(JoinRoom_Received), true, callbackFunction);
        }

        #endregion RoomMessages

        #region Checkout system messages

        public static void CheckoutObject(string objectID, string projectID, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            CheckoutObject_SendToServer checkoutObject = new CheckoutObject_SendToServer(objectID, projectID, ConfigurationSingleton.SingleInstance.CurrentUser.Username);
            FlowWebsocket.SendMessage(checkoutObject);

            ReceivedMessage.AddEventHandler(typeof(CheckoutObject_Received), true, callbackFunction);
        }

        public static async void CheckinObject(string objectID, string projectID, string username, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            // CheckIn will be called updateObject for GraphQL operations
            // TODO: I had to check this out as it was not working properly and we needed to merge, be sure to fix this functionality.

            //            graphqlClient_Editor updateObject = ScriptableObject.CreateInstance<graphqlClient_Editor>();
            //string checkedIn = await updateObject.UpdateObject(objectID, projectID, username);
            // TODO:NEED TO FIX GRAPHQL
            //if(checkedIn != null){
            //    Debug.Log("Getting message type: CheckinObject from GraphQL");
            //    FlowTObject.idToGameObjectMapping[objectID].CanBeModified = false;
            //    mobileObjectCheckIn?.Invoke(objectID);
            //}else{
            //    Debug.Log("Check-In with GraphQL didn't work!");
            //}

            // This was how it used to be done.
            // CheckinObject_SendToServer checkinObject = new CheckinObject_SendToServer(objectID, projectID, ConfigurationSingleton.SingleInstance.CurrentUser.Username);
            // FlowWebsocket.SendMessage(checkinObject);

            // ReceivedMessage.AddEventHandler(typeof(CheckinObject_Received), true, callbackFunction);
        }

        public static void CheckoutVSGraph(string vsGraphID, string projectID, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            CheckoutVSGraph_SendToServer checkoutVSGraph = new CheckoutVSGraph_SendToServer(vsGraphID, projectID);
            FlowWebsocket.SendMessage(checkoutVSGraph);

            ReceivedMessage.AddEventHandler(typeof(CheckoutVSGraph_Received), true, callbackFunction);
        }

        public static void CheckinVSGraph(string vsGraphID, string projectID, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            CheckinVSGraph_SendToServer checkinVSGraph = new CheckinVSGraph_SendToServer(vsGraphID, projectID);
            FlowWebsocket.SendMessage(checkinVSGraph);

            ReceivedMessage.AddEventHandler(typeof(CheckinVSGraph_Received), true, callbackFunction);
        }

        public static void CheckoutNodeView(NodeView nodeView, string projectID, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            string boolValue = (nodeView.CanBeModified ? "true" : "false");
            string message = ("{\"FlowNodeView\": {\"CanBeModified\":" + boolValue + ",\"LocalPos\":" + JsonUtility.ToJson(nodeView.localPos) + ",\"NodeGUID\":\"" + nodeView.nodeGUID + "\"},\"ProjectId\":\"" + projectID + "\",\"MessageType\":\"CheckoutNodeView\"}");
            FlowWebsocket.SendGraphMessage(message);

            ReceivedMessage.AddEventHandler(typeof(CheckoutNodeView_Received), true, callbackFunction);
        }

        public static void CheckinNodeView(string nodeGUID, string projectID, ReceivedMessage.ReceivedMessageEventHandler callbackFunction)
        {
            CheckinNodeView_SendToServer checkinNodeView = new CheckinNodeView_SendToServer(nodeGUID, projectID);
            FlowWebsocket.SendMessage(checkinNodeView);

            ReceivedMessage.AddEventHandler(typeof(CheckinNodeView_Received), true, callbackFunction);
        }

        #endregion Checkout system messages

        /// <summary>
        /// Establish a connection to the server
        /// </summary>
        /// <param name="url"></param>
        public static bool ConnectToServer(string url, FlowUser flowUser = null)
        {
            if (flowUser == null)
            {
                flowUser = new FlowUser("God", "Jesus");
            }
            _FlowWebsocket = new FlowWebsocket(url, flowUser);

            return _FlowWebsocket.IsConnected;
        }

        // TODO: Fill out default behavior

        #region Default actions taken after receiving messages (These happen no matter what the user does)

        #region Object messages received

        private static void _CreateObject(object sender, BaseReceivedEventArgs eventArgs)
        {
        }

        private static void _DeleteObject(object sender, BaseReceivedEventArgs eventArgs)
        {
            if (eventArgs.message.WasSuccessful == true)
            {
                GameObject gameObject = FlowTObject.idToGameObjectMapping[eventArgs.message.DeletedObjectId].AttachedGameObject;

                FlowTObject.idToGameObjectMapping.Remove(eventArgs.message.DeletedObjectId);

                UnityEngine.Object.DestroyImmediate(gameObject);
            }

            Debug.Log("Delete Object: " + eventArgs.message.WasSuccessful);
        }

        #endregion Object messages received

        #region VSGraph messages received

        private static void _CreateVSGraph(object sender, BaseReceivedEventArgs eventArgs)
        {
        }

        private static void _DeleteVSGraph(object sender, BaseReceivedEventArgs eventArgs)
        {
            if (eventArgs.message.WasSuccessful == true)
            {
                deleteVSGraph?.Invoke(eventArgs.message.DeletedVSGraphId);

                GameObject gameObject = FlowVSGraph.idToVSGraphMapping[eventArgs.message.DeletedVSGraphId].AttachedGameObject;

                FlowVSGraph.idToVSGraphMapping.Remove(eventArgs.message.DeletedVSGraphId);

                UnityEngine.Object.DestroyImmediate(gameObject);
            }

            Debug.Log("Delete VSGraph: " + eventArgs.message.WasSuccessful);
        }

        private static void _UpdateVSGraph(object sender, BaseReceivedEventArgs eventArgs)
        {
            updateRFGV?.Invoke();
        }

        private static void _FinalizedUpdateVSGraph(object sender, BaseReceivedEventArgs eventArgs)
        {
            updateRFGV?.Invoke();
        }

        private static void _RunVSGraph(object sender, BaseReceivedEventArgs eventArgs)
        {
            runVSGraph?.Invoke(eventArgs.message.VSGraphId);
        }

        #endregion VSGraph messages received

        #region Behaviour messages received

        private static void _CreateBehaviour(object sender, BaseReceivedEventArgs eventArgs)
        {
            if (eventArgs.message.WasSuccessful == true)
            {
                Debug.Log("Success creating behaviour " + eventArgs.message.flowBehaviour.TypeOfTrigger);

                BehaviourEventManager.CreateNewBehaviour(eventArgs.message.flowBehaviour);

                foreach (string behaviourToLinkId in eventArgs.message.behavioursToLinkTo)
                {
                    BehaviourEventManager.LinkBehaviours(eventArgs.message.flowBehaviour.Id, behaviourToLinkId);
                }
            }

            Debug.Log("Number of behaviours in bem = " + BehaviourEventManager.BehaviourList.Count);
        }

        private static void _DeleteBehaviour(object sender, BaseReceivedEventArgs eventArgs)
        {
            // this is where things happen after a DeleteBehaviour message is deserialized
            if (eventArgs.message.WasSuccessful)
            {
                // for each behaviour id in behaviourIds, delete from behaviour list and from each object's interactablevents
                foreach (string id in eventArgs.message.BehaviourIds)
                {
                    FlowBehaviour fb = BehaviourEventManager.BehaviourList[id];

                    BehaviourEventManager.DeleteFlowBehaviour(fb.TriggerObjectId, fb.TriggerObjectId, fb);
                }
                Debug.Log("Successfully delete all behaviours in the chain");
                Debug.Log("Number of behaviours in bem = " + BehaviourEventManager.BehaviourList.Count);
            }
        }
        private static void _UpdateBehaviour(object sender, BaseReceivedEventArgs eventArgs)
        {
            if (eventArgs.message.WasSuccessful == true)
            {
                Debug.Log("Success updating behaviour " + eventArgs.message.flowBehaviour.TypeOfTrigger);

                BehaviourEventManager.UpdateBehaviour(eventArgs.message.flowBehaviour);
            }
        }

        #endregion Behaviour messages received

        #region Project messages received

        private static void _CreateProject(object sender, BaseReceivedEventArgs eventArgs)
        {
            ConfigurationSingleton.SingleInstance.CurrentProject = eventArgs.message.flowProject;

            Operations.OpenProject(ConfigurationSingleton.SingleInstance.CurrentProject.Id, ConfigurationSingleton.SingleInstance.CurrentUser, (_, e) =>
            {
                // Creating an Avatar Upon entering a newly created project
                if (e.message.WasSuccessful == true)
                {
                    ConfigurationSingleton.SingleInstance.CurrentProject = e.message.flowProject;
                    Debug.Log(e.message);
                }
                Debug.Log("opened project after create: " + e.message.WasSuccessful);
            });

        }

        private static void _OpenProject(object sender, BaseReceivedEventArgs eventArgs)
        {
            Debug.Log("Now in load project");
            ConfigurationSingleton.SingleInstance.CurrentProject = eventArgs.message.flowProject;

            ConfigurationSingleton asset = ScriptableObject.CreateInstance<ConfigurationSingleton>();
            asset.CurrentProject = ConfigurationSingleton.SingleInstance.CurrentProject;
            asset.CurrentUser = ConfigurationSingleton.SingleInstance.CurrentUser;
            #if UNITY_EDITOR
            AssetDatabase.CreateAsset(asset, "Assets/RealityFlowConfiguration.asset");
            AssetDatabase.SaveAssets();
            #endif
            if (eventArgs.message.WasSuccessful == true)
            {
                ConfigurationSingleton.SingleInstance.CurrentProject = eventArgs.message.flowProject;

                // Clear the behaviour list and BEM
                BehaviourEventManager.Clear();
                BehaviourEventManager.Initialize();

                Debug.Log("Number of behaviours in bem = " + BehaviourEventManager.BehaviourList.Count);

                if (eventArgs.message.flowProject.behaviourList == null)
                {
                    Debug.Log("Behaviourlist in this project is null.");
                }
                else
                {
                    Debug.Log("Behaviourlist in this project is not null.");
                }

                foreach (FlowBehaviour fb in eventArgs.message.flowProject.behaviourList)
                {
                    BehaviourEventManager.CreateNewBehaviour(fb);
                }

                Debug.Log("Number of behaviours in bem = " + BehaviourEventManager.BehaviourList.Count);
            }
        }

        private static void _LeaveProject(object sender, BaseReceivedEventArgs eventArgs)
        {
            if (eventArgs.message.WasSuccessful == true)
            {
                Debug.Log("Successfully left project");

                // Clear the behaviour list and BEM
                BehaviourEventManager.Clear();
                BehaviourEventManager.Initialize();
            }
            else
            {
                Debug.LogWarning("Unable to leave project.");
            }
        }

        private static void _DeleteProject(object sender, BaseReceivedEventArgs eventArgs)
        {
            if (eventArgs.message.WasSuccessful == true)
            {
                Debug.Log("Successfully deleted project");

                // Clear the behaviour list and BEM
                BehaviourEventManager.Clear();
                BehaviourEventManager.Initialize();
            }
            else
            {
                Debug.LogWarning("Unable to delete project.");
            }
        }

        #endregion Project messages received

        #region Room messages received

        private static void _UserLeftRoom(object sender, BaseReceivedEventArgs eventArgs)
        {
            Debug.Log("Room Alert: " + eventArgs.message.leftRoomMessage);

        }

        #endregion Room messages received

        #region User messages received

        private static void _LogoutUser(object sender, BaseReceivedEventArgs eventArgs)
        {
            ConfigurationSingleton.SingleInstance.CurrentProject = null;
            ConfigurationSingleton.SingleInstance.CurrentUser = null;
        }

        private static void _DeleteUser(object sender, BaseReceivedEventArgs eventArgs)
        {
            ConfigurationSingleton.SingleInstance.CurrentProject = null;
            ConfigurationSingleton.SingleInstance.CurrentUser = null;
            _FlowWebsocket.Disconnect();
        }

        private static void _RegisterUser(object sender, BaseReceivedEventArgs eventArgs)
        {
            _FlowWebsocket.Disconnect();
        }

        #endregion User messages received

        #endregion Default actions taken after receiving messages (These happen no matter what the user does)
    }
}
