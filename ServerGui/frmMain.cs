using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Byu.IT347.PluginServer.ServerServices;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerGui
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ImageList toolbarImageList;
		private System.Windows.Forms.ToolBarButton btnStart;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton btnPause;
		private System.Windows.Forms.ToolBarButton btnStop;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton btnProperties;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.TextBox txtOutput;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;

		protected Services Services = new Services();

		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Redirect STDOUT to text box
			TextWriter tw = new TextWriterBox(txtOutput);
			Console.SetOut(tw);
			Console.SetError(tw);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.btnStart = new System.Windows.Forms.ToolBarButton();
			this.btnPause = new System.Windows.Forms.ToolBarButton();
			this.btnStop = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.btnProperties = new System.Windows.Forms.ToolBarButton();
			this.toolbarImageList = new System.Windows.Forms.ImageList(this.components);
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// toolBar1
			// 
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton1,
																						this.btnStart,
																						this.btnPause,
																						this.btnStop,
																						this.toolBarButton2,
																						this.btnProperties});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.ImageList = this.toolbarImageList;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(546, 28);
			this.toolBar1.TabIndex = 3;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// btnStart
			// 
			this.btnStart.ImageIndex = 0;
			this.btnStart.ToolTipText = "Start the plug-in server";
			// 
			// btnPause
			// 
			this.btnPause.ImageIndex = 1;
			this.btnPause.ToolTipText = "Pause the plug-in server";
			// 
			// btnStop
			// 
			this.btnStop.ImageIndex = 2;
			this.btnStop.Pushed = true;
			this.btnStop.ToolTipText = "Stop the plug-in server";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// btnProperties
			// 
			this.btnProperties.ImageIndex = 4;
			// 
			// toolbarImageList
			// 
			this.toolbarImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.toolbarImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("toolbarImageList.ImageStream")));
			this.toolbarImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 290);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.ShowPanels = true;
			this.statusBar1.Size = new System.Drawing.Size(546, 22);
			this.statusBar1.TabIndex = 4;
			this.statusBar1.Text = "statusBar1";
			// 
			// txtOutput
			// 
			this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOutput.Location = new System.Drawing.Point(0, 28);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtOutput.Size = new System.Drawing.Size(546, 262);
			this.txtOutput.TabIndex = 5;
			this.txtOutput.Text = "";
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.Description = "Plug-in folder";
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(546, 312);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.toolBar1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Plug-in Server";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if( e.Button == btnStart && Services.Status != Status.Running )
				Services.Start();
			else if( e.Button == btnStop && Services.Status != Status.Stopped )
				Services.Stop();
			else if( e.Button == btnPause && Services.Status == Status.Running )
				Services.Pause();
			else if( e.Button == btnProperties )
			{
				folderBrowserDialog1.SelectedPath = Services.PluginDirectory;
				if( folderBrowserDialog1.ShowDialog(this) == DialogResult.OK )
					Services.PluginDirectory = folderBrowserDialog1.SelectedPath;
			}

			btnStart.Pushed = Services.Status == Status.Running;
			btnPause.Pushed = Services.Status == Status.Paused;
			btnStop.Pushed = Services.Status == Status.Stopped;
		}

		private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if( Services.Status != Status.Stopped )
				Services.Stop();
		}
	}
}
