using Microsoft.PowerShell.Activities;
using Microsoft.PowerShell.Commands;
using System;
using System.Activities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Security.Principal;
using System.Xml;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowConfigurationProvider
	{
		private const string PrivateDataToken = "PrivateData";

		private const string NameToken = "Name";

		private const string ParamToken = "Param";

		private const string ValueToken = "Value";

		internal const string PSDefaultActivities = "PSDefaultActivities";

		internal const string TokenPersistencePath = "persistencepath";

		internal const string TokenPersistWithEncryption = "persistwithencryption";

		internal const string TokenMaxPersistenceStoreSizeGB = "maxpersistencestoresizegb";

		internal const string TokenMaxRunningWorkflows = "maxrunningworkflows";

		internal const int MinMaxRunningWorkflows = 1;

		internal const int MaxMaxRunningWorkflows = 0x7fffffff;

		internal const string TokenAllowedActivity = "allowedactivity";

		internal const string TokenOutOfProcessActivity = "outofprocessactivity";

		internal const string TokenEnableValidation = "enablevalidation";

		internal const string TokenMaxDisconnectedSessions = "maxdisconnectedsessions";

		internal const int MinMaxDisconnectedSessions = 1;

		internal const int MaxMaxDisconnectedSessions = 0x7fffffff;

		internal const string TokenMaxConnectedSessions = "maxconnectedsessions";

		internal const int MinMaxConnectedSessions = 1;

		internal const int MaxMaxConnectedSessions = 0x7fffffff;

		internal const string TokenMaxSessionsPerWorkflow = "maxsessionsperworkflow";

		internal const int MinMaxSessionsPerWorkflow = 1;

		internal const int MaxMaxSessionsPerWorkflow = 0x7fffffff;

		internal const string TokenMaxSessionsPerRemoteNode = "maxsessionsperremotenode";

		internal const int MinMaxSessionsPerRemoteNode = 1;

		internal const int MaxMaxSessionsPerRemoteNode = 0x7fffffff;

		internal const string TokenMaxActivityProcesses = "maxactivityprocesses";

		internal const int MinMaxActivityProcesses = 1;

		internal const int MaxMaxActivityProcesses = 0x7fffffff;

		internal const string TokenActivityProcessIdleTimeoutSec = "activityprocessidletimeoutsec";

		internal const int MinActivityProcessIdleTimeoutSec = 1;

		internal const int MaxActivityProcessIdleTimeoutSec = 0x7fffffff;

		internal const string TokenWorkflowApplicationPersistUnloadTimeoutSec = "workflowapplicationpersistunloadtimeoutsec";

		internal const int MinWorkflowApplicationPersistUnloadTimeoutSec = 0;

		internal const int MaxWorkflowApplicationPersistUnloadTimeoutSec = 0x7fffffff;

		internal const string TokenRemoteNodeSessionIdleTimeoutSec = "remotenodesessionidletimeoutsec";

		internal const int MinRemoteNodeSessionIdleTimeoutSec = 30;

		internal const int MaxRemoteNodeSessionIdleTimeoutSec = 0x7530;

		internal const string TokenSessionThrottleLimit = "sessionthrottlelimit";

		internal const int MinSessionThrottleLimit = 1;

		internal const int MaxSessionThrottleLimit = 0x7fffffff;

		internal const string TokenValidationCacheLimit = "validationcachelimit";

		internal const string TokenCompiledAssemblyCacheLimit = "compiledassemblycachelimit";

		internal const string TokenOutOfProcessActivityCacheLimit = "outofprocessactivitycachelimit";

		internal const string TokenWorkflowShutdownTimeoutMSec = "workflowshutdowntimeoutmsec";

		internal const int MinWorkflowShutdownTimeoutMSec = 0;

		internal const int MaxWorkflowShutdownTimeoutMSec = 0x1388;

		internal const string TokenMaxInProcRunspaces = "maxinprocrunspaces";

		private string _configProviderId;

		private string _privateData;

		private readonly object _syncObject;

		private bool _isPopulated;

		private PSWorkflowExecutionOption _wfOptions;

		internal readonly static string DefaultPersistencePath;

		internal readonly static bool DefaultPersistWithEncryption;

		internal readonly static long DefaultMaxPersistenceStoreSizeGB;

		internal readonly static int DefaultMaxRunningWorkflows;

		internal readonly static IEnumerable<string> DefaultAllowedActivity;

		internal readonly static IEnumerable<string> DefaultOutOfProcessActivity;

		internal readonly static bool DefaultEnableValidation;

		internal readonly static int DefaultMaxDisconnectedSessions;

		internal readonly static int DefaultMaxConnectedSessions;

		internal readonly static int DefaultMaxSessionsPerWorkflow;

		internal readonly static int DefaultMaxSessionsPerRemoteNode;

		internal readonly static int DefaultMaxActivityProcesses;

		internal readonly static int DefaultActivityProcessIdleTimeoutSec;

		internal readonly static int DefaultWorkflowApplicationPersistUnloadTimeoutSec;

		internal readonly static int DefaultRemoteNodeSessionIdleTimeoutSec;

		private int _remoteNodeSessionIdleTimeoutSec;

		internal readonly static int DefaultSessionThrottleLimit;

		internal readonly static int DefaultValidationCacheLimit;

		private int _validationCacheLimit;

		internal readonly static int DefaultCompiledAssemblyCacheLimit;

		private int _compiledAssemblyCacheLimit;

		internal readonly static int DefaultOutOfProcessActivityCacheLimit;

		private int _outOfProcessActivityCacheLimit;

		internal readonly static int DefaultPSPersistInterval;

		internal readonly static int DefaultWorkflowShutdownTimeoutMSec;

		internal readonly static int DefaultMaxInProcRunspaces;

		private int _maxInProcRunspaces;

		private readonly ConcurrentDictionary<Type, bool> outOfProcessActivityCache;

		private bool? _powerShellActivitiesAreAllowed;

		internal PSSenderInfo _senderInfo;

		private WindowsIdentity _currentIdentity;

		internal bool IsDefaultStorePath;

		private string _instanceStorePath;

		public virtual int ActivityProcessIdleTimeoutSec
		{
			get
			{
				return this._wfOptions.ActivityProcessIdleTimeoutSec;
			}
		}

		public virtual IEnumerable<string> AllowedActivity
		{
			get
			{
				return this._wfOptions.AllowedActivity;
			}
		}

		internal int CompiledAssemblyCacheLimit
		{
			get
			{
				return this._compiledAssemblyCacheLimit;
			}
		}

		internal string ConfigProviderId
		{
			get
			{
				return this._configProviderId;
			}
		}

		internal WindowsIdentity CurrentUserIdentity
		{
			get
			{
				WindowsIdentity windowsIdentity = this._currentIdentity;
				WindowsIdentity windowsIdentity1 = windowsIdentity;
				if (windowsIdentity == null)
				{
					WindowsIdentity current = WindowsIdentity.GetCurrent();
					WindowsIdentity windowsIdentity2 = current;
					this._currentIdentity = current;
					windowsIdentity1 = windowsIdentity2;
				}
				return windowsIdentity1;
			}
		}

		public virtual bool EnableValidation
		{
			get
			{
				return this._wfOptions.EnableValidation;
			}
		}

		internal string InstanceStorePath
		{
			get
			{
				string str;
				bool flag;
				if (this._instanceStorePath == null)
				{
					lock (this._syncObject)
					{
						if (this._instanceStorePath == null)
						{
							if (this.CurrentUserIdentity.User != null)
							{
								string persistencePath = this._wfOptions.PersistencePath;
								string defaultPersistencePath = persistencePath;
								if (persistencePath == null)
								{
									defaultPersistencePath = PSWorkflowConfigurationProvider.DefaultPersistencePath;
								}
								string str1 = this._configProviderId;
								string str2 = str1;
								if (str1 == null)
								{
									str2 = "default";
								}
								string str3 = Path.Combine(defaultPersistencePath, str2, this.CurrentUserIdentity.User.Value);
								WindowsPrincipal windowsPrincipal = new WindowsPrincipal(this.CurrentUserIdentity);
								bool flag1 = false;
								bool flag2 = false;
								bool flag3 = false;
								if (windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
								{
									flag1 = true;
								}
								if (!PSSessionConfigurationData.IsServerManager)
								{
									if (!windowsPrincipal.IsInRole(new SecurityIdentifier(WellKnownSidType.InteractiveSid, null)))
									{
										flag2 = true;
									}
									if (this._senderInfo != null && this._senderInfo.UserInfo != null && this._senderInfo.UserInfo.Identity != null && this._senderInfo.UserInfo.Identity.AuthenticationType != null && this._senderInfo.UserInfo.Identity.AuthenticationType.Equals("credssp", StringComparison.OrdinalIgnoreCase))
									{
										flag3 = true;
									}
								}
								if (!flag3)
								{
									if (flag1)
									{
										str3 = string.Concat(str3, "_EL");
									}
									if (flag2)
									{
										str3 = string.Concat(str3, "_NI");
									}
								}
								else
								{
									str3 = string.Concat(str3, "_CP");
								}
								this._instanceStorePath = str3;
							}
							PSWorkflowConfigurationProvider pSWorkflowConfigurationProvider = this;
							if (string.IsNullOrEmpty(this._configProviderId))
							{
								flag = true;
							}
							else
							{
								flag = false;
							}
							pSWorkflowConfigurationProvider.IsDefaultStorePath = flag;
							return this._instanceStorePath;
						}
						else
						{
							str = this._instanceStorePath;
						}
					}
					return str;
				}
				else
				{
					return this._instanceStorePath;
				}
			}
		}

		public virtual PSLanguageMode? LanguageMode
		{
			get
			{
				PSLanguageMode? nullable = null;
				return nullable;
			}
		}

		public virtual int MaxActivityProcesses
		{
			get
			{
				return this._wfOptions.MaxActivityProcesses;
			}
		}

		public virtual int MaxConnectedSessions
		{
			get
			{
				return this._wfOptions.MaxConnectedSessions;
			}
		}

		public virtual int MaxDisconnectedSessions
		{
			get
			{
				return this._wfOptions.MaxDisconnectedSessions;
			}
		}

		public virtual int MaxInProcRunspaces
		{
			get
			{
				return this._maxInProcRunspaces;
			}
		}

		internal long MaxPersistenceStoreSizeGB
		{
			get
			{
				return this._wfOptions.MaxPersistenceStoreSizeGB;
			}
		}

		public virtual int MaxRunningWorkflows
		{
			get
			{
				return this._wfOptions.MaxRunningWorkflows;
			}
		}

		public virtual int MaxSessionsPerRemoteNode
		{
			get
			{
				return this._wfOptions.MaxSessionsPerRemoteNode;
			}
		}

		internal int MaxSessionsPerWorkflow
		{
			get
			{
				return this._wfOptions.MaxSessionsPerWorkflow;
			}
		}

		public virtual IEnumerable<string> OutOfProcessActivity
		{
			get
			{
				return this._wfOptions.OutOfProcessActivity;
			}
		}

		internal int OutOfProcessActivityCacheLimit
		{
			get
			{
				return this._outOfProcessActivityCacheLimit;
			}
		}

		internal string PersistencePath
		{
			get
			{
				return this._wfOptions.PersistencePath;
			}
		}

		internal bool PersistWithEncryption
		{
			get
			{
				return this._wfOptions.PersistWithEncryption;
			}
		}

		internal bool PSDefaultActivitiesAreAllowed
		{
			get
			{
				if (!this._powerShellActivitiesAreAllowed.HasValue)
				{
					lock (this._syncObject)
					{
						if (!this._powerShellActivitiesAreAllowed.HasValue)
						{
							IEnumerable<string> allowedActivity = this.AllowedActivity;
							IEnumerable<string> strs = allowedActivity;
							if (allowedActivity == null)
							{
								strs = (IEnumerable<string>)(new string[0]);
							}
							bool flag = strs.Any<string>((string a) => string.Equals(a, "PSDefaultActivities", StringComparison.OrdinalIgnoreCase));
							this._powerShellActivitiesAreAllowed = new bool?(flag);
						}
					}
				}
				return this._powerShellActivitiesAreAllowed.Value;
			}
		}

		public virtual int PSWorkflowApplicationPersistUnloadTimeoutSec
		{
			get
			{
				return this._wfOptions.WorkflowApplicationPersistUnloadTimeoutSec;
			}
		}

		public virtual int RemoteNodeSessionIdleTimeoutSec
		{
			get
			{
				return this._wfOptions.RemoteNodeSessionIdleTimeoutSec;
			}
		}

		public PSWorkflowRuntime Runtime
		{
			get;
			internal set;
		}

		public virtual int SessionThrottleLimit
		{
			get
			{
				return this._wfOptions.SessionThrottleLimit;
			}
		}

		internal int ValidationCacheLimit
		{
			get
			{
				return this._validationCacheLimit;
			}
		}

		internal int WorkflowShutdownTimeoutMSec
		{
			get
			{
				return this._wfOptions.WorkflowShutdownTimeoutMSec;
			}
		}

		static PSWorkflowConfigurationProvider()
		{
			PSWorkflowConfigurationProvider.DefaultPersistencePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Windows\\PowerShell\\WF\\PS");
			PSWorkflowConfigurationProvider.DefaultPersistWithEncryption = false;
			PSWorkflowConfigurationProvider.DefaultMaxPersistenceStoreSizeGB = (long)10;
			PSWorkflowConfigurationProvider.DefaultMaxRunningWorkflows = 30;
			string[] strArrays = new string[1];
			strArrays[0] = "PSDefaultActivities";
			PSWorkflowConfigurationProvider.DefaultAllowedActivity = strArrays;
			string[] strArrays1 = new string[1];
			strArrays1[0] = "InlineScript";
			PSWorkflowConfigurationProvider.DefaultOutOfProcessActivity = strArrays1;
			PSWorkflowConfigurationProvider.DefaultEnableValidation = false;
			PSWorkflowConfigurationProvider.DefaultMaxDisconnectedSessions = 0x3e8;
			PSWorkflowConfigurationProvider.DefaultMaxConnectedSessions = 100;
			PSWorkflowConfigurationProvider.DefaultMaxSessionsPerWorkflow = 5;
			PSWorkflowConfigurationProvider.DefaultMaxSessionsPerRemoteNode = 5;
			PSWorkflowConfigurationProvider.DefaultMaxActivityProcesses = 5;
			PSWorkflowConfigurationProvider.DefaultActivityProcessIdleTimeoutSec = 60;
			PSWorkflowConfigurationProvider.DefaultWorkflowApplicationPersistUnloadTimeoutSec = 5;
			PSWorkflowConfigurationProvider.DefaultRemoteNodeSessionIdleTimeoutSec = 60;
			PSWorkflowConfigurationProvider.DefaultSessionThrottleLimit = 100;
			PSWorkflowConfigurationProvider.DefaultValidationCacheLimit = 0x2710;
			PSWorkflowConfigurationProvider.DefaultCompiledAssemblyCacheLimit = 0x2710;
			PSWorkflowConfigurationProvider.DefaultOutOfProcessActivityCacheLimit = 0x2710;
			PSWorkflowConfigurationProvider.DefaultPSPersistInterval = 30;
			PSWorkflowConfigurationProvider.DefaultWorkflowShutdownTimeoutMSec = 0x1f4;
			PSWorkflowConfigurationProvider.DefaultMaxInProcRunspaces = PSWorkflowConfigurationProvider.DefaultMaxRunningWorkflows * 2;
		}

		public PSWorkflowConfigurationProvider()
		{
			this._syncObject = new object();
			this._maxInProcRunspaces = PSWorkflowConfigurationProvider.DefaultMaxInProcRunspaces;
			this.outOfProcessActivityCache = new ConcurrentDictionary<Type, bool>();
			this._wfOptions = new PSWorkflowExecutionOption();
			this.LoadFromDefaults();
		}

		public PSWorkflowConfigurationProvider(string applicationPrivateData, string configProviderId)
		{
			this._syncObject = new object();
			this._maxInProcRunspaces = PSWorkflowConfigurationProvider.DefaultMaxInProcRunspaces;
			this.outOfProcessActivityCache = new ConcurrentDictionary<Type, bool>();
			this.Populate(applicationPrivateData, configProviderId);
		}

		public virtual RunspaceProvider CreateLocalRunspaceProvider(bool isUnbounded)
		{
			if (!isUnbounded)
			{
				return new LocalRunspaceProvider(this.RemoteNodeSessionIdleTimeoutSec, this.MaxInProcRunspaces, this.LanguageMode);
			}
			else
			{
				return new LocalRunspaceProvider(this.RemoteNodeSessionIdleTimeoutSec, this.LanguageMode);
			}
		}

		public virtual PSActivityHostController CreatePSActivityHostController()
		{
			return new PSOutOfProcessActivityController(this.Runtime);
		}

		public virtual PSWorkflowInstance CreatePSWorkflowInstance(PSWorkflowDefinition definition, PSWorkflowContext metadata, PSDataCollection<PSObject> pipelineInput, PSWorkflowJob job)
		{
			return new PSWorkflowApplicationInstance(this.Runtime, definition, metadata, pipelineInput, job);
		}

		public virtual PSWorkflowInstance CreatePSWorkflowInstance(PSWorkflowId instanceId)
		{
			return new PSWorkflowApplicationInstance(this.Runtime, instanceId);
		}

		public virtual PSWorkflowInstanceStore CreatePSWorkflowInstanceStore(PSWorkflowInstance workflowInstance)
		{
			return new PSWorkflowFileInstanceStore(this, workflowInstance);
		}

		public virtual RunspaceProvider CreateRemoteRunspaceProvider()
		{
			return new ConnectionManager(this.RemoteNodeSessionIdleTimeoutSec * 0x3e8, this.MaxSessionsPerRemoteNode, this.SessionThrottleLimit, this.MaxConnectedSessions, this.MaxDisconnectedSessions);
		}

		public virtual IEnumerable<Func<object>> CreateWorkflowExtensionCreationFunctions<T>()
		{
			return null;
		}

		public virtual IEnumerable<object> CreateWorkflowExtensions()
		{
			if (PSWorkflowExtensions.CustomHandler == null)
			{
				return null;
			}
			else
			{
				return PSWorkflowExtensions.CustomHandler();
			}
		}

		public virtual ActivityRunMode GetActivityRunMode(Activity activity)
		{
			if (activity != null)
			{
				if (this.outOfProcessActivityCache.Count >= this.OutOfProcessActivityCacheLimit)
				{
					this.outOfProcessActivityCache.Clear();
				}
				bool orAdd = this.outOfProcessActivityCache.GetOrAdd(activity.GetType(), new Func<Type, bool>(this.IsOutOfProcessActivity));
				if (orAdd)
				{
					return ActivityRunMode.OutOfProcess;
				}
				else
				{
					return ActivityRunMode.InProcess;
				}
			}
			else
			{
				throw new ArgumentNullException("activity");
			}
		}

		private static bool IsMatched(string allowedActivity, string match)
		{
			if (WildcardPattern.ContainsWildcardCharacters(allowedActivity))
			{
				return (new WildcardPattern(allowedActivity, WildcardOptions.IgnoreCase)).IsMatch(match);
			}
			else
			{
				return string.Equals(allowedActivity, match, StringComparison.OrdinalIgnoreCase);
			}
		}

		private bool IsOutOfProcessActivity(Type activityType)
		{
			IEnumerable<string> strs = this.OutOfProcessActivity;
			IEnumerable<string> strs1 = strs;
			if (strs == null)
			{
				strs1 = (IEnumerable<string>)(new string[0]);
			}
			return strs1.Any<string>((string outOfProcessActivity) => {
				if (PSWorkflowConfigurationProvider.IsMatched(outOfProcessActivity, activityType.Name) || PSWorkflowConfigurationProvider.IsMatched(outOfProcessActivity, activityType.FullName) || PSWorkflowConfigurationProvider.IsMatched(outOfProcessActivity, string.Concat(activityType.Assembly.GetName().Name, "\\", activityType.Name)) || PSWorkflowConfigurationProvider.IsMatched(outOfProcessActivity, string.Concat(activityType.Assembly.GetName().Name, "\\", activityType.FullName)) || PSWorkflowConfigurationProvider.IsMatched(outOfProcessActivity, string.Concat(activityType.Assembly.GetName().FullName, "\\", activityType.Name)))
				{
					return true;
				}
				else
				{
					return PSWorkflowConfigurationProvider.IsMatched(outOfProcessActivity, string.Concat(activityType.Assembly.GetName().FullName, "\\", activityType.FullName));
				}
			}
			);
		}

		internal static PSWorkflowExecutionOption LoadConfig(string privateData, PSWorkflowConfigurationProvider configuration)
		{
			PSWorkflowExecutionOption pSWorkflowExecutionOption = new PSWorkflowExecutionOption();
			if (!string.IsNullOrEmpty(privateData))
			{
				XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
				xmlReaderSetting.CheckCharacters = false;
				xmlReaderSetting.IgnoreComments = true;
				xmlReaderSetting.IgnoreProcessingInstructions = true;
				xmlReaderSetting.MaxCharactersInDocument = (long)0x2710;
				xmlReaderSetting.XmlResolver = null;
				xmlReaderSetting.ConformanceLevel = ConformanceLevel.Fragment;
				XmlReaderSettings xmlReaderSetting1 = xmlReaderSetting;
				XmlReader xmlReader = XmlReader.Create(new StringReader(privateData), xmlReaderSetting1);
				using (xmlReader)
				{
					if (xmlReader.ReadToFollowing("PrivateData"))
					{
						HashSet<string> strs = new HashSet<string>();
						bool descendant = xmlReader.ReadToDescendant("Param");
						while (descendant)
						{
							if (xmlReader.MoveToAttribute("Name"))
							{
								string value = xmlReader.Value;
								if (xmlReader.MoveToAttribute("Value"))
								{
									if (!strs.Contains(value.ToLower(CultureInfo.InvariantCulture)))
									{
										string str = xmlReader.Value;
										PSWorkflowConfigurationProvider.Update(value, str, pSWorkflowExecutionOption, configuration);
										strs.Add(value.ToLower(CultureInfo.InvariantCulture));
										descendant = xmlReader.ReadToFollowing("Param");
									}
									else
									{
										throw new PSArgumentException(Resources.ParamSpecifiedMoreThanOnce, value);
									}
								}
								else
								{
									throw new PSArgumentException(Resources.ValueNotSpecifiedForParam);
								}
							}
							else
							{
								throw new PSArgumentException(Resources.NameNotSpecifiedForParam);
							}
						}
					}
				}
				return pSWorkflowExecutionOption;
			}
			else
			{
				return pSWorkflowExecutionOption;
			}
		}

		private void LoadFromDefaults()
		{
			this._wfOptions.PersistencePath = PSWorkflowConfigurationProvider.DefaultPersistencePath;
			this._wfOptions.MaxPersistenceStoreSizeGB = PSWorkflowConfigurationProvider.DefaultMaxPersistenceStoreSizeGB;
			this._wfOptions.PersistWithEncryption = PSWorkflowConfigurationProvider.DefaultPersistWithEncryption;
			this._wfOptions.MaxRunningWorkflows = PSWorkflowConfigurationProvider.DefaultMaxRunningWorkflows;
			this._wfOptions.AllowedActivity = (new List<string>(PSWorkflowConfigurationProvider.DefaultAllowedActivity)).ToArray();
			this._wfOptions.OutOfProcessActivity = (new List<string>(PSWorkflowConfigurationProvider.DefaultOutOfProcessActivity)).ToArray();
			this._wfOptions.EnableValidation = PSWorkflowConfigurationProvider.DefaultEnableValidation;
			this._wfOptions.MaxDisconnectedSessions = PSWorkflowConfigurationProvider.DefaultMaxDisconnectedSessions;
			this._wfOptions.MaxConnectedSessions = PSWorkflowConfigurationProvider.DefaultMaxConnectedSessions;
			this._wfOptions.MaxSessionsPerWorkflow = PSWorkflowConfigurationProvider.DefaultMaxSessionsPerWorkflow;
			this._wfOptions.MaxSessionsPerRemoteNode = PSWorkflowConfigurationProvider.DefaultMaxSessionsPerRemoteNode;
			this._wfOptions.MaxActivityProcesses = PSWorkflowConfigurationProvider.DefaultMaxActivityProcesses;
			this._wfOptions.ActivityProcessIdleTimeoutSec = PSWorkflowConfigurationProvider.DefaultActivityProcessIdleTimeoutSec;
			this._wfOptions.WorkflowApplicationPersistUnloadTimeoutSec = PSWorkflowConfigurationProvider.DefaultWorkflowApplicationPersistUnloadTimeoutSec;
			this._wfOptions.RemoteNodeSessionIdleTimeoutSec = PSWorkflowConfigurationProvider.DefaultRemoteNodeSessionIdleTimeoutSec;
			this._wfOptions.SessionThrottleLimit = PSWorkflowConfigurationProvider.DefaultSessionThrottleLimit;
			this._validationCacheLimit = PSWorkflowConfigurationProvider.DefaultValidationCacheLimit;
			this._compiledAssemblyCacheLimit = PSWorkflowConfigurationProvider.DefaultCompiledAssemblyCacheLimit;
			this._outOfProcessActivityCacheLimit = PSWorkflowConfigurationProvider.DefaultOutOfProcessActivityCacheLimit;
			this._wfOptions.WorkflowShutdownTimeoutMSec = PSWorkflowConfigurationProvider.DefaultWorkflowShutdownTimeoutMSec;
			this.ResetCaching();
		}

		internal void Populate(string applicationPrivateData, string configProviderId, PSSenderInfo senderInfo)
		{
			this._senderInfo = senderInfo;
			this.Populate(applicationPrivateData, configProviderId);
		}

		public void Populate(string applicationPrivateData, string configProviderId)
		{
			string str;
			if (!this._isPopulated)
			{
				lock (this._syncObject)
				{
					if (!this._isPopulated)
					{
						this._privateData = applicationPrivateData;
						char[] chrArray = new char[1];
						chrArray[0] = '/';
						string[] strArrays = configProviderId.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
						PSWorkflowConfigurationProvider pSWorkflowConfigurationProvider = this;
						if ((int)strArrays.Length > 0)
						{
							str = strArrays[(int)strArrays.Length - 1];
						}
						else
						{
							str = configProviderId;
						}
						pSWorkflowConfigurationProvider._configProviderId = str;
						this._isPopulated = true;
						this._wfOptions = PSWorkflowConfigurationProvider.LoadConfig(this._privateData, this);
						this.ResetCaching();
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void ResetCaching()
		{
			this._powerShellActivitiesAreAllowed = null;
			this.outOfProcessActivityCache.Clear();
		}

		internal void ResetPopulate()
		{
			lock (this._syncObject)
			{
				this._isPopulated = false;
				this.LoadFromDefaults();
			}
		}

		private static void Update(string optionName, string value, PSWorkflowExecutionOption target, PSWorkflowConfigurationProvider configuration)
		{
			bool flag = false;
			bool flag1 = false;
			string lower = optionName.ToLower(CultureInfo.InvariantCulture);
			string str = lower;
			if (lower != null)
			{
				if (str == "workflowapplicationpersistunloadtimeoutsec")
				{
					target.WorkflowApplicationPersistUnloadTimeoutSec = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "activityprocessidletimeoutsec")
				{
					target.ActivityProcessIdleTimeoutSec = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "allowedactivity")
				{
					char[] chrArray = new char[1];
					chrArray[0] = ',';
					string[] strArrays = value.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
					PSWorkflowExecutionOption array = target;
					string[] strArrays1 = strArrays;
					array.AllowedActivity = strArrays1.Select<string, string>((string activity) => activity.Trim()).ToArray<string>();
					return;
				}
				else if (str == "enablevalidation")
				{
					if (!bool.TryParse(value, out flag))
					{
						return;
					}
					target.EnableValidation = flag;
					return;
				}
				else if (str == "persistencepath")
				{
					target.PersistencePath = Environment.ExpandEnvironmentVariables(value);
					return;
				}
				else if (str == "maxpersistencestoresizegb")
				{
					target.MaxPersistenceStoreSizeGB = (long)LanguagePrimitives.ConvertTo(value, typeof(long), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "persistwithencryption")
				{
					if (!bool.TryParse(value, out flag1))
					{
						return;
					}
					target.PersistWithEncryption = flag1;
					return;
				}
				else if (str == "remotenodesessionidletimeoutsec")
				{
					target.RemoteNodeSessionIdleTimeoutSec = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					if (configuration == null)
					{
						return;
					}
					configuration._remoteNodeSessionIdleTimeoutSec = target.RemoteNodeSessionIdleTimeoutSec;
					return;
				}
				else if (str == "maxactivityprocesses")
				{
					target.MaxActivityProcesses = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "maxconnectedsessions")
				{
					target.MaxConnectedSessions = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "maxdisconnectedsessions")
				{
					target.MaxDisconnectedSessions = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "maxrunningworkflows")
				{
					target.MaxRunningWorkflows = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					configuration._maxInProcRunspaces = target.MaxRunningWorkflows * 2;
					return;
				}
				else if (str == "maxsessionsperremotenode")
				{
					target.MaxSessionsPerRemoteNode = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "maxsessionsperworkflow")
				{
					target.MaxSessionsPerWorkflow = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "outofprocessactivity")
				{
					char[] chrArray1 = new char[1];
					chrArray1[0] = ',';
					string[] strArrays2 = value.Split(chrArray1, StringSplitOptions.RemoveEmptyEntries);
					PSWorkflowExecutionOption pSWorkflowExecutionOption = target;
					string[] strArrays3 = strArrays2;
					pSWorkflowExecutionOption.OutOfProcessActivity = strArrays3.Select<string, string>((string activity) => activity.Trim()).ToArray<string>();
					return;
				}
				else if (str == "sessionthrottlelimit")
				{
					target.SessionThrottleLimit = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "validationcachelimit")
				{
					if (configuration == null)
					{
						return;
					}
					configuration._validationCacheLimit = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "compiledassemblycachelimit")
				{
					if (configuration == null)
					{
						return;
					}
					configuration._compiledAssemblyCacheLimit = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "outofprocessactivitycachelimit")
				{
					if (configuration == null)
					{
						return;
					}
					configuration._outOfProcessActivityCacheLimit = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "workflowshutdowntimeoutmsec")
				{
					target.WorkflowShutdownTimeoutMSec = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				else if (str == "maxinprocrunspaces")
				{
					if (configuration == null)
					{
						return;
					}
					configuration._maxInProcRunspaces = (int)LanguagePrimitives.ConvertTo(value, typeof(int), CultureInfo.InvariantCulture);
					return;
				}
				return;
			}
		}
	}
}