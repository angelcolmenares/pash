using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.WSMan;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;

namespace Microsoft.PowerShell.Workflow
{
	public sealed class WorkflowJobSourceAdapter : JobSourceAdapter
	{
		internal const string AdapterTypeName = "PSWorkflowJob";

		private readonly PowerShellTraceSource _tracer;

		private readonly Tracer _structuredTracer;

		private readonly static WorkflowJobSourceAdapter Instance;

		private readonly WorkflowJobSourceAdapter.ContainerParentJobRepository _jobRepository;

		private readonly object _syncObject;

		private bool _repositoryPopulated;

		private PSWorkflowJobManager _jobManager;

		private PSWorkflowRuntime _runtime;

		private PSWorkflowValidator _wfValidator;

		private readonly object _syncRemoveChilJob;

		internal bool IsShutdownInProgress;

		private bool _fullyLoaded;

		static WorkflowJobSourceAdapter()
		{
			WorkflowJobSourceAdapter.Instance = new WorkflowJobSourceAdapter();
		}

		internal WorkflowJobSourceAdapter()
		{
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._structuredTracer = new Tracer();
			this._jobRepository = new WorkflowJobSourceAdapter.ContainerParentJobRepository("WorkflowJobSourceAdapterRepository");
			this._syncObject = new object();
			this._syncRemoveChilJob = new object();
			WSManServerChannelEvents.add_ShuttingDown(new EventHandler(this.OnWSManServerShuttingDownEventRaised));
			base.Name = "PSWorkflowJob";
		}

		private static void AddStartParametersFromCollection(Dictionary<string, object> collection, CommandParameterCollection allParams)
		{
			if (collection == null || collection.Count <= 0)
			{
				return;
			}
			else
			{
				Dictionary<string, object> strs = collection;
				foreach (CommandParameter commandParameter in strs.Select<KeyValuePair<string, object>, CommandParameter>((KeyValuePair<string, object> o) => new CommandParameter(o.Key, o.Value)))
				{
					allParams.Add(commandParameter);
				}
				return;
			}
		}

		internal void ClearRepository()
		{
			foreach (ContainerParentJob item in this._jobRepository.GetItems())
			{
				this._jobRepository.Remove(item);
			}
		}

		internal void ClearWorkflowTable()
		{
			this.GetJobManager().ClearWorkflowManagerInstanceTable();
			this._fullyLoaded = false;
			this.LoadWorkflowInstancesFromStore();
			this.GetJobManager().ClearWorkflowManagerInstanceTable();
		}

