using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

namespace Byu.IT347.PluginServer.AutoRun
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnLive;
		private System.Windows.Forms.Button btnInstall;
		private System.Windows.Forms.Button btnReadme;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.btnLive = new System.Windows.Forms.Button();
			this.btnInstall = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.btnReadme = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnLive
			// 
			this.btnLive.Location = new System.Drawing.Point(49, 72);
			this.btnLive.Name = "btnLive";
			this.btnLive.Size = new System.Drawing.Size(336, 32);
			this.btnLive.TabIndex = 1;
			this.btnLive.Text = "Run the Plugin Server live from the CD";
			this.btnLive.Click += new System.EventHandler(this.btnLive_Click);
			// 
			// btnInstall
			// 
			this.btnInstall.Location = new System.Drawing.Point(49, 112);
			this.btnInstall.Name = "btnInstall";
			this.btnInstall.Size = new System.Drawing.Size(336, 32);
			this.btnInstall.TabIndex = 2;
			this.btnInstall.Text = "Install the Plugin Server + 4 plugins";
			this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(422, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "You have just inserted the Plugin Server LiveCD + Install.  What do you want to d" +
				"o?";
			// 
			// btnReadme
			// 
			this.btnReadme.Location = new System.Drawing.Point(49, 32);
			this.btnReadme.Name = "btnReadme";
			this.btnReadme.Size = new System.Drawing.Size(336, 32);
			this.btnReadme.TabIndex = 0;
			this.btnReadme.Text = "Read the README";
			this.btnReadme.Click += new System.EventHandler(this.btnReadme_Click);
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(434, 160);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnInstall);
			this.Controls.Add(this.btnLive);
			this.Controls.Add(this.btnReadme);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Plugin Server LiveCD + Install";
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


		private void btnReadme_Click(object sender, System.EventArgs e)
		{
			Process.Start("readme.html");
		}

		private void btnLive_Click(object sender, System.EventArgs e)
		{
			Process.Start("live\\bin\\serverconsole.exe");
		}

		private void btnInstall_Click(object sender, System.EventArgs e)
		{
			Process.Start("setup.exe");
		}
	}
}
