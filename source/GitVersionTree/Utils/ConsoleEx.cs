using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GitVersionTree.Utils
{
	[DebuggerNonUserCode]
	public static class ConsoleEx
	{
		private const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
		private const int ERROR_ACCESS_DENIED 	 = 5;
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AllocConsole();
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", EntryPoint = "FreeConsole", SetLastError = true)]
		public static extern bool Hide();
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(uint dwProcessId);
		//---------------------------------------------------------------------
		public static void Show()
		{
			bool isAttached = AttachConsole(ATTACH_PARENT_PROCESS);
			int errorCode = Marshal.GetLastWin32Error();

			if (!isAttached && errorCode != ERROR_ACCESS_DENIED)
				if (!AllocConsole())
				{
					MessageBox.Show(
						"Console Allocation Failed.",
						"GitVersionTree",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);

					throw new Exception("Console Allocation Failed.");
				}
		}
	}
}