using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Configuration;
using Byu.IT347.PluginServer.PluginServices;
using System.Timers;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// The main plugin server class.
	/// </summary>
	/// <remarks>
	/// This class is used by the thin GUI and text console frontends
	/// applications, and all logic is in this class.  It is stored in
	/// this library rather than the actual application frontends so that
	/// it can appear in code just once, and bug fixes in the GUI app
	/// will therefore automatically benefit the console app.
	/// </remarks>
	public class Services : MarshalByRefObject, IServer
	{
		#region Construction
		/// <summary>
		/// Creates an instance off the <see cref="Services"/> class.
		/// </summary>
		public Services()
		{
			InitializePluginManager();
			InitializePortManager();
		}
		private void InitializePluginManager()
		{
			PluginManager = new PluginManager(this);
			
			// Set plugin directory to setting in .config file
			string relativePath = ConfigurationSettings.AppSettings["PluginsPath"];
			Uri basePath = new Uri(Assembly.GetExecutingAssembly().Location);
			Uri pluginPath = new Uri(basePath, relativePath);
			if( !Directory.Exists(pluginPath.LocalPath) ) Directory.CreateDirectory(pluginPath.LocalPath);
			PluginDirectory = pluginPath.LocalPath;
		}
		private void InitializePortManager()
		{
			PortManager = new PortManager(this);
			PortManager.IncomingRequest += new PortManager.IncomingRequestEventHandler(PortManager_IncomingRequest);
		}
		#endregion

		#region Attributes

		private Status status = Status.Stopped;
		/// <summary>
		/// The running status of the plugin server.
		/// </summary>
		public Status Status
		{
			get
			{
				return status;
			}
		}

		/// <summary>
		/// The directory to watch for plugins to install.
		/// </summary>
		public string PluginDirectory
		{
			get
			{
				return PluginManager.DirectoryWatched;
			}
			set
			{
				PluginManager.DirectoryWatched = value;
			}
		}

		/// <summary>
		/// Gets the number of plugins that are currently running.
		/// </summary>
		public int Count
		{
			get
			{
				return PluginManager.Count;
			}
		}

		protected internal PluginManager PluginManager;
		protected internal PortManager PortManager;
		#endregion

		#region Operations
		/// <summary>
		/// Starts the plugin server.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the server is already in the <see cref="Status.Running"/> state.
		/// </exception>
		public void Start()
		{
			if( status == Status.Running ) throw new InvalidOperationException("Already running.");
			status = Status.Running;
			PluginManager.Status = status;
			PortManager.Status = status; // start port manager after plugins so we only open ports once
		}

		/// <summary>
		/// Stops the plugin server.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the server is already in the <see cref="Status.Stopped"/> state.
		/// </exception>
		public void Stop()
		{
			if( status == Status.Stopped ) throw new InvalidOperationException("Already stopped.");
			status = Status.Stopped;
			PortManager.Status = status; // stop ports before plugins so we only close ports once
			PluginManager.Status = status;
		}

		/// <summary>
		/// Pauses the plugin server.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the server is already in the <see cref="Status.Paused"/> state.
		/// </exception>
		public void Pause()
		{
			if( status == Status.Paused ) throw new InvalidOperationException("Already paused.");
			status = Status.Paused;
			PluginManager.Status = status;
			PortManager.Status = status;
		}
		#endregion

		#region IServer Members
		/// <summary>
		/// Unloads a plugin.
		/// </summary>
		/// <param name="Plugin">
		/// The plugin to unload.
		/// </param>
		/// <remarks>
		/// This method can be called by the plugin to be unloaded.
		/// It provides a way for a plugin to unload itself in case of 
		/// an unrecoverable error.
		/// </remarks>
		public void SurrenderPlugin(IPlugin Plugin)
		{
			// TODO:  Add Services.SurrenderPlugin implementation
		}

		/// <summary>
		/// Prepares an <see cref="IEnumerator"/> object for iterating
		/// over all the loaded plugins.
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			return PluginManager.GetEnumerator();
		}

		#endregion

		#region Event handlers
		private void PortManager_IncomingRequest(NetworkStream channel, IPEndPoint local, IPEndPoint remote)
		{
			IHandler[] handlers = PortManager.ListHandlersOnPort(local.Port);

			// Read first line of request if the port is shared.
			if( handlers[0] is ISharingHandler && handlers.Length > 1 )
			{
				string firstLine = StreamHelps.ReadLine(channel);
				foreach( ISharingHandler handler in handlers )
					if( handler.CanProcessRequest(firstLine.ToString()) )
					{
						handler.HandleRequest(channel, firstLine.ToString(), local, remote);
						break;
					}
			}
			else // non-sharing port
				handlers[0].HandleRequest(channel, local, remote);
		}
		#endregion
	}
}
