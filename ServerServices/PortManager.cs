using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Summary description for PortManager.
	/// </summary>
	public class PortManager
	{
		public const int MaxConnectionQueueSize = 10;
		protected const int PortUnavailableErrorCode = 10048;
		protected const int PortAccessDeniedErrorCode = 10013;

		#region Construction
		public PortManager(Services services)
		{
			Services = services;
			Services.PluginManager.Changed += new EventHandler(PluginManager_Changed);
		}
		#endregion

		#region Attributes
		protected readonly Services Services;
		private Status status = Status.Stopped;
		public Status Status
		{
			get
			{
				return status;
			}
			set
			{
				switch( value )
				{
					case Status.Running:
						RefreshSockets();
						break;
					case Status.Stopped:
						CloseAllSockets();
						break;
					case Status.Paused:
						break;
				}
				status = value;
			}
		}
		protected int[] PortsList
		{
			get
			{
				ArrayList ports = new ArrayList(Services.Count);
				foreach( IPlugin plugin in Services )
				{
					if( !(plugin is IHandler) ) continue;
					IHandler handler = (IHandler) plugin;
					foreach( int port in handler.Ports )
						if( !ports.Contains(port) )
							ports.Add(port);
				}
				return (int[]) ports.ToArray(typeof(int));
			}
		}

		protected PortSocketMap OpenSockets = new PortSocketMap();
		#endregion

		#region Events
		public delegate void IncomingRequestEventHandler(NetworkStream channel, IPEndPoint local, IPEndPoint remote);
		/// <summary>
		/// Fired whenever a request comes in through an open socket.
		/// </summary>
		public event IncomingRequestEventHandler IncomingRequest;
		/// <summary>
		/// Fires the <see cref="IncomingRequest"/> event.
		/// </summary>
		protected void OnIncomingRequest(NetworkStream channel, IPEndPoint local, IPEndPoint remote)
		{
			if( channel == null ) throw new ArgumentNullException("channel");
			if( local == null ) throw new ArgumentNullException("local");
			if( remote == null ) throw new ArgumentNullException("remote");

			IncomingRequestEventHandler incomingRequest = IncomingRequest;
			if( incomingRequest == null ) return; // no handlers
			incomingRequest(channel, local, remote);
		}
		#endregion

		#region Operations
		public IHandler[] ListHandlersOnPort(int port)
		{
			ArrayList handlers = new ArrayList(Services.Count);
			foreach( IPlugin plugin in Services )
			{
				if( !(plugin is IHandler) ) continue;
				IHandler handler = (IHandler) plugin;
				foreach( int portTest in handler.Ports )
					if( port == portTest )
					{
						if( handlers.Count == 0 || handler is ISharingHandler )
							handlers.Add( handler );
						break;
					}
			}
			IHandler[] handlersArray = new IHandler[handlers.Count];
			for( int i = 0; i < handlersArray.Length; i++ )
				handlersArray[i] = (IHandler) handlers[i];
			return handlersArray;
		}
		protected void OpenNewSockets()
		{
			foreach( int port in PortsList )
			{
				try 
				{
					if( OpenSockets[port] == null )
						OpenSockets.Add( OpenListeningSocket(port) );
				}
				catch( SocketException e )
				{
					switch( e.ErrorCode ) 
					{
						case PortUnavailableErrorCode:
							Console.Error.WriteLine("Port {0} unavailable.", port);
							break;
						case PortAccessDeniedErrorCode:
							Console.Error.WriteLine("Port {0} access denied.", port);
							break;
						default:
							Console.Error.WriteLine("Error {0} while opening port {1}.", e.ErrorCode, port);
						throw;
					}
				}				
			}
		}

		protected void CloseOldSockets()
		{
			ArrayList portsToKeepOpen = new ArrayList();
			portsToKeepOpen.AddRange( PortsList );
			foreach( int openPort in OpenSockets.AllPorts )
			{
				if( !portsToKeepOpen.Contains(openPort) )
				{
					if( OpenSockets[openPort].Connected ) 
						OpenSockets[openPort].Shutdown(SocketShutdown.Both);
					OpenSockets[openPort].Close();
					OpenSockets.Remove(openPort);
				}
			}
		}
		protected void CloseAllSockets()
		{
			foreach( Socket socket in OpenSockets.AllSockets )
			{
				OpenSockets.Remove(socket);
				if( socket.Connected ) 
					socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			}
			Console.WriteLine("Sockets all closed.");
		}
		protected void RefreshSockets()
		{
			CloseOldSockets();
			OpenNewSockets();
			if( OpenSockets.Count > 0 )
				Console.WriteLine("Sockets now open: {0}", string.Join(", ", OpenSockets.AllPortsAsStrings));
			else
				Console.WriteLine("Sockets all closed.");
		}

		protected Socket OpenListeningSocket(int port)
		{
			if( port < 1 || port > 65535 ) throw new ArgumentOutOfRangeException("port", port, "Valid range is 1-65535.");
			
			Socket s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			s.Bind( new IPEndPoint(IPAddress.Any, port) );
			s.Listen(MaxConnectionQueueSize);
			s.BeginAccept(new AsyncCallback(AcceptingSocket), s);
			return s;
		}
		protected virtual void AcceptingSocket(IAsyncResult result)
		{
			// recover the listening socket
			Socket listeningSocket = (Socket) result.AsyncState;

			Socket openedSocket;
			// the socket may be shutting down (thus calling this async callback), so catch it
			try 
			{
				// retrieve the newly opened connection
				openedSocket = listeningSocket.EndAccept(result);
			}
			catch( InvalidOperationException )
			{
				return;
			}

			// Initialize a new thread to handle this connection
			RequestHandler handler = new RequestHandler(this, openedSocket);
			(new Thread(new ThreadStart(handler.HandleRequest))).Start();

			// listen for a new connection right away.
			listeningSocket.BeginAccept(new AsyncCallback(AcceptingSocket), listeningSocket);
		}
		protected class RequestHandler
		{
			protected Socket Socket;
			protected PortManager PortManager;

			public RequestHandler(PortManager portManager, Socket socket)
			{
				this.Socket = socket;
				this.PortManager = portManager;
			}

			public virtual void HandleRequest()
			{
				try 
				{
					using( NetworkStream channel = new NetworkStream(Socket, false) )
					{
						PortManager.IncomingRequest(channel, (IPEndPoint)Socket.LocalEndPoint, (IPEndPoint)Socket.RemoteEndPoint);
						// eat remaining incoming data so that the connection will close
						while( channel.DataAvailable ) channel.ReadByte();
						channel.Close();
					}
				}
				catch( SocketException ex )
				{
					Console.Error.WriteLine("Socket error while processing request: {0}.\n{1}", ex.ErrorCode, ex.ToString());
					throw;
				}
				catch( IOException ex )
				{
					Console.Error.WriteLine("Error: {0}  Client probably disconnected prematurely.", ex.Message);
				}
				catch( Exception ex )
				{
					Console.Error.WriteLine("Error processing request: \n{0}", ex.ToString());
					throw;
				}
				finally
				{
					// close socket to end connection
					if( Socket.Connected ) 
					{
						Socket.Shutdown(SocketShutdown.Both);
						Socket.Close();
					}
				}
			}
		}
		#endregion

		#region Event handlers
		private void PluginManager_Changed(object sender, EventArgs e)
		{
			if( Status == Status.Running ) RefreshSockets();
		}
		#endregion
	}
}
