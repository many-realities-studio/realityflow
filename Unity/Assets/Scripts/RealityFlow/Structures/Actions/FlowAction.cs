using Newtonsoft.Json;
using Packages.realityflow_package.Runtime.scripts.Messages;
using System;
using UnityEngine;

namespace Packages.realityflow_package.Runtime.scripts.Structures.Actions
{
    public class FlowAction
    {
        public string Id;
        public string ActionType;

        [JsonConstructor]
        public FlowAction(string ActionType)
        {
            this.ActionType = ActionType;

            Id = Guid.NewGuid().ToString();
        }

        public FlowAction()
        {
        }

        public FlowAction(string ActionType, string Id)
        {
            Debug.Log("in here");
            this.ActionType = ActionType;
            this.Id = Id;
        }

        public FlowAction(bool noAction)
        {
            if (noAction == true)
            {
                ActionType = "NoAction";
            }

            Id = Guid.NewGuid().ToString();
        }

        public static dynamic ConvertToChildClass(string JsonObject, string actionType)
        {
            switch (actionType)
            {
                case "Teleport":
                    return MessageSerializer.DesearializeObject(JsonObject, typeof(TeleportAction));

                case "SnapZone":
                    return MessageSerializer.DesearializeObject(JsonObject, typeof(TeleportAction));

                case "Enable":
                    return MessageSerializer.DesearializeObject(JsonObject, typeof(FlowAction));

                case "Disable":
                    return MessageSerializer.DesearializeObject(JsonObject, typeof(FlowAction));

                default:
                    return new FlowAction(true);
            }
        }
    }
}