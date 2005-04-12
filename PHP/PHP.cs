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
	
	/// <summary>
	/// This plugin handels and request on the specified ports containing .php.  
	/// Plugin is dependant on PHP 5.0 installation.  
	/// Use master configuration file to change file locations.
	/// Date: 4/1/2005
	/// Author: Jason B. Smith
	/// </summary>
	public class PHPplugin : MarshalByRefObject, ISharingHandler
	{
		internal const int DefaultPort = 8080;

		#region ISharingHandler Members
		/// <summary>
		/// Specifies how this plugin will handle a request.  Sends the information or file requested by the client to the server.
		/// </summary>
		/// <param name="stream">The stream from the server that is used to transfer data</param>
		/// <param name="firstLine">The first line of the request from the client</param>
		/// <param name="local"></param>
		/// <param name="remote"></param>
		public void HandleRequest(NetworkStream stream, string firstLine, IPEndPoint local, IPEndPoint remote)
		{
			StreamReader sr = new StreamReader(stream);
			StreamWriter sw = new StreamWriter(stream);
			string urlrequest = firstLine;
			
			int start = urlrequest.IndexOf("/")+1;
			int end = urlrequest.IndexOf(" ",start);
			string url = urlrequest.Substring(start,end-start);
			///creates psi object which allows the redirection of stdin and stdout.
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

		/// <summary>
		/// Method called by server to determine if this plugin can handle a specific request.
		/// </summary>
		/// <param name="firstLine">The first line of the request from the client.</param>
		/// <returns>Returns true if the plugin can handle the specific request, false if it cannot.</returns>
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
		/// <summary>
		/// Depricated method, see the HandleRequest method above
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="local"></param>
		/// <param name="remote"></param>
		public void HandleRequest( NetworkStream stream, IPEndPoint local, IPEndPoint remote )
		{
			HandleRequest(stream, StreamHelps.ReadLine(stream), local, remote);
		}

		#endregion

		#region IPlugin Members
		/// <summary>
		/// Returns the name of this plugin
		/// </summary>
		public string Name 
		{ 
			get
			{
				return "PHP";
			}
		}
		
		/// <summary>
		/// Method used to facilitate port sharing
		/// </summary>
		private int ActivePort
		{
			get
			{
				string sPort = System.Configuration.ConfigurationSettings.AppSettings["HttpPort"];
				return (sPort != null) ? Convert.ToInt32(sPort) : DefaultPort;
			}
		}
		/// <summary>
		/// Returns an array of the ports used by this plugin.
		/// </summary>
		public int[] Ports 
		{ 
			get
			{
				return new int[] { ActivePort };
			}
		}

		/// <summary>
		/// Method called to load the plugin
		/// </summary>
		/// <param name="server">Server in which the plugin will be loaded</param>
		public void Startup(IServer server)
		{
			
		}
		/// <summary>
		/// Method called to unload the plugin when it is removed.
		/// </summary>
		public void Shutdown()
		{

		}

		#endregion

	}
}
