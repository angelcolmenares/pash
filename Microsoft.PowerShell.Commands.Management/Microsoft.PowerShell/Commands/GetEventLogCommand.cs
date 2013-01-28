using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "EventLog", DefaultParameterSetName="LogName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113314", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new Type[] { typeof(EventLog), typeof(EventLogEntry), typeof(string) })]
	public sealed class GetEventLogCommand : PSCmdlet
	{
		private string _logName;

		private string[] _computerName;

		private int _newest;

		private DateTime _after;

		private DateTime _before;

		private string[] _username;

		private long[] _instanceIds;

		private int[] _indexes;

		private string[] _entryTypes;

		private string[] _sources;

		private string _message;

		private SwitchParameter _asbaseobject;

		private SwitchParameter _list;

		private bool _asString;

		private bool isFilterSpecified;

		private bool isDateSpecified;

		private bool isThrowError;

		private DateTime Initial;

		[Parameter(ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		public DateTime After
		{
			get
			{
				return this._after;
			}
			set
			{
				this._after = value;
				this.isDateSpecified = true;
				this.isFilterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="LogName")]
		public SwitchParameter AsBaseObject
		{
			get
			{
				return this._asbaseobject;
			}
			set
			{
				this._asbaseobject = value;
			}
		}

		[Parameter(ParameterSetName="List")]
		public SwitchParameter AsString
		{
			get
			{
				return this._asString;
			}
			set
			{
				this._asString = value;
			}
		}

		[Parameter(ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		public DateTime Before
		{
			get
			{
				return this._before;
			}
			set
			{
				this._before = value;
				this.isDateSpecified = true;
				this.isFilterSpecified = true;
			}
		}

		[Alias(new string[] { "Cn" })]
		[Parameter]
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

		[Alias(new string[] { "ET" })]
		[Parameter(ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "Error", "Information", "FailureAudit", "SuccessAudit", "Warning" })]
		public string[] EntryType
		{
			get
			{
				return this._entryTypes;
			}
			set
			{
				this._entryTypes = value;
				this.isFilterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		[ValidateRange(1, 0x7fffffff)]
		public int[] Index
		{
			get
			{
				return this._indexes;
			}
			set
			{
				this._indexes = value;
				this.isFilterSpecified = true;
			}
		}

		[Parameter(Position=1, ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		[ValidateRange(0L, 0x7fffffffffffffffL)]
		public long[] InstanceId
		{
			get
			{
				return this._instanceIds;
			}
			set
			{
				this._instanceIds = value;
				this.isFilterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="List")]
		public SwitchParameter List
		{
			get
			{
				return this._list;
			}
			set
			{
				this._list = value;
			}
		}

		[Alias(new string[] { "LN" })]
		[Parameter(Position=0, Mandatory=true, ParameterSetName="LogName")]
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
		[Parameter(ParameterSetName="LogName")]
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
				this.isFilterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="LogName")]
		[ValidateRange(0, 0x7fffffff)]
		public int Newest
		{
			get
			{
				return this._newest;
			}
			set
			{
				this._newest = value;
			}
		}

		[Alias(new string[] { "ABO" })]
		[Parameter(ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		public string[] Source
		{
			get
			{
				return this._sources;
			}
			set
			{
				this._sources = value;
				this.isFilterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="LogName")]
		[ValidateNotNullOrEmpty]
		public string[] UserName
		{
			get
			{
				return this._username;
			}
			set
			{
				this._username = value;
				this.isFilterSpecified = true;
			}
		}

		public GetEventLogCommand()
		{
			this._computerName = new string[0];
			this._newest = 0x7fffffff;
			this.isThrowError = true;
			this.Initial = new DateTime();
		}

		protected override void BeginProcessing()
		{
			if (base.ParameterSetName != "List")
			{
				if (WildcardPattern.ContainsWildcardCharacters(this.LogName))
				{
					List<EventLog> matchingLogs = this.GetMatchingLogs(this.LogName);
					if (matchingLogs.Count != 1)
					{
						foreach (EventLog matchingLog in matchingLogs)
						{
							base.WriteObject(matchingLog);
						}
						return;
					}
					else
					{
						this.OutputEvents(matchingLogs[0].Log);
						return;
					}
				}
				else
				{
					this.OutputEvents(this.LogName);
					return;
				}
			}
			else
			{
				if ((int)this._computerName.Length <= 0)
				{
					EventLog[] eventLogs = EventLog.GetEventLogs();
					for (int i = 0; i < (int)eventLogs.Length; i++)
					{
						EventLog eventLog = eventLogs[i];
						if (!this.AsString)
						{
							base.WriteObject(eventLog);
						}
						else
						{
							base.WriteObject(eventLog.Log);
						}
					}
					return;
				}
				else
				{
					string[] strArrays = this._computerName;
					for (int j = 0; j < (int)strArrays.Length; j++)
					{
						string str = strArrays[j];
						EventLog[] eventLogArray = EventLog.GetEventLogs(str);
						for (int k = 0; k < (int)eventLogArray.Length; k++)
						{
							EventLog eventLog1 = eventLogArray[k];
							if (!this.AsString)
							{
								base.WriteObject(eventLog1);
							}
							else
							{
								base.WriteObject(eventLog1.Log);
							}
						}
					}
					return;
				}
			}
		}

		private bool FiltersMatch(EventLogEntry entry)
		{
			if (this._indexes == null || this._indexes.Contains(entry.Index))
			{
				if (this._instanceIds == null || this._instanceIds.Contains(entry.InstanceId))
				{
					if (this._entryTypes != null)
					{
						bool flag = false;
						string[] strArrays = this._entryTypes;
						int num = 0;
						while (num < (int)strArrays.Length)
						{
							string str = strArrays[num];
							if (!str.Equals(entry.EntryType.ToString(), StringComparison.CurrentCultureIgnoreCase))
							{
								num++;
							}
							else
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							return flag;
						}
					}
					if (this._sources != null)
					{
						bool flag1 = false;
						string[] strArrays1 = this._sources;
						int num1 = 0;
						while (num1 < (int)strArrays1.Length)
						{
							string str1 = strArrays1[num1];
							if (WildcardPattern.ContainsWildcardCharacters(str1))
							{
								this.isThrowError = false;
							}
							WildcardPattern wildcardPattern = new WildcardPattern(str1, WildcardOptions.IgnoreCase);
							if (!wildcardPattern.IsMatch(entry.Source))
							{
								num1++;
							}
							else
							{
								flag1 = true;
								break;
							}
						}
						if (!flag1)
						{
							return flag1;
						}
					}
					if (this._message != null)
					{
						if (WildcardPattern.ContainsWildcardCharacters(this._message))
						{
							this.isThrowError = false;
						}
						WildcardPattern wildcardPattern1 = new WildcardPattern(this._message, WildcardOptions.IgnoreCase);
						if (!wildcardPattern1.IsMatch(entry.Message))
						{
							return false;
						}
					}
					if (this._username != null)
					{
						bool flag2 = false;
						string[] strArrays2 = this._username;
						for (int i = 0; i < (int)strArrays2.Length; i++)
						{
							string str2 = strArrays2[i];
							this.isThrowError = false;
							if (entry.UserName != null)
							{
								WildcardPattern wildcardPattern2 = new WildcardPattern(str2, WildcardOptions.IgnoreCase);
								if (wildcardPattern2.IsMatch(entry.UserName))
								{
									flag2 = true;
									break;
								}
							}
						}
						if (!flag2)
						{
							return flag2;
						}
					}
					if (this.isDateSpecified)
					{
						this.isThrowError = false;
						bool flag3 = false;
						if (this._after.Equals(this.Initial) || !this._before.Equals(this.Initial))
						{
							if (this._before.Equals(this.Initial) || !this._after.Equals(this.Initial))
							{
								if (!this._after.Equals(this.Initial) && !this._before.Equals(this.Initial))
								{
									if (this._after > this._before || this._after == this._before)
									{
										if (entry.TimeGenerated > this._after || entry.TimeGenerated < this._before)
										{
											flag3 = true;
										}
									}
									else
									{
										if (entry.TimeGenerated > this._after && entry.TimeGenerated < this._before)
										{
											flag3 = true;
										}
									}
								}
							}
							else
							{
								if (entry.TimeGenerated < this._before)
								{
									flag3 = true;
								}
							}
						}
						else
						{
							if (entry.TimeGenerated > this._after)
							{
								flag3 = true;
							}
						}
						if (!flag3)
						{
							return flag3;
						}
					}
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		private List<EventLog> GetMatchingLogs(string pattern)
		{
			WildcardPattern wildcardPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
			List<EventLog> eventLogs = new List<EventLog>();
			if ((int)this._computerName.Length != 0)
			{
				string[] strArrays = this._computerName;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					EventLog[] eventLogArray = EventLog.GetEventLogs(str);
					for (int j = 0; j < (int)eventLogArray.Length; j++)
					{
						EventLog eventLog = eventLogArray[j];
						if (wildcardPattern.IsMatch(eventLog.Log))
						{
							eventLogs.Add(eventLog);
						}
					}
				}
			}
			else
			{
				EventLog[] eventLogs1 = EventLog.GetEventLogs();
				for (int k = 0; k < (int)eventLogs1.Length; k++)
				{
					EventLog eventLog1 = eventLogs1[k];
					if (wildcardPattern.IsMatch(eventLog1.Log))
					{
						eventLogs.Add(eventLog1);
					}
				}
			}
			return eventLogs;
		}

		private void OutputEvents(string logName)
		{
			bool flag = false;
			try
			{
				if ((int)this._computerName.Length != 0)
				{
					flag = true;
					string[] strArrays = this._computerName;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						using (EventLog eventLog = new EventLog(logName, str))
						{
							this.Process(eventLog);
						}
					}
				}
				else
				{
					using (EventLog eventLog1 = new EventLog(logName))
					{
						flag = true;
						this.Process(eventLog1);
					}
				}
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				if (!flag)
				{
					base.ThrowTerminatingError(new ErrorRecord(invalidOperationException, "EventLogNotFound", ErrorCategory.ObjectNotFound, logName));
				}
				else
				{
					throw;
				}
			}
		}

		private void Process(EventLog log)
		{
			bool flag = false;
			if (this._newest != 0)
			{
				EventLogEntryCollection entries = log.Entries;
				int count = entries.Count;
				int index = -2147483648;
				int num = 0;
				for (int i = count - 1; i >= 0 && num < this._newest; i--)
				{
					EventLogEntry item = null;
					try
					{
						item = entries[i];
					}
					catch (ArgumentException argumentException1)
					{
						ArgumentException argumentException = argumentException1;
						ErrorRecord errorRecord = new ErrorRecord(argumentException, "LogReadError", ErrorCategory.ReadError, null);
						object[] message = new object[2];
						message[0] = log.Log;
						message[1] = argumentException.Message;
						errorRecord.ErrorDetails = new ErrorDetails(this, "EventlogResources", "LogReadError", message);
						base.WriteError(errorRecord);
						break;
					}
					catch (Exception exception)
					{
						throw;
					}
					if (item != null && (index == -2147483648 || index - item.Index == 1))
					{
						index = item.Index;
						if (!this.isFilterSpecified || this.FiltersMatch(item))
						{
							if (this._asbaseobject)
							{
								base.WriteObject(item);
								flag = true;
							}
							else
							{
								PSObject pSObject = new PSObject(item);
								object[] immediateBaseObject = new object[5];
								immediateBaseObject[0] = pSObject.ImmediateBaseObject;
								immediateBaseObject[1] = "#";
								immediateBaseObject[2] = log.Log;
								immediateBaseObject[3] = "/";
								immediateBaseObject[4] = item.Source;
								pSObject.TypeNames.Insert(0, string.Concat(immediateBaseObject));
								object[] source = new object[7];
								source[0] = pSObject.ImmediateBaseObject;
								source[1] = "#";
								source[2] = log.Log;
								source[3] = "/";
								source[4] = item.Source;
								source[5] = "/";
								source[6] = item.InstanceId;
								pSObject.TypeNames.Insert(0, string.Concat(source));
								base.WriteObject(pSObject);
								flag = true;
							}
							num++;
						}
					}
				}
				if (!flag && this.isThrowError)
				{
					Exception exception1 = new ArgumentException(StringUtil.Format(EventlogResources.NoEntriesFound, log.Log, ""));
					base.WriteError(new ErrorRecord(exception1, "GetEventLogNoEntriesFound", ErrorCategory.ObjectNotFound, null));
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}