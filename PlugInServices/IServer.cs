using System;

namespace Byu.IT347.PluginServer.PluginServices
{
	public enum Status
	{
		Stopped,
		Running,
		Paused
	}

	public interface IServer
	{
		Status Status { get; }
		IPlugin[] Plugins { get; }

		void SurrenderPlugin(IPlugin Plugin);
	}
}
