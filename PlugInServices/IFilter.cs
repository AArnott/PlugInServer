using System;
using System.IO;

namespace Byu.IT347.PluginServer.PluginServices
{
	/// <summary>
	/// Describes the appearance of a filter plugin from the server's point of view.
	/// </summary>
	public interface IFilter : IPlugin
	{
		void FilterRequest(Stream request);
		void FilterResponse(Stream response);
	}
}
