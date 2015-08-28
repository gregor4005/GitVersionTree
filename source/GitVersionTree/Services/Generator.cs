using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitVersionTree.Utils;

namespace GitVersionTree.Services
{
	public	class Generator
	{
		public event EventHandler<StatusEventArgs> StatusUpdated;
		private void OnStatusUpdated(string message)
		{
			var tmp = this.StatusUpdated;
			if (tmp != null)
				tmp(this, new StatusEventArgs(message));
		}
		//---------------------------------------------------------------------
		private string Execute(string command, string argument)
		{
			string executeResult = String.Empty;
			
			Process executeProcess = new Process();
			
			executeProcess.StartInfo.UseShellExecute 		= false;
			executeProcess.StartInfo.CreateNoWindow 		= true;
			executeProcess.StartInfo.RedirectStandardOutput = true;
			executeProcess.StartInfo.FileName 				= command;
			executeProcess.StartInfo.Arguments 				= argument;
			executeProcess.StartInfo.WindowStyle 			= ProcessWindowStyle.Hidden;
			
			executeProcess.Start();
			executeResult = executeProcess.StandardOutput.ReadToEnd();
			executeProcess.WaitForExit();
			
			if (executeProcess.ExitCode == 0)
			{
				return executeResult;
			}
			else
			{
				return String.Empty;
			}
		}
		//---------------------------------------------------------------------
		public void Generate(
			string RepositoryName,
			string DotFilename, 
			string PdfFilename, 
			string LogFilename, 
			bool compressHistory)
		{
			Dictionary<string, string> DecorateDictionary = new Dictionary<string, string>();
			List<List<string>> Nodes = new List<List<string>>();
		
			string Result;
			string[] MergedColumns;
			string[] MergedParents;

			this.OnStatusUpdated("Getting git commit(s) ...");
			Result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --all --pretty=format:\"%h|%p|%d\"");
			if (String.IsNullOrEmpty(Result))
			{
				this.OnStatusUpdated("Unable to get get branch or branch empty ...");
			}
			else
			{
				File.AppendAllText(LogFilename, "[commit(s)]\r\n");
				File.AppendAllText(LogFilename, Result + "\r\n");
				string[] DecorateLines = Result.Split('\n');
				foreach (string DecorateLine in DecorateLines)
				{
					MergedColumns = DecorateLine.Split('|');
					if (!String.IsNullOrEmpty(MergedColumns[2]))
					{
						DecorateDictionary.Add(MergedColumns[0], MergedColumns[2]);
					}
				}
				this.OnStatusUpdated("Processed " + DecorateDictionary.Count + " decorate(s) ...");
			}

