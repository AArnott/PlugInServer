using System;
using System.IO;

namespace Byu.IT347.PluginServer.PluginServices
{
	/// <summary>
	/// Allows to easily differentiate between types and purposes of plugins.
	/// </summary>
	public enum PluginType
	{
		/// <summary>
		/// This type of plugin handles incoming network requests.
		/// </summary>
		Handler,
		/// <summary>
		/// This type of plugin intercepts incoming requests to modify their 
		/// up and/or down stream communications.
		/// </summary>
		Filter
	}

	/// <summary>
	/// Describes the appearance of any plugin from the server's point of view.
	/// </summary>
	public interface IPlugin
	{
		/// <summary>
		/// Gets the user-friendly name of the plugin.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets the list of ports that the plugin wishes to listen on.
		/// </summary>
		int[] Ports { get; }

		/// <summary>
		/// Initializes the plugin.
		/// </summary>
		/// <param name="server">
		/// The hosting server.
		/// </param>
		void Startup(IServer server);
		/// <summary>
		/// Shuts the plugin down.
		/// </summary>
		void Shutdown();
	}
}
