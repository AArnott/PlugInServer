using System;
using System.IO;
using System.Collections;
using System.Timers;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Summary description for PluginManager.
	/// </summary>
	public class PluginManager : IEnumerable
	{
		#region Construction
		public PluginManager(IServer server)
		{
			Server = server;
			InitializePluginAppDomainsCollection();
			InitializePluginWatcher();
		}
		private void InitializePluginAppDomainsCollection()
		{
			pluginAppDomains = new PluginAppDomainCollection((Services)Server);
			pluginAppDomains.AppDomainLoaded += new PluginAppDomainCollection.AppDomainEventHandler(pluginAppDomains_AppDomainLoaded);
			pluginAppDomains.AppDomainUnloading += new PluginAppDomainCollection.AppDomainEventHandler(pluginAppDomains_AppDomainUnloading);
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
		#endregion

		#region Attributes
		protected readonly IServer Server;
		internal PluginAppDomainCollection pluginAppDomains;
		protected ArrayList Plugins = new ArrayList();
		public int Count
		{
			get
			{
				return Plugins.Count;
			}
		}

		private Status status = Status.Stopped;
		public Status Status
		{
			get
			{
				return status;
			}
			set
			{
				switch( value )
				{
					case Status.Running:
						if( status == Status.Stopped ) // if we were freshly started, load all plugins
							pluginAppDomains.LoadAll(DirectoryWatched);
						else // running from a paused (or already running) state
							PluginWatcherTimer_Elapsed(this, null);
						break;
					case Status.Stopped:
						PluginWatcherTimer.Stop();
						PendingPluginWatcherEvents.Clear();
						pluginAppDomains.UnloadAll();
						break;
					case Status.Paused:
						// keep running, but don't respond to new plugin events until we are resumed.
						PluginWatcherTimer.Stop();
						break;
				}
				// At least listen to events coming in as long as we're not stopped.
				// If we are in a paused state, these events just queue up until the state changes.
				PluginWatcher.EnableRaisingEvents = (value != Status.Stopped);
				status = value;
			}
		}

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

		/// <summary>
		/// The directory to watch for plugins.
		/// </summary>
		public string DirectoryWatched
		{
			get
			{
				return PluginWatcher.Path;
			}
			set
			{
				if( !Directory.Exists( value ) ) 
					throw new DirectoryNotFoundException();
				if( Status == Status.Paused ) 
					throw new NotSupportedException("Changing the watched plugin directory while in a paused state is not supported.");
				pluginAppDomains.UnloadAll();
				PluginWatcher.Path = value;
				if( Status == Status.Running )
					pluginAppDomains.LoadAll(value);
			}
		}

		#endregion

		#region Events
		public event EventHandler Changed;
		protected virtual void OnChanged()
		{
			EventHandler changed = Changed;
			if( changed == null ) return; // no handlers attached
			changed(this, null);
		}
		#endregion

		#region Event handlers
		private void PluginWatcher_IOEvent(object sender, FileSystemEventArgs e)
		{
			PluginWatcherTimer.Stop();
			PendingPluginWatcherEvents.Enqueue(e);
			if( Status == Status.Running ) PluginWatcherTimer.Start();
		}
		private void PluginWatcherTimer_Elapsed(object sender, ElapsedEventArgs te)
		{
			while( PendingPluginWatcherEvents.Count > 0 )
			{
				FileSystemEventArgs e = PendingPluginWatcherEvents.Dequeue();
				try 
				{
					switch( e.ChangeType )
					{
						case WatcherChangeTypes.Created:
							pluginAppDomains.Load(e.FullPath);
							break;
						case WatcherChangeTypes.Changed:
							pluginAppDomains.Reload(e.FullPath);
							break;
						case WatcherChangeTypes.Deleted:
							pluginAppDomains.Unload(e.FullPath);
							break;
						case WatcherChangeTypes.Renamed:
							// Rename appdomain for the plugin so it can be found when deleted later.
							// TODO: code here
							break;
					}
				}
				catch( Exception ex )
				{
					Console.Error.WriteLine("Error while handling {0} event for assembly {1}.\n{2}", e.ChangeType, e.FullPath, ex.ToString());
					throw;
				}
			}
		}

		private void pluginAppDomains_AppDomainLoaded(PluginAppDomain appDomain)
		{
			foreach( IPlugin plugin in appDomain.Handlers )
			{
				Console.WriteLine("Starting {0} handler plugin.", plugin.Name);
				plugin.Startup(Server);
				Plugins.Add( plugin );
			}
			foreach( IPlugin plugin in appDomain.Filters )
			{
				Console.WriteLine("Starting {0} filter plugin.", plugin.Name);
				plugin.Startup(Server);
				Plugins.Add( plugin );
			}

			OnChanged();
		}

		private void pluginAppDomains_AppDomainUnloading(PluginAppDomain appDomain)
		{
			foreach( IPlugin plugin in appDomain.Filters )
			{
				Plugins.Remove( plugin );
				Console.WriteLine("Stopping {0} filter plugin.", plugin.Name);
				plugin.Shutdown();
			}
			foreach( IPlugin plugin in appDomain.Handlers )
			{
				Plugins.Remove( plugin );
				try 
				{
					Console.WriteLine("Stopping {0} handler plugin.", plugin.Name);
					plugin.Shutdown();
				}
				catch( System.Runtime.Remoting.RemotingException ex )
				{
					Console.Error.WriteLine("Plugin disconnected prematurely.\n{0}", ex.ToString());
				}
			}

			OnChanged();
		}
		#endregion

		#region IEnumerable Members
		public IEnumerator GetEnumerator()
		{
			return Plugins.GetEnumerator();
		}
		#endregion
	}
}
