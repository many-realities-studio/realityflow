using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.CheckoutMessages
{
    public class CheckinObject_Received : ConfirmationMessage_Received
    {
        [JsonProperty("ObjectID")]
        public string ObjectID;

        public CheckinObject_Received(string objectID, bool wasSuccessful) : base(wasSuccessful)
        {
            ObjectID = objectID;
            this.MessageType = "CheckinObject";
        }
    }
}