			this.OnStatusUpdated("Getting git ref branch(es) ...");
			Result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" for-each-ref --format=\"%(objectname:short)|%(refname:short)\" "); //refs/heads/
			if (String.IsNullOrEmpty(Result))
			{
				this.OnStatusUpdated("Unable to get get branch or branch empty ...");
			}
			else
			{
				File.AppendAllText(LogFilename, "[ref branch(es)]\r\n");
				File.AppendAllText(LogFilename, Result + "\r\n");
				string[] RefLines = Result.Split('\n');
				foreach (string RefLine in RefLines)
				{
					if (!String.IsNullOrEmpty(RefLine))
					{
						string[] RefColumns = RefLine.Split('|');
						if (!RefColumns[1].ToLower().StartsWith("refs/tags"))
							if (RefColumns[1].ToLower().Contains("master"))
							{
								Result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --reverse --first-parent --pretty=format:\"%h\" " + RefColumns[0]);
								if (String.IsNullOrEmpty(Result))
								{
									this.OnStatusUpdated("Unable to get commit(s) ...");
								}
								else
								{
									string[] HashLines = Result.Split('\n');
									Nodes.Add(new List<string>());
									foreach (string HashLine in HashLines)
									{
										Nodes[Nodes.Count - 1].Add(HashLine);
									}
								}
							}
					}
				}
				foreach (string RefLine in RefLines)
				{
					if (!String.IsNullOrEmpty(RefLine))
					{
						string[] RefColumns = RefLine.Split('|');
						if (!RefColumns[1].ToLower().StartsWith("refs/tags"))
							if (!RefColumns[1].ToLower().Contains("master"))
							{
								Result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --reverse --first-parent --pretty=format:\"%h\" " + RefColumns[0]);
								if (String.IsNullOrEmpty(Result))
								{
									this.OnStatusUpdated("Unable to get commit(s) ...");
								}
								else
								{
									string[] HashLines = Result.Split('\n');
									Nodes.Add(new List<string>());
									foreach (string HashLine in HashLines)
									{
										Nodes[Nodes.Count - 1].Add(HashLine);
									}
								}
							}
					}
				}
			}

			this.OnStatusUpdated("Getting git merged branch(es) ...");
			Result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --all --merges --pretty=format:\"%h|%p\"");
			if (String.IsNullOrEmpty(Result))
			{
				this.OnStatusUpdated("Unable to get get branch or branch empty ...");
			}
			else
			{
				File.AppendAllText(LogFilename, "[merged branch(es)]\r\n");
				File.AppendAllText(LogFilename, Result + "\r\n");
				string[] MergedLines = Result.Split('\n');
				foreach (string MergedLine in MergedLines)
				{
					MergedColumns = MergedLine.Split('|');
					MergedParents = MergedColumns[1].Split(' ');
					if (MergedParents.Length > 1)
					{
						for (int i = 1; i < MergedParents.Length; i++)
						{
							Result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --reverse --first-parent --pretty=format:\"%h\" " + MergedParents[i]);
							if (String.IsNullOrEmpty(Result))
							{
								this.OnStatusUpdated("Unable to get commit(s) ...");
							}
							else
							{
								string[] HashLines = Result.Split('\n');
								Nodes.Add(new List<string>());
								foreach (string HashLine in HashLines)
								{
									Nodes[Nodes.Count - 1].Add(HashLine);
								}
								Nodes[Nodes.Count - 1].Add(MergedColumns[0]);
							}
						}
					}
				}
			}

			if (compressHistory)
			{
				Nodes = (from node in Nodes
						 select node.Count > 2 ?
									 (new List<string>(new[] { node[0], String.Format("{0} histories omitted", node.Count - 2), node[node.Count - 1] }))
									 : node).ToList();
			}


			this.OnStatusUpdated("Processed " + Nodes.Count + " branch(es) ...");

			StringBuilder DotStringBuilder = new StringBuilder();
			this.OnStatusUpdated("Generating dot file ...");
			DotStringBuilder.Append("strict digraph \"" + RepositoryName + "\" {\r\n");
			//DotStringBuilder.Append("  splines=line;\r\n");
			for (int i = 0; i < Nodes.Count; i++)
			{
				DotStringBuilder.Append("  node[group=\"" + (i + 1) + "\"];\r\n");
				DotStringBuilder.Append("  ");
				for (int j = 0; j < Nodes[i].Count; j++)
				{
					DotStringBuilder.Append("\"" + Nodes[i][j] + "\"");
					if (j < Nodes[i].Count - 1)
					{
						DotStringBuilder.Append(" -> ");
					}
					else
					{
						DotStringBuilder.Append(";");
					}
				}
				DotStringBuilder.Append("\r\n");
			}

			int DecorateCount = 0;
			foreach (KeyValuePair<string, string> DecorateKeyValuePair in DecorateDictionary)
			{
				DecorateCount++;
				DotStringBuilder.Append("  subgraph Decorate" + DecorateCount + "\r\n");
				DotStringBuilder.Append("  {\r\n");
				DotStringBuilder.Append("    rank=\"same\";\r\n");
				if (DecorateKeyValuePair.Value.Trim().StartsWith("(tag:"))
				{
					DotStringBuilder.Append("    \"" + DecorateKeyValuePair.Value.Trim() + "\" [shape=\"box\", style=\"filled\", fillcolor=\"#ffffdd\"];\r\n");
				}
				else
				{
					DotStringBuilder.Append("    \"" + DecorateKeyValuePair.Value.Trim() + "\" [shape=\"box\", style=\"filled\", fillcolor=\"#ddddff\"];\r\n");
				}
				DotStringBuilder.Append("    \"" + DecorateKeyValuePair.Value.Trim() + "\" -> \"" + DecorateKeyValuePair.Key + "\" [weight=0, arrowtype=\"none\", dirtype=\"none\", arrowhead=\"none\", style=\"dotted\"];\r\n");
				DotStringBuilder.Append("  }\r\n");
			}

			DotStringBuilder.Append("}\r\n");
			File.WriteAllText(@DotFilename, DotStringBuilder.ToString());

			this.OnStatusUpdated("Generating version tree ...");
			Process DotProcess = new Process();
			DotProcess.StartInfo.UseShellExecute = false;
			DotProcess.StartInfo.CreateNoWindow = true;
			DotProcess.StartInfo.RedirectStandardOutput = true;
			DotProcess.StartInfo.FileName = Properties.Settings.Default.GraphvizPath;
			DotProcess.StartInfo.Arguments = "\"" + @DotFilename + "\" -Tpdf -Gsize=10,10 -o\"" + @PdfFilename + "\"";
			DotProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			DotProcess.Start();
			DotProcess.WaitForExit();

			DotProcess.StartInfo.Arguments = "\"" + @DotFilename + "\" -Tps -o\"" + @PdfFilename.Replace(".pdf", ".ps") + "\"";
			DotProcess.Start();
			DotProcess.WaitForExit();
			if (DotProcess.ExitCode == 0)
			{
				if (File.Exists(@PdfFilename))
				{
#if (!DEBUG)
					/*
					Process ViewPdfProcess = new Process();
					ViewPdfProcess.StartInfo.FileName = @PdfFilename;
					ViewPdfProcess.Start();
					//ViewPdfProcess.WaitForExit();
					//Close();
					*/
#endif
				}
			}
			else
			{
				this.OnStatusUpdated("Version tree generation failed ...");
			}

			this.OnStatusUpdated("Done! ...");
		}
	}
}