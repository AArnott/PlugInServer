using System;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.Plugins.PHP
{
	
	/// Plugin for dynamic server to handle PHP requests	
	public class PHPplugin : IHandler
	{
		
		#region IHandler Members
		
		static int counter = 0;
		public void HandleRequest( NetworkStream stream, IPEndPoint local, IPEndPoint remote )
		{
			StreamReader sr = new StreamReader(stream);
			string url = sr.ReadLine();
			Console.WriteLine(url);
			

			
			StreamWriter sw = new StreamWriter(stream);
			sw.WriteLine("HTTP/1.0 200 OK\r\nContent-type: text/html\r\n\r\nHello, {1}! {0}", 
				counter++, remote.Address.ToString());
			sw.Flush();
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
				return new int[] {80};
			}
		}

		
		public void Startup(IServer server)
		{
			
		}

		public void Shutdown()
		{

		}

		public bool CanProcessRequest(string url)
		{
			if(url.IndexOf(".php") <= 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		#endregion
	
	}
}
