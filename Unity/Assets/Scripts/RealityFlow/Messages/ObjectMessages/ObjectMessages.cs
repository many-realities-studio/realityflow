using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.ObjectMessages
{
    /// <summary>
    /// Response: <see cref="CreateObject_Received"/>
    /// </summary>
    public class CreateObject_SendToServer : BaseMessage
    {
        [JsonProperty("FlowObject")]
        public FlowTObject flowObject { get; set; }

        //[JsonProperty("")]
        //public FlowUser flowUser { get; set; }

        [JsonProperty("ProjectId")]
        public string projectId { get; set; }

        public CreateObject_SendToServer(FlowTObject flowObject, /*FlowUser flowUser,*/ string projectId)
        {
            this.flowObject = flowObject;
            this.projectId = projectId;

            this.MessageType = "CreateObject";
        }
    }

    /// <summary>
    /// Response: <see cref="UpdateObject_Received"/>
    /// </summary>
    public class UpdateObject_SendToServer : BaseMessage
    {
        [JsonProperty("FlowObject")]
        public FlowTObject flowObject { get; set; }

        [JsonProperty("ProjectId")]
        public string projectId { get; set; }

        [JsonProperty("UserId")]
        public string username { get; set; }

        public UpdateObject_SendToServer(FlowTObject flowObject, string projectId, string username)
        {
            this.flowObject = flowObject;
            this.projectId = projectId;
            this.username = username;

            this.MessageType = "UpdateObject";
        }
    }

    /// <summary>
    /// Response: <see cref="DeleteObject_Received"/>
    /// </summary>
    public class DeleteObject_SendToServer : BaseMessage
    {
        [JsonProperty("ObjectId")]
        public string ObjectId { get; set; } // Id of the deleted object

        [JsonProperty("ProjectId")]
        public string ProjectId { get; set; }

        public DeleteObject_SendToServer(string projectId, string objectId)
        {
            ProjectId = projectId;
            this.ObjectId = objectId;
            this.MessageType = "DeleteObject";
        }
    }

    /// <summary>
    /// Response: <see cref="FinalizedUpdateObject_Received"/>
    /// </summary>
    public class FinalizedUpdateObject_SendToServer : BaseMessage
    {
        [JsonProperty("flowObject")]
        public FlowTObject flowObject { get; set; }

        [JsonProperty("projectId")]
        public string projectId { get; set; }

        public FinalizedUpdateObject_SendToServer(FlowTObject flowObject, string projectId)
        {
            this.flowObject = flowObject;
            this.projectId = projectId;

            this.MessageType = "FinalizedUpdateObject";
        }
    }
}