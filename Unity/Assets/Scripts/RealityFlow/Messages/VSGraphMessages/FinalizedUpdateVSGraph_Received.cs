using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;

namespace Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages
{
    public class FinalizedUpdateVSGraph_Received : ReceivedMessage
    {
        [JsonProperty("flowVSGraph")]
        public FlowVSGraph flowVSGraph { get; set; }

        public FinalizedUpdateVSGraph_Received(FlowVSGraph flowVSGraph) : base(typeof(UpdateVSGraph_Received))
        {
            this.flowVSGraph = flowVSGraph;
        }
    }
}