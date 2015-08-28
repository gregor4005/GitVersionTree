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
		private static string Execute(string command, string argument)
		{
			string executeResult = String.Empty;

			Process executeProcess = new Process();

			executeProcess.StartInfo.UseShellExecute = false;
			executeProcess.StartInfo.CreateNoWindow = true;
			executeProcess.StartInfo.RedirectStandardOutput = true;
			executeProcess.StartInfo.FileName = command;
			executeProcess.StartInfo.Arguments = argument;
			executeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

			executeProcess.Start();
			executeResult = executeProcess.StandardOutput.ReadToEnd();
			executeProcess.WaitForExit();

			if (executeProcess.ExitCode == 0)
				return executeResult;
			else
				return String.Empty;
		}
		//---------------------------------------------------------------------
		public void Generate(
			string repositoryName,
			string dotFilename, 
			string pdfFilename, 
			string logFilename, 
			bool compressHistory)
		{
			Dictionary<string, string> decorateDictionary = new Dictionary<string, string>();
			List<List<string>> nodes = new List<List<string>>();
		
			string result;
			string[] mergedColumns;
			string[] mergedParents;

			this.OnStatusUpdated("Getting git commit(s) ...");
			result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --all --pretty=format:\"%h|%p|%d\"");
			if (String.IsNullOrEmpty(result))
				this.OnStatusUpdated("Unable to get get branch or branch empty ...");
			else
			{
				File.AppendAllText(logFilename, "[commit(s)]\r\n");
				File.AppendAllText(logFilename, result + "\r\n");
				string[] decorateLines = result.Split('\n');
				foreach (string decorateLine in decorateLines)
				{
					mergedColumns = decorateLine.Split('|');
					if (!String.IsNullOrEmpty(mergedColumns[2]))
					{
						decorateDictionary.Add(mergedColumns[0], mergedColumns[2]);
					}
				}
				this.OnStatusUpdated("Processed " + decorateDictionary.Count + " decorate(s) ...");
			}

			this.OnStatusUpdated("Getting git ref branch(es) ...");
			result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" for-each-ref --format=\"%(objectname:short)|%(refname:short)\" "); //refs/heads/
			if (String.IsNullOrEmpty(result))
			{
				this.OnStatusUpdated("Unable to get get branch or branch empty ...");
			}
			else
			{
				File.AppendAllText(logFilename, "[ref branch(es)]\r\n");
				File.AppendAllText(logFilename, result + "\r\n");
				string[] refLines = result.Split('\n');
				foreach (string refLine in refLines)
				{
					if (!String.IsNullOrEmpty(refLine))
					{
						string[] refColumns = refLine.Split('|');
						if (!refColumns[1].ToLower().StartsWith("refs/tags"))
							if (refColumns[1].ToLower().Contains("master"))
							{
								result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --reverse --first-parent --pretty=format:\"%h\" " + refColumns[0]);
								if (String.IsNullOrEmpty(result))
								{
									this.OnStatusUpdated("Unable to get commit(s) ...");
								}
								else
								{
									string[] hashLines = result.Split('\n');
									nodes.Add(new List<string>());
									foreach (string hashLine in hashLines)
									{
										nodes[nodes.Count - 1].Add(hashLine);
									}
								}
							}
					}
				}
				foreach (string refLine in refLines)
				{
					if (!String.IsNullOrEmpty(refLine))
					{
						string[] refColumns = refLine.Split('|');
						if (!refColumns[1].ToLower().StartsWith("refs/tags"))
							if (!refColumns[1].ToLower().Contains("master"))
							{
								result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --reverse --first-parent --pretty=format:\"%h\" " + refColumns[0]);
								if (String.IsNullOrEmpty(result))
								{
									this.OnStatusUpdated("Unable to get commit(s) ...");
								}
								else
								{
									string[] hashLines = result.Split('\n');
									nodes.Add(new List<string>());
									foreach (string hashLine in hashLines)
									{
										nodes[nodes.Count - 1].Add(hashLine);
									}
								}
							}
					}
				}
			}

			this.OnStatusUpdated("Getting git merged branch(es) ...");
			result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --all --merges --pretty=format:\"%h|%p\"");
			if (String.IsNullOrEmpty(result))
			{
				this.OnStatusUpdated("Unable to get get branch or branch empty ...");
			}
			else
			{
				File.AppendAllText(logFilename, "[merged branch(es)]\r\n");
				File.AppendAllText(logFilename, result + "\r\n");
				string[] mergedLines = result.Split('\n');
				foreach (string mergedLine in mergedLines)
				{
					mergedColumns = mergedLine.Split('|');
					mergedParents = mergedColumns[1].Split(' ');
					if (mergedParents.Length > 1)
					{
						for (int i = 1; i < mergedParents.Length; i++)
						{
							result = Execute(Properties.Settings.Default.GitPath, "--git-dir \"" + Path.Combine(Properties.Settings.Default.GitRepositoryPath, ".git") + "\" log --reverse --first-parent --pretty=format:\"%h\" " + mergedParents[i]);
							if (String.IsNullOrEmpty(result))
							{
								this.OnStatusUpdated("Unable to get commit(s) ...");
							}
							else
							{
								string[] hashLines = result.Split('\n');
								nodes.Add(new List<string>());
								foreach (string hashLine in hashLines)
								{
									nodes[nodes.Count - 1].Add(hashLine);
								}
								nodes[nodes.Count - 1].Add(mergedColumns[0]);
							}
						}
					}
				}
			}

			if (compressHistory)
			{
				nodes = (from node in nodes
						 select node.Count > 2 ?
									 (new List<string>(new[] { node[0], String.Format("{0} histories omitted", node.Count - 2), node[node.Count - 1] }))
									 : node).ToList();
			}


			this.OnStatusUpdated("Processed " + nodes.Count + " branch(es) ...");

			StringBuilder dotStringBuilder = new StringBuilder();
			this.OnStatusUpdated("Generating dot file ...");
			dotStringBuilder.Append("strict digraph \"" + repositoryName + "\" {\r\n");
			//DotStringBuilder.Append("  splines=line;\r\n");
			for (int i = 0; i < nodes.Count; i++)
			{
				dotStringBuilder.Append("  node[group=\"" + (i + 1) + "\"];\r\n");
				dotStringBuilder.Append("  ");
				for (int j = 0; j < nodes[i].Count; j++)
				{
					dotStringBuilder.Append("\"" + nodes[i][j] + "\"");
					if (j < nodes[i].Count - 1)
					{
						dotStringBuilder.Append(" -> ");
					}
					else
					{
						dotStringBuilder.Append(";");
					}
				}
				dotStringBuilder.Append("\r\n");
			}

			int decorateCount = 0;
			foreach (KeyValuePair<string, string> decorateKeyValuePair in decorateDictionary)
			{
				decorateCount++;
				dotStringBuilder.Append("  subgraph Decorate" + decorateCount + "\r\n");
				dotStringBuilder.Append("  {\r\n");
				dotStringBuilder.Append("    rank=\"same\";\r\n");
				if (decorateKeyValuePair.Value.Trim().StartsWith("(tag:"))
				{
					dotStringBuilder.Append("    \"" + decorateKeyValuePair.Value.Trim() + "\" [shape=\"box\", style=\"filled\", fillcolor=\"#ffffdd\"];\r\n");
				}
				else
				{
					dotStringBuilder.Append("    \"" + decorateKeyValuePair.Value.Trim() + "\" [shape=\"box\", style=\"filled\", fillcolor=\"#ddddff\"];\r\n");
				}
				dotStringBuilder.Append("    \"" + decorateKeyValuePair.Value.Trim() + "\" -> \"" + decorateKeyValuePair.Key + "\" [weight=0, arrowtype=\"none\", dirtype=\"none\", arrowhead=\"none\", style=\"dotted\"];\r\n");
				dotStringBuilder.Append("  }\r\n");
			}

			dotStringBuilder.Append("}\r\n");
			File.WriteAllText(dotFilename, dotStringBuilder.ToString());

			this.OnStatusUpdated("Generating version tree ...");
			Process dotProcess = new Process();
			dotProcess.StartInfo.UseShellExecute = false;
			dotProcess.StartInfo.CreateNoWindow = true;
			dotProcess.StartInfo.RedirectStandardOutput = true;
			dotProcess.StartInfo.FileName = Properties.Settings.Default.GraphvizPath;
			dotProcess.StartInfo.Arguments = "\"" + dotFilename + "\" -Tpdf -Gsize=10,10 -o\"" + pdfFilename + "\"";
			dotProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			dotProcess.Start();
			dotProcess.WaitForExit();

			dotProcess.StartInfo.Arguments = "\"" + dotFilename + "\" -Tps -o\"" + pdfFilename.Replace(".pdf", ".ps") + "\"";
			dotProcess.Start();
			dotProcess.WaitForExit();
			if (dotProcess.ExitCode == 0)
			{
				if (File.Exists(pdfFilename))
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