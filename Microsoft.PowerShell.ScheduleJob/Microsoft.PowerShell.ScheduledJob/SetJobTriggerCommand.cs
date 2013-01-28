using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Set", "JobTrigger", DefaultParameterSetName="DefaultParams", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223916")]
	[OutputType(new Type[] { typeof(ScheduledJobTrigger) })]
	public sealed class SetJobTriggerCommand : ScheduleJobCmdletBase
	{
		private const string DefaultParameterSet = "DefaultParams";

		private ScheduledJobTrigger[] _triggers;

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

		private SwitchParameter _passThru;

		private string _paramAtStartup;

		private string _paramAtLogon;

		private string _paramOnce;

		private string _paramDaily;

		private string _paramWeekly;

		private string _paramDaysInterval;

		private string _paramWeeksInterval;

		private string _paramRandomDelay;

		private string _paramRepetitionInterval;

		private string _paramRepetitionDuration;

		private string _paramAt;

		private string _paramUser;

		private string _paramDaysOfWeek;

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="DefaultParams")]
		[ValidateNotNullOrEmpty]
		public ScheduledJobTrigger[] InputObject
		{
			get
			{
				return this._triggers;
			}
			set
			{
				this._triggers = value;
			}
		}

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
		public SwitchParameter PassThru
		{
			get
			{
				return this._passThru;
			}
			set
			{
				this._passThru = value;
			}
		}

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		[Parameter(ParameterSetName="DefaultParams")]
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

		public SetJobTriggerCommand()
		{
			this._daysInterval = 1;
			this._weeksInterval = 1;
			this._paramAtStartup = "AtStartup";
			this._paramAtLogon = "AtLogon";
			this._paramOnce = "Once";
			this._paramDaily = "Daily";
			this._paramWeekly = "Weekly";
			this._paramDaysInterval = "DaysInterval";
			this._paramWeeksInterval = "WeeksInterval";
			this._paramRandomDelay = "RandomDelay";
			this._paramRepetitionInterval = "RepetitionInterval";
			this._paramRepetitionDuration = "RepetitionDuration";
			this._paramAt = "At";
			this._paramUser = "User";
			this._paramDaysOfWeek = "DaysOfWeek";
		}

		private void CreateAtLogonTrigger(ScheduledJobTrigger trigger)
		{
			string allUsers;
			TimeSpan timeSpan;
			string str;
			bool enabled = trigger.Enabled;
			int id = trigger.Id;
			TimeSpan randomDelay = trigger.RandomDelay;
			if (string.IsNullOrEmpty(trigger.User))
			{
				allUsers = ScheduledJobTrigger.AllUsers;
			}
			else
			{
				allUsers = trigger.User;
			}
			string str1 = allUsers;
			trigger.ClearProperties();
			trigger.Frequency = TriggerFrequency.AtLogon;
			trigger.Enabled = enabled;
			trigger.Id = id;
			ScheduledJobTrigger scheduledJobTrigger = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				timeSpan = this._randomDelay;
			}
			else
			{
				timeSpan = randomDelay;
			}
			scheduledJobTrigger.RandomDelay = timeSpan;
			ScheduledJobTrigger scheduledJobTrigger1 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramUser))
			{
				str = this._user;
			}
			else
			{
				str = str1;
			}
			scheduledJobTrigger1.User = str;
		}

		private void CreateAtStartupTrigger(ScheduledJobTrigger trigger)
		{
			TimeSpan timeSpan;
			bool enabled = trigger.Enabled;
			int id = trigger.Id;
			TimeSpan randomDelay = trigger.RandomDelay;
			trigger.ClearProperties();
			trigger.Frequency = TriggerFrequency.AtStartup;
			trigger.Enabled = enabled;
			trigger.Id = id;
			ScheduledJobTrigger scheduledJobTrigger = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				timeSpan = this._randomDelay;
			}
			else
			{
				timeSpan = randomDelay;
			}
			scheduledJobTrigger.RandomDelay = timeSpan;
		}

		private void CreateDailyTrigger(ScheduledJobTrigger trigger)
		{
			TimeSpan timeSpan;
			DateTime? nullable;
			int num;
			bool enabled = trigger.Enabled;
			int id = trigger.Id;
			TimeSpan randomDelay = trigger.RandomDelay;
			DateTime? at = trigger.At;
			int interval = trigger.Interval;
			trigger.ClearProperties();
			trigger.Frequency = TriggerFrequency.Daily;
			trigger.Enabled = enabled;
			trigger.Id = id;
			ScheduledJobTrigger scheduledJobTrigger = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				timeSpan = this._randomDelay;
			}
			else
			{
				timeSpan = randomDelay;
			}
			scheduledJobTrigger.RandomDelay = timeSpan;
			ScheduledJobTrigger scheduledJobTrigger1 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
			{
				nullable = new DateTime?(this._atTime);
			}
			else
			{
				nullable = at;
			}
			scheduledJobTrigger1.At = nullable;
			ScheduledJobTrigger scheduledJobTrigger2 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval))
			{
				num = this._daysInterval;
			}
			else
			{
				num = interval;
			}
			scheduledJobTrigger2.Interval = num;
		}

		private void CreateOnceTrigger(ScheduledJobTrigger trigger)
		{
			TimeSpan timeSpan;
			DateTime? nullable;
			TimeSpan? nullable1;
			TimeSpan? nullable2;
			bool enabled = trigger.Enabled;
			int id = trigger.Id;
			TimeSpan randomDelay = trigger.RandomDelay;
			DateTime? at = trigger.At;
			TimeSpan? repetitionInterval = trigger.RepetitionInterval;
			TimeSpan? repetitionDuration = trigger.RepetitionDuration;
			trigger.ClearProperties();
			trigger.Frequency = TriggerFrequency.Once;
			trigger.Enabled = enabled;
			trigger.Id = id;
			ScheduledJobTrigger scheduledJobTrigger = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				timeSpan = this._randomDelay;
			}
			else
			{
				timeSpan = randomDelay;
			}
			scheduledJobTrigger.RandomDelay = timeSpan;
			ScheduledJobTrigger scheduledJobTrigger1 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
			{
				nullable = new DateTime?(this._atTime);
			}
			else
			{
				nullable = at;
			}
			scheduledJobTrigger1.At = nullable;
			ScheduledJobTrigger scheduledJobTrigger2 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval))
			{
				nullable1 = new TimeSpan?(this._repInterval);
			}
			else
			{
				nullable1 = repetitionInterval;
			}
			scheduledJobTrigger2.RepetitionInterval = nullable1;
			ScheduledJobTrigger scheduledJobTrigger3 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
			{
				nullable2 = new TimeSpan?(this._repDuration);
			}
			else
			{
				nullable2 = repetitionDuration;
			}
			scheduledJobTrigger3.RepetitionDuration = nullable2;
		}

		private bool CreateTrigger(ScheduledJobTrigger trigger, TriggerFrequency triggerFrequency)
		{
			TriggerFrequency triggerFrequency1 = triggerFrequency;
			switch (triggerFrequency1)
			{
				case TriggerFrequency.Once:
				{
					if (trigger.Frequency == triggerFrequency || this.ValidateOnceParams(trigger))
					{
						this.CreateOnceTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.Daily:
				{
					if (trigger.Frequency == triggerFrequency || this.ValidateDailyParams(trigger))
					{
						this.CreateDailyTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.Weekly:
				{
					if (trigger.Frequency == triggerFrequency || this.ValidateWeeklyParams(trigger))
					{
						this.CreateWeeklyTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.AtLogon:
				{
					this.CreateAtLogonTrigger(trigger);
					break;
				}
				case TriggerFrequency.AtStartup:
				{
					this.CreateAtStartupTrigger(trigger);
					break;
				}
			}
			return true;
		}

		private void CreateWeeklyTrigger(ScheduledJobTrigger trigger)
		{
			TimeSpan timeSpan;
			DateTime? nullable;
			int num;
			List<DayOfWeek> dayOfWeeks;
			bool enabled = trigger.Enabled;
			int id = trigger.Id;
			TimeSpan randomDelay = trigger.RandomDelay;
			DateTime? at = trigger.At;
			int interval = trigger.Interval;
			List<DayOfWeek> daysOfWeek = trigger.DaysOfWeek;
			trigger.ClearProperties();
			trigger.Frequency = TriggerFrequency.Weekly;
			trigger.Enabled = enabled;
			trigger.Id = id;
			ScheduledJobTrigger scheduledJobTrigger = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				timeSpan = this._randomDelay;
			}
			else
			{
				timeSpan = randomDelay;
			}
			scheduledJobTrigger.RandomDelay = timeSpan;
			ScheduledJobTrigger scheduledJobTrigger1 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
			{
				nullable = new DateTime?(this._atTime);
			}
			else
			{
				nullable = at;
			}
			scheduledJobTrigger1.At = nullable;
			ScheduledJobTrigger scheduledJobTrigger2 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval))
			{
				num = this._weeksInterval;
			}
			else
			{
				num = interval;
			}
			scheduledJobTrigger2.Interval = num;
			ScheduledJobTrigger scheduledJobTrigger3 = trigger;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
			{
				dayOfWeeks = new List<DayOfWeek>(this._daysOfWeek);
			}
			else
			{
				dayOfWeeks = daysOfWeek;
			}
			scheduledJobTrigger3.DaysOfWeek = dayOfWeeks;
		}

		private void ModifyDailyTrigger(ScheduledJobTrigger trigger)
		{
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				trigger.RandomDelay = this._randomDelay;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
			{
				trigger.At = new DateTime?(this._atTime);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval))
			{
				trigger.Interval = this._daysInterval;
			}
		}

		private void ModifyLogonTrigger(ScheduledJobTrigger trigger)
		{
			string allUsers;
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				trigger.RandomDelay = this._randomDelay;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramUser))
			{
				ScheduledJobTrigger scheduledJobTrigger = trigger;
				if (string.IsNullOrEmpty(this._user))
				{
					allUsers = ScheduledJobTrigger.AllUsers;
				}
				else
				{
					allUsers = this._user;
				}
				scheduledJobTrigger.User = allUsers;
			}
		}

		private void ModifyOnceTrigger(ScheduledJobTrigger trigger)
		{
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				trigger.RandomDelay = this._randomDelay;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval))
			{
				trigger.RepetitionInterval = new TimeSpan?(this._repInterval);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
			{
				trigger.RepetitionDuration = new TimeSpan?(this._repDuration);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
			{
				trigger.At = new DateTime?(this._atTime);
			}
		}

		private void ModifyStartupTrigger(ScheduledJobTrigger trigger)
		{
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				trigger.RandomDelay = this._randomDelay;
			}
		}

		private bool ModifyTrigger(ScheduledJobTrigger trigger, TriggerFrequency triggerFrequency, bool validate = false)
		{
			TriggerFrequency triggerFrequency1 = triggerFrequency;
			switch (triggerFrequency1)
			{
				case TriggerFrequency.Once:
				{
					if (!validate || this.ValidateOnceParams(null))
					{
						this.ModifyOnceTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.Daily:
				{
					if (!validate || this.ValidateDailyParams(null))
					{
						this.ModifyDailyTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.Weekly:
				{
					if (!validate || this.ValidateWeeklyParams(null))
					{
						this.ModifyWeeklyTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.AtLogon:
				{
					if (!validate || this.ValidateLogonParams())
					{
						this.ModifyLogonTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
				case TriggerFrequency.AtStartup:
				{
					if (!validate || this.ValidateStartupParams())
					{
						this.ModifyStartupTrigger(trigger);
						break;
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

		private void ModifyWeeklyTrigger(ScheduledJobTrigger trigger)
		{
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRandomDelay))
			{
				trigger.RandomDelay = this._randomDelay;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
			{
				trigger.At = new DateTime?(this._atTime);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval))
			{
				trigger.Interval = this._weeksInterval;
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
			{
				trigger.DaysOfWeek = new List<DayOfWeek>(this._daysOfWeek);
			}
		}

		protected override void ProcessRecord()
		{
			TriggerFrequency triggerFrequency = TriggerFrequency.None;
			if (this.ValidateParameterSet(ref triggerFrequency))
			{
				ScheduledJobTrigger[] scheduledJobTriggerArray = this._triggers;
				for (int i = 0; i < (int)scheduledJobTriggerArray.Length; i++)
				{
					ScheduledJobTrigger scheduledJobTrigger = scheduledJobTriggerArray[i];
					ScheduledJobTrigger scheduledJobTrigger1 = new ScheduledJobTrigger(scheduledJobTrigger);
					if (this.UpdateTrigger(scheduledJobTrigger, triggerFrequency))
					{
						ScheduledJobDefinition jobDefinition = scheduledJobTrigger.JobDefinition;
						if (jobDefinition != null)
						{
							bool flag = false;
							try
							{
								scheduledJobTrigger.UpdateJobDefinition();
							}
							catch (ScheduledJobException scheduledJobException1)
							{
								ScheduledJobException scheduledJobException = scheduledJobException1;
								flag = true;
								object[] name = new object[2];
								name[0] = jobDefinition.Name;
								name[1] = scheduledJobTrigger.Id;
								string str = StringUtil.Format(ScheduledJobErrorStrings.CantUpdateTriggerOnJobDef, name);
								Exception runtimeException = new RuntimeException(str, scheduledJobException);
								ErrorRecord errorRecord = new ErrorRecord(runtimeException, "CantSetPropertiesOnJobTrigger", ErrorCategory.InvalidOperation, scheduledJobTrigger);
								base.WriteError(errorRecord);
							}
							if (flag)
							{
								scheduledJobTrigger1.CopyTo(scheduledJobTrigger);
							}
						}
						if (this._passThru)
						{
							base.WriteObject(scheduledJobTrigger);
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private bool UpdateTrigger(ScheduledJobTrigger trigger, TriggerFrequency triggerFrequency)
		{
			if (triggerFrequency == TriggerFrequency.None)
			{
				return this.ModifyTrigger(trigger, trigger.Frequency, true);
			}
			else
			{
				if (triggerFrequency == trigger.Frequency)
				{
					return this.ModifyTrigger(trigger, triggerFrequency, false);
				}
				else
				{
					return this.CreateTrigger(trigger, triggerFrequency);
				}
			}
		}

		private bool ValidateDailyParams(ScheduledJobTrigger trigger = null)
		{
			if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval) || this._daysInterval >= 1)
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval))
				{
					if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramUser))
					{
						if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
						{
							if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval) || base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
							{
								string str = StringUtil.Format(ScheduledJobErrorStrings.InvalidSetTriggerRepetition, ScheduledJobErrorStrings.TriggerDailyType);
								this.WriteValidationError(str);
								return false;
							}
							else
							{
								if (trigger != null)
								{
									DateTime? at = trigger.At;
									if (!at.HasValue && !base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
									{
										string str1 = StringUtil.Format(ScheduledJobErrorStrings.MissingAtTime, ScheduledJobErrorStrings.TriggerDailyType);
										this.WriteValidationError(str1);
										return false;
									}
								}
								return true;
							}
						}
						else
						{
							string str2 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysOfWeek, ScheduledJobErrorStrings.TriggerDailyType);
							this.WriteValidationError(str2);
							return false;
						}
					}
					else
					{
						string str3 = StringUtil.Format(ScheduledJobErrorStrings.InvalidUser, ScheduledJobErrorStrings.TriggerDailyType);
						this.WriteValidationError(str3);
						return false;
					}
				}
				else
				{
					string str4 = StringUtil.Format(ScheduledJobErrorStrings.InvalidWeeksInterval, ScheduledJobErrorStrings.TriggerDailyType);
					this.WriteValidationError(str4);
					return false;
				}
			}
			else
			{
				this.WriteValidationError(ScheduledJobErrorStrings.InvalidDaysIntervalParam);
				return false;
			}
		}

		private bool ValidateLogonParams()
		{
			if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval))
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval))
				{
					if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
					{
						if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
						{
							if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval) || base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
							{
								string str = StringUtil.Format(ScheduledJobErrorStrings.InvalidSetTriggerRepetition, ScheduledJobErrorStrings.TriggerLogonType);
								this.WriteValidationError(str);
								return false;
							}
							else
							{
								return true;
							}
						}
						else
						{
							string str1 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysOfWeek, ScheduledJobErrorStrings.TriggerLogonType);
							this.WriteValidationError(str1);
							return false;
						}
					}
					else
					{
						string str2 = StringUtil.Format(ScheduledJobErrorStrings.InvalidAtTime, ScheduledJobErrorStrings.TriggerLogonType);
						this.WriteValidationError(str2);
						return false;
					}
				}
				else
				{
					string str3 = StringUtil.Format(ScheduledJobErrorStrings.InvalidWeeksInterval, ScheduledJobErrorStrings.TriggerLogonType);
					this.WriteValidationError(str3);
					return false;
				}
			}
			else
			{
				string str4 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysInterval, ScheduledJobErrorStrings.TriggerLogonType);
				this.WriteValidationError(str4);
				return false;
			}
		}

		private bool ValidateOnceParams(ScheduledJobTrigger trigger = null)
		{
			string str;
			bool flag;
			DateTime? at;
			if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval))
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval))
				{
					if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramUser))
					{
						if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
						{
							if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval) || base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
							{
								try
								{
									ScheduledJobTrigger.ValidateOnceRepetitionParams(new TimeSpan?(this._repInterval), new TimeSpan?(this._repDuration));
									if (trigger != null)
									{
										at = trigger.At;
										if (!at.HasValue && !base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
										{
											str = StringUtil.Format(ScheduledJobErrorStrings.MissingAtTime, ScheduledJobErrorStrings.TriggerOnceType);
											this.WriteValidationError(str);
											return false;
										}
									}
									return true;
								}
								catch (PSArgumentException pSArgumentException1)
								{
									PSArgumentException pSArgumentException = pSArgumentException1;
									this.WriteValidationError(pSArgumentException.Message);
									flag = false;
								}
								return flag;
							}
							if (trigger != null)
							{
								at = trigger.At;
								if (!at.HasValue && !base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
								{
									str = StringUtil.Format(ScheduledJobErrorStrings.MissingAtTime, ScheduledJobErrorStrings.TriggerOnceType);
									this.WriteValidationError(str);
									return false;
								}
							}
							return true;
						}
						else
						{
							string str1 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysOfWeek, ScheduledJobErrorStrings.TriggerOnceType);
							this.WriteValidationError(str1);
							return false;
						}
					}
					else
					{
						string str2 = StringUtil.Format(ScheduledJobErrorStrings.InvalidUser, ScheduledJobErrorStrings.TriggerOnceType);
						this.WriteValidationError(str2);
						return false;
					}
				}
				else
				{
					string str3 = StringUtil.Format(ScheduledJobErrorStrings.InvalidWeeksInterval, ScheduledJobErrorStrings.TriggerOnceType);
					this.WriteValidationError(str3);
					return false;
				}
			}
			else
			{
				string str4 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysInterval, ScheduledJobErrorStrings.TriggerOnceType);
				this.WriteValidationError(str4);
				return false;
			}
		}

		private bool ValidateParameterSet(ref TriggerFrequency newTriggerFrequency)
		{
			TriggerFrequency item;
			List<TriggerFrequency> triggerFrequencies = new List<TriggerFrequency>();
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAtStartup))
			{
				triggerFrequencies.Add(TriggerFrequency.AtStartup);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramAtLogon))
			{
				triggerFrequencies.Add(TriggerFrequency.AtLogon);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramOnce))
			{
				triggerFrequencies.Add(TriggerFrequency.Once);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramDaily))
			{
				triggerFrequencies.Add(TriggerFrequency.Daily);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey(this._paramWeekly))
			{
				triggerFrequencies.Add(TriggerFrequency.Weekly);
			}
			if (triggerFrequencies.Count <= 1)
			{
				TriggerFrequency triggerFrequencyPointer = (TriggerFrequency)newTriggerFrequency;
				if (triggerFrequencies.Count == 1)
				{
					item = triggerFrequencies[0];
				}
				else
				{
					item = TriggerFrequency.None;
				}
				triggerFrequencyPointer = item;
				bool flag = false;
				TriggerFrequency triggerFrequency = (TriggerFrequency)((int)newTriggerFrequency);
				switch (triggerFrequency)
				{
					case TriggerFrequency.None:
					{
						flag = true;
						break;
					}
					case TriggerFrequency.Once:
					{
						flag = this.ValidateOnceParams(null);
						break;
					}
					case TriggerFrequency.Daily:
					{
						flag = this.ValidateDailyParams(null);
						break;
					}
					case TriggerFrequency.Weekly:
					{
						flag = this.ValidateWeeklyParams(null);
						break;
					}
					case TriggerFrequency.AtLogon:
					{
						flag = this.ValidateLogonParams();
						break;
					}
					case TriggerFrequency.AtStartup:
					{
						flag = this.ValidateStartupParams();
						break;
					}
					default:
					{
						flag = false;
						break;
					}
				}
				return flag;
			}
			else
			{
				this.WriteValidationError(ScheduledJobErrorStrings.ConflictingTypeParams);
				return false;
			}
		}

		private bool ValidateStartupParams()
		{
			if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval))
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval))
				{
					if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
					{
						if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramUser))
						{
							if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
							{
								if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval) || base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
								{
									string str = StringUtil.Format(ScheduledJobErrorStrings.InvalidSetTriggerRepetition, ScheduledJobErrorStrings.TriggerStartUpType);
									this.WriteValidationError(str);
									return false;
								}
								else
								{
									return true;
								}
							}
							else
							{
								string str1 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysOfWeek, ScheduledJobErrorStrings.TriggerStartUpType);
								this.WriteValidationError(str1);
								return false;
							}
						}
						else
						{
							string str2 = StringUtil.Format(ScheduledJobErrorStrings.InvalidUser, ScheduledJobErrorStrings.TriggerStartUpType);
							this.WriteValidationError(str2);
							return false;
						}
					}
					else
					{
						string str3 = StringUtil.Format(ScheduledJobErrorStrings.InvalidAtTime, ScheduledJobErrorStrings.TriggerStartUpType);
						this.WriteValidationError(str3);
						return false;
					}
				}
				else
				{
					string str4 = StringUtil.Format(ScheduledJobErrorStrings.InvalidWeeksInterval, ScheduledJobErrorStrings.TriggerStartUpType);
					this.WriteValidationError(str4);
					return false;
				}
			}
			else
			{
				string str5 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysInterval, ScheduledJobErrorStrings.TriggerStartUpType);
				this.WriteValidationError(str5);
				return false;
			}
		}

		private bool ValidateWeeklyParams(ScheduledJobTrigger trigger = null)
		{
			if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysInterval))
			{
				if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramWeeksInterval) || this._weeksInterval >= 1)
				{
					if (!base.MyInvocation.BoundParameters.ContainsKey(this._paramUser))
					{
						if (base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionInterval) || base.MyInvocation.BoundParameters.ContainsKey(this._paramRepetitionDuration))
						{
							string str = StringUtil.Format(ScheduledJobErrorStrings.InvalidSetTriggerRepetition, ScheduledJobErrorStrings.TriggerWeeklyType);
							this.WriteValidationError(str);
							return false;
						}
						else
						{
							if (trigger != null)
							{
								DateTime? at = trigger.At;
								if (at.HasValue || base.MyInvocation.BoundParameters.ContainsKey(this._paramAt))
								{
									if ((trigger.DaysOfWeek == null || trigger.DaysOfWeek.Count == 0) && !base.MyInvocation.BoundParameters.ContainsKey(this._paramDaysOfWeek))
									{
										string str1 = StringUtil.Format(ScheduledJobErrorStrings.MissingDaysOfWeek, ScheduledJobErrorStrings.TriggerDailyType);
										this.WriteValidationError(str1);
										return false;
									}
								}
								else
								{
									string str2 = StringUtil.Format(ScheduledJobErrorStrings.MissingAtTime, ScheduledJobErrorStrings.TriggerDailyType);
									this.WriteValidationError(str2);
									return false;
								}
							}
							return true;
						}
					}
					else
					{
						string str3 = StringUtil.Format(ScheduledJobErrorStrings.InvalidUser, ScheduledJobErrorStrings.TriggerWeeklyType);
						this.WriteValidationError(str3);
						return false;
					}
				}
				else
				{
					this.WriteValidationError(ScheduledJobErrorStrings.InvalidWeeksIntervalParam);
					return false;
				}
			}
			else
			{
				string str4 = StringUtil.Format(ScheduledJobErrorStrings.InvalidDaysInterval, ScheduledJobErrorStrings.TriggerWeeklyType);
				this.WriteValidationError(str4);
				return false;
			}
		}

		private void WriteValidationError(string msg)
		{
			Exception runtimeException = new RuntimeException(msg);
			ErrorRecord errorRecord = new ErrorRecord(runtimeException, "SetJobTriggerParameterValidationError", ErrorCategory.InvalidArgument, null);
			base.WriteError(errorRecord);
		}
	}
}