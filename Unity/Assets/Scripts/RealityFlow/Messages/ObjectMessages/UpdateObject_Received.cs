using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;

namespace Packages.realityflow_package.Runtime.scripts.Messages.ObjectMessages
{
    public class UpdateObject_Received : ReceivedMessage
    {
        [JsonProperty("flowObject")]
        public FlowTObject flowObject { get; set; }

        public UpdateObject_Received(FlowTObject flowObject) : base(typeof(UpdateObject_Received))
        {
            this.flowObject = flowObject;
        }
    }
}