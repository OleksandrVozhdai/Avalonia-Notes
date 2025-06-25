using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotepad.Services
{
	public interface IUndoableAction
	{
		void Undo();
	}

	/// <summary>
	/// Represents an undoable action that can be executed via a button press or similar UI interaction.
	/// Stores a delegate to perform the undo operation.
	/// </summary>
	internal class ButtonAction : IUndoableAction
	{

		private readonly Action _undo;


		public ButtonAction(Action undo)
		{
			_undo = undo;
		}

		public void Undo()
		{
			_undo?.Invoke();
		}
	}
}
