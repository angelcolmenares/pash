using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.PowerShell
{
	[RunInstaller(true)]
	public sealed class EngineInstaller : PSInstaller
	{
		private Dictionary<string, object> _regValues;

		private static string EngineVersion
		{
			get
			{
				return PSVersionInfo.FeatureVersionString;
			}
		}

		internal sealed override string RegKey
		{
			get
			{
				return "PowerShellEngine";
			}
		}

		internal sealed override Dictionary<string, object> RegValues
		{
			get
			{
				if (this._regValues == null)
				{
					this._regValues = new Dictionary<string, object>();
					this._regValues["PowerShellVersion"] = EngineInstaller.EngineVersion;
					this._regValues["ApplicationBase"] = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					this._regValues["ConsoleHostAssemblyName"] = Assembly.GetExecutingAssembly().FullName;
					this._regValues["ConsoleHostModuleName"] = Assembly.GetExecutingAssembly().Location;
					this._regValues["RuntimeVersion"] = Assembly.GetExecutingAssembly().ImageRuntimeVersion;
				}
				return this._regValues;
			}
		}

		public EngineInstaller()
		{
		}
	}
}