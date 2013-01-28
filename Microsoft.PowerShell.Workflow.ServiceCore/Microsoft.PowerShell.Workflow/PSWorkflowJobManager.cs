using Microsoft.PowerShell.Commands;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	public sealed class PSWorkflowJobManager
	{
		private const string Facility = "WorkflowManager : ";

		private const int WaitInterval = 0x1388;

		private const int GcDelayInMinutes = 5;

		private const int InProgress = 1;

		private const int NotInProgress = 0;

		private const int WorkflowLimitBeforeGc = 125;

		private readonly static PowerShellTraceSource Tracer;

		private readonly static Tracer StructuredTracer;

		private readonly PSWorkflowRuntime _runtime;

		private LockObjectsCollection lockObjects;

		private readonly ConcurrentQueue<Tuple<Action<object>, object, JobState>> _pendingQueue;

		private readonly static PSPerfCountersMgr PerfCountersMgr;

		private readonly int _throttleLimit;

		private int _inProgressCount;

		private static DateTime _lastGcTime;

		private static int _workflowsBeforeGc;

		private static int _gcStatus;

		private readonly static Tracer etwTracer;

		internal static bool TestMode;

		internal static long ObjectCounter;

		private readonly object _servicingThreadSyncObject;

		private bool _needToStartServicingThread;

		private readonly Lazy<AutoResetEvent> _waitForJobs;

		private readonly static int CurrentProcessId;

		private readonly ConcurrentDictionary<Guid, PSWorkflowJob> _wfJobTable;

		private bool NeedToStartServicingThread
		{
			get
			{
				bool flag;
				if (this._needToStartServicingThread)
				{
					lock (this._servicingThreadSyncObject)
					{
						if (!this._needToStartServicingThread)
						{
							return false;
						}
						else
						{
							this._needToStartServicingThread = false;
							flag = true;
						}
					}
					return flag;
				}
				return false;
			}
		}

		static PSWorkflowJobManager()
		{
			PSWorkflowJobManager.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			PSWorkflowJobManager.StructuredTracer = new Tracer();
			PSWorkflowJobManager.PerfCountersMgr = PSPerfCountersMgr.Instance;
			PSWorkflowJobManager._lastGcTime = new DateTime(0x7db, 1, 1);
			PSWorkflowJobManager._gcStatus = 0;
			PSWorkflowJobManager.etwTracer = new Tracer();
			PSWorkflowJobManager.TestMode = false;
			PSWorkflowJobManager.ObjectCounter = (long)0;
			PSWorkflowJobManager.CurrentProcessId = Process.GetCurrentProcess().Id;
		}

		public PSWorkflowJobManager(PSWorkflowRuntime runtime, int throttleLimit)
		{
			this.lockObjects = new LockObjectsCollection();
			this._pendingQueue = new ConcurrentQueue<Tuple<Action<object>, object, JobState>>();
			this._servicingThreadSyncObject = new object();
			this._needToStartServicingThread = true;
			PSWorkflowJobManager lazy = this;
			lazy._waitForJobs = new Lazy<AutoResetEvent>(() => new AutoResetEvent(false));
			this._wfJobTable = new ConcurrentDictionary<Guid, PSWorkflowJob>();
			if (runtime != null)
			{
				if (PSWorkflowJobManager.TestMode)
				{
					Interlocked.Increment(ref PSWorkflowJobManager.ObjectCounter);
				}
				this._runtime = runtime;
				this._throttleLimit = throttleLimit;
				return;
			}
			else
			{
				throw new ArgumentNullException("runtime");
			}
		}

		private void AddJob(PSWorkflowJob job)
		{
			if (!this._wfJobTable.ContainsKey(job.InstanceId))
			{
				this._wfJobTable.TryAdd(job.InstanceId, job);
				return;
			}
			else
			{
				ArgumentException argumentException = new ArgumentException(Resources.DuplicateInstanceId);
				PSWorkflowJobManager.Tracer.TraceException(argumentException);
				throw argumentException;
			}
		}

		private void CheckAndStartServicingThread()
		{
			if (this._inProgressCount < this._throttleLimit && this._pendingQueue.Count > 0)
			{
				this._waitForJobs.Value.Set();
			}
			if (this.NeedToStartServicingThread)
			{
				Thread thread = new Thread(new ThreadStart(this.StartOperationsFromQueue));
				thread.Name = "Job Throttling Thread";
				Thread thread1 = thread;
				thread1.IsBackground = true;
				thread1.Start();
				return;
			}
			else
			{
				return;
			}
		}

		internal void CleanUpWorkflowJobTable()
		{
			PSWorkflowJob pSWorkflowJob = null;
			List<PSWorkflowJob> pSWorkflowJobs = new List<PSWorkflowJob>(this._wfJobTable.Values);
			foreach (PSWorkflowJob pSWorkflowJob1 in pSWorkflowJobs)
			{
				if (pSWorkflowJob1.PSWorkflowInstance.PSWorkflowContext.JobMetadata != null && pSWorkflowJob1.PSWorkflowInstance.PSWorkflowContext.JobMetadata.Count != 0)
				{
					continue;
				}
				this._wfJobTable.TryRemove(pSWorkflowJob1.InstanceId, out pSWorkflowJob);
				if (pSWorkflowJob == null)
				{
					continue;
				}
				pSWorkflowJob.Dispose();
			}
		}

		internal void ClearWorkflowManagerInstanceTable()
		{
			foreach (PSWorkflowJob value in this._wfJobTable.Values)
			{
				value.Dispose();
			}
			this._wfJobTable.Clear();
		}

		private void CreateChildJob(JobInvocationInfo specification, Activity activity, ContainerParentJob newJob, CommandParameterCollection commandParameterCollection, Dictionary<string, object> parameterDictionary, string computerName, string[] computerNames)
		{
			if (!string.IsNullOrEmpty(computerName))
			{
				string[] strArrays = new string[1];
				strArrays[0] = computerName;
				string[] strArrays1 = strArrays;
				parameterDictionary["PSComputerName"] = strArrays1;
			}
			JobInvocationInfo jobInvocationInfo = new JobInvocationInfo(specification.Definition, parameterDictionary);
			PSWorkflowJob pSWorkflowJob = new PSWorkflowJob(this._runtime, jobInvocationInfo);
			pSWorkflowJob.JobMetadata = PSWorkflowJobManager.CreateJobMetadata(pSWorkflowJob, newJob.InstanceId, newJob.Id, newJob.Name, newJob.Command, computerNames);
			int num = 0;
			while (num < commandParameterCollection.Count)
			{
				if (!string.Equals(commandParameterCollection[num].Name, "PSComputerName", StringComparison.OrdinalIgnoreCase))
				{
					num++;
				}
				else
				{
					commandParameterCollection.RemoveAt(num);
					break;
				}
			}
			if (!string.IsNullOrEmpty(computerName))
			{
				CommandParameter commandParameter = new CommandParameter("PSComputerName", computerName);
				commandParameterCollection.Add(commandParameter);
			}
			this.AddJob(pSWorkflowJob);
			pSWorkflowJob.LoadWorkflow(commandParameterCollection, activity, null);
			newJob.AddChildJob(pSWorkflowJob);
			PSWorkflowJobManager.StructuredTracer.ChildWorkflowJobAddition(pSWorkflowJob.InstanceId, newJob.InstanceId);
			PSWorkflowJobManager.Tracer.TraceJob(pSWorkflowJob);
			PSWorkflowJobManager.StructuredTracer.WorkflowJobCreated(newJob.InstanceId, pSWorkflowJob.InstanceId, pSWorkflowJob.WorkflowGuid);
		}

		public PSWorkflowJob CreateJob(Guid jobInstanceId, Activity workflow, string command, string name, Dictionary<string, object> parameters)
		{
			return this.CreateJobInternal(jobInstanceId, workflow, command, name, parameters, null);
		}

		public PSWorkflowJob CreateJob(Guid jobInstanceId, string workflowXaml, string command, string name, Dictionary<string, object> parameters)
		{
			Activity activity = ImportWorkflowCommand.ConvertXamlToActivity(workflowXaml);
			return this.CreateJobInternal(jobInstanceId, activity, command, name, parameters, workflowXaml);
		}

		internal ContainerParentJob CreateJob(JobInvocationInfo jobInvocationInfo, Activity activity)
		{
			object obj = null;
			if (jobInvocationInfo != null)
			{
				if (jobInvocationInfo.Definition != null)
				{
					if (jobInvocationInfo.Command != null)
					{
						DynamicActivity dynamicActivity = activity as DynamicActivity;
						object[] instanceId = new object[1];
						instanceId[0] = jobInvocationInfo.Definition.InstanceId;
						PSWorkflowJobManager.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "WorkflowJobSourceAdapter: Creating Workflow job with definition: {0}", instanceId));
						ContainerParentJob containerParentJob = new ContainerParentJob(jobInvocationInfo.Command, jobInvocationInfo.Name, "PSWorkflowJob");
						foreach (CommandParameterCollection commandParameterCollection in commandParameterCollection)
						{
							Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
							IEnumerator<CommandParameter> enumerator = commandParameterCollection.GetEnumerator();
							using (enumerator)
							{
								while (enumerator.MoveNext())
								{
									CommandParameter commandParameter = commandParameterCollection;
									strs.Add(commandParameter.Name, commandParameter.Value);
								}
							}
							string[] strArrays = null;
							bool flag = false;
							if (strs.Count != 0 && strs.TryGetValue("PSComputerName", out obj) && LanguagePrimitives.TryConvertTo<string[]>(obj, CultureInfo.InvariantCulture, out strArrays))
							{
								flag = strArrays != null;
							}
							PSWorkflowJobManager.StructuredTracer.ParentJobCreated(containerParentJob.InstanceId);
							bool flag1 = false;
							if (dynamicActivity != null && dynamicActivity.Properties.Contains("PSComputerName"))
							{
								flag1 = true;
							}
							dynamicActivity = null;
							if (!flag1)
							{
								strs.Remove("PSComputerName");
								if (!flag)
								{
									this.CreateChildJob(jobInvocationInfo, activity, containerParentJob, commandParameterCollection, strs, null, strArrays);
								}
								else
								{
									string[] strArrays1 = strArrays;
									for (int i = 0; i < (int)strArrays1.Length; i++)
									{
										string str = strArrays1[i];
										this.CreateChildJob(jobInvocationInfo, activity, containerParentJob, commandParameterCollection, strs, str, strArrays);
									}
								}
							}
							else
							{
								JobInvocationInfo command = new JobInvocationInfo(jobInvocationInfo.Definition, strs);
								command.Command = containerParentJob.Command;
								if (flag)
								{
									CommandParameter commandParameter1 = new CommandParameter("PSComputerName", strArrays);
									command.Parameters[0].Add(commandParameter1);
								}
								PSWorkflowJob pSWorkflowJob = new PSWorkflowJob(this._runtime, command);
								pSWorkflowJob.JobMetadata = PSWorkflowJobManager.CreateJobMetadata(pSWorkflowJob, containerParentJob.InstanceId, containerParentJob.Id, containerParentJob.Name, containerParentJob.Command, strArrays);
								pSWorkflowJob.LoadWorkflow(commandParameterCollection, activity, null);
								this.AddJob(pSWorkflowJob);
								containerParentJob.AddChildJob(pSWorkflowJob);
								PSWorkflowJobManager.StructuredTracer.ChildWorkflowJobAddition(pSWorkflowJob.InstanceId, containerParentJob.InstanceId);
								PSWorkflowJobManager.StructuredTracer.WorkflowJobCreated(containerParentJob.InstanceId, pSWorkflowJob.InstanceId, pSWorkflowJob.WorkflowGuid);
							}
						}
						PSWorkflowJobManager.StructuredTracer.JobCreationComplete(containerParentJob.InstanceId, jobInvocationInfo.InstanceId);
						PSWorkflowJobManager.Tracer.TraceJob(containerParentJob);
						PSSQMAPI.InitiateWorkflowStateDataTracking(containerParentJob);
						return containerParentJob;
					}
					else
					{
						throw new ArgumentException(Resources.NewJobDefinitionNull, "jobInvocationInfo");
					}
				}
				else
				{
					throw new ArgumentException(Resources.NewJobDefinitionNull, "jobInvocationInfo");
				}
			}
			else
			{
				throw new ArgumentNullException("jobInvocationInfo");
			}
		}

		internal PSWorkflowJob CreateJobInternal(Guid jobInstanceId, Activity workflow, string command, string name, Dictionary<string, object> parameters, string xaml)
		{
			object obj = null;
			PSWorkflowJob pSWorkflowJob;
			if (jobInstanceId != Guid.Empty)
			{
				if (workflow != null)
				{
					if (command != null)
					{
						if (name != null)
						{
							if (!this._wfJobTable.ContainsKey(jobInstanceId))
							{
								lock (this.lockObjects.GetLockObject(jobInstanceId))
								{
									if (!this._wfJobTable.ContainsKey(jobInstanceId))
									{
										JobDefinition jobDefinition = new JobDefinition(typeof(WorkflowJobSourceAdapter), command, name);
										Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
										if (parameters != null)
										{
											foreach (KeyValuePair<string, object> parameter in parameters)
											{
												strs.Add(parameter.Key, parameter.Value);
											}
										}
										string[] strArrays = null;
										bool flag = false;
										if (strs.Count != 0 && strs.TryGetValue("PSComputerName", out obj) && LanguagePrimitives.TryConvertTo<string[]>(obj, CultureInfo.InvariantCulture, out strArrays))
										{
											flag = strArrays != null;
										}
										if (flag)
										{
											if ((int)strArrays.Length <= 1)
											{
												strs.Remove("PSComputerName");
											}
											else
											{
												throw new ArgumentException(Resources.OneComputerNameAllowed);
											}
										}
										JobInvocationInfo jobInvocationInfo = new JobInvocationInfo(jobDefinition, strs);
										jobInvocationInfo.Command = command;
										if (flag)
										{
											CommandParameter commandParameter = new CommandParameter("PSComputerName", strArrays);
											jobInvocationInfo.Parameters[0].Add(commandParameter);
										}
										PSWorkflowJob pSWorkflowJob1 = new PSWorkflowJob(this._runtime, jobInvocationInfo, jobInstanceId);
										pSWorkflowJob1.JobMetadata = PSWorkflowJobManager.CreateJobMetadataWithNoParentDefined(pSWorkflowJob1, strArrays);
										pSWorkflowJob1.LoadWorkflow(jobInvocationInfo.Parameters[0], workflow, xaml);
										this.AddJob(pSWorkflowJob1);
										pSWorkflowJob = pSWorkflowJob1;
									}
									else
									{
										ArgumentException argumentException = new ArgumentException(Resources.DuplicateInstanceId);
										PSWorkflowJobManager.Tracer.TraceException(argumentException);
										throw argumentException;
									}
								}
								return pSWorkflowJob;
							}
							else
							{
								ArgumentException argumentException1 = new ArgumentException(Resources.DuplicateInstanceId);
								PSWorkflowJobManager.Tracer.TraceException(argumentException1);
								throw argumentException1;
							}
						}
						else
						{
							throw new ArgumentNullException("name");
						}
					}
					else
					{
						throw new ArgumentNullException("command");
					}
				}
				else
				{
					throw new ArgumentNullException("workflow");
				}
			}
			else
			{
				throw new ArgumentNullException("jobInstanceId");
			}
		}

		private static Dictionary<string, object> CreateJobMetadata(Job job, Guid parentInstanceId, int parentSessionId, string parentName, string parentCommand, string[] parentComputers)
		{
			Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			strs.Add("InstanceId", job.InstanceId);
			strs.Add("Id", job.Id);
			strs.Add("Name", job.Name);
			strs.Add("Command", job.Command);
			strs.Add("Reason", job.JobStateInfo.Reason);
			strs.Add("StatusMessage", job.StatusMessage);
			strs.Add("ParentInstanceId", parentInstanceId);
			strs.Add("ParentSessionId", parentSessionId);
			strs.Add("ParentName", parentName);
			strs.Add("ParentCommand", parentCommand);
			strs.Add("UserName", Environment.UserName);
			strs.Add("ProcessId", PSWorkflowJobManager.CurrentProcessId);
			Dictionary<string, object> strs1 = strs;
			return strs1;
		}

		private static Dictionary<string, object> CreateJobMetadataWithNoParentDefined(Job job, string[] parentComputers)
		{
			Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			strs.Add("InstanceId", job.InstanceId);
			strs.Add("Id", job.Id);
			strs.Add("Name", job.Name);
			strs.Add("Command", job.Command);
			strs.Add("Reason", job.JobStateInfo.Reason);
			strs.Add("StatusMessage", job.StatusMessage);
			strs.Add("UserName", Environment.UserName);
			strs.Add("ProcessId", PSWorkflowJobManager.CurrentProcessId);
			strs.Add("ParentInstanceId", Guid.NewGuid());
			strs.Add("ParentSessionId", 0);
			strs.Add("ParentName", job.Name);
			strs.Add("ParentCommand", job.Command);
			Dictionary<string, object> strs1 = strs;
			return strs1;
		}

		private void DoUnloadJob(object state)
		{
			Collection<object> objs = state as Collection<object>;
			Guid item = (Guid)objs[0];
			ManualResetEvent manualResetEvent = (ManualResetEvent)objs[1];
			this.UnloadJob(item);
			manualResetEvent.Set();
		}

		private PSWorkflowJob Get(Guid? jobInstanceId, Guid? workflowInstanceId)
		{
			Func<PSWorkflowJob, bool> func = null;
			PSWorkflowJob pSWorkflowJob = null;
			if (jobInstanceId.HasValue)
			{
				this._wfJobTable.TryGetValue(jobInstanceId.Value, out pSWorkflowJob);
			}
			if (workflowInstanceId.HasValue)
			{
				ICollection<PSWorkflowJob> values = this._wfJobTable.Values;
				if (func == null)
				{
					func = (PSWorkflowJob job) => job.PSWorkflowInstance.Id == workflowInstanceId.Value;
				}
				pSWorkflowJob = values.SingleOrDefault<PSWorkflowJob>(func);
			}
			if (pSWorkflowJob != null)
			{
				return pSWorkflowJob;
			}
			else
			{
				return null;
			}
		}

		public PSWorkflowJob GetJob(Guid instanceId)
		{
			Guid? nullable = null;
			return this.Get(new Guid?(instanceId), nullable);
		}

		public IEnumerable<PSWorkflowJob> GetJobs()
		{
			PSWorkflowJobManager.Tracer.WriteMessage("WorkflowManager : Getting all the workflow instances.");
			return this._wfJobTable.Values;
		}

		internal IEnumerable<Job2> GetJobs(WorkflowFilterTypes type, Dictionary<string, object> filters)
		{
			PSWorkflowJobManager.Tracer.WriteMessage("WorkflowManager : Geting workflow instances based on filters");
			return this.GetJobs(this._wfJobTable.Values, type, filters);
		}

		internal IEnumerable<Job2> GetJobs(ICollection<PSWorkflowJob> searchList, WorkflowFilterTypes type, IDictionary<string, object> filters)
		{
			object obj = null;
			Guid guid;
			WildcardPattern value;
			bool flag;
			List<Job2> job2s = new List<Job2>();
			Dictionary<string, object> strs = new Dictionary<string, object>(filters, StringComparer.CurrentCultureIgnoreCase);
			List<Job2> job2s1 = WorkflowJobSourceAdapter.SearchJobsOnV2Parameters(searchList, strs);
			string[] strArrays = new string[4];
			strArrays[0] = "Id";
			strArrays[1] = "InstanceId";
			strArrays[2] = "Name";
			strArrays[3] = "Command";
			string[] strArrays1 = strArrays;
			foreach (string str in strArrays1.Where<string>(new Func<string, bool>(strs.ContainsKey)))
			{
				strs.Remove(str);
			}
			if (strs.Count != 0)
			{
				foreach (Job2 job2 in strs)
				{
					bool flag1 = true;
					bool flag2 = true;
					bool flag3 = true;
					Dictionary<string, object>.Enumerator enumerator = strs.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<string, object> keyValuePair = job2;
							string key = keyValuePair.Key;
							if (PSWorkflowJobManager.SearchAllFilterTypes((PSWorkflowJob)job2, type, key, out obj))
							{
								if (obj as Guid == null)
								{
									if (!key.Equals("PSComputerName", StringComparison.OrdinalIgnoreCase))
									{
										if (!key.Equals("PSCredential", StringComparison.OrdinalIgnoreCase))
										{
											if ((keyValuePair.Value as string != null || keyValuePair.Value as WildcardPattern != null) && obj as string != null)
											{
												string value1 = keyValuePair.Value as string;
												if (value1 == null)
												{
													value = (WildcardPattern)keyValuePair.Value;
												}
												else
												{
													value = new WildcardPattern(value1, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
												}
												if (value.IsMatch((string)obj))
												{
													continue;
												}
												break;
											}
											else
											{
												if (obj != null && obj.Equals(keyValuePair.Value))
												{
													continue;
												}
												break;
											}
										}
										else
										{
											object obj1 = keyValuePair.Value;
											PSCredential baseObject = obj1 as PSCredential;
											if (baseObject == null)
											{
												PSObject pSObject = obj1 as PSObject;
												if (pSObject == null)
												{
													break;
												}
												baseObject = pSObject.BaseObject as PSCredential;
												if (baseObject == null)
												{
													break;
												}
											}
											flag2 = WorkflowUtils.CompareCredential(baseObject, obj as PSCredential);
										}
									}
									else
									{
										string[] strArrays2 = obj as string[];
										if (strArrays2 == null)
										{
											string str1 = obj as string;
											if (str1 == null)
											{
												break;
											}
											string[] strArrays3 = new string[1];
											strArrays3[0] = str1;
											strArrays2 = strArrays3;
										}
										object[] objArray = keyValuePair.Value as object[];
										if (objArray == null)
										{
											string value2 = keyValuePair.Value as string;
											if (value2 == null)
											{
												break;
											}
											object[] objArray1 = new object[1];
											objArray1[0] = value2;
											objArray = objArray1;
										}
										string[] strArrays4 = strArrays2;
										for (int i = 0; i < (int)strArrays4.Length; i++)
										{
											string str2 = strArrays4[i];
											flag1 = false;
											object[] objArray2 = objArray;
											int num = 0;
											while (num < (int)objArray2.Length)
											{
												object obj2 = objArray2[num];
												string str3 = obj2 as string;
												if (str3 == null)
												{
													break;
												}
												WildcardPattern wildcardPattern = new WildcardPattern(str3, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
												if (!wildcardPattern.IsMatch(str2))
												{
													num++;
												}
												else
												{
													flag1 = true;
													break;
												}
											}
											if (!flag1)
											{
												break;
											}
										}
										if (flag1)
										{
											continue;
										}
										break;
									}
								}
								else
								{
									LanguagePrimitives.TryConvertTo<Guid>(keyValuePair.Value, CultureInfo.InvariantCulture, out guid);
									if (guid == (Guid)obj)
									{
										continue;
									}
									break;
								}
							}
							else
							{
								break;
							}
						}
					}
					finally
					{
						enumerator.Dispose();
					}
					if (!flag1 || !flag2)
					{
						flag = false;
					}
					else
					{
						flag = flag3;
					}
					bool flag4 = flag;
					if (!flag4)
					{
						continue;
					}
					job2s.Add(job2);
				}
				return job2s;
			}
			else
			{
				return job2s1;
			}
		}

		public PSWorkflowJob LoadJob(PSWorkflowId storedInstanceId)
		{
			if (storedInstanceId != null)
			{
				if (!this.LoadJobWithIdentifier(storedInstanceId))
				{
					return null;
				}
				else
				{
					Guid? nullable = null;
					return this.Get(nullable, new Guid?(storedInstanceId.Guid));
				}
			}
			else
			{
				throw new ArgumentNullException("storedInstanceId");
			}
		}

		internal bool LoadJobWithIdentifier(PSWorkflowId storedInstanceId)
		{
			string str = null;
			string str1 = null;
			Guid guid;
			bool flag;
			PSWorkflowInstance pSWorkflowContext = this._runtime.Configuration.CreatePSWorkflowInstance(storedInstanceId);
			try
			{
				pSWorkflowContext.InstanceStore.Load(WorkflowStoreComponents.Metadata | WorkflowStoreComponents.JobState);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				PSWorkflowJobManager.Tracer.TraceException(exception);
				pSWorkflowContext.JobStateRetrieved = false;
				pSWorkflowContext.PSWorkflowContext = new PSWorkflowContext();
			}
			if (pSWorkflowContext.JobStateRetrieved)
			{
				if (WorkflowJobSourceAdapter.GetJobInfoFromMetadata(pSWorkflowContext, out str, out str1, out guid))
				{
					if (pSWorkflowContext.Timer != null)
					{
						if (!pSWorkflowContext.Timer.CheckIfTimerHasReachedAlready(WorkflowTimerType.ElapsedTimer))
						{
							pSWorkflowContext.Timer.StartTimer(WorkflowTimerType.ElapsedTimer);
						}
						else
						{
							pSWorkflowContext.RemoveInstance();
							return false;
						}
					}
					if (!this._wfJobTable.ContainsKey(guid))
					{
						lock (this.lockObjects.GetLockObject(guid))
						{
							if (!this._wfJobTable.ContainsKey(guid))
							{
								PSWorkflowJob pSWorkflowJob = new PSWorkflowJob(this._runtime, str, str1, guid);
								pSWorkflowJob.PSWorkflowInstance = pSWorkflowContext;
								pSWorkflowContext.PSWorkflowJob = pSWorkflowJob;
								pSWorkflowJob.RestoreFromWorkflowInstance(pSWorkflowContext);
								pSWorkflowJob.WorkflowInstanceLoaded = true;
								pSWorkflowJob.ConfigureWorkflowHandlers();
								if (!this._wfJobTable.ContainsKey(pSWorkflowJob.InstanceId))
								{
									this.AddJob(pSWorkflowJob);
								}
								flag = true;
							}
							else
							{
								flag = true;
							}
						}
						return flag;
					}
					else
					{
						return true;
					}
				}
				else
				{
					pSWorkflowContext.RemoveInstance();
					return false;
				}
			}
			else
			{
				pSWorkflowContext.RemoveInstance();
				return false;
			}
		}

		private void OnJobStateChanged(object sender, JobStateEventArgs eventArgs)
		{
			JobState state = eventArgs.JobStateInfo.State;
			switch (state)
			{
				case JobState.Completed:
				case JobState.Failed:
				case JobState.Stopped:
				case JobState.Suspended:
				{
					Job2 job2 = sender as Job2;
					job2.StateChanged -= new EventHandler<JobStateEventArgs>(this.OnJobStateChanged);
					Interlocked.Decrement(ref this._inProgressCount);
					PSWorkflowJobManager.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 5, (long)-1, true);
					if (Interlocked.Increment(ref PSWorkflowJobManager._workflowsBeforeGc) >= 125)
					{
						PSWorkflowJobManager.RunGarbageCollection(true);
					}
					this.CheckAndStartServicingThread();
					return;
				}
				case JobState.Blocked:
				{
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal void RemoveChildJob(Job2 childWorkflowJob)
		{
			if (WorkflowJobSourceAdapter.GetInstance().GetJobManager() != this)
			{
				this.RemoveJob(childWorkflowJob.InstanceId);
				return;
			}
			else
			{
				WorkflowJobSourceAdapter.GetInstance().RemoveChildJob(childWorkflowJob);
				return;
			}
		}

		public void RemoveJob(Guid instanceId)
		{
			PSWorkflowJob pSWorkflowJob = null;
			PSWorkflowJobManager.Tracer.WriteMessage(string.Concat("WorkflowManager : Removing job instance with id: ", instanceId));
			this._wfJobTable.TryGetValue(instanceId, out pSWorkflowJob);
			if (pSWorkflowJob != null)
			{
				lock (this.lockObjects.GetLockObject(instanceId))
				{
					this._wfJobTable.TryGetValue(instanceId, out pSWorkflowJob);
					if (pSWorkflowJob != null)
					{
						if (!pSWorkflowJob.IsFinishedState(pSWorkflowJob.JobStateInfo.State))
						{
							try
							{
								pSWorkflowJob.StopJob(true, "Remove");
							}
							catch (ObjectDisposedException objectDisposedException)
							{
								PSWorkflowJobManager.Tracer.WriteMessage("WorkflowManager : ", "RemoveJob", pSWorkflowJob.PSWorkflowInstance.Id, "Worklfow Job is already disposed. so removing it.", new string[0]);
							}
						}
						pSWorkflowJob.PSWorkflowInstance.RemoveInstance();
						pSWorkflowJob.Dispose();
						this._wfJobTable.TryRemove(instanceId, out pSWorkflowJob);
						this.lockObjects.RemoveLockObject(instanceId);
					}
					else
					{
						return;
					}
				}
				PSWorkflowJobManager.StructuredTracer.WorkflowDeletedFromDisk(instanceId, string.Empty);
				PSWorkflowJobManager.StructuredTracer.WorkflowCleanupPerformed(instanceId);
				return;
			}
			else
			{
				return;
			}
		}

		private static void RunGarbageCollection(bool force)
		{
			if (Interlocked.CompareExchange(ref PSWorkflowJobManager._gcStatus, 1, 0) == 0)
			{
				if (force || DateTime.Compare(DateTime.Now, PSWorkflowJobManager._lastGcTime.AddMinutes(5)) >= 0)
				{
					PSWorkflowJobManager._lastGcTime = DateTime.Now;
					Interlocked.Exchange(ref PSWorkflowJobManager._workflowsBeforeGc, 0);
					PSWorkflowJobManager.etwTracer.BeginRunGarbageCollection();
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();
					PSWorkflowJobManager.etwTracer.EndRunGarbageCollection();
				}
				Interlocked.CompareExchange(ref PSWorkflowJobManager._gcStatus, 0, 1);
				return;
			}
			else
			{
				return;
			}
		}

		private static bool SearchAllFilterTypes(PSWorkflowJob job, WorkflowFilterTypes type, string key, out object value)
		{
			object obj = null;
			bool flag;
			PSWorkflowContext pSWorkflowContext = job.PSWorkflowInstance.PSWorkflowContext;
			value = null;
			if (pSWorkflowContext != null)
			{
				Dictionary<WorkflowFilterTypes, Dictionary<string, object>> workflowFilterTypes = new Dictionary<WorkflowFilterTypes, Dictionary<string, object>>();
				workflowFilterTypes.Add(WorkflowFilterTypes.WorkflowSpecificParameters, pSWorkflowContext.WorkflowParameters);
				workflowFilterTypes.Add(WorkflowFilterTypes.JobMetadata, pSWorkflowContext.JobMetadata);
				workflowFilterTypes.Add(WorkflowFilterTypes.CommonParameters, pSWorkflowContext.PSWorkflowCommonParameters);
				workflowFilterTypes.Add(WorkflowFilterTypes.PrivateMetadata, pSWorkflowContext.PrivateMetadata);
				Dictionary<WorkflowFilterTypes, Dictionary<string, object>> workflowFilterTypes1 = workflowFilterTypes;
				Dictionary<WorkflowFilterTypes, Dictionary<string, object>>.KeyCollection.Enumerator enumerator = workflowFilterTypes1.Keys.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						WorkflowFilterTypes current = enumerator.Current;
						if (!type.HasFlag(current) || !PSWorkflowJobManager.SearchOneFilterType(workflowFilterTypes1[current], key, out obj))
						{
							continue;
						}
						value = obj;
						flag = true;
						return flag;
					}
					return false;
				}
				finally
				{
					enumerator.Dispose();
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		private static bool SearchOneFilterType(IDictionary<string, object> tableToSearch, string key, out object value)
		{
			value = null;
			if (tableToSearch != null)
			{
				if (!tableToSearch.ContainsKey(key))
				{
					return false;
				}
				else
				{
					tableToSearch.TryGetValue(key, out value);
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public void ShutdownWorkflowManager(int timeout = 0x1f4)
		{
			if (timeout > 0)
			{
				List<WaitHandle> waitHandles = new List<WaitHandle>();
				foreach (PSWorkflowJob job in this.GetJobs())
				{
					if (job.JobStateInfo.State != JobState.Running && job.JobStateInfo.State != JobState.Suspending)
					{
						continue;
					}
					try
					{
						if (!job.DoAbortJob(Resources.ShutdownAbort))
						{
							waitHandles.Add(job.Finished);
						}
						else
						{
							waitHandles.Add(job.SuspendedOrAborted);
						}
					}
					catch (InvalidOperationException invalidOperationException1)
					{
						InvalidOperationException invalidOperationException = invalidOperationException1;
						object[] objArray = new object[1];
						objArray[0] = invalidOperationException;
						PSWorkflowJobManager.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Shutting down workflow manager: suspend forcefully : Exception details: {0}", objArray));
					}
				}
				if (waitHandles.Count > 0)
				{
					WaitHandle.WaitAll(waitHandles.ToArray(), timeout);
					waitHandles.Clear();
				}
				foreach (PSWorkflowJob pSWorkflowJob in this.GetJobs())
				{
					this.UnloadJob(pSWorkflowJob.InstanceId);
				}
				this._wfJobTable.Clear();
				return;
			}
			else
			{
				throw new ArgumentException(Resources.ForceSuspendTimeout);
			}
		}

		private void StartOperationsFromQueue()
		{
			Tuple<Action<object>, object, JobState> tuple = null;
			while (true)
			{
				if (this._inProgressCount >= this._throttleLimit || !this._pendingQueue.TryDequeue(out tuple))
				{
					if (this._inProgressCount == 0 && this._pendingQueue.Count == 0)
					{
						Timer timer = new Timer();
						timer.Elapsed += new ElapsedEventHandler(this.WaitTimerElapsed);
						timer.Interval = 5000;
						timer.AutoReset = false;
						timer.Enabled = true;
					}
					this._waitForJobs.Value.WaitOne();
				}
				else
				{
					Action<object> item1 = tuple.Item1;
					PSWorkflowJobManager.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 15, (long)-1, true);
					PSWorkflowJob target = tuple.Item1.Target as PSWorkflowJob;
					if (target.CheckAndAddStateChangedEventHandler(new EventHandler<JobStateEventArgs>(this.OnJobStateChanged), tuple.Item3))
					{
						Interlocked.Increment(ref this._inProgressCount);
					}
					PSSQMAPI.UpdateWorkflowsConcurrentExecution(this._inProgressCount);
					PSWorkflowJobManager.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 5, (long)1, true);
					PSWorkflowJobManager.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 6, (long)1, true);
					item1(tuple.Item2);
					tuple = null;
				}
			}
		}

		internal void SubmitOperation(Job2 job, Action<object> operationHandler, object state, JobState expectedState)
		{
			if (job != null)
			{
				this._pendingQueue.Enqueue(Tuple.Create<Action<object>, object, JobState>(operationHandler, state, expectedState));
				PSWorkflowJobManager.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 15, (long)1, true);
				this.CheckAndStartServicingThread();
				return;
			}
			else
			{
				throw new ArgumentNullException("job");
			}
		}

		public void UnloadAllJobs()
		{
			List<WaitHandle> waitHandles = new List<WaitHandle>();
			foreach (KeyValuePair<Guid, PSWorkflowJob> keyValuePair in this._wfJobTable)
			{
				ManualResetEvent manualResetEvent = new ManualResetEvent(false);
				Collection<object> objs = new Collection<object>();
				objs.Add(keyValuePair.Key);
				objs.Add(manualResetEvent);
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoUnloadJob), objs);
				waitHandles.Add(manualResetEvent);
			}
			if (waitHandles.Count > 0)
			{
				if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
				{
					WaitHandle.WaitAll(waitHandles.ToArray());
				}
				else
				{
					foreach (WaitHandle waitHandle in waitHandles)
					{
						waitHandle.WaitOne();
					}
				}
				foreach (WaitHandle waitHandle1 in waitHandles)
				{
					waitHandle1.Dispose();
				}
			}
		}

		public void UnloadJob(Guid instanceId)
		{
			lock (this.lockObjects.GetLockObject(instanceId))
			{
				PSWorkflowJobManager.Tracer.WriteMessage(string.Concat("WorkflowManager : Forgeting job instance with id: ", instanceId));
				PSWorkflowJob job = this.GetJob(instanceId);
				if (job != null)
				{
					job.Dispose();
					this._wfJobTable.TryRemove(job.InstanceId, out job);
				}
			}
		}

		private void WaitTimerElapsed(object sender, ElapsedEventArgs e)
		{
			Timer timer = sender as Timer;
			if (this._inProgressCount == 0 && this._pendingQueue.Count == 0)
			{
				PSWorkflowJobManager.RunGarbageCollection(false);
			}
			timer.Elapsed -= new ElapsedEventHandler(this.WaitTimerElapsed);
			timer.Dispose();
		}
	}
}