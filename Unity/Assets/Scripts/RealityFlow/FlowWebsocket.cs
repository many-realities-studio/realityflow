using Packages.realityflow_package.Runtime.scripts.Messages;
using RealityFlow.Plugin.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace Packages.realityflow_package.Runtime.scripts
{
    /// <summary>
    /// The purpose of this class is to provide a wrapper for websockets that
    /// allows for the use of websockets with Unity. The reason this is necessary is because
    /// Unity cannot be interacted with from within a event (the event that gets fired when
    /// a message is received over a websocket connection)
    /// </summary>
    [ExecuteAlways]
    [Serializable]
    public class FlowWebsocket
    {
        /// <summary>
        /// A message parser is the part of the program that interprets and brings
        /// actions to the messages being received
        /// </summary>
        /// <param name="message"></param>
        public delegate void MessageParser(string message);

        public MessageParser messageParser;

        public static List<String> ReceivedMessages = new List<string>();

        [NonSerialized]
        public static WebSocketSharp.WebSocket websocket;

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Link up the message parser and establish connection to the desired URL
        /// </summary>
        /// <param name="url"></param>
        public FlowWebsocket(string url, FlowUser flowUser)
        {
            messageParser = ReceivedMessageParser.Parse;
            Connect(url, flowUser.Username, flowUser.Password);
        }

        /// <summary>
        /// Connect to the desired server over a websocket connection
        /// </summary>
        /// <param name="url">URL address of server that should be connected to</param>
        public void Connect(string url, string username, string password)
        {
            // Debug.Log("Establishing websocket connection to " + url);
            // try
            // {
            //     websocket = new WebSocketSharp.WebSocket(url);
            //     Debug.Log(url);
            //     Debug.Log(websocket);
            //     //websocket.OnMessage += (sender, e) => Debug.Log("Received message: " + e.Data.ToString());
            //     websocket.OnMessage += (sender, e) => ActionOnReceiveMessage(e.Data.ToString());
            //     websocket.SetCredentials(username, password, true);
            //     websocket.Connect();

            //     IsConnected = true;
            // }
            // catch (Exception e)
            // {
            //     IsConnected = false;
            //     Debug.LogWarning("Failed to establish connection to: " + url + ", " + e);
            // }

            // if (websocket.ReadyState == WebSocketState.Open)
            // {
            //     Debug.Log("Connection is open with: " + url);
            // }
            // else
            // {
            //     Debug.LogWarning("Failed to open connection with: " + url);
            // }
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <typeparam name="T">Type of message that should be sent (Needs to inherit from base message)</typeparam>
        /// <param name="message">The message object that should be sent</param>
        public static void SendMessage<T>(T message) where T : BaseMessage
        {
            string sentMessage = MessageSerializer.SerializeMessage(message);

            try
            {
                Debug.Log("Sending message: " + sentMessage);
                if(websocket == null) {
                  Debug.Log("Error: Websocket doesn't exist");
                } else {

                websocket.Send(sentMessage);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to send message: " + sentMessage + " " + e);
            }
        }

        /// <summary>
        /// This is the callback function for when a message is received over a websocket connection
        /// </summary>
        /// <param name="message"></param>
        private void ActionOnReceiveMessage(string message)
        {
            Debug.Log("Event handler received message: " + message);
            ReceivedMessages.Add(message);
        }

        /// <summary>
        /// Iterates through the received messages and performs the desired action.
        /// This function is utilized by EditorCoroutines (A necessary package to run this project)
        /// This function gets called once per frame (On update)
        /// </summary>
        /// <returns></returns>
        public IEnumerator ReceiveMessage()
        {
            try
            {
                for (int i = 0; i < ReceivedMessages.Count; i++)
                {
                    Debug.Log("Received message: " + ReceivedMessages[i]);
                    messageParser(ReceivedMessages[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            ReceivedMessages.RemoveAll((o) => true); // Remove everything from the list

            yield return null;
        }

        /// <summary>
        /// Send a graph message. 
        /// Does not use the same message system as the rest of Reality Flow due to graph json serialization/deserialization requirements.
        /// </summary>
        /// <param name="message">The message string that should be sent</param>
        public static void SendGraphMessage(string message)
        {
            try
            {
                Debug.Log("Sending graph message: " + message);

                // websocket.Send(message);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to send graph message: " + message + " " + e);
            }
        }

        internal void Disconnect()
        {
            // websocket.Close();
            // websocket = null;
        }
    }
}
