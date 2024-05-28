using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;

namespace Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages
{
    public class UpdateNodeView_Received : ReceivedMessage
    {
        [JsonProperty("FlowNodeView")]
        public NodeView nodeView { get; set; }

        public UpdateNodeView_Received(NodeView nodeView) : base(typeof(UpdateNodeView_Received))
        {
            this.nodeView = nodeView;
        }
    }
}