using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	public class Services : IServer
	{
		#region Construction
		public Services()
		{
			InitializePluginAppDomainsCollection();
			InitializePluginWatcher();
		}
		private void InitializePluginAppDomainsCollection()
		{
			pluginAppDomains = new PluginAppDomainCollection();
		}
		private void InitializePluginWatcher()
		{
			PluginWatcher = new FileSystemWatcher(System.IO.Directory.GetCurrentDirectory(), "*.dll");
			PluginWatcher.IncludeSubdirectories = false;
			PluginWatcher.Created += new FileSystemEventHandler(PluginWatcher_New);
			PluginWatcher.Deleted += new FileSystemEventHandler(PluginWatcher_Deleted);
			PluginWatcher.Changed += new FileSystemEventHandler(PluginWatcher_Changed);
			PluginWatcher.EnableRaisingEvents = true;
		}
		#endregion

		#region Attributes
		protected FileSystemWatcher PluginWatcher;

		public string PluginDirectory
		{
			get
			{
				return PluginWatcher.Path;
			}
			set
			{
				if( !Directory.Exists( value ) ) 
					throw new DirectoryNotFoundException();
				pluginAppDomains.UnloadAll();
				PluginWatcher.Path = value;
				pluginAppDomains.LoadAll(value);
			}
		}

		private PluginAppDomainCollection pluginAppDomains;
		#endregion

		#region Operations
		public void Start()
		{
		}

		public void Stop()
		{
		}
		#endregion

		#region Event handlers
		private void PluginWatcher_New(object sender, FileSystemEventArgs e)
		{
			PluginAppDomain pluginDomain = pluginAppDomains.Load(e.FullPath);
		}

		private void PluginWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			pluginAppDomains.Unload(e.FullPath);
		}

		private void PluginWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			pluginAppDomains.Unload(e.FullPath);
			pluginAppDomains.Load(e.FullPath);
		}

		#endregion

		#region IServer Members

		public void SurrenderPlugin(IPlugin Plugin)
		{
			// TODO:  Add Services.SurrenderPlugin implementation
		}

		IPlugin[] IServer.Plugins
		{
			get
			{
				// TODO:  Add Services.Plugins getter implementation
				return null;
			}
		}

		#endregion
	}
}
