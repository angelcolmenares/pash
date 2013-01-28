using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class CimJobContext
	{
		public string ClassName
		{
			get
			{
				return CimJobContext.GetCimClassName(this.CmdletInvocationContext.CmdletDefinitionContext.CmdletizationClassName);
			}
		}

		public string ClassNameOrNullIfResourceUriIsUsed
		{
			get
			{
				if (this.CmdletInvocationContext.CmdletDefinitionContext.ResourceUri == null)
				{
					return this.ClassName;
				}
				else
				{
					return null;
				}
			}
		}

		public CimCmdletInvocationContext CmdletInvocationContext
		{
			get;
			private set;
		}

		public InvocationInfo CmdletInvocationInfo
		{
			get
			{
				return this.CmdletInvocationContext.CmdletInvocationInfo;
			}
		}

		public string CmdletizationClassName
		{
			get
			{
				return this.CmdletInvocationContext.CmdletDefinitionContext.CmdletizationClassName;
			}
		}

		public Version CmdletizationModuleVersion
		{
			get
			{
				return this.CmdletInvocationContext.CmdletDefinitionContext.CmdletizationModuleVersion;
			}
		}

		public ActionPreference DebugActionPreference
		{
			get
			{
				return this.CmdletInvocationContext.DebugActionPreference;
			}
		}

		public ActionPreference ErrorActionPreference
		{
			get
			{
				return this.CmdletInvocationContext.ErrorActionPreference;
			}
		}

		public bool IsRunningInBackground
		{
			get
			{
				return this.CmdletInvocationContext.IsRunningInBackground;
			}
		}

		public string Namespace
		{
			get
			{
				if (string.IsNullOrEmpty(this.CmdletInvocationContext.NamespaceOverride))
				{
					return CimJobContext.GetCimNamespace(this.CmdletInvocationContext.CmdletDefinitionContext.CmdletizationClassName);
				}
				else
				{
					return this.CmdletInvocationContext.NamespaceOverride;
				}
			}
		}

		public CimSession Session
		{
			get;
			private set;
		}

		public MshCommandRuntime.ShouldProcessPossibleOptimization ShouldProcessOptimization
		{
			get
			{
				return this.CmdletInvocationContext.ShouldProcessOptimization;
			}
		}

		public bool ShowComputerName
		{
			get
			{
				return this.CmdletInvocationContext.ShowComputerName;
			}
		}

		public bool SupportsShouldProcess
		{
			get
			{
				return this.CmdletInvocationContext.CmdletDefinitionContext.SupportsShouldProcess;
			}
		}

		public object TargetObject
		{
			get;
			private set;
		}

		public ActionPreference VerboseActionPreference
		{
			get
			{
				return this.CmdletInvocationContext.VerboseActionPreference;
			}
		}

		public ActionPreference WarningActionPreference
		{
			get
			{
				return this.CmdletInvocationContext.WarningActionPreference;
			}
		}

		internal CimJobContext(CimCmdletInvocationContext cmdletInvocationContext, CimSession session, object targetObject)
		{
			this.CmdletInvocationContext = cmdletInvocationContext;
			this.Session = session;
			CimJobContext cimJobContext = this;
			object obj = targetObject;
			object className = obj;
			if (obj == null)
			{
				className = this.ClassName;
			}
			cimJobContext.TargetObject = className;
		}

		private static void ExtractCimNamespaceAndClassName(string cmdletizationClassName, out string cimNamespace, out string cimClassName)
		{
			int num = cmdletizationClassName.LastIndexOf('\\');
			int num1 = cmdletizationClassName.LastIndexOf('/');
			int num2 = Math.Max(num, num1);
			if (num2 == -1)
			{
				cimNamespace = null;
				cimClassName = cmdletizationClassName;
				return;
			}
			else
			{
				cimNamespace = cmdletizationClassName.Substring(0, num2);
				cimClassName = cmdletizationClassName.Substring(num2 + 1, cmdletizationClassName.Length - num2 - 1);
				return;
			}
		}

		private static string GetCimClassName(string cmdletizationClassName)
		{
			string str = null;
			string str1 = null;
			CimJobContext.ExtractCimNamespaceAndClassName(cmdletizationClassName, out str, out str1);
			return str1;
		}

		private static string GetCimNamespace(string cmdletizationClassName)
		{
			string str = null;
			string str1 = null;
			CimJobContext.ExtractCimNamespaceAndClassName(cmdletizationClassName, out str, out str1);
			return str;
		}

		internal string PrependComputerNameToMessage(string message)
		{
			string computerName = this.Session.ComputerName;
			if (computerName != null)
			{
				object[] objArray = new object[2];
				objArray[0] = computerName;
				objArray[1] = message;
				return string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_ComputerNameConcatenationTemplate, objArray);
			}
			else
			{
				return message;
			}
		}
	}
}