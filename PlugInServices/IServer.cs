using System;
using System.Collections;

namespace Byu.IT347.PluginServer.PluginServices
{
	/// <summary>
	/// A common status enumerable.
	/// </summary>
	public enum Status
	{
		/// <summary>
		/// The service is essentially shutdown.  
		/// Nothing is being processed and the queue is empty.
		/// </summary>
		Stopped,
		/// <summary>
		/// The service is fully running.
		/// There may be processes running and any queue may have
		/// jobs in it.
		/// </summary>
		Running,
		/// <summary>
		/// The service is not accepting new requests,
		/// although existing processes may be finishing,
		/// and the queue may not be empty.
		/// </summary>
		Paused
	}

	/// <summary>
	/// Describes the view of the plugin server from a plugin's perspective.
	/// </summary>
	public interface IServer : IEnumerable
	{
		/// <summary>
		/// The current running status of the server.
		/// </summary>
		Status Status { get; }

		/// <summary>
		/// Asks the server to unload a given plugin.
		/// </summary>
		/// <param name="Plugin">
		/// The plugin to unload.
		/// </param>
		void SurrenderPlugin(IPlugin Plugin);
	}
}
