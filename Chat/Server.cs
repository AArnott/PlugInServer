using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.Plugins.Chat
{
	/// <summary>
	/// Summary description for Server.
	/// </summary>
	
	public class Server : MarshalByRefObject, IHandler
	{
		internal const int DefaultPort = 9020;
		public const int DefaultMaxConnections = 20;
		public const int HistorySize = 100;
		public const string EndOfLine = "\r\n";

		#region Construction
		public Server() :
			this( DefaultMaxConnections )
		{
		}

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
/*		public void Start()
		{
			if( runningThread != null ) throw new InvalidOperationException("Already running!");
			runningThread = new Thread(new ThreadStart(Run));
			runningThread.Start();
		}
		public void Stop()
		{
			runningThread.Abort();
			runningThread = null;
		}
		private void Run()
		{
			TcpListener server = new TcpListener(IPAddress.Any, Port);

			// Start() will bind to the port and start queuing 
			// connection requests.
			server.Start();
			Console.WriteLine("Now waiting for connections...");

			try 
			{
				while( true )
					if( server.Pending() && Sessions.Count < MaximumConnections ) 
						Sessions.Add(server.AcceptTcpClient());
					else
						Thread.Sleep(100); // don't drown the CPU
			}
			catch( ThreadAbortException )
			{
				// just quietly quit.
			}
			finally
			{
				Sessions.CloseAll();
				server.Stop();
			}
		}*/
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
			// TODO:  Add Server.Shutdown implementation
			
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
