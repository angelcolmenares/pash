using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class Forest : IDisposable
	{
		private DirectoryContext context;

		private DirectoryEntryManager directoryEntryMgr;

		private IntPtr dsHandle;

		private IntPtr authIdentity;

		private bool disposed;

		private string forestDnsName;

		private ReadOnlySiteCollection cachedSites;

		private DomainCollection cachedDomains;

		private GlobalCatalogCollection cachedGlobalCatalogs = null;

		private ApplicationPartitionCollection cachedApplicationPartitions;

		private ForestMode currentForestMode;

		private Domain cachedRootDomain;

		private ActiveDirectorySchema cachedSchema;

		private DomainController cachedSchemaRoleOwner;

		private DomainController cachedNamingRoleOwner;

		public ApplicationPartitionCollection ApplicationPartitions
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedApplicationPartitions == null)
				{
					this.cachedApplicationPartitions = new ApplicationPartitionCollection(this.GetApplicationPartitions());
				}
				return this.cachedApplicationPartitions;
			}
		}

		public DomainCollection Domains
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedDomains == null)
				{
					this.cachedDomains = new DomainCollection(this.GetDomains());
				}
				return this.cachedDomains;
			}
		}

		public ForestMode ForestMode
		{
			get
			{
				this.CheckIfDisposed();
				if (this.currentForestMode == (ForestMode.Windows2003InterimForest | ForestMode.Windows2003Forest | ForestMode.Windows2008Forest | ForestMode.Windows2008R2Forest | ForestMode.Windows8Forest))
				{
					this.currentForestMode = this.GetForestMode();
				}
				return this.currentForestMode;
			}
		}

		public GlobalCatalogCollection GlobalCatalogs
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedGlobalCatalogs == null)
				{
					this.cachedGlobalCatalogs = this.FindAllGlobalCatalogs();
				}
				return this.cachedGlobalCatalogs;
			}
		}

		public string Name
		{
			get
			{
				this.CheckIfDisposed();
				return this.forestDnsName;
			}
		}

		public DomainController NamingRoleOwner
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedNamingRoleOwner == null)
				{
					this.cachedNamingRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.NamingRole);
				}
				return this.cachedNamingRoleOwner;
			}
		}

		public Domain RootDomain
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedRootDomain == null)
				{
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.Domain, this.context);
					this.cachedRootDomain = new Domain(newDirectoryContext, this.Name);
				}
				return this.cachedRootDomain;
			}
		}

		public ActiveDirectorySchema Schema
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedSchema == null)
				{
					try
					{
						this.cachedSchema = new ActiveDirectorySchema(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext));
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
				}
				return this.cachedSchema;
			}
		}

		public DomainController SchemaRoleOwner
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedSchemaRoleOwner == null)
				{
					this.cachedSchemaRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.SchemaRole);
				}
				return this.cachedSchemaRoleOwner;
			}
		}

		public ReadOnlySiteCollection Sites
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedSites == null)
				{
					this.cachedSites = new ReadOnlySiteCollection(this.GetSites());
				}
				return this.cachedSites;
			}
		}

		internal Forest(DirectoryContext context, string forestDnsName, DirectoryEntryManager directoryEntryMgr)
		{
			this.dsHandle = IntPtr.Zero;
			this.authIdentity = IntPtr.Zero;
			this.currentForestMode = ForestMode.Windows2003InterimForest | ForestMode.Windows2003Forest | ForestMode.Windows2008Forest | ForestMode.Windows2008R2Forest | ForestMode.Windows8Forest | ForestMode.Windows2012Forest;
			this.context = context;
			this.directoryEntryMgr = directoryEntryMgr;
			this.forestDnsName = forestDnsName;
		}

		internal Forest(DirectoryContext context, string name) : this(context, name, new DirectoryEntryManager(context))
		{
		}

		private void CheckIfDisposed()
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

		public void CreateLocalSideOfTrustRelationship(string targetForestName, TrustDirection direction, string trustPassword)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					if (direction < TrustDirection.Inbound || direction > TrustDirection.Bidirectional)
					{
						throw new InvalidEnumArgumentException("direction", (int)direction, typeof(TrustDirection));
					}
					else
					{
						if (trustPassword != null)
						{
							if (trustPassword.Length != 0)
							{
								Locator.GetDomainControllerInfo(null, targetForestName, null, (long)80);
								DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(targetForestName, DirectoryContextType.Forest, this.context);
								TrustHelper.CreateTrust(this.context, this.Name, newDirectoryContext, targetForestName, true, direction, trustPassword);
								return;
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "trustPassword");
							}
						}
						else
						{
							throw new ArgumentNullException("trustPassword");
						}
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public void CreateTrustRelationship(Forest targetForest, TrustDirection direction)
		{
			this.CheckIfDisposed();
			if (targetForest != null)
			{
				if (direction < TrustDirection.Inbound || direction > TrustDirection.Bidirectional)
				{
					throw new InvalidEnumArgumentException("direction", (int)direction, typeof(TrustDirection));
				}
				else
				{
					string str = TrustHelper.CreateTrustPassword();
					TrustHelper.CreateTrust(this.context, this.Name, targetForest.GetDirectoryContext(), targetForest.Name, true, direction, str);
					int num = 0;
					if ((direction & TrustDirection.Inbound) != 0)
					{
						num = num | 2;
					}
					if ((direction & TrustDirection.Outbound) != 0)
					{
						num = num | 1;
					}
					TrustHelper.CreateTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.context, this.Name, true, (TrustDirection)num, str);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("targetForest");
			}
		}

		public void DeleteLocalSideOfTrustRelationship(string targetForestName)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					TrustHelper.DeleteTrust(this.context, this.Name, targetForestName, true);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public void DeleteTrustRelationship(Forest targetForest)
		{
			this.CheckIfDisposed();
			if (targetForest != null)
			{
				TrustHelper.DeleteTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true);
				TrustHelper.DeleteTrust(this.context, this.Name, targetForest.Name, true);
				return;
			}
			else
			{
				throw new ArgumentNullException("targetForest");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Dispose()
		{
			this.Dispose(true);
		}

		protected void Dispose(bool disposing)
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

		public GlobalCatalogCollection FindAllDiscoverableGlobalCatalogs()
		{
			long num = (long)64;
			this.CheckIfDisposed();
			return new GlobalCatalogCollection(Locator.EnumerateDomainControllers(this.context, this.Name, null, num));
		}

		public GlobalCatalogCollection FindAllDiscoverableGlobalCatalogs(string siteName)
		{
			long num = (long)64;
			this.CheckIfDisposed();
			if (siteName != null)
			{
				if (siteName.Length != 0)
				{
					return new GlobalCatalogCollection(Locator.EnumerateDomainControllers(this.context, this.Name, siteName, num));
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

		public GlobalCatalogCollection FindAllGlobalCatalogs()
		{
			this.CheckIfDisposed();
			return GlobalCatalog.FindAllInternal(this.context, null);
		}

		public GlobalCatalogCollection FindAllGlobalCatalogs(string siteName)
		{
			this.CheckIfDisposed();
			if (siteName != null)
			{
				return GlobalCatalog.FindAllInternal(this.context, siteName);
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public GlobalCatalog FindGlobalCatalog()
		{
			this.CheckIfDisposed();
			return GlobalCatalog.FindOneInternal(this.context, this.Name, null, (LocatorOptions)((long)0));
		}

		public GlobalCatalog FindGlobalCatalog(string siteName)
		{
			this.CheckIfDisposed();
			if (siteName != null)
			{
				return GlobalCatalog.FindOneInternal(this.context, this.Name, siteName, (LocatorOptions)((long)0));
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public GlobalCatalog FindGlobalCatalog(LocatorOptions flag)
		{
			this.CheckIfDisposed();
			return GlobalCatalog.FindOneInternal(this.context, this.Name, null, flag);
		}

		public GlobalCatalog FindGlobalCatalog(string siteName, LocatorOptions flag)
		{
			this.CheckIfDisposed();
			if (siteName != null)
			{
				return GlobalCatalog.FindOneInternal(this.context, this.Name, siteName, flag);
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public TrustRelationshipInformationCollection GetAllTrustRelationships()
		{
			this.CheckIfDisposed();
			return this.GetTrustsHelper(null);
		}

		private ArrayList GetApplicationPartitions()
		{
			ArrayList arrayLists = new ArrayList();
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
			StringBuilder stringBuilder = new StringBuilder(15);
			stringBuilder.Append("(&(");
			stringBuilder.Append(PropertyManager.ObjectCategory);
			stringBuilder.Append("=crossRef)(");
			stringBuilder.Append(PropertyManager.SystemFlags);
			stringBuilder.Append(":1.2.840.113556.1.4.804:=");
			stringBuilder.Append(1);
			stringBuilder.Append(")(!(");
			stringBuilder.Append(PropertyManager.SystemFlags);
			stringBuilder.Append(":1.2.840.113556.1.4.803:=");
			stringBuilder.Append(2);
			stringBuilder.Append(")))");
			string str = stringBuilder.ToString();
			string[] dnsRoot = new string[2];
			dnsRoot[0] = PropertyManager.DnsRoot;
			dnsRoot[1] = PropertyManager.NCName;
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, str, dnsRoot, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			try
			{
				try
				{
					searchResultCollections = aDSearcher.FindAll();
					string str1 = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
					string str2 = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.ConfigurationNamingContext);
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.NCName);
						if (searchResultPropertyValue.Equals(str1) || searchResultPropertyValue.Equals(str2))
						{
							continue;
						}
						string searchResultPropertyValue1 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsRoot);
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(searchResultPropertyValue1, DirectoryContextType.ApplicationPartition, this.context);
						arrayLists.Add(new ApplicationPartition(newDirectoryContext, searchResultPropertyValue, (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsRoot), ApplicationPartitionType.ADApplicationPartition, new DirectoryEntryManager(newDirectoryContext)));
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
				if (searchResultCollections != null)
				{
					searchResultCollections.Dispose();
				}
				directoryEntry.Dispose();
			}
			return arrayLists;
		}

		public static Forest GetCurrentForest()
		{
			return Forest.GetForest(new DirectoryContext(DirectoryContextType.Forest));
		}

		internal DirectoryContext GetDirectoryContext()
		{
			return this.context;
		}

		private ArrayList GetDomains()
		{
			ArrayList arrayLists = new ArrayList();
			var dn = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer);
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
			StringBuilder stringBuilder = new StringBuilder(15);
			stringBuilder.Append("(&(");
			stringBuilder.Append(PropertyManager.ObjectCategory);
			stringBuilder.Append("=crossRef)(");
			stringBuilder.Append(PropertyManager.SystemFlags);
			stringBuilder.Append(":1.2.840.113556.1.4.804:=");
			stringBuilder.Append(1);
			stringBuilder.Append(")(");
			stringBuilder.Append(PropertyManager.SystemFlags);
			stringBuilder.Append(":1.2.840.113556.1.4.804:=");
			stringBuilder.Append(2);
			stringBuilder.Append("))");
			string str = stringBuilder.ToString();
			string[] dnsRoot = new string[1];
			dnsRoot[0] = PropertyManager.DnsRoot;
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, str, dnsRoot, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			try
			{
				try
				{
					searchResultCollections = aDSearcher.FindAll();
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsRoot);
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.Domain, this.context);
						arrayLists.Add(new Domain(newDirectoryContext, searchResultPropertyValue));
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
				if (searchResultCollections != null)
				{
					searchResultCollections.Dispose();
				}
				directoryEntry.Dispose();
			}
			return arrayLists;
		}

		private void GetDSHandle(out IntPtr dsHandle, out IntPtr authIdentity)
		{
			authIdentity = Utils.GetAuthIdentity(this.context, DirectoryContext.ADHandle);
			if (this.context.ContextType != DirectoryContextType.DirectoryServer)
			{
				dsHandle = Utils.GetDSHandle(null, this.context.GetServerName(), authIdentity, DirectoryContext.ADHandle);
				return;
			}
			else
			{
				dsHandle = Utils.GetDSHandle(this.context.GetServerName(), null, authIdentity, DirectoryContext.ADHandle);
				return;
			}
		}

		public static Forest GetForest(DirectoryContext context)
		{
			string propertyValue = null;
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Forest || context.ContextType == DirectoryContextType.DirectoryServer)
				{
					if (context.Name != null || context.isRootDomain())
					{
						if (context.Name == null || context.isRootDomain() || context.isServer())
						{
							context = new DirectoryContext(context);
							DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
							try
							{
								DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
								if (!context.isServer() || Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectory))
								{
									propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.RootDomainNamingContext);
								}
								else
								{
									object[] name = new object[1];
									name[0] = context.Name;
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", name), typeof(Forest), null);
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
									if (context.ContextType != DirectoryContextType.Forest)
									{
										object[] objArray = new object[1];
										objArray[0] = context.Name;
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", objArray), typeof(Forest), null);
									}
									else
									{
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(Forest), context.Name);
									}
								}
							}
							return new Forest(context, Utils.GetDnsNameFromDN(propertyValue), directoryEntryManager);
						}
						else
						{
							if (context.ContextType != DirectoryContextType.Forest)
							{
								object[] name1 = new object[1];
								name1[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", name1), typeof(Forest), null);
							}
							else
							{
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(Forest), context.Name);
							}
						}
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(Forest), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeServerORForest"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private ForestMode GetForestMode()
		{
			ForestMode forestMode;
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
			try
			{
				try
				{
					if (directoryEntry.Properties.Contains(PropertyManager.ForestFunctionality))
					{
						forestMode = (ForestMode)int.Parse((string)directoryEntry.Properties[PropertyManager.ForestFunctionality].Value, NumberFormatInfo.InvariantInfo);
					}
					else
					{
						forestMode = ForestMode.Windows2000Forest;
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
				directoryEntry.Dispose();
			}
			return forestMode;
		}

		private DomainController GetRoleOwner(ActiveDirectoryRole role)
		{
			DirectoryEntry directoryEntry = null;
			string dnsHostNameFromNTDSA = null;
			using (directoryEntry)
			{
				try
				{
					ActiveDirectoryRole activeDirectoryRole = role;
					switch (activeDirectoryRole)
					{
						case ActiveDirectoryRole.SchemaRole:
						{
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext));
							break;
						}
						case ActiveDirectoryRole.NamingRole:
						{
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
							break;
						}
					}
					dnsHostNameFromNTDSA = Utils.GetDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.FsmoRoleOwner));
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(dnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, this.context);
			return new DomainController(newDirectoryContext, dnsHostNameFromNTDSA);
		}

		public bool GetSelectiveAuthenticationStatus(string targetForestName)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					return TrustHelper.GetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, true);
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public bool GetSidFilteringStatus(string targetForestName)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					return TrustHelper.GetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL, true);
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		private ArrayList GetSites ()
		{
			ArrayList arrayLists = new ArrayList ();



			try {
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry (this.context, this.directoryEntryMgr.ExpandWellKnownDN (WellKnownDN.SitesContainer));
				
				ADSearcher aDSearcher = new ADSearcher (directoryEntry, "(&(objectClass=site))", new string[] { "cn" }, SearchScope.OneLevel);
				SearchResultCollection searchResultCollections = null;
				searchResultCollections = aDSearcher.FindAll ();
				foreach(SearchResult result in searchResultCollections)
				{
					var entry = result.GetDirectoryEntry ();
					var site = new System.DirectoryServices.ActiveDirectory.ActiveDirectorySite(this.context, (string)entry.Properties["cn"].Value, true);
					arrayLists.Add (site);
				}
			} catch (Exception ex) 
			{
				var msg = ex.Message;
			}


			/*
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			try
			{
				this.GetDSHandle(out zero, out intPtr);
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsListSitesW");
				if (procAddress != (IntPtr)0)
				{
					NativeMethods.DsListSites delegateForFunctionPointer = (NativeMethods.DsListSites)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsListSites));
					int num = delegateForFunctionPointer(zero, out zero1);
					if (num != 0)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(num, this.context.GetServerName());
					}
					else
					{
						try
						{
							DsNameResult dsNameResult = new DsNameResult();
							Marshal.PtrToStructure(zero1, dsNameResult);
							IntPtr intPtr1 = dsNameResult.items;
							for (int i = 0; i < dsNameResult.itemCount; i++)
							{
								DsNameResultItem dsNameResultItem = new DsNameResultItem();
								Marshal.PtrToStructure(intPtr1, dsNameResultItem);
								if (dsNameResultItem.status == 0)
								{
									string value = Utils.GetDNComponents(dsNameResultItem.name)[0].Value;
									arrayLists.Add(new ActiveDirectorySite(this.context, value, true));
								}
								intPtr1 = (IntPtr)((long)intPtr1 + (long)Marshal.SizeOf(dsNameResultItem));
							}
						}
						finally
						{
							if (zero1 != IntPtr.Zero)
							{
								procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
								if (procAddress != (IntPtr)0)
								{
									UnsafeNativeMethods.DsFreeNameResultW dsFreeNameResultW = (UnsafeNativeMethods.DsFreeNameResultW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsFreeNameResultW));
									dsFreeNameResultW(zero1);
								}
								else
								{
									throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
								}
							}
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
				if (zero != (IntPtr)0)
				{
					Utils.FreeDSHandle(zero, DirectoryContext.ADHandle);
				}
				if (intPtr != (IntPtr)0)
				{
					Utils.FreeAuthIdentity(intPtr, DirectoryContext.ADHandle);
				}
			}
			*/
			return arrayLists;
		}

		public ForestTrustRelationshipInformation GetTrustRelationship(string targetForestName)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					TrustRelationshipInformationCollection trustsHelper = this.GetTrustsHelper(targetForestName);
					if (trustsHelper.Count == 0)
					{
						object[] name = new object[2];
						name[0] = this.Name;
						name[1] = targetForestName;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", name), typeof(TrustRelationshipInformation), null);
					}
					else
					{
						return (ForestTrustRelationshipInformation)trustsHelper[0];
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		private TrustRelationshipInformationCollection GetTrustsHelper(string targetForestName)
		{
			TrustRelationshipInformationCollection trustRelationshipInformationCollection;
			IntPtr intPtr = (IntPtr)0;
			int num = 0;
			TrustRelationshipInformationCollection trustRelationshipInformationCollection1 = new TrustRelationshipInformationCollection();
			int num1 = 0;
			string policyServerName = Utils.GetPolicyServerName(this.context, true, false, this.Name);
			bool flag = Utils.Impersonate(this.context);
			try
			{
				try
				{
					num1 = UnsafeNativeMethods.DsEnumerateDomainTrustsW(policyServerName, 42, out intPtr, out num);
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
				}
			}
			catch
			{
				throw;
			}
			if (num1 == 0)
			{
				try
				{
					if (intPtr != (IntPtr)0 && num != 0)
					{
						for (int i = 0; i < num; i++)
						{
							IntPtr intPtr1 = (IntPtr)((long)intPtr + (long)(i * Marshal.SizeOf(typeof(DS_DOMAIN_TRUSTS))));
							DS_DOMAIN_TRUSTS dSDOMAINTRUST = new DS_DOMAIN_TRUSTS();
							Marshal.PtrToStructure(intPtr1, dSDOMAINTRUST);
							if (targetForestName != null)
							{
								bool flag1 = false;
								string stringUni = null;
								string str = null;
								if (dSDOMAINTRUST.DnsDomainName != (IntPtr)0)
								{
									stringUni = Marshal.PtrToStringUni(dSDOMAINTRUST.DnsDomainName);
								}
								if (dSDOMAINTRUST.NetbiosDomainName != (IntPtr)0)
								{
									str = Marshal.PtrToStringUni(dSDOMAINTRUST.NetbiosDomainName);
								}
								if (stringUni == null || Utils.Compare(targetForestName, stringUni) != 0)
								{
									if (str != null && Utils.Compare(targetForestName, str) == 0)
									{
										flag1 = true;
									}
								}
								else
								{
									flag1 = true;
								}
								if (!flag1)
								{
									goto Label0;
								}
							}
							if (dSDOMAINTRUST.TrustType == TrustHelper.TRUST_TYPE_UPLEVEL && (dSDOMAINTRUST.TrustAttributes & 8) != 0 && (dSDOMAINTRUST.Flags & 8) == 0)
							{
								TrustRelationshipInformation forestTrustRelationshipInformation = new ForestTrustRelationshipInformation(this.context, this.Name, dSDOMAINTRUST, TrustType.Forest);
								trustRelationshipInformationCollection1.Add(forestTrustRelationshipInformation);
							}
						Label0:
							continue;
						}
					}
					trustRelationshipInformationCollection = trustRelationshipInformationCollection1;
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.NetApiBufferFree(intPtr);
					}
				}
				return trustRelationshipInformationCollection;
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(num1, policyServerName);
			}
		}

		public void RaiseForestFunctionality(ForestMode forestMode)
		{
			this.CheckIfDisposed();
			if (forestMode < ForestMode.Windows2000Forest || forestMode > ForestMode.Windows8Forest)
			{
				throw new InvalidEnumArgumentException("forestMode", (int)forestMode, typeof(ForestMode));
			}
			else
			{
				if (forestMode > this.GetForestMode())
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
					try
					{
						try
						{
							directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = (int)forestMode;
							directoryEntry.CommitChanges();
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							if (cOMException.ErrorCode != -2147016694)
							{
								throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
							}
							else
							{
								throw new ArgumentException(Res.GetString("NoW2K3DCsInForest"), "forestMode");
							}
						}
					}
					finally
					{
						directoryEntry.Dispose();
					}
					this.currentForestMode = ForestMode.Windows2003InterimForest | ForestMode.Windows2003Forest | ForestMode.Windows2008Forest | ForestMode.Windows2008R2Forest | ForestMode.Windows8Forest;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidMode"), "forestMode");
				}
			}
		}

		private void RepairTrustHelper(Forest targetForest, TrustDirection direction)
		{
			string str = TrustHelper.CreateTrustPassword();
			string str1 = TrustHelper.UpdateTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, str, true);
			string str2 = TrustHelper.UpdateTrust(this.context, this.Name, targetForest.Name, str, true);
			if ((direction & TrustDirection.Outbound) != 0)
			{
				try
				{
					TrustHelper.VerifyTrust(this.context, this.Name, targetForest.Name, true, TrustDirection.Outbound, true, str1);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] name = new object[3];
					name[0] = this.Name;
					name[1] = targetForest.Name;
					name[2] = direction;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", name), typeof(ForestTrustRelationshipInformation), null);
				}
			}
			if ((direction & TrustDirection.Inbound) != 0)
			{
				try
				{
					TrustHelper.VerifyTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true, TrustDirection.Outbound, true, str2);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException1)
				{
					object[] objArray = new object[3];
					objArray[0] = this.Name;
					objArray[1] = targetForest.Name;
					objArray[2] = direction;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", objArray), typeof(ForestTrustRelationshipInformation), null);
				}
			}
		}

		public void RepairTrustRelationship(Forest targetForest)
		{
			TrustDirection trustDirection = TrustDirection.Bidirectional;
			this.CheckIfDisposed();
			if (targetForest != null)
			{
				try
				{
					trustDirection = this.GetTrustRelationship(targetForest.Name).TrustDirection;
					if ((trustDirection & TrustDirection.Outbound) != 0)
					{
						TrustHelper.VerifyTrust(this.context, this.Name, targetForest.Name, true, TrustDirection.Outbound, true, null);
					}
					if ((trustDirection & TrustDirection.Inbound) != 0)
					{
						TrustHelper.VerifyTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true, TrustDirection.Outbound, true, null);
					}
				}
				catch (ActiveDirectoryOperationException activeDirectoryOperationException)
				{
					this.RepairTrustHelper(targetForest, trustDirection);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException)
				{
					this.RepairTrustHelper(targetForest, trustDirection);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] name = new object[3];
					name[0] = this.Name;
					name[1] = targetForest.Name;
					name[2] = trustDirection;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", name), typeof(ForestTrustRelationshipInformation), null);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("targetForest");
			}
		}

		public void SetSelectiveAuthenticationStatus(string targetForestName, bool enable)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					TrustHelper.SetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, enable, true);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public void SetSidFilteringStatus(string targetForestName, bool enable)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					TrustHelper.SetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL, enable, true);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}

		public void UpdateLocalSideOfTrustRelationship(string targetForestName, string newTrustPassword)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					if (newTrustPassword != null)
					{
						if (newTrustPassword.Length != 0)
						{
							TrustHelper.UpdateTrust(this.context, this.Name, targetForestName, newTrustPassword, true);
							return;
						}
						else
						{
							throw new ArgumentException(Res.GetString("EmptyStringParameter"), "newTrustPassword");
						}
					}
					else
					{
						throw new ArgumentNullException("newTrustPassword");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public void UpdateLocalSideOfTrustRelationship(string targetForestName, TrustDirection newTrustDirection, string newTrustPassword)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					if (newTrustDirection < TrustDirection.Inbound || newTrustDirection > TrustDirection.Bidirectional)
					{
						throw new InvalidEnumArgumentException("newTrustDirection", (int)newTrustDirection, typeof(TrustDirection));
					}
					else
					{
						if (newTrustPassword != null)
						{
							if (newTrustPassword.Length != 0)
							{
								TrustHelper.UpdateTrustDirection(this.context, this.Name, targetForestName, newTrustPassword, true, newTrustDirection);
								return;
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "newTrustPassword");
							}
						}
						else
						{
							throw new ArgumentNullException("newTrustPassword");
						}
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public void UpdateTrustRelationship(Forest targetForest, TrustDirection newTrustDirection)
		{
			this.CheckIfDisposed();
			if (targetForest != null)
			{
				if (newTrustDirection < TrustDirection.Inbound || newTrustDirection > TrustDirection.Bidirectional)
				{
					throw new InvalidEnumArgumentException("newTrustDirection", (int)newTrustDirection, typeof(TrustDirection));
				}
				else
				{
					string str = TrustHelper.CreateTrustPassword();
					TrustHelper.UpdateTrustDirection(this.context, this.Name, targetForest.Name, str, true, newTrustDirection);
					TrustDirection trustDirection = 0;
					if ((newTrustDirection & TrustDirection.Inbound) != 0)
					{
						trustDirection = trustDirection | TrustDirection.Outbound;
					}
					if ((newTrustDirection & TrustDirection.Outbound) != 0)
					{
						trustDirection = trustDirection | TrustDirection.Inbound;
					}
					TrustHelper.UpdateTrustDirection(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, str, true, trustDirection);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("targetForest");
			}
		}

		public void VerifyOutboundTrustRelationship(string targetForestName)
		{
			this.CheckIfDisposed();
			if (targetForestName != null)
			{
				if (targetForestName.Length != 0)
				{
					TrustHelper.VerifyTrust(this.context, this.Name, targetForestName, true, TrustDirection.Outbound, false, null);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetForestName");
			}
		}

		public void VerifyTrustRelationship(Forest targetForest, TrustDirection direction)
		{
			this.CheckIfDisposed();
			if (targetForest != null)
			{
				if (direction < TrustDirection.Inbound || direction > TrustDirection.Bidirectional)
				{
					throw new InvalidEnumArgumentException("direction", (int)direction, typeof(TrustDirection));
				}
				else
				{
					if ((direction & TrustDirection.Outbound) != 0)
					{
						try
						{
							TrustHelper.VerifyTrust(this.context, this.Name, targetForest.Name, true, TrustDirection.Outbound, false, null);
						}
						catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
						{
							object[] name = new object[3];
							name[0] = this.Name;
							name[1] = targetForest.Name;
							name[2] = direction;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", name), typeof(ForestTrustRelationshipInformation), null);
						}
					}
					if ((direction & TrustDirection.Inbound) != 0)
					{
						try
						{
							TrustHelper.VerifyTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true, TrustDirection.Outbound, false, null);
						}
						catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException1)
						{
							object[] objArray = new object[3];
							objArray[0] = this.Name;
							objArray[1] = targetForest.Name;
							objArray[2] = direction;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", objArray), typeof(ForestTrustRelationshipInformation), null);
						}
					}
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("targetForest");
			}
		}
	}
}