using System;
using System.IO;
using System.Reflection;
using CommandLineParser.Arguments;
using CommandLineParser.Validation;

namespace GitVersionTree
{
	internal class CommandLine
	{
		[DirectoryArgument('r', "repo",
			Description = "path to the git repository",
			DirectoryMustExist = true)]
		public DirectoryInfo GitRepositoryPath { get; set; }
		//---------------------------------------------------------------------
		[ValueArgument(typeof(OutputFormat), 'f', "format",
			DefaultValue = OutputFormat.PDF,
			Description = "output format. PDF, EPS, SVG, Jpg")]
		public OutputFormat Format { get; set; }
		//---------------------------------------------------------------------
		public bool Run { get; set; }
		//---------------------------------------------------------------------
		[SwitchArgument('h', "help", false, Description = "shows this help text.")]
		public bool ShowHelp { get; set; }
		//---------------------------------------------------------------------
		public static CommandLine GetCommandLine(string[] args)
		{
			try
			{
				CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
				CommandLine commandLine 				   = new CommandLine();
				parser.ShowUsageHeader 					   = GetHeaderText();
				parser.ShowUsageFooter 					   = GetFooterText();
				parser.ShowUsageOnEmptyCommandline 		   = true;
				parser.ExtractArgumentAttributes(commandLine);
				parser.ParseCommandLine(args);

				commandLine.Run = commandLine.GitRepositoryPath != null;

				if (commandLine.ShowHelp) parser.ShowUsage();

				return commandLine;
			}
			catch (ArgumentConflictException ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}
		//---------------------------------------------------------------------
		private static string GetHeaderText()
		{
			AssemblyName assemblyName = typeof(CommandLine)
				.Assembly
				.GetName();

			string text = string.Format(
				"{0}, Version {1}\n",
				assemblyName.Name,
				assemblyName.Version);

			return text;
		}
		//---------------------------------------------------------------------
		private static string GetFooterText()
		{
			string footer =
				"Usage example: GitVersionTree -r D:\\Working-Copys\\MyProject -f Jpg\n\n" +
				"Copyright (c) 2013-2015";

			return footer;
		}
	}
}