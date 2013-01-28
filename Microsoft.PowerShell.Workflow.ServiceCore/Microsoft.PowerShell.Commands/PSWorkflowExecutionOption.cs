using Microsoft.PowerShell.Workflow;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	public sealed class PSWorkflowExecutionOption : PSSessionTypeOption
	{
		private const string PrivateDataFormat = "<PrivateData>{0}</PrivateData>";

		private const string ParamToken = "<Param Name='{0}' Value='{1}' />";

		private string persistencePath;

		private long maxPersistenceStoreSizeGB;

		private bool persistWithEncryption;

		private int maxRunningWorkflows;

		private string[] allowedActivity;

		private bool enableValidation;

		private string[] outOfProcessActivity;

		private int maxDisconnectedSessions;

		private int maxConnectedSessions;

		private int maxSessionsPerWorkflow;

		private int maxSessionsPerRemoteNode;

		private int maxActivityProcesses;

		private int activityProcessIdleTimeoutSec;

		private int workflowApplicationPersistUnloadTimeoutSec;

		private int remoteNodeSessionIdleTimeoutSec;

		private int sessionThrottleLimit;

		private int workflowShutdownTimeoutMSec;

		public int ActivityProcessIdleTimeoutSec
		{
			get
			{
				return this.activityProcessIdleTimeoutSec;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.activityProcessIdleTimeoutSec = value;
			}
		}

		public string[] AllowedActivity
		{
			get
			{
				return this.allowedActivity;
			}
			set
			{
				this.allowedActivity = value;
			}
		}

		public bool EnableValidation
		{
			get
			{
				return this.enableValidation;
			}
			set
			{
				this.enableValidation = value;
			}
		}

		public int MaxActivityProcesses
		{
			get
			{
				return this.maxActivityProcesses;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.maxActivityProcesses = value;
			}
		}

		public int MaxConnectedSessions
		{
			get
			{
				return this.maxConnectedSessions;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.maxConnectedSessions = value;
			}
		}

		public int MaxDisconnectedSessions
		{
			get
			{
				return this.maxDisconnectedSessions;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.maxDisconnectedSessions = value;
			}
		}

		public long MaxPersistenceStoreSizeGB
		{
			get
			{
				return this.maxPersistenceStoreSizeGB;
			}
			set
			{
				this.maxPersistenceStoreSizeGB = value;
			}
		}

		public int MaxRunningWorkflows
		{
			get
			{
				return this.maxRunningWorkflows;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.maxRunningWorkflows = value;
			}
		}

		public int MaxSessionsPerRemoteNode
		{
			get
			{
				return this.maxSessionsPerRemoteNode;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.maxSessionsPerRemoteNode = value;
			}
		}

		public int MaxSessionsPerWorkflow
		{
			get
			{
				return this.maxSessionsPerWorkflow;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.maxSessionsPerWorkflow = value;
			}
		}

		public string[] OutOfProcessActivity
		{
			get
			{
				return this.outOfProcessActivity;
			}
			set
			{
				this.outOfProcessActivity = value;
			}
		}

		public string PersistencePath
		{
			get
			{
				return this.persistencePath;
			}
			set
			{
				this.persistencePath = value;
			}
		}

		public bool PersistWithEncryption
		{
			get
			{
				return this.persistWithEncryption;
			}
			set
			{
				this.persistWithEncryption = value;
			}
		}

		public int RemoteNodeSessionIdleTimeoutSec
		{
			get
			{
				return this.remoteNodeSessionIdleTimeoutSec;
			}
			set
			{
				this.ValidateRange(30, 0x7530, value);
				this.remoteNodeSessionIdleTimeoutSec = value;
			}
		}

		public int SessionThrottleLimit
		{
			get
			{
				return this.sessionThrottleLimit;
			}
			set
			{
				this.ValidateRange(1, 0x7fffffff, value);
				this.sessionThrottleLimit = value;
			}
		}

		internal int WorkflowApplicationPersistUnloadTimeoutSec
		{
			get
			{
				return this.workflowApplicationPersistUnloadTimeoutSec;
			}
			set
			{
				this.ValidateRange(0, 0x7fffffff, value);
				this.workflowApplicationPersistUnloadTimeoutSec = value;
			}
		}

		public int WorkflowShutdownTimeoutMSec
		{
			get
			{
				return this.workflowShutdownTimeoutMSec;
			}
			set
			{
				this.ValidateRange(0, 0x1388, value);
				this.workflowShutdownTimeoutMSec = value;
			}
		}

		internal PSWorkflowExecutionOption()
		{
			this.persistencePath = PSWorkflowConfigurationProvider.DefaultPersistencePath;
			this.maxPersistenceStoreSizeGB = PSWorkflowConfigurationProvider.DefaultMaxPersistenceStoreSizeGB;
			this.persistWithEncryption = PSWorkflowConfigurationProvider.DefaultPersistWithEncryption;
			this.maxRunningWorkflows = PSWorkflowConfigurationProvider.DefaultMaxRunningWorkflows;
			this.allowedActivity = (new List<string>(PSWorkflowConfigurationProvider.DefaultAllowedActivity)).ToArray();
			this.enableValidation = PSWorkflowConfigurationProvider.DefaultEnableValidation;
			this.outOfProcessActivity = (new List<string>(PSWorkflowConfigurationProvider.DefaultOutOfProcessActivity)).ToArray();
			this.maxDisconnectedSessions = PSWorkflowConfigurationProvider.DefaultMaxDisconnectedSessions;
			this.maxConnectedSessions = PSWorkflowConfigurationProvider.DefaultMaxConnectedSessions;
			this.maxSessionsPerWorkflow = PSWorkflowConfigurationProvider.DefaultMaxSessionsPerWorkflow;
			this.maxSessionsPerRemoteNode = PSWorkflowConfigurationProvider.DefaultMaxSessionsPerRemoteNode;
			this.maxActivityProcesses = PSWorkflowConfigurationProvider.DefaultMaxActivityProcesses;
			this.activityProcessIdleTimeoutSec = PSWorkflowConfigurationProvider.DefaultActivityProcessIdleTimeoutSec;
			this.workflowApplicationPersistUnloadTimeoutSec = PSWorkflowConfigurationProvider.DefaultWorkflowApplicationPersistUnloadTimeoutSec;
			this.remoteNodeSessionIdleTimeoutSec = PSWorkflowConfigurationProvider.DefaultRemoteNodeSessionIdleTimeoutSec;
			this.sessionThrottleLimit = PSWorkflowConfigurationProvider.DefaultSessionThrottleLimit;
			this.workflowShutdownTimeoutMSec = PSWorkflowConfigurationProvider.DefaultWorkflowShutdownTimeoutMSec;
		}

		protected override PSSessionTypeOption ConstructObjectFromPrivateData(string privateData)
		{
			return PSWorkflowConfigurationProvider.LoadConfig(privateData, null);
		}

		internal string ConstructPrivateDataInternal()
		{
			return this.ConstructPrivateData();
		}

		protected override void CopyUpdatedValuesFrom(PSSessionTypeOption updated)
		{
			if (updated != null)
			{
				PSWorkflowExecutionOption pSWorkflowExecutionOption = updated as PSWorkflowExecutionOption;
				if (pSWorkflowExecutionOption != null)
				{
					if (pSWorkflowExecutionOption.activityProcessIdleTimeoutSec != PSWorkflowConfigurationProvider.DefaultActivityProcessIdleTimeoutSec)
					{
						this.activityProcessIdleTimeoutSec = pSWorkflowExecutionOption.activityProcessIdleTimeoutSec;
					}
					if (pSWorkflowExecutionOption.workflowApplicationPersistUnloadTimeoutSec != PSWorkflowConfigurationProvider.DefaultWorkflowApplicationPersistUnloadTimeoutSec)
					{
						this.workflowApplicationPersistUnloadTimeoutSec = pSWorkflowExecutionOption.workflowApplicationPersistUnloadTimeoutSec;
					}
					if (!PSWorkflowExecutionOption.ListsMatch(pSWorkflowExecutionOption.allowedActivity, PSWorkflowConfigurationProvider.DefaultAllowedActivity))
					{
						this.allowedActivity = pSWorkflowExecutionOption.allowedActivity;
					}
					if (!string.Equals(pSWorkflowExecutionOption.persistencePath, PSWorkflowConfigurationProvider.DefaultPersistencePath, StringComparison.OrdinalIgnoreCase))
					{
						this.persistencePath = pSWorkflowExecutionOption.persistencePath;
					}
					if (pSWorkflowExecutionOption.maxPersistenceStoreSizeGB != PSWorkflowConfigurationProvider.DefaultMaxPersistenceStoreSizeGB)
					{
						this.maxPersistenceStoreSizeGB = pSWorkflowExecutionOption.maxPersistenceStoreSizeGB;
					}
					if (pSWorkflowExecutionOption.persistWithEncryption != PSWorkflowConfigurationProvider.DefaultPersistWithEncryption)
					{
						this.persistWithEncryption = pSWorkflowExecutionOption.persistWithEncryption;
					}
					if (pSWorkflowExecutionOption.remoteNodeSessionIdleTimeoutSec != PSWorkflowConfigurationProvider.DefaultRemoteNodeSessionIdleTimeoutSec)
					{
						this.remoteNodeSessionIdleTimeoutSec = pSWorkflowExecutionOption.remoteNodeSessionIdleTimeoutSec;
					}
					if (pSWorkflowExecutionOption.maxActivityProcesses != PSWorkflowConfigurationProvider.DefaultMaxActivityProcesses)
					{
						this.maxActivityProcesses = pSWorkflowExecutionOption.maxActivityProcesses;
					}
					if (pSWorkflowExecutionOption.maxConnectedSessions != PSWorkflowConfigurationProvider.DefaultMaxConnectedSessions)
					{
						this.maxConnectedSessions = pSWorkflowExecutionOption.maxConnectedSessions;
					}
					if (pSWorkflowExecutionOption.maxDisconnectedSessions != PSWorkflowConfigurationProvider.DefaultMaxDisconnectedSessions)
					{
						this.maxDisconnectedSessions = pSWorkflowExecutionOption.maxDisconnectedSessions;
					}
					if (pSWorkflowExecutionOption.maxRunningWorkflows != PSWorkflowConfigurationProvider.DefaultMaxRunningWorkflows)
					{
						this.maxRunningWorkflows = pSWorkflowExecutionOption.maxRunningWorkflows;
					}
					if (pSWorkflowExecutionOption.maxSessionsPerRemoteNode != PSWorkflowConfigurationProvider.DefaultMaxSessionsPerRemoteNode)
					{
						this.maxSessionsPerRemoteNode = pSWorkflowExecutionOption.maxSessionsPerRemoteNode;
					}
					if (pSWorkflowExecutionOption.maxSessionsPerWorkflow != PSWorkflowConfigurationProvider.DefaultMaxSessionsPerWorkflow)
					{
						this.maxSessionsPerWorkflow = pSWorkflowExecutionOption.maxSessionsPerWorkflow;
					}
					if (!PSWorkflowExecutionOption.ListsMatch(pSWorkflowExecutionOption.outOfProcessActivity, PSWorkflowConfigurationProvider.DefaultOutOfProcessActivity))
					{
						this.outOfProcessActivity = pSWorkflowExecutionOption.outOfProcessActivity;
					}
					if (pSWorkflowExecutionOption.enableValidation != PSWorkflowConfigurationProvider.DefaultEnableValidation)
					{
						this.enableValidation = pSWorkflowExecutionOption.enableValidation;
					}
					if (pSWorkflowExecutionOption.sessionThrottleLimit != PSWorkflowConfigurationProvider.DefaultSessionThrottleLimit)
					{
						this.sessionThrottleLimit = pSWorkflowExecutionOption.sessionThrottleLimit;
					}
					if (pSWorkflowExecutionOption.workflowShutdownTimeoutMSec != PSWorkflowConfigurationProvider.DefaultWorkflowShutdownTimeoutMSec)
					{
						this.workflowShutdownTimeoutMSec = pSWorkflowExecutionOption.workflowShutdownTimeoutMSec;
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("updated");
				}
			}
			else
			{
				throw new ArgumentNullException("updated");
			}
		}

		private static bool ListsMatch(IEnumerable<string> a, IEnumerable<string> b)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			foreach (string str in b)
			{
				IEnumerator<string> enumerator = b.GetEnumerator();
				using (enumerator)
				{
					while (enumerator.MoveNext())
					{
						string str1 = str;
						if (string.Compare(str, str1, StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				flag2 = false;
				return flag2;
			}
			IEnumerator<string> enumerator1 = b.GetEnumerator();
			using (enumerator1)
			{
				while (enumerator1.MoveNext())
				{
					string current = enumerator1.Current;
					IEnumerator<string> enumerator2 = a.GetEnumerator();
					using (enumerator2)
					{
						while (enumerator2.MoveNext())
						{
							string current1 = enumerator2.Current;
							if (string.Compare(current, current1, StringComparison.OrdinalIgnoreCase) != 0)
							{
								continue;
							}
							flag1 = true;
							break;
						}
					}
					if (flag1)
					{
						continue;
					}
					flag2 = false;
					return flag2;
				}
				return true;
			}
			return flag2;
		}

		private void ValidateRange(int min, int max, int value)
		{
			if (value < min || value > max)
			{
				object[] objArray = new object[3];
				objArray[0] = value;
				objArray[1] = min;
				objArray[2] = max;
				string str = string.Format(CultureInfo.InvariantCulture, Resources.ProvidedValueIsOutOfRange, objArray);
				throw new ArgumentException(str);
			}
			else
			{
				return;
			}
		}
	}
}