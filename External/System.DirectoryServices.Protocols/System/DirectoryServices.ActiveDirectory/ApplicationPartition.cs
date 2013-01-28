using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ApplicationPartition : ActiveDirectoryPartition
	{
		private bool disposed;

		private ApplicationPartitionType appType;

		private bool committed;

		private DirectoryEntry domainDNSEntry;

		private DirectoryEntry crossRefEntry;

		private string dnsName;

		private DirectoryServerCollection cachedDirectoryServers;

		private bool securityRefDomainModified;

		private string securityRefDomain;

		public DirectoryServerCollection DirectoryServers
		{
			get
			{
				ReadOnlyDirectoryServerCollection readOnlyDirectoryServerCollection;
				bool flag;
				DirectoryEntry directoryEntry;
				base.CheckIfDisposed();
				if (this.cachedDirectoryServers == null)
				{
					if (this.committed)
					{
						readOnlyDirectoryServerCollection = this.FindAllDirectoryServers();
					}
					else
					{
						readOnlyDirectoryServerCollection = new ReadOnlyDirectoryServerCollection();
					}
					ReadOnlyDirectoryServerCollection readOnlyDirectoryServerCollection1 = readOnlyDirectoryServerCollection;
					if (this.appType == ApplicationPartitionType.ADAMApplicationPartition)
					{
						flag = true;
					}
					else
					{
						flag = false;
					}
					bool flag1 = flag;
					if (this.committed)
					{
						this.GetCrossRefEntry();
					}
					ApplicationPartition directoryServerCollection = this;
					DirectoryContext directoryContext = this.context;
					if (this.committed)
					{
						directoryEntry = this.crossRefEntry;
					}
					else
					{
						directoryEntry = null;
					}
					directoryServerCollection.cachedDirectoryServers = new DirectoryServerCollection(directoryContext, directoryEntry, flag1, readOnlyDirectoryServerCollection1);
				}
				return this.cachedDirectoryServers;
			}
		}

		public string SecurityReferenceDomain
		{
			get
			{
				string value;
				base.CheckIfDisposed();
				if (this.appType != ApplicationPartitionType.ADAMApplicationPartition)
				{
					if (!this.committed)
					{
						return this.securityRefDomain;
					}
					else
					{
						this.GetCrossRefEntry();
						try
						{
							if (this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Count <= 0)
							{
								value = null;
							}
							else
							{
								value = (string)this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Value;
							}
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						return value;
					}
				}
				else
				{
					throw new NotSupportedException(Res.GetString("PropertyInvalidForADAM"));
				}
			}
			set
			{
				base.CheckIfDisposed();
				if (this.appType != ApplicationPartitionType.ADAMApplicationPartition)
				{
					if (!this.committed)
					{
						if (this.securityRefDomain != null || value != null)
						{
							this.securityRefDomain = value;
							this.securityRefDomainModified = true;
						}
					}
					else
					{
						this.GetCrossRefEntry();
						if (value != null)
						{
							this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Value = value;
							this.securityRefDomainModified = true;
							return;
						}
						else
						{
							if (this.crossRefEntry.Properties.Contains(PropertyManager.MsDSSDReferenceDomain))
							{
								this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Clear();
								this.securityRefDomainModified = true;
								return;
							}
						}
					}
					return;
				}
				else
				{
					throw new NotSupportedException(Res.GetString("PropertyInvalidForADAM"));
				}
			}
		}

		public ApplicationPartition(DirectoryContext context, string distinguishedName)
		{
			this.appType = ApplicationPartitionType.Unknown;
			this.committed = true;
			this.ValidateApplicationPartitionParameters(context, distinguishedName, null, false);
			this.CreateApplicationPartition(distinguishedName, "domainDns");
		}

		public ApplicationPartition(DirectoryContext context, string distinguishedName, string objectClass)
		{
			this.appType = ApplicationPartitionType.Unknown;
			this.committed = true;
			this.ValidateApplicationPartitionParameters(context, distinguishedName, objectClass, true);
			this.CreateApplicationPartition(distinguishedName, objectClass);
		}

		internal ApplicationPartition(DirectoryContext context, string distinguishedName, string dnsName, ApplicationPartitionType appType, DirectoryEntryManager directoryEntryMgr) : base(context, distinguishedName)
		{
			this.appType = ApplicationPartitionType.Unknown;
			this.committed = true;
			this.directoryEntryMgr = directoryEntryMgr;
			this.appType = appType;
			this.dnsName = dnsName;
		}

		internal ApplicationPartition(DirectoryContext context, string distinguishedName, string dnsName, DirectoryEntryManager directoryEntryMgr) : this(context, distinguishedName, dnsName, ApplicationPartition.GetApplicationPartitionType(context), directoryEntryMgr)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		private void CreateApplicationPartition(string distinguishedName, string objectClass)
		{
			if (this.appType != ApplicationPartitionType.ADApplicationPartition)
			{
				try
				{
					this.InitializeCrossRef(distinguishedName);
					DirectoryEntry directoryEntry = null;
					DirectoryEntry parent = null;
					try
					{
						AuthenticationTypes defaultAuthType = Utils.DefaultAuthType | AuthenticationTypes.FastBind;
						if (DirectoryContext.ServerBindSupported)
						{
							defaultAuthType = defaultAuthType | AuthenticationTypes.ServerBind;
						}
						directoryEntry = new DirectoryEntry(string.Concat("LDAP://", this.context.Name, "/", distinguishedName), this.context.UserName, this.context.Password, defaultAuthType);
						parent = directoryEntry.Parent;
						this.domainDNSEntry = parent.Children.Add(Utils.GetRdnFromDN(distinguishedName), objectClass);
						this.domainDNSEntry.Properties[PropertyManager.InstanceType].Value = NCFlags.InstanceTypeIsNCHead | NCFlags.InstanceTypeIsWriteable;
						this.committed = false;
					}
					finally
					{
						if (parent != null)
						{
							parent.Dispose();
						}
						if (directoryEntry != null)
						{
							directoryEntry.Dispose();
						}
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			else
			{
				DirectoryEntry directoryEntry1 = null;
				DirectoryEntry parent1 = null;
				try
				{
					try
					{
						AuthenticationTypes authenticationType = Utils.DefaultAuthType | AuthenticationTypes.FastBind | AuthenticationTypes.Delegation;
						if (DirectoryContext.ServerBindSupported)
						{
							authenticationType = authenticationType | AuthenticationTypes.ServerBind;
						}
						directoryEntry1 = new DirectoryEntry(string.Concat("LDAP://", this.context.GetServerName(), "/", distinguishedName), this.context.UserName, this.context.Password, authenticationType);
						parent1 = directoryEntry1.Parent;
						this.domainDNSEntry = parent1.Children.Add(Utils.GetRdnFromDN(distinguishedName), PropertyManager.DomainDNS);
						this.domainDNSEntry.Properties[PropertyManager.InstanceType].Value = NCFlags.InstanceTypeIsNCHead | NCFlags.InstanceTypeIsWriteable;
						this.committed = false;
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
					}
				}
				finally
				{
					if (parent1 != null)
					{
						parent1.Dispose();
					}
					if (directoryEntry1 != null)
					{
						directoryEntry1.Dispose();
					}
				}
			}
		}

		public void Delete()
		{
			base.CheckIfDisposed();
			if (this.committed)
			{
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
				try
				{
					try
					{
						this.GetCrossRefEntry();
						directoryEntry.Children.Remove(this.crossRefEntry);
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
				return;
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				try
				{
					if (this.crossRefEntry != null)
					{
						this.crossRefEntry.Dispose();
						this.crossRefEntry = null;
					}
					if (this.domainDNSEntry != null)
					{
						this.domainDNSEntry.Dispose();
						this.domainDNSEntry = null;
					}
					this.disposed = true;
				}
				finally
				{
					base.Dispose();
				}
			}
		}

		public ReadOnlyDirectoryServerCollection FindAllDirectoryServers()
		{
			base.CheckIfDisposed();
			if (this.appType != ApplicationPartitionType.ADApplicationPartition)
			{
				if (this.committed)
				{
					ReadOnlyDirectoryServerCollection readOnlyDirectoryServerCollection = new ReadOnlyDirectoryServerCollection();
					readOnlyDirectoryServerCollection.AddRange(ConfigurationSet.FindAdamInstances(this.context, base.Name, null));
					return readOnlyDirectoryServerCollection;
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
				}
			}
			else
			{
				return this.FindAllDirectoryServersInternal(null);
			}
		}

		public ReadOnlyDirectoryServerCollection FindAllDirectoryServers(string siteName)
		{
			base.CheckIfDisposed();
			if (siteName != null)
			{
				if (this.appType != ApplicationPartitionType.ADApplicationPartition)
				{
					if (this.committed)
					{
						ReadOnlyDirectoryServerCollection readOnlyDirectoryServerCollection = new ReadOnlyDirectoryServerCollection();
						readOnlyDirectoryServerCollection.AddRange(ConfigurationSet.FindAdamInstances(this.context, base.Name, siteName));
						return readOnlyDirectoryServerCollection;
					}
					else
					{
						throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
					}
				}
				else
				{
					return this.FindAllDirectoryServersInternal(siteName);
				}
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		private ReadOnlyDirectoryServerCollection FindAllDirectoryServersInternal(string siteName)
		{
			if (siteName == null || siteName.Length != 0)
			{
				if (this.committed)
				{
					ArrayList arrayLists = new ArrayList();
					foreach (string replicaList in Utils.GetReplicaList(this.context, base.Name, siteName, false, false, false))
					{
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(replicaList, DirectoryContextType.DirectoryServer, this.context);
						arrayLists.Add(new DomainController(newDirectoryContext, replicaList));
					}
					return new ReadOnlyDirectoryServerCollection(arrayLists);
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		public ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServers()
		{
			base.CheckIfDisposed();
			if (this.appType != ApplicationPartitionType.ADApplicationPartition)
			{
				throw new NotSupportedException(Res.GetString("OperationInvalidForADAM"));
			}
			else
			{
				return this.FindAllDiscoverableDirectoryServersInternal(null);
			}
		}

		public ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServers(string siteName)
		{
			base.CheckIfDisposed();
			if (siteName != null)
			{
				if (this.appType != ApplicationPartitionType.ADApplicationPartition)
				{
					throw new NotSupportedException(Res.GetString("OperationInvalidForADAM"));
				}
				else
				{
					return this.FindAllDiscoverableDirectoryServersInternal(siteName);
				}
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		private ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServersInternal(string siteName)
		{
			if (siteName == null || siteName.Length != 0)
			{
				if (this.committed)
				{
					long num = (long)0x8000;
					return new ReadOnlyDirectoryServerCollection(Locator.EnumerateDomainControllers(this.context, this.dnsName, siteName, num));
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		public static ApplicationPartition FindByName(DirectoryContext context, string distinguishedName)
		{
			DomainControllerInfo domainControllerInfo = null;
			string item;
			DirectoryContext newDirectoryContext = null;
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isADAMConfigSet() || context.isServer())
					{
						if (distinguishedName != null)
						{
							if (distinguishedName.Length != 0)
							{
								if (Utils.IsValidDNFormat(distinguishedName))
								{
									context = new DirectoryContext(context);
									DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
									DirectoryEntry directoryEntry = null;
									try
									{
										directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, directoryEntryManager.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
									}
									catch (COMException cOMException1)
									{
										COMException cOMException = cOMException1;
										throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
									}
									catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
									{
										object[] name = new object[1];
										name[0] = context.Name;
										throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", name));
									}
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
									stringBuilder.Append("))(");
									stringBuilder.Append(PropertyManager.NCName);
									stringBuilder.Append("=");
									stringBuilder.Append(Utils.GetEscapedFilterValue(distinguishedName));
									stringBuilder.Append("))");
									string str = stringBuilder.ToString();
									string[] dnsRoot = new string[2];
									dnsRoot[0] = PropertyManager.DnsRoot;
									dnsRoot[1] = PropertyManager.NCName;
									ADSearcher aDSearcher = new ADSearcher(directoryEntry, str, dnsRoot, SearchScope.OneLevel, false, false);
									SearchResult searchResult = null;
									try
									{
										try
										{
											searchResult = aDSearcher.FindOne();
										}
										catch (COMException cOMException3)
										{
											COMException cOMException2 = cOMException3;
											if (cOMException2.ErrorCode != -2147016656)
											{
												throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException2);
											}
											else
											{
												throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
											}
										}
									}
									finally
									{
										directoryEntry.Dispose();
									}
									if (searchResult != null)
									{
										string str1 = null;
										try
										{
											if (searchResult.Properties[PropertyManager.DnsRoot].Count > 0)
											{
												item = (string)searchResult.Properties[PropertyManager.DnsRoot][0];
											}
											else
											{
												item = null;
											}
											str1 = item;
										}
										catch (COMException cOMException5)
										{
											COMException cOMException4 = cOMException5;
											throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException4);
										}
										ApplicationPartitionType applicationPartitionType = ApplicationPartition.GetApplicationPartitionType(context);
										if (context.ContextType != DirectoryContextType.DirectoryServer)
										{
											if (applicationPartitionType != ApplicationPartitionType.ADApplicationPartition)
											{
												string name1 = ConfigurationSet.FindOneAdamInstance(context.Name, context, distinguishedName, null).Name;
												newDirectoryContext = Utils.GetNewDirectoryContext(name1, DirectoryContextType.DirectoryServer, context);
											}
											else
											{
												int num = Locator.DsGetDcNameWrapper(null, str1, null, (long)0x8000, out domainControllerInfo);
												if (num != 0x54b)
												{
													if (num == 0)
													{
														string str2 = domainControllerInfo.DomainControllerName.Substring(2);
														newDirectoryContext = Utils.GetNewDirectoryContext(str2, DirectoryContextType.DirectoryServer, context);
													}
													else
													{
														throw ExceptionHelper.GetExceptionFromErrorCode(num);
													}
												}
												else
												{
													throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
												}
											}
										}
										else
										{
											bool flag = false;
											DistinguishedName distinguishedName1 = new DistinguishedName(distinguishedName);
											DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
											try
											{
												try
												{
													foreach (string item1 in directoryEntry1.Properties[PropertyManager.NamingContexts])
													{
														DistinguishedName distinguishedName2 = new DistinguishedName(item1);
														if (!distinguishedName2.Equals(distinguishedName1))
														{
															continue;
														}
														flag = true;
														break;
													}
												}
												catch (COMException cOMException7)
												{
													COMException cOMException6 = cOMException7;
													throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException6);
												}
											}
											finally
											{
												directoryEntry1.Dispose();
											}
											if (flag)
											{
												newDirectoryContext = context;
											}
											else
											{
												throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
											}
										}
										ApplicationPartition applicationPartition = new ApplicationPartition(newDirectoryContext, (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.NCName), str1, applicationPartitionType, directoryEntryManager);
										return applicationPartition;
									}
									else
									{
										throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
									}
								}
								else
								{
									throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
								}
							}
							else
							{
								throw new ArgumentException(Res.GetString("EmptyStringParameter"), "distinguishedName");
							}
						}
						else
						{
							throw new ArgumentNullException("distinguishedName");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public DirectoryServer FindDirectoryServer()
		{
			DirectoryServer directoryServer = null;
			base.CheckIfDisposed();
			if (this.appType != ApplicationPartitionType.ADApplicationPartition)
			{
				if (this.committed)
				{
					directoryServer = ConfigurationSet.FindOneAdamInstance(this.context, base.Name, null);
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
				}
			}
			else
			{
				directoryServer = this.FindDirectoryServerInternal(null, false);
			}
			return directoryServer;
		}

		public DirectoryServer FindDirectoryServer(string siteName)
		{
			DirectoryServer directoryServer = null;
			base.CheckIfDisposed();
			if (siteName != null)
			{
				if (this.appType != ApplicationPartitionType.ADApplicationPartition)
				{
					if (this.committed)
					{
						directoryServer = ConfigurationSet.FindOneAdamInstance(this.context, base.Name, siteName);
					}
					else
					{
						throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
					}
				}
				else
				{
					directoryServer = this.FindDirectoryServerInternal(siteName, false);
				}
				return directoryServer;
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		public DirectoryServer FindDirectoryServer(bool forceRediscovery)
		{
			DirectoryServer directoryServer = null;
			base.CheckIfDisposed();
			if (this.appType != ApplicationPartitionType.ADApplicationPartition)
			{
				if (this.committed)
				{
					directoryServer = ConfigurationSet.FindOneAdamInstance(this.context, base.Name, null);
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
				}
			}
			else
			{
				directoryServer = this.FindDirectoryServerInternal(null, forceRediscovery);
			}
			return directoryServer;
		}

		public DirectoryServer FindDirectoryServer(string siteName, bool forceRediscovery)
		{
			DirectoryServer directoryServer = null;
			base.CheckIfDisposed();
			if (siteName != null)
			{
				if (this.appType != ApplicationPartitionType.ADApplicationPartition)
				{
					if (this.committed)
					{
						directoryServer = ConfigurationSet.FindOneAdamInstance(this.context, base.Name, siteName);
					}
					else
					{
						throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
					}
				}
				else
				{
					directoryServer = this.FindDirectoryServerInternal(siteName, forceRediscovery);
				}
				return directoryServer;
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		private DirectoryServer FindDirectoryServerInternal(string siteName, bool forceRediscovery)
		{
			DomainControllerInfo domainControllerInfo = null;
			LocatorOptions locatorOption = 0;
			if (siteName == null || siteName.Length != 0)
			{
				if (this.committed)
				{
					if (forceRediscovery)
					{
						locatorOption = LocatorOptions.ForceRediscovery;
					}
					int num = Locator.DsGetDcNameWrapper(null, this.dnsName, siteName, (long)locatorOption | 0x8000, out domainControllerInfo);
					if (num != 0x54b)
					{
						if (num == 0)
						{
							string str = domainControllerInfo.DomainControllerName.Substring(2);
							DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, this.context);
							DirectoryServer domainController = new DomainController(newDirectoryContext, str);
							return domainController;
						}
						else
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(num);
						}
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ReplicaNotFound"), typeof(DirectoryServer), null);
					}
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
			}
		}

		public static ApplicationPartition GetApplicationPartition(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.ApplicationPartition)
				{
					if (context.isNdnc())
					{
						context = new DirectoryContext(context);
						string dNFromDnsName = Utils.GetDNFromDnsName(context.Name);
						DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
						try
						{
							DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(dNFromDnsName);
							//TODO: REVIEW: URGENT: cachedDirectoryEntry.Bind(true);
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
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("NDNCNotFound"), typeof(ApplicationPartition), context.Name);
							}
						}
						return new ApplicationPartition(context, dNFromDnsName, context.Name, ApplicationPartitionType.ADApplicationPartition, directoryEntryManager);
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("NDNCNotFound"), typeof(ApplicationPartition), context.Name);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeAppNCDnsName"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		private static ApplicationPartitionType GetApplicationPartitionType(DirectoryContext context)
		{
			ApplicationPartitionType applicationPartitionType = ApplicationPartitionType.Unknown;
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
			try
			{
				try
				{
					foreach (string item in directoryEntry.Properties[PropertyManager.SupportedCapabilities])
					{
						if (string.Compare(item, SupportedCapability.ADOid, StringComparison.OrdinalIgnoreCase) == 0)
						{
							applicationPartitionType = ApplicationPartitionType.ADApplicationPartition;
						}
						if (string.Compare(item, SupportedCapability.ADAMOid, StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
						applicationPartitionType = ApplicationPartitionType.ADAMApplicationPartition;
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
				}
			}
			finally
			{
				directoryEntry.Dispose();
			}
			if (applicationPartitionType != ApplicationPartitionType.Unknown)
			{
				return applicationPartitionType;
			}
			else
			{
				throw new ActiveDirectoryOperationException(Res.GetString("ApplicationPartitionTypeUnknown"));
			}
		}

		internal DirectoryEntry GetCrossRefEntry()
		{
			if (this.crossRefEntry == null)
			{
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
				try
				{
					this.crossRefEntry = Utils.GetCrossRefEntry(this.context, directoryEntry, base.Name);
				}
				finally
				{
					directoryEntry.Dispose();
				}
				return this.crossRefEntry;
			}
			else
			{
				return this.crossRefEntry;
			}
		}

		[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public override DirectoryEntry GetDirectoryEntry()
		{
			base.CheckIfDisposed();
			if (this.committed)
			{
				return DirectoryEntryManager.GetDirectoryEntry(this.context, base.Name);
			}
			else
			{
				throw new InvalidOperationException(Res.GetString("CannotGetObject"));
			}
		}

		internal string GetNamingRoleOwner()
		{
			string adamDnsHostNameFromNTDSA = null;
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
			try
			{
				if (this.appType != ApplicationPartitionType.ADApplicationPartition)
				{
					adamDnsHostNameFromNTDSA = Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.FsmoRoleOwner));
				}
				else
				{
					adamDnsHostNameFromNTDSA = Utils.GetDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.FsmoRoleOwner));
				}
			}
			finally
			{
				directoryEntry.Dispose();
			}
			return adamDnsHostNameFromNTDSA;
		}

		private void InitializeCrossRef(string distinguishedName)
		{
			string name;
			if (this.crossRefEntry == null)
			{
				DirectoryEntry directoryEntry = null;
				using (directoryEntry)
				{
					try
					{
						string namingRoleOwner = this.GetNamingRoleOwner();
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(namingRoleOwner, DirectoryContextType.DirectoryServer, this.context);
						directoryEntry = DirectoryEntryManager.GetDirectoryEntry(newDirectoryContext, WellKnownDN.PartitionsContainer);
						string str = string.Concat("CN={", Guid.NewGuid(), "}");
						this.crossRefEntry = directoryEntry.Children.Add(str, "crossRef");
						if (this.appType != ApplicationPartitionType.ADAMApplicationPartition)
						{
							name = this.context.Name;
						}
						else
						{
							DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
							string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DsServiceName);
							name = Utils.GetAdamHostNameAndPortsFromNTDSA(this.context, propertyValue);
						}
						this.crossRefEntry.Properties[PropertyManager.DnsRoot].Value = name;
						this.crossRefEntry.Properties[PropertyManager.Enabled].Value = false;
						this.crossRefEntry.Properties[PropertyManager.NCName].Value = distinguishedName;
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		public void Save()
		{
			base.CheckIfDisposed();
			if (this.committed)
			{
				if (this.cachedDirectoryServers != null || this.securityRefDomainModified)
				{
					try
					{
						this.crossRefEntry.CommitChanges();
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
				}
			}
			else
			{
				bool flag = false;
				if (this.appType != ApplicationPartitionType.ADApplicationPartition)
				{
					flag = true;
				}
				else
				{
					try
					{
						this.domainDNSEntry.CommitChanges();
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						if (cOMException2.ErrorCode != -2147016663)
						{
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
						}
						else
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					try
					{
						this.InitializeCrossRef(this.partitionName);
						this.crossRefEntry.CommitChanges();
					}
					catch (COMException cOMException5)
					{
						COMException cOMException4 = cOMException5;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException4);
					}
					try
					{
						this.domainDNSEntry.CommitChanges();
					}
					catch (COMException cOMException9)
					{
						COMException cOMException6 = cOMException9;
						DirectoryEntry parent = this.crossRefEntry.Parent;
						try
						{
							parent.Children.Remove(this.crossRefEntry);
						}
						catch (COMException cOMException8)
						{
							COMException cOMException7 = cOMException8;
							throw ExceptionHelper.GetExceptionFromCOMException(cOMException7);
						}
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException6);
					}
					try
					{
						this.crossRefEntry.RefreshCache();
					}
					catch (COMException cOMException11)
					{
						COMException cOMException10 = cOMException11;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException10);
					}
				}
				DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
				string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DsServiceName);
				if (this.appType == ApplicationPartitionType.ADApplicationPartition)
				{
					this.GetCrossRefEntry();
				}
				string str = (string)PropertyManager.GetPropertyValue(this.context, this.crossRefEntry, PropertyManager.DistinguishedName);
				DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(this.GetNamingRoleOwner(), DirectoryContextType.DirectoryServer, this.context);
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(newDirectoryContext, WellKnownDN.RootDSE);
				try
				{
					try
					{
						directoryEntry.Properties[PropertyManager.ReplicateSingleObject].Value = string.Concat(propertyValue, ":", str);
						directoryEntry.CommitChanges();
					}
					catch (COMException cOMException13)
					{
						COMException cOMException12 = cOMException13;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException12);
					}
				}
				finally
				{
					directoryEntry.Dispose();
				}
				this.committed = true;
				if (this.cachedDirectoryServers != null || this.securityRefDomainModified)
				{
					if (this.cachedDirectoryServers != null)
					{
						this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].AddRange(this.cachedDirectoryServers.GetMultiValuedProperty());
					}
					if (this.securityRefDomainModified)
					{
						this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Value = this.securityRefDomain;
					}
					try
					{
						this.crossRefEntry.CommitChanges();
					}
					catch (COMException cOMException15)
					{
						COMException cOMException14 = cOMException15;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException14);
					}
				}
			}
			this.cachedDirectoryServers = null;
			this.securityRefDomainModified = false;
		}

		private void ValidateApplicationPartitionParameters(DirectoryContext context, string distinguishedName, string objectClass, bool objectClassSpecified)
		{
			if (context != null)
			{
				if (context.Name == null || !context.isServer())
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeServer"), "context");
				}
				else
				{
					if (distinguishedName != null)
					{
						if (distinguishedName.Length != 0)
						{
							this.context = new DirectoryContext(context);
							this.directoryEntryMgr = new DirectoryEntryManager(this.context);
							this.dnsName = Utils.GetDnsNameFromDN(distinguishedName);
							this.partitionName = distinguishedName;
							Component[] dNComponents = Utils.GetDNComponents(distinguishedName);
							if ((int)dNComponents.Length != 1)
							{
								this.appType = ApplicationPartition.GetApplicationPartitionType(this.context);
								if (this.appType != ApplicationPartitionType.ADApplicationPartition || !objectClassSpecified)
								{
									if (objectClassSpecified)
									{
										if (objectClass != null)
										{
											if (objectClass.Length == 0)
											{
												throw new ArgumentException(Res.GetString("EmptyStringParameter"), "objectClass");
											}
										}
										else
										{
											throw new ArgumentNullException("objectClass");
										}
									}
									if (this.appType == ApplicationPartitionType.ADApplicationPartition)
									{
										string propertyValue = null;
										try
										{
											DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
											propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DnsHostName);
										}
										catch (COMException cOMException1)
										{
											COMException cOMException = cOMException1;
											ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
										}
										this.context = Utils.GetNewDirectoryContext(propertyValue, DirectoryContextType.DirectoryServer, context);
									}
									return;
								}
								else
								{
									throw new InvalidOperationException(Res.GetString("NoObjectClassForADPartition"));
								}
							}
							else
							{
								throw new NotSupportedException(Res.GetString("OneLevelPartitionNotSupported"));
							}
						}
						else
						{
							throw new ArgumentException(Res.GetString("EmptyStringParameter"), "distinguishedName");
						}
					}
					else
					{
						throw new ArgumentNullException("distinguishedName");
					}
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}
	}
}