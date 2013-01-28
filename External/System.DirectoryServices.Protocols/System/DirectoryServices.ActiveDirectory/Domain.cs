using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class Domain : ActiveDirectoryPartition
	{
		private string crossRefDN;

		private string trustParent;

		private DomainControllerCollection cachedDomainControllers;

		private DomainCollection cachedChildren;

		private DomainMode currentDomainMode;

		private DomainController cachedPdcRoleOwner;

		private DomainController cachedRidRoleOwner;

		private DomainController cachedInfrastructureRoleOwner;

		private Domain cachedParent;

		private Forest cachedForest;

		private bool isParentInitialized;

		public DomainCollection Children
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedChildren == null)
				{
					this.cachedChildren = new DomainCollection(this.GetChildDomains());
				}
				return this.cachedChildren;
			}
		}

		public DomainControllerCollection DomainControllers
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedDomainControllers == null)
				{
					this.cachedDomainControllers = this.FindAllDomainControllers();
				}
				return this.cachedDomainControllers;
			}
		}

		public DomainMode DomainMode
		{
			get
			{
				base.CheckIfDisposed();
				if (this.currentDomainMode == (DomainMode.Windows2000NativeDomain | DomainMode.Windows2003InterimDomain | DomainMode.Windows2003Domain | DomainMode.Windows2008Domain | DomainMode.Windows2008R2Domain | DomainMode.Windows8Domain))
				{
					this.currentDomainMode = this.GetDomainMode();
				}
				return this.currentDomainMode;
			}
		}

		public Forest Forest
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedForest == null)
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
					string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.RootDomainNamingContext);
					string dnsNameFromDN = Utils.GetDnsNameFromDN(propertyValue);
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(dnsNameFromDN, DirectoryContextType.Forest, this.context);
					this.cachedForest = new Forest(newDirectoryContext, dnsNameFromDN);
				}
				return this.cachedForest;
			}
		}

		public DomainController InfrastructureRoleOwner
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedInfrastructureRoleOwner == null)
				{
					this.cachedInfrastructureRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.InfrastructureRole);
				}
				return this.cachedInfrastructureRoleOwner;
			}
		}

		public Domain Parent
		{
			get
			{
				base.CheckIfDisposed();
				if (!this.isParentInitialized)
				{
					this.cachedParent = this.GetParent();
					this.isParentInitialized = true;
				}
				return this.cachedParent;
			}
		}

		public DomainController PdcRoleOwner
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedPdcRoleOwner == null)
				{
					this.cachedPdcRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.PdcRole);
				}
				return this.cachedPdcRoleOwner;
			}
		}

		public DomainController RidRoleOwner
		{
			get
			{
				base.CheckIfDisposed();
				if (this.cachedRidRoleOwner == null)
				{
					this.cachedRidRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.RidRole);
				}
				return this.cachedRidRoleOwner;
			}
		}

		internal Domain(DirectoryContext context, string domainName, DirectoryEntryManager directoryEntryMgr) : base(context, domainName)
		{
			this.currentDomainMode = DomainMode.Windows2000NativeDomain | DomainMode.Windows2003InterimDomain | DomainMode.Windows2003Domain | DomainMode.Windows2008Domain | DomainMode.Windows2008R2Domain | DomainMode.Windows8Domain;
			this.directoryEntryMgr = directoryEntryMgr;
		}

		internal Domain(DirectoryContext context, string domainName) : this(context, domainName, new DirectoryEntryManager(context))
		{
		}

		public void CreateLocalSideOfTrustRelationship(string targetDomainName, TrustDirection direction, string trustPassword)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
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
								Locator.GetDomainControllerInfo(null, targetDomainName, null, (long)16);
								DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(targetDomainName, DirectoryContextType.Domain, this.context);
								TrustHelper.CreateTrust(this.context, base.Name, newDirectoryContext, targetDomainName, false, direction, trustPassword);
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
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void CreateTrustRelationship(Domain targetDomain, TrustDirection direction)
		{
			base.CheckIfDisposed();
			if (targetDomain != null)
			{
				if (direction < TrustDirection.Inbound || direction > TrustDirection.Bidirectional)
				{
					throw new InvalidEnumArgumentException("direction", (int)direction, typeof(TrustDirection));
				}
				else
				{
					string str = TrustHelper.CreateTrustPassword();
					TrustHelper.CreateTrust(this.context, base.Name, targetDomain.GetDirectoryContext(), targetDomain.Name, false, direction, str);
					int num = 0;
					if ((direction & TrustDirection.Inbound) != 0)
					{
						num = num | 2;
					}
					if ((direction & TrustDirection.Outbound) != 0)
					{
						num = num | 1;
					}
					TrustHelper.CreateTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, this.context, base.Name, false, (TrustDirection)num, str);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomain");
			}
		}

		public void DeleteLocalSideOfTrustRelationship(string targetDomainName)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					TrustHelper.DeleteTrust(this.context, base.Name, targetDomainName, false);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void DeleteTrustRelationship(Domain targetDomain)
		{
			base.CheckIfDisposed();
			if (targetDomain != null)
			{
				TrustHelper.DeleteTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false);
				TrustHelper.DeleteTrust(this.context, base.Name, targetDomain.Name, false);
				return;
			}
			else
			{
				throw new ArgumentNullException("targetDomain");
			}
		}

		public DomainControllerCollection FindAllDiscoverableDomainControllers()
		{
			long num = (long)0x1000;
			base.CheckIfDisposed();
			return new DomainControllerCollection(Locator.EnumerateDomainControllers(this.context, base.Name, null, num));
		}

		public DomainControllerCollection FindAllDiscoverableDomainControllers(string siteName)
		{
			long num = (long)0x1000;
			base.CheckIfDisposed();
			if (siteName != null)
			{
				if (siteName.Length != 0)
				{
					return new DomainControllerCollection(Locator.EnumerateDomainControllers(this.context, base.Name, siteName, num));
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

		public DomainControllerCollection FindAllDomainControllers()
		{
			base.CheckIfDisposed();
			return DomainController.FindAllInternal(this.context, base.Name, true, null);
		}

		public DomainControllerCollection FindAllDomainControllers(string siteName)
		{
			base.CheckIfDisposed();
			if (siteName != null)
			{
				return DomainController.FindAllInternal(this.context, base.Name, true, siteName);
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public DomainController FindDomainController()
		{
			base.CheckIfDisposed();
			return DomainController.FindOneInternal(this.context, base.Name, null, (LocatorOptions)((long)0));
		}

		public DomainController FindDomainController(string siteName)
		{
			base.CheckIfDisposed();
			if (siteName != null)
			{
				return DomainController.FindOneInternal(this.context, base.Name, siteName, (LocatorOptions)((long)0));
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public DomainController FindDomainController(LocatorOptions flag)
		{
			base.CheckIfDisposed();
			return DomainController.FindOneInternal(this.context, base.Name, null, flag);
		}

		public DomainController FindDomainController(string siteName, LocatorOptions flag)
		{
			base.CheckIfDisposed();
			if (siteName != null)
			{
				return DomainController.FindOneInternal(this.context, base.Name, siteName, flag);
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public TrustRelationshipInformationCollection GetAllTrustRelationships()
		{
			base.CheckIfDisposed();
			ArrayList trustsHelper = this.GetTrustsHelper(null);
			TrustRelationshipInformationCollection trustRelationshipInformationCollection = new TrustRelationshipInformationCollection(this.context, base.Name, trustsHelper);
			return trustRelationshipInformationCollection;
		}

		private ArrayList GetChildDomains()
		{
			ArrayList arrayLists = new ArrayList();
			if (this.crossRefDN == null)
			{
				this.LoadCrossRefAttributes();
			}
			DirectoryEntry directoryEntry = null;
			SearchResultCollection searchResultCollections = null;
			try
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
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
					stringBuilder.Append(")(");
					stringBuilder.Append(PropertyManager.TrustParent);
					stringBuilder.Append("=");
					stringBuilder.Append(Utils.GetEscapedFilterValue(this.crossRefDN));
					stringBuilder.Append("))");
					string str = stringBuilder.ToString();
					string[] dnsRoot = new string[1];
					dnsRoot[0] = PropertyManager.DnsRoot;
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, str, dnsRoot, SearchScope.OneLevel);
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
				if (directoryEntry != null)
				{
					directoryEntry.Dispose();
				}
			}
			return arrayLists;
		}

		public static Domain GetComputerDomain()
		{
			string dnsDomainName = DirectoryContext.GetDnsDomainName(null);
			if (dnsDomainName != null)
			{
				return Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, dnsDomainName));
			}
			else
			{
				throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ComputerNotJoinedToDomain"), typeof(Domain), null);
			}
		}

		public static Domain GetCurrentDomain()
		{
			return Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain));
		}

		internal DirectoryContext GetDirectoryContext()
		{
			return this.context;
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override DirectoryEntry GetDirectoryEntry()
		{
			base.CheckIfDisposed();
			return DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
		}

		public static Domain GetDomain(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.Domain || context.ContextType == DirectoryContextType.DirectoryServer)
				{
					if (context.Name != null || context.isDomain())
					{
						if (context.Name == null || context.isDomain() || context.isServer())
						{
							context = new DirectoryContext(context);
							DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
							string propertyValue = null;
							try
							{
								DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
								if (!context.isServer() || Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectory))
								{
									propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DefaultNamingContext);
								}
								else
								{
									object[] name = new object[1];
									name[0] = context.Name;
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", name), typeof(Domain), null);
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
									if (context.ContextType != DirectoryContextType.Domain)
									{
										object[] objArray = new object[1];
										objArray[0] = context.Name;
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", objArray), typeof(Domain), null);
									}
									else
									{
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainNotFound"), typeof(Domain), context.Name);
									}
								}
							}
							return new Domain(context, Utils.GetDnsNameFromDN(propertyValue), directoryEntryManager);
						}
						else
						{
							if (context.ContextType != DirectoryContextType.Domain)
							{
								object[] name1 = new object[1];
								name1[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", name1), typeof(Domain), null);
							}
							else
							{
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainNotFound"), typeof(Domain), context.Name);
							}
						}
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(Domain), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeServerORDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private DomainMode GetDomainMode()
		{
			DomainMode domainMode;
			DirectoryEntry directoryEntry = null;
			DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
			int num = 0;
			try
			{
				try
				{
					if (directoryEntry1.Properties.Contains(PropertyManager.DomainFunctionality))
					{
						num = int.Parse((string)PropertyManager.GetPropertyValue(this.context, directoryEntry1, PropertyManager.DomainFunctionality), NumberFormatInfo.InvariantInfo);
					}
					int num1 = num;
					if (num1 == 0)
					{
						directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
						int propertyValue = (int)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.NTMixedDomain);
						if (propertyValue != 0)
						{
							domainMode = DomainMode.Windows2000MixedDomain;
						}
						else
						{
							domainMode = DomainMode.Windows2000NativeDomain;
						}
					}
					else if (num1 == 1)
					{
						domainMode = DomainMode.Windows2003InterimDomain;
					}
					else if (num1 == 2)
					{
						domainMode = DomainMode.Windows2003Domain;
					}
					else if (num1 == 3)
					{
						domainMode = DomainMode.Windows2008Domain;
					}
					else if (num1 == 4)
					{
						domainMode = DomainMode.Windows2008R2Domain;
					}
					else if (num1 == 5)
					{
						domainMode = DomainMode.Windows8Domain;
					}
					else
					{
						throw new ActiveDirectoryOperationException(Res.GetString("InvalidMode"));
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
				directoryEntry1.Dispose();
				if (directoryEntry != null)
				{
					directoryEntry.Dispose();
				}
			}
			return domainMode;
		}

		private Domain GetParent()
		{
			if (this.crossRefDN == null)
			{
				this.LoadCrossRefAttributes();
			}
			if (this.trustParent == null)
			{
				return null;
			}
			else
			{
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.trustParent);
				string propertyValue = null;
				DirectoryContext newDirectoryContext = null;
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.DnsRoot);
					newDirectoryContext = Utils.GetNewDirectoryContext(propertyValue, DirectoryContextType.Domain, this.context);
				}
				finally
				{
					directoryEntry.Dispose();
				}
				return new Domain(newDirectoryContext, propertyValue);
			}
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
						case ActiveDirectoryRole.PdcRole:
						{
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
							break;
						}
						case ActiveDirectoryRole.RidRole:
						{
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RidManager));
							break;
						}
						case ActiveDirectoryRole.InfrastructureRole:
						{
							directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.Infrastructure));
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

		public bool GetSelectiveAuthenticationStatus(string targetDomainName)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					return TrustHelper.GetTrustedDomainInfoStatus(this.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, false);
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public bool GetSidFilteringStatus(string targetDomainName)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					return TrustHelper.GetTrustedDomainInfoStatus(this.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN, false);
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public TrustRelationshipInformation GetTrustRelationship(string targetDomainName)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					ArrayList trustsHelper = this.GetTrustsHelper(targetDomainName);
					TrustRelationshipInformationCollection trustRelationshipInformationCollection = new TrustRelationshipInformationCollection(this.context, base.Name, trustsHelper);
					if (trustRelationshipInformationCollection.Count != 0)
					{
						return trustRelationshipInformationCollection[0];
					}
					else
					{
						object[] name = new object[2];
						name[0] = base.Name;
						name[1] = targetDomainName;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", name), typeof(TrustRelationshipInformation), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		private ArrayList GetTrustsHelper(string targetDomainName)
		{
			string name;
			ArrayList arrayLists;
			IntPtr intPtr = (IntPtr)0;
			int num = 0;
			ArrayList arrayLists1 = new ArrayList();
			ArrayList arrayLists2 = new ArrayList();
			new TrustRelationshipInformationCollection();
			int num1 = 0;
			string stringUni = null;
			int num2 = 0;
			if (!this.context.isServer())
			{
				name = DomainController.FindOne(this.context).Name;
			}
			else
			{
				name = this.context.Name;
			}
			bool flag = Utils.Impersonate(this.context);
			try
			{
				try
				{
					num2 = UnsafeNativeMethods.DsEnumerateDomainTrustsW(name, 35, out intPtr, out num);
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
			if (num2 == 0)
			{
				try
				{
					if (intPtr != (IntPtr)0 && num != 0)
					{
						int num3 = 0;
						for (int i = 0; i < num; i++)
						{
							IntPtr intPtr1 = (IntPtr)((long)intPtr + (long)(i * Marshal.SizeOf(typeof(DS_DOMAIN_TRUSTS))));
							DS_DOMAIN_TRUSTS dSDOMAINTRUST = new DS_DOMAIN_TRUSTS();
							Marshal.PtrToStructure(intPtr1, dSDOMAINTRUST);
							arrayLists1.Add(dSDOMAINTRUST);
						}
						for (int j = 0; j < arrayLists1.Count; j++)
						{
							DS_DOMAIN_TRUSTS item = (DS_DOMAIN_TRUSTS)arrayLists1[j];
							if ((item.Flags & 42) != 0 && item.TrustType != TrustHelper.TRUST_TYPE_DOWNLEVEL)
							{
								TrustObject trustObject = new TrustObject();
								trustObject.TrustType = TrustType.Unknown;
								if (item.DnsDomainName != (IntPtr)0)
								{
									trustObject.DnsDomainName = Marshal.PtrToStringUni(item.DnsDomainName);
								}
								if (item.NetbiosDomainName != (IntPtr)0)
								{
									trustObject.NetbiosDomainName = Marshal.PtrToStringUni(item.NetbiosDomainName);
								}
								trustObject.Flags = item.Flags;
								trustObject.TrustAttributes = item.TrustAttributes;
								trustObject.OriginalIndex = j;
								trustObject.ParentIndex = item.ParentIndex;
								if (targetDomainName != null)
								{
									bool flag1 = false;
									if (trustObject.DnsDomainName == null || Utils.Compare(targetDomainName, trustObject.DnsDomainName) != 0)
									{
										if (trustObject.NetbiosDomainName != null && Utils.Compare(targetDomainName, trustObject.NetbiosDomainName) == 0)
										{
											flag1 = true;
										}
									}
									else
									{
										flag1 = true;
									}
									if (!flag1 && (trustObject.Flags & 8) == 0)
									{
										goto Label0;
									}
								}
								if ((trustObject.Flags & 8) == 0)
								{
									if (item.TrustType == 3)
									{
										trustObject.TrustType = TrustType.Kerberos;
									}
								}
								else
								{
									num1 = num3;
									if ((trustObject.Flags & 4) == 0)
									{
										DS_DOMAIN_TRUSTS item1 = (DS_DOMAIN_TRUSTS)arrayLists1[trustObject.ParentIndex];
										if (item1.DnsDomainName != (IntPtr)0)
										{
											stringUni = Marshal.PtrToStringUni(item1.DnsDomainName);
										}
									}
									trustObject.TrustType = TrustType.ParentChild | TrustType.CrossLink | TrustType.External | TrustType.Forest | TrustType.Kerberos | TrustType.Unknown;
								}
								num3++;
								arrayLists2.Add(trustObject);
							}
						Label0:
							continue;
						}
						for (int k = 0; k < arrayLists2.Count; k++)
						{
							TrustObject trustObject1 = (TrustObject)arrayLists2[k];
							if (k != num1 && trustObject1.TrustType != TrustType.Kerberos)
							{
								if (stringUni == null || Utils.Compare(stringUni, trustObject1.DnsDomainName) != 0)
								{
									if ((trustObject1.Flags & 1) == 0)
									{
										if ((trustObject1.TrustAttributes & 8) == 0)
										{
											trustObject1.TrustType = TrustType.External;
										}
										else
										{
											trustObject1.TrustType = TrustType.Forest;
										}
									}
									else
									{
										if (trustObject1.ParentIndex != ((TrustObject)arrayLists2[num1]).OriginalIndex)
										{
											if ((trustObject1.Flags & 4) == 0 || (((TrustObject)arrayLists2[num1]).Flags & 4) == 0)
											{
												trustObject1.TrustType = TrustType.CrossLink;
											}
											else
											{
												string str = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RootDomainNamingContext);
												string dnsNameFromDN = Utils.GetDnsNameFromDN(str);
												DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(this.context.Name, DirectoryContextType.Forest, this.context);
												if (newDirectoryContext.isRootDomain() || Utils.Compare(trustObject1.DnsDomainName, dnsNameFromDN) == 0)
												{
													trustObject1.TrustType = TrustType.TreeRoot;
												}
												else
												{
													trustObject1.TrustType = TrustType.CrossLink;
												}
											}
										}
										else
										{
											trustObject1.TrustType = TrustType.ParentChild;
										}
									}
								}
								else
								{
									trustObject1.TrustType = TrustType.ParentChild;
								}
							}
						}
					}
					arrayLists = arrayLists2;
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.NetApiBufferFree(intPtr);
					}
				}
				return arrayLists;
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(num2, name);
			}
		}

		private void LoadCrossRefAttributes()
		{
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
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
					stringBuilder.Append(")(");
					stringBuilder.Append(PropertyManager.DnsRoot);
					stringBuilder.Append("=");
					stringBuilder.Append(Utils.GetEscapedFilterValue(this.partitionName));
					stringBuilder.Append("))");
					string str = stringBuilder.ToString();
					string[] distinguishedName = new string[2];
					distinguishedName[0] = PropertyManager.DistinguishedName;
					distinguishedName[1] = PropertyManager.TrustParent;
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, str, distinguishedName, SearchScope.OneLevel, false, false);
					SearchResult searchResult = aDSearcher.FindOne();
					this.crossRefDN = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DistinguishedName);
					if (searchResult.Properties[PropertyManager.TrustParent].Count > 0)
					{
						this.trustParent = (string)searchResult.Properties[PropertyManager.TrustParent][0];
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
		}

		public void RaiseDomainFunctionality(DomainMode domainMode)
		{
			base.CheckIfDisposed();
			if (domainMode < DomainMode.Windows2000MixedDomain || domainMode > DomainMode.Windows8Domain)
			{
				throw new InvalidEnumArgumentException("domainMode", (int)domainMode, typeof(DomainMode));
			}
			else
			{
				DomainMode domainMode1 = this.GetDomainMode();
				DirectoryEntry directoryEntry = null;
				using (directoryEntry)
				{
					try
					{
						directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
						DomainMode domainMode2 = domainMode1;
						if (domainMode2 == DomainMode.Windows2000MixedDomain)
						{
							if (domainMode != DomainMode.Windows2000NativeDomain)
							{
								if (domainMode != DomainMode.Windows2003InterimDomain)
								{
									if (domainMode != DomainMode.Windows2003Domain)
									{
										throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
									}
									else
									{
										directoryEntry.Properties[PropertyManager.NTMixedDomain].Value = 0;
										directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 2;
									}
								}
								else
								{
									directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 1;
								}
							}
							else
							{
								directoryEntry.Properties[PropertyManager.NTMixedDomain].Value = 0;
							}
						}
						else if (domainMode2 == DomainMode.Windows2000NativeDomain)
						{
							if (domainMode != DomainMode.Windows2003Domain)
							{
								if (domainMode != DomainMode.Windows2008Domain)
								{
									if (domainMode != DomainMode.Windows2008R2Domain)
									{
										if (domainMode != DomainMode.Windows8Domain)
										{
											throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
										}
										else
										{
											directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 5;
										}
									}
									else
									{
										directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 4;
									}
								}
								else
								{
									directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 3;
								}
							}
							else
							{
								directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 2;
							}
						}
						else if (domainMode2 == DomainMode.Windows2003InterimDomain)
						{
							if (domainMode != DomainMode.Windows2003Domain)
							{
								throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
							}
							else
							{
								directoryEntry.Properties[PropertyManager.NTMixedDomain].Value = 0;
								directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 2;
							}
						}
						else if (domainMode2 == DomainMode.Windows2003Domain)
						{
							if (domainMode != DomainMode.Windows2008Domain)
							{
								if (domainMode != DomainMode.Windows2008R2Domain)
								{
									if (domainMode != DomainMode.Windows8Domain)
									{
										throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
									}
									else
									{
										directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 5;
									}
								}
								else
								{
									directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 4;
								}
							}
							else
							{
								directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 3;
							}
						}
						else if (domainMode2 == DomainMode.Windows2008Domain)
						{
							if (domainMode != DomainMode.Windows2008R2Domain)
							{
								if (domainMode != DomainMode.Windows8Domain)
								{
									throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
								}
								else
								{
									directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 5;
								}
							}
							else
							{
								directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 4;
							}
						}
						else if (domainMode2 == DomainMode.Windows2008R2Domain)
						{
							if (domainMode != DomainMode.Windows8Domain)
							{
								throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
							}
							else
							{
								directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 5;
							}
						}
						else if (domainMode2 == DomainMode.Windows8Domain)
						{
							throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
						}
						else
						{
							throw new ActiveDirectoryOperationException();
						}
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
							throw new ArgumentException(Res.GetString("NoW2K3DCs"), "domainMode");
						}
					}
				}
				this.currentDomainMode = DomainMode.Windows2000NativeDomain | DomainMode.Windows2003InterimDomain | DomainMode.Windows2003Domain | DomainMode.Windows2008Domain | DomainMode.Windows2008R2Domain | DomainMode.Windows8Domain;
				return;
			}
		}

		private void RepairTrustHelper(Domain targetDomain, TrustDirection direction)
		{
			string str = TrustHelper.CreateTrustPassword();
			string str1 = TrustHelper.UpdateTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, str, false);
			string str2 = TrustHelper.UpdateTrust(this.context, base.Name, targetDomain.Name, str, false);
			if ((direction & TrustDirection.Outbound) != 0)
			{
				try
				{
					TrustHelper.VerifyTrust(this.context, base.Name, targetDomain.Name, false, TrustDirection.Outbound, true, str1);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] name = new object[3];
					name[0] = base.Name;
					name[1] = targetDomain.Name;
					name[2] = direction;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", name), typeof(TrustRelationshipInformation), null);
				}
			}
			if ((direction & TrustDirection.Inbound) != 0)
			{
				try
				{
					TrustHelper.VerifyTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false, TrustDirection.Outbound, true, str2);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException1)
				{
					object[] objArray = new object[3];
					objArray[0] = base.Name;
					objArray[1] = targetDomain.Name;
					objArray[2] = direction;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", objArray), typeof(TrustRelationshipInformation), null);
				}
			}
		}

		public void RepairTrustRelationship(Domain targetDomain)
		{
			TrustDirection trustDirection = TrustDirection.Bidirectional;
			base.CheckIfDisposed();
			if (targetDomain != null)
			{
				try
				{
					trustDirection = this.GetTrustRelationship(targetDomain.Name).TrustDirection;
					if ((trustDirection & TrustDirection.Outbound) != 0)
					{
						TrustHelper.VerifyTrust(this.context, base.Name, targetDomain.Name, false, TrustDirection.Outbound, true, null);
					}
					if ((trustDirection & TrustDirection.Inbound) != 0)
					{
						TrustHelper.VerifyTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false, TrustDirection.Outbound, true, null);
					}
				}
				catch (ActiveDirectoryOperationException activeDirectoryOperationException)
				{
					this.RepairTrustHelper(targetDomain, trustDirection);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException)
				{
					this.RepairTrustHelper(targetDomain, trustDirection);
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
				{
					object[] name = new object[3];
					name[0] = base.Name;
					name[1] = targetDomain.Name;
					name[2] = trustDirection;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", name), typeof(TrustRelationshipInformation), null);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("targetDomain");
			}
		}

		public void SetSelectiveAuthenticationStatus(string targetDomainName, bool enable)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					TrustHelper.SetTrustedDomainInfoStatus(this.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, enable, false);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void SetSidFilteringStatus(string targetDomainName, bool enable)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					TrustHelper.SetTrustedDomainInfoStatus(this.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN, enable, false);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void UpdateLocalSideOfTrustRelationship(string targetDomainName, string newTrustPassword)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					if (newTrustPassword != null)
					{
						if (newTrustPassword.Length != 0)
						{
							TrustHelper.UpdateTrust(this.context, base.Name, targetDomainName, newTrustPassword, false);
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
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void UpdateLocalSideOfTrustRelationship(string targetDomainName, TrustDirection newTrustDirection, string newTrustPassword)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
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
								TrustHelper.UpdateTrustDirection(this.context, base.Name, targetDomainName, newTrustPassword, false, newTrustDirection);
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
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void UpdateTrustRelationship(Domain targetDomain, TrustDirection newTrustDirection)
		{
			base.CheckIfDisposed();
			if (targetDomain != null)
			{
				if (newTrustDirection < TrustDirection.Inbound || newTrustDirection > TrustDirection.Bidirectional)
				{
					throw new InvalidEnumArgumentException("newTrustDirection", (int)newTrustDirection, typeof(TrustDirection));
				}
				else
				{
					string str = TrustHelper.CreateTrustPassword();
					TrustHelper.UpdateTrustDirection(this.context, base.Name, targetDomain.Name, str, false, newTrustDirection);
					TrustDirection trustDirection = 0;
					if ((newTrustDirection & TrustDirection.Inbound) != 0)
					{
						trustDirection = trustDirection | TrustDirection.Outbound;
					}
					if ((newTrustDirection & TrustDirection.Outbound) != 0)
					{
						trustDirection = trustDirection | TrustDirection.Inbound;
					}
					TrustHelper.UpdateTrustDirection(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, str, false, trustDirection);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomain");
			}
		}

		public void VerifyOutboundTrustRelationship(string targetDomainName)
		{
			base.CheckIfDisposed();
			if (targetDomainName != null)
			{
				if (targetDomainName.Length != 0)
				{
					TrustHelper.VerifyTrust(this.context, base.Name, targetDomainName, false, TrustDirection.Outbound, false, null);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomainName");
			}
		}

		public void VerifyTrustRelationship(Domain targetDomain, TrustDirection direction)
		{
			base.CheckIfDisposed();
			if (targetDomain != null)
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
							TrustHelper.VerifyTrust(this.context, base.Name, targetDomain.Name, false, TrustDirection.Outbound, false, null);
						}
						catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
						{
							object[] name = new object[3];
							name[0] = base.Name;
							name[1] = targetDomain.Name;
							name[2] = direction;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", name), typeof(TrustRelationshipInformation), null);
						}
					}
					if ((direction & TrustDirection.Inbound) != 0)
					{
						try
						{
							TrustHelper.VerifyTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false, TrustDirection.Outbound, false, null);
						}
						catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException1)
						{
							object[] objArray = new object[3];
							objArray[0] = base.Name;
							objArray[1] = targetDomain.Name;
							objArray[2] = direction;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", objArray), typeof(TrustRelationshipInformation), null);
						}
					}
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("targetDomain");
			}
		}
	}
}