		private IEnumerable<Job2> CreateJobsFromWorkflows(IEnumerable<Job2> workflowJobs, bool returnParents)
		{
			object obj = null;
			string str = null;
			string str1 = null;
			Guid guid;
			bool item;
			DynamicActivity workflow;
			bool flag;
			ContainerParentJob containerParentJob;
			Dictionary<Guid, Job2> guids = new Dictionary<Guid, Job2>();
			List<Job2> job2s = new List<Job2>();
			if (workflowJobs != null)
			{
				foreach (Job2 workflowJob in workflowJobs)
				{
					PSWorkflowJob pSWorkflowJob = workflowJob as PSWorkflowJob;
					PSWorkflowInstance pSWorkflowInstance = pSWorkflowJob.PSWorkflowInstance;
					if (!pSWorkflowInstance.JobStateRetrieved || pSWorkflowInstance.PSWorkflowContext.JobMetadata == null || pSWorkflowInstance.PSWorkflowContext.JobMetadata.Count == 0 || !WorkflowJobSourceAdapter.GetJobInfoFromMetadata(pSWorkflowInstance, out str1, out str, out guid) || !pSWorkflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("ParentInstanceId", out obj))
					{
						continue;
					}
					Guid guid1 = (Guid)obj;
					if (returnParents && !guids.ContainsKey(guid1))
					{
						if (!pSWorkflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("ParentName", out obj))
						{
							continue;
						}
						string str2 = (string)obj;
						if (!pSWorkflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("ParentCommand", out obj))
						{
							continue;
						}
						string str3 = (string)obj;
						JobIdentifier jobIdentifier = base.RetrieveJobIdForReuse(guid1);
						if (jobIdentifier != null)
						{
							containerParentJob = new ContainerParentJob(str3, str2, jobIdentifier, "PSWorkflowJob");
						}
						else
						{
							containerParentJob = new ContainerParentJob(str3, str2, guid1, "PSWorkflowJob");
						}
						ContainerParentJob containerParentJob1 = containerParentJob;
						if (pSWorkflowInstance.PSWorkflowContext.JobMetadata.ContainsKey("ParentSessionId"))
						{
							pSWorkflowInstance.PSWorkflowContext.JobMetadata["ParentSessionId"] = containerParentJob1.Id;
						}
						guids.Add(guid1, containerParentJob1);
					}
					if (pSWorkflowInstance.PSWorkflowContext.JobMetadata.ContainsKey("Id"))
					{
						pSWorkflowInstance.PSWorkflowContext.JobMetadata["Id"] = workflowJob.Id;
					}
					if (pSWorkflowInstance.PSWorkflowContext.JobMetadata.ContainsKey("ProcessId"))
					{
						pSWorkflowInstance.PSWorkflowContext.JobMetadata["ProcessId"] = Process.GetCurrentProcess().Id;
					}
					workflowJob.StartParameters = new List<CommandParameterCollection>();
					CommandParameterCollection commandParameterCollection = new CommandParameterCollection();
					WorkflowJobSourceAdapter.AddStartParametersFromCollection(pSWorkflowInstance.PSWorkflowContext.WorkflowParameters, commandParameterCollection);
					WorkflowJobSourceAdapter.AddStartParametersFromCollection(pSWorkflowInstance.PSWorkflowContext.PSWorkflowCommonParameters, commandParameterCollection);
					if (!pSWorkflowInstance.PSWorkflowContext.JobMetadata.ContainsKey("WorkflowTakesPrivateMetadata"))
					{
						if (pSWorkflowInstance.PSWorkflowDefinition != null)
						{
							workflow = pSWorkflowInstance.PSWorkflowDefinition.Workflow as DynamicActivity;
						}
						else
						{
							workflow = null;
						}
						DynamicActivity dynamicActivity = workflow;
						if (dynamicActivity == null)
						{
							flag = false;
						}
						else
						{
							flag = dynamicActivity.Properties.Contains("PSPrivateMetadata");
						}
						item = flag;
					}
					else
					{
						item = (bool)pSWorkflowInstance.PSWorkflowContext.JobMetadata["WorkflowTakesPrivateMetadata"];
					}
					if (pSWorkflowInstance.PSWorkflowContext.PrivateMetadata != null && pSWorkflowInstance.PSWorkflowContext.PrivateMetadata.Count > 0 && !item)
					{
						Hashtable hashtables = new Hashtable();
						foreach (KeyValuePair<string, object> privateMetadatum in pSWorkflowInstance.PSWorkflowContext.PrivateMetadata)
						{
							hashtables.Add(privateMetadatum.Key, privateMetadatum.Value);
						}
						commandParameterCollection.Add(new CommandParameter("PSPrivateMetadata", hashtables));
					}
					workflowJob.StartParameters.Add(commandParameterCollection);
					if (!returnParents)
					{
						job2s.Add(workflowJob);
					}
					else
					{
						((ContainerParentJob)guids[guid1]).AddChildJob(workflowJob);
					}
					if (pSWorkflowJob.WorkflowInstanceLoaded)
					{
						continue;
					}
					pSWorkflowJob.RestoreFromWorkflowInstance(pSWorkflowInstance);
				}
				if (returnParents)
				{
					foreach (Job2 value in guids.Values)
					{
						PSSQMAPI.InitiateWorkflowStateDataTracking(value);
					}
					job2s.AddRange(guids.Values);
				}
				return job2s;
			}
			else
			{
				return job2s;
			}
		}

