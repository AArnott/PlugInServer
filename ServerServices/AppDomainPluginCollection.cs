using System;
using System.Collections;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// Manages the collection of plugins found within a single assembly, 
	/// in memory within a single AppDomain.
	/// </summary>
	public class AppDomainPluginCollection : IEnumerable
	{
		#region Construction
		/// <summary>
		/// Creates an instance of the <see cref="AppDomainPluginCollection"/> class.
		/// </summary>
		public AppDomainPluginCollection()
		{
		}
		#endregion

		#region Attributes
		private ArrayList plugins = new ArrayList();
		#endregion

		#region Operations
		/// <summary>
		/// Adds a plugin to this collection.
		/// </summary>
		public void Add(IPlugin plugin)
		{
			plugins.Add( plugin );
		}
		/// <summary>
		/// Removes all plugins from this collection.
		/// </summary>
		public void Clear()
		{
			plugins.Clear();
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Prepares an <see cref="IEnumerator"/> to iterate over
		/// all plugins within this collection.
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			return new PluginCollectionEnumerator(this);
		}
		
		/// <summary>
		/// Manages the iteration over all plugins with an
		/// <see cref="AppDomainPluginCollection"/> object.
		/// </summary>
		public class PluginCollectionEnumerator : IEnumerator
		{
			IEnumerator enumerator;
			internal PluginCollectionEnumerator(AppDomainPluginCollection collection)
			{
				if( collection == null ) throw new ArgumentNullException("collection");
				enumerator = collection.plugins.GetEnumerator();
			}
			#region IEnumerator Members
			/// <summary>
			/// Restarts enumeration from the first plugin.
			/// </summary>
			public void Reset()
			{
				enumerator.Reset();
			}
			/// <summary>
			/// Prepares to retrieve the next plugin.
			/// </summary>
			/// <returns>
			/// True if another plugin in the list exists.
			/// </returns>
			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			/// <summary>
			/// Gets the next plugin to iterate over.
			/// </summary>
			public IPlugin Current
			{
				get
				{
					return (IPlugin) enumerator.Current;
				}
			}
			#endregion
		}

		#endregion
	}
}
