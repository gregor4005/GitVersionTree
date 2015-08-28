using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitVersionTree.Services;

namespace GitVersionTree
{
	public partial class MainForm : Form
	{
		private string _dotFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), Application.ProductName + ".dot");
		private string _pdfFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), Application.ProductName + ".pdf");
		private string _logFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), Application.ProductName + ".log");
		private string _repositoryName;
		private Generator _generator = new Generator();
		//---------------------------------------------------------------------
		public MainForm()
		{
			InitializeComponent();
			_generator.StatusUpdated += Generator_StatusUpdated;
		}
		//---------------------------------------------------------------------
		private void MainForm_Load(object sender, EventArgs e)
		{
			Text = Application.ProductName + " - v" + Application.ProductVersion.Substring(0, 3);

			this.RefreshPath();
		}
		//---------------------------------------------------------------------
		private void GitPathBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog browseOpenFileDialog = new OpenFileDialog();
			browseOpenFileDialog.Title = "Select git.exe";
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GitPath))
			{
				browseOpenFileDialog.InitialDirectory = Properties.Settings.Default.GitPath;
			}
			browseOpenFileDialog.FileName = "git.exe";
			browseOpenFileDialog.Filter = "Git Application (git.exe)|git.exe";
			if (browseOpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				Properties.Settings.Default.GitPath = browseOpenFileDialog.FileName;
				Properties.Settings.Default.Save();
				RefreshPath();
			}
		}
		//---------------------------------------------------------------------
		private void GraphvizDotPathBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog browseOpenFileDialog = new OpenFileDialog();
			browseOpenFileDialog.Title = "Select dot.exe";
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GraphvizPath))
			{
				browseOpenFileDialog.InitialDirectory = Properties.Settings.Default.GraphvizPath;
			}
			browseOpenFileDialog.FileName = "dot.exe";
			browseOpenFileDialog.Filter = "Graphviz Dot Application (dot.exe)|dot.exe";
			if (browseOpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				Properties.Settings.Default.GraphvizPath = browseOpenFileDialog.FileName;
				Properties.Settings.Default.Save();
				RefreshPath();
			}
		}
		//---------------------------------------------------------------------
		private void GitRepositoryPathBrowseButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog browseFolderBrowserDialog = new FolderBrowserDialog();
			browseFolderBrowserDialog.Description = "Select Git repository";
			browseFolderBrowserDialog.ShowNewFolderButton = false;
			if (!String.IsNullOrEmpty(Properties.Settings.Default.GitRepositoryPath))
			{
				browseFolderBrowserDialog.SelectedPath = Properties.Settings.Default.GitRepositoryPath;
			}
			if (browseFolderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				Properties.Settings.Default.GitRepositoryPath = browseFolderBrowserDialog.SelectedPath;
				Properties.Settings.Default.Save();
				RefreshPath();
			}
		}
		//---------------------------------------------------------------------
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
				_repositoryName = new DirectoryInfo(GitRepositoryPathTextBox.Text).Name;
				_dotFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), _repositoryName + ".dot");
				_pdfFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), _repositoryName + ".pdf");
				_logFilename = Path.Combine(Directory.GetParent(Application.ExecutablePath).ToString(), _repositoryName + ".log");
				File.WriteAllText(_logFilename, "");
				_generator.Generate(_repositoryName, _dotFilename, _pdfFilename, _logFilename, IsCompressHistoryCheckBox.Checked);
			}
		}
		//---------------------------------------------------------------------
		private void HomepageLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://github.com/crc8/GitVersionTree");
		}
		//---------------------------------------------------------------------
		private void ExitButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		//---------------------------------------------------------------------
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