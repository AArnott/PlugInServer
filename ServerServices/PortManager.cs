using System;
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

		#region Operations
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
					if( e.ErrorCode == PortUnavailableErrorCode )
						Console.Error.WriteLine("Port {0} unavailable.", port);
					else
						throw;
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
				socket.Close();
			}
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
			return s;
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
