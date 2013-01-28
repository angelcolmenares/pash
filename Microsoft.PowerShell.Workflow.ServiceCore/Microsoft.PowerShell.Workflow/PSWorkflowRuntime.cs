using Microsoft.PowerShell.Activities;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Tracing;

namespace Microsoft.PowerShell.Workflow
{
	public class PSWorkflowRuntime : PSWorkflowHost
	{
		private static PSWorkflowRuntime powerShellWorkflowHostInstance;

		private readonly static object syncLock;

		private RunspaceProvider _runspaceProvider;

		private RunspaceProvider _localRunspaceProvider;

		private RunspaceProvider _unboundedLocalRunspaceProvider;

		private readonly static Tracer _tracer;

		private readonly static PSPerfCountersMgr _psPerfCountersMgrInst;

		private readonly PSWorkflowConfigurationProvider _configuration;

		private readonly object _syncObject;

		private PSWorkflowJobManager jobManager;

		private PSActivityHostController activityHostController;

		public virtual PSWorkflowConfigurationProvider Configuration
		{
			get
			{
				return this._configuration;
			}
		}

		internal static PSWorkflowRuntime Instance
		{
			get
			{
				if (PSWorkflowRuntime.powerShellWorkflowHostInstance == null)
				{
					lock (PSWorkflowRuntime.syncLock)
					{
						if (PSWorkflowRuntime.powerShellWorkflowHostInstance == null)
						{
							PSWorkflowRuntime.powerShellWorkflowHostInstance = new PSWorkflowRuntime();
						}
					}
				}
				return PSWorkflowRuntime.powerShellWorkflowHostInstance;
			}
		}

		public virtual PSWorkflowJobManager JobManager
		{
			get
			{
				PSWorkflowJobManager pSWorkflowJobManager;
				if (this.jobManager == null)
				{
					lock (PSWorkflowRuntime.syncLock)
					{
						if (this.jobManager == null)
						{
							this.jobManager = new PSWorkflowJobManager(this, this._configuration.MaxRunningWorkflows);
							return this.jobManager;
						}
						else
						{
							pSWorkflowJobManager = this.jobManager;
						}
					}
					return pSWorkflowJobManager;
				}
				else
				{
					return this.jobManager;
				}
			}
		}

		public override RunspaceProvider LocalRunspaceProvider
		{
			get
			{
				if (this._localRunspaceProvider == null)
				{
					lock (this._syncObject)
					{
						if (this._localRunspaceProvider == null)
						{
							this._localRunspaceProvider = this._configuration.CreateLocalRunspaceProvider(false);
						}
					}
				}
				return this._localRunspaceProvider;
			}
		}

		public override PSActivityHostController PSActivityHostController
		{
			get
			{
				PSActivityHostController pSActivityHostController;
				if (this.activityHostController == null)
				{
					lock (PSWorkflowRuntime.syncLock)
					{
						if (this.activityHostController == null)
						{
							this.activityHostController = this._configuration.CreatePSActivityHostController();
							return this.activityHostController;
						}
						else
						{
							pSActivityHostController = this.activityHostController;
						}
					}
					return pSActivityHostController;
				}
				else
				{
					return this.activityHostController;
				}
			}
		}

		public override RunspaceProvider RemoteRunspaceProvider
		{
			get
			{
				if (this._runspaceProvider == null)
				{
					lock (this._syncObject)
					{
						if (this._runspaceProvider == null)
						{
							this._runspaceProvider = this._configuration.CreateRemoteRunspaceProvider();
						}
					}
				}
				return this._runspaceProvider;
			}
		}

		internal Tracer Tracer
		{
			get
			{
				return PSWorkflowRuntime._tracer;
			}
		}

		public override RunspaceProvider UnboundedLocalRunspaceProvider
		{
			get
			{
				if (this._unboundedLocalRunspaceProvider == null)
				{
					lock (this._syncObject)
					{
						if (this._unboundedLocalRunspaceProvider == null)
						{
							this._unboundedLocalRunspaceProvider = this._configuration.CreateLocalRunspaceProvider(true);
						}
					}
				}
				return this._unboundedLocalRunspaceProvider;
			}
		}

		static PSWorkflowRuntime()
		{
			PSWorkflowRuntime.syncLock = new object();
			PSWorkflowRuntime._tracer = new Tracer();
			PSWorkflowRuntime._psPerfCountersMgrInst = PSPerfCountersMgr.Instance;
		}

		public PSWorkflowRuntime()
		{
			this._syncObject = new object();
			this._configuration = new PSWorkflowConfigurationProvider();
			this._configuration.Runtime = this;
			PSCounterSetRegistrar pSCounterSetRegistrar = new PSCounterSetRegistrar(PSWorkflowPerformanceCounterSetInfo.ProviderId, PSWorkflowPerformanceCounterSetInfo.CounterSetId, PSWorkflowPerformanceCounterSetInfo.CounterSetType, PSWorkflowPerformanceCounterSetInfo.CounterInfoArray, null);
			PSWorkflowRuntime._psPerfCountersMgrInst.AddCounterSetInstance(pSCounterSetRegistrar);
			PSModuleInfo.UseAppDomainLevelModuleCache = true;
		}

		public PSWorkflowRuntime(PSWorkflowConfigurationProvider configuration)
		{
			this._syncObject = new object();
			if (configuration != null)
			{
				PSLanguageMode? languageMode = configuration.LanguageMode;
				if (!languageMode.HasValue || !languageMode.HasValue || languageMode.Value != PSLanguageMode.NoLanguage && languageMode.Value != PSLanguageMode.RestrictedLanguage)
				{
					this._configuration = configuration;
					this._configuration.Runtime = this;
					return;
				}
				else
				{
					object[] str = new object[1];
					str[0] = languageMode.Value.ToString();
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.NotSupportedLanguageMode, str));
				}
			}
			else
			{
				throw new ArgumentNullException("configuration");
			}
		}

		internal void SetWorkflowJobManager(PSWorkflowJobManager wfJobManager)
		{
			lock (PSWorkflowRuntime.syncLock)
			{
				this.jobManager = wfJobManager;
			}
		}
	}
}