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
		internal ClientSession(NetworkStream stream, string history)
		{
			Console.WriteLine("Creating client session...");
			this.stream = stream;
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			messagesToGoOut = new Queue(maxMessageQueueSize);
			if( history != null && history.Length > 0 ) 
				messagesToGoOut.Enqueue(history);

			Console.WriteLine("Starting Thread...");

			//thread = new Thread(new ThreadStart(Handler));
			//thread.Start();
			//Handler();
		}
		#endregion

		#region Events
		public event EventHandler Opened;
		public event EventHandler IncomingMessage;
		public event EventHandler Closed;
		#endregion

		#region Attributes
		private const int maxMessageQueueSize = 100;
		//private TcpClient session;
		//public TcpClient Session { get { return session; } }
		private NetworkStream stream;
		private StreamWriter writer;
		private StreamReader reader;
		private Thread thread;
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
		public void Send(string message)
		{
			messagesToGoOut.Enqueue(message);
		}
		public void Close()
		{
			// Wait until the client receives all incoming text
			thread.Abort();
		}
		private void close()
		{
			// Force the stream closed so that waiting client sessions 
			// realize the connection is terminating.
			//stream.Close();
			//session.Close();
			//stream = null;
			//writer = null;
			//reader = null;
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

		protected virtual void OnIncomingMessage()
		{
			EventHandler incomingMessage = IncomingMessage;
			if( incomingMessage != null )
				incomingMessage(this, null);
		}
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
		private void SendWelcome()
		{
			writer.Write("Welcome to the chat server." + Server.EndOfLine);
		}
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
