using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitVersionTree
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (Properties.Settings.Default.NeedsUpgrade)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.NeedsUpgrade = false;
				Properties.Settings.Default.Save();
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}