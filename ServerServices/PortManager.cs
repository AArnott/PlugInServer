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
		}
		#endregion

		#region Attributes
		protected readonly Services Services;
		public int[] DesiredPorts 
		{
			get
			{
				ArrayList ports = new ArrayList(Services.pluginAppDomains.Count);
				foreach( PluginAppDomain appDomain in Services.pluginAppDomains )
				{
					foreach( IHandler handler in appDomain.Handlers )
					{
						foreach( int port in handler.Ports )
							if( !ports.Contains(port) )
								ports.Add(port);
					}
				}
				return (int[]) ports.ToArray(typeof(int));
			}
		}
		protected Socket[] sockets;
		public Socket[] Sockets { get { return sockets; } }
		#endregion

		#region Operations
		public void OpenPorts()
		{
			if( Sockets != null ) throw new InvalidOperationException("Ports already open.");

			ArrayList sockets = new ArrayList();
			foreach( int port in DesiredPorts )
			{
				try 
				{
					Socket s = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
					s.Bind( new IPEndPoint(IPAddress.Any, port) );
					s.Listen(MaxConnectionQueueSize);
					sockets.Add(s);
				}
				catch( SocketException e )
				{
					if( e.ErrorCode == PortUnavailableErrorCode )
						Console.Error.WriteLine("Port {0} unavailable.", port);
					else
						throw;
				}
			}
			this.sockets = (Socket[]) sockets.ToArray(typeof(Socket));
			Console.WriteLine("Now listening on ports: {0}", intarraytostring(Sockets));
		}
		private string intarraytostring(Socket[] array)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach( Socket s in array )
				sb.Append(((IPEndPoint)s.LocalEndPoint).Port + ", ");
			if( sb.Length >= 2 )
				sb.Length -= 2;
			return sb.ToString();
		}
		public void ClosePorts()
		{
			if( Sockets == null ) return;
			foreach( Socket socket in Sockets )
				socket.Close();
			sockets = null;
		}
		#endregion
	}
}
