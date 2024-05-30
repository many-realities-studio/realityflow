using Behaviours;
using Newtonsoft.Json;
using System;

namespace Packages.realityflow_package.Runtime.scripts.Structures.Actions
{
    public class TeleportAction : FlowAction
    {
        public TeleportCoordinates teleportCoordinates;

        [JsonConstructor]
        public TeleportAction(TeleportCoordinates teleportCoordinates, string Id, string ActionType)
        {
            this.Id = Id;

            this.teleportCoordinates = teleportCoordinates;

            this.ActionType = ActionType;
        }

        public TeleportAction(TeleportCoordinates teleportCoordinates)
        {
            Id = Guid.NewGuid().ToString();

            this.teleportCoordinates = teleportCoordinates;

            if (teleportCoordinates.IsSnapZone)
            {
                this.ActionType = "SnapZone";
            }
            else
            {
                this.ActionType = "Teleport";
            }
        }
    }
}