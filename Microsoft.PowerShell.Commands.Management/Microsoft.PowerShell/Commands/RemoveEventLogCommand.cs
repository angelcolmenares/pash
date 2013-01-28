using Microsoft.PowerShell.Commands.Management;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Remove", "EventLog", SupportsShouldProcess=true, DefaultParameterSetName="Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135248", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public class RemoveEventLogCommand : PSCmdlet
	{
		private string[] computername;

		private string[] logname;

		private string[] source;

		[Alias(new string[] { "CN" })]
		[Parameter(Position=1)]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
			}
		}

		[Alias(new string[] { "LN" })]
		[Parameter(Mandatory=true, Position=0, ParameterSetName="Default")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string[] LogName
		{
			get
			{
				return this.logname;
			}
			set
			{
				this.logname = value;
			}
		}

		[Alias(new string[] { "SRC" })]
		[Parameter(ParameterSetName="Source")]
		[ValidateNotNull]
		[ValidateNotNullOrEmpty]
		public string[] Source
		{
			get
			{
				return this.source;
			}
			set
			{
				this.source = value;
			}
		}

		public RemoveEventLogCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this.computername = strArrays;
		}

		protected override void BeginProcessing()
		{
			string str;
			try
			{
				string[] strArrays = this.computername;
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
					if (!base.ParameterSetName.Equals("Default"))
					{
						string[] strArrays1 = this.source;
						for (int j = 0; j < (int)strArrays1.Length; j++)
						{
							string str2 = strArrays1[j];
							try
							{
								if (!EventLog.SourceExists(str2, str1))
								{
									object[] objArray = new object[3];
									objArray[0] = "";
									objArray[1] = str;
									objArray[2] = str2;
									ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.SourceDoesNotExist, objArray)), null, ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord);
								}
								else
								{
									if (base.ShouldProcess(StringUtil.Format(EventlogResources.RemoveSourceWarning, str2, str)))
									{
										EventLog.DeleteEventSource(str2, str1);
									}
								}
							}
							catch (IOException oException)
							{
								ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.PathDoesNotExist, null, str)), null, ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord1);
							}
						}
					}
					else
					{
						string[] strArrays2 = this.logname;
						for (int k = 0; k < (int)strArrays2.Length; k++)
						{
							string str3 = strArrays2[k];
							try
							{
								if (!EventLog.Exists(str3, str1))
								{
									ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, str3, str)), null, ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord2);
								}
								else
								{
									if (base.ShouldProcess(StringUtil.Format(EventlogResources.RemoveEventLogWarning, str3, str)))
									{
										EventLog.Delete(str3, str1);
									}
								}
							}
							catch (IOException oException1)
							{
								ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.PathDoesNotExist, null, str)), null, ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord3);
							}
						}
					}
				}
			}
			catch (SecurityException securityException1)
			{
				SecurityException securityException = securityException1;
				ErrorRecord errorRecord4 = new ErrorRecord(securityException, "NewEventlogException", ErrorCategory.SecurityError, null);
				base.WriteError(errorRecord4);
			}
		}
	}
}