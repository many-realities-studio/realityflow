using System.Diagnostics;

namespace RealityFlow.API
{
    public interface IAction
    {
        public string Name { get; }

        /// <summary>
        /// The stacktrace at the time of this action's creation, which should help tracking down 
        /// what code is doing what through the callback-heavy/delayed nature of actions.
        /// </summary>
        public StackTrace StackSource { get; set; }

        public bool IsPersistent { get; set; }

        public static bool CanBePersistent => false;

        public void Do();

        public void Undo();
    }
}