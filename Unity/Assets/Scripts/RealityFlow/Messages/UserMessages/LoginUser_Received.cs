using Newtonsoft.Json;
using RealityFlow.Plugin.Scripts;
using System.Collections.Generic;

namespace Packages.realityflow_package.Runtime.scripts.Messages.UserMessages
{
    /// <summary>
    /// Handle parsing and relaying message information
    /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/event
    /// </summary>
    public class LoginUser_Received : ConfirmationMessage_Received
    {
        /// <summary>
        /// in the format of (ProjectId, Name)
        /// </summary>
        [JsonProperty("Projects")]
        public List<FlowProject> Projects { get; set; }

        public LoginUser_Received(bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "LoginUser";
            this.WasSuccessful = wasSuccessful;
        }
    }
}