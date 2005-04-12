using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.Plugins.Chat
{
	/// <summary>
	/// Server manages the requests from the Plugin Server and manages 
	/// clients and inter-client communication.
	/// </summary>
	
	public class Server : MarshalByRefObject, IHandler
	{
		/// <summary>
		/// The default port that chat clients will connect to.
		/// </summary>
		internal const int DefaultPort = 9020;
		/// <summary>
		/// Maximum number of allowable connections.
		/// </summary>
		public const int DefaultMaxConnections = 20;
		/// <summary>
		/// number of lines of conversation history that will be sent 
		/// to clients when they first connect.
		/// </summary>
		public const int HistorySize = 100;
		public const string EndOfLine = "\r\n";

		#region Construction
		public Server() :
			this( DefaultMaxConnections )
		{
		}
		/// <summary>
		/// Initializes the variables and objects required to keep track of clients.
		/// </summary>
		/// <param name="maximumConnections"></param>
		public Server(int maximumConnections)
		{
			//this.port = port;
			this.maximumConnections = maximumConnections;
			Sessions = new ClientSessions(MaximumConnections);
			
			Sessions.Opened += new EventHandler(Sessions_Opened);
			Sessions.Closed += new EventHandler(Sessions_Closed);
			Sessions.IncomingMessage += new EventHandler(Sessions_IncomingMessage);
			Console.WriteLine("Server Started.");
		}
		#endregion

		#region Attributes
		private int port;
		public int Port
		{
			get
			{
				return port;
			}
		}
		private int maximumConnections;
		public int MaximumConnections
		{
			get
			{
				return maximumConnections;
			}
		}
		public readonly ClientSessions Sessions;

		private Thread runningThread;
		public bool IsRunning 
		{
			get
			{
				return runningThread != null && runningThread.ThreadState == ThreadState.Running;
			}
		}
		#endregion

		#region Operations
		/// <summary>
		/// Broadcasts the message to all clients that are currently connected.
		/// </summary>
		/// <param name="message"></param>
		public void Broadcast(string message)
		{
			Sessions.Send(message);
			Console.WriteLine(message);
		}
		#endregion

		#region Events
		private void Sessions_Closed(object sender, EventArgs e)
		{
			Console.WriteLine("{0} has left.", ((ClientSession)sender).Name);
		}

		private void Sessions_Opened(object sender, EventArgs e)
		{
			Console.WriteLine("{0} has joined.", ((ClientSession)sender).Name);
		}

		private void Sessions_IncomingMessage(object sender, EventArgs e)
		{
			Console.WriteLine("{0} said: {1}", ((ClientSession)sender).Name, ((ClientSession)sender).LastMessageReceived);
		}
		#endregion

		#region IHandler Members

		public void HandleRequest(NetworkStream stream, IPEndPoint local, IPEndPoint remote)
		{
			if( Sessions.Count < MaximumConnections ) 
			{
				Console.WriteLine("Request received from " + remote);
				Sessions.Add(stream);//.Closed += new EventHandler(Server_Closed);

			}
			
		}

		#endregion

		#region IPlugin Members

		private int ActivePort
		{
			get
			{
				string sPort = System.Configuration.ConfigurationSettings.AppSettings["ChatPort"];
				return (sPort != null) ? Convert.ToInt32(sPort) : DefaultPort;
			}
		}

		public int[] Ports
		{
			get
			{
				return new int[] { ActivePort };
			}
		}

		public void Shutdown()
		{
			// Shut down
		}

		public string Name
		{
			get
			{
				return "ChatterBox";
			}
		}

		public void Startup(IServer server)
		{
			Console.WriteLine("Starting Server...");
			Server serv = new Server();
		}

		#endregion

		private void Server_Closed(object sender, EventArgs e)
		{

		}
	}
}
