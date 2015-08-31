using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GitVersionTree.Services;
using GitVersionTree.Utils;

namespace GitVersionTree
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (Properties.Settings.Default.NeedsUpgrade)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.NeedsUpgrade = false;
				Properties.Settings.Default.Save();
			}

			if (args.Length == 0)
				RunAsWinform();
			else
				RunAsCommandLine(args);
		}
		//---------------------------------------------------------------------
		private static void RunAsWinform()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		//---------------------------------------------------------------------
		private static void RunAsCommandLine(string[] args)
		{
			ConsoleEx.Show();

			CommandLine commandLine = CommandLine.GetCommandLine(args);

			if (commandLine != null && commandLine.Run)
			{
				if (string.IsNullOrWhiteSpace(Properties.Settings.Default.GitPath))
				{
					Console.Write("Enter path to git.exe -> ");
					Properties.Settings.Default.GitPath = Console.ReadLine();
					Properties.Settings.Default.Save();
				}

				if (string.IsNullOrWhiteSpace(Properties.Settings.Default.GraphvizPath))
				{
					Console.Write("Enter path to dot.exe (graphviz) -> ");
					Properties.Settings.Default.GraphvizPath = Console.ReadLine();
					Properties.Settings.Default.Save();
				}

				Generator generator = new Generator();
				generator.StatusUpdated += (s, e) => Console.WriteLine(e.Message);

				generator.Generate(commandLine.GitRepositoryPath.ToString(), commandLine.Format);
			}

			Console.WriteLine("\nBye. Hit ENTER to exit...");

			if (Debugger.IsAttached)
				Console.ReadKey();
		}
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();
	}
}