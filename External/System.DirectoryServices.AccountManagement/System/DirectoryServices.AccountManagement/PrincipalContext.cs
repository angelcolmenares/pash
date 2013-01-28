using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
	public class PrincipalContext : IDisposable
	{
		private bool initialized;

		private object initializationLock;

		private bool disposed;

		private string username;

		private string password;

		private CredentialValidator credValidate;

		private ServerProperties serverProperties;

		private string name;

		private string container;

		private ContextOptions options;

		private ContextType contextType;

		private string connectedServer;

		private StoreCtx userCtx;

		private StoreCtx computerCtx;

		private StoreCtx groupCtx;

		private StoreCtx queryCtx;

		public string ConnectedServer
		{
			get
			{
				this.CheckDisposed();
				this.Initialize();
				return this.connectedServer;
			}
		}

		public string Container
		{
			get
			{
				this.CheckDisposed();
				return this.container;
			}
		}

		public ContextType ContextType
		{
			get
			{
				this.CheckDisposed();
				return this.contextType;
			}
		}

		internal bool Disposed
		{
			get
			{
				return this.disposed;
			}
		}

		public string Name
		{
			get
			{
				this.CheckDisposed();
				return this.name;
			}
		}

		public ContextOptions Options
		{
			get
			{
				this.CheckDisposed();
				return this.options;
			}
		}

		internal StoreCtx QueryCtx
		{
			[SecuritySafeCritical]
			get
			{
				this.Initialize();
				return this.queryCtx;
			}
			set
			{
				this.queryCtx = value;
			}
		}

		internal ServerProperties ServerInformation
		{
			get
			{
				return this.serverProperties;
			}
		}

		public string UserName
		{
			get
			{
				this.CheckDisposed();
				return this.username;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public PrincipalContext(ContextType contextType) : this(contextType, null, null, PrincipalContext.GetDefaultOptionForStore(contextType), null, null)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public PrincipalContext(ContextType contextType, string name) : this(contextType, name, null, PrincipalContext.GetDefaultOptionForStore(contextType), null, null)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public PrincipalContext(ContextType contextType, string name, string container) : this(contextType, name, container, PrincipalContext.GetDefaultOptionForStore(contextType), null, null)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public PrincipalContext(ContextType contextType, string name, string container, ContextOptions options) : this(contextType, name, container, options, null, null)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public PrincipalContext(ContextType contextType, string name, string userName, string password) : this(contextType, name, null, PrincipalContext.GetDefaultOptionForStore(contextType), userName, password)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public PrincipalContext(ContextType contextType, string name, string container, string userName, string password) : this(contextType, name, container, PrincipalContext.GetDefaultOptionForStore(contextType), userName, password)
		{
		}

		[DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
		public PrincipalContext(ContextType contextType, string name, string container, ContextOptions options, string userName, string password)
		{
			this.initializationLock = new object();
			if ((userName != null || password == null) && (userName == null || password != null))
			{
				if ((options & -64) == 0)
				{
					if (contextType != ContextType.Machine || (options & (ContextOptions.SimpleBind | ContextOptions.SecureSocketLayer | ContextOptions.Signing | ContextOptions.Sealing | ContextOptions.ServerBind)) == 0)
					{
						if ((contextType == ContextType.Domain || contextType == ContextType.ApplicationDirectory) && ((options & (ContextOptions.Negotiate | ContextOptions.SimpleBind)) == 0 || (options & (ContextOptions.Negotiate | ContextOptions.SimpleBind)) == (ContextOptions.Negotiate | ContextOptions.SimpleBind)))
						{
							throw new ArgumentException(StringResources.InvalidContextOptionsForAD);
						}
						else
						{
							if (contextType == ContextType.Machine || contextType == ContextType.Domain || contextType == ContextType.ApplicationDirectory)
							{
								if (contextType != ContextType.Machine || container == null)
								{
									if (contextType != ContextType.ApplicationDirectory || !string.IsNullOrEmpty(container) && !string.IsNullOrEmpty(name))
									{
										this.contextType = contextType;
										this.name = name;
										this.container = container;
										this.options = options;
										this.username = userName;
										this.password = password;
										this.DoServerVerifyAndPropRetrieval();
										this.credValidate = new CredentialValidator(contextType, name, this.serverProperties);
										return;
									}
									else
									{
										throw new ArgumentException(StringResources.ContextNoContainerForApplicationDirectoryCtx);
									}
								}
								else
								{
									throw new ArgumentException(StringResources.ContextNoContainerForMachineCtx);
								}
							}
							else
							{
								throw new InvalidEnumArgumentException("contextType", contextType, typeof(ContextType));
							}
						}
					}
					else
					{
						throw new ArgumentException(StringResources.InvalidContextOptionsForMachine);
					}
				}
				else
				{
					throw new InvalidEnumArgumentException("options", options, typeof(ContextOptions));
				}
			}
			else
			{
				throw new ArgumentException(StringResources.ContextBadUserPwdCombo);
			}
		}

		internal void CheckDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException("PrincipalContext");
			}
		}

		internal StoreCtx ContextForType(Type t)
		{
			this.Initialize();
			if (t == typeof(UserPrincipal) || t.IsSubclassOf(typeof(UserPrincipal)))
			{
				return this.userCtx;
			}
			else
			{
				if (t == typeof(ComputerPrincipal) || t.IsSubclassOf(typeof(ComputerPrincipal)))
				{
					return this.computerCtx;
				}
				else
				{
					if (t == typeof(AuthenticablePrincipal) || t.IsSubclassOf(typeof(AuthenticablePrincipal)))
					{
						return this.userCtx;
					}
					else
					{
						return this.groupCtx;
					}
				}
			}
		}

		private StoreCtx CreateContextFromDirectoryEntry(DirectoryEntry entry)
		{
			StoreCtx sAMStoreCtx;
			if (!entry.Path.StartsWith("LDAP:", StringComparison.Ordinal))
			{
				sAMStoreCtx = new SAMStoreCtx(entry, true, this.username, this.password, this.options);
			}
			else
			{
				if (this.ContextType != ContextType.ApplicationDirectory)
				{
					sAMStoreCtx = new ADStoreCtx(entry, true, this.username, this.password, this.options);
				}
				else
				{
					sAMStoreCtx = new ADAMStoreCtx(entry, true, this.username, this.password, this.name, this.options);
				}
			}
			sAMStoreCtx.OwningContext = this;
			return sAMStoreCtx;
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				if (this.userCtx != null)
				{
					this.userCtx.Dispose();
				}
				if (this.groupCtx != null)
				{
					this.groupCtx.Dispose();
				}
				if (this.computerCtx != null)
				{
					this.computerCtx.Dispose();
				}
				if (this.queryCtx != null)
				{
					this.queryCtx.Dispose();
				}
				this.disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		private void DoApplicationDirectoryInit()
		{
			if (this.container != null)
			{
				this.DoLDAPDirectoryInit();
				return;
			}
			else
			{
				this.DoLDAPDirectoryInitNoContainer();
				return;
			}
		}

		private void DoDomainInit()
		{
			if (this.container != null)
			{
				this.DoLDAPDirectoryInit();
				return;
			}
			else
			{
				this.DoLDAPDirectoryInitNoContainer();
				return;
			}
		}

		private void DoLDAPDirectoryInit()
		{
			int num;
			string str = "";
			if (this.name != null)
			{
				if (this.contextType != ContextType.ApplicationDirectory)
				{
					str = this.name;
				}
				else
				{
					string str1 = this.serverProperties.dnsHostName;
					string str2 = ":";
					if ((ContextOptions.SecureSocketLayer & this.options) > 0)
					{
						num = this.serverProperties.portSSL;
					}
					else
					{
						num = this.serverProperties.portLDAP;
					}
					str = string.Concat(str1, str2, num);
				}
				str = string.Concat(str, "/");
			}
			AuthenticationTypes authTypes = SDSUtils.MapOptionsToAuthTypes(this.options);
			using (DirectoryEntry directoryEntry = new DirectoryEntry(string.Concat("LDAP://", str, this.container), this.username, this.password, authTypes))
			{
				try
				{
					if (this.serverProperties.portSSL > 0)
					{
						directoryEntry.Options.PasswordPort = this.serverProperties.portSSL;
					}
					StoreCtx storeCtx = this.CreateContextFromDirectoryEntry(directoryEntry);
					this.queryCtx = storeCtx;
					this.userCtx = storeCtx;
					this.groupCtx = storeCtx;
					this.computerCtx = storeCtx;
					this.connectedServer = ADUtils.GetServerName(directoryEntry);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
				catch (Exception exception)
				{
					throw;
				}
			}
		}

		private void DoLDAPDirectoryInitNoContainer()
		{
			string str = null;
			string str1 = null;
			byte[] numArray = new byte[] { 169, 209, 202, 21, 118, 136, 17, 209, 173, 237, 0, 192, 79, 216, 213, 205 };
			byte[] numArray1 = numArray;
			byte[] numArray2 = new byte[] { 170, 49, 40, 37, 118, 136, 17, 209, 173, 237, 0, 192, 79, 216, 213, 205 };
			byte[] numArray3 = numArray2;
			DirectoryEntry directoryEntry = null;
			DirectoryEntry directoryEntry1 = null;
			DirectoryEntry directoryEntry2 = null;
			ADStoreCtx aDStoreCtx = null;
			ADStoreCtx aDStoreCtx1 = null;
			ADStoreCtx aDStoreCtx2 = null;
			DirectoryEntry directoryEntry3 = null;
			string str2 = "";
			if (this.name != null)
			{
				str2 = string.Concat(this.name, "/");
			}
			AuthenticationTypes authTypes = SDSUtils.MapOptionsToAuthTypes(this.options);
			using (directoryEntry3)
			{
				directoryEntry3 = new DirectoryEntry(string.Concat("LDAP://", str2, "rootDse"), this.username, this.password, authTypes);
				string item = (string)directoryEntry3.Properties["defaultNamingContext"][0];
				str = string.Concat("LDAP://", str2, item);
			}
			try
			{
				directoryEntry2 = new DirectoryEntry(str, this.username, this.password, authTypes);
				if (this.serverProperties.portSSL > 0)
				{
					directoryEntry2.Options.PasswordPort = this.serverProperties.portSSL;
				}
				string str3 = null;
				PropertyValueCollection propertyValueCollection = directoryEntry2.Properties["wellKnownObjects"];
				foreach (UnsafeNativeMethods.IADsDNWithBinary aDsDNWithBinary in propertyValueCollection)
				{
					if (Utils.AreBytesEqual(numArray1, (byte[])aDsDNWithBinary.BinaryValue))
					{
						str3 = string.Concat("LDAP://", str2, aDsDNWithBinary.DNString);
					}
					if (!Utils.AreBytesEqual(numArray3, (byte[])aDsDNWithBinary.BinaryValue))
					{
						continue;
					}
					str1 = string.Concat("LDAP://", str2, aDsDNWithBinary.DNString);
				}
				if (str3 == null || str1 == null)
				{
					throw new PrincipalOperationException(StringResources.ContextNoWellKnownObjects);
				}
				else
				{
					directoryEntry = new DirectoryEntry(str3, this.username, this.password, authTypes);
					directoryEntry1 = new DirectoryEntry(str1, this.username, this.password, authTypes);
					StoreCtx storeCtx = this.CreateContextFromDirectoryEntry(directoryEntry);
					this.userCtx = storeCtx;
					this.groupCtx = storeCtx;
					directoryEntry = null;
					this.computerCtx = this.CreateContextFromDirectoryEntry(directoryEntry1);
					directoryEntry1 = null;
					this.queryCtx = this.CreateContextFromDirectoryEntry(directoryEntry2);
					this.connectedServer = ADUtils.GetServerName(directoryEntry2);
					directoryEntry2 = null;
				}
			}
			catch (Exception exception)
			{
				if (directoryEntry != null)
				{
					directoryEntry.Dispose();
				}
				if (directoryEntry1 != null)
				{
					directoryEntry1.Dispose();
				}
				if (directoryEntry2 != null)
				{
					directoryEntry2.Dispose();
				}
				if (aDStoreCtx != null)
				{
					aDStoreCtx.Dispose();
				}
				if (aDStoreCtx1 != null)
				{
					aDStoreCtx1.Dispose();
				}
				if (aDStoreCtx2 != null)
				{
					aDStoreCtx2.Dispose();
				}
				throw;
			}
		}

		private void DoMachineInit()
		{
			DirectoryEntry directoryEntry = null;
			try
			{
				string computerFlatName = this.name;
				if (computerFlatName == null)
				{
					computerFlatName = Utils.GetComputerFlatName();
				}
				AuthenticationTypes authTypes = SDSUtils.MapOptionsToAuthTypes(this.options);
				directoryEntry = new DirectoryEntry(string.Concat("WinNT://", computerFlatName, ",computer"), this.username, this.password, authTypes);
				directoryEntry.RefreshCache();
				StoreCtx storeCtx = this.CreateContextFromDirectoryEntry(directoryEntry);
				this.queryCtx = storeCtx;
				this.userCtx = storeCtx;
				this.groupCtx = storeCtx;
				this.computerCtx = storeCtx;
				this.connectedServer = computerFlatName;
				directoryEntry = null;
			}
			catch (Exception exception)
			{
				if (directoryEntry != null)
				{
					directoryEntry.Dispose();
				}
				throw;
			}
		}

		private void DoServerVerifyAndPropRetrieval()
		{
			this.serverProperties = new ServerProperties();
			if (this.contextType == ContextType.ApplicationDirectory || this.contextType == ContextType.Domain)
			{
				this.ReadServerConfig(this.name, ref this.serverProperties);
				if (this.serverProperties.contextType != this.contextType)
				{
					object[] str = new object[1];
					str[0] = this.serverProperties.contextType.ToString();
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.PassedContextTypeDoesNotMatchDetectedType, str));
				}
			}
		}

		private static ContextOptions GetDefaultOptionForStore(ContextType storeType)
		{
			if (storeType != ContextType.Machine)
			{
				return DefaultContextOptions.ADDefaultContextOption;
			}
			else
			{
				return DefaultContextOptions.MachineDefaultContextOption;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		private void Initialize()
		{
			if (!this.initialized)
			{
				lock (this.initializationLock)
				{
					if (!this.initialized)
					{
						ContextType contextType = this.contextType;
						switch (contextType)
						{
							case ContextType.Machine:
							{
								this.DoMachineInit();
								break;
							}
							case ContextType.Domain:
							{
								this.DoDomainInit();
								break;
							}
							case ContextType.ApplicationDirectory:
							{
								this.DoApplicationDirectoryInit();
								break;
							}
						}
						this.initialized = true;
					}
				}
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		internal void ReadServerConfig(string serverName, ref ServerProperties properties)
		{
			string[] strArrays = new string[5];
			strArrays[0] = "msDS-PortSSL";
			strArrays[1] = "msDS-PortLDAP";
			strArrays[2] = "domainControllerFunctionality";
			strArrays[3] = "dnsHostName";
			strArrays[4] = "supportedCapabilities";
			string[] strArrays1 = strArrays;
			LdapConnection ldapConnection = null;
			using (ldapConnection)
			{
				bool flag = (this.options & ContextOptions.SecureSocketLayer) > 0;
				if (!flag || this.contextType != ContextType.Domain)
				{
					ldapConnection = new LdapConnection(serverName);
				}
				else
				{
					LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(serverName, LdapConstants.LDAP_SSL_PORT);
					ldapConnection = new LdapConnection(ldapDirectoryIdentifier);
				}
				ldapConnection.AutoBind = false;
				ldapConnection.SessionOptions.SecureSocketLayer = flag;
				string str = null;
				string str1 = "(objectClass=*)";
				SearchResponse searchResponse = null;
				SearchRequest searchRequest = new SearchRequest(str, str1, SearchScope.Base, strArrays1);
				try
				{
					searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);
				}
				catch (LdapException ldapException1)
				{
					LdapException ldapException = ldapException1;
					throw new PrincipalServerDownException(StringResources.ServerDown, ldapException);
				}
				properties.dnsHostName = (string)searchResponse.Entries[0].Attributes["dnsHostName"][0];
				properties.SupportCapabilities = new string[searchResponse.Entries[0].Attributes["supportedCapabilities"].Count];
				for (int i = 0; i < searchResponse.Entries[0].Attributes["supportedCapabilities"].Count; i++)
				{
					properties.SupportCapabilities[i] = (string)searchResponse.Entries[0].Attributes["supportedCapabilities"][i];
				}
				string[] supportCapabilities = properties.SupportCapabilities;
				for (int j = 0; j < (int)supportCapabilities.Length; j++)
				{
					string str2 = supportCapabilities[j];
					if ("1.2.840.113556.1.4.1851" != str2)
					{
						if ("1.2.840.113556.1.4.800" == str2)
						{
							properties.contextType = ContextType.Domain;
						}
					}
					else
					{
						properties.contextType = ContextType.ApplicationDirectory;
					}
				}
				if (!searchResponse.Entries[0].Attributes.Contains("domainControllerFunctionality"))
				{
					properties.OsVersion = DomainControllerMode.Win2k;
				}
				else
				{
					properties.OsVersion = (DomainControllerMode)Convert.ToInt32(searchResponse.Entries[0].Attributes["domainControllerFunctionality"][0], CultureInfo.InvariantCulture);
				}
				if (properties.contextType == ContextType.ApplicationDirectory)
				{
					if (searchResponse.Entries[0].Attributes.Contains("msDS-PortSSL"))
					{
						properties.portSSL = Convert.ToInt32(searchResponse.Entries[0].Attributes["msDS-PortSSL"][0]);
					}
					if (searchResponse.Entries[0].Attributes.Contains("msDS-PortLDAP"))
					{
						properties.portLDAP = Convert.ToInt32(searchResponse.Entries[0].Attributes["msDS-PortLDAP"][0]);
					}
				}
			}
		}

		public bool ValidateCredentials(string userName, string password)
		{
			this.CheckDisposed();
			if ((userName != null || password == null) && (userName == null || password != null))
			{
				return this.credValidate.Validate(userName, password);
			}
			else
			{
				throw new ArgumentException(StringResources.ContextBadUserPwdCombo);
			}
		}

		public bool ValidateCredentials(string userName, string password, ContextOptions options)
		{
			this.CheckDisposed();
			if ((userName != null || password == null) && (userName == null || password != null))
			{
				if (options == ContextOptions.Negotiate || this.contextType != ContextType.Machine)
				{
					return this.credValidate.Validate(userName, password, options);
				}
				else
				{
					throw new ArgumentException(StringResources.ContextOptionsNotValidForMachineStore);
				}
			}
			else
			{
				throw new ArgumentException(StringResources.ContextBadUserPwdCombo);
			}
		}
	}
}