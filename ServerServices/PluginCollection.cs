using System;
using System.Collections;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	public class PluginCollection
	{
		#region Construction
		public PluginCollection()
		{
		}
		#endregion

		#region Attributes
		private ArrayList plugins = new ArrayList();
		#endregion

		#region Operations
		public void Add(IPlugin plugin)
		{
			plugins.Add( plugin );
		}
		public void Clear()
		{
			plugins.Clear();
		}
		#endregion
	}
}
