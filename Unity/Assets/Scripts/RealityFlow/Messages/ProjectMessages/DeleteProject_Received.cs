namespace Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages
{
    public class DeleteProject_Received : ConfirmationMessage_Received
    {
        public DeleteProject_Received(bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "DeleteProject";
        }
    }
}