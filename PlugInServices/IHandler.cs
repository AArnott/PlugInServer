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

	public interface ISharingHandler : IHandler
	{
		bool CanProcessRequest(string firstLine);
		void HandleRequest( NetworkStream stream, string firstLine, IPEndPoint local, IPEndPoint remote );
	}

	public class StreamHelps
	{
		private StreamHelps() {}
		public static string ReadLine(NetworkStream stream)
		{
			System.Text.StringBuilder line = new System.Text.StringBuilder();
			int ich;
			while( (ich = stream.ReadByte()) > -1 )
			{
				char ch = (char) ich;
				if( ch == '\n' ) break;
				if( ch != '\r' )
					line.Append(ch);
			}
			return line.ToString();
		}
	}
}
