using System;
using System.Management.Automation.Runspaces;
using System.IO;

namespace System.Management.Automation
{
	[Cmdlet("Import", "SystemModules", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141553")]
	public class ImportSystemModulesCommand : Cmdlet
	{
		protected override void ProcessRecord ()
		{
			base.ProcessRecord ();
		}
	}
}

