using System;
using System.IO;
using Byu.IT347.PluginServer.PluginServices;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Specialized;


namespace Byu.IT347.PluginServer.Plugins.StaticWeb
{
	public class StaticWebPlugIn : MarshalByRefObject, ISharingHandler
	{
		public StaticWebPlugIn()
		{
		}
		#region ISharingHandler Members

		public bool CanProcessRequest(string firstLine)
		{
			if(firstLine.IndexOf(".php") > 0 || firstLine.IndexOf(".jsp") > 0 || firstLine.IndexOf(".asp") > 0 || firstLine.IndexOf("/serviceadmin/") > 0)

			{	
				return false;
			}
			else 
			{
				return true;
			}
		}

		public void HandleRequest(NetworkStream stream, IPEndPoint local, IPEndPoint remote)
		{
			HandleRequest(stream, StreamHelps.ReadLine(stream), local, remote);
		}

		#endregion
		#region IHandler Members

		static int counter = 0;
		public void HandleRequest(NetworkStream channel, string firstLine, IPEndPoint local, IPEndPoint remote)
		{
			string header = "";
			string sMyWebServerRoot = System.Configuration.ConfigurationSettings.AppSettings["PublicRoot"];
			//StreamReader sr = new StreamReader(channel);
			string url = firstLine;
			//Console.WriteLine(url);
			if( url.StartsWith("GET /favicon.ico ") ) return; // ignore favicon requests
			//StreamWriter sw = new StreamWriter(channel);
			//sw.WriteLine("HTTP/1.0 200 OK\r\nContent-type: text/html\r\n\r\nHello Mike's World, {1}! {0} {2} {3}", 
			//	counter++, remote.Address.ToString(), "<img src=\"http://barlowfamily.freeservers.com/images/img_1528.jpg\">", url);
			//sw.Flush();

			//Look for HTTP request
			int iStartPos = url.IndexOf("HTTP",1);
			//Get the HTTP Text and Version will return "HTTP/1.x"
			string sHTTPVersion = url.Substring(iStartPos,8);

			//Extract the Requested Type and Requested file/directory
			string sRequest = url.Substring(0,iStartPos -1);

			//Replace backslash with forward slash, if any
			sRequest.Replace("\\","/");

			//If file name is not supplied add forward slash to indicate
			//that it is a directory and then we will look for the 
			//default file name.
			if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
			{
				sRequest = sRequest + "/";
			}
			
			//Extract the requested file name
			iStartPos = sRequest.LastIndexOf("/") + 1;
			string sRequestedFile = sRequest.Substring(iStartPos);

			//Extract the directory name
			string sDirName = sRequest.Substring(sRequest.IndexOf("/"), (sRequest.LastIndexOf("/") - 3));
			string sLocalDir = "";
			if (sDirName =="/")
				sLocalDir = sMyWebServerRoot;
			else
			{
				sLocalDir = GetLocalPath(sMyWebServerRoot, sDirName);
			}

			Console.WriteLine("Directory Requested: " + sLocalDir);

            //If the physical directory does not exist then
			//display the error message
			string sErrorMessage = "";
			string mimeType = getMimeType(url);
			if (sLocalDir.Length == 0)
			{
				sErrorMessage = "<H2>ERROR!!! Requested Directory does not exist</H2><br>";
				header = CreateHeader(sHTTPVersion, "", sErrorMessage.Length, " 404 Not Found");
				SendToBrowser(channel, header);
				SendToBrowser(channel, sErrorMessage);				
			}
			
			if(sRequestedFile.Length == 0)
			{
				sRequestedFile = GetTheDefaultFileName(sLocalDir);
				Console.WriteLine("RequestedFile: " + sRequestedFile);
				if (sRequestedFile == "")
				{
					sErrorMessage = "<H2>Error!! No Default File Name Specified</H2>";
					header = CreateHeader(sHTTPVersion, "", sErrorMessage.Length, " 404 Not Found");
					SendToBrowser(channel, header);
					SendToBrowser(channel, sErrorMessage);
				}
				//SendToBrowser(channel, sRequestedFile);
			}
			
			mimeType = getMimeType(sRequestedFile);
			String sPhysicalFilePath = sLocalDir + sRequestedFile;
			Console.WriteLine("File Requested: " + sPhysicalFilePath);
			
			if(File.Exists(sPhysicalFilePath) == false)
			{
				sErrorMessage = "<H2>404 Error! File Does Not Exist...</H2>";
				header = CreateHeader(sHTTPVersion, "", sErrorMessage.Length, " 404 Not Found");
				SendToBrowser(channel, header);
				SendToBrowser(channel, sErrorMessage);
				Console.WriteLine("File Not Found");
			}
			else
			{
				byte[] bytes = null;
				string sMyLine = "";
				try
				{
					using( FileStream fs = new FileStream(sPhysicalFilePath, FileMode.Open) )
					{
						bytes = new byte[fs.Length];
						fs.Read(bytes, 0, bytes.Length);
					}
					
					//pageBody = pageBody + sreader.ReadLine();
					//					while(((sMyLine = sreader.ReadLine()) != null))
					//					{
					//						pageBody = pageBody + sMyLine;
					//						Console.WriteLine("pageBody: " + pageBody);
					//					}

					//pageBody = pageBody + sreader.ReadLine();
					/*while(((sMyLine = sreader.ReadLine()) != null))
					{
						pageBody = pageBody + sMyLine;
						Console.WriteLine("pageBody: " + pageBody);
					}*/
				}
				catch(Exception e)
				{
					Console.WriteLine("Error Reading File: " + e);
				}
				header = CreateHeader(sHTTPVersion, mimeType, bytes.Length, "200 OK");
				SendToBrowser(channel, header);
				channel.Write(bytes, 0, bytes.Length);
			}
			
			//Test Puposes only (Creates Static Page) Maybe use to create the 404 file not found page
			string content = CreateContent(remote);
			
			//Byte[] byteHtmlData = Encoding.ASCII.GetBytes(htmlPage);
			//string sMimeType = "";
			//int iTotBytes = byteHtmlData.Length;
			
			Byte[] byteHtmlData = Encoding.ASCII.GetBytes(content);
			
			int TotBytesToSend = byteHtmlData.Length;
			
			
			
			//string header = CreateHeader(mimeType, TotalBytesToSend);
			//SendToBrowser(channel, header);
			//SendToBrowser(channel, content);
			
			//SendContent(channel, content);
		}
		public string CreateHeader(string httpVersion, string mimeType, int TotalBytesToSend, string StatusCode)
		{
			string header = httpVersion + " " + StatusCode + "\r\n";
			header = header + "Content-type: " + mimeType + "\r\n";
			header = header + "Content-Length: " + TotalBytesToSend + "\r\n\r\n";
			return header;
		}

	
		public void SendToBrowser(NetworkStream channel, string data)
		{
			/*StreamWriter sw = new StreamWriter(channel);
			sw.Write(data);
			
			sw.Flush();*/
			Console.WriteLine("Encoding: ");
			
			SendToBrowser(channel, Encoding.ASCII.GetBytes(data));
		}
		public void SendToBrowser(NetworkStream channel, Byte[] bSendData)
		{
			int numBytes = 0;
			try
			{				
				channel.Write(bSendData,0,bSendData.Length);
					
			}
			catch(Exception e)
			{
				Console.WriteLine("Error Occured while writing to browser: " + e);
			}
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
				if( stopPosition == -1 ) stopPosition = request.Length;
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
		public string GetLocalPath(string WebServerRoot, string Dirname)
		{
			StreamReader sr;
			String sLine = "";
			String sVirtualDir = "";
			String sRealDir = "";
			int iStartPos = 0;
			
			//remove extra spaces
			Dirname.Trim();
			
			//convert to lowercase
			WebServerRoot = WebServerRoot.ToLower();
			
			//convert to lowercase
			Dirname = Dirname.ToLower();

			try
			{
				Uri ownPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
				sr = new StreamReader(ownPath.LocalPath + ".conf");
				
				sLine = sr.ReadLine();
				//skip to the correct configuration settings
				while(!sLine.StartsWith("%"))
				{
					sLine = sr.ReadLine();
				}
				//ignore the commentted parts of the configuration file
				while(sLine.StartsWith("#"))
				{
					sLine = sr.ReadLine();
				}
				while ((sLine = sr.ReadLine()) != null)
				{
					while(sLine.StartsWith("#"))
					{
						sLine = sr.ReadLine();
					}
					sLine.Trim();
					
					if (sLine.Length > 0)
					{
						//find the separator
						iStartPos = sLine.IndexOf(";");
						
						Console.WriteLine("StartPos: " + iStartPos);

						//Convert to lowercase
						sLine = sLine.ToLower();

						sVirtualDir = sLine.Substring(0, iStartPos);
						Console.WriteLine("VirtualDir: " + sVirtualDir);
						sRealDir = sLine.Substring(iStartPos + 1);
						Console.WriteLine("RealDir: " + sRealDir);

						if (sVirtualDir == Dirname)
						{
							break;
						}
					}

				}
			}
			catch(Exception e)
			{
				Console.WriteLine("An mike Exception Occured: " + e.ToString());
			}

			if (sVirtualDir == Dirname)
				return sRealDir;
			else
				return "";
			
		}
		public string GetTheDefaultFileName(string LocalDirectory)
		{
			StreamReader sr;
			String sLine = "";

			try
			{
				//Open the configuration file to find the list of default files
				Uri ownPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
				sr = new StreamReader(ownPath.LocalPath + ".conf");
				Console.WriteLine("LocalDirectory: " + LocalDirectory);
				sLine = sr.ReadLine();
				//Ignore Comments in the config file
				while (sLine.StartsWith("#"))
				{
					sLine = sr.ReadLine();
					
				}
				while ((sLine = sr.ReadLine()) != null)
				{
					//Look for the default file in the web server root folder
					Console.WriteLine("sLine: " + sLine);
					if (File.Exists(LocalDirectory + sLine) == true)
						break;
				}						
			}
			catch(Exception e)
			{
				Console.WriteLine("An mandy Exception Occured : " + e.ToString());
			}
			if (File.Exists(LocalDirectory + sLine) == true)
			{
				Console.WriteLine("Found file: " + LocalDirectory + sLine);
				return sLine;
			}
			else
			{
				Console.WriteLine("Did NOT find file: " + LocalDirectory + sLine);
				return "";
			}
		}
		
		#endregion


		#region IPlugin Members

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
			Uri thisPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
			if(File.Exists(thisPath.LocalPath + ".conf") == true)
			{
				Console.WriteLine("Configuration File Exists...");
			}
			else
			{	
				Console.WriteLine("Creating Mikes " + System.Reflection.Assembly.GetExecutingAssembly().CodeBase + ".conf File");
				Uri ownPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
				StreamWriter sw = new StreamWriter(ownPath.LocalPath + ".conf");
				sw.WriteLine("######################\r\n#Default File Names\r\n\r\ndefault.html\r\ndefault.htm\r\nIndex.html\r\nIndex.htm;\r\n\r\n%\r\n#####################\r\n#Default File Paths\r\n#Format: <Virtual Dir>; <Local Path>\r\n\r\n/test/; C:\\myWebServerRoot\\Imtiaz\\");
				sw.Close();
			}
		}

		#endregion

	}
}
