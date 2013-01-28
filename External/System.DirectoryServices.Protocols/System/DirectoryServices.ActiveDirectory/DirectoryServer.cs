using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public abstract class DirectoryServer : IDisposable
	{
		private bool disposed;

		internal DirectoryContext context;

		internal string replicaName;

		internal DirectoryEntryManager directoryEntryMgr;

		internal bool siteInfoModified;

		internal string cachedSiteName;

		internal string cachedSiteObjectName;

		internal string cachedServerObjectName;

		internal string cachedNtdsaObjectName;

		internal Guid cachedNtdsaObjectGuid;

		internal string cachedIPAddress;

		internal ReadOnlyStringCollection cachedPartitions;

		private ReplicationConnectionCollection inbound;

		private ReplicationConnectionCollection outbound;

		internal const int DS_REPSYNC_ASYNCHRONOUS_OPERATION = 1;

		internal const int DS_REPSYNC_ALL_SOURCES = 16;

		internal const int DS_REPSYNCALL_ID_SERVERS_BY_DN = 4;

		internal const int DS_REPL_NOTSUPPORTED = 50;

		private const int DS_REPL_INFO_FLAG_IMPROVE_LINKED_ATTRS = 1;

		internal DirectoryContext Context
		{
			get
			{
				return this.context;
			}
		}

		public abstract ReplicationConnectionCollection InboundConnections
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get;
		}

		public abstract string IPAddress
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get;
		}

		public string Name
		{
			get
			{
				this.CheckIfDisposed();
				return this.replicaName;
			}
		}

		public abstract ReplicationConnectionCollection OutboundConnections
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get;
		}

		public ReadOnlyStringCollection Partitions
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedPartitions == null)
				{
					this.cachedPartitions = new ReadOnlyStringCollection(this.GetPartitions());
				}
				return this.cachedPartitions;
			}
		}

		public abstract string SiteName
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get;
		}

		public abstract SyncUpdateCallback SyncFromAllServersCallback
		{
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			get;
			[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
			[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
			set;
		}

		protected DirectoryServer()
		{
			this.cachedNtdsaObjectGuid = Guid.Empty;
		}

		internal void CheckConsistencyHelper(IntPtr dsHandle, LoadLibrarySafeHandle libHandle)
		{
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaConsistencyCheck");
			if (procAddress != (IntPtr)0)
			{
				UnsafeNativeMethods.DsReplicaConsistencyCheck delegateForFunctionPointer = (UnsafeNativeMethods.DsReplicaConsistencyCheck)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaConsistencyCheck));
				int num = delegateForFunctionPointer(dsHandle, 0, 0);
				if (num == 0)
				{
					return;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num, this.Name);
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		internal void CheckIfDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract void CheckReplicationConsistency();

		internal ReplicationFailureCollection ConstructFailures(IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
		{
			ReplicationFailureCollection replicationFailureCollection = new ReplicationFailureCollection(server);
			try
			{
				if (info != (IntPtr)0)
				{
					DS_REPL_KCC_DSA_FAILURES dSREPLKCCDSAFAILURE = new DS_REPL_KCC_DSA_FAILURES();
					Marshal.PtrToStructure(info, dSREPLKCCDSAFAILURE);
					int num = dSREPLKCCDSAFAILURE.cNumEntries;
					if (num > 0)
					{
						replicationFailureCollection.AddHelper(dSREPLKCCDSAFAILURE, info);
					}
				}
			}
			finally
			{
				this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_KCC_DSA_CONNECT_FAILURES, info, libHandle);
			}
			return replicationFailureCollection;
		}

		internal ActiveDirectoryReplicationMetadata ConstructMetaData(bool advanced, IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
		{
			int num;
			ActiveDirectoryReplicationMetadata activeDirectoryReplicationMetadatum = new ActiveDirectoryReplicationMetadata(server);
			if (!advanced)
			{
				try
				{
					DS_REPL_OBJ_META_DATA dSREPLOBJMETADATum = new DS_REPL_OBJ_META_DATA();
					Marshal.PtrToStructure(info, dSREPLOBJMETADATum);
					num = dSREPLOBJMETADATum.cNumEntries;
					if (num > 0)
					{
						activeDirectoryReplicationMetadatum.AddHelper(num, info, false);
					}
				}
				finally
				{
					this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_METADATA_FOR_OBJ, info, libHandle);
				}
			}
			else
			{
				try
				{
					if (info != (IntPtr)0)
					{
						DS_REPL_OBJ_META_DATA_2 dSREPLOBJMETADATA2 = new DS_REPL_OBJ_META_DATA_2();
						Marshal.PtrToStructure(info, dSREPLOBJMETADATA2);
						num = dSREPLOBJMETADATA2.cNumEntries;
						if (num > 0)
						{
							activeDirectoryReplicationMetadatum.AddHelper(num, info, true);
						}
					}
				}
				finally
				{
					this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_METADATA_2_FOR_OBJ, info, libHandle);
				}
			}
			return activeDirectoryReplicationMetadatum;
		}

		internal ReplicationNeighborCollection ConstructNeighbors(IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
		{
			ReplicationNeighborCollection replicationNeighborCollection = new ReplicationNeighborCollection(server);
			try
			{
				if (info != (IntPtr)0)
				{
					DS_REPL_NEIGHBORS dSREPLNEIGHBOR = new DS_REPL_NEIGHBORS();
					Marshal.PtrToStructure(info, dSREPLNEIGHBOR);
					int num = dSREPLNEIGHBOR.cNumNeighbors;
					if (num > 0)
					{
						replicationNeighborCollection.AddHelper(dSREPLNEIGHBOR, info);
					}
				}
			}
			finally
			{
				this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_NEIGHBORS, info, libHandle);
			}
			return replicationNeighborCollection;
		}

		internal ReplicationOperationInformation ConstructPendingOperations(IntPtr info, DirectoryServer server, LoadLibrarySafeHandle libHandle)
		{
			ReplicationOperationInformation replicationOperationInformation = new ReplicationOperationInformation();
			ReplicationOperationCollection replicationOperationCollection = new ReplicationOperationCollection(server);
			replicationOperationInformation.collection = replicationOperationCollection;
			try
			{
				if (info != (IntPtr)0)
				{
					DS_REPL_PENDING_OPS dSREPLPENDINGOP = new DS_REPL_PENDING_OPS();
					Marshal.PtrToStructure(info, dSREPLPENDINGOP);
					int num = dSREPLPENDINGOP.cNumPendingOps;
					if (num > 0)
					{
						replicationOperationCollection.AddHelper(dSREPLPENDINGOP, info);
						replicationOperationInformation.startTime = DateTime.FromFileTime(dSREPLPENDINGOP.ftimeCurrentOpStarted);
						replicationOperationInformation.currentOp = replicationOperationCollection.GetFirstOperation();
					}
				}
			}
			finally
			{
				this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_PENDING_OPS, info, libHandle);
			}
			return replicationOperationInformation;
		}

		internal ReplicationCursorCollection ConstructReplicationCursors(IntPtr dsHandle, bool advanced, IntPtr info, string partition, DirectoryServer server, LoadLibrarySafeHandle libHandle)
		{
			int num = 0;
			ReplicationCursorCollection replicationCursorCollection = new ReplicationCursorCollection(server);
			if (!advanced)
			{
				try
				{
					if (info != (IntPtr)0)
					{
						DS_REPL_CURSORS dSREPLCURSOR = new DS_REPL_CURSORS();
						Marshal.PtrToStructure(info, dSREPLCURSOR);
						replicationCursorCollection.AddHelper(partition, dSREPLCURSOR, advanced, info);
					}
				}
				finally
				{
					this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_CURSORS_FOR_NC, info, libHandle);
				}
			}
			else
			{
				while (true)
				{
					try
					{
						if (info == (IntPtr)0)
						{
							break;
						}
						else
						{
							DS_REPL_CURSORS_3 dSREPLCURSORS3 = new DS_REPL_CURSORS_3();
							Marshal.PtrToStructure(info, dSREPLCURSORS3);
							int num1 = dSREPLCURSORS3.cNumCursors;
							if (num1 > 0)
							{
								replicationCursorCollection.AddHelper(partition, dSREPLCURSORS3, advanced, info);
							}
							num = dSREPLCURSORS3.dwEnumerationContext;
							if (num == -1 || num1 == 0)
							{
								break;
							}
						}
					}
					finally
					{
						this.FreeReplicaInfo(DS_REPL_INFO_TYPE.DS_REPL_INFO_CURSORS_3_FOR_NC, info, libHandle);
					}
					info = this.GetReplicationInfoHelper(dsHandle, 8, 1, partition, ref advanced, num, libHandle);
				}
			}
			return replicationCursorCollection;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					foreach (DirectoryEntry cachedDirectoryEntry in this.directoryEntryMgr.GetCachedDirectoryEntries())
					{
						cachedDirectoryEntry.Dispose();
					}
				}
				this.disposed = true;
			}
		}

		~DirectoryServer()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		private void FreeReplicaInfo(DS_REPL_INFO_TYPE type, IntPtr value, LoadLibrarySafeHandle libHandle)
		{
			if (value != (IntPtr)0)
			{
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaFreeInfo");
				if (procAddress != (IntPtr)0)
				{
					UnsafeNativeMethods.DsReplicaFreeInfo delegateForFunctionPointer = (UnsafeNativeMethods.DsReplicaFreeInfo)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaFreeInfo));
					delegateForFunctionPointer((int)type, value);
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract ReplicationNeighborCollection GetAllReplicationNeighbors();

		public DirectoryEntry GetDirectoryEntry()
		{
			string serverObjectName;
			this.CheckIfDisposed();
			if (this as DomainController != null)
			{
				serverObjectName = ((DomainController)this).ServerObjectName;
			}
			else
			{
				serverObjectName = ((AdamInstance)this).ServerObjectName;
			}
			string str = serverObjectName;
			return DirectoryEntryManager.GetDirectoryEntry(this.context, str);
		}

		internal ReplicationConnectionCollection GetInboundConnectionsHelper()
		{
			string serverObjectName;
			if (this.inbound == null)
			{
				this.inbound = new ReplicationConnectionCollection();
				DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context);
				if (this as DomainController != null)
				{
					serverObjectName = ((DomainController)this).ServerObjectName;
				}
				else
				{
					serverObjectName = ((AdamInstance)this).ServerObjectName;
				}
				string str = serverObjectName;
				string str1 = string.Concat("CN=NTDS Settings,", str);
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context), str1);
				string[] strArrays = new string[1];
				strArrays[0] = "cn";
				ADSearcher aDSearcher = new ADSearcher(directoryEntry, "(&(objectClass=nTDSConnection)(objectCategory=nTDSConnection))", strArrays, SearchScope.OneLevel);
				SearchResultCollection searchResultCollections = null;
				try
				{
					try
					{
						searchResultCollections = aDSearcher.FindAll();
						foreach (SearchResult searchResult in searchResultCollections)
						{
							ReplicationConnection replicationConnection = new ReplicationConnection(newDirectoryContext, searchResult.GetDirectoryEntry(), (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn));
							this.inbound.Add(replicationConnection);
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(newDirectoryContext, cOMException);
					}
				}
				finally
				{
					if (searchResultCollections != null)
					{
						searchResultCollections.Dispose();
					}
					directoryEntry.Dispose();
				}
			}
			return this.inbound;
		}

		internal ReplicationConnectionCollection GetOutboundConnectionsHelper()
		{
			string siteObjectName;
			string serverObjectName;
			if (this.outbound == null)
			{
				if (this as DomainController != null)
				{
					siteObjectName = ((DomainController)this).SiteObjectName;
				}
				else
				{
					siteObjectName = ((AdamInstance)this).SiteObjectName;
				}
				string str = siteObjectName;
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context), str);
				if (this as DomainController != null)
				{
					serverObjectName = ((DomainController)this).ServerObjectName;
				}
				else
				{
					serverObjectName = ((AdamInstance)this).ServerObjectName;
				}
				string str1 = serverObjectName;
				string[] strArrays = new string[2];
				strArrays[0] = "objectClass";
				strArrays[1] = "cn";
				ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=nTDSConnection)(objectCategory=nTDSConnection)(fromServer=CN=NTDS Settings,", str1, "))"), strArrays, SearchScope.Subtree);
				SearchResultCollection searchResultCollections = null;
				DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.DirectoryServer, this.context);
				try
				{
					try
					{
						searchResultCollections = aDSearcher.FindAll();
						this.outbound = new ReplicationConnectionCollection();
						foreach (SearchResult searchResult in searchResultCollections)
						{
							ReplicationConnection replicationConnection = new ReplicationConnection(newDirectoryContext, searchResult.GetDirectoryEntry(), (string)searchResult.Properties["cn"][0]);
							this.outbound.Add(replicationConnection);
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(newDirectoryContext, cOMException);
					}
				}
				finally
				{
					if (searchResultCollections != null)
					{
						searchResultCollections.Dispose();
					}
					directoryEntry.Dispose();
				}
			}
			return this.outbound;
		}

		internal ArrayList GetPartitions()
		{
			string ntdsaObjectName;
			ArrayList arrayLists = new ArrayList();
			DirectoryEntry directoryEntry = null;
			DirectoryEntry directoryEntry1 = null;
			try
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					foreach (string item in directoryEntry.Properties[PropertyManager.NamingContexts])
					{
						arrayLists.Add(item);
					}
					if (this as DomainController != null)
					{
						ntdsaObjectName = ((DomainController)this).NtdsaObjectName;
					}
					else
					{
						ntdsaObjectName = ((AdamInstance)this).NtdsaObjectName;
					}
					string str = ntdsaObjectName;
					directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
					ArrayList arrayLists1 = new ArrayList();
					arrayLists1.Add(PropertyManager.HasPartialReplicaNCs);
					Hashtable valuesWithRangeRetrieval = null;
					try
					{
						valuesWithRangeRetrieval = Utils.GetValuesWithRangeRetrieval(directoryEntry1, null, arrayLists1, SearchScope.Base);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					ArrayList item1 = (ArrayList)valuesWithRangeRetrieval[PropertyManager.HasPartialReplicaNCs.ToLower(CultureInfo.InvariantCulture)];
					foreach (string str1 in item1)
					{
						arrayLists.Add(str1);
					}
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
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
			return arrayLists;
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract ReplicationFailureCollection GetReplicationConnectionFailures();

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract ReplicationCursorCollection GetReplicationCursors(string partition);

		internal IntPtr GetReplicationInfoHelper(IntPtr dsHandle, int type, int secondaryType, string partition, ref bool advanced, int context, LoadLibrarySafeHandle libHandle)
		{
			IntPtr intPtr = (IntPtr)0;
			int num = 0;
			bool flag = true;
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaGetInfo2W");
			if (procAddress != (IntPtr)0)
			{
				UnsafeNativeMethods.DsReplicaGetInfo2W delegateForFunctionPointer = (UnsafeNativeMethods.DsReplicaGetInfo2W)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaGetInfo2W));
				num = delegateForFunctionPointer(dsHandle, type, partition, (IntPtr)0, null, null, 0, context, ref intPtr);
			}
			else
			{
				procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaGetInfoW");
				if (procAddress != (IntPtr)0)
				{
					UnsafeNativeMethods.DsReplicaGetInfoW dsReplicaGetInfoW = (UnsafeNativeMethods.DsReplicaGetInfoW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaGetInfoW));
					num = dsReplicaGetInfoW(dsHandle, secondaryType, partition, (IntPtr)0, ref intPtr);
					advanced = false;
					flag = false;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
			if (flag && num == 50)
			{
				procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaGetInfoW");
				if (procAddress != (IntPtr)0)
				{
					UnsafeNativeMethods.DsReplicaGetInfoW delegateForFunctionPointer1 = (UnsafeNativeMethods.DsReplicaGetInfoW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaGetInfoW));
					num = delegateForFunctionPointer1(dsHandle, secondaryType, partition, (IntPtr)0, ref intPtr);
					advanced = false;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
			if (num == 0)
			{
				return intPtr;
			}
			else
			{
				if (partition != null)
				{
					if (type != 9)
					{
						if (!this.Partitions.Contains(partition))
						{
							throw new ArgumentException(Res.GetString("ServerNotAReplica"), "partition");
						}
					}
					else
					{
						if (num == ExceptionHelper.ERROR_DS_DRA_BAD_DN || num == ExceptionHelper.ERROR_DS_NAME_UNPARSEABLE)
						{
							throw new ArgumentException(ExceptionHelper.GetErrorMessage(num, false), "objectPath");
						}
						else
						{
							DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, partition);
							try
							{
								string[] strArrays = new string[1];
								strArrays[0] = "name";
								directoryEntry.RefreshCache(strArrays);
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (!(cOMException.ErrorCode == -2147016672 | cOMException.ErrorCode == -2147016656))
								{
									if (cOMException.ErrorCode == -2147463168 | cOMException.ErrorCode == -2147016654)
									{
										throw new ArgumentException(Res.GetString("DSInvalidPath"), "objectPath");
									}
								}
								else
								{
									throw new ArgumentException(Res.GetString("DSNoObject"), "objectPath");
								}
							}
						}
					}
				}
				throw ExceptionHelper.GetExceptionFromErrorCode(num, this.Name);
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract ActiveDirectoryReplicationMetadata GetReplicationMetadata(string objectPath);

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract ReplicationNeighborCollection GetReplicationNeighbors(string partition);

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract ReplicationOperationInformation GetReplicationOperationInformation();

		public void MoveToAnotherSite(string siteName)
		{
			string serverObjectName;
			this.CheckIfDisposed();
			if (siteName != null)
			{
				if (siteName.Length != 0)
				{
					if (Utils.Compare(this.SiteName, siteName) != 0)
					{
						DirectoryEntry directoryEntry = null;
						using (directoryEntry)
						{
							try
							{
								string str = string.Concat("CN=Servers,CN=", siteName, ",", this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SitesContainer));
								directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
								if (this as DomainController != null)
								{
									serverObjectName = ((DomainController)this).ServerObjectName;
								}
								else
								{
									serverObjectName = ((AdamInstance)this).ServerObjectName;
								}
								string str1 = serverObjectName;
								DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(str1);
								cachedDirectoryEntry.MoveTo(directoryEntry);
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
							}
						}
						this.siteInfoModified = true;
						this.cachedSiteName = null;
						if (this.cachedSiteObjectName != null)
						{
							this.directoryEntryMgr.RemoveIfExists(this.cachedSiteObjectName);
							this.cachedSiteObjectName = null;
						}
						if (this.cachedServerObjectName != null)
						{
							this.directoryEntryMgr.RemoveIfExists(this.cachedServerObjectName);
							this.cachedServerObjectName = null;
						}
						if (this.cachedNtdsaObjectName != null)
						{
							this.directoryEntryMgr.RemoveIfExists(this.cachedNtdsaObjectName);
							this.cachedNtdsaObjectName = null;
						}
					}
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
				}
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		internal bool SyncAllCallbackRoutine(IntPtr data, IntPtr update)
		{
			if (this.SyncFromAllServersCallback != null)
			{
				DS_REPSYNCALL_UPDATE dSREPSYNCALLUPDATE = new DS_REPSYNCALL_UPDATE();
				Marshal.PtrToStructure(update, dSREPSYNCALLUPDATE);
				SyncFromAllServersEvent syncFromAllServersEvent = dSREPSYNCALLUPDATE.eventType;
				IntPtr intPtr = dSREPSYNCALLUPDATE.pErrInfo;
				SyncFromAllServersOperationException syncFromAllServersOperationException = null;
				if (intPtr != (IntPtr)0)
				{
					syncFromAllServersOperationException = ExceptionHelper.CreateSyncAllException(intPtr, true);
					if (syncFromAllServersOperationException == null)
					{
						return true;
					}
				}
				string stringUni = null;
				string str = null;
				intPtr = dSREPSYNCALLUPDATE.pSync;
				if (intPtr != (IntPtr)0)
				{
					DS_REPSYNCALL_SYNC dSREPSYNCALLSYNC = new DS_REPSYNCALL_SYNC();
					Marshal.PtrToStructure(intPtr, dSREPSYNCALLSYNC);
					stringUni = Marshal.PtrToStringUni(dSREPSYNCALLSYNC.pszDstId);
					str = Marshal.PtrToStringUni(dSREPSYNCALLSYNC.pszSrcId);
				}
				SyncUpdateCallback syncFromAllServersCallback = this.SyncFromAllServersCallback;
				return syncFromAllServersCallback(syncFromAllServersEvent, stringUni, str, syncFromAllServersOperationException);
			}
			else
			{
				return true;
			}
		}

		internal void SyncReplicaAllHelper(IntPtr handle, SyncReplicaFromAllServersCallback syncAllFunctionPointer, string partition, SyncFromAllServersOptions option, SyncUpdateCallback callback, LoadLibrarySafeHandle libHandle)
		{
			IntPtr intPtr = (IntPtr)0;
			if (this.Partitions.Contains(partition))
			{
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaSyncAllW");
				if (procAddress != (IntPtr)0)
				{
					UnsafeNativeMethods.DsReplicaSyncAllW delegateForFunctionPointer = (UnsafeNativeMethods.DsReplicaSyncAllW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaSyncAllW));
					int num = delegateForFunctionPointer(handle, partition, (int)option | 4, syncAllFunctionPointer, (IntPtr)0, ref intPtr);
					try
					{
						if (intPtr == (IntPtr)0)
						{
							if (num != 0)
							{
								throw new SyncFromAllServersOperationException(ExceptionHelper.GetErrorMessage(num, false));
							}
						}
						else
						{
							SyncFromAllServersOperationException syncFromAllServersOperationException = ExceptionHelper.CreateSyncAllException(intPtr, false);
							if (syncFromAllServersOperationException != null)
							{
								throw syncFromAllServersOperationException;
							}
						}
					}
					finally
					{
						if (intPtr != (IntPtr)0)
						{
							UnsafeNativeMethods.LocalFree(intPtr);
						}
					}
					return;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("ServerNotAReplica"), "partition");
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract void SyncReplicaFromAllServers(string partition, SyncFromAllServersOptions options);

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract void SyncReplicaFromServer(string partition, string sourceServer);

		internal void SyncReplicaHelper(IntPtr dsHandle, bool isADAM, string partition, string sourceServer, int option, LoadLibrarySafeHandle libHandle)
		{
			Guid ntdsaObjectGuid;
			int num = Marshal.SizeOf(typeof(Guid));
			AdamInstance adamInstance = null;
			DomainController domainController = null;
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			try
			{
				if (sourceServer != null)
				{
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(sourceServer, DirectoryContextType.DirectoryServer, this.context);
					if (!isADAM)
					{
						domainController = DomainController.GetDomainController(newDirectoryContext);
						ntdsaObjectGuid = domainController.NtdsaObjectGuid;
					}
					else
					{
						adamInstance = AdamInstance.GetAdamInstance(newDirectoryContext);
						ntdsaObjectGuid = adamInstance.NtdsaObjectGuid;
					}
					Marshal.StructureToPtr(ntdsaObjectGuid, intPtr, false);
				}
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsReplicaSyncW");
				if (procAddress != (IntPtr)0)
				{
					UnsafeNativeMethods.DsReplicaSyncW delegateForFunctionPointer = (UnsafeNativeMethods.DsReplicaSyncW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsReplicaSyncW));
					int num1 = delegateForFunctionPointer(dsHandle, partition, intPtr, option);
					if (num1 != 0)
					{
						if (this.Partitions.Contains(partition))
						{
							string name = null;
							if (num1 != ExceptionHelper.RPC_S_SERVER_UNAVAILABLE)
							{
								if (num1 == ExceptionHelper.RPC_S_CALL_FAILED)
								{
									name = this.Name;
								}
							}
							else
							{
								name = sourceServer;
							}
							throw ExceptionHelper.GetExceptionFromErrorCode(num1, name);
						}
						else
						{
							throw new ArgumentException(Res.GetString("ServerNotAReplica"), "partition");
						}
					}
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Marshal.FreeHGlobal(intPtr);
				}
				if (adamInstance != null)
				{
					adamInstance.Dispose();
				}
				if (domainController != null)
				{
					domainController.Dispose();
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract void TriggerSyncReplicaFromNeighbors(string partition);
	}
}