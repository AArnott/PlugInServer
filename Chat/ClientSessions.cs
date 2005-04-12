using System;
using System.Text;
using System.Collections;
using System.Net.Sockets;

namespace Byu.IT347.PluginServer.Plugins.Chat
{
	/// <summary>
	/// Serves as a holding point for all of the client objects.  
	/// Also provides functionality for communicating with the clients.
	/// </summary>
	public class ClientSessions : MarshalByRefObject
	{
		#region Construction
		/// <summary>
		/// Instantiates the Collections for the history and list of clients.
		/// </summary>
		/// <param name="capacity"></param>
		internal ClientSessions(int capacity)
		{
			sessions = new ArrayList(capacity);
			MessageHistory = new Queue(Server.HistorySize);
		}
		#endregion

		#region Events
		public event EventHandler Opened;
		public event EventHandler IncomingMessage;
		public event EventHandler Closed;
		#endregion

		#region Attributes
		private ArrayList sessions;
		public int Count
		{
			get
			{
				return sessions.Count;
			}
		}
		public Queue MessageHistory;
		#endregion

		#region Operations
		/// <summary>
		/// Adds the client to the collection of connected clients.
		/// </summary>
		/// <param name="client"></param>
		public void Add(ClientSession client)
		{
			if( client == null ) throw new ArgumentNullException("client");
			sessions.Add( client );
			client.Opened += new EventHandler(client_Opened);
			client.Closed += new EventHandler(client_Closed);
			client.IncomingMessage += new EventHandler(client_IncomingMessage);
			Console.WriteLine("ClientSession added.");
		}
		/// <summary>
		/// Takes a Network Stream from Server and makes it into a ClientSession object for storage.
		/// </summary>
		/// <param name="clientStream"></param>
		/// <returns></returns>
		public ClientSession Add(NetworkStream clientStream)
		{
			Console.WriteLine("Adding client stream to list");
			ClientSession session = new ClientSession(clientStream, HistoryDump);
			Add(session);
			session.Handler();
			return session;
		}
		/// <summary>
		/// Sends the message out to all connected clients and adds the message to the history.
		/// </summary>
		/// <param name="message"></param>
		public void Send(string message)
		{
			MessageHistory.Enqueue(message);
			if( MessageHistory.Count > Server.HistorySize ) MessageHistory.Dequeue();
			lock( sessions )
				foreach( ClientSession client in sessions )
					client.Send(message);
		}
		/// <summary>
		/// Disconnects all clients.
		/// </summary>
		public void CloseAll()
		{
			lock( sessions )
			{
				for( int i = sessions.Count-1; i >= 0; i-- )
					((ClientSession)sessions[i]).Close();
			}
		}
		#endregion

		#region Event handlers
		private void client_Closed(object sender, EventArgs e)
		{
			sessions.Remove(sender);

			Send(string.Format("{0} has left.", ((ClientSession)sender).Name));
			
			EventHandler closed = Closed;
			if( closed != null ) 
				closed(sender, null);
		}

		private void client_Opened(object sender, EventArgs e)
		{
			Send(string.Format("{0} has joined.", ((ClientSession)sender).Name));
			
			EventHandler opened = Opened;
			if( opened != null ) 
				opened(sender, null);
		}

		private void client_IncomingMessage(object sender, EventArgs e)
		{
			// relay to all clients
//			if( sessions.Count > 1 )
				Send(string.Format("{0} said: {1}", ((ClientSession)sender).Name, ((ClientSession)sender).LastMessageReceived));

			EventHandler incomingMessage = IncomingMessage;
			if( incomingMessage != null ) 
				incomingMessage(sender, null);
		}
		#endregion

		/// <summary>
		/// Compiles all of the messages in the history in preparation 
		/// for sending to new clients.
		/// </summary>
		private string HistoryDump 
		{
			get
			{
				StringBuilder s = new StringBuilder(50*MessageHistory.Count);
				foreach( string msg in MessageHistory )
					s.Append(msg + Server.EndOfLine);
				return s.ToString();
			}
		}
	}
}
