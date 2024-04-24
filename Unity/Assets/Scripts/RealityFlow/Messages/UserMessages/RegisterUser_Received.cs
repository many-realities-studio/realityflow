namespace Packages.realityflow_package.Runtime.scripts.Messages.UserMessages
{
    public class RegisterUser_Received : ConfirmationMessage_Received
    {
        public RegisterUser_Received(bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "CreateUser";
            this.WasSuccessful = wasSuccessful;
        }
    }
}