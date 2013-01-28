using Microsoft.PowerShell.Commands.Management;
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Security;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("New", "EventLog", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135235", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public class NewEventLogCommand : PSCmdlet
	{
		private string _categoryResourceFile;

		private string[] _computerName;

		private string _logName;

		private string _messageResourceFile;

		private string _parameterResourceFile;

		private string[] _source;

		[Alias(new string[] { "CRF" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string CategoryResourceFile
		{
			get
			{
				return this._categoryResourceFile;
			}
			set
			{
				this._categoryResourceFile = value;
			}
		}

		[Alias(new string[] { "CN" })]
		[Parameter(Position=2)]
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
		[Parameter(Mandatory=true, Position=0)]
		[ValidateNotNullOrEmpty]
		public string LogName
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

		[Alias(new string[] { "MRF" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string MessageResourceFile
		{
			get
			{
				return this._messageResourceFile;
			}
			set
			{
				this._messageResourceFile = value;
			}
		}

		[Alias(new string[] { "PRF" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string ParameterResourceFile
		{
			get
			{
				return this._parameterResourceFile;
			}
			set
			{
				this._parameterResourceFile = value;
			}
		}

		[Alias(new string[] { "SRC" })]
		[Parameter(Mandatory=true, Position=1)]
		[ValidateNotNullOrEmpty]
		public string[] Source
		{
			get
			{
				return this._source;
			}
			set
			{
				this._source = value;
			}
		}

		public NewEventLogCommand()
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
				try
				{
					string[] strArrays1 = this._source;
					for (int j = 0; j < (int)strArrays1.Length; j++)
					{
						string str2 = strArrays1[j];
						if (EventLog.SourceExists(str2, str1))
						{
							object[] objArray = new object[3];
							objArray[1] = str;
							objArray[2] = str2;
							ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.SourceExistInComp, objArray)), null, ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
						else
						{
							EventSourceCreationData eventSourceCreationDatum = new EventSourceCreationData(str2, this._logName);
							eventSourceCreationDatum.MachineName = str1;
							if (!string.IsNullOrEmpty(this._messageResourceFile))
							{
								eventSourceCreationDatum.MessageResourceFile = this._messageResourceFile;
							}
							if (!string.IsNullOrEmpty(this._parameterResourceFile))
							{
								eventSourceCreationDatum.ParameterResourceFile = this._parameterResourceFile;
							}
							if (!string.IsNullOrEmpty(this._categoryResourceFile))
							{
								eventSourceCreationDatum.CategoryResourceFile = this._categoryResourceFile;
							}
							EventLog.CreateEventSource(eventSourceCreationDatum);
						}
					}
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					this.WriteNonTerminatingError(invalidOperationException, EventlogResources.PermissionDenied, "PermissionDenied", ErrorCategory.PermissionDenied, this._logName, str, null, null);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					ErrorRecord errorRecord1 = new ErrorRecord(argumentException, "NewEventlogException", ErrorCategory.InvalidArgument, null);
					base.WriteError(errorRecord1);
				}
				catch (SecurityException securityException1)
				{
					SecurityException securityException = securityException1;
					this.WriteNonTerminatingError(securityException, EventlogResources.AccessIsDenied, "AccessIsDenied", ErrorCategory.InvalidOperation, null, null, null, null);
				}
			}
		}

		private void WriteNonTerminatingError(Exception exception, string resourceId, string errorId, ErrorCategory category, string _logName, string _compName, string _source, string _resourceFile)
		{
			object[] objArray = new object[4];
			objArray[0] = _logName;
			objArray[1] = _compName;
			objArray[2] = _source;
			objArray[3] = _resourceFile;
			Exception exception1 = new Exception(StringUtil.Format(resourceId, objArray), exception);
			base.WriteError(new ErrorRecord(exception1, errorId, category, null));
		}
	}
}