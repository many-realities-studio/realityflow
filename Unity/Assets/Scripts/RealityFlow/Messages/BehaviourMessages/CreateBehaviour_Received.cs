using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using System.Collections.Generic;

namespace Packages.realityflow_package.Runtime.scripts.Messages.BehaviourMessages
{
    public class CreateBehaviour_Received : ConfirmationMessage_Received
    {
        [JsonProperty("FlowBehaviour")]
        public FlowBehaviour flowBehaviour { get; set; }

        [JsonProperty("BehaviorsToLinkTo")]
        public List<string> behavioursToLinkTo { get; set; }

        public CreateBehaviour_Received(string message, bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "CreateBehaviour";
        }
    }
}