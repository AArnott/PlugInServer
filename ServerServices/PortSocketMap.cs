using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Summary description for PortSocketMap.
	/// </summary>
	public class PortSocketMap : NameObjectCollectionBase
	{
		#region Construction
		public PortSocketMap()
		{
		}
		#endregion

		#region Attributes
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
		
		public string[] AllPortsAsStrings
		{
			get
			{
				return BaseGetAllKeys();
			}
		}

		public Socket[] AllSockets
		{
			get
			{
				return (Socket[]) BaseGetAllValues(typeof(Socket));
			}
		}

		#endregion

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
		public void Add(Socket socket)
		{
			BaseAdd(((IPEndPoint)socket.LocalEndPoint).Port.ToString(), socket);
		}

		public void Remove(int port)
		{
			BaseRemove(port.ToString());
		}

		public void Remove(Socket socket)
		{
			Remove(((IPEndPoint)socket.LocalEndPoint).Port);
		}

		public void Clear()
		{
			BaseClear();
		}
		#endregion
	}
}
