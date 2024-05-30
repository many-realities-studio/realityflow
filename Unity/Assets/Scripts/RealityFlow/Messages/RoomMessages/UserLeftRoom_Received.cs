using Newtonsoft.Json;

namespace Packages.realityflow_package.Runtime.scripts.Messages.RoomMessages
{
    public class UserLeftRoom_Received : ReceivedMessage
    {
        [JsonProperty("Message")]
        public string leftRoomMessage { get; set; }

        public UserLeftRoom_Received(string Message) : base(typeof(UserLeftRoom_Received))
        {
            this.leftRoomMessage = Message;
            this.MessageType = "UserLeftRoom";
        }
    }
}