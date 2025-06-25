using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotepad.Services
{
	/// <summary>
	/// Manages a stack of undoable actions, allowing actions to be added and undone.
	/// </summary>
	internal class UndoManager
	{
		private Stack<IUndoableAction> _undoStack = new Stack<IUndoableAction>();

		public void AddAction(IUndoableAction action)
		{
			_undoStack.Push(action);
		}

		public void UndoLast()
		{
			if (_undoStack.Any())
			{
				_undoStack.Pop().Undo();
			}
		}
	}
}
