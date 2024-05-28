using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.RoomMessages
{
    /// <summary>
    /// Receive: <see cref="JoinRoom_Received"/>
    /// </summary>
    public class JoinRoom_SendToServer : BaseMessage
    {
        [JsonProperty("projectId")]
        public string projectId { get; set; }

        [JsonProperty("flowUser")]
        public FlowUser flowUser { get; set; }

        public JoinRoom_SendToServer(string projectId, FlowUser flowUser)
        {
            this.projectId = projectId;
            this.flowUser = flowUser;
            this.MessageType = "JoinRoom";
        }
    }
}