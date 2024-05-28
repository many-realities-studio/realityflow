using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages
{
    public class CheckinNodeView_Received : ConfirmationMessage_Received
    {
        [JsonProperty("NodeGUID")]
        public string NodeGUID;

        public CheckinNodeView_Received(string nodeGUID, bool wasSuccessful) : base(wasSuccessful)
        {
            NodeGUID = nodeGUID;
            this.MessageType = "CheckinNodeView";
        }
    }
}