using System;
using System.Collections.Generic;

namespace Packages.realityflow_package.Runtime.scripts.Messages
{
    public abstract class ReceivedMessage : BaseMessage
    {
        private static Dictionary<Type, List<ReceivedMessageEventHandler>> receivedMessageEventHandlers_ToBeRemovedAfterInvoke = new Dictionary<Type, List<ReceivedMessageEventHandler>>();

        public ReceivedMessage(Type typeOfObject)
        {
            if (receivedMessageEventHandlers.ContainsKey(typeOfObject) == false)
            {
                receivedMessageEventHandlers[typeOfObject] = null;
            }
        }

        private static Dictionary<Type, ReceivedMessageEventHandler> receivedMessageEventHandlers = new Dictionary<Type, ReceivedMessageEventHandler>();

        public delegate void ReceivedMessageEventHandler(object sender, BaseReceivedEventArgs eventArgs);

        public static void ReceiveMessage<T>(string message) where T : ReceivedMessage
        {
            var p2 = MessageSerializer.DesearializeObject(message, typeof(T));
            p2.RaiseEvent();
        }

        public void RaiseEvent()
        {
            if (receivedMessageEventHandlers[this.GetType()] != null)
            {
                receivedMessageEventHandlers[this.GetType()].Invoke(this, new BaseReceivedEventArgs(this, this.GetType()));

                if (receivedMessageEventHandlers_ToBeRemovedAfterInvoke.ContainsKey(this.GetType()) == true)
                {
                    foreach (ReceivedMessageEventHandler evt in receivedMessageEventHandlers_ToBeRemovedAfterInvoke[this.GetType()])
                    {
                        receivedMessageEventHandlers[this.GetType()] -= evt;
                    }
                    receivedMessageEventHandlers_ToBeRemovedAfterInvoke[this.GetType()] = new List<ReceivedMessageEventHandler>();
                }
            }
        }

        public static void AddEventHandler(Type typeOfObject, bool removeWhenReceived, ReceivedMessageEventHandler eventHandlerToAdd)
        {
            if (removeWhenReceived == true)
            {
                if (receivedMessageEventHandlers_ToBeRemovedAfterInvoke.ContainsKey(typeOfObject) == false)
                {
                    receivedMessageEventHandlers_ToBeRemovedAfterInvoke[typeOfObject] = new List<ReceivedMessageEventHandler>();
                }
                receivedMessageEventHandlers_ToBeRemovedAfterInvoke[typeOfObject].Add(eventHandlerToAdd);
            }

            if (receivedMessageEventHandlers.ContainsKey(typeOfObject) == false)
            {
                receivedMessageEventHandlers[typeOfObject] = null;
            }

            receivedMessageEventHandlers[typeOfObject] += eventHandlerToAdd;
        }
    }

    public class BaseReceivedEventArgs : EventArgs
    {
        public dynamic message;
        public Type typeOfMessage;

        public BaseReceivedEventArgs(dynamic receivedMessage, Type typeOfMessage)
        {
            message = receivedMessage;
            this.typeOfMessage = typeOfMessage;
        }
    }
}