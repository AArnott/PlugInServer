using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	[Serializable]
	public class PluginAppDomainCollection
	{
		private const bool FileNamesAreCaseSensitive = false;
		#region Construction
		public PluginAppDomainCollection()
		{
			appdomains = new HybridDictionary(4, !FileNamesAreCaseSensitive);
		}
		#endregion

		#region Attributes
		private HybridDictionary appdomains;
		public bool Contains(string assemblyPath)
		{
			return appdomains.Contains(assemblyPath);
		}
		public bool Contains(AppDomain appdomain)
		{
			return appdomains.Contains(appdomain.FriendlyName);
		}
		public PluginAppDomain this[string assemblyPath]
		{
			get
			{
				return (PluginAppDomain) appdomains[assemblyPath];
			}
		}
		#endregion

		#region Operations
		public PluginAppDomain Load(string assemblyFilename)
		{
			try 
			{
				PluginAppDomain appdomain = new PluginAppDomain(AssemblyName.GetAssemblyName(assemblyFilename));
				// Keep track of the appdomain so we can unload it if the plugin changes or is removed.
				appdomains.Add( assemblyFilename, appdomain );
				return appdomain;
			}
			catch( Exception ex )
			{
				Console.Error.WriteLine("Error loading assembly: {0}" + Environment.NewLine + "{1}", assemblyFilename, ex.ToString());
				return null;
			}
		}
		public void Unload(string assemblyPath)
		{
			if( !Contains( assemblyPath ) ) return;
			PluginAppDomain appdomainToUnload = this[assemblyPath];
			appdomains.Remove( assemblyPath );

			appdomainToUnload.Unload();
		}
		public void Unload(AppDomain appdomain)
		{
			if( !Contains( appdomain ) ) return;
			appdomains.Remove( appdomain.FriendlyName );
			
			AppDomain.Unload( appdomain );
		}
		public void UnloadAll()
		{
			lock( appdomains )
			{
				foreach( AppDomain appdomain in appdomains.Values )
					Unload(appdomain);
			}
		}
		public PluginAppDomain[] LoadAll(string assemblyDirectory)
		{
			ArrayList domainsLoaded = new ArrayList();
			foreach( string filename in Directory.GetFiles(assemblyDirectory, "*.dll") )
			{
				PluginAppDomain appdomain = Load(filename);
				if( appdomain != null )
					domainsLoaded.Add( appdomain );
			}
			return (PluginAppDomain[]) domainsLoaded.ToArray(typeof(PluginAppDomain));
		}
		#endregion

		#region Event handlers
		private void pluginDomain_DomainUnload(object sender, EventArgs e)
		{
			Console.WriteLine("Plug-in appdomain unloading.");

			try 
			{
				// Any plugins being hosted from this app domain should be unloaded.
				// TODO: code here
			}
			catch( Exception ex )
			{
				Console.Error.WriteLine("Error while shutting down plugin: {0}", "[TODO: assemblyName here]");
			}

			// TODO: code here to make sure app domain no longer belongs to collection.
		}
		#endregion
	}
}
