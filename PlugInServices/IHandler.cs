using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.PluginServices
{
	/// <summary>
	/// Describes the appearance of an incoming request handler plugin 
	/// from the server's point of view.
	/// </summary>
	public interface IHandler : IPlugin
	{
		void HandleRequest( NetworkStream stream, IPEndPoint local, IPEndPoint remote );
	}

	/// <summary>
	/// Describes the appearance of an incoming request handler plugin 
	/// that can share ports with other plugins.
	/// </summary>
	public interface ISharingHandler : IHandler
	{
		bool CanProcessRequest(string firstLine);
		void HandleRequest( NetworkStream stream, string firstLine, IPEndPoint local, IPEndPoint remote );
	}

	/// <summary>
	/// Utilities for the benefit of plugins.
	/// </summary>
	public class StreamHelps
	{
		// prevents the default constructor from being added by the compiler
		private StreamHelps() {}
		/// <summary>
		/// Reads a line from the stream in a way such that it does not create a
		/// <see cref="StreamReader"/>.
		/// </summary>
		/// <param name="stream">
		/// The <see cref="NetworkStream"/> to read from.
		/// </param>
		/// <returns>
		/// The line read from the stream.
		/// </returns>
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
