using System;
using System.Collections;
using System.Reflection;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Finds and loads plugins in an assembly loaded in a remote <see cref="AppDomain"/>.
	/// </summary>
	public class PluginFinder : MarshalByRefObject
	{
		/// <summary>
		/// Creates an instance of the <see cref="PluginFinder"/> class.
		/// </summary>
		public PluginFinder()
		{
		}

		/// <summary>
		/// Finds and loads plugins in an assembly loaded in a remote <see cref="AppDomain"/>.
		/// </summary>
		/// <param name="assemblyRef">
		/// The assembly to search for plugins.
		/// </param>
		/// <returns>
		/// The list of loaded plugins.
		/// </returns>
		public IPlugin[] LoadPluginsInAssembly(AssemblyName assemblyRef)
		{
			Assembly assembly = AppDomain.CurrentDomain.Load(assemblyRef);
			
			// Look for plug-ins
			ArrayList plugins = new ArrayList();
			foreach( Type type in assembly.GetExportedTypes() )
			{
				if( !type.IsClass || type.IsAbstract ) continue; // only wholly-defined classes, please
				if( typeof(IPlugin).IsAssignableFrom(type) )
					plugins.Add( System.Activator.CreateInstance(type) );
			}

			return (IPlugin[]) plugins.ToArray(typeof(IPlugin));
		}
	}
}
