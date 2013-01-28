using System;
using System.Collections;
using System.ComponentModel;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ConfigurationSet
	{
		private DirectoryContext context;

		private DirectoryEntryManager directoryEntryMgr;

		private bool disposed;

		private string configSetName;

		private ReadOnlySiteCollection cachedSites;

		private AdamInstanceCollection cachedADAMInstances;

		private ApplicationPartitionCollection cachedApplicationPartitions;

		private ActiveDirectorySchema cachedSchema;

		private AdamInstance cachedSchemaRoleOwner;

		private AdamInstance cachedNamingRoleOwner;

		private ReplicationSecurityLevel cachedSecurityLevel;

		private static TimeSpan locationTimeout;

		public AdamInstanceCollection AdamInstances
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedADAMInstances == null)
				{
					this.cachedADAMInstances = this.FindAllAdamInstances();
				}
				return this.cachedADAMInstances;
			}
		}

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

		public string Name
		{
			get
			{
				this.CheckIfDisposed();
				return this.configSetName;
			}
		}

		public AdamInstance NamingRoleOwner
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedNamingRoleOwner == null)
				{
					this.cachedNamingRoleOwner = this.GetRoleOwner(AdamRole.NamingRole);
				}
				return this.cachedNamingRoleOwner;
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

		public AdamInstance SchemaRoleOwner
		{
			get
			{
				this.CheckIfDisposed();
				if (this.cachedSchemaRoleOwner == null)
				{
					this.cachedSchemaRoleOwner = this.GetRoleOwner(AdamRole.SchemaRole);
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

		static ConfigurationSet()
		{
			ConfigurationSet.locationTimeout = new TimeSpan(0, 4, 0);
		}

		internal ConfigurationSet(DirectoryContext context, string configSetName, DirectoryEntryManager directoryEntryMgr)
		{
			this.cachedSecurityLevel = ReplicationSecurityLevel.MutualAuthentication | ReplicationSecurityLevel.Negotiate;
			this.context = context;
			this.configSetName = configSetName;
			this.directoryEntryMgr = directoryEntryMgr;
		}

		internal ConfigurationSet(DirectoryContext context, string configSetName) : this(context, configSetName, new DirectoryEntryManager(context))
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

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Dispose()
		{
			this.Dispose(true);
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

		public AdamInstance FindAdamInstance()
		{
			this.CheckIfDisposed();
			return ConfigurationSet.FindOneAdamInstance(this.Name, this.context, null, null);
		}

		public AdamInstance FindAdamInstance(string partitionName)
		{
			this.CheckIfDisposed();
			if (partitionName != null)
			{
				return ConfigurationSet.FindOneAdamInstance(this.Name, this.context, partitionName, null);
			}
			else
			{
				throw new ArgumentNullException("partitionName");
			}
		}

		public AdamInstance FindAdamInstance(string partitionName, string siteName)
		{
			this.CheckIfDisposed();
			if (siteName != null)
			{
				return ConfigurationSet.FindOneAdamInstance(this.Name, this.context, partitionName, siteName);
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		internal static AdamInstanceCollection FindAdamInstances(DirectoryContext context, string partitionName, string siteName)
		{
			if (partitionName == null || partitionName.Length != 0)
			{
				if (siteName == null || siteName.Length != 0)
				{
					ArrayList arrayLists = new ArrayList();
					foreach (string replicaList in Utils.GetReplicaList(context, partitionName, siteName, false, true, false))
					{
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(replicaList, DirectoryContextType.DirectoryServer, context);
						arrayLists.Add(new AdamInstance(newDirectoryContext, replicaList));
					}
					return new AdamInstanceCollection(arrayLists);
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
			}
		}

		internal static AdamInstance FindAliveAdamInstance(string configSetName, DirectoryContext context, ArrayList adamInstanceNames)
		{
			AdamInstance adamInstance;
			object name;
			object obj;
			bool flag = false;
			AdamInstance adamInstance1 = null;
			DateTime utcNow = DateTime.UtcNow;
			IEnumerator enumerator = adamInstanceNames.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current = (string)enumerator.Current;
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(current, DirectoryContextType.DirectoryServer, context);
					DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(newDirectoryContext);
					DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
					try
					{
						//TODO: REVIEW: URGENT!!: cachedDirectoryEntry.Bind(true);
						adamInstance1 = new AdamInstance(newDirectoryContext, current, directoryEntryManager, true);
						flag = true;
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						if (cOMException.ErrorCode == -2147016646 || cOMException.ErrorCode == -2147016690 || cOMException.ErrorCode == -2147016689 || cOMException.ErrorCode == -2147023436)
						{
							DateTime dateTime = DateTime.UtcNow;
							if (dateTime.Subtract(utcNow) > ConfigurationSet.locationTimeout)
							{
								string str = "ADAMInstanceNotFoundInConfigSet";
								object[] objArray = new object[1];
								object[] objArray1 = objArray;
								int num = 0;
								if (configSetName != null)
								{
									obj = configSetName;
								}
								else
								{
									obj = context.Name;
								}
								objArray1[num] = obj;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString(str, objArray), typeof(AdamInstance), null);
							}
						}
						else
						{
							throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
						}
					}
					if (!flag)
					{
						continue;
					}
					adamInstance = adamInstance1;
					return adamInstance;
				}
				string str1 = "ADAMInstanceNotFoundInConfigSet";
				object[] objArray2 = new object[1];
				object[] objArray3 = objArray2;
				int num1 = 0;
				if (configSetName != null)
				{
					name = configSetName;
				}
				else
				{
					name = context.Name;
				}
				objArray3[num1] = name;
				throw new ActiveDirectoryObjectNotFoundException(Res.GetString(str1, objArray2), typeof(AdamInstance), null);
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return adamInstance;
		}

		public AdamInstanceCollection FindAllAdamInstances()
		{
			this.CheckIfDisposed();
			return ConfigurationSet.FindAdamInstances(this.context, null, null);
		}

		public AdamInstanceCollection FindAllAdamInstances(string partitionName)
		{
			this.CheckIfDisposed();
			if (partitionName != null)
			{
				return ConfigurationSet.FindAdamInstances(this.context, partitionName, null);
			}
			else
			{
				throw new ArgumentNullException("partitionName");
			}
		}

		public AdamInstanceCollection FindAllAdamInstances(string partitionName, string siteName)
		{
			this.CheckIfDisposed();
			if (siteName != null)
			{
				return ConfigurationSet.FindAdamInstances(this.context, partitionName, siteName);
			}
			else
			{
				throw new ArgumentNullException("siteName");
			}
		}

		internal static AdamInstance FindAnyAdamInstance(DirectoryContext context)
		{
			if (context.ContextType == DirectoryContextType.ConfigurationSet)
			{
				DirectoryEntry searchRootEntry = ConfigurationSet.GetSearchRootEntry(Forest.GetCurrentForest());
				ArrayList arrayLists = new ArrayList();
				try
				{
					try
					{
						StringBuilder stringBuilder = new StringBuilder(15);
						stringBuilder.Append("(&(");
						stringBuilder.Append(PropertyManager.ObjectCategory);
						stringBuilder.Append("=serviceConnectionPoint)");
						stringBuilder.Append("(");
						stringBuilder.Append(PropertyManager.Keywords);
						stringBuilder.Append("=1.2.840.113556.1.4.1851)(");
						stringBuilder.Append(PropertyManager.Keywords);
						stringBuilder.Append("=");
						stringBuilder.Append(Utils.GetEscapedFilterValue(context.Name));
						stringBuilder.Append("))");
						string str = stringBuilder.ToString();
						string[] serviceBindingInformation = new string[1];
						serviceBindingInformation[0] = PropertyManager.ServiceBindingInformation;
						ADSearcher aDSearcher = new ADSearcher(searchRootEntry, str, serviceBindingInformation, SearchScope.Subtree, false, false);
						SearchResultCollection searchResultCollections = aDSearcher.FindAll();
						try
						{
							foreach (SearchResult item in searchResultCollections)
							{
								string str1 = "ldap://";
								IEnumerator enumerator = item.Properties[PropertyManager.ServiceBindingInformation].GetEnumerator();
								try
								{
									while (enumerator.MoveNext())
									{
										string str2 = item.ToString ();
										if (str2.Length <= str1.Length || string.Compare(str2.Substring(0, str1.Length), str1, StringComparison.OrdinalIgnoreCase) != 0)
										{
											continue;
										}
										arrayLists.Add(str2.Substring(str1.Length));
									}
								}
								finally
								{
									IDisposable disposable = enumerator as IDisposable;
									if (disposable != null)
									{
										disposable.Dispose();
									}
								}
							}
						}
						finally
						{
							searchResultCollections.Dispose();
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
					searchRootEntry.Dispose();
				}
				return ConfigurationSet.FindAliveAdamInstance(null, context, arrayLists);
			}
			else
			{
				DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
				DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
				if (Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryApplicationMode))
				{
					string propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DnsHostName);
					return new AdamInstance(context, propertyValue, directoryEntryManager);
				}
				else
				{
					directoryEntryManager.RemoveIfExists(directoryEntryManager.ExpandWellKnownDN(WellKnownDN.RootDSE));
					throw new ArgumentException(Res.GetString("TargetShouldBeServerORConfigSet"), "context");
				}
			}
		}

		internal static AdamInstance FindOneAdamInstance(DirectoryContext context, string partitionName, string siteName)
		{
			return ConfigurationSet.FindOneAdamInstance(null, context, partitionName, siteName);
		}

		internal static AdamInstance FindOneAdamInstance(string configSetName, DirectoryContext context, string partitionName, string siteName)
		{
			if (partitionName == null || partitionName.Length != 0)
			{
				if (siteName == null || siteName.Length != 0)
				{
					ArrayList replicaList = Utils.GetReplicaList(context, partitionName, siteName, false, true, false);
					if (replicaList.Count >= 1)
					{
						return ConfigurationSet.FindAliveAdamInstance(configSetName, context, replicaList);
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ADAMInstanceNotFound"), typeof(AdamInstance), null);
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
				}
			}
			else
			{
				throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
			}
		}

		private ArrayList GetApplicationPartitions()
		{
			ArrayList arrayLists = new ArrayList();
			DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
			DirectoryEntry directoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.PartitionsContainer);
			StringBuilder stringBuilder = new StringBuilder(100);
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
			string[] nCName = new string[2];
			nCName[0] = PropertyManager.NCName;
			nCName[1] = PropertyManager.MsDSNCReplicaLocations;
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, str, nCName, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			using (searchResultCollections)
			{
				try
				{
					searchResultCollections = aDSearcher.FindAll();
					string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.SchemaNamingContext);
					string propertyValue1 = (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.ConfigurationNamingContext);
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.NCName);
						if (searchResultPropertyValue.Equals(propertyValue) || searchResultPropertyValue.Equals(propertyValue1))
						{
							continue;
						}
						ResultPropertyValueCollection item = searchResult.Properties[PropertyManager.MsDSNCReplicaLocations];
						if (item.Count <= 0)
						{
							continue;
						}
						string adamDnsHostNameFromNTDSA = Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string)item[Utils.GetRandomIndex(item.Count)]);
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(adamDnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, this.context);
						arrayLists.Add(new ApplicationPartition(newDirectoryContext, searchResultPropertyValue, null, ApplicationPartitionType.ADAMApplicationPartition, new DirectoryEntryManager(newDirectoryContext)));
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			return arrayLists;
		}

		public static ConfigurationSet GetConfigurationSet(DirectoryContext context)
		{
			if (context != null)
			{
				if (context.ContextType == DirectoryContextType.ConfigurationSet || context.ContextType == DirectoryContextType.DirectoryServer)
				{
					if (context.isServer() || context.isADAMConfigSet())
					{
						context = new DirectoryContext(context);
						DirectoryEntryManager directoryEntryManager = new DirectoryEntryManager(context);
						string propertyValue = null;
						try
						{
							DirectoryEntry cachedDirectoryEntry = directoryEntryManager.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
							if (!context.isServer() || Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryApplicationMode))
							{
								propertyValue = (string)PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.ConfigurationNamingContext);
							}
							else
							{
								object[] name = new object[1];
								name[0] = context.Name;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", name), typeof(ConfigurationSet), null);
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
								if (context.ContextType != DirectoryContextType.ConfigurationSet)
								{
									object[] objArray = new object[1];
									objArray[0] = context.Name;
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", objArray), typeof(ConfigurationSet), null);
								}
								else
								{
									throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ConfigurationSet), context.Name);
								}
							}
						}
						catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
						{
							if (context.ContextType != DirectoryContextType.ConfigurationSet)
							{
								throw;
							}
							else
							{
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ConfigurationSet), context.Name);
							}
						}
						return new ConfigurationSet(context, propertyValue, directoryEntryManager);
					}
					else
					{
						if (context.ContextType != DirectoryContextType.ConfigurationSet)
						{
							object[] name1 = new object[1];
							name1[0] = context.Name;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", name1), typeof(ConfigurationSet), null);
						}
						else
						{
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ConfigurationSet), context.Name);
						}
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("TargetShouldBeServerORConfigSet"), "context");
				}
			}
			else
			{
				throw new ArgumentNullException("context");
			}
		}

		public DirectoryEntry GetDirectoryEntry()
		{
			this.CheckIfDisposed();
			return DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.ConfigurationNamingContext);
		}

		private AdamInstance GetRoleOwner(AdamRole role)
		{
			DirectoryEntry cachedDirectoryEntry = null;
			string adamDnsHostNameFromNTDSA = null;
			using (cachedDirectoryEntry)
			{
				try
				{
					AdamRole adamRole = role;
					switch (adamRole)
					{
						case AdamRole.SchemaRole:
						{
							cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.SchemaNamingContext);
							break;
						}
						case AdamRole.NamingRole:
						{
							cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.PartitionsContainer);
							break;
						}
					}
					cachedDirectoryEntry.RefreshCache();
					adamDnsHostNameFromNTDSA = Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.FsmoRoleOwner));
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(adamDnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, this.context);
			return new AdamInstance(newDirectoryContext, adamDnsHostNameFromNTDSA);
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		private static DirectoryEntry GetSearchRootEntry(Forest forest)
		{
			DirectoryEntry directoryEntry;
			DirectoryContext directoryContext = forest.GetDirectoryContext();
			bool flag = false;
			bool flag1 = false;
			AuthenticationTypes defaultAuthType = Utils.DefaultAuthType;
			if (directoryContext.ContextType == DirectoryContextType.DirectoryServer)
			{
				flag = true;
				DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(directoryContext, WellKnownDN.RootDSE);
				string propertyValue = (string)PropertyManager.GetPropertyValue(directoryContext, directoryEntry1, PropertyManager.IsGlobalCatalogReady);
				flag1 = Utils.Compare(propertyValue, "TRUE") == 0;
			}
			if (!flag)
			{
				directoryEntry = new DirectoryEntry(string.Concat("GC://", forest.Name), directoryContext.UserName, directoryContext.Password, defaultAuthType);
			}
			else
			{
				if (DirectoryContext.ServerBindSupported)
				{
					defaultAuthType = defaultAuthType | AuthenticationTypes.ServerBind;
				}
				if (!flag1)
				{
					directoryEntry = new DirectoryEntry(string.Concat("LDAP://", directoryContext.GetServerName()), directoryContext.UserName, directoryContext.Password, defaultAuthType);
				}
				else
				{
					directoryEntry = new DirectoryEntry(string.Concat("GC://", directoryContext.GetServerName()), directoryContext.UserName, directoryContext.Password, defaultAuthType);
				}
			}
			return directoryEntry;
		}

		public ReplicationSecurityLevel GetSecurityLevel()
		{
			this.CheckIfDisposed();
			if (this.cachedSecurityLevel == (ReplicationSecurityLevel.MutualAuthentication | ReplicationSecurityLevel.Negotiate))
			{
				DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.ConfigurationNamingContext);
				this.cachedSecurityLevel = (ReplicationSecurityLevel)((int)PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.MsDSReplAuthenticationMode));
			}
			return this.cachedSecurityLevel;
		}

		private ArrayList GetSites()
		{
			ArrayList arrayLists = new ArrayList();
			DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.SitesContainer);
			string str = string.Concat("(", PropertyManager.ObjectCategory, "=site)");
			string[] cn = new string[1];
			cn[0] = PropertyManager.Cn;
			ADSearcher aDSearcher = new ADSearcher(cachedDirectoryEntry, str, cn, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			using (searchResultCollections)
			{
				try
				{
					searchResultCollections = aDSearcher.FindAll();
					foreach (SearchResult searchResult in searchResultCollections)
					{
						arrayLists.Add(new ActiveDirectorySite(this.context, (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn), true));
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
			}
			return arrayLists;
		}

		public void SetSecurityLevel(ReplicationSecurityLevel securityLevel)
		{
			this.CheckIfDisposed();
			if (securityLevel < ReplicationSecurityLevel.NegotiatePassThrough || securityLevel > ReplicationSecurityLevel.MutualAuthentication)
			{
				throw new InvalidEnumArgumentException("securityLevel", (int)securityLevel, typeof(ReplicationSecurityLevel));
			}
			else
			{
				try
				{
					DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.ConfigurationNamingContext);
					cachedDirectoryEntry.Properties[PropertyManager.MsDSReplAuthenticationMode].Value = (int)securityLevel;
					cachedDirectoryEntry.CommitChanges();
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
				}
				this.cachedSecurityLevel = ReplicationSecurityLevel.MutualAuthentication | ReplicationSecurityLevel.Negotiate;
				return;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}
	}
}