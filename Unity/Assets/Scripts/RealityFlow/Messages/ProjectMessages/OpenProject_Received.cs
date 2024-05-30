using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages
{
    /// <summary>
    /// Open project received message format
    /// </summary>
    public class OpenProject_Received : ConfirmationMessage_Received
    {
        [JsonProperty("FlowProject")]
        public FlowProject flowProject { get; set; }

        public OpenProject_Received(FlowProject flowProject, bool wasSuccessful) : base(wasSuccessful)
        {
            this.flowProject = flowProject;
            this.MessageType = "OpenProject";
        }
    }
}