using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.PowerShell.ScheduledJob
{
	public sealed class ScheduledJobSourceAdapter : JobSourceAdapter
	{
		internal const string AdapterTypeName = "PSScheduledJob";

		public const string BeforeFilter = "Before";

		public const string AfterFilter = "After";

		public const string NewestFilter = "Newest";

		private static FileSystemWatcher StoreWatcher;

		private static object SyncObject;

		private static ScheduledJobSourceAdapter.ScheduledJobRepository JobRepository;

		static ScheduledJobSourceAdapter()
		{
			ScheduledJobSourceAdapter.SyncObject = new object();
			ScheduledJobSourceAdapter.JobRepository = new ScheduledJobSourceAdapter.ScheduledJobRepository();
		}

		public ScheduledJobSourceAdapter()
		{
			base.Name = "PSScheduledJob";
		}

		internal static void AddToRepository(Job2 job)
		{
			if (job != null)
			{
				ScheduledJobSourceAdapter.JobRepository.AddOrReplace(job);
				return;
			}
			else
			{
				throw new PSArgumentNullException("job");
			}
		}

		private static void CheckJobStoreResults(string outputPath, int executionHistoryLength)
		{
			DateTime dateTime;
			Collection<DateTime> jobRunsForDefinitionPath = ScheduledJobStore.GetJobRunsForDefinitionPath(outputPath);
			if (jobRunsForDefinitionPath.Count > executionHistoryLength)
			{
				DateTime maxValue = DateTime.MaxValue;
				foreach (DateTime dateTime1 in jobRunsForDefinitionPath)
				{
					if (dateTime1 < maxValue)
					{
						dateTime = dateTime1;
					}
					else
					{
						dateTime = maxValue;
					}
					maxValue = dateTime;
				}
				try
				{
					ScheduledJobStore.RemoveJobRunFromOutputPath(outputPath, maxValue);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException)
				{
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal static void ClearRepository()
		{
			ScheduledJobSourceAdapter.JobRepository.Clear();
		}

		internal static void ClearRepositoryForDefinition(string definitionName)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				List<Job2> jobs = ScheduledJobSourceAdapter.JobRepository.Jobs;
				foreach (Job2 job in jobs)
				{
					if (string.Compare(definitionName, job.Name, StringComparison.OrdinalIgnoreCase) != 0)
					{
						continue;
					}
					ScheduledJobSourceAdapter.JobRepository.Remove(job);
				}
				return;
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		private void CreateFileSystemWatcher()
		{
			if (ScheduledJobSourceAdapter.StoreWatcher == null)
			{
				lock (ScheduledJobSourceAdapter.SyncObject)
				{
					if (ScheduledJobSourceAdapter.StoreWatcher == null)
					{
						ScheduledJobSourceAdapter.StoreWatcher = new FileSystemWatcher(ScheduledJobStore.GetJobDefinitionLocation());
						ScheduledJobSourceAdapter.StoreWatcher.IncludeSubdirectories = true;
						ScheduledJobSourceAdapter.StoreWatcher.NotifyFilter = NotifyFilters.LastWrite;
						ScheduledJobSourceAdapter.StoreWatcher.Filter = "Results.xml";
						ScheduledJobSourceAdapter.StoreWatcher.EnableRaisingEvents = true;
						FileSystemWatcher storeWatcher = ScheduledJobSourceAdapter.StoreWatcher;
						storeWatcher.Changed += (object sender, FileSystemEventArgs e) => ScheduledJobSourceAdapter.UpdateRepositoryObjects(e);
					}
				}
			}
		}

		public override Job2 GetJobByInstanceId(Guid instanceId, bool recurse)
		{
			Job2 job2;
			this.RefreshRepository();
			List<Job2>.Enumerator enumerator = ScheduledJobSourceAdapter.JobRepository.Jobs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Job2 current = enumerator.Current;
					if (!object.Equals(current.InstanceId, instanceId))
					{
						continue;
					}
					job2 = current;
					return job2;
				}
				return null;
			}
			finally
			{
				enumerator.Dispose();
			}
			return job2;
		}

		public override Job2 GetJobBySessionId(int id, bool recurse)
		{
			Job2 job2;
			this.RefreshRepository();
			List<Job2>.Enumerator enumerator = ScheduledJobSourceAdapter.JobRepository.Jobs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Job2 current = enumerator.Current;
					if (id != current.Id)
					{
						continue;
					}
					job2 = current;
					return job2;
				}
				return null;
			}
			finally
			{
				enumerator.Dispose();
			}
			return job2;
		}

		private static bool GetJobRunInfo(string path, out string definitionName, out DateTime jobRunReturn)
		{
			char[] directorySeparatorChar = new char[1];
			directorySeparatorChar[0] = Path.DirectorySeparatorChar;
			string[] strArrays = path.Split(directorySeparatorChar);
			if ((int)strArrays.Length != 4)
			{
				definitionName = null;
				jobRunReturn = DateTime.MinValue;
				return false;
			}
			else
			{
				definitionName = strArrays[0];
				return ScheduledJobStore.ConvertJobRunNameToDateTime(strArrays[2], out jobRunReturn);
			}
		}

		internal static Collection<DateTime> GetJobRuns(string definitionName)
		{
			Collection<DateTime> jobRunsForDefinition = null;
			try
			{
				jobRunsForDefinition = ScheduledJobStore.GetJobRunsForDefinition(definitionName);
			}
			catch (DirectoryNotFoundException directoryNotFoundException)
			{
			}
			catch (FileNotFoundException fileNotFoundException)
			{
			}
			catch (UnauthorizedAccessException unauthorizedAccessException)
			{
			}
			catch (IOException oException)
			{
			}
			return jobRunsForDefinition;
		}

		public override IList<Job2> GetJobs()
		{
			this.RefreshRepository();
			List<Job2> job2s = new List<Job2>();
			foreach (Job2 job in ScheduledJobSourceAdapter.JobRepository.Jobs)
			{
				job2s.Add(job);
			}
			return job2s;
		}

		private void GetJobsAfter(DateTime dateTime, ref List<Job2> jobList)
		{
			bool valueOrDefault;
			foreach (Job2 job in ScheduledJobSourceAdapter.JobRepository.Jobs)
			{
				DateTime? pSEndTime = job.PSEndTime;
				DateTime dateTime1 = dateTime;
				if (pSEndTime.HasValue)
				{
					valueOrDefault = pSEndTime.GetValueOrDefault() > dateTime1;
				}
				else
				{
					valueOrDefault = false;
				}
				if (!valueOrDefault || jobList.Contains(job))
				{
					continue;
				}
				jobList.Add(job);
			}
		}

		private void GetJobsBefore(DateTime dateTime, ref List<Job2> jobList)
		{
			bool valueOrDefault;
			foreach (Job2 job in ScheduledJobSourceAdapter.JobRepository.Jobs)
			{
				DateTime? pSEndTime = job.PSEndTime;
				DateTime dateTime1 = dateTime;
				if (pSEndTime.HasValue)
				{
					valueOrDefault = pSEndTime.GetValueOrDefault() < dateTime1;
				}
				else
				{
					valueOrDefault = false;
				}
				if (!valueOrDefault || jobList.Contains(job))
				{
					continue;
				}
				jobList.Add(job);
			}
		}

		public override IList<Job2> GetJobsByCommand(string command, bool recurse)
		{
			if (!string.IsNullOrEmpty(command))
			{
				this.RefreshRepository();
				WildcardPattern wildcardPattern = new WildcardPattern(command, WildcardOptions.IgnoreCase);
				List<Job2> job2s = new List<Job2>();
				foreach (Job2 job in ScheduledJobSourceAdapter.JobRepository.Jobs)
				{
					if (!wildcardPattern.IsMatch(job.Command))
					{
						continue;
					}
					job2s.Add(job);
				}
				return job2s;
			}
			else
			{
				throw new PSArgumentException("command");
			}
		}

		public override IList<Job2> GetJobsByFilter(Dictionary<string, object> filter, bool recurse)
		{
			if (filter != null)
			{
				List<Job2> job2s = new List<Job2>();
				foreach (KeyValuePair<string, object> keyValuePair in filter)
				{
					string key = keyValuePair.Key;
					string str = key;
					if (key == null)
					{
						continue;
					}
					if (str == "Before")
					{
						this.GetJobsBefore((DateTime)keyValuePair.Value, ref job2s);
					}
					else
					{
						if (str == "After")
						{
							this.GetJobsAfter((DateTime)keyValuePair.Value, ref job2s);
						}
						else
						{
							if (str == "Newest")
							{
								this.GetNewestJobs((int)keyValuePair.Value, ref job2s);
							}
						}
					}
				}
				return job2s;
			}
			else
			{
				throw new PSArgumentNullException("filter");
			}
		}

		public override IList<Job2> GetJobsByName(string name, bool recurse)
		{
			if (!string.IsNullOrEmpty(name))
			{
				this.RefreshRepository();
				WildcardPattern wildcardPattern = new WildcardPattern(name, WildcardOptions.IgnoreCase);
				List<Job2> job2s = new List<Job2>();
				foreach (Job2 job in ScheduledJobSourceAdapter.JobRepository.Jobs)
				{
					if (!wildcardPattern.IsMatch(job.Name))
					{
						continue;
					}
					job2s.Add(job);
				}
				return job2s;
			}
			else
			{
				throw new PSArgumentException("name");
			}
		}

		public override IList<Job2> GetJobsByState(JobState state, bool recurse)
		{
			this.RefreshRepository();
			List<Job2> job2s = new List<Job2>();
			foreach (Job2 job in ScheduledJobSourceAdapter.JobRepository.Jobs)
			{
				if (state != job.JobStateInfo.State)
				{
					continue;
				}
				job2s.Add(job);
			}
			return job2s;
		}

		private void GetNewestJobs(int maxNumber, ref List<Job2> jobList)
		{
			List<Job2> jobs = ScheduledJobSourceAdapter.JobRepository.Jobs;
			List<Job2> job2s = jobs;
			job2s.Sort((Job2 firstJob, Job2 secondJob) => {
				bool valueOrDefault;
				bool flag;
				DateTime? pSEndTime = firstJob.PSEndTime;
				DateTime? nullable = secondJob.PSEndTime;
				if (pSEndTime.HasValue & nullable.HasValue)
				{
					valueOrDefault = pSEndTime.GetValueOrDefault() > nullable.GetValueOrDefault();
				}
				else
				{
					valueOrDefault = false;
				}
				if (!valueOrDefault)
				{
					DateTime? pSEndTime1 = firstJob.PSEndTime;
					DateTime? nullable1 = secondJob.PSEndTime;
					if (pSEndTime1.HasValue & nullable1.HasValue)
					{
						flag = pSEndTime1.GetValueOrDefault() < nullable1.GetValueOrDefault();
					}
					else
					{
						flag = false;
					}
					if (!flag)
					{
						return 0;
					}
					else
					{
						return 1;
					}
				}
				else
				{
					return -1;
				}
			}
			);
			int num = 0;
			foreach (Job2 job in jobs)
			{
				int num1 = num + 1;
				num = num1;
				if (num1 > maxNumber)
				{
					break;
				}
				if (jobList.Contains(job))
				{
					continue;
				}
				jobList.Add(job);
			}
		}

		internal static Job2 LoadJobFromStore(string definitionName, DateTime jobRun)
		{
			FileStream fileForJobRunItem = null;
			Exception exception = null;
			bool flag = false;
			Job2 job2 = null;
			try
			{
				try
				{
					fileForJobRunItem = ScheduledJobStore.GetFileForJobRunItem(definitionName, jobRun, ScheduledJobStore.JobRunItem.Results, FileMode.Open, FileAccess.Read, FileShare.Read);
					job2 = ScheduledJobSourceAdapter.LoadResultsFromFile(fileForJobRunItem);
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					exception = argumentException;
				}
				catch (DirectoryNotFoundException directoryNotFoundException1)
				{
					DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
					exception = directoryNotFoundException;
				}
				catch (FileNotFoundException fileNotFoundException1)
				{
					FileNotFoundException fileNotFoundException = fileNotFoundException1;
					exception = fileNotFoundException;
					flag = true;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					exception = unauthorizedAccessException;
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					exception = oException;
				}
				catch (SerializationException serializationException)
				{
					flag = true;
				}
				catch (InvalidDataContractException invalidDataContractException)
				{
					flag = true;
				}
				catch (XmlException xmlException)
				{
					flag = true;
				}
				catch (TypeInitializationException typeInitializationException)
				{
					flag = true;
				}
			}
			finally
			{
				if (fileForJobRunItem != null)
				{
					fileForJobRunItem.Close();
				}
			}
			if (flag)
			{
				ScheduledJobStore.RemoveJobRun(definitionName, jobRun);
			}
			if (exception == null)
			{
				return job2;
			}
			else
			{
				object[] objArray = new object[2];
				objArray[0] = definitionName;
				objArray[1] = jobRun;
				string str = StringUtil.Format(ScheduledJobErrorStrings.CantLoadJobRunFromStore, objArray);
				throw new ScheduledJobException(str, exception);
			}
		}

		private static Job2 LoadResultsFromFile(FileStream fs)
		{
			XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
			return (Job2)netDataContractSerializer.ReadObject(fs);
		}

		public override Job2 NewJob(JobInvocationInfo specification)
		{
			if (specification != null)
			{
				ScheduledJobDefinition scheduledJobDefinition = new ScheduledJobDefinition(specification, null, null, null);
				return new ScheduledJob(specification.Command, specification.Name, scheduledJobDefinition);
			}
			else
			{
				throw new PSArgumentNullException("specification");
			}
		}

		public override Job2 NewJob(string definitionName, string definitionPath)
		{
			if (!string.IsNullOrEmpty(definitionName))
			{
				Job2 scheduledJob = null;
				try
				{
					ScheduledJobDefinition scheduledJobDefinition = ScheduledJobDefinition.LoadFromStore(definitionName, definitionPath);
					scheduledJob = new ScheduledJob(scheduledJobDefinition.Command, scheduledJobDefinition.Name, scheduledJobDefinition);
				}
				catch (FileNotFoundException fileNotFoundException)
				{
				}
				return scheduledJob;
			}
			else
			{
				throw new PSArgumentException("definitionName");
			}
		}

		public override void PersistJob(Job2 job)
		{
			if (job != null)
			{
				ScheduledJobSourceAdapter.SaveJobToStore(job as ScheduledJob);
				return;
			}
			else
			{
				throw new PSArgumentNullException("job");
			}
		}

		private void RefreshRepository()
		{
			Collection<DateTime> jobRuns = null;
			Job2 job2;
			this.CreateFileSystemWatcher();
			IEnumerable<string> jobDefinitions = ScheduledJobStore.GetJobDefinitions();
			foreach (string str in jobDefinitions)
			{
				jobRuns = ScheduledJobSourceAdapter.GetJobRuns(str);
				if (jobRuns == null)
				{
					continue;
				}
				ScheduledJobDefinition scheduledJobDefinition = null;
				IEnumerator<DateTime> enumerator = jobRuns.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						DateTime dateTime = enumerator.Current;
						if (dateTime <= ScheduledJobSourceAdapter.JobRepository.GetLatestJobRun(str))
						{
							continue;
						}
						try
						{
							if (scheduledJobDefinition == null)
							{
								scheduledJobDefinition = ScheduledJobDefinition.LoadFromStore(str, null);
							}
							job2 = ScheduledJobSourceAdapter.LoadJobFromStore(scheduledJobDefinition.Name, dateTime);
						}
						catch (ScheduledJobException scheduledJobException)
						{
							continue;
						}
						catch (DirectoryNotFoundException directoryNotFoundException)
						{
							continue;
						}
						catch (FileNotFoundException fileNotFoundException)
						{
							continue;
						}
						catch (UnauthorizedAccessException unauthorizedAccessException)
						{
							continue;
						}
						catch (IOException oException)
						{
							continue;
						}
						ScheduledJobSourceAdapter.JobRepository.AddOrReplace(job2);
						ScheduledJobSourceAdapter.JobRepository.SetLatestJobRun(str, dateTime);
					}
				}
			}
		}

		public override void RemoveJob(Job2 job)
		{
			DateTime valueOrDefault;
			if (job != null)
			{
				this.RefreshRepository();
				try
				{
					ScheduledJobSourceAdapter.JobRepository.Remove(job);
					string name = job.Name;
					DateTime? pSBeginTime = job.PSBeginTime;
					if (pSBeginTime.HasValue)
					{
						valueOrDefault = pSBeginTime.GetValueOrDefault();
					}
					else
					{
						valueOrDefault = DateTime.MinValue;
					}
					ScheduledJobStore.RemoveJobRun(name, valueOrDefault);
				}
				catch (DirectoryNotFoundException directoryNotFoundException)
				{
				}
				catch (FileNotFoundException fileNotFoundException)
				{
				}
				return;
			}
			else
			{
				throw new PSArgumentNullException("job");
			}
		}

		internal static void SaveJobToStore(ScheduledJob job)
		{
			DateTime valueOrDefault;
			DateTime minValue;
			string outputPath = job.Definition.OutputPath;
			if (!string.IsNullOrEmpty(outputPath))
			{
				FileStream fileStream = null;
				FileStream fileStream1 = null;
				try
				{
					ScheduledJobSourceAdapter.CheckJobStoreResults(outputPath, job.Definition.ExecutionHistoryLength);
					string str = outputPath;
					DateTime? pSBeginTime = job.PSBeginTime;
					if (pSBeginTime.HasValue)
					{
						valueOrDefault = pSBeginTime.GetValueOrDefault();
					}
					else
					{
						valueOrDefault = DateTime.MinValue;
					}
					fileStream = ScheduledJobStore.CreateFileForJobRunItem(str, valueOrDefault, ScheduledJobStore.JobRunItem.Status);
					ScheduledJobSourceAdapter.SaveStatusToFile(job, fileStream);
					string str1 = outputPath;
					DateTime? nullable = job.PSBeginTime;
					if (nullable.HasValue)
					{
						minValue = nullable.GetValueOrDefault();
					}
					else
					{
						minValue = DateTime.MinValue;
					}
					fileStream1 = ScheduledJobStore.CreateFileForJobRunItem(str1, minValue, ScheduledJobStore.JobRunItem.Results);
					ScheduledJobSourceAdapter.SaveResultsToFile(job, fileStream1);
				}
				finally
				{
					if (fileStream != null)
					{
						fileStream.Close();
					}
					if (fileStream1 != null)
					{
						fileStream1.Close();
					}
				}
				return;
			}
			else
			{
				string str2 = StringUtil.Format(ScheduledJobErrorStrings.CantSaveJobNoFilePathSpecified, job.Name);
				throw new ScheduledJobException(str2);
			}
		}

		private static void SaveResultsToFile(ScheduledJob job, FileStream fs)
		{
			XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
			netDataContractSerializer.WriteObject(fs, job);
			fs.Flush();
		}

		private static void SaveStatusToFile(ScheduledJob job, FileStream fs)
		{
			StatusInfo statusInfo = new StatusInfo(job.InstanceId, job.Name, job.Location, job.Command, job.StatusMessage, job.JobStateInfo.State, job.HasMoreData, job.PSBeginTime, job.PSEndTime, job.Definition);
			XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
			netDataContractSerializer.WriteObject(fs, statusInfo);
			fs.Flush();
		}

		private static void UpdateRepositoryObjects(FileSystemEventArgs e)
		{
			string str = null;
			DateTime dateTime;
			if (ScheduledJobSourceAdapter.GetJobRunInfo(e.Name, out str, out dateTime))
			{
				ScheduledJob job = ScheduledJobSourceAdapter.JobRepository.GetJob(str, dateTime);
				if (job != null)
				{
					Job2 job2 = null;
					try
					{
						job2 = ScheduledJobSourceAdapter.LoadJobFromStore(str, dateTime);
					}
					catch (ScheduledJobException scheduledJobException)
					{
					}
					catch (DirectoryNotFoundException directoryNotFoundException)
					{
					}
					catch (FileNotFoundException fileNotFoundException)
					{
					}
					catch (UnauthorizedAccessException unauthorizedAccessException)
					{
					}
					catch (IOException oException)
					{
					}
					if (job2 != null)
					{
						job.Update(job2 as ScheduledJob);
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal class ScheduledJobRepository
		{
			private object _syncObject;

			private Dictionary<Guid, Job2> _jobs;

			private Dictionary<string, DateTime> _latestJobRuns;

			public int Count
			{
				get
				{
					int count;
					lock (this._syncObject)
					{
						count = this._jobs.Count;
					}
					return count;
				}
			}

			public List<Job2> Jobs
			{
				get
				{
					List<Job2> job2s;
					lock (this._syncObject)
					{
						job2s = new List<Job2>(this._jobs.Values);
					}
					return job2s;
				}
			}

			public ScheduledJobRepository()
			{
				this._syncObject = new object();
				this._jobs = new Dictionary<Guid, Job2>();
				this._latestJobRuns = new Dictionary<string, DateTime>();
			}

			public void Add(Job2 job)
			{
				if (job != null)
				{
					lock (this._syncObject)
					{
						if (!this._jobs.ContainsKey(job.InstanceId))
						{
							this._jobs.Add(job.InstanceId, job);
						}
						else
						{
							object[] name = new object[2];
							name[0] = job.Name;
							name[1] = job.InstanceId;
							string str = StringUtil.Format(ScheduledJobErrorStrings.ScheduledJobAlreadyExistsInLocal, name);
							throw new ScheduledJobException(str);
						}
					}
					return;
				}
				else
				{
					throw new PSArgumentNullException("job");
				}
			}

			public void AddOrReplace(Job2 job)
			{
				if (job != null)
				{
					lock (this._syncObject)
					{
						if (this._jobs.ContainsKey(job.InstanceId))
						{
							this._jobs.Remove(job.InstanceId);
						}
						this._jobs.Add(job.InstanceId, job);
					}
					return;
				}
				else
				{
					throw new PSArgumentNullException("job");
				}
			}

			public void Clear()
			{
				lock (this._syncObject)
				{
					this._jobs.Clear();
				}
			}

			public ScheduledJob GetJob(string definitionName, DateTime jobRun)
			{
				ScheduledJob scheduledJob;
				DateTime valueOrDefault;
				lock (this._syncObject)
				{
					foreach (ScheduledJob value in this._jobs.Values)
					{
						DateTime? pSBeginTime = value.PSBeginTime;
						if (!pSBeginTime.HasValue)
						{
							continue;
						}
						DateTime? nullable = value.PSBeginTime;
						if (nullable.HasValue)
						{
							valueOrDefault = nullable.GetValueOrDefault();
						}
						else
						{
							valueOrDefault = DateTime.MinValue;
						}
						DateTime dateTime = valueOrDefault;
						if (!definitionName.Equals(value.Definition.Name, StringComparison.OrdinalIgnoreCase) || jobRun.Year != dateTime.Year || jobRun.Month != dateTime.Month || jobRun.Day != dateTime.Day || jobRun.Hour != dateTime.Hour || jobRun.Minute != dateTime.Minute || jobRun.Second != dateTime.Second || jobRun.Millisecond != dateTime.Millisecond)
						{
							continue;
						}
						scheduledJob = value;
						return scheduledJob;
					}
					return null;
				}
				return scheduledJob;
			}

			public DateTime GetLatestJobRun(string definitionName)
			{
				DateTime item;
				if (!string.IsNullOrEmpty(definitionName))
				{
					lock (this._syncObject)
					{
						if (!this._latestJobRuns.ContainsKey(definitionName))
						{
							DateTime minValue = DateTime.MinValue;
							this._latestJobRuns.Add(definitionName, minValue);
							item = minValue;
						}
						else
						{
							item = this._latestJobRuns[definitionName];
						}
					}
					return item;
				}
				else
				{
					throw new PSArgumentException("definitionName");
				}
			}

			public void Remove(Job2 job)
			{
				if (job != null)
				{
					lock (this._syncObject)
					{
						if (this._jobs.ContainsKey(job.InstanceId))
						{
							this._jobs.Remove(job.InstanceId);
						}
						else
						{
							string str = StringUtil.Format(ScheduledJobErrorStrings.ScheduledJobNotInRepository, job.Name);
							throw new ScheduledJobException(str);
						}
					}
					return;
				}
				else
				{
					throw new PSArgumentNullException("job");
				}
			}

			public void SetLatestJobRun(string definitionName, DateTime jobRun)
			{
				if (!string.IsNullOrEmpty(definitionName))
				{
					lock (this._syncObject)
					{
						if (!this._latestJobRuns.ContainsKey(definitionName))
						{
							this._latestJobRuns.Add(definitionName, jobRun);
						}
						else
						{
							this._latestJobRuns.Remove(definitionName);
							this._latestJobRuns.Add(definitionName, jobRun);
						}
					}
					return;
				}
				else
				{
					throw new PSArgumentException("definitionName");
				}
			}
		}
	}
}