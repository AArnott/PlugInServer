using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// An <see cref="AppDomain"/> that hosts a single
	/// assembly with at least one plugin.
	/// </summary>
	public class PluginAppDomain
	{
		#region Construction
		/// <summary>
		/// Creates an instance of the <see cref="PluginAppDomain"/> class.
		/// </summary>
		/// <param name="assemblyName">
		/// The <see cref="AssemblyName"/> of the assembly to load
		/// into the new AppDomain.
		/// </param>
		public PluginAppDomain(AssemblyName assemblyName)
		{
			if( assemblyName == null ) throw new ArgumentNullException("assemblyName");

			this.assemblyName = assemblyName;
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
				throw;
			}
		}
		private void InitPluginsCollections()
		{
			filters = new AppDomainPluginCollection();
			handlers = new AppDomainPluginCollection();
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
			Uri assemblyPath = new Uri( AssemblyName.CodeBase );
			setup.PrivateBinPath = Path.GetDirectoryName(assemblyPath.LocalPath);
			// Shadow-copy assembly to avoid file locking
			setup.ShadowCopyFiles = "true";
			setup.ShadowCopyDirectories = setup.ApplicationBase + ";" + setup.PrivateBinPath;

			// Create the domain to load the plugin into.
			AppDomain = AppDomain.CreateDomain(AssemblyName.FullName, PluginEvidence, setup );
			
			AppDomainExceptionHandler errorHandler = new AppDomainExceptionHandler(AppDomain);
			errorHandler.Attach();
		}
		private void LoadAssemblyPlugins()
		{
			Console.WriteLine("Loading {0} assembly.", AssemblyName.Name);
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
		private AssemblyName assemblyName;
		/// <summary>
		/// Gets the <see cref="AssemblyName"/> of the loaded plugin assembly.
		/// </summary>
		public AssemblyName AssemblyName
		{
			get
			{
				return assemblyName;
			}
		}
		protected IPlugin[] Plugins = null;

		private AppDomainPluginCollection filters;
		/// <summary>
		/// Gets a list of all <see cref="IFilter"/> plugins.
		/// </summary>
		public AppDomainPluginCollection Filters { get { return filters; } }
		private AppDomainPluginCollection handlers;
		/// <summary>
		/// Gets a list of all <see cref="IHandler"/> plugins.
		/// </summary>
		public AppDomainPluginCollection Handlers { get { return handlers; } }
		#endregion

		#region Operations
		/// <summary>
		/// Unloads the <see cref="AppDomain"/> and the contained plugin assembly.
		/// </summary>
		public void Unload()
		{
			Console.WriteLine("Unloading {0} assembly.", AssemblyName.Name);
			Filters.Clear();
			Handlers.Clear();
			
			Plugins = null;
			AppDomain.Unload(AppDomain);
			AppDomain = null;
		}
		#endregion

		#region Error reporting
		[Serializable]
		class AppDomainExceptionHandler
		{
			AppDomain AppDomain;
			public AppDomainExceptionHandler(AppDomain appDomain)
			{
				AppDomain = appDomain;
			}

			public void Attach()
			{
				AppDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
			}

			public void Detach()
			{
				AppDomain.UnhandledException -= new UnhandledExceptionEventHandler(AppDomain_UnhandledException);
			}

			public void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
			{
				Console.Error.WriteLine("Unhandled exception in plug-in: {0}.  Details follow: \n{1}",
					AppDomain.FriendlyName, (Exception)e.ExceptionObject);
			}
		}
		#endregion
	}
}
