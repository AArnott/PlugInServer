using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net.Sockets;
using Byu.IT347.PluginServer.PluginServices;
using System.Web;
using System.Windows.Forms;

using System.Net;

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

		protected string UrlBase
		{
			get
			{
				return "http://localhost:" + PreferredPort.ToString() + "/";
			}
		}
		public void HandleRequest(NetworkStream stream)
		{
			if( stream == null ) throw new ArgumentNullException("stream");

			StreamReader reader = new StreamReader(stream);
			StreamWriter writer = new StreamWriter(stream);

			WriteHeaders(writer);
			writer.WriteLine("<html><body>");
			writer.WriteLine("<a href=\"{0}\">Refresh</a>", UrlBase);
			writer.WriteLine("<table>");
			writer.WriteLine("<thead><tr><th>Service Name</th><th>Status</th><th>Actions</th></tr></thead>");
			writer.WriteLine("<tbody>");

			Match task = Regex.Match(GetUrlRequested(reader), @"(?<verb>\w+)=(?<service>.+)");
			switch( task.Groups["verb"].Value )
			{
				case "log":
					EventLog.WriteEntry("Lab 7", task.Groups["service"].Value, EventLogEntryType.Information);
					break;
				case "msg":
					Process p = Process.Start("net", "send " + SystemInformation.ComputerName + " " + task.Groups["service"].Value);
					p.WaitForExit(5000);
					break;
			}

			foreach( ServiceController service in ServiceController.GetServices() )
			{
				if( task.Success && service.ServiceName == task.Groups["service"].Value )
				{
					ServiceControllerStatus desiredStatus;
					try 
					{
						switch( task.Groups["verb"].Value )
						{
							case "start":
								service.Start();
								desiredStatus = ServiceControllerStatus.Running;
								break;
							case "pause":
								service.Pause();
								desiredStatus = ServiceControllerStatus.Paused;
								break;
							case "resume":
								service.Continue();
								desiredStatus = ServiceControllerStatus.Running;
								break;
							case "stop":
								service.Stop();
								desiredStatus = ServiceControllerStatus.Stopped;
								break;
							default:
								throw new ApplicationException("Unsupported action verb.");
						}
						service.WaitForStatus(desiredStatus, TimeSpan.FromSeconds(3));
						writer.WriteLine("<tr style=\"background-color: cyan\">");
					}
					catch( TimeoutException )
					{
						writer.WriteLine("<tr style=\"background-color: yellow\">");
					}
					catch( InvalidOperationException )
					{
						writer.WriteLine("<tr style=\"background-color: red\">");
					}
				}
				else if( (task.Groups["verb"].Value == "log" && service.ServiceName == "Eventlog") ||
					(task.Groups["verb"].Value == "msg" && service.ServiceName == "Messenger") )
					writer.WriteLine("<tr style=\"background-color: cyan\">");
				else					
					writer.WriteLine("<tr>");
				writer.WriteLine("<td>{0}</td>", service.DisplayName);
				writer.WriteLine("<td title=\"{1}\">{0}</td>", service.Status, service.ServiceName);
				writer.Write("<td>");
				switch( service.Status )
				{
					case ServiceControllerStatus.Stopped:
						writer.Write("<a href=\"{0}\">Start</a>", UrlBase + "start=" + HttpUtility.UrlEncode(service.ServiceName));
						break;
					case ServiceControllerStatus.Running:
						if( service.CanPauseAndContinue )
							writer.Write("<a href=\"{0}\">Pause</a> ", UrlBase + "pause=" + HttpUtility.UrlEncode(service.ServiceName));
						if( service.CanStop )
							writer.Write("<a href=\"{0}\">Stop</a>", UrlBase + "stop=" + HttpUtility.UrlEncode(service.ServiceName));
						break;
					case ServiceControllerStatus.Paused:
						writer.Write("<a href=\"{0}\">Resume</a> ", UrlBase + "resume=" + HttpUtility.UrlEncode(service.ServiceName));
						if( service.CanStop )
							writer.Write("<a href=\"{0}\">Stop</a>", UrlBase + "stop=" + HttpUtility.UrlEncode(service.ServiceName));
						break;
				}
				writer.Write("</td>");
				switch( service.ServiceName )
				{
					case "Eventlog":
						if( service.Status != ServiceControllerStatus.Running ) break;
						writer.Write("<td>");
						writer.Write("<form action=\"{0}\">", UrlBase);
						writer.WriteLine("Log entry: <input name=log> <input type=submit value=log>");
						writer.Write("</form>");
						writer.Write("</td>");
						break;
					case "Messenger":
						if( service.Status != ServiceControllerStatus.Running ) break;
						writer.Write("<td>");
						writer.Write("<form action=\"{0}\">", UrlBase);
						writer.WriteLine("say: <input name=msg> <input type=submit value=send>");
						writer.Write("</form>");
						writer.Write("</td>");
						break;
				}
				writer.WriteLine("</tr>");
			}
			writer.WriteLine("</tbody>");
			writer.WriteLine("</table>");
			writer.WriteLine("</body></html>");
			writer.Flush();
		}

		protected void WriteHeaders(TextWriter writer)
		{
			writer.WriteLine("HTTP/1.1 200 OK");
			writer.WriteLine("Content-Type: text/html");
			writer.WriteLine("Connection: close"); 
			writer.WriteLine();
		}
		protected string ReadHeaders(TextReader reader)
		{
			StringBuilder headers = new StringBuilder(100);
			string line;
			while( (line = reader.ReadLine()).Length > 0 )
				headers.Append(line + Environment.NewLine);
			return headers.ToString();
		}
		protected string[] SplitHeaders(TextReader reader)
		{
			return ReadHeaders(reader).Split(Environment.NewLine.ToCharArray());
		}
		protected string GetTopHeader(TextReader reader)
		{
			return SplitHeaders(reader)[0];
		}
		protected string GetUrlRequested(TextReader reader)
		{
			return HttpUtility.UrlDecode(GetTopHeader(reader).Split(' ')[1]);
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

		public static void Main()
		{
			WindowsServicesManagement handler = new WindowsServicesManagement();
			TcpListener listener = new TcpListener(IPAddress.Any, PreferredPort);
			listener.Start();
			try 
			{
				while( true )
				{
					TcpClient client = listener.AcceptTcpClient();
					handler.HandleRequest(client.GetStream());
					client.Close();
				}
			}
			finally
			{
				listener.Stop();
			}
		}
	}
}
