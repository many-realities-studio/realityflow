namespace Packages.realityflow_package.Runtime.scripts.Messages.ProjectMessages
{
    /// <summary>
    /// Leave project received message format
    /// </summary>
    public class LeaveProject_Received : ConfirmationMessage_Received
    {
        public LeaveProject_Received(bool wasSuccessful) : base(wasSuccessful)
        {
            this.MessageType = "LeaveProject";
        }
    }
}