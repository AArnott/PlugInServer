using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Byu.IT347.PluginServer.PluginServices;
using System.Timers;

namespace Byu.IT347.PluginServer.ServerServices
{
	public class Services : MarshalByRefObject, IServer
	{
		#region Construction
		public Services()
		{
			InitializePluginManager();
			InitializePortManager();
		}
		private void InitializePluginManager()
		{
			PluginManager = new PluginManager(this);
		}
		private void InitializePortManager()
		{
			PortManager = new PortManager(this);
		}
		#endregion

		#region Attributes

		private Status status = Status.Stopped;
		public Status Status
		{
			get
			{
				return status;
			}
		}

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

		protected PluginManager PluginManager;
		protected PortManager PortManager;
		#endregion

		#region Operations
		public void Start()
		{
			if( status == Status.Running ) throw new InvalidOperationException("Already running.");
			status = Status.Running;
			PluginManager.Status = status;

			// Open any ports
			PortManager.OpenPorts();
		}

		public void Stop()
		{
			if( status == Status.Stopped ) throw new InvalidOperationException("Already stopped.");
			status = Status.Stopped;
			PluginManager.Status = status;

			// Close all ports
			PortManager.ClosePorts();
		}

		public void Pause()
		{
			if( status == Status.Paused ) throw new InvalidOperationException("Already paused.");
			status = Status.Paused;
			PluginManager.Status = status;

			// Stop accepting requests on ports, but leave them open
			// TODO: code here
		}
		#endregion

		#region IServer Members

		public void SurrenderPlugin(IPlugin Plugin)
		{
			// TODO:  Add Services.SurrenderPlugin implementation
		}

		public IEnumerator GetEnumerator()
		{
			return PluginManager.GetEnumerator();
		}

		#endregion
	}
}
