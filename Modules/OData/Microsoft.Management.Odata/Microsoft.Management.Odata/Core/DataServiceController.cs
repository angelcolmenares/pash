using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.GenericInvoke;
using Microsoft.Management.Odata.PS;
using Microsoft.Management.Odata.Schema;
using Microsoft.Management.Odata.Tracing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Services.Providers;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Security.Principal;

namespace Microsoft.Management.Odata.Core
{
	internal class DataServiceController
	{
		private static object syncObject;

		private static DataServiceController current;

		private SharedItemStore<Microsoft.Management.Odata.Schema.Schema, UserContext> schemaStore;

		private SharedItemStore<InitialSessionState, UserContext> intialSessionStateStore;

		private Dictionary<ManagementSystemType, ICommandManager> cmdManagers;

		private CustomAuthorizationHandler customAuthorizationHandler;

		public DSConfiguration Configuration
		{
			get;
			private set;
		}

		public static DataServiceController Current
		{
			get
			{
				if (DataServiceController.current == null)
				{
					lock (DataServiceController.syncObject)
					{
						if (DataServiceController.current == null)
						{
							DataServiceController.CreateDataServiceController(null, null);
						}
					}
				}
				return DataServiceController.current;
			}
		}

		public PerfCounters PerfCounters
		{
			get;
			private set;
		}

		public QuotaSystem QuotaSystem
		{
			get;
			private set;
		}

		public UserDataCache UserDataCache
		{
			get;
			private set;
		}

		static DataServiceController()
		{
			DataServiceController.syncObject = new object();
		}

		private DataServiceController(DSConfiguration settings, CustomAuthorizationHandler.ICustomContextStore customContextStore, ISchemaBuilder testSchemaBuilder)
		{
			TraceHelper.Current.CorrelateWithActivity(EtwActivity.GetActivityId());
			try
			{
				this.Configuration = settings;
				this.cmdManagers = new Dictionary<ManagementSystemType, ICommandManager>();
				this.PerfCounters = new PerfCounters(CurrentRequestHelper.EndPointAddress);
				this.UserDataCache = new UserDataCache(this.Configuration.Invocation.Lifetime);
				this.QuotaSystem = new QuotaSystem();
				InitialSessionStateManager initialSessionStateManager = new InitialSessionStateManager(this.Configuration.PowerShell.SessionConfig.Assembly, this.Configuration.PowerShell.SessionConfig.Type);
				this.intialSessionStateStore = new SharedItemStore<InitialSessionState, UserContext>(initialSessionStateManager, this.Configuration.Quotas.UserSchemaTimeout, this.Configuration.Quotas.MaxUserSchemas);
				PSRunspaceFactory pSRunspaceFactory = new PSRunspaceFactory(this.intialSessionStateStore, true);
				int runspaceTimeout = this.Configuration.PowerShell.Quotas.RunspaceTimeout;
				ExclusiveItemStore<PSRunspace, UserContext> exclusiveItemStore = new ExclusiveItemStore<PSRunspace, UserContext>(pSRunspaceFactory, runspaceTimeout, this.Configuration.PowerShell.Quotas.MaxRunspaces);
				PSCommandManager pSCommandManager = new PSCommandManager(exclusiveItemStore);
				this.cmdManagers.Add(ManagementSystemType.PowerShell, pSCommandManager);
				PSRunspaceFactory pSRunspaceFactory1 = new PSRunspaceFactory(this.intialSessionStateStore, false);
				ExclusiveItemStore<PSRunspace, UserContext> exclusiveItemStore1 = new ExclusiveItemStore<PSRunspace, UserContext>(pSRunspaceFactory1, runspaceTimeout, this.Configuration.PowerShell.Quotas.MaxRunspaces);
				this.cmdManagers.Add(ManagementSystemType.GenericInvoke, new GICommandManager(exclusiveItemStore1));
				List<ISchemaBuilder> schemaBuilders = new List<ISchemaBuilder>();
				if (testSchemaBuilder != null)
				{
					schemaBuilders.Add(testSchemaBuilder);
				}
				schemaBuilders.Add(new PSSchemaBuilder(exclusiveItemStore));
				schemaBuilders.Add(new GISchemaBuilder());
				SchemaFactory schemaFactory = new SchemaFactory(this.Configuration.SchemaFileName, this.Configuration.ResourceMappingFileName, schemaBuilders, settings);
				this.schemaStore = new SharedItemStore<Microsoft.Management.Odata.Schema.Schema, UserContext>(schemaFactory, this.Configuration.Quotas.UserSchemaTimeout, this.Configuration.Quotas.MaxUserSchemas);
				this.customAuthorizationHandler = new CustomAuthorizationHandler(this.Configuration.CustomAuthorization.Assembly, this.Configuration.CustomAuthorization.Type, customContextStore);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.DataServiceControllerCreationFailedOperational(exception.Message);
				if (TraceHelper.IsEnabled(5))
				{
					TraceHelper.Current.DebugMessage(exception.ToTraceMessage("DataServiceController failed to create"));
				}
				if (this.PerfCounters != null)
				{
					this.PerfCounters.Dispose();
				}
				throw;
			}
			TraceHelper.Current.DataServiceControllerCreationSucceeded();
		}

