using System;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.Plugins.PHP
{
	/// <summary>
	/// Plugin for dynamic server to handle PHP requests
	/// </summary>
	public class PHPplugin : IHandler
	{
		public PHPplugin()
		{
		}
		
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
			return true;
		}

	}
}
