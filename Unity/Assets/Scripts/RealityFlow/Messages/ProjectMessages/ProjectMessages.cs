using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages
{
    /// <summary>
    /// Create project request message format
    /// Receive: <see cref="CreateProject_Received"/>
    /// </summary>
    public class CreateProject_SendToServer : BaseMessage
    {
        [JsonProperty("FlowProject")]
        private FlowProject flowProject { get; set; }

        [JsonProperty("FlowUser")]
        private FlowUser flowUser { get; set; }

        public CreateProject_SendToServer(FlowProject flowProject, FlowUser flowUser)
        {
            this.flowProject = flowProject;
            this.flowUser = flowUser;
            this.MessageType = "CreateProject";
        }
    }

    /// <summary>
    /// Delete project request message format
    /// Receive: <see cref="DeleteProject_Received"/>
    /// </summary>
    public class DeleteProject_SendToServer : BaseMessage
    {
        [JsonProperty("FlowProject")]
        private FlowProject flowProject { get; set; }

        [JsonProperty("FlowUser")]
        private FlowUser flowUser { get; set; }

        public DeleteProject_SendToServer(FlowProject flowProject, FlowUser flowUser)
        {
            this.flowProject = flowProject;
            this.flowUser = flowUser;
            this.MessageType = "DeleteProject";
        }
    }

    /// <summary>
    /// Open project request message format
    /// <see cref="OpenProject_Received"/>
    /// </summary>
    public class OpenProject_SendToServer : BaseMessage
    {
        [JsonProperty("ProjectId")]
        private string projectId { get; set; }

        [JsonProperty("FlowUser")]
        private FlowUser flowUser { get; set; }

        public OpenProject_SendToServer(string projectId, FlowUser flowUser)
        {
            this.projectId = projectId;
            this.flowUser = flowUser;
            this.MessageType = "OpenProject";
        }
    }

    /// <summary>
    /// Leave project request message format
    /// <see cref="LeaveProject_Received"/>
    /// </summary>
    public class LeaveProject_SendToServer : BaseMessage
    {
        [JsonProperty("ProjectId")]
        private string projectId { get; set; }

        [JsonProperty("FlowUser")]
        private FlowUser flowUser { get; set; }

        public LeaveProject_SendToServer(string projectId, FlowUser flowUser)
        {
            this.projectId = projectId;
            this.flowUser = flowUser;
            this.MessageType = "LeaveProject";
        }
    }

    /// <summary>
    /// Message format to request a list of all projects that the user has
    /// <see cref="GetAllUserProjects_Received"/>
    /// </summary>
    public class GetAllUserProjects_SendToServer : BaseMessage
    {
        [JsonProperty("FlowUser")]
        private FlowUser flowUser { get; set; }

        public GetAllUserProjects_SendToServer(FlowUser flowUser)
        {
            this.flowUser = flowUser;
            this.MessageType = "FetchProjects";
        }
    }
}