		private ICollection<PSWorkflowJob> GetChildJobsFromRepository()
		{
			List<ContainerParentJob> items = this._jobRepository.GetItems();
			return items.SelectMany<ContainerParentJob, Job>((ContainerParentJob parentJob) => parentJob.ChildJobs).Cast<PSWorkflowJob>().ToList<PSWorkflowJob>();
		}

		public static WorkflowJobSourceAdapter GetInstance()
		{
			return WorkflowJobSourceAdapter.Instance;
		}

		public override Job2 GetJobByInstanceId(Guid instanceId, bool recurse)
		{
			Func<PSWorkflowJob, bool> func = null;
			object[] objArray = new object[1];
			objArray[0] = instanceId;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Getting Workflow job by instance id: {0}", objArray));
			Job2 item = this._jobRepository.GetItem(instanceId);
			if (item == null)
			{
				this.PopulateJobRepositoryIfRequired();
				item = this._jobRepository.GetItem(instanceId);
			}
			if (item == null)
			{
				if (recurse)
				{
					ICollection<PSWorkflowJob> childJobsFromRepository = this.GetChildJobsFromRepository();
					if (func == null)
					{
						func = (PSWorkflowJob job) => job.InstanceId == instanceId;
					}
					item = childJobsFromRepository.FirstOrDefault<PSWorkflowJob>(func);
				}
				return item;
			}
			else
			{
				return item;
			}
		}

		public override Job2 GetJobBySessionId(int id, bool recurse)
		{
			WorkflowJobSourceAdapter.WorkflowJobSourceAdapter variable = null;
			Func<PSWorkflowJob, bool> func = null;
			object[] objArray = new object[1];
			objArray[0] = id;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Getting Workflow job by session id: {0}", objArray));
			this.PopulateJobRepositoryIfRequired();
			Job2 job2 = this._jobRepository.GetItems().FirstOrDefault<ContainerParentJob>((ContainerParentJob job) => job.Id == id);
			if (job2 == null)
			{
				if (recurse)
				{
					ICollection<PSWorkflowJob> childJobsFromRepository = this.GetChildJobsFromRepository();
					if (func == null)
					{
						func = (PSWorkflowJob job) => job.Id == this.id;
					}
					job2 = childJobsFromRepository.FirstOrDefault<PSWorkflowJob>(func);
				}
				return job2;
			}
			else
			{
				return job2;
			}
		}

		internal static bool GetJobInfoFromMetadata(PSWorkflowInstance workflowInstance, out string command, out string name, out Guid instanceId)
		{
			object obj = null;
			bool flag = false;
			command = string.Empty;
			name = string.Empty;
			instanceId = Guid.Empty;
			if (workflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("Name", out obj))
			{
				name = (string)obj;
				if (workflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("Command", out obj))
				{
					command = (string)obj;
					if (workflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("InstanceId", out obj))
					{
						instanceId = (Guid)obj;
						flag = true;
					}
				}
			}
			return flag;
		}

		public PSWorkflowJobManager GetJobManager()
		{
			PSWorkflowJobManager pSWorkflowJobManager;
			if (this._jobManager == null)
			{
				lock (this._syncObject)
				{
					if (this._jobManager == null)
					{
						this._jobManager = PSWorkflowRuntime.Instance.JobManager;
						return this._jobManager;
					}
					else
					{
						pSWorkflowJobManager = this._jobManager;
					}
				}
				return pSWorkflowJobManager;
			}
			else
			{
				return this._jobManager;
			}
		}

		public override IList<Job2> GetJobs()
		{
			this._tracer.WriteMessage("WorkflowJobSourceAdapter: Getting all Workflow jobs");
			this.PopulateJobRepositoryIfRequired();
			return new List<Job2>(this._jobRepository.GetItems());
		}

