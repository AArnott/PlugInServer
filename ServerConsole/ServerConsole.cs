using System;
using System.IO;
using Byu.IT347.PluginServer.ServerServices;

namespace Byu.IT347.PluginServer.ServerConsole
{
	class ServerConsole
	{
		private const string PluginDirectoryName = "plugins";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("Plug-in Server starting.");
			
			Services services = new Services();
			string pluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), PluginDirectoryName);
			if( !Directory.Exists(pluginDirectory) ) Directory.CreateDirectory(pluginDirectory);
			services.PluginDirectory = pluginDirectory;
			services.Start();

			Console.WriteLine("Watching directory for changes: {0}", services.PluginDirectory);
			
			Console.ReadLine();
		}
	}
}
