using System;
using System.IO;
using System.Collections;

namespace Byu.IT347.PluginServer.ServerServices
{
	/// <summary>
	/// A queue specifically for storing notifications from
	/// the <see cref="FileSystemWatcher"/> class.
	/// </summary>
	public class IONotificationQueue
	{
		#region Construction
		public IONotificationQueue()
		{
			queue = new ArrayList();
		}
		public IONotificationQueue(int capacity)
		{
			queue = new ArrayList(capacity);
		}
		#endregion 

		#region Attributes
		protected ArrayList queue;
		public int Count
		{
			get
			{
				return queue.Count;
			}
		}
		protected FileSystemEventArgs this[int index]
		{
			get
			{
				return (FileSystemEventArgs) queue[index];
			}
		}
		#endregion

		#region Operations
		public void Enqueue(FileSystemEventArgs e)
		{
			if( e == null ) throw new ArgumentNullException("e");

			lock( queue )
			{
				// Check whether this new event will obsolete any previous one
				// in the queue already.  If so, remove the old one.
				RemoveSimilar(e);

				queue.Add(e);
			}
		}
		public FileSystemEventArgs Dequeue()
		{
			lock( queue ) 
			{
				if( queue.Count == 0 ) return null;

				FileSystemEventArgs e = (FileSystemEventArgs) queue[0];
				queue.RemoveAt(0);
				return e;
			}
		}
		protected bool ContainsSimilar(FileSystemEventArgs e)
		{
			return IndexOfSimilar(e) >= 0;
		}
		protected int IndexOfSimilar(FileSystemEventArgs e)
		{
			for( int i = 0; i < Count; i++ )
				if( this[i].FullPath == e.FullPath )
					return i;
			return -1;
		}
		protected void RemoveSimilar(FileSystemEventArgs e)
		{
			lock( queue )
			{
				int indexOfSimilar = IndexOfSimilar(e);
				if( indexOfSimilar >= 0 ) 
					queue.RemoveAt(indexOfSimilar);
			}
		}
		#endregion
	}
}
