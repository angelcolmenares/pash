using System;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Microsoft.PowerShell
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
				//Cross-Platform Environment Variables
				Environment.SetEnvironmentVariable ("LOCALAPPDATA", "~/.config");
				Environment.SetEnvironmentVariable ("MONO_XMLSERIALIZER_THS", "no");
				Environment.SetEnvironmentVariable ("MONO_XMLSERIALIZER_DEBUG", "0");


				Console.TreatControlCAsInput = true;
				ConsoleShell.Start(RunspaceConfiguration.Create(), "PowerShell", "", new string[] { "-ImportSystemModules" });
            }
            catch (Exception ex)
            {
				Console.WriteLine ();
                Console.WriteLine(ex.Message);
				Console.WriteLine ();
				Console.WriteLine (ex.StackTrace);
				Console.WriteLine ();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
