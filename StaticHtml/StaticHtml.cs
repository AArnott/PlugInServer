using System;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.Plugins.StaticHtml
{
	public class StaticHtmlPlugIn : MarshalByRefObject, IHandler
	{
		public StaticHtmlPlugIn()
		{
		}
		#region IHandler Members

		static int counter = 0;
		public void HandleRequest(NetworkStream channel, IPEndPoint local, IPEndPoint remote)
		{
			StreamReader sr = new StreamReader(channel);
			string url = sr.ReadLine();
			Console.WriteLine(url);
			if( url.StartsWith("GET /favicon.ico ") ) return; // ignore favicon requests
			StreamWriter sw = new StreamWriter(channel);
			sw.WriteLine("HTTP/1.0 200 OK\r\nContent-type: text/html\r\n\r\nHello, {1}! {0}", 
				counter++, remote.Address.ToString());
			sw.Flush();
		}

		#endregion

		#region IPlugin Members

		public bool CanProcessRequest(string url)
		{
			// TODO:  Add StaticHtmlPlugIn.CanProcessRequest implementation
			return false;
		}

		public int[] Ports
		{
			get
			{
				return new int[] { 80, 8080 };
			}
		}

		public void Shutdown()
		{
			// TODO:  Add StaticHtmlPlugIn.Shutdown implementation
		}

		public string Name
		{
			get
			{
				return "Static HTML server";
			}
		}

		public void Startup(IServer server)
		{
			// TODO:  Add StaticHtmlPlugIn.Startup implementation
		}

		#endregion
	}
}
