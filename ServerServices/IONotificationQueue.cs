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
		/// <summary>
		/// Creates an instance of the <see cref="IONotificationQueue"/> class.
		/// </summary>
		public IONotificationQueue()
		{
			queue = new ArrayList();
		}
		/// <summary>
		/// Creates an instance of the <see cref="IONotificationQueue"/> class,
		/// with memory reserved for a given queue capacity.
		/// </summary>
		public IONotificationQueue(int capacity)
		{
			queue = new ArrayList(capacity);
		}
		#endregion 

		#region Attributes
		protected ArrayList queue;
		/// <summary>
		/// Gets the number of elements in the queue.
		/// </summary>
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
		/// <summary>
		/// Adds a <see cref="FileSystemEventArgs">file system event</see>
		/// to the queue.
		/// </summary>
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
		/// <summary>
		/// Removes the oldest <see cref="FileSystemEventArgs">file system event</see>
		/// from the queue.
		/// </summary>
		/// <returns>
		/// The oldest, removed <see cref="FileSystemEventArgs">file system event</see>.
		/// </returns>
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
		/// <summary>
		/// Clears the queue of <see cref="FileSystemEventArgs">file system events</see>.
		/// </summary>
		public void Clear()
		{
			queue.Clear();
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
