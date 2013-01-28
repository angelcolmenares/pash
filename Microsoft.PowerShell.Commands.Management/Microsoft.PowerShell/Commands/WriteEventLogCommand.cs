using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Write", "EventLog", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135281", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public sealed class WriteEventLogCommand : PSCmdlet
	{
		private string _logName;

		private string _source;

		private EventLogEntryType _entryType;

		private short _category;

		private int _eventId;

		private string _message;

		private byte[] _rawData;

		private string compname;

		[Parameter]
		public short Category
		{
			get
			{
				return this._category;
			}
			set
			{
				this._category = value;
			}
		}

		[Alias(new string[] { "CN" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string ComputerName
		{
			get
			{
				return this.compname;
			}
			set
			{
				this.compname = value;
			}
		}

		[Alias(new string[] { "ET" })]
		[Parameter(Position=3)]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "Error", "Information", "FailureAudit", "SuccessAudit", "Warning" })]
		public EventLogEntryType EntryType
		{
			get
			{
				return this._entryType;
			}
			set
			{
				this._entryType = value;
			}
		}

		[Alias(new string[] { "ID", "EID" })]
		[Parameter(Position=2, Mandatory=true)]
		[ValidateNotNullOrEmpty]
		[ValidateRange(0, 0xffff)]
		public int EventId
		{
			get
			{
				return this._eventId;
			}
			set
			{
				this._eventId = value;
			}
		}

		[Alias(new string[] { "LN" })]
		[Parameter(Position=0, Mandatory=true)]
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

		[Alias(new string[] { "MSG" })]
		[Parameter(Position=4, Mandatory=true)]
		[ValidateLength(0, 0x7ffe)]
		[ValidateNotNullOrEmpty]
		public string Message
		{
			get
			{
				return this._message;
			}
			set
			{
				this._message = value;
			}
		}

		[Alias(new string[] { "RD" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public byte[] RawData
		{
			get
			{
				return this._rawData;
			}
			set
			{
				this._rawData = value;
			}
		}

		[Alias(new string[] { "SRC" })]
		[Parameter(Position=1, Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string Source
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

		public WriteEventLogCommand()
		{
			this._entryType = EventLogEntryType.Information;
			this._category = 1;
			this.compname = ".";
		}

		protected override void BeginProcessing()
		{
			string str;
			if (this.compname.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) || this.compname.Equals(".", StringComparison.OrdinalIgnoreCase))
			{
				str = "localhost";
			}
			else
			{
				str = this.compname;
			}
			try
			{
				if (EventLog.SourceExists(this._source, this.compname))
				{
					if (EventLog.Exists(this._logName, this.compname))
					{
						EventLog eventLog = new EventLog(this._logName, this.compname, this._source);
						eventLog.WriteEntry(this._message, this._entryType, this._eventId, this._category, this._rawData);
					}
					else
					{
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, this._logName, str)), null, ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord);
					}
				}
				else
				{
					object[] objArray = new object[3];
					objArray[1] = str;
					objArray[2] = this._source;
					ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.SourceDoesNotExist, objArray)), null, ErrorCategory.InvalidOperation, null);
					base.WriteError(errorRecord1);
				}
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				this.WriteNonTerminatingError(argumentException, argumentException.Message, argumentException.Message, ErrorCategory.InvalidOperation);
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				object[] objArray1 = new object[3];
				objArray1[0] = this._logName;
				objArray1[2] = this._source;
				this.WriteNonTerminatingError(invalidOperationException, "AccessDenied", StringUtil.Format(EventlogResources.AccessDenied, objArray1), ErrorCategory.PermissionDenied);
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				object[] objArray2 = new object[3];
				this.WriteNonTerminatingError(win32Exception, "OSWritingError", StringUtil.Format(EventlogResources.OSWritingError, objArray2), ErrorCategory.WriteError);
			}
			catch (IOException oException1)
			{
				IOException oException = oException1;
				object[] objArray3 = new object[3];
				objArray3[1] = this.compname;
				this.WriteNonTerminatingError(oException, "PathDoesNotExist", StringUtil.Format(EventlogResources.PathDoesNotExist, objArray3), ErrorCategory.InvalidOperation);
			}
		}

		private void WriteNonTerminatingError(Exception exception, string errorId, string errorMessage, ErrorCategory category)
		{
			Exception exception1 = new Exception(errorMessage, exception);
			base.WriteError(new ErrorRecord(exception1, errorId, category, null));
		}
	}
}