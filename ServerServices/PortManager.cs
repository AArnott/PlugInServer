using System;
using System.Collections;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Summary description for PortManager.
	/// </summary>
	public class PortManager
	{
		#region Construction
		public PortManager(Services services)
		{
			Services = services;
		}
		#endregion

		#region Attributes
		protected readonly Services Services;
		public int[] Ports 
		{
			get
			{
				ArrayList ports = new ArrayList(Services.pluginAppDomains.Count);
				foreach( PluginAppDomain appDomain in Services.pluginAppDomains )
				{
					foreach( IHandler handler in appDomain.Handlers )
					{
						foreach( int port in handler.Ports )
							if( !ports.Contains(port) )
								ports.Add(port);
					}
				}
				return (int[]) ports.ToArray(typeof(int));
			}
		}
		#endregion

		#region Operations
		#endregion
	}
}
