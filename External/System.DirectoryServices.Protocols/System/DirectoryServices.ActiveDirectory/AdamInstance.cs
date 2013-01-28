using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class AdamInstance : DirectoryServer
	{
		private string[] becomeRoleOwnerAttrs;

		private bool disposed;

		private string cachedHostName;

		private int cachedLdapPort;

		private int cachedSslPort;

		private bool defaultPartitionInitialized;

		private bool defaultPartitionModified;

		private ConfigurationSet currentConfigSet;

		private string cachedDefaultPartition;

		private AdamRoleCollection cachedRoles;

		private IntPtr ADAMHandle;

		private IntPtr authIdentity;

		private SyncUpdateCallback userDelegate;

		private SyncReplicaFromAllServersCallback syncAllFunctionPointer;

		public ConfigurationSet ConfigurationSet
		{
			get
			{
				base.CheckIfDisposed();
				if (this.currentConfigSet == null)
				{
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, this.context);
					this.currentConfigSet = ConfigurationSet.GetConfigurationSet(newDirectoryContext);
				}
				return this.currentConfigSet;
			}
		}

		public string DefaultPartition
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.defaultPartitionInitialized || this.defaultPartitionModified)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
					try
					{
						cachedDirectoryEntry.RefreshCache();
						if (cachedDirectoryEntry.Properties[PropertyManager.MsDSDefaultNamingContext].Value != null)
						{
							this.cachedDefaultPartition = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.MsDSDefaultNamingContext);
						}
						else
						{
							this.cachedDefaultPartition = null;
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					this.defaultPartitionInitialized = true;
				}
				return this.cachedDefaultPartition;
			}
			set
			{
				base.CheckIfDisposed();
				DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
				if (value != null)
				{
					if (Utils.IsValidDNFormat(value))
					{
						if (base.Partitions.Contains(value))
						{
							cachedDirectoryEntry.Properties[PropertyManager.MsDSDefaultNamingContext].Value = value;
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = value;
							throw new ArgumentException(Res.GetString("ServerNotAReplica", objArray), "value");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidDNFormat"), "value");
					}
				}
				else
				{
					if (cachedDirectoryEntry.Properties.Contains(PropertyManager.MsDSDefaultNamingContext))
					{
						cachedDirectoryEntry.Properties[PropertyManager.MsDSDefaultNamingContext].Clear();
					}
				}
				this.defaultPartitionModified = true;
			}
		}

		public string HostName
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedHostName == null)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.ServerObjectName);
					this.cachedHostName = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DnsHostName);
				}
				return this.cachedHostName;
			}
		}

		public override ReplicationConnectionCollection InboundConnections
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get
			{
				return base.GetInboundConnectionsHelper();
			}
		}

		public override string IPAddress
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			[DnsPermission(SecurityAction.Assert, Unrestricted=true)]
			get
			{
				base.CheckIfDisposed();
				IPHostEntry hostEntry = Dns.GetHostEntry(this.HostName);
				if (hostEntry.AddressList.GetLength(0) <= 0)
				{
					return null;
				}
				else
				{
					return hostEntry.AddressList[0].ToString();
				}
			}
		}

		public int LdapPort
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedLdapPort == -1)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
					this.cachedLdapPort = (int)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.MsDSPortLDAP);
				}
				return this.cachedLdapPort;
			}
		}

		internal Guid NtdsaObjectGuid
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedNtdsaObjectGuid == Guid.Empty)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
					byte[] propertyValue = (byte[])PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.ObjectGuid);
					this.cachedNtdsaObjectGuid = new Guid(propertyValue);
				}
				return this.cachedNtdsaObjectGuid;
			}
		}

		internal string NtdsaObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedNtdsaObjectName == null)
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					try
					{
						try
						{
							this.cachedNtdsaObjectName = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.DsServiceName);
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
					}
					finally
					{
						directoryEntry.Dispose();
					}
				}
				return this.cachedNtdsaObjectName;
			}
		}

		public override ReplicationConnectionCollection OutboundConnections
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get
			{
				return base.GetOutboundConnectionsHelper();
			}
		}

		public AdamRoleCollection Roles
		{
			get
			{
				base.CheckIfDisposed();
				DirectoryEntry directoryEntry = null;
				DirectoryEntry directoryEntry1 = null;
				try
				{
					try
					{
						if (this.cachedRoles == null)
						{
							ArrayList arrayLists = new ArrayList();
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext));
							if (this.NtdsaObjectName.Equals((string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.FsmoRoleOwner)))
							{
								arrayLists.Add(AdamRole.SchemaRole);
							}
							directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
							if (this.NtdsaObjectName.Equals((string)PropertyManager.GetPropertyValue(this.context, directoryEntry1, PropertyManager.FsmoRoleOwner)))
							{
								arrayLists.Add(AdamRole.NamingRole);
							}
							this.cachedRoles = new AdamRoleCollection(arrayLists);
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
				}
				finally
				{
					if (directoryEntry != null)
					{
						directoryEntry.Dispose();
					}
					if (directoryEntry1 != null)
					{
						directoryEntry1.Dispose();
					}
				}
				return this.cachedRoles;
			}
		}

		internal string ServerObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedServerObjectName == null)
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					try
					{
						try
						{
							this.cachedServerObjectName = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ServerName);
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
					}
					finally
					{
						directoryEntry.Dispose();
					}
				}
				return this.cachedServerObjectName;
			}
		}

		public override string SiteName
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get
			{
				base.CheckIfDisposed();
				if (this.cachedSiteName == null)
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.SiteObjectName);
					try
					{
						this.cachedSiteName = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.Cn);
					}
					finally
					{
						directoryEntry.Dispose();
					}
				}
				return this.cachedSiteName;
			}
		}

		internal string SiteObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedSiteObjectName == null)
				{
					char[] chrArray = new char[1];
					chrArray[0] = ',';
					string[] strArrays = this.ServerObjectName.Split(chrArray);
					if (strArrays.GetLength(0) >= 3)
					{
						this.cachedSiteObjectName = strArrays[2];
						for (int i = 3; i < strArrays.GetLength(0); i++)
						{
							AdamInstance adamInstance = this;
							adamInstance.cachedSiteObjectName = string.Concat(adamInstance.cachedSiteObjectName, ",", strArrays[i]);
						}
					}
					else
					{
						throw new ActiveDirectoryOperationException(Res.GetString("InvalidServerNameFormat"));
					}
				}
				return this.cachedSiteObjectName;
			}
		}

		public int SslPort
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedSslPort == -1)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
					this.cachedSslPort = (int)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.MsDSPortSSL);
				}
				return this.cachedSslPort;
			}
		}

		public override SyncUpdateCallback SyncFromAllServersCallback
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get
			{
				if (!this.disposed)
				{
					return this.userDelegate;
				}
				else
				{
					throw new ObjectDisposedException(base.GetType().Name);
				}
			}
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			set
			{
				if (!this.disposed)
				{
					this.userDelegate = value;
					return;
				}
				else
				{
					throw new ObjectDisposedException(base.GetType().Name);
				}
			}
		}

		internal AdamInstance(DirectoryContext context, string adamInstanceName) : this(context, adamInstanceName, new DirectoryEntryManager(context), true)
		{
		}

		internal AdamInstance(DirectoryContext context, string adamInstanceName, DirectoryEntryManager directoryEntryMgr, bool nameIncludesPort)
		{
			this.cachedLdapPort = -1;
			this.cachedSslPort = -1;
			this.ADAMHandle = (IntPtr)0;
			this.authIdentity = IntPtr.Zero;
			this.context = context;
			this.replicaName = adamInstanceName;
			this.directoryEntryMgr = directoryEntryMgr;
			this.becomeRoleOwnerAttrs = new string[2];
			this.becomeRoleOwnerAttrs[0] = PropertyManager.BecomeSchemaMaster;
			this.becomeRoleOwnerAttrs[1] = PropertyManager.BecomeDomainMaster;
			this.syncAllFunctionPointer = new SyncReplicaFromAllServersCallback(this.SyncAllCallbackRoutine);
		}

		internal AdamInstance(DirectoryContext context, string adamHostName, DirectoryEntryManager directoryEntryMgr)
		{
			string str = null;
			this.cachedLdapPort = -1;
			this.cachedSslPort = -1;
			this.ADAMHandle = (IntPtr)0;
			this.authIdentity = IntPtr.Zero;
			this.context = context;
			this.replicaName = adamHostName;
			Utils.SplitServerNameAndPortNumber(context.Name, out str);
			if (str != null)
			{
				this.replicaName = string.Concat(this.replicaName, ":", str);
			}
			this.directoryEntryMgr = directoryEntryMgr;
			this.becomeRoleOwnerAttrs = new string[2];
			this.becomeRoleOwnerAttrs[0] = PropertyManager.BecomeSchemaMaster;
			this.becomeRoleOwnerAttrs[1] = PropertyManager.BecomeDomainMaster;
			this.syncAllFunctionPointer = new SyncReplicaFromAllServersCallback(this.SyncAllCallbackRoutine);
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override void CheckReplicationConsistency()
		{
			if (!this.disposed)
			{
				this.GetADAMHandle();
				base.CheckConsistencyHelper(this.ADAMHandle, DirectoryContext.ADAMHandle);
				return;
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{
					this.FreeADAMHandle();
					this.disposed = true;
				}
				finally
				{
					base.Dispose();
				}
			}
		}

		~AdamInstance()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//base.Finalize();
			}
		}

		public static AdamInstanceCollection FindAll(DirectoryContext context, string partitionName)
		{
			AdamInstanceCollection adamInstanceCollection;
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.ConfigurationSet)
				{
					if (partitionName != null)
					{
						if (partitionName.Length != 0)
						{
							context = new DirectoryContext(context);
							try
							{
								adamInstanceCollection = ConfigurationSet.FindAdamInstances(context, partitionName, null);
							}
							catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
							{
								adamInstanceCollection = new AdamInstanceCollection(new ArrayList());
							}
							return adamInstanceCollection;
						}
						else
						{
							throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
						}
					}
					else
					{
						throw new ArgumentNullException("partitionName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeConfigSet"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static AdamInstance FindOne(DirectoryContext context, string partitionName)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.ConfigurationSet)
				{
					if (partitionName != null)
					{
						if (partitionName.Length != 0)
						{
							context = new DirectoryContext(context);
							return ConfigurationSet.FindOneAdamInstance(context, partitionName, null);
						}
						else
						{
							throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
						}
					}
					else
					{
						throw new ArgumentNullException("partitionName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeConfigSet"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private void FreeADAMHandle()
		{
			lock (this)
			{
				Utils.FreeDSHandle(this.ADAMHandle, DirectoryContext.ADAMHandle);
				Utils.FreeAuthIdentity(this.authIdentity, DirectoryContext.ADAMHandle);
			}
		}

		private void GetADAMHandle()
		{
			lock (this)
			{
				if (this.ADAMHandle == IntPtr.Zero)
				{
					if (this.authIdentity == IntPtr.Zero)
					{
						this.authIdentity = Utils.GetAuthIdentity(this.context, DirectoryContext.ADAMHandle);
					}
					string str = string.Concat(this.HostName, ":", this.LdapPort);
					this.ADAMHandle = Utils.GetDSHandle(str, null, this.authIdentity, DirectoryContext.ADAMHandle);
				}
			}
		}

		public static AdamInstance GetAdamInstance(DirectoryContext context)
		{
			DirectoryEntryManager directoryEntryManager = null;
			string propertyValue = null;
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.DirectoryServer)
				{
					if (context.isServer())
					{
						context = new DirectoryContext(context);
						try
						{
							directoryEntryManager = new DirectoryEntryManager(context);
							DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
							if (Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryApplicationMode))
							{
								propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DnsHostName);
							}
							else
							{
								object[] name = new object[1];
								name[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", name), typeof(AdamInstance), context.Name);
							}
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							int errorCode = cOMException.ErrorCode;
							if (errorCode != -2147016646)
							{
								throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", objArray), typeof(AdamInstance), context.Name);
							}
						}
						return new AdamInstance(context, propertyValue, directoryEntryManager);
					}
					else
					{
						object[] name1 = new object[1];
						name1[0] = context.Name;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", name1), typeof(AdamInstance), context.Name);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeADAMServer"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override ReplicationNeighborCollection GetAllReplicationNeighbors()
		{
			bool flag = true;
			if (!this.disposed)
			{
				this.GetADAMHandle();
				IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.ADAMHandle, 0, 0, null, ref flag, 0, DirectoryContext.ADAMHandle);
				return base.ConstructNeighbors(replicationInfoHelper, this, DirectoryContext.ADAMHandle);
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override ReplicationFailureCollection GetReplicationConnectionFailures()
		{
			return this.GetReplicationFailures(DS_REPL_INFO_TYPE.DS_REPL_INFO_KCC_DSA_CONNECT_FAILURES);
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override ReplicationCursorCollection GetReplicationCursors(string partition)
		{
			int num = 0;
			bool flag = true;
			if (!this.disposed)
			{
				if (partition != null)
				{
					if (partition.Length != 0)
					{
						this.GetADAMHandle();
						IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.ADAMHandle, 8, 1, partition, ref flag, num, DirectoryContext.ADAMHandle);
						return base.ConstructReplicationCursors(this.ADAMHandle, flag, replicationInfoHelper, partition, this, DirectoryContext.ADAMHandle);
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
					}
				}
				else
				{
					throw new ArgumentNullException("partition");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		private ReplicationFailureCollection GetReplicationFailures(DS_REPL_INFO_TYPE type)
		{
			bool flag = true;
			if (!this.disposed)
			{
				this.GetADAMHandle();
				IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.ADAMHandle, (int)type, (int)type, null, ref flag, 0, DirectoryContext.ADAMHandle);
				return base.ConstructFailures(replicationInfoHelper, this, DirectoryContext.ADAMHandle);
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override ActiveDirectoryReplicationMetadata GetReplicationMetadata(string objectPath)
		{
			bool flag = true;
			if (!this.disposed)
			{
				if (objectPath != null)
				{
					if (objectPath.Length != 0)
					{
						this.GetADAMHandle();
						IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.ADAMHandle, 9, 2, objectPath, ref flag, 0, DirectoryContext.ADAMHandle);
						return base.ConstructMetaData(flag, replicationInfoHelper, this, DirectoryContext.ADAMHandle);
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "objectPath");
					}
				}
				else
				{
					throw new ArgumentNullException("objectPath");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override ReplicationNeighborCollection GetReplicationNeighbors(string partition)
		{
			bool flag = true;
			if (!this.disposed)
			{
				if (partition != null)
				{
					if (partition.Length != 0)
					{
						this.GetADAMHandle();
						IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.ADAMHandle, 0, 0, partition, ref flag, 0, DirectoryContext.ADAMHandle);
						return base.ConstructNeighbors(replicationInfoHelper, this, DirectoryContext.ADAMHandle);
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
					}
				}
				else
				{
					throw new ArgumentNullException("partition");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override ReplicationOperationInformation GetReplicationOperationInformation()
		{
			bool flag = true;
			if (!this.disposed)
			{
				this.GetADAMHandle();
				IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.ADAMHandle, 5, 5, null, ref flag, 0, DirectoryContext.ADAMHandle);
				return base.ConstructPendingOperations(replicationInfoHelper, this, DirectoryContext.ADAMHandle);
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		public void Save()
		{
			base.CheckIfDisposed();
			if (this.defaultPartitionModified)
			{
				DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
				try
				{
					cachedDirectoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			this.defaultPartitionInitialized = false;
			this.defaultPartitionModified = false;
		}

		public void SeizeRoleOwnership(AdamRole role)
		{
			string str = null;
			base.CheckIfDisposed();
			AdamRole adamRole = role;
			if (adamRole == AdamRole.SchemaRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
			}
			else if (adamRole == AdamRole.NamingRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer);
			}
			else
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(AdamRole));
			}
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
					directoryEntry.Properties[PropertyManager.FsmoRoleOwner].Value = this.NtdsaObjectName;
					directoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			this.cachedRoles = null;
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override void SyncReplicaFromAllServers(string partition, SyncFromAllServersOptions options)
		{
			if (!this.disposed)
			{
				if (partition != null)
				{
					if (partition.Length != 0)
					{
						this.GetADAMHandle();
						base.SyncReplicaAllHelper(this.ADAMHandle, this.syncAllFunctionPointer, partition, options, this.SyncFromAllServersCallback, DirectoryContext.ADAMHandle);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
					}
				}
				else
				{
					throw new ArgumentNullException("partition");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override void SyncReplicaFromServer(string partition, string sourceServer)
		{
			if (!this.disposed)
			{
				if (partition != null)
				{
					if (partition.Length != 0)
					{
						if (sourceServer != null)
						{
							if (sourceServer.Length != 0)
							{
								this.GetADAMHandle();
								base.SyncReplicaHelper(this.ADAMHandle, true, partition, sourceServer, 0, DirectoryContext.ADAMHandle);
								return;
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "sourceServer");
							}
						}
						else
						{
							throw new ArgumentNullException("sourceServer");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
					}
				}
				else
				{
					throw new ArgumentNullException("partition");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		public void TransferRoleOwnership(AdamRole role)
		{
			base.CheckIfDisposed();
			if (role < AdamRole.SchemaRole || role > AdamRole.NamingRole)
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(AdamRole));
			}
			else
			{
				try
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
					cachedDirectoryEntry.Properties[this.becomeRoleOwnerAttrs[(int)role]].Value = 1;
					cachedDirectoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				this.cachedRoles = null;
				return;
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override void TriggerSyncReplicaFromNeighbors(string partition)
		{
			if (!this.disposed)
			{
				if (partition != null)
				{
					if (partition.Length != 0)
					{
						this.GetADAMHandle();
						base.SyncReplicaHelper(this.ADAMHandle, true, partition, null, 17, DirectoryContext.ADAMHandle);
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
					}
				}
				else
				{
					throw new ArgumentNullException("partition");
				}
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}
	}
}