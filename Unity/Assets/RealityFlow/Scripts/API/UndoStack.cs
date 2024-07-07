using System;
using System.Collections.Generic;
using RealityFlow.Collections;

namespace RealityFlow.API
{
    public class UndoStack
    {
        List<IAction> actions;
        /// <summary>
        /// Cursor pointing to the index of the latest performed action that isn't undone.
        /// </summary>
        int currentCursor;

        public void DoAction(IAction action)
        {
            if (currentCursor != actions.Count - 1)
                actions.RemoveRange(currentCursor + 1, actions.Count - (currentCursor + 1));

            action.Do();
            actions.Push(action);
            currentCursor += 1;
        }

        public bool Undo()
        {
            if (currentCursor == 0)
                return false;

            IAction last = actions[currentCursor];
            last.Undo();
            currentCursor -= 1;

            return true;
        }

        public bool Redo()
        {
            if (currentCursor == actions.Count - 1)
                return false;

            IAction next = actions[currentCursor + 1];
            next.Do();
            currentCursor += 1;

            return true;
        }
    }
}