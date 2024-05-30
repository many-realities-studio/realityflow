using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages
{
    public class CheckoutObject_SendToServer : BaseMessage
    {
        [JsonProperty("ObjectId")]
        public string ObjectId;

        [JsonProperty("ProjectId")]
        public string ProjectId;

        [JsonProperty("UserId")]
        public string UserId;

        public CheckoutObject_SendToServer(string objectId, string projectId, string userId)
        {
            ObjectId = objectId;
            ProjectId = projectId;
            UserId = userId;
            this.MessageType = "CheckoutObject";
        }
    }

    public class CheckinObject_SendToServer : BaseMessage
    {
        [JsonProperty("ObjectId")]
        public string ObjectId;

        [JsonProperty("ProjectId")]
        public string ProjectId;

        [JsonProperty("UserId")]
        public string UserId;

        public CheckinObject_SendToServer(string objectId, string projectId, string userId)
        {
            ObjectId = objectId;
            ProjectId = projectId;
            UserId = userId;
            this.MessageType = "CheckinObject";
        }
    }

    public class CheckoutVSGraph_SendToServer : BaseMessage
    {
        [JsonProperty("VSGraphId")]
        public string VSGraphId;

        [JsonProperty("ProjectId")]
        public string ProjectId;

        public CheckoutVSGraph_SendToServer(string vsGraphId, string projectId)
        {
            VSGraphId = vsGraphId;
            ProjectId = projectId;
            this.MessageType = "CheckoutVSGraph";
        }
    }

    public class CheckinVSGraph_SendToServer : BaseMessage
    {
        [JsonProperty("VSGraphId")]
        public string VSGraphId;

        [JsonProperty("ProjectId")]
        public string ProjectId;

        public CheckinVSGraph_SendToServer(string vsGraphId, string projectId)
        {
            VSGraphId = vsGraphId;
            ProjectId = projectId;
            this.MessageType = "CheckinVSGraph";
        }
    }

    public class CheckoutNodeView_SendToServer : BaseMessage
    {
        [JsonProperty("NodeGUID")]
        public string NodeGUID;

        [JsonProperty("ProjectId")]
        public string ProjectId;

        public CheckoutNodeView_SendToServer(string nodeGUID, string projectId)
        {
            NodeGUID = nodeGUID;
            ProjectId = projectId;
            this.MessageType = "CheckoutNodeView";
        }
    }

    public class CheckinNodeView_SendToServer : BaseMessage
    {
        [JsonProperty("NodeGUID")]
        public string NodeGUID;

        [JsonProperty("ProjectId")]
        public string ProjectId;

        public CheckinNodeView_SendToServer(string nodeGUID, string projectId)
        {
            NodeGUID = nodeGUID;
            ProjectId = projectId;
            this.MessageType = "CheckinNodeView";
        }
    }
}