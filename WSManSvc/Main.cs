using System;
using Microsoft.WSMan;

namespace WSManSvc
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//Cross-Platform Environment Variables
			Environment.SetEnvironmentVariable ("LOCALAPPDATA", "~/.config");
			Environment.SetEnvironmentVariable ("MONO_XMLSERIALIZER_THS", "no");
			Environment.SetEnvironmentVariable ("MONO_XMLSERIALIZER_DEBUG", "0");

			Console.WriteLine ("WSMan Service");

			using (var host = new WSManServiceHost())
			{
				host.Open ();
				Console.WriteLine ("Service Opened.");
				Console.WriteLine ("Press enter to close server...");

				Console.ReadLine ();
				Environment.Exit (0);
			}
			Console.WriteLine ("Service Closed");
			Console.WriteLine ("Exiting...");
		}
	}
}
