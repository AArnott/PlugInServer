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
				
		public void HandleRequest( NetworkStream stream, IPEndPoint local, IPEndPoint remote )
		{

		}

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
	}
}
