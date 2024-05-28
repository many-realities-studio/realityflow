//using Newtonsoft.Json;
using Packages.realityflow_package.Runtime.scripts.Messages;
using Packages.realityflow_package.Runtime.scripts.Messages.BehaviourMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.ObjectMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.RoomMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.UserMessages;
using Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages;

//using RealityFlow.Plugin.Scripts.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.realityflow_package.Runtime.scripts
{
    /// <summary>
    /// The purpose of this class is to parse messages that are received from the server
    /// into their respective representations in C#
    /// </summary>
    public class ReceivedMessageParser
    {
        public static Dictionary<string, Type> messageRouter = new Dictionary<string, Type>();

        /// <summary>
        /// Links all message types to their respective function. Note that this can be improved
        /// such that each message object adds themself to the dictionary
        /// </summary>
        static ReceivedMessageParser()
        {
            // Object Messages
            messageRouter.Add("CreateObject", typeof(CreateObject_Received));
            messageRouter.Add("DeleteObject", typeof(DeleteObject_Received));
            messageRouter.Add("UpdateObject", typeof(UpdateObject_Received));


            // Project Messages
            messageRouter.Add("CreateProject", typeof(CreateProject_Received));
            messageRouter.Add("DeleteProject", typeof(DeleteProject_Received));
            messageRouter.Add("FetchProjects", typeof(GetAllUserProjects_Received));
            messageRouter.Add("OpenProject", typeof(OpenProject_Received));
            messageRouter.Add("LeaveProject", typeof(LeaveProject_Received));

            // Room Messages
            messageRouter.Add("JoinRoom", typeof(JoinRoom_Received));
            messageRouter.Add("UserJoinedRoom", typeof(JoinRoom_Received));
            messageRouter.Add("UserLeftRoom", typeof(UserLeftRoom_Received));

            // User Messages
            messageRouter.Add("LoginUser", typeof(LoginUser_Received));
            messageRouter.Add("LogoutUser", typeof(LogoutUser_Received));
            messageRouter.Add("CreateUser", typeof(RegisterUser_Received));
            messageRouter.Add("DeleteUser", typeof(DeleteUser_Received));

            // Behaviour Messages
            messageRouter.Add("CreateBehaviour", typeof(CreateBehaviour_Received));
            messageRouter.Add("UpdateBehaviour", typeof(UpdateBehaviour_Received));
            messageRouter.Add("DeleteBehaviour", typeof(DeleteBehaviour_Received));

            // Checkout system messages
            messageRouter.Add("CheckinObject", typeof(CheckinObject_Received));
            messageRouter.Add("CheckoutObject", typeof(CheckoutObject_Received));
            messageRouter.Add("CheckinVSGraph", typeof(CheckinVSGraph_Received));
            messageRouter.Add("CheckoutVSGraph", typeof(CheckoutVSGraph_Received));
            messageRouter.Add("CheckinNodeView", typeof(CheckinNodeView_Received));
            messageRouter.Add("CheckoutNodeView", typeof(CheckoutNodeView_Received));

            // Visual Scripting Graph messages
            messageRouter.Add("CreateVSGraph", typeof(CreateVSGraph_Received));
            messageRouter.Add("DeleteVSGraph", typeof(DeleteVSGraph_Received));
            messageRouter.Add("UpdateVSGraph", typeof(UpdateVSGraph_Received));
            messageRouter.Add("FinalizedUpdateVSGraph", typeof(FinalizedUpdateVSGraph_Received));
            messageRouter.Add("UpdateNodeView", typeof(UpdateNodeView_Received));
            messageRouter.Add("RunVSGraph", typeof(RunVSGraph_Received));
        }

        /// <summary>
        /// Request to parse a message from a server into its respective c# representation
        /// </summary>
        /// <param name="message">The message from the server to parse</param>
        public static void Parse(string message)
        {
            string messageType = GetMessageType(message);
            if (messageRouter.ContainsKey(messageType))
            {
                dynamic obj = MessageSerializer.DesearializeObject(message, messageRouter[messageType]);
                obj.RaiseEvent();
            }
            else
            {
                Debug.LogError("Unknown received message type: " + messageType);
            }
        }

        /// <summary>
        /// Retrieve the message type from a message.
        /// </summary>
        /// <param name="messageToConvert">The message that should be converted into its C# representation</param>
        /// <returns></returns>
        public static string GetMessageType(string messageToConvert)
        {
            var deserializedObject = MessageSerializer.DesearializeObject(messageToConvert, typeof(BaseMessage));

            Debug.Log("Getting message type: " + deserializedObject.MessageType);

            return deserializedObject.MessageType;
        }
    }
}