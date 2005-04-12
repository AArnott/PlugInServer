using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;

namespace Byu.IT347.PluginServer.Plugins.Chat
{
	public class ClientSession : MarshalByRefObject
	{
		#region Construction
		/// <summary>
		/// Creates a Client Session that contains the incoming and outgoing streams.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="history"></param>
		internal ClientSession(NetworkStream stream, string history)
		{
			Console.WriteLine("Creating client session...");
			this.stream = stream;
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			messagesToGoOut = new Queue(maxMessageQueueSize);
			if( history != null && history.Length > 0 ) 
				messagesToGoOut.Enqueue(history);
		}
		#endregion

		#region Events
		public event EventHandler Opened;
		public event EventHandler IncomingMessage;
		public event EventHandler Closed;
		#endregion

		#region Attributes
		private const int maxMessageQueueSize = 100;
		private NetworkStream stream;
		private StreamWriter writer;
		private StreamReader reader;
		private Queue messagesToGoOut;

		private string name;
		public string Name
		{
			get
			{
				return name;
			}
		}
		private string lastMessageReceived = "";
		public string LastMessageReceived { get { return lastMessageReceived; } }
		#endregion

		#region Operations
		/// <summary>
		/// Puts the message into the queue waiting till it can be sent to the client.
		/// </summary>
		/// <param name="message"></param>
		public void Send(string message)
		{
			messagesToGoOut.Enqueue(message);
		}
		public void Close()
		{
			// Wait until the client receives all incoming text
			//thread.Abort();
		}
		private void close()
		{
			OnClosed();
		}
		protected virtual void OnOpened()
		{
			EventHandler opened = Opened;
			if( opened != null )
				opened(this, null);
		}
		protected virtual void OnClosed()
		{
			EventHandler closed = Closed;
			if( closed != null )
				closed(this, null);
		}

		/// <summary>
		/// Triggers an event when a message has come in from the client.
		/// </summary>
		protected virtual void OnIncomingMessage()
		{
			EventHandler incomingMessage = IncomingMessage;
			if( incomingMessage != null )
				incomingMessage(this, null);
		}
		/// <summary>
		/// Handles the communication with the client.
		/// This is where the main processing for each client takes place.
		/// </summary>
		internal void Handler()
		{
			try 
			{
				Console.WriteLine("Sending Welcome...");
				SendWelcome();
				Console.WriteLine("Getting Name...");
				if( GetName() ) 
					Listen();
				close();
			}
			catch( ThreadAbortException )
			{
				// Send any last messages
				if( messagesToGoOut.Count > 0 )
					writer.Write(DequeueAllMessages());
				writer.Flush();
				close();
			}
		}
		/// <summary>
		/// Sends a greeting to the new client
		/// </summary>
		private void SendWelcome()
		{
			writer.Write("Welcome to the chat server." + Server.EndOfLine);
		}
		/// <summary>
		/// Gets the name for the newly connected user.
		/// </summary>
		/// <returns></returns>
		private bool GetName()
		{
			writer.WriteLine("Enter your name:");
			writer.Flush();
			try 
			{
				name = reader.ReadLine();
			}
			catch( IOException )
			{
				return false;
			}
			OnOpened();
			return true;
		}
		/// <summary>
		/// Contains the main operating loop for the individual client communication.
		/// </summary>
		private void Listen()
		{
			try 
			{
				while( !lastMessageReceived.ToLower().StartsWith("bye") )
				{
					writer.Write(DequeueAllMessages());
					//writer.Write("{0}>", Name);
					writer.Flush();
					NetworkStream ns = (NetworkStream) reader.BaseStream;
					if(ns.DataAvailable)
					{
						lastMessageReceived = reader.ReadLine();
						if( lastMessageReceived == null ) return;
						OnIncomingMessage();
					}
					
				}

			}
			catch( IOException )
			{
				// just drop the connection
			}
		}
		/// <summary>
		/// gets all the ready messages and sends them to the client.  (typically should only
		/// be the single most recent message)
		/// </summary>
		/// <returns></returns>
		private string DequeueAllMessages()
		{
			// Wait until we have at least one message (for our synchronous clients)
			//while( messagesToGoOut.Count == 0 ) Thread.Sleep(100);

			// Dequeue all waiting messages for the client as one long string.
			StringBuilder s = new StringBuilder(50*messagesToGoOut.Count);
			while( messagesToGoOut.Count > 0 ) 
				s.Append(messagesToGoOut.Dequeue() + Server.EndOfLine);
			return s.ToString();
		}
		#endregion
	}
}
