using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using TaskScheduler;
using TaskScheduler.Implementation;

namespace Microsoft.PowerShell.ScheduledJob
{
	internal sealed class ScheduledJobWTS : IDisposable
	{
		private const short WTSSunday = 1;

		private const short WTSMonday = 2;

		private const short WTSTuesday = 4;

		private const short WTSWednesday = 8;

		private const short WTSThursday = 16;

		private const short WTSFriday = 32;

		private const short WTSSaturday = 64;

		private const string TaskSchedulerWindowsFolder = "\\Microsoft\\Windows";

		private const string ScheduledJobSubFolder = "PowerShell\\ScheduledJobs";

		private const string ScheduledJobTasksRootFolder = "\\Microsoft\\Windows\\PowerShell\\ScheduledJobs";

		private const string ScheduledJobTaskActionId = "StartPowerShellJob";

		private ITaskService _taskScheduler;

		private ITaskFolder _iRootFolder;

		public ScheduledJobWTS()
		{
			this._taskScheduler = CreateImpl();
			this._taskScheduler.Connect(null, null, null, null);
			this._iRootFolder = this.GetRootFolder();
		}


		private static TaskScheduler.TaskScheduler CreateImpl ()
		{
			if (OSHelper.IsMacOSX) return new OSXTaskScheduler();
			else if (OSHelper.IsUnix) return new CronTaskScheduler();
			return (TaskScheduler.TaskScheduler)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("0F87369F-A4E5-4CFC-BD3E-73E6154572DD")));
		}

		private void AddTaskAction(ITaskDefinition iTaskDefinition, ScheduledJobDefinition definition)
		{
			IExecAction pSExecutionPath = iTaskDefinition.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC) as IExecAction;
			pSExecutionPath.Id = "StartPowerShellJob";
			pSExecutionPath.Path = definition.PSExecutionPath;
			pSExecutionPath.Arguments = definition.PSExecutionArgs;
		}

		private void AddTaskOptions(ITaskDefinition iTaskDefinition, ScheduledJobOptions jobOptions)
		{
			_TASK_RUNLEVEL variable;
			iTaskDefinition.Settings.DisallowStartIfOnBatteries = !jobOptions.StartIfOnBatteries;
			iTaskDefinition.Settings.StopIfGoingOnBatteries = jobOptions.StopIfGoingOnBatteries;
			iTaskDefinition.Settings.WakeToRun = jobOptions.WakeToRun;
			iTaskDefinition.Settings.RunOnlyIfIdle = !jobOptions.StartIfNotIdle;
			iTaskDefinition.Settings.IdleSettings.StopOnIdleEnd = jobOptions.StopIfGoingOffIdle;
			iTaskDefinition.Settings.IdleSettings.RestartOnIdle = jobOptions.RestartOnIdleResume;
			iTaskDefinition.Settings.IdleSettings.IdleDuration = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobOptions.IdleDuration);
			iTaskDefinition.Settings.IdleSettings.WaitTimeout = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobOptions.IdleTimeout);
			iTaskDefinition.Settings.Hidden = !jobOptions.ShowInTaskScheduler;
			iTaskDefinition.Settings.RunOnlyIfNetworkAvailable = !jobOptions.RunWithoutNetwork;
			iTaskDefinition.Settings.AllowDemandStart = !jobOptions.DoNotAllowDemandStart;
			iTaskDefinition.Settings.MultipleInstances = this.ConvertFromMultiInstances(jobOptions.MultipleInstancePolicy);
			TaskScheduler.IPrincipal principal = iTaskDefinition.Principal;
			if (jobOptions.RunElevated)
			{
				variable = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
			}
			else
			{
				variable = _TASK_RUNLEVEL.TASK_RUNLEVEL_LUA;
			}
			principal.RunLevel = variable;
		}

		private void AddTaskTrigger(ITaskDefinition iTaskDefinition, ScheduledJobTrigger jobTrigger)
		{
			ITrigger str;
			string user;
			TriggerFrequency frequency = jobTrigger.Frequency;
			switch (frequency)
			{
				case TriggerFrequency.Once:
				{
					str = iTaskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);
					ITimeTrigger wTSString = str as ITimeTrigger;
					wTSString.RandomDelay = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobTrigger.RandomDelay);
					TimeSpan? repetitionInterval = jobTrigger.RepetitionInterval;
					if (repetitionInterval.HasValue)
					{
						TimeSpan? repetitionDuration = jobTrigger.RepetitionDuration;
						if (repetitionDuration.HasValue)
						{
							TimeSpan? nullable = jobTrigger.RepetitionInterval;
							wTSString.Repetition.Interval = ScheduledJobWTS.ConvertTimeSpanToWTSString(nullable.Value);
							TimeSpan? repetitionDuration1 = jobTrigger.RepetitionDuration;
							if (repetitionDuration1.Value != TimeSpan.MaxValue)
							{
								wTSString.Repetition.StopAtDurationEnd = true;
								TimeSpan? nullable1 = jobTrigger.RepetitionDuration;
								wTSString.Repetition.Duration = ScheduledJobWTS.ConvertTimeSpanToWTSString(nullable1.Value);
							}
							else
							{
								wTSString.Repetition.StopAtDurationEnd = false;
							}
						}
					}
					str.StartBoundary = ScheduledJobWTS.ConvertDateTimeToString(jobTrigger.At);
					int id = jobTrigger.Id;
					str.Id = id.ToString(CultureInfo.InvariantCulture);
					str.Enabled = jobTrigger.Enabled;
					return;
				}
				case TriggerFrequency.Daily:
				{
					str = iTaskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_DAILY);
					IDailyTrigger interval = str as IDailyTrigger;
					interval.RandomDelay = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobTrigger.RandomDelay);
					interval.DaysInterval = (short)jobTrigger.Interval;
					str.StartBoundary = ScheduledJobWTS.ConvertDateTimeToString(jobTrigger.At);
					int num = jobTrigger.Id;
					str.Id = num.ToString(CultureInfo.InvariantCulture);
					str.Enabled = jobTrigger.Enabled;
					return;
				}
				case TriggerFrequency.Weekly:
				{
					str = iTaskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_WEEKLY);
					IWeeklyTrigger mask = str as IWeeklyTrigger;
					mask.RandomDelay = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobTrigger.RandomDelay);
					mask.WeeksInterval = (short)jobTrigger.Interval;
					mask.DaysOfWeek = ScheduledJobWTS.ConvertDaysOfWeekToMask(jobTrigger.DaysOfWeek);
					str.StartBoundary = ScheduledJobWTS.ConvertDateTimeToString(jobTrigger.At);
					int id1 = jobTrigger.Id;
					str.Id = id1.ToString(CultureInfo.InvariantCulture);
					str.Enabled = jobTrigger.Enabled;
					return;
				}
				case TriggerFrequency.AtLogon:
				{
					str = iTaskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
					ILogonTrigger variable = str as ILogonTrigger;
					ILogonTrigger variable1 = variable;
					if (ScheduledJobTrigger.IsAllUsers(jobTrigger.User))
					{
						user = null;
					}
					else
					{
						user = jobTrigger.User;
					}
					variable1.UserId = user;
					variable.Delay = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobTrigger.RandomDelay);
					int num1 = jobTrigger.Id;
					str.Id = num1.ToString(CultureInfo.InvariantCulture);
					str.Enabled = jobTrigger.Enabled;
					return;
				}
				case TriggerFrequency.AtStartup:
				{
					str = iTaskDefinition.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_BOOT);
					IBootTrigger wTSString1 = str as IBootTrigger;
					wTSString1.Delay = ScheduledJobWTS.ConvertTimeSpanToWTSString(jobTrigger.RandomDelay);
					int id2 = jobTrigger.Id;
					str.Id = id2.ToString(CultureInfo.InvariantCulture);
					str.Enabled = jobTrigger.Enabled;
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal static string ConvertDateTimeToString(DateTime? dt)
		{
			if (dt.HasValue)
			{
				DateTime value = dt.Value;
				return value.ToString("s", CultureInfo.InvariantCulture);
			}
			else
			{
				return string.Empty;
			}
		}

		internal static short ConvertDaysOfWeekToMask(IEnumerable<DayOfWeek> daysOfWeek)
		{
			short num = 0;
			foreach (DayOfWeek dayOfWeek in daysOfWeek)
			{
				DayOfWeek dayOfWeek1 = dayOfWeek;
				switch (dayOfWeek1)
				{
					case DayOfWeek.Sunday:
					{
						num = (short)(num | 1);
					}
					break;
					case DayOfWeek.Monday:
					{
						num = (short)(num | 2);
					}
					break;
					case DayOfWeek.Tuesday:
					{
						num = (short)(num | 4);
					}
					break;
					case DayOfWeek.Wednesday:
					{
						num = (short)(num | 8);
					}
					break;
					case DayOfWeek.Thursday:
					{
						num = (short)(num | 16);
					}
					break;
					case DayOfWeek.Friday:
					{
						num = (short)(num | 32);
					}
					break;
					case DayOfWeek.Saturday:
					{
						num = (short)(num | 64);
					}
					break;
				}
			}
			return num;
		}

		private _TASK_INSTANCES_POLICY ConvertFromMultiInstances(TaskMultipleInstancePolicy jobPolicies)
		{
			TaskMultipleInstancePolicy taskMultipleInstancePolicy = jobPolicies;
			switch (taskMultipleInstancePolicy)
			{
				case TaskMultipleInstancePolicy.IgnoreNew:
				{
					return _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
				}
				case TaskMultipleInstancePolicy.Parallel:
				{
					return _TASK_INSTANCES_POLICY.TASK_INSTANCES_PARALLEL;
				}
				case TaskMultipleInstancePolicy.Queue:
				{
					return _TASK_INSTANCES_POLICY.TASK_INSTANCES_QUEUE;
				}
				case TaskMultipleInstancePolicy.StopExisting:
				{
					return _TASK_INSTANCES_POLICY.TASK_INSTANCES_STOP_EXISTING;
				}
			}
			return _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;
		}

		private List<DayOfWeek> ConvertMaskToDaysOfWeekArray(short mask)
		{
			List<DayOfWeek> dayOfWeeks = new List<DayOfWeek>();
			if ((mask & 1) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Sunday);
			}
			if ((mask & 2) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Monday);
			}
			if ((mask & 4) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Tuesday);
			}
			if ((mask & 8) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Wednesday);
			}
			if ((mask & 16) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Thursday);
			}
			if ((mask & 32) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Friday);
			}
			if ((mask & 64) != 0)
			{
				dayOfWeeks.Add(DayOfWeek.Saturday);
			}
			return dayOfWeeks;
		}

		private int ConvertStringId(string triggerId)
		{
			int num = 0;
			try
			{
				num = Convert.ToInt32(triggerId);
			}
			catch (FormatException formatException)
			{
			}
			catch (OverflowException overflowException)
			{
			}
			return num;
		}

		internal static string ConvertTimeSpanToWTSString(TimeSpan time)
		{
			object[] days = new object[4];
			days[0] = time.Days;
			days[1] = time.Hours;
			days[2] = time.Minutes;
			days[3] = time.Seconds;
			return string.Format(CultureInfo.InvariantCulture, "P{0}DT{1}H{2}M{3}S", days);
		}

		private TaskMultipleInstancePolicy ConvertToMultiInstances(ITaskSettings iTaskSettings)
		{
			_TASK_INSTANCES_POLICY multipleInstances = iTaskSettings.MultipleInstances;
			switch (multipleInstances)
			{
				case _TASK_INSTANCES_POLICY.TASK_INSTANCES_PARALLEL:
				{
					return TaskMultipleInstancePolicy.Parallel;
				}
				case _TASK_INSTANCES_POLICY.TASK_INSTANCES_QUEUE:
				{
					return TaskMultipleInstancePolicy.Queue;
				}
				case _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW:
				{
					return TaskMultipleInstancePolicy.IgnoreNew;
				}
				case _TASK_INSTANCES_POLICY.TASK_INSTANCES_STOP_EXISTING:
				{
					return TaskMultipleInstancePolicy.StopExisting;
				}
			}
			return TaskMultipleInstancePolicy.None;
		}

		private ScheduledJobOptions CreateJobOptions(ITaskDefinition iTaskDefinition)
		{
			ITaskSettings settings = iTaskDefinition.Settings;
			TaskScheduler.IPrincipal principal = iTaskDefinition.Principal;
			return new ScheduledJobOptions(!settings.DisallowStartIfOnBatteries, settings.StopIfGoingOnBatteries, settings.WakeToRun, !settings.RunOnlyIfIdle, settings.IdleSettings.StopOnIdleEnd, settings.IdleSettings.RestartOnIdle, this.ParseWTSTime(settings.IdleSettings.IdleDuration), this.ParseWTSTime(settings.IdleSettings.WaitTimeout), !settings.Hidden, principal.RunLevel == _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST, !settings.RunOnlyIfNetworkAvailable, !settings.AllowDemandStart, this.ConvertToMultiInstances(settings));
		}

		private ScheduledJobTrigger CreateJobTrigger(ITrigger iTrigger)
		{
			TimeSpan maxValue;
			ScheduledJobTrigger scheduledJobTrigger = null;
			if (iTrigger as IBootTrigger == null)
			{
				if (iTrigger as ILogonTrigger == null)
				{
					if (iTrigger as ITimeTrigger == null)
					{
						if (iTrigger as IDailyTrigger == null)
						{
							if (iTrigger as IWeeklyTrigger != null)
							{
								IWeeklyTrigger variable = (IWeeklyTrigger)iTrigger;
								scheduledJobTrigger = ScheduledJobTrigger.CreateWeeklyTrigger(DateTime.Parse(variable.StartBoundary, CultureInfo.InvariantCulture), variable.WeeksInterval, this.ConvertMaskToDaysOfWeekArray(variable.DaysOfWeek), this.ParseWTSTime(variable.RandomDelay), this.ConvertStringId(variable.Id), variable.Enabled);
							}
						}
						else
						{
							IDailyTrigger variable1 = (IDailyTrigger)iTrigger;
							scheduledJobTrigger = ScheduledJobTrigger.CreateDailyTrigger(DateTime.Parse(variable1.StartBoundary, CultureInfo.InvariantCulture), variable1.DaysInterval, this.ParseWTSTime(variable1.RandomDelay), this.ConvertStringId(variable1.Id), variable1.Enabled);
						}
					}
					else
					{
						ITimeTrigger variable2 = (ITimeTrigger)iTrigger;
						TimeSpan timeSpan = this.ParseWTSTime(variable2.Repetition.Interval);
						if (!(timeSpan != TimeSpan.Zero) || variable2.Repetition.StopAtDurationEnd)
						{
							maxValue = this.ParseWTSTime(variable2.Repetition.Duration);
						}
						else
						{
							maxValue = TimeSpan.MaxValue;
						}
						TimeSpan timeSpan1 = maxValue;
						scheduledJobTrigger = ScheduledJobTrigger.CreateOnceTrigger(DateTime.Parse(variable2.StartBoundary, CultureInfo.InvariantCulture), this.ParseWTSTime(variable2.RandomDelay), new TimeSpan?(timeSpan), new TimeSpan?(timeSpan1), this.ConvertStringId(variable2.Id), variable2.Enabled);
					}
				}
				else
				{
					ILogonTrigger variable3 = (ILogonTrigger)iTrigger;
					scheduledJobTrigger = ScheduledJobTrigger.CreateAtLogOnTrigger(variable3.UserId, this.ParseWTSTime(variable3.Delay), this.ConvertStringId(variable3.Id), variable3.Enabled);
				}
			}
			else
			{
				IBootTrigger variable4 = (IBootTrigger)iTrigger;
				scheduledJobTrigger = ScheduledJobTrigger.CreateAtStartupTrigger(this.ParseWTSTime(variable4.Delay), this.ConvertStringId(variable4.Id), variable4.Enabled);
			}
			return scheduledJobTrigger;
		}

		public void CreateTask(ScheduledJobDefinition definition)
		{
			if (definition != null)
			{
				ITaskDefinition variable = this._taskScheduler.NewTask(0);
				this.AddTaskOptions(variable, definition.Options);
				foreach (ScheduledJobTrigger jobTrigger in definition.JobTriggers)
				{
					this.AddTaskTrigger(variable, jobTrigger);
				}
				this.AddTaskAction(variable, definition);
				string str = "D:P(A;;GA;;;SY)(A;;GA;;;BA)";
				SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
				CommonSecurityDescriptor commonSecurityDescriptor = new CommonSecurityDescriptor(false, false, str);
				commonSecurityDescriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, user, 0x10000000, InheritanceFlags.None, PropagationFlags.None);
				string sddlForm = commonSecurityDescriptor.GetSddlForm(AccessControlSections.All);
				if (definition.Credential != null)
				{
					this._iRootFolder.RegisterTaskDefinition(definition.Name, variable, 2, definition.Credential.UserName, this.GetCredentialPassword(definition.Credential), _TASK_LOGON_TYPE.TASK_LOGON_PASSWORD, sddlForm);
					return;
				}
				else
				{
					this._iRootFolder.RegisterTaskDefinition(definition.Name, variable, 2, null, null, _TASK_LOGON_TYPE.TASK_LOGON_S4U, sddlForm);
					return;
				}
			}
			else
			{
				throw new PSArgumentNullException("definition");
			}
		}

		public void Dispose()
		{
			this._iRootFolder = null;
			this._taskScheduler = null;
			GC.SuppressFinalize(this);
		}

		private ITaskDefinition FindTask(string taskId)
		{
			ITaskDefinition definition;
			try
			{
				ITaskFolder folder = this._taskScheduler.GetFolder("\\Microsoft\\Windows\\PowerShell\\ScheduledJobs");
				IRegisteredTask task = folder.GetTask(taskId);
				definition = task.Definition;
			}
			catch (DirectoryNotFoundException directoryNotFoundException1)
			{
				DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
				string str = StringUtil.Format(ScheduledJobErrorStrings.CannotFindTaskId, taskId);
				throw new ScheduledJobException(str, directoryNotFoundException);
			}
			return definition;
		}

		private string GetCredentialPassword(PSCredential credential)
		{
			string stringUni;
			if (credential != null)
			{
				IntPtr zero = IntPtr.Zero;
				try
				{
					zero = Marshal.SecureStringToGlobalAllocUnicode(credential.Password);
					stringUni = Marshal.PtrToStringUni(zero);
				}
				finally
				{
					Marshal.ZeroFreeGlobalAllocUnicode(zero);
				}
				return stringUni;
			}
			else
			{
				return null;
			}
		}

		public ScheduledJobOptions GetJobOptions(string taskId)
		{
			if (!string.IsNullOrEmpty(taskId))
			{
				ITaskDefinition variable = this.FindTask(taskId);
				return this.CreateJobOptions(variable);
			}
			else
			{
				throw new PSArgumentException("taskId");
			}
		}

		public Collection<ScheduledJobTrigger> GetJobTriggers(string taskId)
		{
			if (!string.IsNullOrEmpty(taskId))
			{
				ITaskDefinition variable = this.FindTask(taskId);
				Collection<ScheduledJobTrigger> scheduledJobTriggers = new Collection<ScheduledJobTrigger>();
				ITriggerCollection triggers = variable.Triggers;
				if (triggers != null)
				{
					foreach (ITrigger trigger in triggers)
					{
						ScheduledJobTrigger scheduledJobTrigger = this.CreateJobTrigger(trigger);
						if (scheduledJobTrigger != null)
						{
							scheduledJobTriggers.Add(scheduledJobTrigger);
						}
						else
						{
							object[] id = new object[2];
							id[0] = taskId;
							id[1] = trigger.Id;
							string str = StringUtil.Format(ScheduledJobErrorStrings.UnknownTriggerType, id);
							throw new ScheduledJobException(str);
						}
					}
				}
				return scheduledJobTriggers;
			}
			else
			{
				throw new PSArgumentException("taskId");
			}
		}

		private ITaskFolder GetRootFolder()
		{
			ITaskFolder folder = null;
			try
			{
				folder = this._taskScheduler.GetFolder("\\Microsoft\\Windows\\PowerShell\\ScheduledJobs");
			}
			catch (DirectoryNotFoundException directoryNotFoundException)
			{

			}
			catch (FileNotFoundException fileNotFoundException)
			{

			}
			if (folder == null)
			{
				ITaskFolder variable = this._taskScheduler.GetFolder(TaskSchedulerWindowsFolder);
				folder = variable.CreateFolder("PowerShell\\ScheduledJobs", Missing.Value);
			}
			return folder;
		}

		public bool GetTaskEnabled(string taskId)
		{
			if (!string.IsNullOrEmpty(taskId))
			{
				ITaskDefinition variable = this.FindTask(taskId);
				return variable.Settings.Enabled;
			}
			else
			{
				throw new PSArgumentException("taskId");
			}
		}

		private TimeSpan ParseWTSTime(string wtsTime)
		{
			if (!string.IsNullOrEmpty(wtsTime))
			{
				int num = 0;
				int num1 = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int length = wtsTime.Length;
				StringBuilder stringBuilder = new StringBuilder();
				try
				{
				Label0:
					while (num4 != length)
					{
						int num5 = num4;
						num4 = num5 + 1;
						char chr = wtsTime[num5];
						char chr1 = chr;
						if (chr1 == 'P')
						{
							stringBuilder.Clear();
							while (num4 != length)
							{
								if (wtsTime[num4] != 'T')
								{
									int num6 = num4;
									num4 = num6 + 1;
									char chr2 = wtsTime[num6];
									if (chr2 != 'Y')
									{
										if (chr2 != 'M')
										{
											if (chr2 != 'D')
											{
												if (chr2 < '0' || chr2 > '9')
												{
													continue;
												}
												stringBuilder.Append(chr2);
											}
											else
											{
												num = Convert.ToInt32(stringBuilder.ToString(), CultureInfo.InvariantCulture);
												stringBuilder.Clear();
											}
										}
										else
										{
											stringBuilder.Clear();
										}
									}
									else
									{
										stringBuilder.Clear();
									}
								}
								else
								{
									goto Label0;
								}
							}
						}
						else
						{
							if (chr1 == 'T')
							{
								stringBuilder.Clear();
								while (num4 != length && wtsTime[num4] != 'P')
								{
									int num7 = num4;
									num4 = num7 + 1;
									char chr3 = wtsTime[num7];
									if (chr3 != 'H')
									{
										if (chr3 != 'M')
										{
											if (chr3 != 'S')
											{
												if (chr3 < '0' || chr3 > '9')
												{
													continue;
												}
												stringBuilder.Append(chr3);
											}
											else
											{
												num3 = Convert.ToInt32(stringBuilder.ToString(), CultureInfo.InvariantCulture);
												stringBuilder.Clear();
											}
										}
										else
										{
											num2 = Convert.ToInt32(stringBuilder.ToString(), CultureInfo.InvariantCulture);
											stringBuilder.Clear();
										}
									}
									else
									{
										num1 = Convert.ToInt32(stringBuilder.ToString(), CultureInfo.InvariantCulture);
										stringBuilder.Clear();
									}
								}
							}
						}
					}
				}
				catch (FormatException formatException)
				{
				}
				catch (OverflowException overflowException)
				{
				}
				return new TimeSpan(num, num1, num2, num3);
			}
			else
			{
				return new TimeSpan((long)0);
			}
		}

		public void RemoveTask(ScheduledJobDefinition definition, bool force = false)
		{
			if (definition != null)
			{
				this.RemoveTaskByName(definition.Name, force, false);
				return;
			}
			else
			{
				throw new PSArgumentNullException("definition");
			}
		}

		public void RemoveTaskByName(string taskName, bool force, bool firstCheckForTask)
		{
			IRegisteredTask task = null;
			try
			{
				task = this._iRootFolder.GetTask(taskName);
			}
			catch (DirectoryNotFoundException directoryNotFoundException)
			{
				if (!firstCheckForTask)
				{
					throw;
				}
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				if (!firstCheckForTask)
				{
					throw;
				}
			}
			if (task != null)
			{
				IRunningTaskCollection instances = task.GetInstances(0);
				if (instances.Count > 0)
				{
					if (force)
					{
						task.Stop(0);
					}
					else
					{
						string str = StringUtil.Format(ScheduledJobErrorStrings.CannotRemoveTaskRunningInstance, taskName);
						throw new ScheduledJobException(str);
					}
				}
				this._iRootFolder.DeleteTask(taskName, 0);
				return;
			}
			else
			{
				return;
			}
		}

		public void UpdateTask(ScheduledJobDefinition definition)
		{
			if (definition != null)
			{
				ITaskDefinition enabled = this.FindTask(definition.Name);
				this.AddTaskOptions(enabled, definition.Options);
				enabled.Settings.Enabled = definition.Enabled;
				enabled.Triggers.Clear();
				foreach (ScheduledJobTrigger jobTrigger in definition.JobTriggers)
				{
					this.AddTaskTrigger(enabled, jobTrigger);
				}
				enabled.Actions.Clear();
				this.AddTaskAction(enabled, definition);
				if (definition.Credential != null)
				{
					this._iRootFolder.RegisterTaskDefinition(definition.Name, enabled, 4, definition.Credential.UserName, this.GetCredentialPassword(definition.Credential), _TASK_LOGON_TYPE.TASK_LOGON_PASSWORD, null);
					return;
				}
				else
				{
					this._iRootFolder.RegisterTaskDefinition(definition.Name, enabled, 4, null, null, _TASK_LOGON_TYPE.TASK_LOGON_S4U, null);
					return;
				}
			}
			else
			{
				throw new PSArgumentNullException("definition");
			}
		}
	}
}