using System;

namespace GitVersionTree.Utils
{
	public class StatusEventArgs : EventArgs
	{
		public string Message { get; private set; }
		//---------------------------------------------------------------------
		public StatusEventArgs(string message)
		{
			this.Message = message;
		}
	}
}