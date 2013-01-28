using Microsoft.Management.Infrastructure;
using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	public sealed class JobTriggerToCimInstanceConverter : PSTypeConverter
	{
		private readonly static string CIM_TRIGGER_NAMESPACE;

		static JobTriggerToCimInstanceConverter()
		{
			JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE = "Root\\Microsoft\\Windows\\TaskScheduler";
		}

		public JobTriggerToCimInstanceConverter()
		{
		}

		private static void AddCommonProperties(ScheduledJobTrigger trigger, CimInstance cimInstance)
		{
			cimInstance.CimInstanceProperties["Enabled"].Value = trigger.Enabled;
			DateTime? at = trigger.At;
			if (at.HasValue)
			{
				cimInstance.CimInstanceProperties["StartBoundary"].Value = ScheduledJobWTS.ConvertDateTimeToString(trigger.At);
			}
		}

		public override bool CanConvertFrom(object sourceValue, Type destinationType)
		{
			if (destinationType != null)
			{
				if (sourceValue as ScheduledJobTrigger == null)
				{
					return false;
				}
				else
				{
					return destinationType.Equals(typeof(CimInstance));
				}
			}
			else
			{
				throw new ArgumentNullException("destinationType");
			}
		}

		public override bool CanConvertTo(object sourceValue, Type destinationType)
		{
			return false;
		}

		public override object ConvertFrom(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
		{
			object @default;
			if (destinationType != null)
			{
				if (sourceValue != null)
				{
					ScheduledJobTrigger scheduledJobTrigger = (ScheduledJobTrigger)sourceValue;
					CimSession cimSession = CimSession.Create(null);
					using (cimSession)
					{
						TriggerFrequency frequency = scheduledJobTrigger.Frequency;
						switch (frequency)
						{
							case TriggerFrequency.None:
							{
								@default = this.ConvertToDefault(scheduledJobTrigger, cimSession);
								return @default;
							}
							case TriggerFrequency.Once:
							{
								@default = this.ConvertToOnce(scheduledJobTrigger, cimSession);
								return @default;
							}
							case TriggerFrequency.Daily:
							{
								@default = this.ConvertToDaily(scheduledJobTrigger, cimSession);
								return @default;
							}
							case TriggerFrequency.Weekly:
							{
								@default = this.ConvertToWeekly(scheduledJobTrigger, cimSession);
								return @default;
							}
							case TriggerFrequency.AtLogon:
							{
								@default = this.ConvertToAtLogon(scheduledJobTrigger, cimSession);
								return @default;
							}
							case TriggerFrequency.AtStartup:
							{
								@default = this.ConvertToAtStartup(scheduledJobTrigger, cimSession);
								return @default;
							}
						}
						string str = StringUtil.Format(ScheduledJobErrorStrings.UnknownTriggerFrequency, scheduledJobTrigger.Frequency.ToString());
						throw new PSInvalidOperationException(str);
					}
					return @default;
				}
				else
				{
					throw new ArgumentNullException("sourceValue");
				}
			}
			else
			{
				throw new ArgumentNullException("destinationType");
			}
		}

		public override object ConvertTo(object sourceValue, Type destinationType, IFormatProvider formatProvider, bool ignoreCase)
		{
			throw new NotImplementedException();
		}

		private CimInstance ConvertToAtLogon(ScheduledJobTrigger trigger, CimSession cimSession)
		{
			string user;
			CimClass @class = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskLogonTrigger");
			CimInstance cimInstance = new CimInstance(@class);
			cimInstance.CimInstanceProperties["Delay"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(trigger.RandomDelay);
			if (ScheduledJobTrigger.IsAllUsers(trigger.User))
			{
				user = null;
			}
			else
			{
				user = trigger.User;
			}
			string str = user;
			cimInstance.CimInstanceProperties["UserId"].Value = str;
			JobTriggerToCimInstanceConverter.AddCommonProperties(trigger, cimInstance);
			return cimInstance;
		}

		private CimInstance ConvertToAtStartup(ScheduledJobTrigger trigger, CimSession cimSession)
		{
			CimClass @class = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskBootTrigger");
			CimInstance cimInstance = new CimInstance(@class);
			cimInstance.CimInstanceProperties["Delay"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(trigger.RandomDelay);
			JobTriggerToCimInstanceConverter.AddCommonProperties(trigger, cimInstance);
			return cimInstance;
		}

		private CimInstance ConvertToDaily(ScheduledJobTrigger trigger, CimSession cimSession)
		{
			CimClass @class = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskDailyTrigger");
			CimInstance cimInstance = new CimInstance(@class);
			cimInstance.CimInstanceProperties["RandomDelay"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(trigger.RandomDelay);
			cimInstance.CimInstanceProperties["DaysInterval"].Value = trigger.Interval;
			JobTriggerToCimInstanceConverter.AddCommonProperties(trigger, cimInstance);
			return cimInstance;
		}

		private CimInstance ConvertToDefault(ScheduledJobTrigger trigger, CimSession cimSession)
		{
			CimClass @class = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskTrigger");
			CimInstance cimInstance = new CimInstance(@class);
			JobTriggerToCimInstanceConverter.AddCommonProperties(trigger, cimInstance);
			return cimInstance;
		}

		private CimInstance ConvertToOnce(ScheduledJobTrigger trigger, CimSession cimSession)
		{
			bool valueOrDefault;
			CimClass @class = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskTimeTrigger");
			CimInstance cimInstance = new CimInstance(@class);
			cimInstance.CimInstanceProperties["RandomDelay"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(trigger.RandomDelay);
			TimeSpan? repetitionInterval = trigger.RepetitionInterval;
			if (repetitionInterval.HasValue)
			{
				TimeSpan? repetitionDuration = trigger.RepetitionDuration;
				if (repetitionDuration.HasValue)
				{
					CimClass cimClass = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskRepetitionPattern");
					CimInstance wTSString = new CimInstance(cimClass);
					TimeSpan? nullable = trigger.RepetitionInterval;
					wTSString.CimInstanceProperties["Interval"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(nullable.Value);
					TimeSpan? repetitionDuration1 = trigger.RepetitionDuration;
					TimeSpan maxValue = TimeSpan.MaxValue;
					if (!repetitionDuration1.HasValue)
					{
						valueOrDefault = false;
					}
					else
					{
						valueOrDefault = repetitionDuration1.GetValueOrDefault() == maxValue;
					}
					if (!valueOrDefault)
					{
						wTSString.CimInstanceProperties["StopAtDurationEnd"].Value = true;
						TimeSpan? nullable1 = trigger.RepetitionDuration;
						wTSString.CimInstanceProperties["Duration"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(nullable1.Value);
					}
					else
					{
						wTSString.CimInstanceProperties["StopAtDurationEnd"].Value = false;
					}
					cimInstance.CimInstanceProperties["Repetition"].Value = wTSString;
				}
			}
			JobTriggerToCimInstanceConverter.AddCommonProperties(trigger, cimInstance);
			return cimInstance;
		}

		private CimInstance ConvertToWeekly(ScheduledJobTrigger trigger, CimSession cimSession)
		{
			CimClass @class = cimSession.GetClass(JobTriggerToCimInstanceConverter.CIM_TRIGGER_NAMESPACE, "MSFT_TaskWeeklyTrigger");
			CimInstance cimInstance = new CimInstance(@class);
			cimInstance.CimInstanceProperties["DaysOfWeek"].Value = ScheduledJobWTS.ConvertDaysOfWeekToMask(trigger.DaysOfWeek);
			cimInstance.CimInstanceProperties["RandomDelay"].Value = ScheduledJobWTS.ConvertTimeSpanToWTSString(trigger.RandomDelay);
			cimInstance.CimInstanceProperties["WeeksInterval"].Value = trigger.Interval;
			JobTriggerToCimInstanceConverter.AddCommonProperties(trigger, cimInstance);
			return cimInstance;
		}
	}
}