using System;
using System.Collections;
using Byu.IT347.PluginServer.PluginServices;

namespace Byu.IT347.PluginServer.ServerServices
{
	public class AppDomainPluginCollection : IEnumerable
	{
		#region Construction
		public AppDomainPluginCollection()
		{
		}
		#endregion

		#region Attributes
		private ArrayList plugins = new ArrayList();
		#endregion

		#region Operations
		public void Add(IPlugin plugin)
		{
			plugins.Add( plugin );
		}
		public void Clear()
		{
			plugins.Clear();
		}
		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return new PluginCollectionEnumerator(this);
		}
		
		public class PluginCollectionEnumerator : IEnumerator
		{
			IEnumerator enumerator;
			internal PluginCollectionEnumerator(AppDomainPluginCollection collection)
			{
				if( collection == null ) throw new ArgumentNullException("collection");
				enumerator = collection.plugins.GetEnumerator();
			}
			#region IEnumerator Members

			public void Reset()
			{
				enumerator.Reset();
			}

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
