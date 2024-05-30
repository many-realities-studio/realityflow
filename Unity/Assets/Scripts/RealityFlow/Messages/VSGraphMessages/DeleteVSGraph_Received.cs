using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.VSGraphMessages
{
    public class DeleteVSGraph_Received : ConfirmationMessage_Received
    {
        [JsonProperty("VSGraphId")]
        public string DeletedVSGraphId { get; set; } // The deleted graph

        public DeleteVSGraph_Received(string deletedVSGraphId, bool wasSuccessful) : base(wasSuccessful)
        {
            DeletedVSGraphId = deletedVSGraphId;
            this.MessageType = "DeleteVSGraph";
        }
    }
}