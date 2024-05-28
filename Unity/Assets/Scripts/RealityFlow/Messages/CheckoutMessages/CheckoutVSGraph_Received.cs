using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages
{
    public class CheckoutVSGraph_Received : ConfirmationMessage_Received
    {
        [JsonProperty("VSGraphID")]
        public string VSGraphID;

        public CheckoutVSGraph_Received(string vsGraphID, bool wasSuccessful) : base(wasSuccessful)
        {
            VSGraphID = vsGraphID;
            this.MessageType = "CheckoutVSGraph";
        }
    }
}