using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class ActiveDirectorySite : IDisposable
	{
		internal DirectoryContext context;

		private string name;

		internal DirectoryEntry cachedEntry;

		private DirectoryEntry ntdsEntry;

		private ActiveDirectorySubnetCollection subnets;

		private DirectoryServer topologyGenerator;

		private ReadOnlySiteCollection adjacentSites;

		private bool disposed;

		private DomainCollection domains;

		private ReadOnlyDirectoryServerCollection servers;

		private ReadOnlySiteLinkCollection links;

		private ActiveDirectorySiteOptions siteOptions;

		private ReadOnlyDirectoryServerCollection bridgeheadServers;

		private DirectoryServerCollection SMTPBridgeheadServers;

		private DirectoryServerCollection RPCBridgeheadServers;

		private byte[] replicationSchedule;

		internal bool existing;

		private bool subnetRetrieved;

		private bool isADAMServer;

		private bool checkADAM;

		private bool topologyTouched;

		private bool adjacentSitesRetrieved;

		private string siteDN;

		private bool domainsRetrieved;

		private bool serversRetrieved;

		private bool belongLinksRetrieved;

		private bool bridgeheadServerRetrieved;

		private bool SMTPBridgeRetrieved;

		private bool RPCBridgeRetrieved;

		private static int ERROR_NO_SITENAME;

		public ReadOnlySiteCollection AdjacentSites
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.adjacentSitesRetrieved)
					{
						this.adjacentSites.Clear();
						this.GetAdjacentSites();
						this.adjacentSitesRetrieved = true;
					}
					return this.adjacentSites;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ReadOnlyDirectoryServerCollection BridgeheadServers
		{
			get
			{
				if (!this.disposed)
				{
					if (!this.bridgeheadServerRetrieved)
					{
						this.bridgeheadServers = this.GetBridgeheadServers();
						this.bridgeheadServerRetrieved = true;
					}
					return this.bridgeheadServers;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public DomainCollection Domains
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.domainsRetrieved)
					{
						this.domains.Clear();
						this.GetDomains();
						this.domainsRetrieved = true;
					}
					return this.domains;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public DirectoryServer InterSiteTopologyGenerator
		{
			get
			{
				bool flag;
				if (!this.disposed)
				{
					if (this.existing && this.topologyGenerator == null && !this.topologyTouched)
					{
						try
						{
							flag = this.NTDSSiteEntry.Properties.Contains("interSiteTopologyGenerator");
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						if (flag)
						{
							string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, this.NTDSSiteEntry, PropertyManager.InterSiteTopologyGenerator);
							string str = null;
							DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, propertyValue);
							try
							{
								str = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry.Parent, PropertyManager.DnsHostName);
							}
							catch (COMException cOMException3)
							{
								COMException cOMException2 = cOMException3;
								if (cOMException2.ErrorCode == -2147016656)
								{
									DirectoryServer directoryServer = null;
									return directoryServer;
								}
							}
							if (!this.IsADAM)
							{
								this.topologyGenerator = new DomainController(Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, this.context), str);
							}
							else
							{
								int num = (int)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.MsDSPortLDAP);
								string str1 = str;
								if (num != 0x185)
								{
									str1 = string.Concat(str, ":", num);
								}
								this.topologyGenerator = new AdamInstance(Utils.GetNewDirectoryContext(str1, DirectoryContextType.DirectoryServer, this.context), str1);
							}
						}
					}
					return this.topologyGenerator;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.disposed)
				{
					if (value != null)
					{
						if (this.existing)
						{
							//TODO: Review: this.NTDSSiteEntry;
						}
						this.topologyTouched = true;
						this.topologyGenerator = value;
						return;
					}
					else
					{
						throw new ArgumentNullException("value");
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ActiveDirectorySchedule IntraSiteReplicationSchedule
		{
			get
			{
				if (!this.disposed)
				{
					ActiveDirectorySchedule activeDirectorySchedule = null;
					if (!this.existing)
					{
						if (this.replicationSchedule != null)
						{
							activeDirectorySchedule = new ActiveDirectorySchedule();
							activeDirectorySchedule.SetUnmanagedSchedule(this.replicationSchedule);
						}
					}
					else
					{
						try
						{
							if (this.NTDSSiteEntry.Properties.Contains("schedule"))
							{
								byte[] item = (byte[])this.NTDSSiteEntry.Properties["schedule"][0];
								activeDirectorySchedule = new ActiveDirectorySchedule();
								activeDirectorySchedule.SetUnmanagedSchedule(item);
							}
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
					}
					return activeDirectorySchedule;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.disposed)
				{
					if (!this.existing)
					{
						if (value != null)
						{
							this.replicationSchedule = value.GetUnmanagedSchedule();
						}
						else
						{
							this.replicationSchedule = null;
							return;
						}
					}
					else
					{
						try
						{
							if (value != null)
							{
								this.NTDSSiteEntry.Properties["schedule"].Value = value.GetUnmanagedSchedule();
							}
							else
							{
								if (this.NTDSSiteEntry.Properties.Contains("schedule"))
								{
									this.NTDSSiteEntry.Properties["schedule"].Clear();
								}
							}
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
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		private bool IsADAM
		{
			get
			{
				if (!this.checkADAM)
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
					PropertyValueCollection item = null;
					try
					{
						item = directoryEntry.Properties["supportedCapabilities"];
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					if (item.Contains(SupportedCapability.ADAMOid))
					{
						this.isADAMServer = true;
					}
				}
				return this.isADAMServer;
			}
		}

		public string Location
		{
			get
			{
				string item;
				if (!this.disposed)
				{
					try
					{
						if (!this.cachedEntry.Properties.Contains("location"))
						{
							item = null;
						}
						else
						{
							item = (string)this.cachedEntry.Properties["location"][0];
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return item;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.disposed)
				{
					try
					{
						if (value != null)
						{
							this.cachedEntry.Properties["location"].Value = value;
						}
						else
						{
							if (this.cachedEntry.Properties.Contains("location"))
							{
								this.cachedEntry.Properties["location"].Clear();
							}
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public string Name
		{
			get
			{
				if (!this.disposed)
				{
					return this.name;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		private DirectoryEntry NTDSSiteEntry
		{
			get
			{
				if (this.ntdsEntry == null)
				{
					DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, string.Concat("CN=NTDS Site Settings,", (string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)));
					try
					{
						directoryEntry.RefreshCache();
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						if (cOMException.ErrorCode != -2147016656)
						{
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = this.name;
							string str = Res.GetString("NTDSSiteSetting", objArray);
							throw new ActiveDirectoryOperationException(str, cOMException, 0x2030);
						}
					}
					this.ntdsEntry = directoryEntry;
				}
				return this.ntdsEntry;
			}
		}

		public ActiveDirectorySiteOptions Options
		{
			get
			{
				ActiveDirectorySiteOptions item;
				if (!this.disposed)
				{
					if (!this.existing)
					{
						return this.siteOptions;
					}
					else
					{
						try
						{
							if (!this.NTDSSiteEntry.Properties.Contains("options"))
							{
								item = ActiveDirectorySiteOptions.None;
							}
							else
							{
								item = (ActiveDirectorySiteOptions)this.NTDSSiteEntry.Properties["options"][0];
							}
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
						}
						return item;
					}
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
			set
			{
				if (!this.disposed)
				{
					if (!this.existing)
					{
						this.siteOptions = value;
					}
					else
					{
						try
						{
							this.NTDSSiteEntry.Properties["options"].Value = value;
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
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public DirectoryServerCollection PreferredRpcBridgeheadServers
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.RPCBridgeRetrieved)
					{
						this.RPCBridgeheadServers.initialized = false;
						this.RPCBridgeheadServers.Clear();
						this.GetPreferredBridgeheadServers(ActiveDirectoryTransportType.Rpc);
						this.RPCBridgeRetrieved = true;
					}
					this.RPCBridgeheadServers.initialized = true;
					return this.RPCBridgeheadServers;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public DirectoryServerCollection PreferredSmtpBridgeheadServers
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.SMTPBridgeRetrieved)
					{
						this.SMTPBridgeheadServers.initialized = false;
						this.SMTPBridgeheadServers.Clear();
						this.GetPreferredBridgeheadServers(ActiveDirectoryTransportType.Smtp);
						this.SMTPBridgeRetrieved = true;
					}
					this.SMTPBridgeheadServers.initialized = true;
					return this.SMTPBridgeheadServers;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ReadOnlyDirectoryServerCollection Servers
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.serversRetrieved)
					{
						this.servers.Clear();
						this.GetServers();
						this.serversRetrieved = true;
					}
					return this.servers;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ReadOnlySiteLinkCollection SiteLinks
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.belongLinksRetrieved)
					{
						this.links.Clear();
						this.GetLinks();
						this.belongLinksRetrieved = true;
					}
					return this.links;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public ActiveDirectorySubnetCollection Subnets
		{
			get
			{
				if (!this.disposed)
				{
					if (this.existing && !this.subnetRetrieved)
					{
						this.subnets.initialized = false;
						this.subnets.Clear();
						this.GetSubnets();
						this.subnetRetrieved = true;
					}
					this.subnets.initialized = true;
					return this.subnets;
				}
				else
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		static ActiveDirectorySite()
		{
			ActiveDirectorySite.ERROR_NO_SITENAME = 0x77f;
		}

		public ActiveDirectorySite(DirectoryContext context, string siteName)
		{
			this.adjacentSites = new ReadOnlySiteCollection();
			this.domains = new DomainCollection(null);
			this.servers = new ReadOnlyDirectoryServerCollection();
			this.links = new ReadOnlySiteLinkCollection();
			this.bridgeheadServers = new ReadOnlyDirectoryServerCollection();
			ActiveDirectorySite.ValidateArgument(context, siteName);
			context = new DirectoryContext(context);
			this.context = context;
			this.name = siteName;
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
					string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
					this.siteDN = string.Concat("CN=Sites,", propertyValue);
					directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, this.siteDN);
					string escapedPath = string.Concat("cn=", this.name);
					escapedPath = Utils.GetEscapedPath(escapedPath);
					this.cachedEntry = directoryEntry.Children.Add(escapedPath, "site");
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
			}
			this.subnets = new ActiveDirectorySubnetCollection(context, string.Concat("CN=", siteName, ",", this.siteDN));
			string str = string.Concat("CN=IP,CN=Inter-Site Transports,", this.siteDN);
			this.RPCBridgeheadServers = new DirectoryServerCollection(context, string.Concat("CN=", siteName, ",", this.siteDN), str);
			str = string.Concat("CN=SMTP,CN=Inter-Site Transports,", this.siteDN);
			this.SMTPBridgeheadServers = new DirectoryServerCollection(context, string.Concat("CN=", siteName, ",", this.siteDN), str);
		}

		internal ActiveDirectorySite(DirectoryContext context, string siteName, bool existing)
		{
			this.adjacentSites = new ReadOnlySiteCollection();
			this.domains = new DomainCollection(null);
			this.servers = new ReadOnlyDirectoryServerCollection();
			this.links = new ReadOnlySiteLinkCollection();
			this.bridgeheadServers = new ReadOnlyDirectoryServerCollection();
			this.context = context;
			this.name = siteName;
			this.existing = existing;
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
			this.siteDN = string.Concat("CN=Sites,", (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext));
			this.cachedEntry = DirectoryEntryManager.GetDirectoryEntry(context, string.Concat("CN=", siteName, ",", this.siteDN));
			this.subnets = new ActiveDirectorySubnetCollection(context, string.Concat("CN=", siteName, ",", this.siteDN));
			string str = string.Concat("CN=IP,CN=Inter-Site Transports,", this.siteDN);
			this.RPCBridgeheadServers = new DirectoryServerCollection(context, (string)PropertyManager.GetPropertyValue(context, this.cachedEntry, PropertyManager.DistinguishedName), str);
			str = string.Concat("CN=SMTP,CN=Inter-Site Transports,", this.siteDN);
			this.SMTPBridgeheadServers = new DirectoryServerCollection(context, (string)PropertyManager.GetPropertyValue(context, this.cachedEntry, PropertyManager.DistinguishedName), str);
		}

		public void Delete()
		{
			if (!this.disposed)
			{
				if (this.existing)
				{
					try
					{
						this.cachedEntry.DeleteTree();
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					return;
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotDelete"));
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.cachedEntry != null)
				{
					this.cachedEntry.Dispose();
				}
				if (this.ntdsEntry != null)
				{
					this.ntdsEntry.Dispose();
				}
			}
			this.disposed = true;
		}

		public static ActiveDirectorySite FindByName(DirectoryContext context, string siteName)
		{
			DirectoryEntry directoryEntry;
			ActiveDirectorySite activeDirectorySite;
			ActiveDirectorySite.ValidateArgument(context, siteName);
			context = new DirectoryContext(context);
			try
			{
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
				string str = string.Concat("CN=Sites,", (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext));
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str);
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
			try
			{
				try
				{
					string[] strArrays = new string[1];
					strArrays[0] = "distinguishedName";
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=site)(objectCategory=site)(name=", Utils.GetEscapedFilterValue(siteName), "))"), strArrays, SearchScope.OneLevel, false, false);
					SearchResult searchResult = aDSearcher.FindOne();
					if (searchResult != null)
					{
						ActiveDirectorySite activeDirectorySite1 = new ActiveDirectorySite(context, siteName, true);
						activeDirectorySite = activeDirectorySite1;
					}
					else
					{
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySite), siteName);
					}
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
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySite), siteName);
					}
				}
			}
			finally
			{
				directoryEntry.Dispose();
			}
			return activeDirectorySite;
		}

		private void GetAdjacentSites()
		{
			ActiveDirectoryTransportType activeDirectoryTransportType;
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
			string item = (string)directoryEntry.Properties["configurationNamingContext"][0];
			string str = string.Concat("CN=Inter-Site Transports,CN=Sites,", item);
			directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
			string[] strArrays = new string[2];
			strArrays[0] = "cn";
			strArrays[1] = "distinguishedName";
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=siteLink)(objectCategory=SiteLink)(siteList=", Utils.GetEscapedFilterValue((string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)), "))"), strArrays, SearchScope.Subtree);
			SearchResultCollection searchResultCollections = null;
			try
			{
				searchResultCollections = aDSearcher.FindAll();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			try
			{
				ActiveDirectorySiteLink activeDirectorySiteLink = null;
				foreach (SearchResult searchResult in searchResultCollections)
				{
					string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DistinguishedName);
					string searchResultPropertyValue1 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
					string value = Utils.GetDNComponents(searchResultPropertyValue)[1].Value;
					if (string.Compare(value, "IP", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (string.Compare(value, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
						{
							object[] objArray = new object[1];
							objArray[0] = value;
							string str1 = Res.GetString("UnknownTransport", objArray);
							throw new ActiveDirectoryOperationException(str1);
						}
						else
						{
							activeDirectoryTransportType = ActiveDirectoryTransportType.Smtp;
						}
					}
					else
					{
						activeDirectoryTransportType = ActiveDirectoryTransportType.Rpc;
					}
					try
					{
						activeDirectorySiteLink = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue1, activeDirectoryTransportType, true, searchResult.GetDirectoryEntry());
						foreach (ActiveDirectorySite site in activeDirectorySiteLink.Sites)
						{
							if (Utils.Compare(site.Name, this.Name) == 0 || this.adjacentSites.Contains(site))
							{
								continue;
							}
							this.adjacentSites.Add(site);
						}
					}
					finally
					{
						activeDirectorySiteLink.Dispose();
					}
				}
			}
			finally
			{
				searchResultCollections.Dispose();
				directoryEntry.Dispose();
			}
		}

		private ReadOnlyDirectoryServerCollection GetBridgeheadServers()
		{
			DirectoryServer domainController;
			NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
			pathname.EscapedMode = 4;
			ReadOnlyDirectoryServerCollection readOnlyDirectoryServerCollection = new ReadOnlyDirectoryServerCollection();
			if (this.existing)
			{
				Hashtable hashtables = new Hashtable();
				Hashtable hashtables1 = new Hashtable();
				Hashtable hashtables2 = new Hashtable();
				string str = string.Concat("CN=Servers,", (string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName));
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
				try
				{
					string[] strArrays = new string[4];
					strArrays[0] = "fromServer";
					strArrays[1] = "distinguishedName";
					strArrays[2] = "dNSHostName";
					strArrays[3] = "objectCategory";
					ADSearcher aDSearcher = new ADSearcher(directoryEntry, "(|(objectCategory=server)(objectCategory=NTDSConnection))", strArrays, SearchScope.Subtree, true, true);
					SearchResultCollection searchResultCollections = null;
					try
					{
						searchResultCollections = aDSearcher.FindAll();
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
					}
					try
					{
						foreach (SearchResult searchResult in searchResultCollections)
						{
							string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.ObjectCategory);
							if (Utils.Compare(searchResultPropertyValue, 0, "CN=Server".Length, "CN=Server", 0, "CN=Server".Length) != 0)
							{
								continue;
							}
							hashtables2.Add((string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DistinguishedName), (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsHostName));
						}
						foreach (SearchResult searchResult1 in searchResultCollections)
						{
							string searchResultPropertyValue1 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult1, PropertyManager.ObjectCategory);
							if (Utils.Compare(searchResultPropertyValue1, 0, "CN=Server".Length, "CN=Server", 0, "CN=Server".Length) == 0)
							{
								continue;
							}
							string str1 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult1, PropertyManager.FromServer);
							string partialDN = Utils.GetPartialDN(str1, 3);
							pathname.Set(partialDN, 4);
							partialDN = pathname.Retrieve(11);
							partialDN = partialDN.Substring(3);
							string partialDN1 = Utils.GetPartialDN((string)PropertyManager.GetSearchResultPropertyValue(searchResult1, PropertyManager.DistinguishedName), 2);
							if (hashtables.Contains(partialDN1))
							{
								continue;
							}
							string item = (string)hashtables2[partialDN1];
							if (!hashtables1.Contains(partialDN1))
							{
								hashtables1.Add(partialDN1, item);
							}
							if (Utils.Compare((string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.Cn), partialDN) == 0)
							{
								continue;
							}
							hashtables.Add(partialDN1, item);
							hashtables1.Remove(partialDN1);
						}
					}
					finally
					{
						searchResultCollections.Dispose();
					}
				}
				finally
				{
					directoryEntry.Dispose();
				}
				if (hashtables1.Count != 0)
				{
					DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(this.context, this.siteDN);
					StringBuilder stringBuilder = new StringBuilder(100);
					if (hashtables1.Count > 1)
					{
						stringBuilder.Append("(|");
					}
					foreach (DictionaryEntry hashtable in hashtables1)
					{
						stringBuilder.Append("(fromServer=");
						stringBuilder.Append("CN=NTDS Settings,");
						stringBuilder.Append(Utils.GetEscapedFilterValue((string)hashtable.Key));
						stringBuilder.Append(")");
					}
					if (hashtables1.Count > 1)
					{
						stringBuilder.Append(")");
					}
					string[] strArrays1 = new string[2];
					strArrays1[0] = "fromServer";
					strArrays1[1] = "distinguishedName";
					ADSearcher aDSearcher1 = new ADSearcher(directoryEntry1, string.Concat("(&(objectClass=nTDSConnection)(objectCategory=NTDSConnection)", stringBuilder.ToString(), ")"), strArrays1, SearchScope.Subtree);
					SearchResultCollection searchResultCollections1 = null;
					try
					{
						searchResultCollections1 = aDSearcher1.FindAll();
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
					}
					try
					{
						foreach (SearchResult searchResult2 in searchResultCollections1)
						{
							string searchResultPropertyValue2 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult2, PropertyManager.FromServer);
							string str2 = searchResultPropertyValue2.Substring(17);
							if (!hashtables1.Contains(str2))
							{
								continue;
							}
							string partialDN2 = Utils.GetPartialDN((string)PropertyManager.GetSearchResultPropertyValue(searchResult2, PropertyManager.DistinguishedName), 4);
							pathname.Set(partialDN2, 4);
							partialDN2 = pathname.Retrieve(11);
							partialDN2 = partialDN2.Substring(3);
							if (Utils.Compare(partialDN2, (string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.Cn)) == 0)
							{
								continue;
							}
							string item1 = (string)hashtables1[str2];
							hashtables1.Remove(str2);
							hashtables.Add(str2, item1);
						}
					}
					finally
					{
						searchResultCollections1.Dispose();
						directoryEntry1.Dispose();
					}
				}
				foreach (DictionaryEntry dictionaryEntry in hashtables)
				{
					string value = (string)dictionaryEntry.Value;
					if (!this.IsADAM)
					{
						domainController = new DomainController(Utils.GetNewDirectoryContext(value, DirectoryContextType.DirectoryServer, this.context), value);
					}
					else
					{
						DirectoryEntry directoryEntry2 = DirectoryEntryManager.GetDirectoryEntry(this.context, string.Concat("CN=NTDS Settings,", dictionaryEntry.Key));
						int propertyValue = (int)PropertyManager.GetPropertyValue(this.context, directoryEntry2, PropertyManager.MsDSPortLDAP);
						string str3 = value;
						if (propertyValue != 0x185)
						{
							str3 = string.Concat(value, ":", propertyValue);
						}
						domainController = new AdamInstance(Utils.GetNewDirectoryContext(str3, DirectoryContextType.DirectoryServer, this.context), str3);
					}
					readOnlyDirectoryServerCollection.Add(domainController);
				}
			}
			return readOnlyDirectoryServerCollection;
		}

		public static ActiveDirectorySite GetComputerSite()
		{
			ActiveDirectorySite activeDirectorySite;
			new DirectoryContext(DirectoryContextType.Forest);
			IntPtr intPtr = (IntPtr)0;
			int num = UnsafeNativeMethods.DsGetSiteName(null, ref intPtr);
			if (num == 0)
			{
				try
				{
					string stringUni = Marshal.PtrToStringUni(intPtr);
					string dnsForestName = Locator.GetDomainControllerInfo(null, null, null, (long)16).DnsForestName;
					DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(dnsForestName, DirectoryContextType.Forest, null);
					ActiveDirectorySite activeDirectorySite1 = ActiveDirectorySite.FindByName(newDirectoryContext, stringUni);
					activeDirectorySite = activeDirectorySite1;
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr);
					}
				}
				return activeDirectorySite;
			}
			else
			{
				if (num != ActiveDirectorySite.ERROR_NO_SITENAME)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num);
				}
				else
				{
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("NoCurrentSite"), typeof(ActiveDirectorySite), null);
				}
			}
		}

		public DirectoryEntry GetDirectoryEntry()
		{
			if (!this.disposed)
			{
				if (this.existing)
				{
					return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedEntry.Path);
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("CannotGetObject"));
				}
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private void GetDomains()
		{
			if (!this.IsADAM)
			{
				string currentServerName = this.servers[0].Name; //TODO: REVIEW: this.cachedEntry.Options.GetCurrentServerName();
				DomainController domainController = DomainController.GetDomainController(Utils.GetNewDirectoryContext(currentServerName, DirectoryContextType.DirectoryServer, this.context));
				IntPtr handle = domainController.Handle;
				IntPtr intPtr = (IntPtr)0;
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsListDomainsInSiteW");
				if (procAddress != (IntPtr)0)
				{
					UnsafeNativeMethods.DsListDomainsInSiteW delegateForFunctionPointer = (UnsafeNativeMethods.DsListDomainsInSiteW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsListDomainsInSiteW));
					int propertyValue = delegateForFunctionPointer(handle, (string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName), ref intPtr);
					if (propertyValue == 0)
					{
						try
						{
							DS_NAME_RESULT dSNAMERESULT = new DS_NAME_RESULT();
							Marshal.PtrToStructure(intPtr, dSNAMERESULT);
							int num = dSNAMERESULT.cItems;
							IntPtr intPtr1 = dSNAMERESULT.rItems;
							if (num > 0)
							{
								Marshal.ReadInt32(intPtr1);
								for (int i = 0; i < num; i++)
								{
									IntPtr intPtr2 = (IntPtr)((long)intPtr1 + (long)(Marshal.SizeOf(typeof(DS_NAME_RESULT_ITEM)) * i));
									DS_NAME_RESULT_ITEM dSNAMERESULTITEM = new DS_NAME_RESULT_ITEM();
									Marshal.PtrToStructure(intPtr2, dSNAMERESULTITEM);
									if (dSNAMERESULTITEM.status == DS_NAME_ERROR.DS_NAME_NO_ERROR || dSNAMERESULTITEM.status == DS_NAME_ERROR.DS_NAME_ERROR_DOMAIN_ONLY)
									{
										string stringUni = Marshal.PtrToStringUni(dSNAMERESULTITEM.pName);
										if (stringUni != null && stringUni.Length > 0)
										{
											string dnsNameFromDN = Utils.GetDnsNameFromDN(stringUni);
											Domain domain = new Domain(Utils.GetNewDirectoryContext(dnsNameFromDN, DirectoryContextType.Domain, this.context), dnsNameFromDN);
											this.domains.Add(domain);
										}
									}
								}
							}
						}
						finally
						{
							procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
							if (procAddress != (IntPtr)0)
							{
								UnsafeNativeMethods.DsFreeNameResultW dsFreeNameResultW = (UnsafeNativeMethods.DsFreeNameResultW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsFreeNameResultW));
								dsFreeNameResultW(intPtr);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
							}
						}
					}
					else
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(propertyValue, currentServerName);
					}
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
		}

		private void GetLinks()
		{
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
			string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ConfigurationNamingContext);
			string str = string.Concat("CN=Inter-Site Transports,CN=Sites,", propertyValue);
			directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
			string[] strArrays = new string[2];
			strArrays[0] = "cn";
			strArrays[1] = "distinguishedName";
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=siteLink)(objectCategory=SiteLink)(siteList=", Utils.GetEscapedFilterValue((string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)), "))"), strArrays, SearchScope.Subtree);
			SearchResultCollection searchResultCollections = null;
			try
			{
				searchResultCollections = aDSearcher.FindAll();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			try
			{
				foreach (SearchResult searchResult in searchResultCollections)
				{
					DirectoryEntry directoryEntry1 = searchResult.GetDirectoryEntry();
					string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
					string value = Utils.GetDNComponents((string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DistinguishedName))[1].Value;
					ActiveDirectorySiteLink activeDirectorySiteLink = null;
					if (string.Compare(value, "IP", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (string.Compare(value, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
						{
							object[] objArray = new object[1];
							objArray[0] = value;
							string str1 = Res.GetString("UnknownTransport", objArray);
							throw new ActiveDirectoryOperationException(str1);
						}
						else
						{
							activeDirectorySiteLink = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue, ActiveDirectoryTransportType.Smtp, true, directoryEntry1);
						}
					}
					else
					{
						activeDirectorySiteLink = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue, ActiveDirectoryTransportType.Rpc, true, directoryEntry1);
					}
					this.links.Add(activeDirectorySiteLink);
				}
			}
			finally
			{
				searchResultCollections.Dispose();
				directoryEntry.Dispose();
			}
		}

		private void GetPreferredBridgeheadServers(ActiveDirectoryTransportType transport)
		{
			string str;
			DirectoryServer domainController;
			string str1 = string.Concat("CN=Servers,", PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName));
			if (transport != ActiveDirectoryTransportType.Smtp)
			{
				str = string.Concat("CN=IP,CN=Inter-Site Transports,", this.siteDN);
			}
			else
			{
				str = string.Concat("CN=SMTP,CN=Inter-Site Transports,", this.siteDN);
			}
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str1);
			string[] strArrays = new string[2];
			strArrays[0] = "dNSHostName";
			strArrays[1] = "distinguishedName";
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=server)(objectCategory=Server)(bridgeheadTransportList=", Utils.GetEscapedFilterValue(str), "))"), strArrays, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			try
			{
				searchResultCollections = aDSearcher.FindAll();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			try
			{
				DirectoryEntry directoryEntry1 = null;
				foreach (SearchResult searchResult in searchResultCollections)
				{
					string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsHostName);
					DirectoryEntry directoryEntry2 = searchResult.GetDirectoryEntry();
					try
					{
						directoryEntry1 = directoryEntry2.Children.Find("CN=NTDS Settings", "nTDSDSA");
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
					}
					if (!this.IsADAM)
					{
						domainController = new DomainController(Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.DirectoryServer, this.context), searchResultPropertyValue);
					}
					else
					{
						int propertyValue = (int)PropertyManager.GetPropertyValue(this.context, directoryEntry1, PropertyManager.MsDSPortLDAP);
						string str2 = searchResultPropertyValue;
						if (propertyValue != 0x185)
						{
							str2 = string.Concat(searchResultPropertyValue, ":", propertyValue);
						}
						domainController = new AdamInstance(Utils.GetNewDirectoryContext(str2, DirectoryContextType.DirectoryServer, this.context), str2);
					}
					if (transport != ActiveDirectoryTransportType.Smtp)
					{
						this.RPCBridgeheadServers.Add(domainController);
					}
					else
					{
						this.SMTPBridgeheadServers.Add(domainController);
					}
				}
			}
			finally
			{
				directoryEntry.Dispose();
				searchResultCollections.Dispose();
			}
		}

		private void GetServers()
		{
			DirectoryServer domainController;
			string[] strArrays = new string[1];
			strArrays[0] = "dNSHostName";
			ADSearcher aDSearcher = new ADSearcher(this.cachedEntry, "(&(objectClass=server)(objectCategory=server))", strArrays, SearchScope.Subtree);
			SearchResultCollection searchResultCollections = null;
			try
			{
				searchResultCollections = aDSearcher.FindAll();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			try
			{
				foreach (SearchResult searchResult in searchResultCollections)
				{
					string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsHostName);
					DirectoryEntry directoryEntry = searchResult.GetDirectoryEntry();
					DirectoryEntry directoryEntry1 = null;
					try
					{
						directoryEntry1 = directoryEntry.Children.Find("CN=NTDS Settings", "nTDSDSA");
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						if (cOMException2.ErrorCode != -2147016656)
						{
							throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException2);
						}
						else
						{
							continue;
						}
					}
					if (!this.IsADAM)
					{
						domainController = new DomainController(Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.DirectoryServer, this.context), searchResultPropertyValue);
					}
					else
					{
						int propertyValue = (int)PropertyManager.GetPropertyValue(this.context, directoryEntry1, PropertyManager.MsDSPortLDAP);
						string str = searchResultPropertyValue;
						if (propertyValue != 0x185)
						{
							str = string.Concat(searchResultPropertyValue, ":", propertyValue);
						}
						domainController = new AdamInstance(Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, this.context), str);
					}
					this.servers.Add(domainController);
				}
			}
			finally
			{
				searchResultCollections.Dispose();
			}
		}

		private void GetSubnets()
		{
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
			string propertyValue = (string)PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ConfigurationNamingContext);
			string str = string.Concat("CN=Subnets,CN=Sites,", propertyValue);
			directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, str);
			string[] strArrays = new string[2];
			strArrays[0] = "cn";
			strArrays[1] = "location";
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, string.Concat("(&(objectClass=subnet)(objectCategory=subnet)(siteObject=", Utils.GetEscapedFilterValue((string)PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)), "))"), strArrays, SearchScope.OneLevel);
			SearchResultCollection searchResultCollections = null;
			try
			{
				searchResultCollections = aDSearcher.FindAll();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException);
			}
			try
			{
				foreach (SearchResult searchResult in searchResultCollections)
				{
					string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.Cn);
					ActiveDirectorySubnet activeDirectorySubnet = new ActiveDirectorySubnet(this.context, searchResultPropertyValue, null, true);
					activeDirectorySubnet.cachedEntry = searchResult.GetDirectoryEntry();
					activeDirectorySubnet.Site = this;
					this.subnets.Add(activeDirectorySubnet);
				}
			}
			finally
			{
				searchResultCollections.Dispose();
				directoryEntry.Dispose();
			}
		}

		public void Save()
		{
			string ntdsaObjectName;
			string str;
			if (!this.disposed)
			{
				try
				{
					this.cachedEntry.CommitChanges();
					foreach (DictionaryEntry subnet in this.subnets.changeList)
					{
						try
						{
							((DirectoryEntry)subnet.Value).CommitChanges();
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							if (cOMException.ErrorCode != -2147016694)
							{
								throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
							}
						}
					}
					this.subnets.changeList.Clear();
					this.subnetRetrieved = false;
					foreach (DictionaryEntry sMTPBridgeheadServer in this.SMTPBridgeheadServers.changeList)
					{
						try
						{
							((DirectoryEntry)sMTPBridgeheadServer.Value).CommitChanges();
						}
						catch (COMException cOMException3)
						{
							COMException cOMException2 = cOMException3;
							if (!this.IsADAM || cOMException2.ErrorCode != -2147016657)
							{
								if (cOMException2.ErrorCode != -2147016694)
								{
									throw ExceptionHelper.GetExceptionFromCOMException(cOMException2);
								}
							}
							else
							{
								throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
							}
						}
					}
					this.SMTPBridgeheadServers.changeList.Clear();
					this.SMTPBridgeRetrieved = false;
					foreach (DictionaryEntry rPCBridgeheadServer in this.RPCBridgeheadServers.changeList)
					{
						try
						{
							((DirectoryEntry)rPCBridgeheadServer.Value).CommitChanges();
						}
						catch (COMException cOMException5)
						{
							COMException cOMException4 = cOMException5;
							if (cOMException4.ErrorCode != -2147016694)
							{
								throw ExceptionHelper.GetExceptionFromCOMException(cOMException4);
							}
						}
					}
					this.RPCBridgeheadServers.changeList.Clear();
					this.RPCBridgeRetrieved = false;
					if (!this.existing)
					{
						try
						{
							DirectoryEntry directoryEntry = this.cachedEntry.Children.Add("CN=NTDS Site Settings", "nTDSSiteSettings");
							DirectoryServer interSiteTopologyGenerator = this.InterSiteTopologyGenerator;
							if (interSiteTopologyGenerator != null)
							{
								if (interSiteTopologyGenerator as DomainController != null)
								{
									ntdsaObjectName = ((DomainController)interSiteTopologyGenerator).NtdsaObjectName;
								}
								else
								{
									ntdsaObjectName = ((AdamInstance)interSiteTopologyGenerator).NtdsaObjectName;
								}
								string str1 = ntdsaObjectName;
								directoryEntry.Properties["interSiteTopologyGenerator"].Value = str1;
							}
							directoryEntry.Properties["options"].Value = this.siteOptions;
							if (this.replicationSchedule != null)
							{
								directoryEntry.Properties["schedule"].Value = this.replicationSchedule;
							}
							directoryEntry.CommitChanges();
							this.ntdsEntry = directoryEntry;
							directoryEntry = this.cachedEntry.Children.Add("CN=Servers", "serversContainer");
							directoryEntry.CommitChanges();
							if (!this.IsADAM)
							{
								directoryEntry = this.cachedEntry.Children.Add("CN=Licensing Site Settings", "licensingSiteSettings");
								directoryEntry.CommitChanges();
							}
						}
						finally
						{
							this.existing = true;
						}
					}
					else
					{
						if (this.topologyTouched)
						{
							try
							{
								DirectoryServer directoryServer = this.InterSiteTopologyGenerator;
								if (directoryServer as DomainController != null)
								{
									str = ((DomainController)directoryServer).NtdsaObjectName;
								}
								else
								{
									str = ((AdamInstance)directoryServer).NtdsaObjectName;
								}
								string str2 = str;
								this.NTDSSiteEntry.Properties["interSiteTopologyGenerator"].Value = str2;
							}
							catch (COMException cOMException7)
							{
								COMException cOMException6 = cOMException7;
								throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException6);
							}
						}
						this.NTDSSiteEntry.CommitChanges();
						this.topologyTouched = false;
					}
				}
				catch (COMException cOMException9)
				{
					COMException cOMException8 = cOMException9;
					throw ExceptionHelper.GetExceptionFromCOMException(this.context, cOMException8);
				}
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override string ToString()
		{
			if (!this.disposed)
			{
				return this.name;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		private static void ValidateArgument(DirectoryContext context, string siteName)
		{
			if (context != null)
			{
				if (context.Name != null || context.isRootDomain())
				{
					if (context.Name == null || context.isRootDomain() || context.isServer() || context.isADAMConfigSet())
					{
						if (siteName != null)
						{
							if (siteName.Length != 0)
							{
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
	}
}