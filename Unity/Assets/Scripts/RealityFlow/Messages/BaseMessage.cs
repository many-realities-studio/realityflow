using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages
{
    /// <summary>
    /// The purpose of this class is to provide the information which all messages contain.
    /// (Both those sent to and received from the server)
    /// </summary>
    public class BaseMessage
    {
        [JsonProperty("MessageType")]
        public string MessageType { get; set; } // Type of message being sent
    }
}