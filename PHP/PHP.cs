using System;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;
using System.Net;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Diagnostics;

namespace Byu.IT347.PluginServer.Plugins.PHP
{
	
	/// Plugin for dynamic server to handle PHP requests	
	public class PHPplugin : MarshalByRefObject, ISharingHandler
	{
		
		#region ISharingHandler Members

		public void HandleRequest(NetworkStream stream, string firstLine, IPEndPoint local, IPEndPoint remote)
		{
			StreamReader sr = new StreamReader(stream);
			StreamWriter sw = new StreamWriter(stream);
			string urlrequest = firstLine;
			//
			int start = urlrequest.IndexOf("/")+1;
			int end = urlrequest.IndexOf(" ",start);
			string url = urlrequest.Substring(start,end-start);
			//
			ProcessStartInfo psi = new ProcessStartInfo(
				System.Configuration.ConfigurationSettings.AppSettings["PHPInterpreterPath"], 
				Path.Combine(System.Configuration.ConfigurationSettings.AppSettings["PublicRoot"], url));
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
				
			Process proc = Process.Start(psi);
			sw.Write("HTTP/1.0 200 OK\r\n");
			while( !proc.HasExited )
				sw.Write(proc.StandardOutput.ReadToEnd());
			sw.Flush();
		}

		public bool CanProcessRequest(string firstLine)
		{
			if(firstLine.IndexOf(".php") <= 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region IHandler Members
		
		public void HandleRequest( NetworkStream stream, IPEndPoint local, IPEndPoint remote )
		{
			HandleRequest(stream, StreamHelps.ReadLine(stream), local, remote);
		}

		#endregion

		#region IPlugin Members
		
		public string Name 
		{ 
			get
			{
				return "PHP";
			}
		}
		
		
		public int[] Ports 
		{ 
			get
			{
				return new int[] {80,8080};
			}
		}

		
		public void Startup(IServer server)
		{
			
		}

		public void Shutdown()
		{

		}

		#endregion

	}
}
