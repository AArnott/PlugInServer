using System;
using System.Collections;

namespace Byu.IT347.PluginServer.PluginServices
{
	public enum Status
	{
		Stopped,
		Running,
		Paused
	}

	public interface IServer : IEnumerable
	{
		Status Status { get; }

		void SurrenderPlugin(IPlugin Plugin);
	}
}
