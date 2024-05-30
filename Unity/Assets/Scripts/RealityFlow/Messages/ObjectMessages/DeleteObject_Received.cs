using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.ObjectMessages
{
    public class DeleteObject_Received : ConfirmationMessage_Received
    {
        [JsonProperty("ObjectId")]
        public string DeletedObjectId { get; set; } // The deleted object

        public DeleteObject_Received(string deletedObjectId, bool wasSuccessful) : base(wasSuccessful)
        {
            DeletedObjectId = deletedObjectId;
            this.MessageType = "DeleteObject";
        }
    }
}