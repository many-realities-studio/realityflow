using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using System.Collections.Generic;

namespace Packages.realityflow_package.Runtime.scripts.Messages.BehaviourMessages
{
    /// <summary>
    /// Create behaviour request message format
    /// Receive: <see cref="CreateBehaviour_Received"/>
    /// </summary>
    public class CreateBehaviour_SendToServer : BaseMessage
    {
        [JsonProperty("FlowBehaviour")]
        private FlowBehaviour FlowBehaviour { get; set; }

        [JsonProperty("ProjectId")]
        private string ProjectId { get; set; }

        [JsonProperty("BehaviorsToLinkTo")]
        private List<string> BehavioursToLinkTo { get; set; }

        public CreateBehaviour_SendToServer(FlowBehaviour behaviour, string projectId, List<string> behavioursToLinkTo)
        {
            this.FlowBehaviour = behaviour;
            this.ProjectId = projectId;

            this.MessageType = "CreateBehaviour";
            this.BehavioursToLinkTo = behavioursToLinkTo;
        }
    }

    /// <summary>
    /// Delete project request message format
    /// Receive: <see cref="DeleteBehaviour_Received"/>
    /// </summary>
    public class DeleteBehaviour_SendToServer : BaseMessage
    {
        [JsonProperty("ProjectId")]
        private string ProjectId { get; set; }

        [JsonProperty("BehaviourIds")]
        private List<string> behaviourIds { get; set; }

        public DeleteBehaviour_SendToServer(List<string> behaviourIds, string projectId)
        {
            this.ProjectId = projectId;
            this.behaviourIds = behaviourIds;
            this.MessageType = "DeleteBehaviour";
        }
    }

    /// <summary>
    /// Update behaviour request message format
    /// Receive: <see cref="UpdateBehaviour_Received"/>
    /// </summary>
    public class UpdateBehaviour_SendToServer : BaseMessage
    {
        [JsonProperty("FlowBehaviour")]
        private FlowBehaviour FlowBehaviour { get; set; }

        [JsonProperty("ProjectId")]
        private string ProjectId { get; set; }

        public UpdateBehaviour_SendToServer(FlowBehaviour behaviour, string projectId)
        {
            this.FlowBehaviour = behaviour;
            this.ProjectId = projectId;

            this.MessageType = "UpdateBehaviour";
        }
    }
}