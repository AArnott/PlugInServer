using System;
using System.IO;

namespace Byu.IT347.PluginServer.PluginServices
{
	public interface IHandler : IPlugin
	{
		void HandleRequest(Stream request, Stream response);
	}
}
