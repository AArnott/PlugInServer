using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Manages the collection of <see cref="PluginAppDomain"/> objects
	/// loaded by the server.
	/// </summary>
	[Serializable]
	public class PluginAppDomainCollection
	{
		private bool FileNamesAreCaseSensitive = (Path.PathSeparator == '/'); // little hack to detect Linux
		#region Construction
		/// <summary>
		/// Creates an instance of the <see cref="PluginAppDomainCollection"/> class.
		/// </summary>
		/// <param name="services">
		/// A reference to the hosting <see cref="Services"/> object.
		/// </param>
		public PluginAppDomainCollection(Services services)
		{
			this.services = services;
			appdomains = new HybridDictionary(4, !FileNamesAreCaseSensitive);
		}
		#endregion

		#region Attributes
		protected Services services;
		/// <summary>
		/// Gets the number of <see cref="PluginAppDomain"/> objects
		/// that are currently loaded.
		/// </summary>
		public int Count
		{
			get
			{
				return appdomains.Count;
			}
		}
		private HybridDictionary appdomains;
		/// <summary>
		/// Checks whether a given assembly is loaded in the server.
		/// </summary>
		public bool Contains(AssemblyName assemblyName)
		{
			return appdomains.Contains(assemblyName.FullName);
		}
		/// <summary>
		/// Checks whether a given <see cref="PluginAppDomain"/> is already
		/// a member of this collection.
		/// </summary>
		public bool Contains(PluginAppDomain appdomain)
		{
			return Contains(appdomain.AssemblyName);
		}
		/// <summary>
		/// Gets the <see cref="PluginAppDomain"/> for the 
		/// given <see cref="AssemblyName"/>.
		/// </summary>
		public PluginAppDomain this[AssemblyName assemblyName]
		{
			get
			{
				return (PluginAppDomain) appdomains[assemblyName.FullName];
			}
		}
		/// <summary>
		/// Gets the <see cref="AssemblyName"/> for a loaded assembly 
		/// at a given path in the filesystem.  
		/// </summary>
		/// <param name="assemblyPath">
		/// The physical path in the file system of the assembly.
		/// </param>
		/// <returns>
		/// The <see cref="AssemblyName"/> of the assembly if the assembly
		/// is already loaded in the collection.
		/// </returns>
		public AssemblyName FindAssemblyNameFor(string assemblyPath)
		{
			Uri assemblyUri = new Uri(assemblyPath);
			foreach( PluginAppDomain appdomain in appdomains.Values )
				if( assemblyUri.Equals(appdomain.AssemblyName.CodeBase) )
					return appdomain.AssemblyName;
			return null; // could not be found
		}
		#endregion

		#region Events
		/// <summary>
		/// The delegate used by the <see cref="AppDomainLoaded"/> 
		/// and <see cref="AppDomainUnloading"/> events.
		/// </summary>
		public delegate void AppDomainEventHandler(PluginAppDomain appDomain);
		/// <summary>
		/// Fired when a new <see cref="PluginAppDomain"/> is loaded.
		/// </summary>
		public event AppDomainEventHandler AppDomainLoaded;
		protected void OnAppDomainLoaded(PluginAppDomain appDomain)
		{
			AppDomainEventHandler appDomainLoaded = AppDomainLoaded;
			if( appDomainLoaded == null ) return; // no listeners
			appDomainLoaded(appDomain);
		}
		/// <summary>
		/// Fired when a loaded <see cref="PluginAppDomain"/> is about to be unloaded.
		/// </summary>
		public event AppDomainEventHandler AppDomainUnloading;
		protected void OnAppDomainUnloading(PluginAppDomain appDomain)
		{
			AppDomainEventHandler appDomainUnloading = AppDomainUnloading;
			if( appDomainUnloading == null ) return; // no listeners
			appDomainUnloading(appDomain);
		}
		#endregion

		#region Operations
		/// <summary>
		/// Loads the plugins in a given assembly into the server.
		/// </summary>
		/// <param name="assemblyPath">
		/// The file system path to the assembly to load.
		/// </param>
		/// <returns>
		/// The <see cref="PluginAppDomain"/> created to host the
		/// loaded assembly.
		/// </returns>
		public PluginAppDomain Load(string assemblyPath)
		{
			return Load(AssemblyName.GetAssemblyName(assemblyPath));
		}
		/// <summary>
		/// Loads the plugins in a given assembly into the server.
		/// </summary>
		/// <param name="assemblyName">
		/// The <see cref="AssemblyName"/> of the plugin assembly to load.
		/// </param>
		/// <returns>
		/// The <see cref="PluginAppDomain"/> created to host the
		/// loaded assembly.
		/// </returns>
		public PluginAppDomain Load(AssemblyName assemblyName)
		{
			try 
			{
				if( appdomains.Contains(assemblyName.FullName) ) throw new InvalidOperationException("Assembly already loaded.");
				PluginAppDomain appdomain = new PluginAppDomain(assemblyName);
				// Keep track of the appdomain so we can unload it if the plugin changes or is removed.
				appdomains.Add( assemblyName.FullName, appdomain );
				OnAppDomainLoaded(appdomain);
				return appdomain;
			}
			catch( Exception ex )
			{
				Console.Error.WriteLine("Error loading assembly: {0}" + Environment.NewLine + "{1}", assemblyName.FullName, ex.ToString());
				return null;
			}
		}
		/// <summary>
		/// Unloads a plugin assembly.
		/// </summary>
		/// <param name="assemblyPath">
		/// The file system path of the assembly to unload.
		/// </param>
		public void Unload(string assemblyPath)
		{
			AssemblyName assemblyName = FindAssemblyNameFor(assemblyPath);
			if( assemblyName == null ) return;
			Unload(this[assemblyName]);
		}
		/// <summary>
		/// Unloads a <see cref="PluginAppDomain"/>.
		/// </summary>
		public void Unload(PluginAppDomain appdomain)
		{
			if( !Contains( appdomain ) ) return;
			OnAppDomainUnloading(appdomain);
			appdomains.Remove( appdomain.AssemblyName.FullName );
			appdomain.Unload();
		}
		/// <summary>
		/// Unloads and reloads a plugin assembly.
		/// </summary>
		/// <param name="assemblyPath">
		/// The file system path of the assembly to reload.
		/// </param>
		public void Reload(string assemblyPath)
		{
			Status oldPortManagerStatus = services.PortManager.Status;
			if( services.PortManager.Status == Status.Running )
				services.PortManager.Status = Status.Paused;
			Unload(assemblyPath);
			Load(assemblyPath);
			services.PortManager.Status = oldPortManagerStatus;
		}
		/// <summary>
		/// Unloads all plugin assemblies.
		/// </summary>
		public void UnloadAll()
		{
			lock( appdomains )
			{
				PluginAppDomain[] appdomainsArray = new PluginAppDomain[appdomains.Count];
				appdomains.Values.CopyTo(appdomainsArray, 0);
				for( int i = 0; i < appdomainsArray.Length; i++ )
					Unload(appdomainsArray[i]);
			}
		}
		/// <summary>
		/// Loads all plugin assemblies found in a given directory.
		/// </summary>
		/// <param name="assemblyDirectory">
		/// The file system directory to scan for plugins.
		/// </param>
		/// <returns>
		/// A list of all <see cref="PluginAppDomain"/> objects 
		/// created to host the plugin assemblies.
		/// </returns>
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

		#region Enumerator
		public PluginAppDomainCollectionEnumerator GetEnumerator()
		{
			return new PluginAppDomainCollectionEnumerator(this);
		}
		public class PluginAppDomainCollectionEnumerator
		{
			IDictionaryEnumerator enumerator;
			internal PluginAppDomainCollectionEnumerator(PluginAppDomainCollection collection)
			{
				enumerator = collection.appdomains.GetEnumerator();
			}

			public PluginAppDomain Current
			{
				get
				{
					return (PluginAppDomain) enumerator.Value;
				}
			}

			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}
		}
		#endregion
	}
}
