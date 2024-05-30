using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages
{
    /// <summary>
    /// Received message format for pass/fail requests
    /// </summary>
    public abstract class ConfirmationMessage_Received : ReceivedMessage
    {
        [JsonProperty("WasSuccessful")]
        public bool WasSuccessful { get; set; }

        protected ConfirmationMessage_Received(bool wasSuccessful) : base(typeof(ConfirmationMessage_Received))
        {
            WasSuccessful = wasSuccessful;
        }
    }
}