using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("New", "JobTrigger", DefaultParameterSetName="Once", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223912")]
	[OutputType(new Type[] { typeof(ScheduledJobTrigger) })]
	public sealed class NewJobTriggerCommand : ScheduleJobCmdletBase
	{
		private const string AtLogonParameterSet = "AtLogon";

		private const string AtStartupParameterSet = "AtStartup";

		private const string OnceParameterSet = "Once";

		private const string DailyParameterSet = "Daily";

		private const string WeeklyParameterSet = "Weekly";

		private int _daysInterval;

		private int _weeksInterval;

		private TimeSpan _randomDelay;

		private DateTime _atTime;

		private string _user;

		private DayOfWeek[] _daysOfWeek;

		private SwitchParameter _atStartup;

		private SwitchParameter _atLogon;

		private SwitchParameter _once;

		private TimeSpan _repInterval;

		private TimeSpan _repDuration;

		private SwitchParameter _daily;

		private SwitchParameter _weekly;

		[Parameter(Mandatory=true, ParameterSetName="Once")]
		[Parameter(Mandatory=true, ParameterSetName="Weekly")]
		[Parameter(Mandatory=true, ParameterSetName="Daily")]
		public DateTime At
		{
			get
			{
				return this._atTime;
			}
			set
			{
				this._atTime = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="AtLogon")]
		public SwitchParameter AtLogOn
		{
			get
			{
				return this._atLogon;
			}
			set
			{
				this._atLogon = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="AtStartup")]
		public SwitchParameter AtStartup
		{
			get
			{
				return this._atStartup;
			}
			set
			{
				this._atStartup = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="Daily")]
		public SwitchParameter Daily
		{
			get
			{
				return this._daily;
			}
			set
			{
				this._daily = value;
			}
		}

		[Parameter(ParameterSetName="Daily")]
		public int DaysInterval
		{
			get
			{
				return this._daysInterval;
			}
			set
			{
				this._daysInterval = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="Weekly")]
		[ValidateNotNullOrEmpty]
		public DayOfWeek[] DaysOfWeek
		{
			get
			{
				return this._daysOfWeek;
			}
			set
			{
				this._daysOfWeek = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="Once")]
		public SwitchParameter Once
		{
			get
			{
				return this._once;
			}
			set
			{
				this._once = value;
			}
		}

		[Parameter(ParameterSetName="Daily")]
		[Parameter(ParameterSetName="Once")]
		[Parameter(ParameterSetName="AtLogon")]
		[Parameter(ParameterSetName="AtStartup")]
		[Parameter(ParameterSetName="Weekly")]
		public TimeSpan RandomDelay
		{
			get
			{
				return this._randomDelay;
			}
			set
			{
				this._randomDelay = value;
			}
		}

		[Parameter(ParameterSetName="Once")]
		public TimeSpan RepetitionDuration
		{
			get
			{
				return this._repDuration;
			}
			set
			{
				this._repDuration = value;
			}
		}

		[Parameter(ParameterSetName="Once")]
		public TimeSpan RepetitionInterval
		{
			get
			{
				return this._repInterval;
			}
			set
			{
				this._repInterval = value;
			}
		}

		[Parameter(ParameterSetName="AtLogon")]
		[ValidateNotNullOrEmpty]
		public string User
		{
			get
			{
				return this._user;
			}
			set
			{
				this._user = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ParameterSetName="Weekly")]
		public SwitchParameter Weekly
		{
			get
			{
				return this._weekly;
			}
			set
			{
				this._weekly = value;
			}
		}

		[Parameter(ParameterSetName="Weekly")]
		public int WeeksInterval
		{
			get
			{
				return this._weeksInterval;
			}
			set
			{
				this._weeksInterval = value;
			}
		}

		public NewJobTriggerCommand()
		{
			this._daysInterval = 1;
			this._weeksInterval = 1;
		}

		protected override void BeginProcessing()
		{
			base.BeginProcessing();
			if (this._daysInterval >= 1)
			{
				if (this._weeksInterval >= 1)
				{
					return;
				}
				else
				{
					throw new PSArgumentException(ScheduledJobErrorStrings.InvalidWeeksIntervalParam);
				}
			}
			else
			{
				throw new PSArgumentException(ScheduledJobErrorStrings.InvalidDaysIntervalParam);
			}
		}

		private void CreateAtLogonTrigger()
		{
			base.WriteObject(ScheduledJobTrigger.CreateAtLogOnTrigger(this._user, this._randomDelay, 0, true));
		}

		private void CreateAtStartupTrigger()
		{
			base.WriteObject(ScheduledJobTrigger.CreateAtStartupTrigger(this._randomDelay, 0, true));
		}

		private void CreateDailyTrigger()
		{
			base.WriteObject(ScheduledJobTrigger.CreateDailyTrigger(this._atTime, this._daysInterval, this._randomDelay, 0, true));
		}

		private void CreateOnceTrigger()
		{
			TimeSpan? nullable = null;
			TimeSpan? nullable1 = null;
			if (base.MyInvocation.BoundParameters.ContainsKey("RepetitionInterval") || base.MyInvocation.BoundParameters.ContainsKey("RepetitionDuration"))
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey("RepetitionInterval") || !base.MyInvocation.BoundParameters.ContainsKey("RepetitionDuration"))
				{
					throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionParams);
				}
				else
				{
					if (this._repInterval < TimeSpan.Zero || this._repDuration < TimeSpan.Zero)
					{
						throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionParamValues);
					}
					else
					{
						if (this._repInterval >= TimeSpan.FromMinutes(1))
						{
							if (this._repInterval <= this._repDuration)
							{
								nullable = new TimeSpan?(this._repInterval);
								nullable1 = new TimeSpan?(this._repDuration);
							}
							else
							{
								throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionInterval);
							}
						}
						else
						{
							throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionIntervalValue);
						}
					}
				}
			}
			base.WriteObject(ScheduledJobTrigger.CreateOnceTrigger(this._atTime, this._randomDelay, nullable, nullable1, 0, true));
		}

		private void CreateWeeklyTrigger()
		{
			base.WriteObject(ScheduledJobTrigger.CreateWeeklyTrigger(this._atTime, this._weeksInterval, this._daysOfWeek, this._randomDelay, 0, true));
		}

		protected override void ProcessRecord()
		{
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "AtLogon")
				{
					this.CreateAtLogonTrigger();
					return;
				}
				else
				{
					if (str == "AtStartup")
					{
						this.CreateAtStartupTrigger();
						return;
					}
					else
					{
						if (str == "Once")
						{
							this.CreateOnceTrigger();
							return;
						}
						else
						{
							if (str == "Daily")
							{
								this.CreateDailyTrigger();
								return;
							}
							else
							{
								if (str == "Weekly")
								{
									this.CreateWeeklyTrigger();
								}
								else
								{
									return;
								}
							}
						}
					}
				}
			}
		}
	}
}