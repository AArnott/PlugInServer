using System;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.Plugins.WindowsServicesManagement
{
	public class WindowsServicesManagement : IHandler
	{
		public const string ServiceName = "Windows Services Management";
		internal const int PreferredPort = 9910;

		#region Construction
		public WindowsServicesManagement()
		{
		}
		#endregion

		#region IHandler Members

		public void HandleRequest(System.IO.Stream request, System.IO.Stream response)
		{
			// TODO:  Add WindowsServicesManagement.HandleRequest implementation
		}

		#endregion

		#region IPlugin Members

		public bool CanProcessRequest(string url)
		{
			// TODO:  Add WindowsServicesManagement.CanProcessRequest implementation
			return false;
		}

		public int[] Ports
		{
			get
			{
				return new int[] { PreferredPort };
			}
		}

		public void Shutdown()
		{
			// TODO:  Add WindowsServicesManagement.Shutdown implementation
		}

		public string Name
		{
			get
			{
				return ServiceName;
			}
		}

		public void Startup(IServer server)
		{
			// TODO:  Add WindowsServicesManagement.Startup implementation
		}

		#endregion
	}
}
