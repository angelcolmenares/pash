using Microsoft.PowerShell.Commands.Management;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Limit", "EventLog", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135227", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public sealed class LimitEventLogCommand : PSCmdlet
	{
		private string[] _logName;

		private string[] _computerName;

		private int _retention;

		private bool retentionSpecified;

		private OverflowAction _overflowaction;

		private bool overflowSpecified;

		private long _maximumKilobytes;

		private bool maxkbSpecified;

		[Alias(new string[] { "CN" })]
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

		[Alias(new string[] { "LN" })]
		[Parameter(Position=0, Mandatory=true)]
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

		[Parameter]
		[ValidateNotNullOrEmpty]
		public long MaximumSize
		{
			get
			{
				return this._maximumKilobytes;
			}
			set
			{
				this._maximumKilobytes = value;
				this.maxkbSpecified = true;
			}
		}

		[Alias(new string[] { "OFA" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "OverwriteOlder", "OverwriteAsNeeded", "DoNotOverwrite" })]
		public OverflowAction OverflowAction
		{
			get
			{
				return this._overflowaction;
			}
			set
			{
				this._overflowaction = value;
				this.overflowSpecified = true;
			}
		}

		[Alias(new string[] { "MRD" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		[ValidateRange(1, 0x16d)]
		public int RetentionDays
		{
			get
			{
				return this._retention;
			}
			set
			{
				this._retention = value;
				this.retentionSpecified = true;
			}
		}

		public LimitEventLogCommand()
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
							if (base.ShouldProcess(StringUtil.Format(EventlogResources.LimitEventLogWarning, str2, str)))
							{
								EventLog eventLog = new EventLog(str2, str1);
								int minimumRetentionDays = eventLog.MinimumRetentionDays;
								OverflowAction overflowAction = eventLog.OverflowAction;
								if (!this.retentionSpecified || !this.overflowSpecified)
								{
									if (!this.retentionSpecified || this.overflowSpecified)
									{
										if (!this.retentionSpecified && this.overflowSpecified)
										{
											eventLog.ModifyOverflowPolicy(this._overflowaction, minimumRetentionDays);
										}
									}
									else
									{
										if (overflowAction.CompareTo(OverflowAction.OverwriteOlder) != 0)
										{
											ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.InvalidOverflowAction, new object[0])), null, ErrorCategory.InvalidOperation, null);
											base.WriteError(errorRecord);
											goto Label0;
										}
										else
										{
											eventLog.ModifyOverflowPolicy(overflowAction, this._retention);
										}
									}
								}
								else
								{
									if (this._overflowaction.CompareTo(OverflowAction.OverwriteOlder) != 0)
									{
										ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.InvalidOverflowAction, new object[0])), null, ErrorCategory.InvalidOperation, null);
										base.WriteError(errorRecord1);
										goto Label0;
									}
									else
									{
										eventLog.ModifyOverflowPolicy(this._overflowaction, this._retention);
									}
								}
								if (this.maxkbSpecified)
								{
									int num = 0x400;
									this._maximumKilobytes = this._maximumKilobytes / (long)num;
									eventLog.MaximumKilobytes = this._maximumKilobytes;
								}
							}
						}
						else
						{
							ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, str2, str)), null, ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord2);
						}
					}
					catch (InvalidOperationException invalidOperationException1)
					{
						InvalidOperationException invalidOperationException = invalidOperationException1;
						this.WriteNonTerminatingError(invalidOperationException, EventlogResources.PermissionDenied, "PermissionDenied", ErrorCategory.PermissionDenied, str2, str);
					}
					catch (IOException oException1)
					{
						IOException oException = oException1;
						this.WriteNonTerminatingError(oException, EventlogResources.PathDoesNotExist, "PathDoesNotExist", ErrorCategory.InvalidOperation, null, str);
					}
					catch (ArgumentOutOfRangeException argumentOutOfRangeException1)
					{
						ArgumentOutOfRangeException argumentOutOfRangeException = argumentOutOfRangeException1;
						if (this.retentionSpecified || this.maxkbSpecified)
						{
							this.WriteNonTerminatingError(argumentOutOfRangeException, EventlogResources.ValueOutofRange, "ValueOutofRange", ErrorCategory.InvalidData, null, null);
						}
						else
						{
							this.WriteNonTerminatingError(argumentOutOfRangeException, EventlogResources.InvalidArgument, "InvalidArgument", ErrorCategory.InvalidData, null, null);
						}
					}
                Label0:
                    continue;
				}
			}
		}

		private void WriteNonTerminatingError(Exception exception, string resourceId, string errorId, ErrorCategory category, string _logName, string _compName)
		{
			Exception exception1 = new Exception(StringUtil.Format(resourceId, _logName, _compName), exception);
			base.WriteError(new ErrorRecord(exception1, errorId, category, null));
		}
	}
}