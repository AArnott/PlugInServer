using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Maps port numbers to open listening sockets.
	/// </summary>
	public class PortSocketMap : NameObjectCollectionBase
	{
		#region Construction
		/// <summary>
		/// Creates an instance of the <see cref="PortSocketMap"/> class.
		/// </summary>
		public PortSocketMap()
		{
		}
		#endregion

		#region Attributes
		/// <summary>
		/// Gets a list of all listening ports in the map.
		/// </summary>
		public int[] AllPorts 
		{
			get
			{
				string[] allkeys = BaseGetAllKeys();
				int[] ports = new int[allkeys.Length];
				for( int i = 0; i < ports.Length; i++ )
					ports[i] = int.Parse(allkeys[i]);
				return ports;
			}
		}
		
		/// <summary>
		/// Gets a list of all listening ports in the map, with all
		/// the port numbers cast as <see cref="String"/> objects.
		/// </summary>
		public string[] AllPortsAsStrings
		{
			get
			{
				return BaseGetAllKeys();
			}
		}

		/// <summary>
		/// Gets a list of all listening sockets.
		/// </summary>
		public Socket[] AllSockets
		{
			get
			{
				return (Socket[]) BaseGetAllValues(typeof(Socket));
			}
		}

		#endregion

		/// <summary>
		/// Gets the listening socket for a given port.
		/// </summary>
		public Socket this[ int port ]
		{
			get
			{
				return (Socket) BaseGet(port.ToString());
			}
			set
			{
				BaseSet(port.ToString(), value);
			}
		}

		#region Operations
		/// <summary>
		/// Adds a listening socket to the map.
		/// </summary>
		public void Add(Socket socket)
		{
			BaseAdd(((IPEndPoint)socket.LocalEndPoint).Port.ToString(), socket);
		}

		/// <summary>
		/// Removes a socket from the map.
		/// </summary>
		/// <param name="port">
		/// The port number of the socket to remove.
		/// </param>
		public void Remove(int port)
		{
			BaseRemove(port.ToString());
		}

		/// <summary>
		/// Removes a socket from the map.
		/// </summary>
		/// <param name="socket">
		/// The socket to remove.
		/// </param>
		public void Remove(Socket socket)
		{
			Remove(((IPEndPoint)socket.LocalEndPoint).Port);
		}

		/// <summary>
		/// Clears all ports and sockets from the map.
		/// </summary>
		public void Clear()
		{
			BaseClear();
		}
		#endregion
	}
}
