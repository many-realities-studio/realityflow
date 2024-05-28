using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using GraphProcessor;

namespace Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages
{
    public class RunVSGraph_Received : ConfirmationMessage_Received
    {
        [JsonProperty("vsGraphId")]
        public string VSGraphId { get; set; }

        public RunVSGraph_Received(string vsGraphId, bool wasSuccessful) : base(wasSuccessful)
        {
            VSGraphId = vsGraphId;
            this.MessageType = "RunVSGraph";
        }
    }
}