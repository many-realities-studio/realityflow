using Newtonsoft.Json;
using System.Collections.Generic;

namespace Packages.realityflow_package.Runtime.scripts.Messages.BehaviourMessages
{
    public class DeleteBehaviour_Received : ConfirmationMessage_Received
    {
        [JsonProperty("BehaviourId")]
        public List<string> BehaviourIds { get; set; }

        public DeleteBehaviour_Received(string message, bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "DeleteBehaviour";
            this.WasSuccessful = wasSuccessful;
        }
    }
}