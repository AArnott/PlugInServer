using System;

namespace Byu.IT347.PluginServer.PluginServices
{
	public interface IServer
	{
		IPlugin[] Plugins { get; }

		void SurrenderPlugin(IPlugin Plugin);
	}
}
