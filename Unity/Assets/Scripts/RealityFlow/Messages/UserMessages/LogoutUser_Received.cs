namespace Packages.realityflow_package.Runtime.scripts.Messages.UserMessages
{
    public class LogoutUser_Received : ConfirmationMessage_Received
    {
        public LogoutUser_Received(bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "LogoutUser";
            this.WasSuccessful = wasSuccessful;
        }
    }
}