		public static void CreateDataServiceController(DSConfiguration settings, CustomAuthorizationHandler.ICustomContextStore customContextStore)
		{
			DataServiceController.CreateDataServiceController(settings, customContextStore, null);
		}

		public static void CreateDataServiceController(DSConfiguration settings, CustomAuthorizationHandler.ICustomContextStore customContextStore, ISchemaBuilder testSchemaBuilder)
		{
			string filePath;
			lock (DataServiceController.syncObject)
			{
				Configuration configuration = null;
				if (settings == null)
				{
					try
					{
						settings = DSConfiguration.GetSection(configuration);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						Microsoft.Management.Odata.Tracing.Tracer current = TraceHelper.Current;
						string str = "config file is ";
						if (configuration == null)
						{
							filePath = "(not available)";
						}
						else
						{
							filePath = configuration.FilePath;
						}
						current.DebugMessage(string.Concat(str, filePath));
						if (TraceHelper.IsEnabled(5))
						{
							TraceHelper.Current.DebugMessage(exception.ToTraceMessage("DataServiceController failed to create"));
						}
						TraceHelper.Current.DataServiceControllerCreationFailedOperational(exception.Message);
						throw;
					}
				}
				DataServiceController.current = new DataServiceController(settings, customContextStore, testSchemaBuilder);
			}
		}

		public static void DeleteDataServiceController()
		{
			lock (DataServiceController.syncObject)
			{
				if (DataServiceController.current != null)
				{
					DataServiceController.current.PerfCounters.Dispose();
					DataServiceController.current = null;
				}
			}
		}

		public SafeRefCountedContainer<WindowsIdentity> GetAuthorizedUserIdentity(UserContext userContext)
		{
			return this.customAuthorizationHandler.GetAuthorizedUserIdentity(userContext);
		}

		public ICommand GetCommand(CommandType commandType, UserContext userContext, ResourceType entityType, EntityMetadata entityMetadata, string membershipId)
		{
			return this.cmdManagers[entityMetadata.MgmtSystem].GetCommand(commandType, userContext, entityType, entityMetadata, membershipId);
		}

		public DataContext GetCurrentContext()
		{
			return this.customAuthorizationHandler.GetDataContext();
		}

		public Uri GetCurrentResourceUri()
		{
			return CurrentRequestHelper.Uri;
		}

		public string GetMembershipId(UserContext userContext, Uri resourceUri)
		{
			return this.customAuthorizationHandler.GetMembershipId(userContext, resourceUri);
		}

		public IReferenceSetCommand GetReferenceSetCommand(CommandType commandType, UserContext userContext, ResourceProperty property, EntityMetadata entityMetadata, string membershipId, ResourceType entityType = null)
		{
			return this.cmdManagers[entityMetadata.MgmtSystem].GetReferenceSetCommand(commandType, userContext, property, entityMetadata, membershipId, entityType);
		}

		public Envelope<Microsoft.Management.Odata.Schema.Schema, UserContext> GetSchema(UserContext userContext, string membershipId)
		{
			return this.schemaStore.Borrow(userContext, membershipId);
		}

		public UserQuota GetUserQuota(UserContext userContext)
		{
			return this.customAuthorizationHandler.GetUserQuota(userContext);
		}

		public int IncrementCmdletExecutionCount(UserContext userContext)
		{
			return this.customAuthorizationHandler.IncrementCmdletExecutionCount(userContext);
		}

		public bool IsAuthorized(UserContext userContext, Uri resourceUri)
		{
			return this.customAuthorizationHandler.IsAuthorized(userContext, resourceUri);
		}

		public static bool IsCurrentInstancePresent()
		{
			bool flag;
			lock (DataServiceController.syncObject)
			{
				flag = DataServiceController.current != null;
			}
			return flag;
		}

		public bool IsRequestProcessingStarted(UserContext userContext)
		{
			return this.customAuthorizationHandler.IsRequestProcessingStarted(userContext);
		}

		public void SetCurrentContext(DataContext dataContext)
		{
			this.customAuthorizationHandler.SetDataContext(dataContext);
		}

		public void SetCustomStateStore(CustomAuthorizationHandler.ICustomContextStore context)
		{
			this.customAuthorizationHandler.SetCustomStateStore(context);
		}

		public void SetRequestProcessingState(UserContext userContext, bool requestProcessingState)
		{
			this.customAuthorizationHandler.SetRequestProcessingState(userContext, requestProcessingState);
		}
	}
}