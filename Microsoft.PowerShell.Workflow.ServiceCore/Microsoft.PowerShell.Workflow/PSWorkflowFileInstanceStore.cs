using Microsoft.PowerShell.Commands;
using System;
using System.Activities.Persistence;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Reflection;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowFileInstanceStore : PSWorkflowInstanceStore
	{
		private readonly PowerShellTraceSource Tracer;

		private readonly static Tracer etwTracer;

		private readonly string Streams;

		private readonly string Error;

		private readonly string Metadatas;

		private readonly string Definition;

		private readonly string WorkflowState;

		private readonly string Version_xml;

		private readonly string InputStream_xml;

		private readonly string OutputStream_xml;

		private readonly string ErrorStream_xml;

		private readonly string WarningStream_xml;

		private readonly string VerboseStream_xml;

		private readonly string ProgressStream_xml;

		private readonly string DebugStream_xml;

		private readonly string ErrorException_xml;

		private readonly string Input_xml;

		private readonly string PSWorkflowCommonParameters_xml;

		private readonly string JobMetadata_xml;

		private readonly string PrivateMetadata_xml;

		private readonly string Timer_xml;

		private readonly string WorkflowInstanceState_xml;

		private readonly string WorkflowDefinition_xaml;

		private readonly string RuntimeAssembly_dll;

		private readonly string State_xml;

		private readonly object _syncLock;

		private bool firstTimeStoringDefinition;

		private PSWorkflowConfigurationProvider _configuration;

		private Dictionary<InternalStoreComponents, long> SavedComponentLengths;

		private long writtenTotalBytes;

		internal static bool TestMode;

		internal static long ObjectCounter;

		private PersistenceVersion _version;

		private ArraySegment<byte> serializedInputStreamData;

		private ArraySegment<byte> serializedOutputStreamData;

		private ArraySegment<byte> serializedErrorStreamData;

		private ArraySegment<byte> serializedWarningStreamData;

		private ArraySegment<byte> serializedVerboseStreamData;

		private ArraySegment<byte> serializedProgressStreamData;

		private ArraySegment<byte> serializedDebugStreamData;

		private ArraySegment<byte> serializedWorkflowParameters;

		private ArraySegment<byte> serializedPSWorkflowCommonParameters;

		private ArraySegment<byte> serializedJobMetadata;

		private ArraySegment<byte> serializedPrivateMetadata;

		private ArraySegment<byte> serializedErrorException;

		private ArraySegment<byte> serializedTimerData;

		private ArraySegment<byte> serializedJobState;

		private ArraySegment<byte> serializedContext;

		private readonly static byte[] NullArray;

		private readonly static byte[] EncryptFalse;

		private bool serializationErrorHasOccured;

		private bool _disablePersistenceLimits;

		private string _workflowStorePath;

		private readonly static object MaxPersistenceStoreSizeLock;

		internal static long CurrentPersistenceStoreSize;

		private static bool _firstTimeCalculatingCurrentStoreSize;

		private string WorkflowStorePath
		{
			get
			{
				if (string.IsNullOrEmpty(this._workflowStorePath))
				{
					Guid id = base.PSWorkflowInstance.Id;
					this._workflowStorePath = Path.Combine(this._configuration.InstanceStorePath, id.ToString());
				}
				return this._workflowStorePath;
			}
		}

		static PSWorkflowFileInstanceStore()
		{
			PSWorkflowFileInstanceStore.etwTracer = new Tracer();
			PSWorkflowFileInstanceStore.TestMode = false;
			PSWorkflowFileInstanceStore.ObjectCounter = (long)0;
			byte[] numArray = new byte[] { _PrivateImplementationDetails__8AEF7EB8_CA5E_4A0A_BDCE_FF01E6B16089_.__method0x6000694_1 };
			PSWorkflowFileInstanceStore.NullArray = numArray;
			byte[] numArray1 = new byte[1];
			numArray1[0] = 70;
			PSWorkflowFileInstanceStore.EncryptFalse = numArray1;
			PSWorkflowFileInstanceStore.MaxPersistenceStoreSizeLock = new object();
			PSWorkflowFileInstanceStore.CurrentPersistenceStoreSize = (long)0;
			PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize = true;
		}

		public PSWorkflowFileInstanceStore(PSWorkflowConfigurationProvider configuration, PSWorkflowInstance instance) : base(instance)
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.Streams = "Str";
			this.Error = "Err";
			this.Metadatas = "Meta";
			this.Definition = "Def";
			this.WorkflowState = "Stat";
			this.Version_xml = "V.xml";
			this.InputStream_xml = "IS.xml";
			this.OutputStream_xml = "OS.xml";
			this.ErrorStream_xml = "ES.xml";
			this.WarningStream_xml = "WS.xml";
			this.VerboseStream_xml = "VS.xml";
			this.ProgressStream_xml = "PS.xml";
			this.DebugStream_xml = "DS.xml";
			this.ErrorException_xml = "EE.xml";
			this.Input_xml = "I.xml";
			this.PSWorkflowCommonParameters_xml = "UI.xml";
			this.JobMetadata_xml = "JM.xml";
			this.PrivateMetadata_xml = "PM.xml";
			this.Timer_xml = "TI.xml";
			this.WorkflowInstanceState_xml = "WS.xml";
			this.WorkflowDefinition_xaml = "WD.xaml";
			this.RuntimeAssembly_dll = "RA.dll";
			this.State_xml = "S.xml";
			this._syncLock = new object();
			if (configuration != null)
			{
				if (PSWorkflowFileInstanceStore.TestMode)
				{
					Interlocked.Increment(ref PSWorkflowFileInstanceStore.ObjectCounter);
				}
				this._configuration = configuration;
				this.firstTimeStoringDefinition = true;
				this.SavedComponentLengths = new Dictionary<InternalStoreComponents, long>();
				bool flag = true;
				this._disablePersistenceLimits = true;
				if (PSSessionConfigurationData.IsServerManager)
				{
					flag = false;
					this._disablePersistenceLimits = false;
				}
				this._version = new PersistenceVersion(this._configuration.PersistWithEncryption, flag);
				Guid id = base.PSWorkflowInstance.Id;
				this._version.load(Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Version_xml));
				return;
			}
			else
			{
				throw new ArgumentNullException("configuration");
			}
		}

		internal void CalculatePersistenceStoreSizeForFirstTime()
		{
			if (!this._configuration.IsDefaultStorePath)
			{
				if (PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize)
				{
					lock (PSWorkflowFileInstanceStore.MaxPersistenceStoreSizeLock)
					{
						if (PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize)
						{
							PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize = false;
							PSWorkflowFileInstanceStore.CurrentPersistenceStoreSize = this.GetDirectoryLength(new DirectoryInfo(this._configuration.InstanceStorePath));
						}
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

		internal IEnumerable<object> CallDoLoad(IEnumerable<Type> componentTypes)
		{
			return this.DoLoad(componentTypes);
		}

		internal void CallDoSave(IEnumerable<object> components)
		{
			this.DoSave(components);
		}

		private bool CheckMaxPersistenceSize(long oldValue, long newValue)
		{
			bool flag;
			this.CalculatePersistenceStoreSizeForFirstTime();
			lock (PSWorkflowFileInstanceStore.MaxPersistenceStoreSizeLock)
			{
				long currentPersistenceStoreSize = PSWorkflowFileInstanceStore.CurrentPersistenceStoreSize - oldValue + newValue;
				long maxPersistenceStoreSizeGB = this._configuration.MaxPersistenceStoreSizeGB * (long)0x400 * (long)0x400 * (long)0x400;
				if (currentPersistenceStoreSize >= maxPersistenceStoreSizeGB)
				{
					flag = false;
				}
				else
				{
					PSWorkflowFileInstanceStore.CurrentPersistenceStoreSize = currentPersistenceStoreSize;
					flag = true;
				}
			}
			return flag;
		}

		private void CreateAndEnsureInstancePath(string subPath, out string storePath)
		{
			storePath = Path.Combine(this.WorkflowStorePath, subPath);
			this.EnsureInstancePath(storePath);
		}

		public override InstanceStore CreateInstanceStore()
		{
			return new FileInstanceStore(this);
		}

		public override PersistenceIOParticipant CreatePersistenceIOParticipant()
		{
			return null;
		}

		protected internal virtual ArraySegment<byte> Decrypt(ArraySegment<byte> source)
		{
			bool flag = false;
			if (source.Array[0] == 84)
			{
				flag = true;
			}
			if (flag)
			{
				byte[] numArray = new byte[source.Count - 1];
				Buffer.BlockCopy(source.Array, 1, numArray, 0, source.Count - 1);
				byte[] numArray1 = InstanceStoreCryptography.Unprotect(numArray);
				ArraySegment<byte> nums = new ArraySegment<byte>(numArray1, 0, (int)numArray1.Length);
				return nums;
			}
			else
			{
				ArraySegment<byte> nums1 = new ArraySegment<byte>(source.Array, 1, source.Count - 1);
				return nums1;
			}
		}

		internal Dictionary<string, object> DeserializeContextFromStore()
		{
			Dictionary<string, object> strs;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.WorkflowState);
			if (Directory.Exists(str))
			{
				if (this._disablePersistenceLimits)
				{
					strs = (Dictionary<string, object>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.State_xml));
				}
				else
				{
					strs = (Dictionary<string, object>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.State_xml))));
				}
				Dictionary<string, object> strs1 = strs;
				return strs1;
			}
			else
			{
				return null;
			}
		}

		internal object DeserializeObject(ArraySegment<byte> source)
		{
			object obj;
			try
			{
				XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
				using (MemoryStream memoryStream = new MemoryStream(source.Array, source.Offset, source.Count))
				{
					object obj1 = netDataContractSerializer.ReadObject(memoryStream);
					obj = obj1;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.TraceException(exception);
				throw;
			}
			return obj;
		}

		internal object DeserializeObject2(ArraySegment<byte> source)
		{
			object obj;
			try
			{
				XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
				using (MemoryStream memoryStream = new MemoryStream(source.Array, source.Offset, source.Count))
				{
					object obj1 = netDataContractSerializer.ReadObject(memoryStream);
					obj = obj1;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.TraceException(exception);
				throw;
			}
			return obj;
		}

		private PSWorkflowDefinition DeserializeWorkflowDefinitionFromStore()
		{
			PSWorkflowDefinition pSWorkflowDefinition;
			string str = null;
			try
			{
				Guid id = base.PSWorkflowInstance.Id;
				string str1 = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Definition);
				if (Directory.Exists(str1))
				{
					string str2 = File.ReadAllText(Path.Combine(str1, this.WorkflowDefinition_xaml));
					if (File.Exists(Path.Combine(str1, this.RuntimeAssembly_dll)))
					{
						str = Path.Combine(str1, this.RuntimeAssembly_dll);
					}
					PSWorkflowDefinition activity = new PSWorkflowDefinition(null, str2, str);
					if (activity.Workflow == null && !string.IsNullOrEmpty(activity.WorkflowXaml))
					{
						if (!string.IsNullOrEmpty(activity.RuntimeAssemblyPath))
						{
							Assembly assembly = Assembly.LoadFrom(activity.RuntimeAssemblyPath);
							string name = assembly.GetName().Name;
							string str3 = null;
							activity.Workflow = ImportWorkflowCommand.ConvertXamlToActivity(activity.WorkflowXaml, null, null, ref str3, ref assembly, ref name);
						}
						else
						{
							activity.Workflow = ImportWorkflowCommand.ConvertXamlToActivity(activity.WorkflowXaml);
						}
					}
					return activity;
				}
				else
				{
					pSWorkflowDefinition = null;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.TraceException(exception);
				throw;
			}
			return pSWorkflowDefinition;
		}

		private Exception DeserializeWorkflowErrorExceptionFromStore()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Error);
			if (Directory.Exists(str))
			{
				if (this._disablePersistenceLimits)
				{
					return (Exception)this.LoadFromFileAndDeserialize(Path.Combine(str, this.ErrorException_xml));
				}
				else
				{
					return (Exception)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.ErrorException_xml))));
				}
			}
			else
			{
				return null;
			}
		}

		internal JobState? DeserializeWorkflowInstanceStateFromStore()
		{
			JobState jobState;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas, this.WorkflowInstanceState_xml);
			if (File.Exists(str))
			{
				if (this._disablePersistenceLimits)
				{
					jobState = (JobState)this.LoadFromFileAndDeserialize(str);
				}
				else
				{
					jobState = (JobState)this.DeserializeObject(this.Decrypt(this.LoadFromFile(str)));
				}
				return new JobState?(jobState);
			}
			else
			{
				JobState? nullable = null;
				return nullable;
			}
		}

		private PSWorkflowContext DeserializeWorkflowMetadataFromStore()
		{
			PSWorkflowContext pSWorkflowContext = new PSWorkflowContext();
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			if (Directory.Exists(str))
			{
				if (!this._disablePersistenceLimits)
				{
					pSWorkflowContext.WorkflowParameters = (Dictionary<string, object>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.Input_xml))));
					pSWorkflowContext.PSWorkflowCommonParameters = (Dictionary<string, object>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.PSWorkflowCommonParameters_xml))));
					pSWorkflowContext.JobMetadata = (Dictionary<string, object>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.JobMetadata_xml))));
					pSWorkflowContext.PrivateMetadata = (Dictionary<string, object>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.PrivateMetadata_xml))));
				}
				else
				{
					pSWorkflowContext.WorkflowParameters = (Dictionary<string, object>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.Input_xml));
					pSWorkflowContext.PSWorkflowCommonParameters = (Dictionary<string, object>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.PSWorkflowCommonParameters_xml));
					pSWorkflowContext.JobMetadata = (Dictionary<string, object>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.JobMetadata_xml));
					pSWorkflowContext.PrivateMetadata = (Dictionary<string, object>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.PrivateMetadata_xml));
				}
				return pSWorkflowContext;
			}
			else
			{
				return pSWorkflowContext;
			}
		}

		private PowerShellStreams<PSObject, PSObject> DeserializeWorkflowStreamsFromStore()
		{
			PowerShellStreams<PSObject, PSObject> powerShellStream = new PowerShellStreams<PSObject, PSObject>(null);
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Streams);
			if (Directory.Exists(str))
			{
				if (!this._disablePersistenceLimits)
				{
					powerShellStream.InputStream = (PSDataCollection<PSObject>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.InputStream_xml))));
					powerShellStream.OutputStream = (PSDataCollection<PSObject>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.OutputStream_xml))));
					powerShellStream.ErrorStream = (PSDataCollection<ErrorRecord>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.ErrorStream_xml))));
					powerShellStream.WarningStream = (PSDataCollection<WarningRecord>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.WarningStream_xml))));
					powerShellStream.VerboseStream = (PSDataCollection<VerboseRecord>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.VerboseStream_xml))));
					powerShellStream.ProgressStream = (PSDataCollection<ProgressRecord>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.ProgressStream_xml))));
					powerShellStream.DebugStream = (PSDataCollection<DebugRecord>)this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.DebugStream_xml))));
				}
				else
				{
					powerShellStream.InputStream = (PSDataCollection<PSObject>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.InputStream_xml));
					powerShellStream.OutputStream = (PSDataCollection<PSObject>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.OutputStream_xml));
					powerShellStream.ErrorStream = (PSDataCollection<ErrorRecord>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.ErrorStream_xml));
					powerShellStream.WarningStream = (PSDataCollection<WarningRecord>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.WarningStream_xml));
					powerShellStream.VerboseStream = (PSDataCollection<VerboseRecord>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.VerboseStream_xml));
					powerShellStream.ProgressStream = (PSDataCollection<ProgressRecord>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.ProgressStream_xml));
					powerShellStream.DebugStream = (PSDataCollection<DebugRecord>)this.LoadFromFileAndDeserialize(Path.Combine(str, this.DebugStream_xml));
				}
			}
			return powerShellStream;
		}

		private object DeserializeWorkflowTimerFromStore()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			if (Directory.Exists(str))
			{
				if (this._disablePersistenceLimits)
				{
					return this.LoadFromFileAndDeserialize(Path.Combine(str, this.Timer_xml));
				}
				else
				{
					return this.DeserializeObject(this.Decrypt(this.LoadFromFile(Path.Combine(str, this.Timer_xml))));
				}
			}
			else
			{
				return null;
			}
		}

		protected override void DoDelete()
		{
			lock (this._syncLock)
			{
				this.InternalDelete();
			}
		}

		protected override IEnumerable<object> DoLoad(IEnumerable<Type> componentTypes)
		{
			object pSWorkflowTimer;
			Collection<object> objs = new Collection<object>();
			lock (this._syncLock)
			{
				foreach (Type componentType in componentTypes)
				{
					if ((JobState)componentType != typeof(JobState))
					{
						if (componentType != typeof(Dictionary<string, object>))
						{
							if (componentType != typeof(PSWorkflowDefinition))
							{
								if (componentType != typeof(Exception))
								{
									if (componentType != typeof(PSWorkflowContext))
									{
										if (componentType != typeof(PowerShellStreams<PSObject, PSObject>))
										{
											if (componentType != typeof(PSWorkflowTimer))
											{
												continue;
											}
											object obj = this.DeserializeWorkflowTimerFromStore();
											Collection<object> objs1 = objs;
											if (obj == null)
											{
												pSWorkflowTimer = new PSWorkflowTimer(base.PSWorkflowInstance);
											}
											else
											{
												pSWorkflowTimer = new PSWorkflowTimer(base.PSWorkflowInstance, obj);
											}
											objs1.Add(pSWorkflowTimer);
										}
										else
										{
											objs.Add(this.DeserializeWorkflowStreamsFromStore());
										}
									}
									else
									{
										objs.Add(this.DeserializeWorkflowMetadataFromStore());
									}
								}
								else
								{
									Exception exception = this.DeserializeWorkflowErrorExceptionFromStore();
									if (exception == null)
									{
										continue;
									}
									objs.Add(exception);
								}
							}
							else
							{
								PSWorkflowDefinition pSWorkflowDefinition = this.DeserializeWorkflowDefinitionFromStore();
								if (pSWorkflowDefinition == null)
								{
									continue;
								}
								objs.Add(pSWorkflowDefinition);
							}
						}
						else
						{
							Dictionary<string, object> strs = this.DeserializeContextFromStore();
							if (strs == null)
							{
								continue;
							}
							objs.Add(strs);
						}
					}
					else
					{
						JobState? nullable = this.DeserializeWorkflowInstanceStateFromStore();
						if (!nullable.HasValue)
						{
							continue;
						}
						JobState value = nullable.Value;
						if (value == JobState.Running || value == JobState.Suspended || value == JobState.Suspending || value == JobState.Stopping || value == JobState.NotStarted)
						{
							value = JobState.Suspended;
						}
						objs.Add(value);
					}
				}
			}
			return objs;
		}

		protected override void DoSave(IEnumerable<object> components)
		{
			long item;
			long savedJobStateLength;
			long savedTimerLength;
			long savedDefinitionLength;
			long savedMetadataLength;
			long savedStreamDataLength;
			long savedContextLength;
			if (!this.serializationErrorHasOccured)
			{
				this.SaveVersionFile();
				if (!this._disablePersistenceLimits)
				{
					lock (this._syncLock)
					{
						long num = (long)0;
						long num1 = (long)0;
						long num2 = (long)0;
						long num3 = (long)0;
						long num4 = (long)0;
						long num5 = (long)0;
						long num6 = (long)0;
						long num7 = (long)0;
						long num8 = (long)0;
						long num9 = (long)0;
						long num10 = (long)0;
						long num11 = (long)0;
						long num12 = (long)0;
						long num13 = (long)0;
						foreach (object component in components)
						{
							Type type = component.GetType();
							if (type != typeof(Dictionary<string, object>))
							{
								if (type != typeof(PowerShellStreams<PSObject, PSObject>))
								{
									if (type != typeof(PSWorkflowContext))
									{
										if (type != typeof(PSWorkflowDefinition))
										{
											if (type != typeof(PSWorkflowTimer))
											{
												if ((JobState)type != typeof(JobState))
												{
													if (component as Exception == null)
													{
														continue;
													}
													num6 = this.LoadSerializedErrorException(component);
													if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.TerminatingError))
													{
														item = this.SavedComponentLengths[InternalStoreComponents.TerminatingError];
													}
													else
													{
														item = this.GetSavedErrorExceptionLength();
													}
													num13 = item;
													this.SaveSerializedErrorException();
													if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.TerminatingError))
													{
														this.SavedComponentLengths.Remove(InternalStoreComponents.TerminatingError);
													}
													this.SavedComponentLengths.Add(InternalStoreComponents.TerminatingError, num6);
												}
												else
												{
													num5 = this.LoadSerializedJobState(component);
													if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.JobState))
													{
														savedJobStateLength = this.SavedComponentLengths[InternalStoreComponents.JobState];
													}
													else
													{
														savedJobStateLength = this.GetSavedJobStateLength();
													}
													num12 = savedJobStateLength;
													this.SaveSerializedJobState();
													if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.JobState))
													{
														this.SavedComponentLengths.Remove(InternalStoreComponents.JobState);
													}
													this.SavedComponentLengths.Add(InternalStoreComponents.JobState, num5);
												}
											}
											else
											{
												num4 = this.LoadSerializedTimer(component as PSWorkflowTimer);
												if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Timer))
												{
													savedTimerLength = this.SavedComponentLengths[InternalStoreComponents.Timer];
												}
												else
												{
													savedTimerLength = this.GetSavedTimerLength();
												}
												num11 = savedTimerLength;
												this.SaveSerializedTimer();
												if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Timer))
												{
													this.SavedComponentLengths.Remove(InternalStoreComponents.Timer);
												}
												this.SavedComponentLengths.Add(InternalStoreComponents.Timer, num4);
											}
										}
										else
										{
											if (!this.firstTimeStoringDefinition)
											{
												continue;
											}
											num3 = this.LoadSerializedDefinition(component);
											if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Definition))
											{
												savedDefinitionLength = this.SavedComponentLengths[InternalStoreComponents.Definition];
											}
											else
											{
												savedDefinitionLength = this.GetSavedDefinitionLength();
											}
											num10 = savedDefinitionLength;
											this.SaveSerializedDefinition(component);
											if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Definition))
											{
												this.SavedComponentLengths.Remove(InternalStoreComponents.Definition);
											}
											this.SavedComponentLengths.Add(InternalStoreComponents.Definition, num3);
										}
									}
									else
									{
										num2 = this.LoadSerializedMetadata(component);
										if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Metadata))
										{
											savedMetadataLength = this.SavedComponentLengths[InternalStoreComponents.Metadata];
										}
										else
										{
											savedMetadataLength = this.GetSavedMetadataLength();
										}
										num9 = savedMetadataLength;
										this.SaveSerializedMetadata();
										if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Metadata))
										{
											this.SavedComponentLengths.Remove(InternalStoreComponents.Metadata);
										}
										this.SavedComponentLengths.Add(InternalStoreComponents.Metadata, num2);
									}
								}
								else
								{
									num1 = this.LoadSerializedStreamData(component);
									if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Streams))
									{
										savedStreamDataLength = this.SavedComponentLengths[InternalStoreComponents.Streams];
									}
									else
									{
										savedStreamDataLength = this.GetSavedStreamDataLength();
									}
									num8 = savedStreamDataLength;
									this.SaveSerializedStreamData();
									if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Streams))
									{
										this.SavedComponentLengths.Remove(InternalStoreComponents.Streams);
									}
									this.SavedComponentLengths.Add(InternalStoreComponents.Streams, num1);
									PSSQMAPI.NoteWorkflowOutputStreamSize(this.serializedOutputStreamData.Count, "output");
									PSSQMAPI.NoteWorkflowOutputStreamSize(this.serializedProgressStreamData.Count, "progress");
									PSSQMAPI.NoteWorkflowOutputStreamSize(this.serializedDebugStreamData.Count, "debug");
									PSSQMAPI.NoteWorkflowOutputStreamSize(this.serializedWarningStreamData.Count, "warning");
									PSSQMAPI.NoteWorkflowOutputStreamSize(this.serializedVerboseStreamData.Count, "verbose");
									PSSQMAPI.NoteWorkflowOutputStreamSize(this.serializedErrorStreamData.Count, "error");
								}
							}
							else
							{
								num = this.LoadSerializedContext(component);
								if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Context))
								{
									savedContextLength = this.SavedComponentLengths[InternalStoreComponents.Context];
								}
								else
								{
									savedContextLength = this.GetSavedContextLength();
								}
								num7 = savedContextLength;
								this.SaveSerializedContext();
								if (this.SavedComponentLengths.ContainsKey(InternalStoreComponents.Context))
								{
									this.SavedComponentLengths.Remove(InternalStoreComponents.Context);
								}
								this.SavedComponentLengths.Add(InternalStoreComponents.Context, num);
							}
						}
						long num14 = num7 + num8 + num9 + num10 + num11 + num12 + num13;
						long num15 = num + num1 + num2 + num3 + num4 + num5 + num6;
						bool flag = this.CheckMaxPersistenceSize(num14, num15);
						if (!flag)
						{
							this.WriteWarning(Resources.PersistenceSizeReached);
							PSWorkflowFileInstanceStore.etwTracer.PersistenceStoreMaxSizeReached();
						}
					}
					return;
				}
				else
				{
					this.DoSave2(components);
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal void DoSave2(IEnumerable<object> components)
		{
			string str = null;
			lock (this._syncLock)
			{
				long file = (long)0;
				if (this.writtenTotalBytes == (long)0)
				{
					this.writtenTotalBytes = this.GetSavedContextLength() + this.GetSavedStreamDataLength() + this.GetSavedMetadataLength() + this.GetSavedDefinitionLength() + this.GetSavedTimerLength() + this.GetSavedJobStateLength() + this.GetSavedErrorExceptionLength();
				}
				foreach (object component in components)
				{
					Type type = component.GetType();
					if (type != typeof(Dictionary<string, object>))
					{
						if (type != typeof(PowerShellStreams<PSObject, PSObject>))
						{
							if (type != typeof(PSWorkflowContext))
							{
								if (type != typeof(PSWorkflowDefinition))
								{
									if (type != typeof(PSWorkflowTimer))
									{
										if ((JobState)type != typeof(JobState))
										{
											if (component as Exception == null)
											{
												continue;
											}
											this.CreateAndEnsureInstancePath(this.Error, out str);
											file = file + (long)this.SerializeAndSaveToFile(component, Path.Combine(str, this.ErrorException_xml));
										}
										else
										{
											this.CreateAndEnsureInstancePath(this.Metadatas, out str);
											file = file + (long)this.SerializeAndSaveToFile(component, Path.Combine(str, this.WorkflowInstanceState_xml));
										}
									}
									else
									{
										this.CreateAndEnsureInstancePath(this.Metadatas, out str);
										PSWorkflowTimer pSWorkflowTimer = (PSWorkflowTimer)component;
										file = file + (long)this.SerializeAndSaveToFile(pSWorkflowTimer.GetSerializedData(), Path.Combine(str, this.Timer_xml));
									}
								}
								else
								{
									if (!this.firstTimeStoringDefinition)
									{
										continue;
									}
									PSWorkflowDefinition pSWorkflowDefinition = (PSWorkflowDefinition)component;
									this.SaveSerializedDefinition(pSWorkflowDefinition);
									file = file + this.GetSavedDefinitionLength();
								}
							}
							else
							{
								this.CreateAndEnsureInstancePath(this.Metadatas, out str);
								PSWorkflowContext pSWorkflowContext = (PSWorkflowContext)component;
								file = file + (long)this.SerializeAndSaveToFile(pSWorkflowContext.WorkflowParameters, Path.Combine(str, this.Input_xml));
								file = file + (long)this.SerializeAndSaveToFile(pSWorkflowContext.PSWorkflowCommonParameters, Path.Combine(str, this.PSWorkflowCommonParameters_xml));
								file = file + (long)this.SerializeAndSaveToFile(pSWorkflowContext.JobMetadata, Path.Combine(str, this.JobMetadata_xml));
								file = file + (long)this.SerializeAndSaveToFile(pSWorkflowContext.PrivateMetadata, Path.Combine(str, this.PrivateMetadata_xml));
							}
						}
						else
						{
							this.CreateAndEnsureInstancePath(this.Streams, out str);
							PowerShellStreams<PSObject, PSObject> powerShellStream = (PowerShellStreams<PSObject, PSObject>)component;
							file = file + (long)this.SerializeAndSaveToFile(powerShellStream.InputStream, Path.Combine(str, this.InputStream_xml));
							long num = (long)this.SerializeAndSaveToFile(powerShellStream.OutputStream, Path.Combine(str, this.OutputStream_xml));
							file = file + num;
							PSSQMAPI.NoteWorkflowOutputStreamSize((uint)num, "output");
							num = (long)this.SerializeAndSaveToFile(powerShellStream.ErrorStream, Path.Combine(str, this.ErrorStream_xml));
							file = file + num;
							PSSQMAPI.NoteWorkflowOutputStreamSize((uint)num, "error");
							num = (long)this.SerializeAndSaveToFile(powerShellStream.WarningStream, Path.Combine(str, this.WarningStream_xml));
							file = file + num;
							PSSQMAPI.NoteWorkflowOutputStreamSize((uint)num, "warning");
							num = (long)this.SerializeAndSaveToFile(powerShellStream.VerboseStream, Path.Combine(str, this.VerboseStream_xml));
							file = file + num;
							PSSQMAPI.NoteWorkflowOutputStreamSize((uint)num, "verbose");
							num = (long)this.SerializeAndSaveToFile(powerShellStream.ProgressStream, Path.Combine(str, this.ProgressStream_xml));
							file = file + num;
							PSSQMAPI.NoteWorkflowOutputStreamSize((uint)num, "progress");
							num = (long)this.SerializeAndSaveToFile(powerShellStream.DebugStream, Path.Combine(str, this.DebugStream_xml));
							file = file + num;
							PSSQMAPI.NoteWorkflowOutputStreamSize((uint)num, "debug");
						}
					}
					else
					{
						this.CreateAndEnsureInstancePath(this.WorkflowState, out str);
						file = file + (long)this.SerializeAndSaveToFile(component, Path.Combine(str, this.State_xml));
					}
				}
				long num1 = this.writtenTotalBytes;
				this.writtenTotalBytes = file;
				bool flag = this.CheckMaxPersistenceSize(num1, this.writtenTotalBytes);
				if (!flag)
				{
					this.WriteWarning(Resources.PersistenceSizeReached);
					PSWorkflowFileInstanceStore.etwTracer.PersistenceStoreMaxSizeReached();
				}
			}
		}

		protected internal virtual ArraySegment<byte> Encrypt(ArraySegment<byte> source)
		{
			if (!this._version.EnableCompression)
			{
				if (this._version.EnableEncryption)
				{
					byte[] numArray = new byte[source.Count];
					Buffer.BlockCopy(source.Array, 0, numArray, 0, source.Count);
					byte[] numArray1 = InstanceStoreCryptography.Protect(numArray);
					byte[] numArray2 = new byte[(int)numArray1.Length + 1];
					numArray2[0] = 84;
					Buffer.BlockCopy(numArray1, 0, numArray2, 1, (int)numArray1.Length);
					ArraySegment<byte> nums = new ArraySegment<byte>(numArray2, 0, (int)numArray1.Length + 1);
					return nums;
				}
				else
				{
					byte[] numArray3 = new byte[source.Count + 1];
					numArray3[0] = 70;
					Buffer.BlockCopy(source.Array, 0, numArray3, 1, source.Count);
					ArraySegment<byte> nums1 = new ArraySegment<byte>(numArray3, 0, (int)numArray3.Length);
					return nums1;
				}
			}
			else
			{
				return this.Encrypt2(source);
			}
		}

		private ArraySegment<byte> Encrypt2(ArraySegment<byte> source)
		{
			if (this._version.EnableEncryption)
			{
				byte[] numArray = new byte[source.Count - 1];
				Buffer.BlockCopy(source.Array, 1, numArray, 0, source.Count - 1);
				byte[] numArray1 = InstanceStoreCryptography.Protect(numArray);
				byte[] numArray2 = new byte[(int)numArray1.Length + 1];
				numArray2[0] = 84;
				Buffer.BlockCopy(numArray1, 0, numArray2, 1, (int)numArray1.Length);
				ArraySegment<byte> nums = new ArraySegment<byte>(numArray2, 0, (int)numArray1.Length + 1);
				return nums;
			}
			else
			{
				return source;
			}
		}

		private void EnsureInstancePath(string storePath)
		{
			if (!Directory.Exists(storePath))
			{
				try
				{
					Directory.CreateDirectory(storePath);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					throw;
				}
				InstanceStorePermission.SetDirectoryPermissions(storePath);
			}
		}

		public static IEnumerable<PSWorkflowId> GetAllWorkflowInstanceIds()
		{
			PSWorkflowConfigurationProvider configuration = PSWorkflowRuntime.Instance.Configuration;
			DirectoryInfo directoryInfo = new DirectoryInfo(configuration.InstanceStorePath);
			if (directoryInfo.Exists)
			{
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				for (int i = 0; i < (int)directories.Length; i++)
				{
					DirectoryInfo directoryInfo1 = directories[i];
					if (Guid.TryParse(directoryInfo1.Name, out id))
					{
						yield return new PSWorkflowId(id);
					}
				}
			}
		}

		private long GetDirectoryLength(DirectoryInfo directory)
		{
			long length;
			long directoryLength = (long)0;
			try
			{
				if (directory.Exists)
				{
					DirectoryInfo[] directories = directory.GetDirectories();
					FileInfo[] files = directory.GetFiles();
					FileInfo[] fileInfoArray = files;
					for (int i = 0; i < (int)fileInfoArray.Length; i++)
					{
						FileInfo fileInfo = fileInfoArray[i];
						long num = directoryLength;
						if (fileInfo.Exists)
						{
							length = fileInfo.Length;
						}
						else
						{
							length = (long)0;
						}
						directoryLength = num + length;
					}
					DirectoryInfo[] directoryInfoArray = directories;
					for (int j = 0; j < (int)directoryInfoArray.Length; j++)
					{
						DirectoryInfo directoryInfo = directoryInfoArray[j];
						directoryLength = directoryLength + this.GetDirectoryLength(directoryInfo);
					}
				}
			}
			catch (DirectoryNotFoundException directoryNotFoundException1)
			{
				DirectoryNotFoundException directoryNotFoundException = directoryNotFoundException1;
				this.Tracer.TraceException(directoryNotFoundException);
				PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize = true;
			}
			catch (FileNotFoundException fileNotFoundException1)
			{
				FileNotFoundException fileNotFoundException = fileNotFoundException1;
				this.Tracer.TraceException(fileNotFoundException);
				PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize = true;
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				this.Tracer.TraceException(unauthorizedAccessException);
				PSWorkflowFileInstanceStore._firstTimeCalculatingCurrentStoreSize = true;
			}
			return directoryLength;
		}

		private long GetSavedContextLength()
		{
			long length;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.WorkflowState);
			long num = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.State_xml));
				long num1 = num;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num = num1 + length;
			}
			return num;
		}

		private long GetSavedDefinitionLength()
		{
			long length;
			long num;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Definition);
			long num1 = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.WorkflowDefinition_xaml));
				long num2 = num1;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num1 = num2 + length;
				fileInfo = new FileInfo(Path.Combine(str, this.RuntimeAssembly_dll));
				long num3 = num1;
				if (fileInfo.Exists)
				{
					num = fileInfo.Length;
				}
				else
				{
					num = (long)0;
				}
				num1 = num3 + num;
			}
			return num1;
		}

		private long GetSavedErrorExceptionLength()
		{
			long length;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Error);
			long num = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.ErrorException_xml));
				long num1 = num;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num = num1 + length;
			}
			return num;
		}

		private long GetSavedJobStateLength()
		{
			long length;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			long num = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.WorkflowInstanceState_xml));
				long num1 = num;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num = num1 + length;
			}
			return num;
		}

		private long GetSavedMetadataLength()
		{
			long length;
			long num;
			long length1;
			long num1;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			long num2 = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.Input_xml));
				long num3 = num2;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num2 = num3 + length;
				fileInfo = new FileInfo(Path.Combine(str, this.PSWorkflowCommonParameters_xml));
				long num4 = num2;
				if (fileInfo.Exists)
				{
					num = fileInfo.Length;
				}
				else
				{
					num = (long)0;
				}
				num2 = num4 + num;
				fileInfo = new FileInfo(Path.Combine(str, this.JobMetadata_xml));
				long num5 = num2;
				if (fileInfo.Exists)
				{
					length1 = fileInfo.Length;
				}
				else
				{
					length1 = (long)0;
				}
				num2 = num5 + length1;
				fileInfo = new FileInfo(Path.Combine(str, this.PrivateMetadata_xml));
				long num6 = num2;
				if (fileInfo.Exists)
				{
					num1 = fileInfo.Length;
				}
				else
				{
					num1 = (long)0;
				}
				num2 = num6 + num1;
			}
			return num2;
		}

		private long GetSavedStreamDataLength()
		{
			long length;
			long num;
			long length1;
			long num1;
			long length2;
			long num2;
			long length3;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Streams);
			long num3 = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.InputStream_xml));
				long num4 = num3;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num3 = num4 + length;
				fileInfo = new FileInfo(Path.Combine(str, this.OutputStream_xml));
				long num5 = num3;
				if (fileInfo.Exists)
				{
					num = fileInfo.Length;
				}
				else
				{
					num = (long)0;
				}
				num3 = num5 + num;
				fileInfo = new FileInfo(Path.Combine(str, this.ErrorStream_xml));
				long num6 = num3;
				if (fileInfo.Exists)
				{
					length1 = fileInfo.Length;
				}
				else
				{
					length1 = (long)0;
				}
				num3 = num6 + length1;
				fileInfo = new FileInfo(Path.Combine(str, this.WarningStream_xml));
				long num7 = num3;
				if (fileInfo.Exists)
				{
					num1 = fileInfo.Length;
				}
				else
				{
					num1 = (long)0;
				}
				num3 = num7 + num1;
				fileInfo = new FileInfo(Path.Combine(str, this.VerboseStream_xml));
				long num8 = num3;
				if (fileInfo.Exists)
				{
					length2 = fileInfo.Length;
				}
				else
				{
					length2 = (long)0;
				}
				num3 = num8 + length2;
				fileInfo = new FileInfo(Path.Combine(str, this.ProgressStream_xml));
				long num9 = num3;
				if (fileInfo.Exists)
				{
					num2 = fileInfo.Length;
				}
				else
				{
					num2 = (long)0;
				}
				num3 = num9 + num2;
				fileInfo = new FileInfo(Path.Combine(str, this.DebugStream_xml));
				long num10 = num3;
				if (fileInfo.Exists)
				{
					length3 = fileInfo.Length;
				}
				else
				{
					length3 = (long)0;
				}
				num3 = num10 + length3;
			}
			return num3;
		}

		private long GetSavedTimerLength()
		{
			long length;
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			long num = (long)0;
			if (Directory.Exists(str))
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(str, this.Timer_xml));
				long num1 = num;
				if (fileInfo.Exists)
				{
					length = fileInfo.Length;
				}
				else
				{
					length = (long)0;
				}
				num = num1 + length;
			}
			return num;
		}

		private void InternalDelete()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString());
			if (Directory.Exists(str))
			{
				try
				{
					long directoryLength = this.GetDirectoryLength(new DirectoryInfo(str));
					Directory.Delete(str, true);
					this.ReducePersistenceSize(directoryLength);
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					this.RetryDelete(oException, str);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					this.RetryDelete(unauthorizedAccessException, str);
				}
			}
		}

		private ArraySegment<byte> LoadFromFile(string filePath)
		{
			if (!this._version.EnableCompression)
			{
				ArraySegment<byte> nums = new ArraySegment<byte>();
				try
				{
					FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
					using (fileStream)
					{
						if (fileStream.Length != (long)0)
						{
							byte[] numArray = new byte[(IntPtr)fileStream.Length];
							fileStream.Read(numArray, 0, (int)fileStream.Length);
							nums = new ArraySegment<byte>(numArray, 0, (int)fileStream.Length);
						}
						fileStream.Close();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					throw;
				}
				return nums;
			}
			else
			{
				return this.LoadFromFile2(filePath);
			}
		}

		private ArraySegment<byte> LoadFromFile2(string filePath)
		{
			ArraySegment<byte> nums = new ArraySegment<byte>();
			try
			{
				FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
				using (fileStream)
				{
					if (fileStream.Length != (long)0)
					{
						using (GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress))
						{
							using (MemoryStream memoryStream = new MemoryStream())
							{
								gZipStream.CopyTo(memoryStream);
								nums = new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
								gZipStream.Close();
								memoryStream.Close();
							}
						}
					}
					fileStream.Close();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.TraceException(exception);
				throw;
			}
			return nums;
		}

		internal object LoadFromFileAndDeserialize(string filePath)
		{
			object obj;
			FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
			try
			{
				if (fileStream.Length == (long)0)
				{
					return null;
				}
				else
				{
					GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Decompress);
					try
					{
						try
						{
							byte[] numArray = new byte[1];
							gZipStream.Read(numArray, 0, 1);
							if (numArray[0] != 84)
							{
								XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
								obj = netDataContractSerializer.ReadObject(gZipStream);
							}
							else
							{
								gZipStream.Close();
								fileStream.Close();
								ArraySegment<byte> nums = this.LoadFromFile2(filePath);
								obj = this.DeserializeObject2(this.Decrypt(nums));
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							this.Tracer.TraceException(exception);
							throw;
						}
					}
					finally
					{
						gZipStream.Close();
						gZipStream.Dispose();
					}
				}
			}
			finally
			{
				fileStream.Close();
				fileStream.Dispose();
			}
			return obj;
		}

		private long LoadSerializedContext(object data)
		{
			this.serializedContext = this.Encrypt(this.SerializeObject(data));
			long count = (long)this.serializedContext.Count;
			return count;
		}

		private long LoadSerializedDefinition(object data)
		{
			object length;
			PSWorkflowDefinition pSWorkflowDefinition = (PSWorkflowDefinition)data;
			string workflowXaml = pSWorkflowDefinition.WorkflowXaml;
			string runtimeAssemblyPath = pSWorkflowDefinition.RuntimeAssemblyPath;
			if (workflowXaml == null)
			{
				length = null;
			}
			else
			{
				length = workflowXaml.Length;
			}
			long num = (long)length;
			if (!string.IsNullOrEmpty(runtimeAssemblyPath) && File.Exists(runtimeAssemblyPath))
			{
				FileInfo fileInfo = new FileInfo(runtimeAssemblyPath);
				num = num + fileInfo.Length;
			}
			return num;
		}

		private long LoadSerializedErrorException(object data)
		{
			this.serializedErrorException = this.Encrypt(this.SerializeObject(data));
			long count = (long)this.serializedErrorException.Count;
			return count;
		}

		private long LoadSerializedJobState(object data)
		{
			this.serializedJobState = this.Encrypt(this.SerializeObject(data));
			long count = (long)this.serializedJobState.Count;
			return count;
		}

		private long LoadSerializedMetadata(object data)
		{
			PSWorkflowContext pSWorkflowContext = (PSWorkflowContext)data;
			this.serializedWorkflowParameters = this.Encrypt(this.SerializeObject(pSWorkflowContext.WorkflowParameters));
			this.serializedPSWorkflowCommonParameters = this.Encrypt(this.SerializeObject(pSWorkflowContext.PSWorkflowCommonParameters));
			this.serializedJobMetadata = this.Encrypt(this.SerializeObject(pSWorkflowContext.JobMetadata));
			this.serializedPrivateMetadata = this.Encrypt(this.SerializeObject(pSWorkflowContext.PrivateMetadata));
			long count = (long)(this.serializedWorkflowParameters.Count + this.serializedPSWorkflowCommonParameters.Count + this.serializedJobMetadata.Count + this.serializedPrivateMetadata.Count);
			return count;
		}

		private long LoadSerializedStreamData(object data)
		{
			PowerShellStreams<PSObject, PSObject> powerShellStream = (PowerShellStreams<PSObject, PSObject>)data;
			this.serializedInputStreamData = this.Encrypt(this.SerializeObject(powerShellStream.InputStream));
			this.serializedOutputStreamData = this.Encrypt(this.SerializeObject(powerShellStream.OutputStream));
			this.serializedErrorStreamData = this.Encrypt(this.SerializeObject(powerShellStream.ErrorStream));
			this.serializedWarningStreamData = this.Encrypt(this.SerializeObject(powerShellStream.WarningStream));
			this.serializedVerboseStreamData = this.Encrypt(this.SerializeObject(powerShellStream.VerboseStream));
			this.serializedProgressStreamData = this.Encrypt(this.SerializeObject(powerShellStream.ProgressStream));
			this.serializedDebugStreamData = this.Encrypt(this.SerializeObject(powerShellStream.DebugStream));
			long count = (long)(this.serializedInputStreamData.Count + this.serializedOutputStreamData.Count + this.serializedErrorStreamData.Count + this.serializedWarningStreamData.Count + this.serializedVerboseStreamData.Count + this.serializedProgressStreamData.Count + this.serializedDebugStreamData.Count);
			return count;
		}

		private long LoadSerializedTimer(PSWorkflowTimer data)
		{
			this.serializedTimerData = this.Encrypt(this.SerializeObject(data.GetSerializedData()));
			long count = (long)this.serializedTimerData.Count;
			return count;
		}

		private void ReducePersistenceSize(long value)
		{
			this.CalculatePersistenceStoreSizeForFirstTime();
			lock (PSWorkflowFileInstanceStore.MaxPersistenceStoreSizeLock)
			{
				PSWorkflowFileInstanceStore.CurrentPersistenceStoreSize = PSWorkflowFileInstanceStore.CurrentPersistenceStoreSize - value;
			}
		}

		private void RetryDelete(Exception e, string instanceFolder)
		{
			this.Tracer.TraceException(e);
			this.Tracer.WriteMessage("Trying to delete one more time.");
			Directory.Delete(instanceFolder, true);
		}

		private void SaveSerializedContext()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.WorkflowState);
			this.EnsureInstancePath(str);
			this.SaveToFile(this.serializedContext, Path.Combine(str, this.State_xml));
		}

		private void SaveSerializedDefinition(object data)
		{
			PSWorkflowDefinition pSWorkflowDefinition = (PSWorkflowDefinition)data;
			if (this.firstTimeStoringDefinition)
			{
				this.firstTimeStoringDefinition = false;
				string workflowXaml = pSWorkflowDefinition.WorkflowXaml;
				string runtimeAssemblyPath = pSWorkflowDefinition.RuntimeAssemblyPath;
				Guid id = base.PSWorkflowInstance.Id;
				string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Definition);
				this.EnsureInstancePath(str);
				try
				{
					File.WriteAllText(Path.Combine(str, this.WorkflowDefinition_xaml), workflowXaml);
					if (!string.IsNullOrEmpty(runtimeAssemblyPath) && File.Exists(runtimeAssemblyPath))
					{
						File.Copy(runtimeAssemblyPath, Path.Combine(str, this.RuntimeAssembly_dll));
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					throw;
				}
			}
		}

		private void SaveSerializedErrorException()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Error);
			this.EnsureInstancePath(str);
			this.SaveToFile(this.serializedErrorException, Path.Combine(str, this.ErrorException_xml));
		}

		private void SaveSerializedJobState()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			this.EnsureInstancePath(str);
			this.SaveToFile(this.serializedJobState, Path.Combine(str, this.WorkflowInstanceState_xml));
		}

		private void SaveSerializedMetadata()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			this.EnsureInstancePath(str);
			this.SaveToFile(this.serializedWorkflowParameters, Path.Combine(str, this.Input_xml));
			this.SaveToFile(this.serializedPSWorkflowCommonParameters, Path.Combine(str, this.PSWorkflowCommonParameters_xml));
			this.SaveToFile(this.serializedJobMetadata, Path.Combine(str, this.JobMetadata_xml));
			this.SaveToFile(this.serializedPrivateMetadata, Path.Combine(str, this.PrivateMetadata_xml));
		}

		private void SaveSerializedStreamData()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Streams);
			this.EnsureInstancePath(str);
			this.SaveToFile(this.serializedInputStreamData, Path.Combine(str, this.InputStream_xml));
			this.SaveToFile(this.serializedOutputStreamData, Path.Combine(str, this.OutputStream_xml));
			this.SaveToFile(this.serializedErrorStreamData, Path.Combine(str, this.ErrorStream_xml));
			this.SaveToFile(this.serializedWarningStreamData, Path.Combine(str, this.WarningStream_xml));
			this.SaveToFile(this.serializedVerboseStreamData, Path.Combine(str, this.VerboseStream_xml));
			this.SaveToFile(this.serializedProgressStreamData, Path.Combine(str, this.ProgressStream_xml));
			this.SaveToFile(this.serializedDebugStreamData, Path.Combine(str, this.DebugStream_xml));
		}

		private void SaveSerializedTimer()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString(), this.Metadatas);
			this.EnsureInstancePath(str);
			this.SaveToFile(this.serializedTimerData, Path.Combine(str, this.Timer_xml));
		}

		private void SaveToFile(ArraySegment<byte> source, string filePath)
		{
			if (!this._version.EnableCompression)
			{
				try
				{
					FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
					using (fileStream)
					{
						fileStream.Write(source.Array, 0, source.Count);
						fileStream.Flush();
						fileStream.Close();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					throw;
				}
				return;
			}
			else
			{
				this.SaveToFile2(source, filePath);
				return;
			}
		}

		private void SaveToFile2(ArraySegment<byte> source, string filePath)
		{
			try
			{
				FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
				using (fileStream)
				{
					using (GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Compress))
					{
						gZipStream.Write(source.Array, 0, source.Count);
						gZipStream.Flush();
						gZipStream.Close();
					}
					fileStream.Close();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.TraceException(exception);
				throw;
			}
		}

		private void SaveVersionFile()
		{
			Guid id = base.PSWorkflowInstance.Id;
			string str = Path.Combine(this._configuration.InstanceStorePath, id.ToString());
			this.EnsureInstancePath(str);
			this._version.save(Path.Combine(str, this.Version_xml));
		}

		internal int SerializeAndSaveToFile(object source, string filePath)
		{
			int length;
			if (!this._version.EnableEncryption)
			{
				FileStream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
				GZipStream gZipStream = new GZipStream(fileStream, CompressionMode.Compress);
				bool flag = false;
				Exception exception = null;
				try
				{
					try
					{
						XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
						gZipStream.Write(PSWorkflowFileInstanceStore.EncryptFalse, 0, 1);
						netDataContractSerializer.WriteObject(gZipStream, source);
						length = (int)fileStream.Length;
						return length;
					}
					catch (SerializationException serializationException1)
					{
						SerializationException serializationException = serializationException1;
						flag = true;
						exception = serializationException;
					}
					catch (InvalidDataContractException invalidDataContractException1)
					{
						InvalidDataContractException invalidDataContractException = invalidDataContractException1;
						flag = true;
						exception = invalidDataContractException;
					}
					catch (Exception exception2)
					{
						Exception exception1 = exception2;
						this.Tracer.TraceException(exception1);
						throw;
					}
					if (flag)
					{
						this.ThrowErrorOrWriteWarning(exception);
					}
					return 0;
				}
				finally
				{
					gZipStream.Close();
					fileStream.Close();
					gZipStream.Dispose();
					fileStream.Dispose();
				}
				return length;
			}
			else
			{
				ArraySegment<byte> nums = this.Encrypt(this.SerializeObject(source));
				this.SaveToFile2(nums, filePath);
				return nums.Count;
			}
		}

		internal ArraySegment<byte> SerializeObject(object source)
		{
			if (!this._version.EnableCompression)
			{
				ArraySegment<byte> nums = new ArraySegment<byte>(PSWorkflowFileInstanceStore.NullArray);
				try
				{
					XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
					using (MemoryStream memoryStream = new MemoryStream())
					{
						netDataContractSerializer.WriteObject(memoryStream, source);
						nums = new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
					}
				}
				catch (SerializationException serializationException1)
				{
					SerializationException serializationException = serializationException1;
					this.ThrowErrorOrWriteWarning(serializationException);
				}
				catch (InvalidDataContractException invalidDataContractException1)
				{
					InvalidDataContractException invalidDataContractException = invalidDataContractException1;
					this.ThrowErrorOrWriteWarning(invalidDataContractException);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Tracer.TraceException(exception);
					throw;
				}
				return nums;
			}
			else
			{
				return this.SerializeObject2(source);
			}
		}

		internal ArraySegment<byte> SerializeObject2(object source)
		{
			ArraySegment<byte> nums = new ArraySegment<byte>(PSWorkflowFileInstanceStore.NullArray);
			try
			{
				XmlObjectSerializer netDataContractSerializer = new NetDataContractSerializer();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					netDataContractSerializer.WriteObject(memoryStream, source);
					byte[] numArray = new byte[(IntPtr)(memoryStream.Length + (long)1)];
					numArray[0] = 70;
					Buffer.BlockCopy(memoryStream.GetBuffer(), 0, numArray, 1, (int)memoryStream.Length);
					nums = new ArraySegment<byte>(numArray, 0, (int)numArray.Length);
					memoryStream.Close();
				}
			}
			catch (SerializationException serializationException1)
			{
				SerializationException serializationException = serializationException1;
				this.ThrowErrorOrWriteWarning(serializationException);
			}
			catch (InvalidDataContractException invalidDataContractException1)
			{
				InvalidDataContractException invalidDataContractException = invalidDataContractException1;
				this.ThrowErrorOrWriteWarning(invalidDataContractException);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this.Tracer.TraceException(exception);
				throw;
			}
			return nums;
		}

		private void ThrowErrorOrWriteWarning(Exception e)
		{
			if (!PSSessionConfigurationData.IsServerManager)
			{
				this.InternalDelete();
				this.serializationErrorHasOccured = true;
				SerializationException serializationException = new SerializationException(Resources.SerializationErrorException, e);
				throw serializationException;
			}
			else
			{
				object[] message = new object[1];
				message[0] = e.Message;
				string str = string.Format(CultureInfo.CurrentCulture, Resources.SerializationWarning, message);
				this.WriteWarning(str);
				return;
			}
		}

		private void WriteWarning(string warningMessage)
		{
			this.Tracer.WriteMessage("WorkflowAdditionalStores", "WriteWarning", base.PSWorkflowInstance.Id, warningMessage, new string[0]);
			if (base.PSWorkflowInstance.Streams.WarningStream.IsOpen)
			{
				base.PSWorkflowInstance.Streams.WarningStream.Add(new WarningRecord(warningMessage));
			}
		}
	}
}