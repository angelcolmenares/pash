using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Clear", "EventLog", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135198", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public sealed class ClearEventLogCommand : PSCmdlet
	{
		private string[] _logName;

		private string[] _computerName;

		[Alias(new string[] { "Cn" })]
		[Parameter(Position=1, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this._computerName;
			}
			set
			{
				this._computerName = value;
			}
		}

		[Alias(new string[] { "LN" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] LogName
		{
			get
			{
				return this._logName;
			}
			set
			{
				this._logName = value;
			}
		}

		public ClearEventLogCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this._computerName = strArrays;
		}

		protected override void BeginProcessing()
		{
			string str;
			string[] strArrays = this._computerName;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str1 = strArrays[i];
				if (str1.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) || str1.Equals(".", StringComparison.OrdinalIgnoreCase))
				{
					str = "localhost";
				}
				else
				{
					str = str1;
				}
				string[] strArrays1 = this._logName;
				for (int j = 0; j < (int)strArrays1.Length; j++)
				{
					string str2 = strArrays1[j];
					try
					{
						if (EventLog.Exists(str2, str1))
						{
							if (base.ShouldProcess(StringUtil.Format(EventlogResources.ClearEventLogWarning, str2, str)))
							{
								EventLog eventLog = new EventLog(str2, str1);
								eventLog.Clear();
							}
						}
						else
						{
							ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, str2, str)), null, ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
					}
					catch (IOException oException)
					{
						ErrorRecord errorRecord1 = new ErrorRecord(new IOException(StringUtil.Format(EventlogResources.PathDoesNotExist, null, str)), null, ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord1);
					}
					catch (Win32Exception win32Exception)
					{
						ErrorRecord errorRecord2 = new ErrorRecord(new Win32Exception(StringUtil.Format(EventlogResources.NoAccess, null, str)), null, ErrorCategory.PermissionDenied, null);
						base.WriteError(errorRecord2);
					}
					catch (InvalidOperationException invalidOperationException)
					{
						ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.OSWritingError, new object[0])), null, ErrorCategory.ReadError, null);
						base.WriteError(errorRecord3);
					}
				}
			}
		}
	}
}