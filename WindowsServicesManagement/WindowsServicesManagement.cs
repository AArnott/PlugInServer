using System;
using System.IO;
using System.ServiceProcess;
using System.Net.Sockets;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.Plugins.WindowsServicesManagement
{
	public class WindowsServicesManagement : MarshalByRefObject, IHandler
	{
		public const string ServiceName = "Windows Services Management";
		internal const int PreferredPort = 9910;

		#region Construction
		public WindowsServicesManagement()
		{
		}
		#endregion

		#region IHandler Members

		public void HandleRequest(NetworkStream stream)
		{
			StreamReader reader = new StreamReader(stream);
			StreamWriter writer = new StreamWriter(stream);

			WriteHeaders(writer);
			writer.WriteLine("<html><body>");
			writer.WriteLine("<table>");
			
			foreach( ServiceController service in ServiceController.GetServices() )
			{
				writer.WriteLine("<tr><td>{0}</td><td>{1}</td></tr>", service.DisplayName, service.Status.ToString());
			}
			
			writer.WriteLine("</table>");
			writer.WriteLine("</body></html>");
		}

		protected void WriteHeaders(StreamWriter writer)
		{
			writer.WriteLine("HTTP/1.1 200/OK");
			writer.WriteLine("Content-Type: text/html");
			writer.WriteLine();

		}

		#endregion

		#region IPlugin Members

		public bool CanProcessRequest(string url)
		{
			return true;
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
