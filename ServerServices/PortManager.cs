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

		#region Construction
		public PortManager(Services services)
		{
			Services = services;
		}
		#endregion

		#region Attributes
		protected readonly Services Services;
		public int[] Ports 
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
		public Socket[] Sockets;
		#endregion

		#region Operations
		public void OpenPorts()
		{
			if( Sockets != null ) throw new InvalidOperationException("Ports already open.");

			ArrayList sockets = new ArrayList();
			foreach( int port in Ports )
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
					Console.Error.WriteLine(e.ToString());
				}
			}
			Sockets = (Socket[]) sockets.ToArray(typeof(Socket));
		}
		public void ClosePorts()
		{
			if( Sockets == null ) return;
			foreach( Socket socket in Sockets )
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			}
			Sockets = null;
		}
		#endregion
	}
}
