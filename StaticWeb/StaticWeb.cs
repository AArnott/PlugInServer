using System;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Specialized;


namespace Byu.IT347.PluginServer.Plugins.StaticWeb
{
	public class StaticWebPlugIn : MarshalByRefObject, IHandler
	{
		public StaticWebPlugIn()
		{
		}
		#region IHandler Members

		static int counter = 0;
		public void HandleRequest(NetworkStream channel, IPEndPoint local, IPEndPoint remote)
		{
			StreamReader sr = new StreamReader(channel);
			string url = sr.ReadLine();
			//Console.WriteLine(url);
			if( url.StartsWith("GET /favicon.ico ") ) return; // ignore favicon requests
			//StreamWriter sw = new StreamWriter(channel);
			//sw.WriteLine("HTTP/1.0 200 OK\r\nContent-type: text/html\r\n\r\nHello Mike's World, {1}! {0} {2} {3}", 
			//	counter++, remote.Address.ToString(), "<img src=\"http://barlowfamily.freeservers.com/images/img_1528.jpg\">", url);
			//sw.Flush();
			string content = CreateContent(remote);
			
			//Byte[] byteHtmlData = Encoding.ASCII.GetBytes(htmlPage);
			//string sMimeType = "";
			//int iTotBytes = byteHtmlData.Length;
			
			Byte[] byteHtmlData = Encoding.ASCII.GetBytes(content);
			
			int TotalBytesToSend = byteHtmlData.Length;
			
			string mimeType = getMimeType(url);
			
			string header = CreateHeader(mimeType, TotalBytesToSend);
			SendToBrowser(channel, header);
			SendToBrowser(channel, content);
			
			//SendContent(channel, content);
		}
		public string CreateHeader(string mimeType, int TotalBytesToSend)
		{
			string header = "HTTP/1.0 200 OK\r\n";
			header = header + "Content-type: " + mimeType + "\r\n";
			header = header + "Content-Length: " + TotalBytesToSend + "\r\n\r\n";
			return header;
		}
		public void SendToBrowser(NetworkStream channel, string data)
		{
			StreamWriter sw = new StreamWriter(channel);
			sw.WriteLine(data);
			sw.Flush();
		}
		public string CreateContent(IPEndPoint remote)
		{
			string content = "Barlow Triplets<br><img src=\"http://barlowfamily.freeservers.com/images/img_1528.jpg\"><br>" + remote.Address.ToString();
			return content;
		}
		public void SendContent(NetworkStream channel, string content)
		{
			StreamWriter sw = new StreamWriter(channel);
			sw.WriteLine(content);
			sw.Flush();
		}
		public string getMimeType(string request)
		{
			NameValueCollection mt = new NameValueCollection();
			mt.Add("htm", "text/html");
			mt.Add("html", "text/html");
			mt.Add("gif", "image/gif");
			mt.Add("jpeg", "image/jpeg");
			mt.Add("jpg", "image/jpeg");
			mt.Add("png", "image/png");

			//Response.WriteLine("Content-type: " + mt["gif"]);
			request = request.ToLower();
			int startPosition = request.IndexOf(".");
			Console.WriteLine(request);
			//End of file extension where the GET method begins with "?"
			int stopPosition = request.IndexOf("?",startPosition);
			if (stopPosition == -1)
			{
				//If there is no GET method used, then the stopPosition 
				stopPosition = request.IndexOf(" ",startPosition);
			}
			int length = stopPosition - startPosition;
			if (length <= 0)
			{
				return mt["htm"];
			}
			else
			{
				//Console.WriteLine("Length: " + length + "\r\nStartPosition: " + startPosition + "\r\nStopPosition: " + stopPosition);
				string FileExt = request.Substring((startPosition + 1),(length - 1));
				//Console.WriteLine("File Extension:" + mt[FileExt] + ":");
				return mt[FileExt];
			}
		}
		/*public string GetLocalPath(string WebServerRoot, string Dirname)
		{
			//TODO:  get the path of where the files should be stored on the local machine that can be accessed and displayed by the webserver

		}*/
		
		
		#endregion


		#region IPlugin Members

		public bool CanProcessRequest(string url)
		{
			// TODO:  Add StaticWebPlugIn.CanProcessRequest implementation
			return false;
		}

		public int[] Ports
		{
			get
			{
				return new int[] { 80, 8080 };
			}
		}

		public void Shutdown()
		{
			// TODO:  Add StaticWebPlugIn.Shutdown implementation
		}

		public string Name
		{
			get
			{
				return "Static Web server";
			}
		}

		public void Startup(IServer server)
		{
			// TODO:  Add StaticWebPlugIn.Startup implementation
		}

		#endregion
	}
}
