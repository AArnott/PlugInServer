using System;
using System.Diagnostics;
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
		private bool FileNamesAreCaseSensitive = (Path.PathSeparator == '/'); // little hack to detect Linux
		#region Construction
		public PluginAppDomainCollection(Services services)
		{
			this.services = services;
			appdomains = new HybridDictionary(4, !FileNamesAreCaseSensitive);
		}
		#endregion

		#region Attributes
		protected Services services;
		public int Count
		{
			get
			{
				return appdomains.Count;
			}
		}
		private HybridDictionary appdomains;
		public bool Contains(AssemblyName assemblyName)
		{
			return appdomains.Contains(assemblyName.FullName);
		}
		public bool Contains(PluginAppDomain appdomain)
		{
			return Contains(appdomain.AssemblyName);
		}
		public PluginAppDomain this[AssemblyName assemblyName]
		{
			get
			{
				return (PluginAppDomain) appdomains[assemblyName.FullName];
			}
		}
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
		public delegate void AppDomainEventHandler(PluginAppDomain appDomain);
		public event AppDomainEventHandler AppDomainLoaded;
		protected void OnAppDomainLoaded(PluginAppDomain appDomain)
		{
			AppDomainEventHandler appDomainLoaded = AppDomainLoaded;
			if( appDomainLoaded == null ) return; // no listeners
			appDomainLoaded(appDomain);
		}
		public event AppDomainEventHandler AppDomainUnloading;
		protected void OnAppDomainUnloading(PluginAppDomain appDomain)
		{
			AppDomainEventHandler appDomainUnloading = AppDomainUnloading;
			if( appDomainUnloading == null ) return; // no listeners
			appDomainUnloading(appDomain);
		}
		#endregion

		#region Operations
		public PluginAppDomain Load(string assemblyPath)
		{
			return Load(AssemblyName.GetAssemblyName(assemblyPath));
		}
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
		public void Unload(string assemblyPath)
		{
			AssemblyName assemblyName = FindAssemblyNameFor(assemblyPath);
			if( assemblyName == null ) return;
			Unload(this[assemblyName]);
		}
		public void Unload(PluginAppDomain appdomain)
		{
			if( !Contains( appdomain ) ) return;
			OnAppDomainUnloading(appdomain);
			appdomains.Remove( appdomain.AssemblyName.FullName );
			appdomain.Unload();
		}
		public void Reload(string assemblyPath)
		{
			Status oldPortManagerStatus = services.PortManager.Status;
			if( services.PortManager.Status == Status.Running )
				services.PortManager.Status = Status.Paused;
			Unload(assemblyPath);
			Load(assemblyPath);
			services.PortManager.Status = oldPortManagerStatus;
		}
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
