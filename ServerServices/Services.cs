using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Byu.IT347.PluginServer.PluginServices;
using System.Timers;

namespace Byu.IT347.PluginServer.ServerServices
{
	public class Services : IServer
	{
		#region Construction
		public Services()
		{
			InitializePluginAppDomainsCollection();
			InitializePluginWatcher();
			InitializePortManager();
		}
		private void InitializePluginAppDomainsCollection()
		{
			pluginAppDomains = new PluginAppDomainCollection();
		}
		private void InitializePluginWatcher()
		{
			// Initialize timer for delaying response to I/O events
			PluginWatcherTimer = new Timer(PluginWatcherMinimumWaitTimeDefault);
			PluginWatcherTimer.AutoReset = false;
			PluginWatcherTimer.Elapsed += new ElapsedEventHandler(PluginWatcherTimer_Elapsed);

			// Initialize queue for storing delayed I/O events for later handling
			PendingPluginWatcherEvents = new IONotificationQueue(8);
			
			// Initialize the file system watcher itself
			PluginWatcher = new FileSystemWatcher(System.IO.Directory.GetCurrentDirectory(), "*.dll");
			PluginWatcher.IncludeSubdirectories = false;
			PluginWatcher.Created += new FileSystemEventHandler(PluginWatcher_IOEvent);
			PluginWatcher.Deleted += new FileSystemEventHandler(PluginWatcher_IOEvent);
			PluginWatcher.Changed += new FileSystemEventHandler(PluginWatcher_IOEvent);
			PluginWatcher.EnableRaisingEvents = true;
		}
		private void InitializePortManager()
		{
			PortManager = new PortManager(this);
		}
		#endregion

		#region Attributes
		protected FileSystemWatcher PluginWatcher;
		/// <summary>
		/// Started when an I/O event if raised, and a delay is necessary.
		/// Raises an event when the necessary timeout has passed.
		/// </summary>
		protected Timer PluginWatcherTimer;
		private const int PluginWatcherMinimumWaitTimeDefault = 500;
		/// <summary>
		/// The minimum time (in milliseconds) that must elapse between the 
		/// most recent I/O event in the plugin directory and the time that 
		/// the changes are handled by the plugin manager.
		/// </summary>
		/// <remarks>
		/// This becomes necessary because the <see cref="FileSystemWatcher"/>
		/// tends to throw several events for just a single file I/O operation,
		/// leading to many needless reloads of any new plugin.
		/// </remarks>
		public int PluginWatcherMinimumWaitTime
		{
			get
			{
				return (int) PluginWatcherTimer.Interval;
			}
			set
			{
				PluginWatcherTimer.Interval = value;
			}
		}
		protected IONotificationQueue PendingPluginWatcherEvents;

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

		internal PluginAppDomainCollection pluginAppDomains;

		private Status status = Status.Stopped;
		public Status Status
		{
			get
			{
				return status;
			}
		}

		protected PortManager PortManager;
		#endregion

		#region Operations
		public void Start()
		{
			status = Status.Running;

			// Open any ports
			// TODO: code here
		}

		public void Stop()
		{
			status = Status.Stopped;

			// Close all ports
			// TODO: code here
		}

		public void Pause()
		{
			status = Status.Paused;

			// Stop accepting requests on ports, but leave them open
			// TODO: code here
		}
		#endregion

		#region Event handlers
		private void PluginWatcher_IOEvent(object sender, FileSystemEventArgs e)
		{
			PluginWatcherTimer.Stop();
			PendingPluginWatcherEvents.Enqueue(e);
			PluginWatcherTimer.Start();
		}
		private void PluginWatcherTimer_Elapsed(object sender, ElapsedEventArgs te)
		{
			while( PendingPluginWatcherEvents.Count > 0 )
			{
				FileSystemEventArgs e = PendingPluginWatcherEvents.Dequeue();
				switch( e.ChangeType )
				{
					case WatcherChangeTypes.Created:
						pluginAppDomains.Load(e.FullPath);
						break;
					case WatcherChangeTypes.Changed:
						pluginAppDomains.Unload(e.FullPath);
						pluginAppDomains.Load(e.FullPath);
						break;
					case WatcherChangeTypes.Deleted:
						pluginAppDomains.Unload(e.FullPath);
						break;
					case WatcherChangeTypes.Renamed:
						// Rename appdomain for the plugin so it can be found when deleted later.
						break;
				}

				Console.WriteLine("Now listening on ports: {0}", intarraytostring(PortManager.Ports));
			}
		}
		private string intarraytostring(int[] array)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach( int i in array )
				sb.Append(i.ToString() + ", ");
			sb.Length -= 2;
			return sb.ToString();
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
