using System;
using System.Collections;
using System.Reflection;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	public class PluginFinder : MarshalByRefObject
	{
		public PluginFinder()
		{
		}

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
