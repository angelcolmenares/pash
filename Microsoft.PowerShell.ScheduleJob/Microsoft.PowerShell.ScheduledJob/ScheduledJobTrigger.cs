using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Serializable]
	public sealed class ScheduledJobTrigger : ISerializable
	{
		private DateTime? _time;

		private List<DayOfWeek> _daysOfWeek;

		private TimeSpan _randomDelay;

		private int _interval;

		private string _user;

		private TriggerFrequency _frequency;

		private TimeSpan? _repInterval;

		private TimeSpan? _repDuration;

		private int _id;

		private bool _enabled;

		private ScheduledJobDefinition _jobDefAssociation;

		private static string _allUsers;

		internal static string AllUsers
		{
			get
			{
				return ScheduledJobTrigger._allUsers;
			}
		}

		public DateTime? At
		{
			get
			{
				return this._time;
			}
			set
			{
				this._time = value;
			}
		}

		public List<DayOfWeek> DaysOfWeek
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

		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
			set
			{
				this._enabled = value;
			}
		}

		public TriggerFrequency Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				this._frequency = value;
			}
		}

		public int Id
		{
			get
			{
				return this._id;
			}
			internal set
			{
				this._id = value;
			}
		}

		public int Interval
		{
			get
			{
				return this._interval;
			}
			set
			{
				this._interval = value;
			}
		}

		public ScheduledJobDefinition JobDefinition
		{
			get
			{
				return this._jobDefAssociation;
			}
			internal set
			{
				this._jobDefAssociation = value;
			}
		}

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

		public TimeSpan? RepetitionDuration
		{
			get
			{
				return this._repDuration;
			}
			set
			{
				TimeSpan? nullable;
				ScheduledJobTrigger scheduledJobTrigger = this;
				if (!value.HasValue || !(value.Value == TimeSpan.Zero))
				{
					nullable = value;
				}
				else
				{
					TimeSpan? nullable1 = null;
					nullable = nullable1;
				}
				scheduledJobTrigger._repDuration = nullable;
			}
		}

		public TimeSpan? RepetitionInterval
		{
			get
			{
				return this._repInterval;
			}
			set
			{
				TimeSpan? nullable;
				ScheduledJobTrigger scheduledJobTrigger = this;
				if (!value.HasValue || !(value.Value == TimeSpan.Zero))
				{
					nullable = value;
				}
				else
				{
					TimeSpan? nullable1 = null;
					nullable = nullable1;
				}
				scheduledJobTrigger._repInterval = nullable;
			}
		}

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

		static ScheduledJobTrigger()
		{
			ScheduledJobTrigger._allUsers = "*";
		}

		public ScheduledJobTrigger()
		{
			this._interval = 1;
			this._enabled = true;
		}

		private ScheduledJobTrigger(bool enabled, TriggerFrequency frequency, DateTime? time, List<DayOfWeek> daysOfWeek, int interval, TimeSpan randomDelay, TimeSpan? repetitionInterval, TimeSpan? repetitionDuration, string user, int id)
		{
			this._interval = 1;
			this._enabled = true;
			this._enabled = enabled;
			this._frequency = frequency;
			this._time = time;
			this._daysOfWeek = daysOfWeek;
			this._interval = interval;
			this._randomDelay = randomDelay;
			this.RepetitionInterval = repetitionInterval;
			this.RepetitionDuration = repetitionDuration;
			this._user = user;
			this._id = id;
		}

		internal ScheduledJobTrigger(ScheduledJobTrigger copyTrigger)
		{
			this._interval = 1;
			this._enabled = true;
			if (copyTrigger != null)
			{
				this._enabled = copyTrigger.Enabled;
				this._frequency = copyTrigger.Frequency;
				this._id = copyTrigger.Id;
				this._time = copyTrigger.At;
				this._daysOfWeek = copyTrigger.DaysOfWeek;
				this._interval = copyTrigger.Interval;
				this._randomDelay = copyTrigger.RandomDelay;
				this._repInterval = copyTrigger.RepetitionInterval;
				this._repDuration = copyTrigger.RepetitionDuration;
				this._user = copyTrigger.User;
				this._jobDefAssociation = copyTrigger.JobDefinition;
				return;
			}
			else
			{
				throw new PSArgumentNullException("copyTrigger");
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		private ScheduledJobTrigger(SerializationInfo info, StreamingContext context)
		{
			this._interval = 1;
			this._enabled = true;
			if (info != null)
			{
				DateTime dateTime = info.GetDateTime("Time_Value");
				if (dateTime == DateTime.MinValue)
				{
					this._time = null;
				}
				else
				{
					this._time = new DateTime?(dateTime);
				}
				this.RepetitionInterval = (TimeSpan?)info.GetValue("RepetitionInterval_Value", typeof(TimeSpan));
				this.RepetitionDuration = (TimeSpan?)info.GetValue("RepetitionDuration_Value", typeof(TimeSpan));
				this._daysOfWeek = (List<DayOfWeek>)info.GetValue("DaysOfWeek_Value", typeof(List<DayOfWeek>));
				this._randomDelay = (TimeSpan)info.GetValue("RandomDelay_Value", typeof(TimeSpan));
				this._interval = info.GetInt32("Interval_Value");
				this._user = info.GetString("User_Value");
				this._frequency = (TriggerFrequency)info.GetValue("TriggerFrequency_Value", typeof(TriggerFrequency));
				this._id = info.GetInt32("ID_Value");
				this._enabled = info.GetBoolean("Enabled_Value");
				this._jobDefAssociation = null;
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		internal void ClearProperties()
		{
			this._time = null;
			this._daysOfWeek = null;
			this._interval = 1;
			this._randomDelay = TimeSpan.Zero;
			this._repInterval = null;
			this._repDuration = null;
			this._user = null;
			this._frequency = TriggerFrequency.None;
			this._enabled = false;
			this._id = 0;
		}

		internal void CopyTo(ScheduledJobTrigger targetTrigger)
		{
			if (targetTrigger != null)
			{
				targetTrigger.Enabled = this._enabled;
				targetTrigger.Frequency = this._frequency;
				targetTrigger.Id = this._id;
				targetTrigger.At = this._time;
				targetTrigger.DaysOfWeek = this._daysOfWeek;
				targetTrigger.Interval = this._interval;
				targetTrigger.RandomDelay = this._randomDelay;
				targetTrigger.RepetitionInterval = this._repInterval;
				targetTrigger.RepetitionDuration = this._repDuration;
				targetTrigger.User = this._user;
				targetTrigger.JobDefinition = this._jobDefAssociation;
				return;
			}
			else
			{
				throw new PSArgumentNullException("targetTrigger");
			}
		}

		public static ScheduledJobTrigger CreateAtLogOnTrigger(string user, TimeSpan delay, int id, bool enabled)
		{
			string allUsers;
			bool flag = enabled;
			int num = 4;
			DateTime? nullable = null;
			DateTime? nullable1 = nullable;
			int num1 = 1;
			TimeSpan timeSpan = delay;
			TimeSpan? nullable2 = null;
			TimeSpan? nullable3 = nullable2;
			TimeSpan? nullable4 = null;
			TimeSpan? nullable5 = nullable4;
			if (string.IsNullOrEmpty(user))
			{
				allUsers = ScheduledJobTrigger.AllUsers;
			}
			else
			{
				allUsers = user;
			}
			return new ScheduledJobTrigger(flag, (TriggerFrequency)num, nullable1, null, num1, timeSpan, nullable3, nullable5, allUsers, id);
		}

		public static ScheduledJobTrigger CreateAtStartupTrigger(TimeSpan delay, int id, bool enabled)
		{
			DateTime? nullable = null;
			TimeSpan? nullable1 = null;
			TimeSpan? nullable2 = null;
			return new ScheduledJobTrigger(enabled, TriggerFrequency.AtStartup, nullable, null, 1, delay, nullable1, nullable2, null, id);
		}

		public static ScheduledJobTrigger CreateDailyTrigger(DateTime time, int interval, TimeSpan delay, int id, bool enabled)
		{
			TimeSpan? nullable = null;
			TimeSpan? nullable1 = null;
			return new ScheduledJobTrigger(enabled, TriggerFrequency.Daily, new DateTime?(time), null, interval, delay, nullable, nullable1, null, id);
		}

		public static ScheduledJobTrigger CreateOnceTrigger(DateTime time, TimeSpan delay, TimeSpan? repetitionInterval, TimeSpan? repetitionDuration, int id, bool enabled)
		{
			return new ScheduledJobTrigger(enabled, TriggerFrequency.Once, new DateTime?(time), null, 1, delay, repetitionInterval, repetitionDuration, null, id);
		}

		public static ScheduledJobTrigger CreateWeeklyTrigger(DateTime time, int interval, IEnumerable<DayOfWeek> daysOfWeek, TimeSpan delay, int id, bool enabled)
		{
			List<DayOfWeek> dayOfWeeks;
			if (daysOfWeek != null)
			{
				dayOfWeeks = new List<DayOfWeek>(daysOfWeek);
			}
			else
			{
				dayOfWeeks = null;
			}
			List<DayOfWeek> dayOfWeeks1 = dayOfWeeks;
			TimeSpan? nullable = null;
			TimeSpan? nullable1 = null;
			return new ScheduledJobTrigger(enabled, TriggerFrequency.Weekly, new DateTime?(time), dayOfWeeks1, interval, delay, nullable, nullable1, null, id);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info != null)
			{
				if (this._time.HasValue)
				{
					info.AddValue("Time_Value", this._time);
				}
				else
				{
					info.AddValue("Time_Value", DateTime.MinValue);
				}
				if (this._repInterval.HasValue)
				{
					info.AddValue("RepetitionInterval_Value", this._repInterval);
				}
				else
				{
					info.AddValue("RepetitionInterval_Value", TimeSpan.Zero);
				}
				if (this._repDuration.HasValue)
				{
					info.AddValue("RepetitionDuration_Value", this._repDuration);
				}
				else
				{
					info.AddValue("RepetitionDuration_Value", TimeSpan.Zero);
				}
				info.AddValue("DaysOfWeek_Value", this._daysOfWeek);
				info.AddValue("RandomDelay_Value", this._randomDelay);
				info.AddValue("Interval_Value", this._interval);
				info.AddValue("User_Value", this._user);
				info.AddValue("TriggerFrequency_Value", this._frequency);
				info.AddValue("ID_Value", this._id);
				info.AddValue("Enabled_Value", this._enabled);
				return;
			}
			else
			{
				throw new PSArgumentNullException("info");
			}
		}

		internal static bool IsAllUsers(string userName)
		{
			return string.Compare(userName, ScheduledJobTrigger.AllUsers, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public void UpdateJobDefinition()
		{
			if (this._jobDefAssociation != null)
			{
				ScheduledJobTrigger[] scheduledJobTriggerArray = new ScheduledJobTrigger[1];
				scheduledJobTriggerArray[0] = this;
				this._jobDefAssociation.UpdateTriggers(scheduledJobTriggerArray, true);
				return;
			}
			else
			{
				string str = StringUtil.Format(ScheduledJobErrorStrings.NoAssociatedJobDefinitionForTrigger, this._id);
				throw new RuntimeException(str);
			}
		}

		internal void Validate()
		{
			TriggerFrequency triggerFrequency = this._frequency;
			switch (triggerFrequency)
			{
				case TriggerFrequency.None:
				{
					throw new ScheduledJobException(ScheduledJobErrorStrings.MissingJobTriggerType);
				}
				case TriggerFrequency.Once:
				{
					if (this._time.HasValue)
					{
						if (!this._repInterval.HasValue && !this._repDuration.HasValue)
						{
							return;
						}
						ScheduledJobTrigger.ValidateOnceRepetitionParams(this._repInterval, this._repDuration);
						return;
					}
					else
					{
						string str = StringUtil.Format(ScheduledJobErrorStrings.MissingJobTriggerTime, ScheduledJobErrorStrings.TriggerOnceType);
						throw new ScheduledJobException(str);
					}
				}
				case TriggerFrequency.Daily:
				{
					if (this._time.HasValue)
					{
						if (this._interval >= 1)
						{
							return;
						}
						throw new ScheduledJobException(ScheduledJobErrorStrings.InvalidDaysIntervalParam);
					}
					else
					{
						string str1 = StringUtil.Format(ScheduledJobErrorStrings.MissingJobTriggerTime, ScheduledJobErrorStrings.TriggerDailyType);
						throw new ScheduledJobException(str1);
					}
				}
				case TriggerFrequency.Weekly:
				{
					if (this._time.HasValue)
					{
						if (this._interval >= 1)
						{
							if (this._daysOfWeek != null && this._daysOfWeek.Count != 0)
							{
								return;
							}
							string str2 = StringUtil.Format(ScheduledJobErrorStrings.MissingJobTriggerDaysOfWeek, ScheduledJobErrorStrings.TriggerWeeklyType);
							throw new ScheduledJobException(str2);
						}
						else
						{
							throw new ScheduledJobException(ScheduledJobErrorStrings.InvalidWeeksIntervalParam);
						}
					}
					else
					{
						string str3 = StringUtil.Format(ScheduledJobErrorStrings.MissingJobTriggerTime, ScheduledJobErrorStrings.TriggerWeeklyType);
						throw new ScheduledJobException(str3);
					}
				}
				case TriggerFrequency.AtLogon:
				case TriggerFrequency.AtStartup:
				{
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal static void ValidateOnceRepetitionParams(TimeSpan? repInterval, TimeSpan? repDuration)
		{
			bool valueOrDefault;
			bool flag;
			bool valueOrDefault1;
			bool flag1;
			bool valueOrDefault2;
			bool flag2;
			bool valueOrDefault3;
			bool flag3;
			if (!repInterval.HasValue || !repDuration.HasValue)
			{
				throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionParams);
			}
			else
			{
				TimeSpan? nullable = repInterval;
				TimeSpan zero = TimeSpan.Zero;
				if (nullable.HasValue)
				{
					valueOrDefault = nullable.GetValueOrDefault() < zero;
				}
				else
				{
					valueOrDefault = false;
				}
				if (!valueOrDefault)
				{
					TimeSpan? nullable1 = repDuration;
					TimeSpan timeSpan = TimeSpan.Zero;
					if (nullable1.HasValue)
					{
						flag = nullable1.GetValueOrDefault() < timeSpan;
					}
					else
					{
						flag = false;
					}
					if (!flag)
					{
						TimeSpan? nullable2 = repInterval;
						TimeSpan zero1 = TimeSpan.Zero;
						if (!nullable2.HasValue)
						{
							valueOrDefault1 = false;
						}
						else
						{
							valueOrDefault1 = nullable2.GetValueOrDefault() == zero1;
						}
						if (valueOrDefault1)
						{
							TimeSpan? nullable3 = repDuration;
							TimeSpan timeSpan1 = TimeSpan.Zero;
							if (!nullable3.HasValue)
							{
								flag3 = true;
							}
							else
							{
								flag3 = nullable3.GetValueOrDefault() != timeSpan1;
							}
							if (flag3)
							{
								throw new PSArgumentException(ScheduledJobErrorStrings.MismatchedRepetitionParamValues);
							}
						}
						TimeSpan? nullable4 = repInterval;
						TimeSpan timeSpan2 = TimeSpan.FromMinutes(1);
						if (nullable4.HasValue)
						{
							flag1 = nullable4.GetValueOrDefault() < timeSpan2;
						}
						else
						{
							flag1 = false;
						}
						if (flag1)
						{
							TimeSpan? nullable5 = repInterval;
							TimeSpan zero2 = TimeSpan.Zero;
							if (!nullable5.HasValue)
							{
								flag2 = false;
							}
							else
							{
								flag2 = nullable5.GetValueOrDefault() == zero2;
							}
							if (flag2)
							{
								TimeSpan? nullable6 = repDuration;
								TimeSpan zero3 = TimeSpan.Zero;
								if (!nullable6.HasValue)
								{
									valueOrDefault3 = false;
								}
								else
								{
									valueOrDefault3 = nullable6.GetValueOrDefault() == zero3;
								}
								if (valueOrDefault3)
								{
									goto Label0;
								}
							}
							throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionIntervalValue);
						}
					Label0:
						TimeSpan? nullable7 = repInterval;
						TimeSpan? nullable8 = repDuration;
						if (nullable7.HasValue & nullable8.HasValue)
						{
							valueOrDefault2 = nullable7.GetValueOrDefault() > nullable8.GetValueOrDefault();
						}
						else
						{
							valueOrDefault2 = false;
						}
						if (!valueOrDefault2)
						{
							return;
						}
						else
						{
							throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionInterval);
						}
					}
				}
				throw new PSArgumentException(ScheduledJobErrorStrings.InvalidRepetitionParamValues);
			}
		}
	}
}