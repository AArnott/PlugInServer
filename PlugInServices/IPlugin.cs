using System;
using System.IO;

namespace Byu.IT347.PluginServer.PluginServices
{
	public enum PluginType
	{
		Handler,
		Filter
	}

	public interface IPlugin
	{
		string Name { get; }
		int[] Ports { get; }

		void Startup(IServer server);
		void Shutdown();

		bool CanProcessRequest(string url);
	}
}