		public override IList<Job2> GetJobsByCommand(string command, bool recurse)
		{
			object[] objArray = new object[1];
			objArray[0] = command;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Getting Workflow jobs by command: {0}", objArray));
			this.PopulateJobRepositoryIfRequired();
			List<Job2> job2s = new List<Job2>();
			WildcardPattern wildcardPattern = new WildcardPattern(command, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
			List<Job2> list = this._jobRepository.GetItems().Where<ContainerParentJob>((ContainerParentJob parentJob) => wildcardPattern.IsMatch(parentJob.Command)).Cast<Job2>().ToList<Job2>();
			if (list.Count > 0)
			{
				job2s.AddRange(list);
			}
			if (recurse)
			{
				Dictionary<string, object> strs = new Dictionary<string, object>();
				strs.Add("ParentCommand", wildcardPattern);
				Dictionary<string, object> strs1 = strs;
				IEnumerable<Job2> jobs = this.GetJobManager().GetJobs(this.GetChildJobsFromRepository(), WorkflowFilterTypes.JobMetadata, strs1);
				job2s.AddRange(jobs);
			}
			return job2s;
		}

		public override IList<Job2> GetJobsByFilter(Dictionary<string, object> filter, bool recurse)
		{
			if (filter != null)
			{
				object[] objArray = new object[1];
				objArray[0] = filter;
				this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Getting Workflow jobs by filter: {0}", objArray));
				this.PopulateJobRepositoryIfRequired();
				Dictionary<string, object> strs = new Dictionary<string, object>(filter, StringComparer.CurrentCultureIgnoreCase);
				bool flag = false;
				bool flag1 = true;
				if (strs.Keys.Count != 0)
				{
					var keys = strs.Keys;
					if (keys.Any<string>((string key) => {
						if (key.Equals("Id", StringComparison.OrdinalIgnoreCase) || key.Equals("InstanceId", StringComparison.OrdinalIgnoreCase) || key.Equals("Name", StringComparison.OrdinalIgnoreCase) || key.Equals("Command", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						else
						{
							return !key.Equals("State", StringComparison.OrdinalIgnoreCase);
						}
					}
					))
					{
						flag1 = false;
					}
				}
				else
				{
					flag1 = false;
				}
				List<Job2> job2s = new List<Job2>();
				if (flag1)
				{
					List<ContainerParentJob> items = this._jobRepository.GetItems();
					List<Job2> job2s1 = WorkflowJobSourceAdapter.SearchJobsOnV2Parameters(items, strs);
					items.Clear();
					if (job2s1.Count > 0)
					{
						job2s.AddRange(job2s1);
					}
				}
				if (recurse)
				{
					if (strs.ContainsKey("Id"))
					{
						flag = true;
					}
					if (flag)
					{
						strs.Add("ProcessId", Process.GetCurrentProcess().Id);
					}
					if (strs.ContainsKey("State"))
					{
						strs.Remove("State");
					}
					this.LoadWorkflowInstancesFromStore();
					IEnumerable<Job2> jobs = this.GetJobManager().GetJobs(WorkflowFilterTypes.All, strs);
					if (!filter.ContainsKey("State"))
					{
						job2s.AddRange(jobs);
					}
					else
					{
						List<Job2> list = jobs.Where<Job2>((Job2 job) => job.JobStateInfo.State == (JobState)LanguagePrimitives.ConvertTo(filter["State"], typeof(JobState), CultureInfo.InvariantCulture)).ToList<Job2>();
						job2s.AddRange(list);
					}
				}
				List<Job2> job2s2 = new List<Job2>();
				foreach (Job2 job2 in job2s)
				{
					if (job2 as ContainerParentJob == null || job2s2.Contains(job2))
					{
						PSWorkflowJob pSWorkflowJob = job2 as PSWorkflowJob;
						ContainerParentJob item = this._jobRepository.GetItem((Guid)pSWorkflowJob.JobMetadata["ParentInstanceId"]);
						if (job2s2.Contains(item))
						{
							continue;
						}
						job2s2.Add(item);
					}
					else
					{
						job2s2.Add(job2);
					}
				}
				return job2s2;
			}
			else
			{
				throw new ArgumentNullException("filter");
			}
		}

		public override IList<Job2> GetJobsByName(string name, bool recurse)
		{
			object[] objArray = new object[1];
			objArray[0] = name;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Getting Workflow jobs by name: {0}", objArray));
			this.PopulateJobRepositoryIfRequired();
			WildcardPattern wildcardPattern = new WildcardPattern(name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
			List<Job2> job2s = new List<Job2>();
			List<Job2> list = this._jobRepository.GetItems().Where<ContainerParentJob>((ContainerParentJob parentJob) => wildcardPattern.IsMatch(parentJob.Name)).Cast<Job2>().ToList<Job2>();
			job2s.AddRange(list);
			if (recurse)
			{
				Dictionary<string, object> strs = new Dictionary<string, object>();
				strs.Add("Name", wildcardPattern);
				Dictionary<string, object> strs1 = strs;
				IEnumerable<Job2> jobs = this.GetJobManager().GetJobs(this.GetChildJobsFromRepository(), WorkflowFilterTypes.JobMetadata, strs1);
				job2s.AddRange(jobs);
			}
			return job2s;
		}

		public override IList<Job2> GetJobsByState(JobState state, bool recurse)
		{
			List<Job2> list;
			object[] objArray = new object[1];
			objArray[0] = state;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Getting Workflow jobs by state: {0}", objArray));
			this.PopulateJobRepositoryIfRequired();
			if (recurse)
			{
				list = this.GetChildJobsFromRepository().Where<PSWorkflowJob>((PSWorkflowJob job) => job.JobStateInfo.State == this.state).Cast<Job2>().ToList<Job2>();
			}
			else
			{
				list = this._jobRepository.GetItems().Where<ContainerParentJob>((ContainerParentJob job) => job.JobStateInfo.State == state).Cast<Job2>().ToList<Job2>();
			}
			List<Job2> job2s = list;
			return job2s;
		}

		public PSWorkflowRuntime GetPSWorkflowRuntime()
		{
			PSWorkflowRuntime pSWorkflowRuntime;
			if (this._runtime == null)
			{
				lock (this._syncObject)
				{
					if (this._runtime == null)
					{
						this._runtime = PSWorkflowRuntime.Instance;
						return this._runtime;
					}
					else
					{
						pSWorkflowRuntime = this._runtime;
					}
				}
				return pSWorkflowRuntime;
			}
			else
			{
				return this._runtime;
			}
		}

		internal PSWorkflowValidator GetWorkflowValidator()
		{
			PSWorkflowValidator pSWorkflowValidator;
			if (this._wfValidator == null)
			{
				lock (this._syncObject)
				{
					if (this._wfValidator == null)
					{
						this._wfValidator = new PSWorkflowValidator(PSWorkflowRuntime.Instance.Configuration);
						return this._wfValidator;
					}
					else
					{
						pSWorkflowValidator = this._wfValidator;
					}
				}
				return pSWorkflowValidator;
			}
			else
			{
				return this._wfValidator;
			}
		}

		internal void LoadWorkflowInstancesFromStore()
		{
			if (!this._fullyLoaded)
			{
				lock (this._syncObject)
				{
					if (!this._fullyLoaded)
					{
						foreach (PSWorkflowId allWorkflowInstanceId in PSWorkflowFileInstanceStore.GetAllWorkflowInstanceIds())
						{
							try
							{
								this.GetJobManager().LoadJobWithIdentifier(allWorkflowInstanceId);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this._tracer.WriteMessage("Getting an exception while loading the previously persisted workflows...");
								this._tracer.TraceException(exception);
							}
						}
						this._fullyLoaded = true;
					}
					else
					{
						return;
					}
				}
				this.GetJobManager().CleanUpWorkflowJobTable();
				return;
			}
			else
			{
				return;
			}
		}

		public override Job2 NewJob(JobInvocationInfo specification)
		{
			bool hasValue;
			if (specification != null)
			{
				if (specification.Definition != null)
				{
					if (specification.Definition.JobSourceAdapterType == base.GetType())
					{
						if (specification.Parameters.Count == 0)
						{
							specification.Parameters.Add(new CommandParameterCollection());
						}
						bool? nullable = null;
						Activity activity = this.ValidateWorkflow(specification, null, ref nullable);
						ContainerParentJob containerParentJob = this.GetJobManager().CreateJob(specification, activity);
						if (!PSSessionConfigurationData.IsServerManager)
						{
							foreach (PSWorkflowJob childJob in containerParentJob.ChildJobs)
							{
								bool? item = null;
								PSWorkflowContext pSWorkflowContext = childJob.PSWorkflowInstance.PSWorkflowContext;
								if (pSWorkflowContext != null && pSWorkflowContext.PSWorkflowCommonParameters != null && pSWorkflowContext.PSWorkflowCommonParameters.ContainsKey("PSPersist"))
								{
									item = (bool?)(pSWorkflowContext.PSWorkflowCommonParameters["PSPersist"] as bool?);
								}
								if (item.HasValue)
								{
									bool? nullable1 = item;
									if (nullable1.GetValueOrDefault())
									{
										hasValue = false;
									}
									else
									{
										hasValue = nullable1.HasValue;
									}
									if (!hasValue)
									{
										continue;
									}
								}
								if (!nullable.HasValue || nullable.Value)
								{
									continue;
								}
								childJob.Warning.Add(new WarningRecord(Resources.WarningMessageForPersistence));
								childJob.IsSuspendable = nullable;
							}
						}
						base.StoreJobIdForReuse(containerParentJob, true);
						this._jobRepository.Add(containerParentJob);
						return containerParentJob;
					}
					else
					{
						throw new InvalidOperationException(Resources.NewJobWrongType);
					}
				}
				else
				{
					throw new ArgumentException(Resources.NewJobDefinitionNull, "specification");
				}
			}
			else
			{
				throw new ArgumentNullException("specification");
			}
		}

		private void OnWSManServerShuttingDownEventRaised(object sender, EventArgs e)
		{
			try
			{
				this.IsShutdownInProgress = true;
				PSWorkflowConfigurationProvider configuration = PSWorkflowRuntime.Instance.Configuration;
				int workflowShutdownTimeoutMSec = configuration.WorkflowShutdownTimeoutMSec;
				this.GetJobManager().ShutdownWorkflowManager(workflowShutdownTimeoutMSec);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				object[] objArray = new object[1];
				objArray[0] = exception;
				this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Shutting down WSMan server: Exception details: {0}", objArray));
			}
		}

		private void PopulateJobRepositoryIfRequired()
		{
			if (!this._repositoryPopulated)
			{
				lock (this._syncObject)
				{
					if (!this._repositoryPopulated)
					{
						this._repositoryPopulated = true;
						this.LoadWorkflowInstancesFromStore();
						foreach (Job2 job2 in this.CreateJobsFromWorkflows(this.GetJobManager().GetJobs(), true))
						{
							try
							{
								this._jobRepository.Add((ContainerParentJob)job2);
							}
							catch (ArgumentException argumentException)
							{
							}
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

		internal void RemoveChildJob(Job2 childWorkflowJob)
		{
			object obj = null;
			this._structuredTracer.RemoveJobStarted(childWorkflowJob.InstanceId);
			this.PopulateJobRepositoryIfRequired();
			object[] instanceId = new object[1];
			instanceId[0] = childWorkflowJob.InstanceId;
			this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Removing Workflow job with instance id: {0}", instanceId));
			lock (this._syncRemoveChilJob)
			{
				PSWorkflowJob pSWorkflowJob = childWorkflowJob as PSWorkflowJob;
				if (pSWorkflowJob != null)
				{
					PSWorkflowInstance pSWorkflowInstance = pSWorkflowJob.PSWorkflowInstance;
					if (pSWorkflowInstance.PSWorkflowContext.JobMetadata.TryGetValue("ParentInstanceId", out obj))
					{
						Guid guid = (Guid)obj;
						ContainerParentJob item = this._jobRepository.GetItem(guid);
						item.ChildJobs.Remove(pSWorkflowJob);
						try
						{
							this.GetJobManager().RemoveJob(pSWorkflowJob.InstanceId);
							this._structuredTracer.JobRemoved(item.InstanceId, pSWorkflowJob.InstanceId, pSWorkflowJob.WorkflowGuid);
						}
						catch (ArgumentException argumentException1)
						{
							ArgumentException argumentException = argumentException1;
							object[] objArray = new object[1];
							objArray[0] = argumentException;
							this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Ingnoring the exception. Exception details: {0}", objArray));
							this._structuredTracer.JobRemoveError(item.InstanceId, pSWorkflowJob.InstanceId, pSWorkflowJob.WorkflowGuid, argumentException.Message);
						}
						if (item.ChildJobs.Count == 0)
						{
							try
							{
								this._jobRepository.Remove(item);
							}
							catch (ArgumentException argumentException3)
							{
								ArgumentException argumentException2 = argumentException3;
								object[] objArray1 = new object[1];
								objArray1[0] = argumentException2;
								this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Ingnoring the exception. Exception details: {0}", objArray1));
							}
							item.Dispose();
						}
					}
				}
			}
		}

		public override void RemoveJob(Job2 job)
		{
			if (job != null)
			{
				this._structuredTracer.RemoveJobStarted(job.InstanceId);
				ContainerParentJob item = this._jobRepository.GetItem(job.InstanceId);
				if (item == null)
				{
					this.PopulateJobRepositoryIfRequired();
				}
				if (job as ContainerParentJob != null)
				{
					object[] instanceId = new object[1];
					instanceId[0] = job.InstanceId;
					this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Removing Workflow job with instance id: {0}", instanceId));
					Exception exception = null;
					foreach (Job childJob in job.ChildJobs)
					{
						PSWorkflowJob pSWorkflowJob = childJob as PSWorkflowJob;
						if (pSWorkflowJob == null)
						{
							continue;
						}
						try
						{
							this.GetJobManager().RemoveJob(pSWorkflowJob.InstanceId);
							this._structuredTracer.JobRemoved(job.InstanceId, childJob.InstanceId, pSWorkflowJob.WorkflowGuid);
						}
						catch (ArgumentException argumentException1)
						{
							ArgumentException argumentException = argumentException1;
							object[] objArray = new object[1];
							objArray[0] = argumentException;
							this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Ingnoring the exception. Exception details: {0}", objArray));
							exception = argumentException;
							this._structuredTracer.JobRemoveError(job.InstanceId, childJob.InstanceId, pSWorkflowJob.WorkflowGuid, argumentException.Message);
						}
					}
					try
					{
						this._jobRepository.Remove((ContainerParentJob)job);
					}
					catch (ArgumentException argumentException3)
					{
						ArgumentException argumentException2 = argumentException3;
						object[] objArray1 = new object[1];
						objArray1[0] = argumentException2;
						this._tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Ingnoring the exception. Exception details: {0}", objArray1));
						exception = argumentException2;
					}
					job.Dispose();
					if (exception != null)
					{
						new ArgumentException(Resources.WorkflowChildCouldNotBeRemoved, "job", exception);
					}
					return;
				}
				else
				{
					throw new InvalidOperationException(Resources.CannotRemoveWorkflowJobDirectly);
				}
			}
			else
			{
				throw new ArgumentNullException("job");
			}
		}

		internal static List<Job2> SearchJobsOnV2Parameters(IEnumerable<Job2> jobsToSearch, IDictionary<string, object> filter)
		{
			List<Job2> list;
			List<Job2> job2s = new List<Job2>();
			job2s.AddRange(jobsToSearch);
			if (filter.ContainsKey("Id"))
			{
				list = job2s.Where<Job2>((Job2 job) => job.Id == (int)filter["Id"]).ToList<Job2>();
				job2s.Clear();
				job2s = list;
			}
			if (filter.ContainsKey("InstanceId"))
			{
				object item = filter["InstanceId"];
				LanguagePrimitives.TryConvertTo<Guid>(item, CultureInfo.InvariantCulture, out LambdaVar2);
				list = job2s.Where<Job2>((Job2 job) => job.InstanceId == LambdaVar2).ToList<Job2>();
				job2s.Clear();
				job2s = list;
			}
			if (filter.ContainsKey("State"))
			{
				list = job2s.Where<Job2>((Job2 job) => job.JobStateInfo.State == (JobState)LanguagePrimitives.ConvertTo(filter["State"], typeof(JobState), CultureInfo.InvariantCulture)).ToList<Job2>();
				job2s.Clear();
				job2s = list;
			}
			if (filter.ContainsKey("Name"))
			{
				if (filter["Name"] as string == null)
				{
					WildcardPattern wildcardPattern = (WildcardPattern)filter["Name"];
				}
				else
				{
					string str = (string)filter["Name"];
					wildcardPattern = new WildcardPattern(str, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
				}
				list = job2s.Where<Job2>((Job2 parentJob) => wildcardPattern.IsMatch(parentJob.Name)).ToList<Job2>();
				job2s.Clear();
				job2s = list;
			}
			if (filter.ContainsKey("Command"))
			{
				if (filter["Command"] as string == null)
				{
					WildcardPattern item1 = (WildcardPattern)filter["Command"];
				}
				else
				{
					string str1 = (string)filter["Command"];
					item1 = new WildcardPattern(str1, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
				}
				list = job2s.Where<Job2>((Job2 parentJob) => item1.IsMatch(parentJob.Command)).ToList<Job2>();
				job2s.Clear();
				job2s = list;
			}
			return job2s;
		}

		private Activity ValidateWorkflow(JobInvocationInfo invocationInfo, Activity activity, ref bool? isSuspendable)
		{
			bool flag = false;
			PSWorkflowRuntime instance = PSWorkflowRuntime.Instance;
			JobDefinition definition = invocationInfo.Definition;
			WorkflowJobDefinition workflowJobDefinition = WorkflowJobDefinition.AsWorkflowJobDefinition(definition);
			bool flag1 = true;
			if (activity == null)
			{
				activity = DefinitionCache.Instance.GetActivityFromCache(workflowJobDefinition, out flag);
				if (activity != null)
				{
					flag1 = false;
					if (flag)
					{
						return activity;
					}
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			if (instance.Configuration.EnableValidation)
			{
				if (!flag1 || DefinitionCache.Instance.AllowExternalActivity)
				{
					PSWorkflowValidationResults pSWorkflowValidationResult = this.GetWorkflowValidator().ValidateWorkflow(definition.InstanceId, activity, DefinitionCache.Instance.GetRuntimeAssemblyName(workflowJobDefinition));
					if (pSWorkflowValidationResult.Results != null)
					{
						this.GetWorkflowValidator().ProcessValidationResults(pSWorkflowValidationResult.Results);
					}
					isSuspendable = new bool?(pSWorkflowValidationResult.IsWorkflowSuspendable);
					return activity;
				}
				else
				{
					if (Validation.CustomHandler != null)
					{
						if (!Validation.CustomHandler(activity))
						{
							string displayName = activity.DisplayName;
							if (string.IsNullOrEmpty(displayName))
							{
								displayName = base.GetType().Name;
							}
							object[] objArray = new object[1];
							objArray[0] = displayName;
							string str = string.Format(CultureInfo.CurrentCulture, Resources.InvalidActivity, objArray);
							throw new ValidationException(str);
						}
						else
						{
							return activity;
						}
					}
					else
					{
						throw new InvalidOperationException();
					}
				}
			}
			else
			{
				return activity;
			}
		}

		private class ContainerParentJobRepository : Repository<ContainerParentJob>
		{
			internal ContainerParentJobRepository(string identifier) : base(identifier)
			{
			}

			protected override Guid GetKey(ContainerParentJob item)
			{
				return item.InstanceId;
			}
		}
	}
}