using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.PluginServices
{
	public interface IHandler : IPlugin
	{
		void HandleRequest( NetworkStream stream, IPEndPoint local, IPEndPoint remote );
	}
}
