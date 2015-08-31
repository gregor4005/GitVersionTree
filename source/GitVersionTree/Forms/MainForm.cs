using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitVersionTree.Services;

namespace GitVersionTree
{
	public partial class MainForm : Form
	{
		private Generator _generator = new Generator();
		//---------------------------------------------------------------------
		public MainForm()
		{
			InitializeComponent();
			outputFormatListBox.DataSource = Enum.GetNames(typeof(OutputFormat));

			_generator.StatusUpdated += Generator_StatusUpdated;
		}
		//---------------------------------------------------------------------
		private void MainForm_Load(object sender, EventArgs e)
		{
			this.Text = Application.ProductName + " - v" + Application.ProductVersion.Substring(0, 3);

			this.RefreshPath();
		}
		//---------------------------------------------------------------------
		private void BrowseForPath(string fileName, string filter, ref string path)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = string.Format("Select {0}", fileName);

				if (!string.IsNullOrEmpty(path))
					openFileDialog.InitialDirectory = path;

				openFileDialog.FileName = fileName;
				openFileDialog.Filter = filter;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
					path = openFileDialog.FileName;
			}
		}
		//---------------------------------------------------------------------
		private void GitPathBrowseButton_Click(object sender, EventArgs e)
		{
			string path = Properties.Settings.Default.GitPath;
			this.BrowseForPath("git.exe", "Git Application (git.exe)|git.exe", ref path);
			Properties.Settings.Default.GitPath = path;
			Properties.Settings.Default.Save();
			this.RefreshPath();
		}
		//---------------------------------------------------------------------
		private void GraphvizDotPathBrowseButton_Click(object sender, EventArgs e)
		{
			string path = Properties.Settings.Default.GraphvizPath;
			this.BrowseForPath("dot.exe", "Graphviz Dot Application (dot.exe)|dot.exe", ref path);
			Properties.Settings.Default.GraphvizPath = path;
			Properties.Settings.Default.Save();
			this.RefreshPath();
		}
		//---------------------------------------------------------------------
		private void GitRepositoryPathBrowseButton_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog browseFolderBrowserDialog = new FolderBrowserDialog())
			{
				browseFolderBrowserDialog.Description = "Select Git repository";
				browseFolderBrowserDialog.ShowNewFolderButton = false;

				if (!string.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))

					browseFolderBrowserDialog.SelectedPath = Properties.Settings.Default.GitRepositoryPath;

				if (browseFolderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					Properties.Settings.Default.GitRepositoryPath = browseFolderBrowserDialog.SelectedPath;
					Properties.Settings.Default.Save();
					this.RefreshPath();
				}
			}
		}
		//---------------------------------------------------------------------
		private void GenerateButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Properties.Settings.Default.GitPath) ||
				string.IsNullOrEmpty(Properties.Settings.Default.GraphvizPath) ||
				string.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))
			{
				MessageBox.Show("Please select a Git, Graphviz & Git repository.", "Generate", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			OutputFormat outputFormat = (OutputFormat)Enum.Parse(typeof(OutputFormat), outputFormatListBox.SelectedItem as string);

			StatusRichTextBox.Text = "";
			_generator.Generate(GitRepositoryPathTextBox.Text, outputFormat, IsCompressHistoryCheckBox.Checked);
		}
		//---------------------------------------------------------------------
		private void HomepageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://github.com/gfoidl/GitVersionTree");
		}
		//---------------------------------------------------------------------
		private void ExitButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		//---------------------------------------------------------------------
		private void RefreshPath()
		{
			if (!string.IsNullOrEmpty(Properties.Settings.Default.GitPath))
				GitPathTextBox.Text = Properties.Settings.Default.GitPath;

			if (!string.IsNullOrEmpty(Properties.Settings.Default.GraphvizPath))
				GraphvizDotPathTextBox.Text = Properties.Settings.Default.GraphvizPath;

			if (!string.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))
				GitRepositoryPathTextBox.Text = Properties.Settings.Default.GitRepositoryPath;
		}
		//---------------------------------------------------------------------
		private void Generator_StatusUpdated(object sender, Utils.StatusEventArgs e)
		{
			StatusRichTextBox.AppendText(DateTime.Now + " - " + e.Message + "\r\n");
			StatusRichTextBox.SelectionStart = StatusRichTextBox.Text.Length;
			StatusRichTextBox.ScrollToCaret();
			Refresh();
		}
	}
}