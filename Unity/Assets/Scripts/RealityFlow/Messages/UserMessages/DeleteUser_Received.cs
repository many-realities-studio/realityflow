namespace Packages.realityflow_package.Runtime.scripts.Messages.UserMessages
{
    public class DeleteUser_Received : ConfirmationMessage_Received
    {
        public DeleteUser_Received(bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "DeleteUser";
            this.WasSuccessful = wasSuccessful;
        }
    }
}