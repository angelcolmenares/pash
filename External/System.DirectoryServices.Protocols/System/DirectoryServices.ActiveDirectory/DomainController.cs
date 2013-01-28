using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class DomainController : DirectoryServer
	{
		private IntPtr dsHandle;

		private IntPtr authIdentity;

		private string[] becomeRoleOwnerAttrs;

		private bool disposed;

		private string cachedComputerObjectName;

		private string cachedOSVersion;

		private double cachedNumericOSVersion;

		private Forest currentForest;

		private Domain cachedDomain;

		private ActiveDirectoryRoleCollection cachedRoles;

		private bool dcInfoInitialized;

		internal SyncUpdateCallback userDelegate;

		internal SyncReplicaFromAllServersCallback syncAllFunctionPointer;

		internal string ComputerObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.dcInfoInitialized)
				{
					this.GetDomainControllerInfo();
				}
				if (this.cachedComputerObjectName != null)
				{
					return this.cachedComputerObjectName;
				}
				else
				{
					object[] name = new object[1];
					name[0] = base.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("ComputerObjectNameNotFound", name));
				}
			}
		}

		public DateTime CurrentTime
		{
			get
			{
				base.CheckIfDisposed();
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
				string propertyValue = null;
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.CurrentTime);
				}
				finally
				{
					directoryEntry.Dispose();
				}
				return this.ParseDateTime(propertyValue);
			}
		}

		public Domain Domain
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedDomain == null)
				{
					string dnsNameFromDN = null;
					try
					{
						string str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext);
						dnsNameFromDN = Utils.GetDnsNameFromDN(str);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, this.context);
					this.cachedDomain = new Domain(newDirectoryContext, dnsNameFromDN);
				}
				return this.cachedDomain;
			}
		}

		public Forest Forest
		{
			get
			{
				base.CheckIfDisposed();
				if (this.currentForest == null)
				{
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, this.context);
					this.currentForest = Forest.GetForest(newDirectoryContext);
				}
				return this.currentForest;
			}
		}

		internal IntPtr Handle
		{
			get
			{
				this.GetDSHandle();
				return this.dsHandle;
			}
		}

		public long HighestCommittedUsn
		{
			get
			{
				base.CheckIfDisposed();
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
				string propertyValue = null;
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.HighestCommittedUSN);
				}
				finally
				{
					directoryEntry.Dispose();
				}
				return long.Parse(propertyValue, NumberFormatInfo.InvariantInfo);
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
				IPHostEntry hostEntry = Dns.GetHostEntry(base.Name);
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

		internal Guid NtdsaObjectGuid
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.dcInfoInitialized || this.siteInfoModified)
				{
					this.GetDomainControllerInfo();
				}
				if (!this.cachedNtdsaObjectGuid.Equals(Guid.Empty))
				{
					return this.cachedNtdsaObjectGuid;
				}
				else
				{
					object[] name = new object[1];
					name[0] = base.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("NtdsaObjectGuidNotFound", name));
				}
			}
		}

		internal string NtdsaObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.dcInfoInitialized || this.siteInfoModified)
				{
					this.GetDomainControllerInfo();
				}
				if (this.cachedNtdsaObjectName != null)
				{
					return this.cachedNtdsaObjectName;
				}
				else
				{
					object[] name = new object[1];
					name[0] = base.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("NtdsaObjectNameNotFound", name));
				}
			}
		}

		internal double NumericOSVersion
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedNumericOSVersion == 0)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.ComputerObjectName);
					string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.OperatingSystemVersion);
					int num = propertyValue.IndexOf('(');
					if (num != -1)
					{
						propertyValue = propertyValue.Substring(0, num);
					}
					this.cachedNumericOSVersion = (double)double.Parse(propertyValue, NumberFormatInfo.InvariantInfo);
				}
				return this.cachedNumericOSVersion;
			}
		}

		public string OSVersion
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedOSVersion == null)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.ComputerObjectName);
					this.cachedOSVersion = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.OperatingSystem);
				}
				return this.cachedOSVersion;
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

		public ActiveDirectoryRoleCollection Roles
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedRoles == null)
				{
					this.cachedRoles = new ActiveDirectoryRoleCollection(this.GetRoles());
				}
				return this.cachedRoles;
			}
		}

		internal string ServerObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.dcInfoInitialized || this.siteInfoModified)
				{
					this.GetDomainControllerInfo();
				}
				if (this.cachedServerObjectName != null)
				{
					return this.cachedServerObjectName;
				}
				else
				{
					object[] name = new object[1];
					name[0] = base.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("ServerObjectNameNotFound", name));
				}
			}
		}

		public override string SiteName
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get
			{
				base.CheckIfDisposed();
				if (!this.dcInfoInitialized || this.siteInfoModified)
				{
					this.GetDomainControllerInfo();
				}
				if (this.cachedSiteName != null)
				{
					return this.cachedSiteName;
				}
				else
				{
					object[] name = new object[1];
					name[0] = base.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("SiteNameNotFound", name));
				}
			}
		}

		internal string SiteObjectName
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.dcInfoInitialized || this.siteInfoModified)
				{
					this.GetDomainControllerInfo();
				}
				if (this.cachedSiteObjectName != null)
				{
					return this.cachedSiteObjectName;
				}
				else
				{
					object[] name = new object[1];
					name[0] = base.Name;
					throw new ActiveDirectoryOperationException(Res.GetString("SiteObjectNameNotFound", name));
				}
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

		protected DomainController()
		{
			this.dsHandle = IntPtr.Zero;
			this.authIdentity = IntPtr.Zero;
		}

		internal DomainController(DirectoryContext context, string domainControllerName) : this(context, domainControllerName, new DirectoryEntryManager(context))
		{
		}

		internal DomainController(DirectoryContext context, string domainControllerName, DirectoryEntryManager directoryEntryMgr)
		{
			this.dsHandle = IntPtr.Zero;
			this.authIdentity = IntPtr.Zero;
			this.context = context;
			this.replicaName = domainControllerName;
			this.directoryEntryMgr = directoryEntryMgr;
			this.becomeRoleOwnerAttrs = new string[5];
			this.becomeRoleOwnerAttrs[0] = PropertyManager.BecomeSchemaMaster;
			this.becomeRoleOwnerAttrs[1] = PropertyManager.BecomeDomainMaster;
			this.becomeRoleOwnerAttrs[2] = PropertyManager.BecomePdc;
			this.becomeRoleOwnerAttrs[3] = PropertyManager.BecomeRidMaster;
			this.becomeRoleOwnerAttrs[4] = PropertyManager.BecomeInfrastructureMaster;
			this.syncAllFunctionPointer = new SyncReplicaFromAllServersCallback(this.SyncAllCallbackRoutine);
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override void CheckReplicationConsistency()
		{
			if (!this.disposed)
			{
				this.GetDSHandle();
				base.CheckConsistencyHelper(this.dsHandle, DirectoryContext.ADHandle);
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
					this.FreeDSHandle();
					this.disposed = true;
				}
				finally
				{
					base.Dispose();
				}
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public virtual GlobalCatalog EnableGlobalCatalog()
		{
			base.CheckIfDisposed();
			try
			{
				DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
				int value = 0;
				if (cachedDirectoryEntry.Properties[PropertyManager.Options].Value != null)
				{
					value = (int)cachedDirectoryEntry.Properties[PropertyManager.Options].Value;
				}
				cachedDirectoryEntry.Properties[PropertyManager.Options].Value = value | 1;
				cachedDirectoryEntry.CommitChanges();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			return new GlobalCatalog(this.context, base.Name);
		}

		~DomainController()
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

		public static DomainControllerCollection FindAll(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain)
				{
					context = new DirectoryContext(context);
					return DomainController.FindAllInternal(context, context.Name, false, null);
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static DomainControllerCollection FindAll(DirectoryContext context, string siteName)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain)
				{
					if (siteName != null)
					{
						context = new DirectoryContext(context);
						return DomainController.FindAllInternal(context, context.Name, false, siteName);
					}
					else
					{
						throw new ArgumentNullException("siteName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		internal static DomainControllerCollection FindAllInternal(DirectoryContext context, string domainName, bool isDnsDomainName, string siteName)
		{
			DomainControllerInfo domainControllerInfo = null;
			string loggedOnDomain;
			ArrayList arrayLists = new ArrayList();
			if (siteName == null || siteName.Length != 0)
			{
				if (domainName == null || !isDnsDomainName)
				{
					object obj = null;
					if (domainName != null)
					{
						loggedOnDomain = domainName;
					}
					else
					{
						loggedOnDomain = DirectoryContext.GetLoggedOnDomain();
					}
					int num = Locator.DsGetDcNameWrapper((string)obj, loggedOnDomain, null, (long)16, out domainControllerInfo);
					if (num != 0x54b)
					{
						if (num == 0)
						{
							domainName = domainControllerInfo.DomainName;
						}
						else
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(num);
						}
					}
					else
					{
						return new DomainControllerCollection(arrayLists);
					}
				}
				foreach (string replicaList in Utils.GetReplicaList(context, Utils.GetDNFromDnsName(domainName), siteName, true, false, false))
				{
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(replicaList, DirectoryContextType.DirectoryServer, context);
					arrayLists.Add(new DomainController(newDirectoryContext, replicaList));
				}
				return new DomainControllerCollection(arrayLists);
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		public static DomainController FindOne(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain)
				{
					return DomainController.FindOneWithCredentialValidation(context, null, (LocatorOptions)((long)0));
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static DomainController FindOne(DirectoryContext context, string siteName)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain)
				{
					if (siteName != null)
					{
						return DomainController.FindOneWithCredentialValidation(context, siteName, (LocatorOptions)((long)0));
					}
					else
					{
						throw new ArgumentNullException("siteName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static DomainController FindOne(DirectoryContext context, LocatorOptions flag)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain)
				{
					return DomainController.FindOneWithCredentialValidation(context, null, flag);
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public static DomainController FindOne(DirectoryContext context, string siteName, LocatorOptions flag)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain)
				{
					if (siteName != null)
					{
						return DomainController.FindOneWithCredentialValidation(context, siteName, flag);
					}
					else
					{
						throw new ArgumentNullException("siteName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		internal static DomainController FindOneInternal(DirectoryContext context, string domainName, string siteName, LocatorOptions flag)
		{
			DomainControllerInfo domainControllerInfo = null;
			if (siteName == null || siteName.Length != 0)
			{
				if (((int)flag & -23554) == 0)
				{
					if (domainName == null)
					{
						domainName = DirectoryContext.GetLoggedOnDomain();
					}
					int num = Locator.DsGetDcNameWrapper(null, domainName, siteName, (int)flag | 16, out domainControllerInfo);
					if (num != 0x54b)
					{
						if (num != 0x3ec)
						{
							if (num == 0)
							{
								string str = domainControllerInfo.DomainControllerName.Substring(2);
								DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
								return new DomainController(newDirectoryContext, str);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num);
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
						}
					}
					else
					{
						object[] objArray = new object[1];
						objArray[0] = domainName;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFoundInDomain", objArray), typeof(DomainController), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		internal static DomainController FindOneWithCredentialValidation(DirectoryContext context, string siteName, LocatorOptions flag)
		{
			bool flag1 = false;
			bool flag2 = false;
			context = new DirectoryContext(context);
			DomainController domainController = DomainController.FindOneInternal(context, context.Name, siteName, flag);
			using (domainController)
			{
				if (flag2)
				{
					try
					{
						DomainController.ValidateCredential(domainController, context);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						if (cOMException.ErrorCode != -2147016646)
						{
							throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
						}
						else
						{
							if ((flag & LocatorOptions.ForceRediscovery) != 0)
							{
								object[] name = new object[1];
								name[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFoundInDomain", name), typeof(DomainController), null);
							}
							else
							{
								flag1 = true;
							}
						}
					}
				}
			}
			if (flag1)
			{
				flag2 = false;
				domainController = DomainController.FindOneInternal(context, context.Name, siteName, flag | LocatorOptions.ForceRediscovery);
				using (domainController)
				{
					if (flag2)
					{
						try
						{
							DomainController.ValidateCredential(domainController, context);
						}
						catch (COMException cOMException3)
						{
							COMException cOMException2 = cOMException3;
							if (cOMException2.ErrorCode != -2147016646)
							{
								throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException2);
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFoundInDomain", objArray), typeof(DomainController), null);
							}
						}
					}
				}
			}
			return domainController;
		}

		internal void FreeDSHandle()
		{
			lock (this)
			{
				Utils.FreeDSHandle(this.dsHandle, DirectoryContext.ADHandle);
				Utils.FreeAuthIdentity(this.authIdentity, DirectoryContext.ADHandle);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override ReplicationNeighborCollection GetAllReplicationNeighbors()
		{
			bool flag = true;
			if (!this.disposed)
			{
				this.GetDSHandle();
				IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.dsHandle, 0, 0, null, ref flag, 0, DirectoryContext.ADHandle);
				return base.ConstructNeighbors(replicationInfoHelper, this, DirectoryContext.ADHandle);
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public virtual DirectorySearcher GetDirectorySearcher()
		{
			base.CheckIfDisposed();
			return this.InternalGetDirectorySearcher();
		}

		public static DomainController GetDomainController(DirectoryContext context)
		{
			string propertyValue = null;
			DirectoryEntryManager directoryEntryManager = null;
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
							if (Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectory))
							{
								propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DnsHostName);
							}
							else
							{
								object[] name = new object[1];
								name[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", name), typeof(DomainController), context.Name);
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
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", objArray), typeof(DomainController), context.Name);
							}
						}
						return new DomainController(context, propertyValue, directoryEntryManager);
					}
					else
					{
						object[] name1 = new object[1];
						name1[0] = context.Name;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", name1), typeof(DomainController), context.Name);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeDC"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private void GetDomainControllerInfo()
		{
			int num = 0;
			IntPtr zero = IntPtr.Zero;
			bool flag = false;
			this.GetDSHandle();
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsGetDomainControllerInfoW");
			if (procAddress != (IntPtr)0)
			{
				NativeMethods.DsGetDomainControllerInfo delegateForFunctionPointer = (NativeMethods.DsGetDomainControllerInfo)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsGetDomainControllerInfo));
				int num1 = 3;
				int name = delegateForFunctionPointer(this.dsHandle, this.Domain.Name, num1, out num, out zero);
				if (name != 0)
				{
					num1 = 2;
					name = delegateForFunctionPointer(this.dsHandle, this.Domain.Name, num1, out num, out zero);
				}
				if (name != 0)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(name, base.Name);
				}
				else
				{
					try
					{
						IntPtr intPtr = zero;
						for (int i = 0; i < num; i++)
						{
							if (num1 != 3)
							{
								DsDomainControllerInfo2 dsDomainControllerInfo2 = new DsDomainControllerInfo2();
								Marshal.PtrToStructure(intPtr, dsDomainControllerInfo2);
								if (dsDomainControllerInfo2 != null && Utils.Compare(dsDomainControllerInfo2.dnsHostName, this.replicaName) == 0)
								{
									flag = true;
									this.cachedSiteName = dsDomainControllerInfo2.siteName;
									this.cachedSiteObjectName = dsDomainControllerInfo2.siteObjectName;
									this.cachedComputerObjectName = dsDomainControllerInfo2.computerObjectName;
									this.cachedServerObjectName = dsDomainControllerInfo2.serverObjectName;
									this.cachedNtdsaObjectName = dsDomainControllerInfo2.ntdsaObjectName;
									this.cachedNtdsaObjectGuid = dsDomainControllerInfo2.ntdsDsaObjectGuid;
								}
								intPtr = (IntPtr)((long)intPtr + (long)Marshal.SizeOf(dsDomainControllerInfo2));
							}
							else
							{
								DsDomainControllerInfo3 dsDomainControllerInfo3 = new DsDomainControllerInfo3();
								Marshal.PtrToStructure(intPtr, dsDomainControllerInfo3);
								if (dsDomainControllerInfo3 != null && Utils.Compare(dsDomainControllerInfo3.dnsHostName, this.replicaName) == 0)
								{
									flag = true;
									this.cachedSiteName = dsDomainControllerInfo3.siteName;
									this.cachedSiteObjectName = dsDomainControllerInfo3.siteObjectName;
									this.cachedComputerObjectName = dsDomainControllerInfo3.computerObjectName;
									this.cachedServerObjectName = dsDomainControllerInfo3.serverObjectName;
									this.cachedNtdsaObjectName = dsDomainControllerInfo3.ntdsaObjectName;
									this.cachedNtdsaObjectGuid = dsDomainControllerInfo3.ntdsDsaObjectGuid;
								}
								intPtr = (IntPtr)((long)intPtr + (long)Marshal.SizeOf(dsDomainControllerInfo3));
							}
						}
					}
					finally
					{
						if (zero != IntPtr.Zero)
						{
							procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeDomainControllerInfoW");
							if (procAddress != (IntPtr)0)
							{
								NativeMethods.DsFreeDomainControllerInfo dsFreeDomainControllerInfo = (NativeMethods.DsFreeDomainControllerInfo)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsFreeDomainControllerInfo));
								dsFreeDomainControllerInfo(num1, num, zero);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
							}
						}
					}
					if (flag)
					{
						this.dcInfoInitialized = true;
						this.siteInfoModified = false;
						return;
					}
					else
					{
						throw new ActiveDirectoryOperationException(Res.GetString("DCInfoNotFound"));
					}
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		internal void GetDSHandle()
		{
			if (!this.disposed)
			{
				lock (this)
				{
					if (this.dsHandle == IntPtr.Zero)
					{
						if (this.authIdentity == IntPtr.Zero)
						{
							this.authIdentity = Utils.GetAuthIdentity(this.context, DirectoryContext.ADHandle);
						}
						this.dsHandle = Utils.GetDSHandle(this.replicaName, null, this.authIdentity, DirectoryContext.ADHandle);
					}
				}
				return;
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
						this.GetDSHandle();
						IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.dsHandle, 8, 1, partition, ref flag, num, DirectoryContext.ADHandle);
						return base.ConstructReplicationCursors(this.dsHandle, flag, replicationInfoHelper, partition, this, DirectoryContext.ADHandle);
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

		internal ReplicationFailureCollection GetReplicationFailures(DS_REPL_INFO_TYPE type)
		{
			bool flag = true;
			if (!this.disposed)
			{
				this.GetDSHandle();
				IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.dsHandle, (int)type, (int)type, null, ref flag, 0, DirectoryContext.ADHandle);
				return base.ConstructFailures(replicationInfoHelper, this, DirectoryContext.ADHandle);
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
						this.GetDSHandle();
						IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.dsHandle, 9, 2, objectPath, ref flag, 0, DirectoryContext.ADHandle);
						return base.ConstructMetaData(flag, replicationInfoHelper, this, DirectoryContext.ADHandle);
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
						this.GetDSHandle();
						IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.dsHandle, 0, 0, partition, ref flag, 0, DirectoryContext.ADHandle);
						return base.ConstructNeighbors(replicationInfoHelper, this, DirectoryContext.ADHandle);
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
				this.GetDSHandle();
				IntPtr replicationInfoHelper = base.GetReplicationInfoHelper(this.dsHandle, 5, 5, null, ref flag, 0, DirectoryContext.ADHandle);
				return base.ConstructPendingOperations(replicationInfoHelper, this, DirectoryContext.ADHandle);
			}
			else
			{
				throw new ObjectDisposedException(base.GetType().Name);
			}
		}

		private ArrayList GetRoles()
		{
			ArrayList arrayLists = new ArrayList();
			IntPtr zero = IntPtr.Zero;
			this.GetDSHandle();
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsListRolesW");
			if (procAddress != (IntPtr)0)
			{
				NativeMethods.DsListRoles delegateForFunctionPointer = (NativeMethods.DsListRoles)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsListRoles));
				int num = delegateForFunctionPointer(this.dsHandle, out zero);
				if (num != 0)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num, base.Name);
				}
				else
				{
					try
					{
						DsNameResult dsNameResult = new DsNameResult();
						Marshal.PtrToStructure(zero, dsNameResult);
						IntPtr intPtr = dsNameResult.items;
						for (int i = 0; i < dsNameResult.itemCount; i++)
						{
							DsNameResultItem dsNameResultItem = new DsNameResultItem();
							Marshal.PtrToStructure(intPtr, dsNameResultItem);
							if (dsNameResultItem.status == 0 && dsNameResultItem.name.Equals(this.NtdsaObjectName))
							{
								arrayLists.Add((ActiveDirectoryRole)i);
							}
							intPtr = (IntPtr)((long)intPtr + (long)Marshal.SizeOf(dsNameResultItem));
						}
					}
					finally
					{
						if (zero != IntPtr.Zero)
						{
							procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
							if (procAddress != (IntPtr)0)
							{
								UnsafeNativeMethods.DsFreeNameResultW dsFreeNameResultW = (UnsafeNativeMethods.DsFreeNameResultW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsFreeNameResultW));
								dsFreeNameResultW(zero);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
							}
						}
					}
					return arrayLists;
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		private DirectorySearcher InternalGetDirectorySearcher()
		{
			DirectoryEntry directoryEntry = new DirectoryEntry(string.Concat("LDAP://", base.Name));
			if (!DirectoryContext.ServerBindSupported)
			{
				directoryEntry.AuthenticationType = Utils.DefaultAuthType;
			}
			else
			{
				directoryEntry.AuthenticationType = Utils.DefaultAuthType | AuthenticationTypes.ServerBind;
			}
			directoryEntry.Username = this.context.UserName;
			directoryEntry.Password = this.context.Password;
			return new DirectorySearcher(directoryEntry);
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public virtual bool IsGlobalCatalog()
		{
			bool flag;
			base.CheckIfDisposed();
			try
			{
				DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
				cachedDirectoryEntry.RefreshCache();
				int value = 0;
				if (cachedDirectoryEntry.Properties[PropertyManager.Options].Value != null)
				{
					value = (int)cachedDirectoryEntry.Properties[PropertyManager.Options].Value;
				}
				if ((value & 1) != 1)
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			return flag;
		}

		private DateTime ParseDateTime(string dateTime)
		{
			int num = int.Parse(dateTime.Substring(0, 4), NumberFormatInfo.InvariantInfo);
			int num1 = int.Parse(dateTime.Substring(4, 2), NumberFormatInfo.InvariantInfo);
			int num2 = int.Parse(dateTime.Substring(6, 2), NumberFormatInfo.InvariantInfo);
			int num3 = int.Parse(dateTime.Substring(8, 2), NumberFormatInfo.InvariantInfo);
			int num4 = int.Parse(dateTime.Substring(10, 2), NumberFormatInfo.InvariantInfo);
			int num5 = int.Parse(dateTime.Substring(12, 2), NumberFormatInfo.InvariantInfo);
			return new DateTime(num, num1, num2, num3, num4, num5, 0);
		}

		public void SeizeRoleOwnership(ActiveDirectoryRole role)
		{
			string str = null;
			base.CheckIfDisposed();
			ActiveDirectoryRole activeDirectoryRole = role;
			if (activeDirectoryRole == ActiveDirectoryRole.SchemaRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
			}
			else if (activeDirectoryRole == ActiveDirectoryRole.NamingRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer);
			}
			else if (activeDirectoryRole == ActiveDirectoryRole.PdcRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext);
			}
			else if (activeDirectoryRole == ActiveDirectoryRole.RidRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RidManager);
			}
			else if (activeDirectoryRole == ActiveDirectoryRole.InfrastructureRole)
			{
				str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.Infrastructure);
			}
			else
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(ActiveDirectoryRole));
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
						this.GetDSHandle();
						base.SyncReplicaAllHelper(this.dsHandle, this.syncAllFunctionPointer, partition, options, this.SyncFromAllServersCallback, DirectoryContext.ADHandle);
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
								this.GetDSHandle();
								base.SyncReplicaHelper(this.dsHandle, false, partition, sourceServer, 0, DirectoryContext.ADHandle);
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

		public void TransferRoleOwnership(ActiveDirectoryRole role)
		{
			base.CheckIfDisposed();
			if (role < ActiveDirectoryRole.SchemaRole || role > ActiveDirectoryRole.InfrastructureRole)
			{
				throw new InvalidEnumArgumentException("role", (int)role, typeof(ActiveDirectoryRole));
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
						this.GetDSHandle();
						base.SyncReplicaHelper(this.dsHandle, false, partition, null, 17, DirectoryContext.ADHandle);
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

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		internal static void ValidateCredential(DomainController dc, DirectoryContext context)
		{
			DirectoryEntry directoryEntry;
			if (!DirectoryContext.ServerBindSupported)
			{
				directoryEntry = new DirectoryEntry(string.Concat("LDAP://", dc.Name, "/RootDSE"), context.UserName, context.Password, Utils.DefaultAuthType);
			}
			else
			{
				directoryEntry = new DirectoryEntry(string.Concat("LDAP://", dc.Name, "/RootDSE"), context.UserName, context.Password, Utils.DefaultAuthType | AuthenticationTypes.ServerBind);
			}
			//TODO: REVIEW: URGENT!!: directoryEntry.Bind(true);
		}
	}
}