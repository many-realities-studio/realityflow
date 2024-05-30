using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages
{
    public class CheckoutObject_Received : ConfirmationMessage_Received
    {
        [JsonProperty("ObjectID")]
        public string ObjectID;

        public CheckoutObject_Received(string objectID, bool wasSuccessful) : base(wasSuccessful)
        {
            ObjectID = objectID;
            this.MessageType = "CheckoutObject";
        }
    }
}