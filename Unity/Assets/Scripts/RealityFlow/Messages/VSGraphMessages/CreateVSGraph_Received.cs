using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;

namespace Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages
{
    public class CreateVSGraph_Received : ReceivedMessage
    {
        // Upon receiving a message of this type, setting this property here allows deserialization to instantiate a new FlowVSGraph using
        // its JsonConstructor. This is done similarly for updates.
        [JsonProperty("flowVSGraph")]
        public FlowVSGraph flowVSGraph { get; set; }

        public CreateVSGraph_Received(FlowVSGraph flowVSGraph) : base(typeof(CreateVSGraph_Received))
        {
            this.flowVSGraph = flowVSGraph;
            this.MessageType = "CreateVSGraph";
        }
    }
}