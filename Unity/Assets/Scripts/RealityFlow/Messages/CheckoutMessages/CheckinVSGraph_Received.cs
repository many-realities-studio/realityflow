using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages
{
    public class CheckinVSGraph_Received : ConfirmationMessage_Received
    {
        [JsonProperty("VSGraphID")]
        public string VSGraphID;

        public CheckinVSGraph_Received(string vsGraphID, bool wasSuccessful) : base(wasSuccessful)
        {
            VSGraphID = vsGraphID;
            this.MessageType = "CheckinVSGraph";
        }
    }
}