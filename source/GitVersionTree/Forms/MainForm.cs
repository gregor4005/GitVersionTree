using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.IO;
using GitVersionTree.Services;

namespace GitVersionTree
{
	public partial class MainForm : Form
	{		
		private string DotFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), Application.ProductName + ".dot");
		private string PdfFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), Application.ProductName + ".pdf");
		private string LogFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), Application.ProductName + ".log");
		private string RepositoryName;
		private Generator _generator = new Generator();

		public MainForm()
		{
			InitializeComponent();
			_generator.StatusUpdated += Generator_StatusUpdated;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			Text = Application.ProductName + " - v" + Application.ProductVersion.Substring(0, 3);

			RefreshPath();
		}

		private void GitPathBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog BrowseOpenFileDialog = new OpenFileDialog();
			BrowseOpenFileDialog.Title = "Select git.exe";
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GitPath))
			{
				BrowseOpenFileDialog.InitialDirectory = Properties.Settings.Default.GitPath;
			}
			BrowseOpenFileDialog.FileName = "git.exe";
			BrowseOpenFileDialog.Filter = "Git Application (git.exe)|git.exe";
			if (BrowseOpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				Properties.Settings.Default.GitPath = BrowseOpenFileDialog.FileName;
				Properties.Settings.Default.Save();
				RefreshPath();
			}
		}

		private void GraphvizDotPathBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog BrowseOpenFileDialog = new OpenFileDialog();
			BrowseOpenFileDialog.Title = "Select dot.exe";
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GraphvizPath))
			{
				BrowseOpenFileDialog.InitialDirectory = Properties.Settings.Default.GraphvizPath;
			}
			BrowseOpenFileDialog.FileName = "dot.exe";
			BrowseOpenFileDialog.Filter = "Graphviz Dot Application (dot.exe)|dot.exe";
			if (BrowseOpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				Properties.Settings.Default.GraphvizPath = BrowseOpenFileDialog.FileName;
				Properties.Settings.Default.Save();
				RefreshPath();
			}
		}

		private void GitRepositoryPathBrowseButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog BrowseFolderBrowserDialog = new FolderBrowserDialog();
			BrowseFolderBrowserDialog.Description = "Select Git repository";
			BrowseFolderBrowserDialog.ShowNewFolderButton = false;
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))
			{
				BrowseFolderBrowserDialog.SelectedPath = Properties.Settings.Default.GitRepositoryPath;
			}
			if (BrowseFolderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				Properties.Settings.Default.GitRepositoryPath = BrowseFolderBrowserDialog.SelectedPath;
				Properties.Settings.Default.Save();
				RefreshPath();
			}
		}

		private void GenerateButton_Click(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(Properties.Settings.Default.GitPath) ||
				String.IsNullOrEmpty(Properties.Settings.Default.GraphvizPath) ||
				String.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))
			{
				MessageBox.Show("Please select a Git, Graphviz & Git repository.", "Generate", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				StatusRichTextBox.Text = "";
				RepositoryName = new DirectoryInfo(GitRepositoryPathTextBox.Text).Name;
				DotFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), RepositoryName + ".dot");
				PdfFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), RepositoryName + ".pdf");
				LogFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), RepositoryName + ".log");
				File.WriteAllText(LogFilename, "");
				_generator.Generate(RepositoryName, DotFilename, PdfFilename, LogFilename, IsCompressHistoryCheckBox.Checked);
			}
		}

		private void HomepageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://github.com/crc8/GitVersionTree");
		}
		
		private void ExitButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void RefreshPath()
		{
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GitPath))
			{
				GitPathTextBox.Text = Properties.Settings.Default.GitPath;
			}
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GraphvizPath))
			{
				GraphvizDotPathTextBox.Text = Properties.Settings.Default.GraphvizPath;
			}
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))
			{
				GitRepositoryPathTextBox.Text = Properties.Settings.Default.GitRepositoryPath;
			}
		}

		void Generator_StatusUpdated(object sender, Utils.StatusEventArgs e)
		{
			StatusRichTextBox.AppendText(DateTime.Now + " - " + e.Message + "\r\n");
			StatusRichTextBox.SelectionStart = StatusRichTextBox.Text.Length;
			StatusRichTextBox.ScrollToCaret();
			Refresh();
		}		
	}
}