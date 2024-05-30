using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.BehaviourMessages
{
    public class UpdateBehaviour_Received : ConfirmationMessage_Received
    {
        [JsonProperty("FlowBehaviour")]
        public FlowBehaviour flowBehaviour { get; set; }

        public UpdateBehaviour_Received(FlowBehaviour flowBehaviour, bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "UpdateBehaviour";
            this.flowBehaviour = flowBehaviour;
        }
    }
}