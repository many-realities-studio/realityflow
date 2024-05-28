using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages
{
    public class CreateProject_Received : ConfirmationMessage_Received
    {
        [JsonProperty("FlowProject")]
        public FlowProject flowProject { get; set; }

        public CreateProject_Received(FlowProject flowProject, bool wasSuccessful) : base(wasSuccessful)
        {
            this.flowProject = flowProject;
            this.MessageType = "CreateProject";
        }
    }
}