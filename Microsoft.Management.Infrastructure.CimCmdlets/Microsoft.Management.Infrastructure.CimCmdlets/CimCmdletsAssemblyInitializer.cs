using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	public sealed class CimCmdletsAssemblyInitializer : IModuleAssemblyInitializer
	{
		internal static CimCmdletsAssemblyInitializer.CimCmdletAliasEntry[] Aliases;

		static CimCmdletsAssemblyInitializer()
		{
			CimCmdletsAssemblyInitializer.CimCmdletAliasEntry[] cimCmdletAliasEntry = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry[12];
			cimCmdletAliasEntry[0] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("gcim", "Get-CimInstance");
			cimCmdletAliasEntry[1] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("scim", "Set-CimInstance");
			cimCmdletAliasEntry[2] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("ncim", "New-CimInstance ");
			cimCmdletAliasEntry[3] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("rcim", "Remove-CimInstance");
			cimCmdletAliasEntry[4] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("icim", "Invoke-CimMethod");
			cimCmdletAliasEntry[5] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("gcai", "Get-CimAssociatedInstance");
			cimCmdletAliasEntry[6] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("rcie", "Register-CimIndicationEvent");
			cimCmdletAliasEntry[7] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("ncms", "New-CimSession");
			cimCmdletAliasEntry[8] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("rcms", "Remove-CimSession");
			cimCmdletAliasEntry[9] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("gcms", "Get-CimSession");
			cimCmdletAliasEntry[10] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("ncso", "New-CimSessionOption");
			cimCmdletAliasEntry[11] = new CimCmdletsAssemblyInitializer.CimCmdletAliasEntry("gcls", "Get-CimClass");
			CimCmdletsAssemblyInitializer.Aliases = cimCmdletAliasEntry;
		}

		public CimCmdletsAssemblyInitializer()
		{
		}

		public void OnImport()
		{
			DebugHelper.WriteLogEx();
			System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
			using (powerShell)
			{
				CimCmdletsAssemblyInitializer.CimCmdletAliasEntry[] aliases = CimCmdletsAssemblyInitializer.Aliases;
				for (int i = 0; i < (int)aliases.Length; i++)
				{
					CimCmdletsAssemblyInitializer.CimCmdletAliasEntry cimCmdletAliasEntry = aliases[i];
					object[] name = new object[3];
					name[0] = cimCmdletAliasEntry.Name;
					name[1] = cimCmdletAliasEntry.Value;
					name[2] = cimCmdletAliasEntry.Options;
					powerShell.AddScript(string.Format(CultureInfo.CurrentUICulture, "New-Alias -Name {0} -Value {1} -Option {2}", name));
					object[] value = new object[3];
					value[0] = cimCmdletAliasEntry.Name;
					value[1] = cimCmdletAliasEntry.Value;
					value[2] = cimCmdletAliasEntry.Options;
					DebugHelper.WriteLog("Add commands {0} of {1} with option {2} to current runspace.", 1, value);
				}
				Collection<PSObject> pSObjects = powerShell.Invoke();
				object[] count = new object[1];
				count[0] = pSObjects.Count;
				DebugHelper.WriteLog("Invoke results {0}.", 1, count);
			}
		}

		internal sealed class CimCmdletAliasEntry
		{
			private string _name;

			private string _value;

			private ScopedItemOptions _options;

			internal string Name
			{
				get
				{
					return this._name;
				}
			}

			internal ScopedItemOptions Options
			{
				get
				{
					return this._options;
				}
			}

			internal string Value
			{
				get
				{
					return this._value;
				}
			}

			internal CimCmdletAliasEntry(string name, string value)
			{
				this._value = string.Empty;
				this._options = ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope;
				this._name = name;
				this._value = value;
			}
		}
	}
}