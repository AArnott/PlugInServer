using System;
using System.IO;

namespace Byu.IT347.PluginServer.PluginServices
{
	public interface IFilter : IPlugin
	{
		void FilterRequest(Stream request);
		void FilterResponse(Stream response);
	}
}
