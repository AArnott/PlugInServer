using System;
using Byu.IT347.PluginServer.PluginServices;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.Plugins.StaticHtml
{
	public class StaticHtmlPlugIn : MarshalByRefObject, IHandler
	{
		public StaticHtmlPlugIn()
		{
		}
		#region IHandler Members

		public void HandleRequest(NetworkStream stream)
		{
			// TODO:  Add StaticHtmlPlugIn.HandleRequest implementation
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
				return new int[] { 80 };
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
