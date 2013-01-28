using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Show", "EventLog", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135257", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public sealed class ShowEventLogCommand : PSCmdlet
	{
		private string _computerName;

		[Alias(new string[] { "CN" })]
		[Parameter(Position=0)]
		[ValidateNotNullOrEmpty]
		public string ComputerName
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

		public ShowEventLogCommand()
		{
			this._computerName = ".";
		}

		protected override void BeginProcessing()
		{
			try
			{
				string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "eventvwr.exe");
				Process.Start(str, this._computerName);
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				int nativeErrorCode = win32Exception.NativeErrorCode;
				if (!nativeErrorCode.Equals(2))
				{
					ErrorRecord errorRecord = new ErrorRecord(win32Exception, "Win32Exception", ErrorCategory.InvalidArgument, null);
					base.WriteError(errorRecord);
				}
				else
				{
					string str1 = StringUtil.Format(EventlogResources.NotSupported, new object[0]);
					InvalidOperationException invalidOperationException = new InvalidOperationException(str1);
					ErrorRecord errorRecord1 = new ErrorRecord(invalidOperationException, "Win32Exception", ErrorCategory.InvalidOperation, null);
					base.WriteError(errorRecord1);
				}
			}
			catch (SystemException systemException1)
			{
				SystemException systemException = systemException1;
				ErrorRecord errorRecord2 = new ErrorRecord(systemException, "InvalidComputerName", ErrorCategory.InvalidArgument, this._computerName);
				base.WriteError(errorRecord2);
			}
		}
	}
}