using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands;
using System.Security;

namespace System.Management.Automation
{
	internal static class PowerShellConfiguration
	{
		private static readonly object _lock = new object();
		private static PowerShellEngineConfiguration _powerShellEngine;
		private static PowerShellModuleLoggingConfiguraiton _powerShellLogging;
		private static Dictionary<string, string> _policyValues = new Dictionary<string, string>();

		private static XDocument GetConfigDoc ()
		{
			var root = "";
			var assembly = Assembly.GetEntryAssembly ();
			if (assembly == null) {
				//We're on web!
				root = "/Users/bruno/Projects/PowerShell/v1.0"; //TODO: Urgent Change this!
			} else {
				FileInfo entry = new FileInfo (Assembly.GetEntryAssembly ().Location);
				root = entry.Directory.FullName;
			}
			string configFile = Path.Combine (root, "PowerShell.xml");
			if (File.Exists (configFile)) {
				return XDocument.Load (configFile);
			}
			return null;
		}

		private static void LoadDefaults (ref PowerShellEngineConfiguration powerShellEngine)
		{
			var assembly = Assembly.GetEntryAssembly ();
			if (assembly == null) {
				//We're on web!
				powerShellEngine.ApplicationBase = "/Users/bruno/Projects/PowerShell/v1.0"; //TODO: Urgent Change this!
			} else {
				FileInfo entry = new FileInfo (Assembly.GetEntryAssembly ().Location);
				powerShellEngine.ApplicationBase = entry.Directory.FullName;
			}

		}

		private static void LoadDefaults (ref PowerShellModuleLoggingConfiguraiton powerShellLogging)
		{
			powerShellLogging.EnableModuleLogging = ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Undefined;
			powerShellLogging.ModuleNames = new List<string>();
		}

		static PowerShellConfiguration ()
		{
			PowerShellEngineConfiguration powerShellEngine = new PowerShellEngineConfiguration ();
			PowerShellModuleLoggingConfiguraiton powerShellLogging = new PowerShellModuleLoggingConfiguraiton();
			if (!IsWindows) {
				LoadDefaults(ref powerShellEngine);
				LoadDefaults(ref powerShellLogging);
				XDocument doc = GetConfigDoc ();
				if (doc != null)
				{

					var root = doc.Document.Root;
					var policyRoot = root.Element ("Policy");
					var engine = root.Element ("PowerShellEngine");
					var appBase = engine.Element ("ApplicationBase").Value;
					if (!string.IsNullOrEmpty (appBase))
					{
						powerShellEngine.ApplicationBase = appBase;
					}
					if (policyRoot != null) {
						var policyNodes  = policyRoot.Elements ();
						foreach(var elPol in policyNodes)
						{
							_policyValues.Add (elPol.Name.LocalName, elPol.Value);
						}
					}
					var moduleLogging = root.Element ("ModuleLogging");
					var enableModLogging = moduleLogging.Element ("EnableModuleLogging").Value;
					if (string.Equals(enableModLogging, "0", StringComparison.OrdinalIgnoreCase))
					{
						powerShellLogging.EnableModuleLogging = ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Disabled;
					}
					else if (!string.Equals(enableModLogging, "1", StringComparison.OrdinalIgnoreCase))
					{
						powerShellLogging.EnableModuleLogging = ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Undefined;
					}
					else {
						powerShellLogging.EnableModuleLogging = ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Enabled;
					}

					var moduleNamesKey = moduleLogging.Element ("ModuleNames").Elements ("ModuleName");
					List<string> moduleNames = new List<string>();
					foreach(var x in moduleNamesKey)
					{
						moduleNames.Add (x.Value);
					}
					powerShellLogging.ModuleNames = moduleNames;
				}
				
			} else {
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Xamarin\PowerShell\" + PSVersionInfo.RegistryVersionKey + @"\PowerShellEngine"))
				{
					powerShellEngine.ApplicationBase = (string) key.GetValue("ApplicationBase");
				}
				using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"Software\Xamarin\PowerShell\ModuleLogging")) //"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging"
				{
					if (key2 != null)
					{
						IEnumerable<string> moduleNames = null;
						powerShellLogging.EnableModuleLogging = GetModuleLoggingValue("EnableModuleLogging", key2, out moduleNames);
						powerShellLogging.ModuleNames = moduleNames;
						key2.Close();
					}
				}
			}

			lock (_lock) {
				_powerShellEngine = powerShellEngine;
				_powerShellLogging = powerShellLogging;
			}
		}

		private static ModuleCmdletBase.ModuleLoggingGroupPolicyStatus GetModuleLoggingValue(string groupPolicyValue, RegistryKey key, out IEnumerable<string> moduleNames)
		{
			ModuleCmdletBase.ModuleLoggingGroupPolicyStatus undefined = ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Undefined;
			moduleNames = new List<string>();
			if (key != null)
			{
				object obj2 = key.GetValue(groupPolicyValue);
				if (obj2 == null)
				{
					return undefined;
				}
				if (string.Equals(obj2.ToString(), "0", StringComparison.OrdinalIgnoreCase))
				{
					return ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Disabled;
				}
				if (!string.Equals(obj2.ToString(), "1", StringComparison.OrdinalIgnoreCase))
				{
					return undefined;
				}
				undefined = ModuleCmdletBase.ModuleLoggingGroupPolicyStatus.Enabled;
				try
				{
					using (RegistryKey key2 = key.OpenSubKey("ModuleNames"))
					{
						if (key2 != null)
						{
							string[] valueNames = key2.GetValueNames();
							if ((valueNames != null) && (valueNames.Length > 0))
							{
								moduleNames = new List<string>(valueNames);
							}
						}
					}
				}
				catch (SecurityException)
				{
				}
			}
			return undefined;
		}

		public static bool IsWindows {
			get { return false; /* Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix; */ }
		}

		public static PowerShellEngineConfiguration PowerShellEngine {
			get { return _powerShellEngine; }
		}

		public static PowerShellModuleLoggingConfiguraiton ModuleLogging {
			get { return _powerShellLogging; }
		}

		public static string GetPolicyValue (string policyValueName, string policyValue)
		{
			if (_policyValues.ContainsKey (policyValueName)) {
				return _policyValues[policyValueName];
			}
			return policyValue;
		}

		public static int GetPolicyValue (string policyValueName, int policyValue)
		{
			if (_policyValues.ContainsKey (policyValueName)) {
				int ret = 0;
				if (Int32.TryParse(_policyValues[policyValueName], out ret)) {
					return ret;
				}
			}
			return policyValue;
		}

		public static bool GetPolicyValue (string policyValueName, bool policyValue)
		{
			if (_policyValues.ContainsKey (policyValueName)) {
				bool ret = policyValue;
				if (Boolean.TryParse(_policyValues[policyValueName], out ret)) {
					return ret;
				}
			}
			return policyValue;
		}

	}

	internal class PowerShellEngineConfiguration
	{
		public string ApplicationBase
		{
			get;set;
		}
	}

	internal class PowerShellModuleLoggingConfiguraiton
	{
		public ModuleCmdletBase.ModuleLoggingGroupPolicyStatus EnableModuleLogging
		{
			get;set;
		}

		public IEnumerable<string> ModuleNames
		{
			get;set;
		}
	}

}

