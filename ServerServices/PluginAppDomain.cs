using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Summary description for PluginAppDomain.
	/// </summary>
	public class PluginAppDomain
	{
		#region Construction
		public PluginAppDomain(AssemblyName assemblyName)
		{
			if( assemblyName == null ) throw new ArgumentNullException("assemblyName");

			AssemblyName = assemblyName;
			InitPluginsCollections();
			InitializePluginEvidence();

			PrepareAppDomain();
			try 
			{
				LoadAssemblyPlugins();
				InstallPlugins();
			}
			catch
			{
				Unload();
			}
		}
		private void InitPluginsCollections()
		{
			filters = new PluginCollection();
			handlers = new PluginCollection();
		}
		private void InitializePluginEvidence()
		{
			PluginEvidence = new Evidence(AppDomain.CurrentDomain.Evidence);
		}
		private void PrepareAppDomain()
		{
			AppDomainSetup setup = new AppDomainSetup();
			setup.ApplicationName = AssemblyName.Name;
			// Teach the new appdomain where to find the plugin assemblies
			setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
			// Teach the new appdomain where the plugins can find the web server assemblies
			Uri assemblyPath = new Uri( Path.GetDirectoryName(AssemblyName.CodeBase) );
			setup.PrivateBinPath = assemblyPath.LocalPath;
			// Shadow-copy assembly to avoid file locking
			setup.ShadowCopyFiles = "true";
			setup.ShadowCopyDirectories = setup.ApplicationBase + ";" + setup.PrivateBinPath;

			// Create the domain to load the plugin into.
			AppDomain = AppDomain.CreateDomain(AssemblyName.FullName, PluginEvidence, setup );
		}
		private void LoadAssemblyPlugins()
		{
			Console.WriteLine("Loading plugins in: {0}", AssemblyName.Name);
			// Start a remote plugin launcher within the new appdomain.
			PluginFinder finder = (PluginFinder) AppDomain.CreateInstanceFromAndUnwrap(
				typeof(PluginFinder).Assembly.CodeBase, typeof(PluginFinder).FullName);
			// Instruct the launcher to find and load all plugins.
			Plugins = finder.LoadPluginsInAssembly(AssemblyName);
			//pluginDomain.DomainUnload += new EventHandler(pluginDomain_DomainUnload);
		}
		private void InstallPlugins()
		{
			foreach( IPlugin plugin in Plugins )
			{
				Console.WriteLine("Loading plugin: {0}", plugin.Name);
				if( plugin is IHandler )
					Handlers.Add( plugin );
				if( plugin is IFilter )
					Filters.Add( plugin );
			}
		}
		#endregion

		#region Attributes
		protected Evidence PluginEvidence;
		protected AppDomain AppDomain = null;
		protected AssemblyName AssemblyName;
		protected IPlugin[] Plugins = null;

		private PluginCollection filters;
		public PluginCollection Filters { get { return filters; } }
		private PluginCollection handlers;
		public PluginCollection Handlers { get { return handlers; } }
		#endregion

		#region Operations
		public void Unload()
		{
			Console.WriteLine("Unloading: {0}", AssemblyName.Name);
			Filters.Clear();
			Handlers.Clear();
			
			Plugins = null;
			AppDomain.Unload(AppDomain);
			AppDomain = null;
		}
		#endregion
	}
}
