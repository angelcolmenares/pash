using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class ADStoreCtx : StoreCtx
	{
		private const int mappingIndex = 0;

		private const string SelfSddl = "S-1-5-10";

		private const string WorldSddl = "S-1-1-0";

		protected DirectoryEntry ctxBase;

		private object ctxBaseLock;

		private bool ownCtxBase;

		private bool disposed;

		protected NetCred credentials;

		protected AuthenticationTypes authTypes;

		protected ContextOptions contextOptions;

		protected object domainInfoLock;

		protected string domainFlatName;

		protected string domainDnsName;

		protected string forestDnsName;

		protected string userSuppliedServerName;

		protected string defaultNamingContext;

		protected string contextBasePartitionDN;

		protected string dnsHostName;

		protected ulong lockoutDuration;

		protected ADStoreCtx.StoreCapabilityMap storeCapability;

		private static object[,] filterPropertiesTableRaw;

		private static Hashtable filterPropertiesTable;

		private static object[,] propertyMappingTableRaw;

		private static Hashtable propertyMappingTableByProperty;

		private static Hashtable propertyMappingTableByLDAP;

		protected static Dictionary<string, bool> NonPresentAttrDefaultStateMapping;

		private static Hashtable propertyMappingTableByPropertyFull;

		protected static Dictionary<int, Dictionary<Type, StringCollection>> TypeToLdapPropListMap;

		private readonly static Guid ChangePasswordGuid;

		protected internal AuthenticationTypes AuthTypes
		{
			get
			{
				return this.authTypes;
			}
		}

		internal override string BasePath
		{
			get
			{
				return this.ctxBase.Path;
			}
		}

		internal string ContextBasePartitionDN
		{
			get
			{
				if (this.contextBasePartitionDN == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.contextBasePartitionDN == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.contextBasePartitionDN;
			}
		}

		protected internal NetCred Credentials
		{
			get
			{
				return this.credentials;
			}
		}

		internal string DefaultNamingContext
		{
			get
			{
				if (this.defaultNamingContext == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.defaultNamingContext == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.defaultNamingContext;
			}
		}

		internal string DnsDomainName
		{
			get
			{
				if (this.domainDnsName == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.domainDnsName == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.domainDnsName;
			}
		}

		internal string DnsForestName
		{
			get
			{
				if (this.forestDnsName == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.forestDnsName == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.forestDnsName;
			}
		}

		internal string DnsHostName
		{
			get
			{
				if (this.dnsHostName == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.dnsHostName == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.dnsHostName;
			}
		}

		private string FlatDomainName
		{
			get
			{
				if (this.domainFlatName == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.domainFlatName == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.domainFlatName;
			}
		}

		private ulong LockoutDuration
		{
			get
			{
				if (this.domainDnsName == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.domainDnsName == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.lockoutDuration;
			}
		}

		protected virtual int MappingTableIndex
		{
			get
			{
				return 0;
			}
		}

		internal override bool SupportsNativeMembershipTest
		{
			get
			{
				return true;
			}
		}

		internal override bool SupportsSearchNatively
		{
			get
			{
				return true;
			}
		}

		internal string UserSuppliedServerName
		{
			get
			{
				if (this.userSuppliedServerName == null)
				{
					lock (this.domainInfoLock)
					{
						if (this.userSuppliedServerName == null)
						{
							this.LoadDomainInfo();
						}
					}
				}
				return this.userSuppliedServerName;
			}
		}

		static ADStoreCtx()
		{
			object[,] filterConverterDelegate = new object[39, 3];
			filterConverterDelegate[0, 0] = typeof(DescriptionFilter);
			filterConverterDelegate[0, 1] = "description";
			filterConverterDelegate[0, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[1, 0] = typeof(DisplayNameFilter);
			filterConverterDelegate[1, 1] = "displayName";
			filterConverterDelegate[1, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[2, 0] = typeof(IdentityClaimFilter);
			filterConverterDelegate[2, 1] = "";
			filterConverterDelegate[2, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.IdentityClaimConverter);
			filterConverterDelegate[3, 0] = typeof(SamAccountNameFilter);
			filterConverterDelegate[3, 1] = "sAMAccountName";
			filterConverterDelegate[3, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[4, 0] = typeof(DistinguishedNameFilter);
			filterConverterDelegate[4, 1] = "distinguishedName";
			filterConverterDelegate[4, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[5, 0] = typeof(GuidFilter);
			filterConverterDelegate[5, 1] = "objectGuid";
			filterConverterDelegate[5, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.GuidConverter);
			filterConverterDelegate[6, 0] = typeof(UserPrincipalNameFilter);
			filterConverterDelegate[6, 1] = "userPrincipalName";
			filterConverterDelegate[6, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[7, 0] = typeof(StructuralObjectClassFilter);
			filterConverterDelegate[7, 1] = "objectClass";
			filterConverterDelegate[7, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[8, 0] = typeof(NameFilter);
			filterConverterDelegate[8, 1] = "name";
			filterConverterDelegate[8, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[9, 0] = typeof(CertificateFilter);
			filterConverterDelegate[9, 1] = "";
			filterConverterDelegate[9, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.CertificateConverter);
			filterConverterDelegate[10, 0] = typeof(AuthPrincEnabledFilter);
			filterConverterDelegate[10, 1] = "userAccountControl";
			filterConverterDelegate[10, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[11, 0] = typeof(PermittedWorkstationFilter);
			filterConverterDelegate[11, 1] = "userWorkstations";
			filterConverterDelegate[11, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.CommaStringConverter);
			filterConverterDelegate[12, 0] = typeof(PermittedLogonTimesFilter);
			filterConverterDelegate[12, 1] = "logonHours";
			filterConverterDelegate[12, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.BinaryConverter);
			filterConverterDelegate[13, 0] = typeof(ExpirationDateFilter);
			filterConverterDelegate[13, 1] = "accountExpires";
			filterConverterDelegate[13, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.ExpirationDateConverter);
			filterConverterDelegate[14, 0] = typeof(SmartcardLogonRequiredFilter);
			filterConverterDelegate[14, 1] = "userAccountControl";
			filterConverterDelegate[14, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[15, 0] = typeof(DelegationPermittedFilter);
			filterConverterDelegate[15, 1] = "userAccountControl";
			filterConverterDelegate[15, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[16, 0] = typeof(HomeDirectoryFilter);
			filterConverterDelegate[16, 1] = "homeDirectory";
			filterConverterDelegate[16, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[17, 0] = typeof(HomeDriveFilter);
			filterConverterDelegate[17, 1] = "homeDrive";
			filterConverterDelegate[17, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[18, 0] = typeof(ScriptPathFilter);
			filterConverterDelegate[18, 1] = "scriptPath";
			filterConverterDelegate[18, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[19, 0] = typeof(PasswordNotRequiredFilter);
			filterConverterDelegate[19, 1] = "userAccountControl";
			filterConverterDelegate[19, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[20, 0] = typeof(PasswordNeverExpiresFilter);
			filterConverterDelegate[20, 1] = "userAccountControl";
			filterConverterDelegate[20, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[21, 0] = typeof(CannotChangePasswordFilter);
			filterConverterDelegate[21, 1] = "userAccountControl";
			filterConverterDelegate[21, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[22, 0] = typeof(AllowReversiblePasswordEncryptionFilter);
			filterConverterDelegate[22, 1] = "userAccountControl";
			filterConverterDelegate[22, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[23, 0] = typeof(GivenNameFilter);
			filterConverterDelegate[23, 1] = "givenName";
			filterConverterDelegate[23, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[24, 0] = typeof(MiddleNameFilter);
			filterConverterDelegate[24, 1] = "middleName";
			filterConverterDelegate[24, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[25, 0] = typeof(SurnameFilter);
			filterConverterDelegate[25, 1] = "sn";
			filterConverterDelegate[25, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[26, 0] = typeof(EmailAddressFilter);
			filterConverterDelegate[26, 1] = "mail";
			filterConverterDelegate[26, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[27, 0] = typeof(VoiceTelephoneNumberFilter);
			filterConverterDelegate[27, 1] = "telephoneNumber";
			filterConverterDelegate[27, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[28, 0] = typeof(EmployeeIDFilter);
			filterConverterDelegate[28, 1] = "employeeID";
			filterConverterDelegate[28, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[29, 0] = typeof(GroupIsSecurityGroupFilter);
			filterConverterDelegate[29, 1] = "groupType";
			filterConverterDelegate[29, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.GroupTypeConverter);
			filterConverterDelegate[30, 0] = typeof(GroupScopeFilter);
			filterConverterDelegate[30, 1] = "groupType";
			filterConverterDelegate[30, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.GroupTypeConverter);
			filterConverterDelegate[31, 0] = typeof(ServicePrincipalNameFilter);
			filterConverterDelegate[31, 1] = "servicePrincipalName";
			filterConverterDelegate[31, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[32, 0] = typeof(ExtensionCacheFilter);
			filterConverterDelegate[32, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.ExtensionCacheConverter);
			filterConverterDelegate[33, 0] = typeof(BadPasswordAttemptFilter);
			filterConverterDelegate[33, 1] = "badPasswordTime";
			filterConverterDelegate[33, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValutMatchingDateTimeConverter);
			filterConverterDelegate[34, 0] = typeof(ExpiredAccountFilter);
			filterConverterDelegate[34, 1] = "accountExpires";
			filterConverterDelegate[34, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.MatchingDateTimeConverter);
			filterConverterDelegate[35, 0] = typeof(LastLogonTimeFilter);
			filterConverterDelegate[35, 1] = "lastLogon";
			filterConverterDelegate[35, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.LastLogonConverter);
			filterConverterDelegate[36, 0] = typeof(LockoutTimeFilter);
			filterConverterDelegate[36, 1] = "lockoutTime";
			filterConverterDelegate[36, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.MatchingDateTimeConverter);
			filterConverterDelegate[37, 0] = typeof(PasswordSetTimeFilter);
			filterConverterDelegate[37, 1] = "pwdLastSet";
			filterConverterDelegate[37, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValutMatchingDateTimeConverter);
			filterConverterDelegate[38, 0] = typeof(BadLogonCountFilter);
			filterConverterDelegate[38, 1] = "badPwdCount";
			filterConverterDelegate[38, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.MatchingIntConverter);
			ADStoreCtx.filterPropertiesTableRaw = filterConverterDelegate;
			ADStoreCtx.filterPropertiesTable = null;
			object[,] fromLdapConverterDelegate = new object[39, 4];
			fromLdapConverterDelegate[0, 0] = "Principal.DisplayName";
			fromLdapConverterDelegate[0, 1] = "displayname";
			fromLdapConverterDelegate[0, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[0, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[1, 0] = "Principal.Description";
			fromLdapConverterDelegate[1, 1] = "description";
			fromLdapConverterDelegate[1, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[1, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[2, 0] = "Principal.DistinguishedName";
			fromLdapConverterDelegate[2, 1] = "distinguishedname";
			fromLdapConverterDelegate[2, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[2, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[3, 0] = "Principal.Sid";
			fromLdapConverterDelegate[3, 1] = "objectsid";
			fromLdapConverterDelegate[3, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.SidFromLdapConverter);
			fromLdapConverterDelegate[4, 0] = "Principal.SamAccountName";
			fromLdapConverterDelegate[4, 1] = "samaccountname";
			fromLdapConverterDelegate[4, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[4, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[5, 0] = "Principal.UserPrincipalName";
			fromLdapConverterDelegate[5, 1] = "userprincipalname";
			fromLdapConverterDelegate[5, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[5, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[6, 0] = "Principal.Guid";
			fromLdapConverterDelegate[6, 1] = "objectguid";
			fromLdapConverterDelegate[6, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GuidFromLdapConverter);
			fromLdapConverterDelegate[7, 0] = "Principal.StructuralObjectClass";
			fromLdapConverterDelegate[7, 1] = "objectclass";
			fromLdapConverterDelegate[7, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.ObjectClassFromLdapConverter);
			fromLdapConverterDelegate[8, 0] = "Principal.Name";
			fromLdapConverterDelegate[8, 1] = "name";
			fromLdapConverterDelegate[8, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[8, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[9, 0] = "Principal.ExtensionCache";
			fromLdapConverterDelegate[9, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.ExtensionCacheToLdapConverter);
			fromLdapConverterDelegate[10, 0] = "AuthenticablePrincipal.Enabled";
			fromLdapConverterDelegate[10, 1] = "useraccountcontrol";
			fromLdapConverterDelegate[10, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.UACFromLdapConverter);
			fromLdapConverterDelegate[10, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.UACToLdapConverter);
			fromLdapConverterDelegate[11, 0] = "AuthenticablePrincipal.Certificates";
			fromLdapConverterDelegate[11, 1] = "usercertificate";
			fromLdapConverterDelegate[11, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.CertFromLdapConverter);
			fromLdapConverterDelegate[11, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.CertToLdap);
			fromLdapConverterDelegate[12, 0] = "GroupPrincipal.IsSecurityGroup";
			fromLdapConverterDelegate[12, 1] = "grouptype";
			fromLdapConverterDelegate[12, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GroupTypeFromLdapConverter);
			fromLdapConverterDelegate[12, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.GroupTypeToLdapConverter);
			fromLdapConverterDelegate[13, 0] = "GroupPrincipal.GroupScope";
			fromLdapConverterDelegate[13, 1] = "grouptype";
			fromLdapConverterDelegate[13, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GroupTypeFromLdapConverter);
			fromLdapConverterDelegate[13, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.GroupTypeToLdapConverter);
			fromLdapConverterDelegate[14, 0] = "UserPrincipal.GivenName";
			fromLdapConverterDelegate[14, 1] = "givenname";
			fromLdapConverterDelegate[14, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[14, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[15, 0] = "UserPrincipal.MiddleName";
			fromLdapConverterDelegate[15, 1] = "middlename";
			fromLdapConverterDelegate[15, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[15, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[16, 0] = "UserPrincipal.Surname";
			fromLdapConverterDelegate[16, 1] = "sn";
			fromLdapConverterDelegate[16, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[16, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[17, 0] = "UserPrincipal.EmailAddress";
			fromLdapConverterDelegate[17, 1] = "mail";
			fromLdapConverterDelegate[17, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[17, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[18, 0] = "UserPrincipal.VoiceTelephoneNumber";
			fromLdapConverterDelegate[18, 1] = "telephonenumber";
			fromLdapConverterDelegate[18, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[18, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[19, 0] = "UserPrincipal.EmployeeId";
			fromLdapConverterDelegate[19, 1] = "employeeid";
			fromLdapConverterDelegate[19, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[19, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[20, 0] = "ComputerPrincipal.ServicePrincipalNames";
			fromLdapConverterDelegate[20, 1] = "serviceprincipalname";
			fromLdapConverterDelegate[20, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.MultiStringFromLdapConverter);
			fromLdapConverterDelegate[20, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.MultiStringToLdapConverter);
			fromLdapConverterDelegate[21, 0] = "AuthenticablePrincipal.AccountInfo.AccountLockoutTime";
			fromLdapConverterDelegate[21, 1] = "lockouttime";
			fromLdapConverterDelegate[21, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GenericDateTimeFromLdapConverter);
			fromLdapConverterDelegate[22, 0] = "AuthenticablePrincipal.AccountInfo.LastLogon";
			fromLdapConverterDelegate[22, 1] = "lastlogon";
			fromLdapConverterDelegate[22, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.LastLogonFromLdapConverter);
			fromLdapConverterDelegate[23, 0] = "AuthenticablePrincipal.AccountInfo.LastLogon";
			fromLdapConverterDelegate[23, 1] = "lastlogontimestamp";
			fromLdapConverterDelegate[23, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.LastLogonFromLdapConverter);
			fromLdapConverterDelegate[24, 0] = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";
			fromLdapConverterDelegate[24, 1] = "userworkstations";
			fromLdapConverterDelegate[24, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.CommaStringFromLdapConverter);
			fromLdapConverterDelegate[24, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.CommaStringToLdapConverter);
			fromLdapConverterDelegate[25, 0] = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";
			fromLdapConverterDelegate[25, 1] = "logonhours";
			fromLdapConverterDelegate[25, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.BinaryFromLdapConverter);
			fromLdapConverterDelegate[25, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.BinaryToLdapConverter);
			fromLdapConverterDelegate[26, 0] = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";
			fromLdapConverterDelegate[26, 1] = "accountexpires";
			fromLdapConverterDelegate[26, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.AcctExpirFromLdapConverter);
			fromLdapConverterDelegate[26, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.AcctExpirToLdapConverter);
			fromLdapConverterDelegate[27, 0] = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";
			fromLdapConverterDelegate[27, 1] = "useraccountcontrol";
			fromLdapConverterDelegate[27, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.UACFromLdapConverter);
			fromLdapConverterDelegate[27, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.UACToLdapConverter);
			fromLdapConverterDelegate[28, 0] = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";
			fromLdapConverterDelegate[28, 1] = "useraccountcontrol";
			fromLdapConverterDelegate[28, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.UACFromLdapConverter);
			fromLdapConverterDelegate[28, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.UACToLdapConverter);
			fromLdapConverterDelegate[29, 0] = "AuthenticablePrincipal.AccountInfo.BadLogonCount";
			fromLdapConverterDelegate[29, 1] = "badpwdcount";
			fromLdapConverterDelegate[29, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.IntFromLdapConverter);
			fromLdapConverterDelegate[30, 0] = "AuthenticablePrincipal.AccountInfo.HomeDirectory";
			fromLdapConverterDelegate[30, 1] = "homedirectory";
			fromLdapConverterDelegate[30, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[30, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[31, 0] = "AuthenticablePrincipal.AccountInfo.HomeDrive";
			fromLdapConverterDelegate[31, 1] = "homedrive";
			fromLdapConverterDelegate[31, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[31, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[32, 0] = "AuthenticablePrincipal.AccountInfo.ScriptPath";
			fromLdapConverterDelegate[32, 1] = "scriptpath";
			fromLdapConverterDelegate[32, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[32, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[33, 0] = "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";
			fromLdapConverterDelegate[33, 1] = "pwdlastset";
			fromLdapConverterDelegate[33, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GenericDateTimeFromLdapConverter);
			fromLdapConverterDelegate[34, 0] = "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";
			fromLdapConverterDelegate[34, 1] = "badpasswordtime";
			fromLdapConverterDelegate[34, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GenericDateTimeFromLdapConverter);
			fromLdapConverterDelegate[35, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";
			fromLdapConverterDelegate[35, 1] = "useraccountcontrol";
			fromLdapConverterDelegate[35, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.UACFromLdapConverter);
			fromLdapConverterDelegate[35, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.UACToLdapConverter);
			fromLdapConverterDelegate[36, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";
			fromLdapConverterDelegate[36, 1] = "useraccountcontrol";
			fromLdapConverterDelegate[36, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.UACFromLdapConverter);
			fromLdapConverterDelegate[36, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.UACToLdapConverter);
			fromLdapConverterDelegate[37, 0] = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";
			fromLdapConverterDelegate[37, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.CannotChangePwdToLdapConverter);
			fromLdapConverterDelegate[38, 0] = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";
			fromLdapConverterDelegate[38, 1] = "useraccountcontrol";
			fromLdapConverterDelegate[38, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.UACFromLdapConverter);
			fromLdapConverterDelegate[38, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.UACToLdapConverter);
			ADStoreCtx.propertyMappingTableRaw = fromLdapConverterDelegate;
			ADStoreCtx.propertyMappingTableByProperty = null;
			ADStoreCtx.propertyMappingTableByLDAP = null;
			ADStoreCtx.NonPresentAttrDefaultStateMapping = null;
			ADStoreCtx.propertyMappingTableByPropertyFull = null;
			ADStoreCtx.TypeToLdapPropListMap = null;
			ADStoreCtx.ChangePasswordGuid = new Guid("{ab721a53-1e2f-11d0-9819-00aa0040529b}");
			ADStoreCtx.LoadFilterMappingTable(0, ADStoreCtx.filterPropertiesTableRaw);
			ADStoreCtx.LoadPropertyMappingTable(0, ADStoreCtx.propertyMappingTableRaw);
		}

		public ADStoreCtx(DirectoryEntry ctxBase, bool ownCtxBase, string username, string password, ContextOptions options)
		{
			this.ctxBaseLock = new object();
			this.domainInfoLock = new object();
			if (this.IsContainer(ctxBase))
			{
				this.ctxBase = ctxBase;
				this.ownCtxBase = ownCtxBase;
				if (username != null && password != null)
				{
					this.credentials = new NetCred(username, password);
				}
				this.contextOptions = options;
				this.authTypes = SDSUtils.MapOptionsToAuthTypes(options);
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.ADStoreCtxMustBeContainer);
			}
		}

		internal override bool AccessCheck(Principal p, PrincipalAccessMask targetPermission)
		{
			PrincipalAccessMask principalAccessMask = targetPermission;
			if (principalAccessMask != PrincipalAccessMask.ChangePassword)
			{
				return false;
			}
			else
			{
				return ADStoreCtx.CannotChangePwdFromLdapConverter((DirectoryEntry)p.GetUnderlyingObject());
			}
		}

		protected static string AcctDisabledConverter(FilterBase filter, string suggestedAdProperty)
		{
			string str;
			StringBuilder stringBuilder = new StringBuilder();
			if (filter.Value == null)
			{
				stringBuilder.Append("(!(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=*))");
			}
			else
			{
				stringBuilder.Append("(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=");
				StringBuilder stringBuilder1 = stringBuilder;
				if (!(bool)filter.Value)
				{
					str = "TRUE";
				}
				else
				{
					str = "FALSE";
				}
				stringBuilder1.Append(str);
				stringBuilder.Append(")");
			}
			return stringBuilder.ToString();
		}

		protected static void AcctDisabledFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			if (properties[suggestedAdProperty].Count > 0)
			{
				p.LoadValueIntoProperty(propertyName, !(bool)properties[suggestedAdProperty][0]);
			}
		}

		protected static void AcctDisabledToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			if (!p.unpersisted)
			{
				object valueForProperty = (bool)p.GetValueForProperty(propertyName);
				if (valueForProperty == null)
				{
					de.Properties[suggestedAdProperty].Value = null;
				}
				else
				{
					de.Properties[suggestedAdProperty].Value = !(bool)valueForProperty;
					return;
				}
			}
		}

		protected static void AcctExpirFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			ADStoreCtx.DateTimeFromLdapConverter(properties, suggestedAdProperty, p, propertyName, true);
		}

		protected static void AcctExpirToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			DateTime? valueForProperty = (DateTime?)p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty.HasValue)
			{
				UnsafeNativeMethods.ADsLargeInteger aDsLargeInteger = new UnsafeNativeMethods.ADsLargeInteger();
				UnsafeNativeMethods.IADsLargeInteger aDsLargeInteger1 = (UnsafeNativeMethods.IADsLargeInteger)aDsLargeInteger;
				if (valueForProperty.HasValue)
				{
					long aDFileTime = ADUtils.DateTimeToADFileTime(valueForProperty.Value);
					uint num = (uint)(aDFileTime & (long)-1);
					uint num1 = (uint)((aDFileTime & -4294967296L) >> 32);
					aDsLargeInteger1.LowPart = num;
					aDsLargeInteger1.HighPart = num1;
				}
				else
				{
					aDsLargeInteger1.LowPart = -1;
					aDsLargeInteger1.HighPart = 0x7fffffff;
				}
				de.Properties[suggestedAdProperty].Value = aDsLargeInteger1;
				return;
			}
			else
			{
				return;
			}
		}

		private void AddPropertySetToTypePropListMap(Type principalType, StringCollection propertySet)
		{
			lock (ADStoreCtx.TypeToLdapPropListMap)
			{
				if (!ADStoreCtx.TypeToLdapPropListMap[this.MappingTableIndex].ContainsKey(principalType))
				{
					ADStoreCtx.TypeToLdapPropListMap[this.MappingTableIndex].Add(principalType, propertySet);
				}
			}
		}

		protected static string BinaryConverter(FilterBase filter, string suggestedAdProperty)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (filter.Value == null)
			{
				stringBuilder.Append("(!(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=*))");
			}
			else
			{
				stringBuilder.Append("(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=");
				stringBuilder.Append(ADUtils.EscapeBinaryValue((byte[])filter.Value));
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected static void BinaryFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.SingleScalarFromDirectoryEntry<byte[]>(properties, suggestedAdProperty, p, propertyName);
		}

		protected static void BinaryToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			byte[] valueForProperty = (byte[])p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty != null)
			{
				if (valueForProperty == null || (int)valueForProperty.Length == 0)
				{
					de.Properties[suggestedAdProperty].Value = null;
					return;
				}
				else
				{
					de.Properties[suggestedAdProperty].Value = valueForProperty;
					return;
				}
			}
			else
			{
				return;
			}
		}

		protected static void BoolFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.SingleScalarFromDirectoryEntry<bool>(properties, suggestedAdProperty, p, propertyName);
		}

		protected static void BoolToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			object valueForProperty = (bool)p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty != null)
			{
				if (valueForProperty == null)
				{
					de.Properties[suggestedAdProperty].Value = null;
					return;
				}
				else
				{
					de.Properties[suggestedAdProperty].Value = (bool)valueForProperty;
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void BuildExtensionPropertyList(Hashtable propertyList, Type p)
		{
			PropertyInfo[] properties = p.GetProperties();
			PropertyInfo[] propertyInfoArray = properties;
			for (int i = 0; i < (int)propertyInfoArray.Length; i++)
			{
				PropertyInfo propertyInfo = propertyInfoArray[i];
				DirectoryPropertyAttribute[] customAttributes = (DirectoryPropertyAttribute[])propertyInfo.GetCustomAttributes(typeof(DirectoryPropertyAttribute), true);
				DirectoryPropertyAttribute[] directoryPropertyAttributeArray = customAttributes;
				for (int j = 0; j < (int)directoryPropertyAttributeArray.Length; j++)
				{
					DirectoryPropertyAttribute directoryPropertyAttribute = directoryPropertyAttributeArray[j];
					if (!propertyList.Contains(directoryPropertyAttribute.SchemaAttributeName))
					{
						propertyList.Add(directoryPropertyAttribute.SchemaAttributeName, directoryPropertyAttribute.SchemaAttributeName);
					}
				}
			}
		}

		protected bool BuildLdapFilterFromIdentityClaim(string urnValue, string urnScheme, ref string filter, bool useSidHistory, bool throwOnFail)
		{
			IdentityClaim identityClaim = new IdentityClaim();
			identityClaim.UrnValue = urnValue;
			identityClaim.UrnScheme = urnScheme;
			IdentityClaimFilter identityClaimFilter = new IdentityClaimFilter();
			identityClaimFilter.Value = identityClaim;
			if (!useSidHistory)
			{
				if (!ADStoreCtx.IdentityClaimToFilter(urnValue, urnScheme, ref filter, throwOnFail))
				{
					return false;
				}
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (ADStoreCtx.SecurityIdentityClaimConverterHelper(urnValue, useSidHistory, stringBuilder, throwOnFail))
				{
					filter = stringBuilder.ToString();
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		protected void BuildPropertySet(Type p, StringCollection propertySet)
		{
			Type type;
			if (!ADStoreCtx.TypeToLdapPropListMap[this.MappingTableIndex].ContainsKey(p))
			{
				if (!p.IsSubclassOf(typeof(UserPrincipal)))
				{
					if (!p.IsSubclassOf(typeof(GroupPrincipal)))
					{
						if (!p.IsSubclassOf(typeof(ComputerPrincipal)))
						{
							if (!p.IsSubclassOf(typeof(AuthenticablePrincipal)))
							{
								type = typeof(Principal);
							}
							else
							{
								type = typeof(AuthenticablePrincipal);
							}
						}
						else
						{
							type = typeof(ComputerPrincipal);
						}
					}
					else
					{
						type = typeof(GroupPrincipal);
					}
				}
				else
				{
					type = typeof(UserPrincipal);
				}
				Hashtable hashtables = new Hashtable();
				foreach (string item in ADStoreCtx.TypeToLdapPropListMap[this.MappingTableIndex][type])
				{
					if (hashtables.Contains(item))
					{
						continue;
					}
					hashtables.Add(item, item);
				}
				this.BuildExtensionPropertyList(hashtables, p);
				foreach (string value in hashtables.Values)
				{
					propertySet.Add(value);
				}
				this.AddPropertySetToTypePropListMap(p, propertySet);
				return;
			}
			else
			{
				string[] strArrays = new string[ADStoreCtx.TypeToLdapPropListMap[this.MappingTableIndex][p].Count];
				ADStoreCtx.TypeToLdapPropListMap[this.MappingTableIndex][p].CopyTo(strArrays, 0);
				propertySet.AddRange(strArrays);
				return;
			}
		}

		internal override bool CanGroupBeCleared(GroupPrincipal g, out string explanationForFailure)
		{
			bool flag;
			explanationForFailure = null;
			if (g.unpersisted || g.fakePrincipal)
			{
				return true;
			}
			else
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)g.UnderlyingObject;
				DirectorySearcher directorySearcherFromGroupID = null;
				using (directorySearcherFromGroupID)
				{
					try
					{
						if (underlyingObject.Properties["objectSid"].Count <= 0)
						{
							flag = true;
						}
						else
						{
							byte[] item = (byte[])underlyingObject.Properties["objectSid"][0];
							directorySearcherFromGroupID = this.GetDirectorySearcherFromGroupID(item);
							directorySearcherFromGroupID.SizeLimit = 1;
							SearchResult searchResult = directorySearcherFromGroupID.FindOne();
							if (searchResult == null)
							{
								flag = true;
							}
							else
							{
								explanationForFailure = StringResources.ADStoreCtxCantClearGroup;
								flag = false;
							}
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
				}
				return flag;
			}
		}

		internal override bool CanGroupMemberBeRemoved(GroupPrincipal g, Principal member, out string explanationForFailure)
		{
			bool flag;
			explanationForFailure = null;
			if (member.unpersisted || member.fakePrincipal)
			{
				return true;
			}
			else
			{
				if (g.unpersisted || g.fakePrincipal)
				{
					return true;
				}
				else
				{
					if ((g.ContextType != ContextType.Domain || member.ContextType == ContextType.Domain) && (member.ContextType == ContextType.Domain || member.ContextType == ContextType.ApplicationDirectory))
					{
						try
						{
							DirectoryEntry underlyingObject = (DirectoryEntry)g.UnderlyingObject;
							DirectoryEntry directoryEntry = (DirectoryEntry)member.UnderlyingObject;
							if (underlyingObject.Properties["objectSid"].Count <= 0 || directoryEntry.Properties["primaryGroupID"].Count <= 0)
							{
								flag = true;
							}
							else
							{
								byte[] item = (byte[])underlyingObject.Properties["objectSid"][0];
								int num = (int)directoryEntry.Properties["primaryGroupID"][0];
								int lastRidFromSid = Utils.GetLastRidFromSid(item);
								if (lastRidFromSid != num)
								{
									flag = true;
								}
								else
								{
									explanationForFailure = StringResources.ADStoreCtxCantRemoveMemberFromGroup;
									flag = false;
								}
							}
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
						}
						return flag;
					}
					else
					{
						return true;
					}
				}
			}
		}

		protected static bool CannotChangePwdFromLdapConverter(DirectoryEntry de)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4;
			if (!de.Properties.Contains("nTSecurityDescriptor"))
			{
				string[] strArrays = new string[1];
				strArrays[0] = "nTSecurityDescriptor";
				de.RefreshCache(strArrays);
			}
			ActiveDirectorySecurity objectSecurity = de.ObjectSecurity;
			ADStoreCtx.ScanACLForChangePasswordRight(objectSecurity, out flag, out flag1, out flag2, out flag3);
			if (flag || flag1)
			{
				flag4 = true;
			}
			else
			{
				if (flag || flag1 || !flag2 && !flag3)
				{
					flag4 = false;
				}
				else
				{
					flag4 = false;
				}
			}
			return flag4;
		}

		protected static void CannotChangePwdToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			if (!p.unpersisted)
			{
				ADStoreCtx.SetCannotChangePasswordStatus(p, (bool)p.GetValueForProperty(propertyName), false);
				return;
			}
			else
			{
				return;
			}
		}

		protected static void CertFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.MultiScalarFromDirectoryEntry<byte[]>(properties, suggestedAdProperty, p, propertyName);
		}

		protected static string CertificateConverter(FilterBase filter, string suggestedAdProperty)
		{
			X509Certificate2 value = (X509Certificate2)filter.Value;
			byte[] rawData = value.RawData;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(userCertificate=");
			stringBuilder.Append(ADUtils.EscapeBinaryValue(rawData));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected static void CertToLdap(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			X509Certificate2Collection valueForProperty = (X509Certificate2Collection)p.GetValueForProperty(propertyName);
			if (valueForProperty.Count != 0)
			{
				byte[][] rawData = new byte[valueForProperty.Count][];
				for (int i = 0; i < valueForProperty.Count; i++)
				{
					rawData[i] = valueForProperty[i].RawData;
				}
				de.Properties[suggestedAdProperty].Value = null;
				de.Properties[suggestedAdProperty].Value = rawData;
				return;
			}
			else
			{
				de.Properties[suggestedAdProperty].Value = null;
				return;
			}
		}

		internal override void ChangePassword(AuthenticablePrincipal p, string oldPassword, string newPassword)
		{
			if (p.GetType() == typeof(ComputerPrincipal) || p.GetType().IsSubclassOf(typeof(ComputerPrincipal)))
			{
				throw new NotSupportedException(StringResources.ADStoreCtxNoComputerPasswordChange);
			}
			else
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				SDSUtils.ChangePassword(underlyingObject, oldPassword, newPassword);
				return;
			}
		}

		protected static string CommaStringConverter(FilterBase filter, string suggestedAdProperty)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (filter.Value == null)
			{
				stringBuilder.Append("(!(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=*))");
			}
			else
			{
				stringBuilder.Append("(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=*");
				stringBuilder.Append(ADUtils.PAPIQueryToLdapQueryString((string)filter.Value));
				stringBuilder.Append("*");
				stringBuilder.Append(")");
			}
			return stringBuilder.ToString();
		}

		protected static void CommaStringFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			dSPropertyValueCollection item = properties[suggestedAdProperty];
			if (item.Count != 0)
			{
				string str = (string)item[0];
				char[] chrArray = new char[1];
				chrArray[0] = ',';
				string[] strArrays = str.Split(chrArray);
				List<string> strs = new List<string>((int)strArrays.Length);
				string[] strArrays1 = strArrays;
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					string str1 = strArrays1[i];
					strs.Add(str1);
				}
				p.LoadValueIntoProperty(propertyName, strs);
			}
		}

		protected static void CommaStringToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			string str;
			PrincipalValueCollection<string> valueForProperty = (PrincipalValueCollection<string>)p.GetValueForProperty(propertyName);
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string str1 in valueForProperty)
			{
				stringBuilder.Append(str1);
				stringBuilder.Append(",");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			if (stringBuilder.Length > 0)
			{
				str = stringBuilder.ToString();
			}
			else
			{
				str = null;
			}
			string str2 = str;
			if (!p.unpersisted || str2 != null)
			{
				de.Properties[suggestedAdProperty].Value = str2;
				return;
			}
			else
			{
				return;
			}
		}

		internal override Principal ConstructFakePrincipalFromSID(byte[] sid)
		{
			UnsafeNativeMethods.IAdsObjectOptions nativeObject = (UnsafeNativeMethods.IAdsObjectOptions)this.ctxBase.NativeObject;
			string option = (string)nativeObject.GetOption(0);
			Principal principal = Utils.ConstructFakePrincipalFromSID(sid, base.OwningContext, option, this.credentials, this.DnsDomainName);
			ADStoreKey aDStoreKey = new ADStoreKey(this.DnsDomainName, sid);
			principal.Key = aDStoreKey;
			return principal;
		}

		public static string DateTimeFilterBuilder(string attributeName, DateTime searchValue, DateTime defaultValue, bool requirePresence, MatchType mt)
		{
			string str;
			bool flag = false;
			string aDString = ADUtils.DateTimeToADString(searchValue);
			string aDString1 = ADUtils.DateTimeToADString(defaultValue);
			StringBuilder stringBuilder = new StringBuilder("(");
			if (mt != MatchType.Equals && mt != MatchType.NotEquals)
			{
				flag = true;
			}
			if (flag || mt == MatchType.NotEquals && requirePresence)
			{
				stringBuilder.Append("&(");
			}
			MatchType matchType = mt;
			switch (matchType)
			{
				case MatchType.Equals:
				{
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=");
					stringBuilder.Append(aDString);
					break;
				}
				case MatchType.NotEquals:
				{
					stringBuilder.Append("!(");
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=");
					stringBuilder.Append(aDString);
					stringBuilder.Append(")");
					break;
				}
				case MatchType.GreaterThan:
				case MatchType.LessThan:
				{
					stringBuilder.Append("&");
					stringBuilder.Append("(");
					stringBuilder.Append(attributeName);
					StringBuilder stringBuilder1 = stringBuilder;
					if (mt == MatchType.GreaterThan)
					{
						str = ">=";
					}
					else
					{
						str = "<=";
					}
					stringBuilder1.Append(str);
					stringBuilder.Append(aDString);
					stringBuilder.Append(")");
					stringBuilder.Append("(!(");
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=");
					stringBuilder.Append(aDString);
					stringBuilder.Append("))");
					stringBuilder.Append("(");
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=*)");
					break;
				}
				case MatchType.GreaterThanOrEquals:
				{
					stringBuilder.Append(attributeName);
					stringBuilder.Append(">=");
					stringBuilder.Append(aDString);
					break;
				}
				case MatchType.LessThanOrEquals:
				{
					stringBuilder.Append(attributeName);
					stringBuilder.Append("<=");
					stringBuilder.Append(aDString);
					break;
				}
			}
			stringBuilder.Append(")");
			bool flag1 = false;
			if (flag)
			{
				stringBuilder.Append("(!");
				stringBuilder.Append(attributeName);
				stringBuilder.Append("=");
				stringBuilder.Append(aDString1);
				stringBuilder.Append(")");
				flag1 = true;
			}
			if (mt == MatchType.NotEquals && requirePresence)
			{
				stringBuilder.Append("(");
				stringBuilder.Append(attributeName);
				stringBuilder.Append("=*)");
				flag1 = true;
			}
			if (flag1)
			{
				stringBuilder.Append(")");
			}
			return stringBuilder.ToString();
		}

		protected static void DateTimeFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName, bool useAcctExpLogic)
		{
			long num;
			DateTime? nullable;
			dSPropertyValueCollection item = properties[suggestedAdProperty];
			if (item.Count != 0)
			{
				if (item[0] as long == 0)
				{
					num = ADUtils.LargeIntToInt64((UnsafeNativeMethods.IADsLargeInteger)item[0]);
				}
				else
				{
					num = (long)item[0];
				}
				if (useAcctExpLogic || num != (long)0)
				{
					if (!useAcctExpLogic || num != (long)0 && num != 0x7fffffffffffffffL)
					{
						nullable = new DateTime?(ADUtils.ADFileTimeToDateTime(num));
					}
					else
					{
						nullable = null;
					}
				}
				else
				{
					nullable = null;
				}
				p.LoadValueIntoProperty(propertyName, nullable);
			}
		}

		protected static string DefaultValueBoolConverter(FilterBase filter, string suggestedAdProperty)
		{
			string str;
			string str1;
			StringBuilder stringBuilder = new StringBuilder();
			if (filter.Value == null)
			{
				stringBuilder.Append("(!(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=*))");
			}
			else
			{
				bool item = ADStoreCtx.NonPresentAttrDefaultStateMapping[suggestedAdProperty];
				if (item != (bool)filter.Value)
				{
					stringBuilder.Append("(");
					stringBuilder.Append(suggestedAdProperty);
					stringBuilder.Append("=");
					StringBuilder stringBuilder1 = stringBuilder;
					if ((bool)filter.Value)
					{
						str = "TRUE";
					}
					else
					{
						str = "FALSE";
					}
					stringBuilder1.Append(str);
					stringBuilder.Append(")");
				}
				else
				{
					stringBuilder.Append("(|(!(");
					stringBuilder.Append(suggestedAdProperty);
					stringBuilder.Append("=*)(");
					stringBuilder.Append(suggestedAdProperty);
					stringBuilder.Append("=");
					StringBuilder stringBuilder2 = stringBuilder;
					if ((bool)filter.Value)
					{
						str1 = "TRUE";
					}
					else
					{
						str1 = "FALSE";
					}
					stringBuilder2.Append(str1);
					stringBuilder.Append(")))");
				}
			}
			return stringBuilder.ToString();
		}

		protected static string DefaultValutMatchingDateTimeConverter(FilterBase filter, string suggestedAdProperty)
		{
			QbeMatchType value = (QbeMatchType)filter.Value;
			return ADStoreCtx.DateTimeFilterBuilder(suggestedAdProperty, (DateTime)value.Value, LdapConstants.defaultUtcTime, false, value.Match);
		}

		internal override void Delete(Principal p)
		{
			try
			{
				SDSUtils.DeleteDirectoryEntry((DirectoryEntry)p.UnderlyingObject);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		public override void Dispose()
		{
			try
			{
				if (!this.disposed)
				{
					if (this.ownCtxBase)
					{
						this.ctxBase.Dispose();
					}
					this.disposed = true;
				}
			}
			finally
			{
				base.Dispose();
			}
		}

		private void EnablePrincipalIfNecessary(Principal p)
		{
			if (p.GetChangeStatusForProperty("AuthenticablePrincipal.Enabled"))
			{
				bool valueForProperty = (bool)p.GetValueForProperty("AuthenticablePrincipal.Enabled");
				this.SetAuthPrincipalEnableStatus((AuthenticablePrincipal)p, valueForProperty);
			}
		}

		protected static string ExpirationDateConverter(FilterBase filter, string suggestedAdProperty)
		{
			DateTime? value = (DateTime?)filter.Value;
			StringBuilder stringBuilder = new StringBuilder();
			if (value.HasValue)
			{
				stringBuilder.Append("(accountExpires=");
				stringBuilder.Append(ADUtils.DateTimeToADString(value.Value));
				stringBuilder.Append(")");
			}
			else
			{
				stringBuilder.Append("(|(accountExpires=9223372036854775807)(accountExpires=0))");
			}
			return stringBuilder.ToString();
		}

		internal override void ExpirePassword(AuthenticablePrincipal p)
		{
			this.WriteAttribute(p, "pwdLastSet", 0);
		}

		protected static string ExtensionCacheConverter(FilterBase filter, string suggestedAdProperty)
		{
			Type type;
			StringBuilder stringBuilder = new StringBuilder();
			if (filter.Value != null)
			{
				ExtensionCache value = (ExtensionCache)filter.Value;
				foreach (KeyValuePair<string, ExtensionCacheValue> property in value.properties)
				{
					if (property.Value.Type == null)
					{
						type = property.Value.Value.GetType();
					}
					else
					{
						type = property.Value.Type;
					}
					Type type1 = type;
					if (property.Value.Value == null)
					{
						stringBuilder.Append(ADStoreCtx.ExtensionTypeConverter(property.Key, type1, property.Value.Value, property.Value.MatchType));
					}
					else
					{
						ICollection collections = property.Value.Value;
						foreach (object obj in collections)
						{
							stringBuilder.Append(ADStoreCtx.ExtensionTypeConverter(property.Key, obj.GetType(), obj, property.Value.MatchType));
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		protected static void ExtensionCacheToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			ICollection value;
			ExtensionCache valueForProperty = (ExtensionCache)p.GetValueForProperty(propertyName);
			foreach (KeyValuePair<string, ExtensionCacheValue> property in valueForProperty.properties)
			{
				if (property.Value.Filter || property.Value.Value == null || (int)property.Value.Value.Length == 0)
				{
					continue;
				}
				if (((int)property.Value.Value.Length != 1 || property.Value.Value[0] as ICollection == null) && (int)property.Value.Value.Length <= 1)
				{
					if (p.unpersisted && property.Value.Value[0] == null)
					{
						continue;
					}
					de.Properties[property.Key].Value = property.Value.Value[0];
				}
				else
				{
					if ((int)property.Value.Value.Length <= 1 || property.Value.Value[0] as ICollection == null)
					{
						if ((int)property.Value.Value.Length != 1 || property.Value.Value[0] as ICollection == null || property.Value.Value[0] as byte[] != null)
						{
							value = property.Value.Value;
						}
						else
						{
							value = (ICollection)property.Value.Value[0];
						}
						foreach (object obj in value)
						{
							if (obj == null || obj as ICollection == null && obj as IList == null || obj as byte[] != null)
							{
								if (p.unpersisted && obj == null)
								{
									continue;
								}
								de.Properties[property.Key].Add(obj);
							}
							else
							{
								throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
							}
						}
					}
					else
					{
						throw new ArgumentException(StringResources.InvalidExtensionCollectionType);
					}
				}
			}
		}

		public static string ExtensionTypeConverter(string attributeName, Type type, object value, MatchType mt)
		{
			string ldapQueryString;
			string str;
			string str1;
			StringBuilder stringBuilder = new StringBuilder("(");
			if (typeof(bool) != type)
			{
				if (type as ICollection == null)
				{
					if (typeof(DateTime) != type)
					{
						ldapQueryString = ADUtils.PAPIQueryToLdapQueryString(value.ToString());
					}
					else
					{
						ldapQueryString = ADUtils.DateTimeToADString((DateTime)value);
					}
				}
				else
				{
					StringBuilder stringBuilder1 = new StringBuilder();
					ICollection collections = (ICollection)value;
					foreach (object obj in collections)
					{
						stringBuilder1.Append(ADStoreCtx.ExtensionTypeConverter(attributeName, obj.GetType(), obj, mt));
					}
					return stringBuilder1.ToString();
				}
			}
			else
			{
				if ((bool)value)
				{
					str1 = "TRUE";
				}
				else
				{
					str1 = "FALSE";
				}
				ldapQueryString = str1;
			}
			MatchType matchType = mt;
			switch (matchType)
			{
				case MatchType.Equals:
				{
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=");
					stringBuilder.Append(ldapQueryString);
					break;
				}
				case MatchType.NotEquals:
				{
					stringBuilder.Append("!(");
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=");
					stringBuilder.Append(ldapQueryString);
					stringBuilder.Append(")");
					break;
				}
				case MatchType.GreaterThan:
				case MatchType.LessThan:
				{
					stringBuilder.Append("&");
					stringBuilder.Append("(");
					stringBuilder.Append(attributeName);
					StringBuilder stringBuilder2 = stringBuilder;
					if (mt == MatchType.GreaterThan)
					{
						str = ">=";
					}
					else
					{
						str = "<=";
					}
					stringBuilder2.Append(str);
					stringBuilder.Append(ldapQueryString);
					stringBuilder.Append(")");
					stringBuilder.Append("(!(");
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=");
					stringBuilder.Append(ldapQueryString);
					stringBuilder.Append("))");
					stringBuilder.Append("(");
					stringBuilder.Append(attributeName);
					stringBuilder.Append("=*)");
					break;
				}
				case MatchType.GreaterThanOrEquals:
				{
					stringBuilder.Append(attributeName);
					stringBuilder.Append(">=");
					stringBuilder.Append(ldapQueryString);
					break;
				}
				case MatchType.LessThanOrEquals:
				{
					stringBuilder.Append(attributeName);
					stringBuilder.Append("<=");
					stringBuilder.Append(ldapQueryString);
					break;
				}
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		internal override ResultSet FindByBadPasswordAttempt(DateTime dt, MatchType matchType, Type principalType)
		{
			string[] strArrays = new string[1];
			strArrays[0] = "badPasswordTime";
			return this.FindByDate(principalType, strArrays, matchType, dt);
		}

		private ResultSet FindByDate(Type subtype, string[] ldapAttributes, MatchType matchType, DateTime value)
		{
			ResultSet resultSet;
			string str;
			DirectorySearcher directorySearcher = new DirectorySearcher(this.ctxBase);
			try
			{
				try
				{
					directorySearcher.PageSize = 0x100;
					directorySearcher.ServerTimeLimit = new TimeSpan(0, 0, 30);
					this.BuildPropertySet(subtype, directorySearcher.PropertiesToLoad);
					string aDString = ADUtils.DateTimeToADString(value);
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(this.GetObjectClassPortion(subtype));
					stringBuilder.Append("(|");
					string[] strArrays = ldapAttributes;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str1 = strArrays[i];
						stringBuilder.Append("(");
						MatchType matchType1 = matchType;
						switch (matchType1)
						{
							case MatchType.Equals:
							{
								stringBuilder.Append(str1);
								stringBuilder.Append("=");
								stringBuilder.Append(aDString);
								break;
							}
							case MatchType.NotEquals:
							{
								stringBuilder.Append("!(");
								stringBuilder.Append(str1);
								stringBuilder.Append("=");
								stringBuilder.Append(aDString);
								stringBuilder.Append(")");
								break;
							}
							case MatchType.GreaterThan:
							case MatchType.LessThan:
							{
								stringBuilder.Append("&");
								stringBuilder.Append("(");
								stringBuilder.Append(str1);
								StringBuilder stringBuilder1 = stringBuilder;
								if (matchType == MatchType.GreaterThan)
								{
									str = ">=";
								}
								else
								{
									str = "<=";
								}
								stringBuilder1.Append(str);
								stringBuilder.Append(aDString);
								stringBuilder.Append(")");
								stringBuilder.Append("(!(");
								stringBuilder.Append(str1);
								stringBuilder.Append("=");
								stringBuilder.Append(aDString);
								stringBuilder.Append("))");
								stringBuilder.Append("(");
								stringBuilder.Append(str1);
								stringBuilder.Append("=*)");
								break;
							}
							case MatchType.GreaterThanOrEquals:
							{
								stringBuilder.Append(str1);
								stringBuilder.Append(">=");
								stringBuilder.Append(aDString);
								break;
							}
							case MatchType.LessThanOrEquals:
							{
								stringBuilder.Append(str1);
								stringBuilder.Append("<=");
								stringBuilder.Append(aDString);
								break;
							}
						}
						stringBuilder.Append(")");
					}
					stringBuilder.Append("))");
					directorySearcher.Filter = stringBuilder.ToString();
					SearchResultCollection searchResultCollections = directorySearcher.FindAll();
					ADEntriesSet aDEntriesSet = new ADEntriesSet(searchResultCollections, this);
					resultSet = aDEntriesSet;
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
			finally
			{
				directorySearcher.Dispose();
			}
			return resultSet;
		}

		internal override ResultSet FindByExpirationTime(DateTime dt, MatchType matchType, Type principalType)
		{
			string[] strArrays = new string[1];
			strArrays[0] = "accountExpires";
			return this.FindByDate(principalType, strArrays, matchType, dt);
		}

		internal override ResultSet FindByLockoutTime(DateTime dt, MatchType matchType, Type principalType)
		{
			string[] strArrays = new string[1];
			strArrays[0] = "lockoutTime";
			return this.FindByDate(principalType, strArrays, matchType, dt);
		}

		internal override ResultSet FindByLogonTime(DateTime dt, MatchType matchType, Type principalType)
		{
			string[] strArrays = new string[2];
			strArrays[0] = "lastLogon";
			strArrays[1] = "lastLogonTimestamp";
			return this.FindByDate(principalType, strArrays, matchType, dt);
		}

		internal override ResultSet FindByPasswordSetTime(DateTime dt, MatchType matchType, Type principalType)
		{
			string[] strArrays = new string[1];
			strArrays[0] = "pwdLastSet";
			return this.FindByDate(principalType, strArrays, matchType, dt);
		}

		internal override Principal FindPrincipalByIdentRef(Type principalType, string urnScheme, string urnValue, DateTime referenceDate)
		{
			return this.FindPrincipalByIdentRefHelper(principalType, urnScheme, urnValue, referenceDate, false);
		}

		private Principal FindPrincipalByIdentRefHelper(Type principalType, string urnScheme, string urnValue, DateTime referenceDate, bool useSidHistory)
		{
			Principal asPrincipal;
			DirectorySearcher directorySearcher = new DirectorySearcher(this.ctxBase);
			SearchResultCollection searchResultCollections = null;
			try
			{
				try
				{
					directorySearcher.SizeLimit = 2;
					if (principalType == typeof(Principal) || principalType == typeof(AuthenticablePrincipal))
					{
						this.BuildPropertySet(typeof(UserPrincipal), directorySearcher.PropertiesToLoad);
						this.BuildPropertySet(typeof(GroupPrincipal), directorySearcher.PropertiesToLoad);
						this.BuildPropertySet(typeof(ComputerPrincipal), directorySearcher.PropertiesToLoad);
						if (principalType == typeof(Principal))
						{
							this.BuildPropertySet(typeof(AuthenticablePrincipal), directorySearcher.PropertiesToLoad);
						}
					}
					this.BuildPropertySet(principalType, directorySearcher.PropertiesToLoad);
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(this.GetObjectClassPortion(principalType));
					if (urnScheme == null)
					{
						if (principalType == typeof(Principal) || principalType == typeof(GroupPrincipal) || principalType.IsSubclassOf(typeof(GroupPrincipal)))
						{
							byte[] numArray = null;
							try
							{
								SecurityIdentifier securityIdentifier = new SecurityIdentifier(urnValue);
								numArray = new byte[securityIdentifier.BinaryLength];
								securityIdentifier.GetBinaryForm(numArray, 0);
							}
							catch (ArgumentException argumentException)
							{
							}
							if (numArray != null)
							{
								IntPtr zero = IntPtr.Zero;
								try
								{
									zero = Utils.ConvertByteArrayToIntPtr(numArray);
									if (UnsafeNativeMethods.IsValidSid(zero) && Utils.ClassifySID(zero) == SidType.FakeObject)
									{
										asPrincipal = this.ConstructFakePrincipalFromSID(numArray);
										return asPrincipal;
									}
								}
								finally
								{
									if (zero != IntPtr.Zero)
									{
										Marshal.FreeHGlobal(zero);
									}
								}
							}
						}
						string[] strArrays = new string[6];
						strArrays[0] = "ms-nt4account";
						strArrays[1] = "ms-upn";
						strArrays[2] = "ldap-dn";
						strArrays[3] = "ms-sid";
						strArrays[4] = "ms-guid";
						strArrays[5] = "ms-name";
						string[] strArrays1 = strArrays;
						StringBuilder stringBuilder1 = new StringBuilder();
						stringBuilder1.Append("(|");
						string str = null;
						string[] strArrays2 = strArrays1;
						for (int i = 0; i < (int)strArrays2.Length; i++)
						{
							string str1 = strArrays2[i];
							if (this.BuildLdapFilterFromIdentityClaim(urnValue, str1, ref str, useSidHistory, false) && str != null)
							{
								stringBuilder1.Append(str);
							}
						}
						stringBuilder1.Append(")");
						stringBuilder.Append(stringBuilder1.ToString());
					}
					else
					{
						if (urnScheme == "ms-sid" && (principalType == typeof(Principal) || principalType == typeof(GroupPrincipal) || principalType.IsSubclassOf(typeof(GroupPrincipal))))
						{
							SecurityIdentifier securityIdentifier1 = new SecurityIdentifier(urnValue);
							byte[] numArray1 = new byte[securityIdentifier1.BinaryLength];
							securityIdentifier1.GetBinaryForm(numArray1, 0);
							if (securityIdentifier1 != null)
							{
								IntPtr intPtr = IntPtr.Zero;
								try
								{
									intPtr = Utils.ConvertByteArrayToIntPtr(numArray1);
									if (UnsafeNativeMethods.IsValidSid(intPtr) && Utils.ClassifySID(intPtr) == SidType.FakeObject)
									{
										asPrincipal = this.ConstructFakePrincipalFromSID(numArray1);
										return asPrincipal;
									}
								}
								finally
								{
									if (intPtr != IntPtr.Zero)
									{
										Marshal.FreeHGlobal(intPtr);
									}
								}
							}
							else
							{
								throw new ArgumentException(StringResources.StoreCtxSecurityIdentityClaimBadFormat);
							}
						}
						string str2 = null;
						this.BuildLdapFilterFromIdentityClaim(urnValue, urnScheme, ref str2, useSidHistory, true);
						stringBuilder.Append(str2);
					}
					stringBuilder.Append(")");
					directorySearcher.Filter = stringBuilder.ToString();
					searchResultCollections = directorySearcher.FindAll();
					if (searchResultCollections != null)
					{
						int count = searchResultCollections.Count;
						if (count <= 1)
						{
							if (count != 0)
							{
								asPrincipal = this.GetAsPrincipal(searchResultCollections[0], principalType);
							}
							else
							{
								asPrincipal = null;
							}
						}
						else
						{
							throw new MultipleMatchesException(StringResources.MultipleMatchingPrincipals);
						}
					}
					else
					{
						asPrincipal = null;
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
			finally
			{
				directorySearcher.Dispose();
				if (searchResultCollections != null)
				{
					searchResultCollections.Dispose();
				}
			}
			return asPrincipal;
		}

		internal Principal FindPrincipalBySID(Type principalType, IdentityReference ir, bool useSidHistory)
		{
			return this.FindPrincipalByIdentRefHelper(principalType, ir.UrnScheme, ir.UrnValue, DateTime.UtcNow, useSidHistory);
		}

		protected static void GenericDateTimeFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			ADStoreCtx.DateTimeFromLdapConverter(properties, suggestedAdProperty, p, propertyName, false);
		}

		internal override Principal GetAsPrincipal(object storeObject, object discriminant)
		{
			string path;
			string item;
			Principal principal;
			Guid guid;
			Principal principal1;
			object directoryEntry;
			DirectoryEntry directoryEntry1;
			DirectoryEntry directoryEntry2 = null;
			SearchResult searchResult = null;
			if (storeObject as DirectoryEntry == null)
			{
				searchResult = (SearchResult)storeObject;
				path = searchResult.Path;
				item = (string)searchResult.Properties["distinguishedName"][0];
			}
			else
			{
				directoryEntry2 = (DirectoryEntry)storeObject;
				path = directoryEntry2.Path;
				item = (string)directoryEntry2.Properties["distinguishedName"].Value;
			}
			bool flag = SDSUtils.IsObjectFromGC(path);
			using (directoryEntry2)
			{
				if (!flag || directoryEntry2 == null)
				{
					try
					{
						DirectoryEntry directoryEntry3 = null;
						PrincipalContext context = null;
						if (flag || base.OwningContext.ContextType == ContextType.Domain)
						{
							string str = SDSUtils.ConstructDnsDomainNameFromDn(item);
							if (flag || string.Compare(this.DnsDomainName, str, StringComparison.OrdinalIgnoreCase) != 0)
							{
								context = SDSCache.Domain.GetContext(str, this.Credentials, base.OwningContext.Options);
							}
							if (flag)
							{
								directoryEntry3 = SDSUtils.BuildDirectoryEntry(string.Concat("LDAP://", str, "/", this.GetEscapedDN(item)), this.Credentials, this.authTypes);
								this.InitializeNewDirectoryOptions(directoryEntry3);
							}
						}
						if (directoryEntry2 == null)
						{
							SearchResult searchResult1 = searchResult;
							PrincipalContext principalContext = context;
							PrincipalContext owningContext = principalContext;
							if (principalContext == null)
							{
								owningContext = base.OwningContext;
							}
							principal = SDSUtils.SearchResultToPrincipal(searchResult1, owningContext, (Type)discriminant);
							Principal principal2 = principal;
							if (flag)
							{
								directoryEntry = directoryEntry3;
							}
							else
							{
								directoryEntry = searchResult.GetDirectoryEntry();
							}
							principal2.UnderlyingObject = directoryEntry;
						}
						else
						{
							if (flag)
							{
								directoryEntry1 = directoryEntry3;
							}
							else
							{
								directoryEntry1 = directoryEntry2;
							}
							PrincipalContext principalContext1 = context;
							PrincipalContext owningContext1 = principalContext1;
							if (principalContext1 == null)
							{
								owningContext1 = base.OwningContext;
							}
							principal = SDSUtils.DirectoryEntryToPrincipal(directoryEntry1, owningContext1, (Type)discriminant);
						}
						if (directoryEntry2 == null)
						{
							byte[] numArray = (byte[])searchResult.Properties["objectGuid"][0];
							guid = new Guid(numArray);
						}
						else
						{
							guid = directoryEntry2.Guid;
						}
						ADStoreKey aDStoreKey = new ADStoreKey(guid);
						principal.Key = aDStoreKey;
						principal1 = principal;
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
				}
			}
			return principal1;
		}

		private DirectorySearcher GetDirectorySearcherFromGroupID(byte[] groupSid)
		{
			int lastRidFromSid = Utils.GetLastRidFromSid(groupSid);
			DirectorySearcher directorySearcher = new DirectorySearcher(this.ctxBase);
			directorySearcher.Filter = string.Concat(this.GetObjectClassPortion(typeof(Principal)), "(primaryGroupId=", lastRidFromSid.ToString(CultureInfo.InvariantCulture), "))");
			directorySearcher.PageSize = 0x100;
			directorySearcher.ServerTimeLimit = new TimeSpan(0, 0, 30);
			this.BuildPropertySet(typeof(Principal), directorySearcher.PropertiesToLoad);
			return directorySearcher;
		}

		private string GetEscapedDN(string dn)
		{
			UnsafeNativeMethods.Pathname pathname = new UnsafeNativeMethods.Pathname();
			UnsafeNativeMethods.IADsPathname aDsPathname = (UnsafeNativeMethods.IADsPathname)pathname;
			aDsPathname.EscapedMode = 2;
			aDsPathname.Set(dn, 4);
			return aDsPathname.Retrieve(7);
		}

		private string GetGroupDnFromGroupID(byte[] userSid, int primaryGroupId)
		{
			IntPtr zero = IntPtr.Zero;
			byte[] byteArray = null;
			try
			{
				string sDDL = Utils.ConvertSidToSDDL(userSid);
				if (sDDL != null)
				{
					int num = sDDL.LastIndexOf('-');
					if (num != -1)
					{
						int num1 = primaryGroupId;
						sDDL = string.Concat(sDDL.Substring(0, num), "-", num1.ToString(CultureInfo.InvariantCulture));
						if (UnsafeNativeMethods.ConvertStringSidToSid(sDDL, ref zero))
						{
							byteArray = Utils.ConvertNativeSidToByteArray(zero);
						}
					}
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.LocalFree(zero);
				}
			}
			if (byteArray == null)
			{
				return null;
			}
			else
			{
				return string.Concat("<SID=", Utils.ByteArrayToString(byteArray), ">");
			}
		}

		internal override BookmarkableResultSet GetGroupMembership(GroupPrincipal g, bool recursive)
		{
			BookmarkableResultSet bookmarkableResultSet;
			bool hasValue;
			if (!g.fakePrincipal)
			{
				try
				{
					DirectoryEntry underlyingObject = (DirectoryEntry)g.UnderlyingObject;
					DirectorySearcher directorySearcherFromGroupID = null;
					if (underlyingObject.Properties["objectSid"].Count > 0)
					{
						byte[] item = (byte[])underlyingObject.Properties["objectSid"][0];
						directorySearcherFromGroupID = this.GetDirectorySearcherFromGroupID(item);
					}
					string value = (string)underlyingObject.Properties["distinguishedName"].Value;
					BookmarkableResultSet aDDNLinkedAttrSet = null;
					if (g.Context.ContextType != ContextType.ApplicationDirectory && g.Context.ServerInformation.OsVersion != DomainControllerMode.Win2k)
					{
						GroupScope? groupScope = g.GroupScope;
						if (groupScope.GetValueOrDefault() != GroupScope.Global)
						{
							hasValue = true;
						}
						else
						{
							hasValue = !groupScope.HasValue;
						}
						if (hasValue)
						{
							goto Label1;
						}
						DirectorySearcher[] directorySearcherArray = new DirectorySearcher[1];
						directorySearcherArray[0] = SDSUtils.ConstructSearcher((DirectoryEntry)g.UnderlyingObject);
						directorySearcherArray[0].AttributeScopeQuery = "member";
						directorySearcherArray[0].SearchScope = SearchScope.Base;
						directorySearcherArray[0].Filter = "(objectClass=*)";
						directorySearcherArray[0].CacheResults = false;
						this.BuildPropertySet(typeof(UserPrincipal), directorySearcherArray[0].PropertiesToLoad);
						this.BuildPropertySet(typeof(GroupPrincipal), directorySearcherArray[0].PropertiesToLoad);
						aDDNLinkedAttrSet = new ADDNLinkedAttrSet(value, directorySearcherArray, null, directorySearcherFromGroupID, recursive, this);
						goto Label0;
					}
				Label1:
					IEnumerable rangeRetrievers = new RangeRetriever(underlyingObject, "member", false);
					IEnumerable[] enumerableArray = new IEnumerable[1];
					enumerableArray[0] = rangeRetrievers;
					aDDNLinkedAttrSet = new ADDNLinkedAttrSet(value, enumerableArray, null, directorySearcherFromGroupID, recursive, this);
				Label0:
					bookmarkableResultSet = aDDNLinkedAttrSet;
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
				return bookmarkableResultSet;
			}
			else
			{
				return new EmptySet();
			}
		}

		internal override ResultSet GetGroupsMemberOf(Principal p)
		{
			ResultSet aDDNLinkedAttrSet;
			ResultSet groupsMemberOf;
			string userName;
			string password;
			string str;
			string password1;
			string userName1;
			string str1;
			ADDNConstraintLinkedAttrSet.ResultValidator resultValidator = null;
			DirectoryEntry directoryEntry = null;
			DirectorySearcher directorySearcher = null;
			ADDNConstraintLinkedAttrSet.ResultValidator resultValidator1 = null;
			try
			{
				try
				{
					if (!p.fakePrincipal)
					{
						string groupDnFromGroupID = null;
						bool flag = false;
						List<DirectoryEntry> directoryEntries = new List<DirectoryEntry>(1);
						DirectorySearcher[] directorySearcherArray = null;
						IEnumerable[] rangeRetrievers = null;
						DirectoryEntry underlyingObject = (DirectoryEntry)p.GetUnderlyingObject();
						if (p.ContextType == ContextType.ApplicationDirectory || p.Context.ServerInformation.OsVersion == DomainControllerMode.Win2k)
						{
							flag = false;
						}
						else
						{
							flag = true;
						}
						if (p.ContextType != ContextType.ApplicationDirectory)
						{
							int num = 1;
							string dnsForestName = this.DnsForestName;
							if (this.credentials != null)
							{
								userName = this.credentials.UserName;
							}
							else
							{
								userName = null;
							}
							if (this.credentials != null)
							{
								password = this.credentials.Password;
							}
							else
							{
								password = null;
							}
							Forest forest = Forest.GetForest(new DirectoryContext((DirectoryContextType)num, dnsForestName, userName, password));
							int num1 = 0;
							string dnsDomainName = this.DnsDomainName;
							if (this.credentials != null)
							{
								str = this.credentials.UserName;
							}
							else
							{
								str = null;
							}
							if (this.credentials != null)
							{
								password1 = this.credentials.Password;
							}
							else
							{
								password1 = null;
							}
							DirectoryContext directoryContext = new DirectoryContext((DirectoryContextType)num1, dnsDomainName, str, password1);
							DomainController domainController = DomainController.FindOne(directoryContext);
							GlobalCatalog globalCatalog = null;
							try
							{
								try
								{
									globalCatalog = forest.FindGlobalCatalog();
									GlobalCatalogCollection globalCatalogCollection = forest.FindAllGlobalCatalogs(domainController.SiteName);
									foreach (GlobalCatalog globalCatalog1 in globalCatalogCollection)
									{
										if (string.Compare(this.DnsDomainName, globalCatalog1.Domain.Name, StringComparison.OrdinalIgnoreCase) != 0)
										{
											continue;
										}
										globalCatalog = globalCatalog1;
										break;
									}
									List<DirectoryEntry> directoryEntries1 = directoryEntries;
									string str2 = string.Concat("GC://", globalCatalog.Name, "/", p.DistinguishedName);
									if (this.credentials != null)
									{
										userName1 = this.credentials.UserName;
									}
									else
									{
										userName1 = null;
									}
									if (this.credentials != null)
									{
										str1 = this.credentials.Password;
									}
									else
									{
										str1 = null;
									}
									directoryEntries1.Add(new DirectoryEntry(str2, userName1, str1, this.AuthTypes));
									if (string.Compare(this.DnsDomainName, globalCatalog.Domain.Name, StringComparison.OrdinalIgnoreCase) != 0)
									{
										directoryEntries.Add(underlyingObject);
										if (resultValidator == null)
										{
											resultValidator = (dSPropertyCollection resultPropCollection) => {
												if (resultPropCollection["groupType"].Count > 0 && resultPropCollection["objectSid"].Count > 0)
												{
													int? item = (int?)resultPropCollection["groupType"][0];
													if (item.HasValue && (item.Value & 4) == 4)
													{
														byte[] numArray = (byte[])resultPropCollection["objectSid"][0];
														SecurityIdentifier securityIdentifier = new SecurityIdentifier(numArray, 0);
														return ADUtils.AreSidsInSameDomain(p.Sid, securityIdentifier);
													}
												}
												return true;
											}
											;
										}
										resultValidator1 = resultValidator;
									}
								}
								catch (ActiveDirectoryOperationException activeDirectoryOperationException1)
								{
									ActiveDirectoryOperationException activeDirectoryOperationException = activeDirectoryOperationException1;
									throw new PrincipalOperationException(activeDirectoryOperationException.Message, activeDirectoryOperationException);
								}
								catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException1)
								{
									ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException = activeDirectoryObjectNotFoundException1;
									throw new PrincipalOperationException(activeDirectoryObjectNotFoundException.Message, activeDirectoryObjectNotFoundException);
								}
							}
							finally
							{
								if (globalCatalog != null)
								{
									globalCatalog.Dispose();
								}
								if (forest != null)
								{
									forest.Dispose();
								}
							}
						}
						if (flag)
						{
							int num2 = 0;
							directorySearcherArray = new DirectorySearcher[directoryEntries.Count];
							foreach (DirectoryEntry directoryEntry1 in directoryEntries)
							{
								directorySearcherArray[num2] = SDSUtils.ConstructSearcher(directoryEntry1);
								directorySearcherArray[num2].SearchScope = SearchScope.Base;
								directorySearcherArray[num2].AttributeScopeQuery = "memberOf";
								directorySearcherArray[num2].Filter = "(objectClass=*)";
								directorySearcherArray[num2].CacheResults = false;
								this.BuildPropertySet(typeof(GroupPrincipal), directorySearcherArray[num2].PropertiesToLoad);
								num2++;
							}
						}
						else
						{
							if (p.ContextType == ContextType.ApplicationDirectory)
							{
								rangeRetrievers = new IEnumerable[1];
								rangeRetrievers[0] = new RangeRetriever(underlyingObject, "memberOf", false);
							}
							else
							{
								int num3 = 0;
								rangeRetrievers = new IEnumerable[directoryEntries.Count];
								foreach (DirectoryEntry directoryEntry2 in directoryEntries)
								{
									rangeRetrievers[num3] = new RangeRetriever(directoryEntry2, "memberOf", directoryEntry2 != underlyingObject);
									num3++;
								}
							}
						}
						string value = (string)underlyingObject.Properties["distinguishedName"].Value;
						string[] strArrays = new string[2];
						strArrays[0] = "memberOf";
						strArrays[1] = "primaryGroupID";
						underlyingObject.RefreshCache(strArrays);
						if (underlyingObject.Properties["primaryGroupID"].Count > 0 && underlyingObject.Properties["objectSid"].Count > 0)
						{
							int value1 = (int)underlyingObject.Properties["primaryGroupID"].Value;
							byte[] numArray1 = (byte[])underlyingObject.Properties["objectSid"].Value;
							groupDnFromGroupID = this.GetGroupDnFromGroupID(numArray1, value1);
						}
						if (!flag)
						{
							if (resultValidator1 == null)
							{
								aDDNLinkedAttrSet = new ADDNLinkedAttrSet(value, rangeRetrievers, groupDnFromGroupID, null, false, this);
							}
							else
							{
								aDDNLinkedAttrSet = new ADDNConstraintLinkedAttrSet(ADDNConstraintLinkedAttrSet.ConstraintType.ResultValidatorDelegateMatch, resultValidator1, value, rangeRetrievers, groupDnFromGroupID, null, false, this);
							}
						}
						else
						{
							if (resultValidator1 == null)
							{
								aDDNLinkedAttrSet = new ADDNLinkedAttrSet(value, directorySearcherArray, groupDnFromGroupID, null, false, this);
							}
							else
							{
								aDDNLinkedAttrSet = new ADDNConstraintLinkedAttrSet(ADDNConstraintLinkedAttrSet.ConstraintType.ResultValidatorDelegateMatch, resultValidator1, value, directorySearcherArray, groupDnFromGroupID, null, false, this);
							}
						}
						groupsMemberOf = aDDNLinkedAttrSet;
					}
					else
					{
						groupsMemberOf = this.GetGroupsMemberOf(p, this);
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
			finally
			{
				if (directoryEntry != null)
				{
					directoryEntry.Dispose();
				}
				if (directorySearcher != null)
				{
					directorySearcher.Dispose();
				}
			}
			return groupsMemberOf;
		}

		internal override ResultSet GetGroupsMemberOf(Principal foreignPrincipal, StoreCtx foreignContext)
		{
			ResultSet emptySet;
			DirectoryEntry directoryEntry;
			string userName;
			string password;
			string str;
			string password1;
			SecurityIdentifier sid = foreignPrincipal.Sid;
			if (sid != null)
			{
				bool flag = true;
				if (foreignContext as ADStoreCtx != null && foreignContext as ADAMStoreCtx == null)
				{
					ADStoreCtx aDStoreCtx = (ADStoreCtx)foreignContext;
					if (string.Compare(aDStoreCtx.DnsForestName, this.DnsForestName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (string.Compare(aDStoreCtx.DnsDomainName, this.DnsDomainName, StringComparison.OrdinalIgnoreCase) != 0)
						{
							flag = false;
						}
						else
						{
							flag = true;
						}
					}
				}
				DirectoryEntry directoryEntry1 = null;
				string str1 = null;
				DirectoryEntry directoryEntry2 = null;
				ResultSet aDDNLinkedAttrSet = null;
				DirectorySearcher directorySearcher = null;
				try
				{
					try
					{
						if (!flag)
						{
							DirectorySearcher[] directorySearcherArray = new DirectorySearcher[1];
							directorySearcherArray[0] = SDSUtils.ConstructSearcher(this.ctxBase);
							DirectorySearcher[] directorySearcherArray1 = directorySearcherArray;
							directorySearcherArray1[0].Filter = string.Concat("(&(objectClass=Group)(member=", foreignPrincipal.DistinguishedName, "))");
							directorySearcherArray1[0].CacheResults = false;
							aDDNLinkedAttrSet = new ADDNLinkedAttrSet(foreignPrincipal.DistinguishedName, directorySearcherArray1, null, null, false, this);
						}
						else
						{
							if (this.DefaultNamingContext != null)
							{
								string str2 = string.Concat("LDAP://", this.UserSuppliedServerName, "/", this.DefaultNamingContext);
								if (this.Credentials != null)
								{
									userName = this.Credentials.UserName;
								}
								else
								{
									userName = null;
								}
								if (this.Credentials != null)
								{
									password = this.Credentials.Password;
								}
								else
								{
									password = null;
								}
								directoryEntry1 = new DirectoryEntry(str2, userName, password, this.AuthTypes);
								str1 = ADUtils.RetriveWkDn(directoryEntry1, this.DefaultNamingContext, this.UserSuppliedServerName, Constants.GUID_FOREIGNSECURITYPRINCIPALS_CONTAINER_BYTE);
								if (str1 != null)
								{
									string str3 = str1;
									if (this.Credentials != null)
									{
										str = this.credentials.UserName;
									}
									else
									{
										str = null;
									}
									if (this.Credentials != null)
									{
										password1 = this.credentials.Password;
									}
									else
									{
										password1 = null;
									}
									directoryEntry2 = new DirectoryEntry(str3, str, password1, this.authTypes);
								}
							}
							if (directoryEntry2 != null)
							{
								directoryEntry = directoryEntry2;
							}
							else
							{
								if (directoryEntry1 != null)
								{
									directoryEntry = directoryEntry1;
								}
								else
								{
									directoryEntry = this.ctxBase;
								}
							}
							directorySearcher = new DirectorySearcher(directoryEntry);
							directorySearcher.PageSize = 0x100;
							directorySearcher.ServerTimeLimit = new TimeSpan(0, 0, 30);
							string ldapHexFilterString = Utils.SecurityIdentifierToLdapHexFilterString(sid);
							if (ldapHexFilterString != null)
							{
								directorySearcher.Filter = string.Concat("(objectSid=", ldapHexFilterString, ")");
								directorySearcher.PropertiesToLoad.Add("memberOf");
								directorySearcher.PropertiesToLoad.Add("distinguishedName");
								directorySearcher.PropertiesToLoad.Add("primaryGroupID");
								directorySearcher.PropertiesToLoad.Add("objectSid");
								SearchResult searchResult = directorySearcher.FindOne();
								if (searchResult == null)
								{
									if (str1 != null)
									{
										directorySearcher.SearchRoot = directoryEntry1;
										searchResult = directorySearcher.FindOne();
										if (searchResult == null)
										{
											emptySet = new EmptySet();
											return emptySet;
										}
									}
									else
									{
										emptySet = new EmptySet();
										return emptySet;
									}
								}
								string item = (string)searchResult.Properties["distinguishedName"][0];
								IEnumerable rangeRetrievers = new RangeRetriever(searchResult.GetDirectoryEntry(), "memberOf", true);
								string groupDnFromGroupID = null;
								if (searchResult.Properties["primaryGroupID"].Count > 0 && searchResult.Properties["objectSid"].Count > 0)
								{
									int num = (int)searchResult.Properties["primaryGroupID"][0];
									byte[] numArray = (byte[])searchResult.Properties["objectSid"][0];
									groupDnFromGroupID = this.GetGroupDnFromGroupID(numArray, num);
								}
								IEnumerable[] enumerableArray = new IEnumerable[1];
								enumerableArray[0] = rangeRetrievers;
								aDDNLinkedAttrSet = new ADDNConstraintLinkedAttrSet(ADDNConstraintLinkedAttrSet.ConstraintType.ContainerStringMatch, this.ctxBase.Properties["distinguishedName"].Value, item, enumerableArray, groupDnFromGroupID, null, false, this);
							}
							else
							{
								throw new InvalidOperationException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
							}
						}
						emptySet = aDDNLinkedAttrSet;
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
				}
				finally
				{
					if (directoryEntry2 != null)
					{
						directoryEntry2.Dispose();
					}
					if (directorySearcher != null)
					{
						directorySearcher.Dispose();
					}
					if (directoryEntry1 != null)
					{
						directoryEntry1.Dispose();
					}
				}
				return emptySet;
			}
			else
			{
				throw new InvalidOperationException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
			}
		}

		internal override ResultSet GetGroupsMemberOfAZ(Principal p)
		{
			ResultSet tokenGroupSet;
			string userName;
			string password;
			SecurityIdentifier sid = p.Sid;
			if (sid != null)
			{
				byte[] numArray = new byte[sid.BinaryLength];
				sid.GetBinaryForm(numArray, 0);
				if (numArray != null)
				{
					try
					{
						string dnsDomainName = this.DnsDomainName;
						if (this.credentials == null)
						{
							userName = null;
						}
						else
						{
							userName = this.credentials.UserName;
						}
						if (this.credentials == null)
						{
							password = null;
						}
						else
						{
							password = this.credentials.Password;
						}
						if (!ADUtils.VerifyOutboundTrust(dnsDomainName, userName, password))
						{
							DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
							string value = (string)underlyingObject.Properties["distinguishedName"].Value;
							tokenGroupSet = new TokenGroupSet(value, this, true);
						}
						else
						{
							tokenGroupSet = new AuthZSet(numArray, this.credentials, this.contextOptions, this.FlatDomainName, this, this.ctxBase);
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
					return tokenGroupSet;
				}
				else
				{
					throw new ArgumentException(StringResources.StoreCtxSecurityIdentityClaimBadFormat);
				}
			}
			else
			{
				throw new InvalidOperationException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
			}
		}

		protected virtual string GetObjectClassPortion(Type principalType)
		{
			string str;
			if (principalType != typeof(UserPrincipal))
			{
				if (principalType != typeof(GroupPrincipal))
				{
					if (principalType != typeof(ComputerPrincipal))
					{
						if (principalType != typeof(Principal))
						{
							if (principalType != typeof(AuthenticablePrincipal))
							{
								string str1 = ExtensionHelper.ReadStructuralObjectClass(principalType);
								if (str1 != null)
								{
									StringBuilder stringBuilder = new StringBuilder();
									stringBuilder.Append("(&(objectClass=");
									stringBuilder.Append(str1);
									stringBuilder.Append(")");
									str = stringBuilder.ToString();
								}
								else
								{
									object[] objArray = new object[1];
									objArray[0] = principalType.ToString();
									throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPrincipalTypeForQuery, objArray));
								}
							}
							else
							{
								str = "(&(objectClass=user)";
							}
						}
						else
						{
							str = "(&(|(objectClass=user)(objectClass=group))";
						}
					}
					else
					{
						str = "(&(objectClass=computer)";
					}
				}
				else
				{
					str = "(&(objectClass=group)";
				}
			}
			else
			{
				str = "(&(objectCategory=user)(objectClass=user)";
			}
			return str;
		}

		protected static string GetSidPathFromPrincipal(Principal p)
		{
			if (!p.fakePrincipal)
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				if (!underlyingObject.Properties.Contains("objectSid"))
				{
					string[] strArrays = new string[1];
					strArrays[0] = "objectSid";
					underlyingObject.RefreshCache(strArrays);
				}
				byte[] value = (byte[])underlyingObject.Properties["objectSid"].Value;
				if (value != null)
				{
					return string.Concat("<SID=", Utils.ByteArrayToString(value), ">");
				}
				else
				{
					return null;
				}
			}
			else
			{
				SecurityIdentifier sid = p.Sid;
				if (sid != null)
				{
					return string.Concat("<SID=", Utils.SecurityIdentifierToLdapHexBindingString(sid), ">");
				}
				else
				{
					throw new InvalidOperationException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
				}
			}
		}

		protected static string GroupTypeConverter(FilterBase filter, string suggestedAdProperty)
		{
			string propertyName = filter.PropertyName;
			string str = propertyName;
			if (propertyName != null)
			{
				if (str == "GroupPrincipal.IsSecurityGroup")
				{
					bool value = (bool)filter.Value;
					if (!value)
					{
						return "(!(groupType:1.2.840.113556.1.4.803:=2147483648))";
					}
					else
					{
						return "(groupType:1.2.840.113556.1.4.803:=2147483648)";
					}
				}
				else
				{
					if (str == "GroupPrincipal.GroupScope")
					{
						GroupScope groupScope = (GroupScope)filter.Value;
						GroupScope groupScope1 = groupScope;
						switch (groupScope1)
						{
							case GroupScope.Local:
							{
								return "(groupType:1.2.840.113556.1.4.803:=4)";
							}
							case GroupScope.Global:
							{
								return "(groupType:1.2.840.113556.1.4.803:=2)";
							}
						}
						return "(groupType:1.2.840.113556.1.4.803:=8)";
					}
				}
			}
			return "";
		}

		protected static void GroupTypeFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			dSPropertyValueCollection item = properties[suggestedAdProperty];
			if (item.Count != 0)
			{
				int num = (int)item[0];
				string str = propertyName;
				string str1 = str;
				if (str != null)
				{
					if (str1 == "GroupPrincipal.IsSecurityGroup")
					{
						bool flag = false;
						if (((long)num & (long)-2147483648) != (long)0)
						{
							flag = true;
						}
						p.LoadValueIntoProperty(propertyName, flag);
						return;
					}
					else
					{
						if (str1 == "GroupPrincipal.GroupScope")
						{
							GroupScope groupScope = GroupScope.Universal;
							if ((num & 2) == 0)
							{
								if ((num & 4) != 0)
								{
									groupScope = GroupScope.Local;
								}
							}
							else
							{
								groupScope = GroupScope.Global;
							}
							p.LoadValueIntoProperty(propertyName, groupScope);
						}
						else
						{
							return;
						}
					}
				}
			}
		}

		protected static void GroupTypeToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			int item;
			if (de.Properties[suggestedAdProperty].Count <= 0)
			{
				if (p.unpersisted)
				{
					item = -2147483646;
				}
				else
				{
					throw new PrincipalOperationException(StringResources.ADStoreCtxUnableToReadExistingGroupTypeFlagsForUpdate);
				}
			}
			else
			{
				item = (int)de.Properties[suggestedAdProperty][0];
			}
			string str = propertyName;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "GroupPrincipal.IsSecurityGroup")
				{
					bool valueForProperty = (bool)p.GetValueForProperty(propertyName);
					if (valueForProperty)
					{
						Utils.SetBit(ref item, -2147483648);
					}
					else
					{
						Utils.ClearBit(ref item, -2147483648);
					}
				}
				else
				{
					if (str1 == "GroupPrincipal.GroupScope")
					{
						GroupScope groupScope = (GroupScope)p.GetValueForProperty(propertyName);
						Utils.ClearBit(ref item, 4);
						Utils.ClearBit(ref item, 2);
						Utils.ClearBit(ref item, 8);
						if (groupScope != GroupScope.Local)
						{
							if (groupScope != GroupScope.Global)
							{
								Utils.SetBit(ref item, 8);
							}
							else
							{
								Utils.SetBit(ref item, 2);
							}
						}
						else
						{
							Utils.SetBit(ref item, 4);
						}
					}
				}
			}
			de.Properties[suggestedAdProperty].Value = item;
		}

		protected static string GuidConverter(FilterBase filter, string suggestedAdProperty)
		{
			Guid? value = (Guid?)filter.Value;
			StringBuilder stringBuilder = new StringBuilder();
			if (value.HasValue)
			{
				stringBuilder.Append("(objectGuid=");
				string ldapHexString = ADUtils.HexStringToLdapHexString(value.ToString());
				if (ldapHexString != null)
				{
					stringBuilder.Append(ldapHexString);
					stringBuilder.Append(")");
				}
				else
				{
					throw new InvalidOperationException(StringResources.StoreCtxGuidIdentityClaimBadFormat);
				}
			}
			return stringBuilder.ToString();
		}

		protected static void GuidFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			if (properties["objectGuid"].Count != 1)
			{
				p.LoadValueIntoProperty(propertyName, null);
				return;
			}
			else
			{
				byte[] item = (byte[])properties["objectGuid"][0];
				Guid guid = new Guid(item);
				p.LoadValueIntoProperty(propertyName, guid);
				return;
			}
		}

		protected static string IdentityClaimConverter(FilterBase filter, string suggestedAdProperty)
		{
			IdentityClaim value = (IdentityClaim)filter.Value;
			if (value.UrnScheme != null)
			{
				string urnValue = value.UrnValue;
				if (urnValue == null)
				{
					urnValue = "";
				}
				string str = null;
				ADStoreCtx.IdentityClaimToFilter(urnValue, value.UrnScheme, ref str, true);
				return str;
			}
			else
			{
				throw new ArgumentException(StringResources.StoreCtxIdentityClaimMustHaveScheme);
			}
		}

		protected static bool IdentityClaimToFilter(string identity, string identityFormat, ref string filter, bool throwOnFail)
		{
			Guid guid;
			bool flag;
			string str;
			if (identity == null)
			{
				identity = "";
			}
			StringBuilder stringBuilder = new StringBuilder();
			string str1 = identityFormat;
			string str2 = str1;
			if (str1 != null)
			{
				if (str2 == "ms-guid")
				{
					try
					{
						guid = new Guid(identity);
						goto Label1;
					}
					catch (FormatException formatException1)
					{
						FormatException formatException = formatException1;
						if (!throwOnFail)
						{
							flag = false;
						}
						else
						{
							throw new ArgumentException(formatException.Message, formatException);
						}
					}
					return flag;
				}
				else if (str2 == "ldap-dn")
				{
					stringBuilder.Append("(distinguishedName=");
					stringBuilder.Append(ADUtils.EscapeRFC2254SpecialChars(identity));
					stringBuilder.Append(")");
				}
				else if (str2 == "ms-sid")
				{
					if (ADStoreCtx.SecurityIdentityClaimConverterHelper(identity, false, stringBuilder, throwOnFail))
					{
						filter = stringBuilder.ToString();
						return true;
					}
					return false;
				}
				else if (str2 == "ms-nt4account")
				{
					int num = identity.IndexOf('\\');
					if (num != identity.Length - 1)
					{
						if (num != -1)
						{
							str = identity.Substring(num + 1);
						}
						else
						{
							str = identity;
						}
						string str3 = str;
						stringBuilder.Append("(samAccountName=");
						stringBuilder.Append(ADUtils.EscapeRFC2254SpecialChars(str3));
						stringBuilder.Append(")");
					}
					else
					{
						if (!throwOnFail)
						{
							return false;
						}
						else
						{
							throw new ArgumentException(StringResources.StoreCtxNT4IdentityClaimWrongForm);
						}
					}
				}
				else if (str2 == "ms-name")
				{
					stringBuilder.Append("(name=");
					stringBuilder.Append(ADUtils.EscapeRFC2254SpecialChars(identity));
					stringBuilder.Append(")");
				}
				else if (str2 == "ms-upn")
				{
					stringBuilder.Append("(userPrincipalName=");
					stringBuilder.Append(ADUtils.EscapeRFC2254SpecialChars(identity));
					stringBuilder.Append(")");
				}
				else
				{
					goto Label3;
				}
				filter = stringBuilder.ToString();
				return true;
			}
		Label3:
			if (!throwOnFail)
			{
				return false;
			}
			else
			{
				throw new ArgumentException(StringResources.StoreCtxUnsupportedIdentityClaimForQuery);
			}
		Label1:
			byte[] byteArray = guid.ToByteArray();
			StringBuilder stringBuilder1 = new StringBuilder();
			byte[] numArray = byteArray;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				byte num1 = numArray[i];
				stringBuilder1.Append(num1.ToString("x2", CultureInfo.InvariantCulture));
			}
			string ldapHexString = ADUtils.HexStringToLdapHexString(stringBuilder1.ToString());
			if (ldapHexString != null)
			{
				stringBuilder.Append("(objectGuid=");
				stringBuilder.Append(ldapHexString);
				stringBuilder.Append(")");
				filter = stringBuilder.ToString();
				return true;
			}
			else
			{
				if (!throwOnFail)
				{
					return false;
				}
				else
				{
					throw new ArgumentException(StringResources.StoreCtxGuidIdentityClaimBadFormat);
				}
			}
		}

		protected internal virtual void InitializeNewDirectoryOptions(DirectoryEntry newDeChild)
		{
		}

		internal override void InitializeUserAccountControl(AuthenticablePrincipal p)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			Type type = p.GetType();
			if (type == typeof(ComputerPrincipal) || type.IsSubclassOf(typeof(ComputerPrincipal)))
			{
				underlyingObject.Properties["userAccountControl"].Value = 0x1022;
				return;
			}
			else
			{
				if (type == typeof(UserPrincipal) || type.IsSubclassOf(typeof(UserPrincipal)))
				{
					underlyingObject.Properties["userAccountControl"].Value = 0x222;
				}
				return;
			}
		}

		internal override void Insert(Principal p)
		{
			try
			{
				SDSUtils.InsertPrincipal(p, this, new SDSUtils.GroupMembershipUpdater(ADStoreCtx.UpdateGroupMembership), this.credentials, this.authTypes, true);
				this.LoadDirectoryEntryAttributes((DirectoryEntry)p.UnderlyingObject);
				this.EnablePrincipalIfNecessary(p);
				this.SetPasswordSecurityifNeccessary(p);
				ADStoreKey aDStoreKey = new ADStoreKey(((DirectoryEntry)p.UnderlyingObject).Guid);
				p.Key = aDStoreKey;
				p.ResetAllChangeStatus();
			}
			catch (PrincipalExistsException principalExistsException)
			{
				throw;
			}
			catch (SystemException systemException1)
			{
				SystemException systemException = systemException1;
				try
				{
					if (p.UnderlyingObject != null)
					{
						SDSUtils.DeleteDirectoryEntry((DirectoryEntry)p.UnderlyingObject);
					}
				}
				catch (COMException cOMException)
				{
				}
				if (systemException as COMException == null)
				{
					throw systemException;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromCOMException((COMException)systemException);
				}
			}
		}

		protected static void IntFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.SingleScalarFromDirectoryEntry<int>(properties, suggestedAdProperty, p, propertyName);
		}

		protected bool IsContainer(DirectoryEntry de)
		{
			bool flag;
			using (de.SchemaEntry)
			{
				if (V_0.Properties["possibleInferiors"].Count != 0)
				{
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			return flag;
		}

		internal override bool IsLockedOut(AuthenticablePrincipal p)
		{
			bool flag;
			try
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				string[] strArrays = new string[2];
				strArrays[0] = "msDS-User-Account-Control-Computed";
				strArrays[1] = "lockoutTime";
				underlyingObject.RefreshCache(strArrays);
				if (underlyingObject.Properties["msDS-User-Account-Control-Computed"].Count <= 0)
				{
					bool flag1 = false;
					if (underlyingObject.Properties["lockoutTime"].Count > 0)
					{
						ulong num = ADUtils.LargeIntToInt64((UnsafeNativeMethods.IADsLargeInteger)underlyingObject.Properties["lockoutTime"][0]);
						if (num != (long)0)
						{
							ulong lockoutDuration = this.LockoutDuration;
							if (lockoutDuration + num > ADUtils.DateTimeToADFileTime(DateTime.UtcNow))
							{
								flag1 = true;
							}
						}
					}
					flag = flag1;
				}
				else
				{
					int item = (int)underlyingObject.Properties["msDS-User-Account-Control-Computed"][0];
					flag = (item & 16) != 0;
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			return flag;
		}

		internal override bool IsMemberOfInStore(GroupPrincipal g, Principal p)
		{
			bool flag;
			object dnsHostName;
			if (!g.fakePrincipal)
			{
				if (p.ContextType == ContextType.Domain || p.ContextType == ContextType.ApplicationDirectory)
				{
					IEnumerable item = null;
					if (!p.fakePrincipal)
					{
						DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
						DirectoryEntry directoryEntry = (DirectoryEntry)g.UnderlyingObject;
						string value = (string)underlyingObject.Properties["distinguishedName"].Value;
						if (!g.IsSmallGroup())
						{
							RangeRetriever rangeRetrievers = new RangeRetriever(directoryEntry, "member", false);
							rangeRetrievers.CacheValues = true;
							foreach (string rangeRetriever in rangeRetrievers)
							{
								if (!value.Equals(rangeRetriever, StringComparison.OrdinalIgnoreCase))
								{
									continue;
								}
								flag = true;
								return flag;
							}
							rangeRetrievers.Reset();
							item = rangeRetrievers;
						}
						else
						{
							item = g.SmallGroupMemberSearchResult.Properties["member"];
							if (g.SmallGroupMemberSearchResult != null && g.SmallGroupMemberSearchResult.Properties["member"].Contains(value))
							{
								return true;
							}
						}
					}
					SecurityIdentifier sid = p.Sid;
					if (sid != null)
					{
						DirectoryEntry directoryEntry1 = null;
						DirectorySearcher directorySearcher = null;
						try
						{
							try
							{
								CultureInfo invariantCulture = CultureInfo.InvariantCulture;
								string str = "LDAP://{0}/{1}";
								object[] contextBasePartitionDN = new object[2];
								object[] objArray = contextBasePartitionDN;
								int num = 0;
								if (string.IsNullOrEmpty(this.UserSuppliedServerName))
								{
									dnsHostName = this.DnsHostName;
								}
								else
								{
									dnsHostName = this.UserSuppliedServerName;
								}
								objArray[num] = dnsHostName;
								contextBasePartitionDN[1] = this.ContextBasePartitionDN;
								string str1 = string.Format(invariantCulture, str, contextBasePartitionDN);
								directoryEntry1 = SDSUtils.BuildDirectoryEntry(str1, this.credentials, this.authTypes);
								directorySearcher = new DirectorySearcher(directoryEntry1);
								directorySearcher.ServerTimeLimit = new TimeSpan(0, 0, 30);
								string ldapHexFilterString = Utils.SecurityIdentifierToLdapHexFilterString(sid);
								if (ldapHexFilterString != null)
								{
									directorySearcher.Filter = string.Concat("(&(objectClass=foreignSecurityPrincipal)(objectSid=", ldapHexFilterString, "))");
									directorySearcher.PropertiesToLoad.Add("distinguishedName");
									SearchResult searchResult = directorySearcher.FindOne();
									if (searchResult != null)
									{
										string item1 = (string)searchResult.Properties["distinguishedName"][0];
										foreach (string str2 in item)
										{
											if (!string.Equals(item1, str2, StringComparison.OrdinalIgnoreCase))
											{
												continue;
											}
											flag = true;
											return flag;
										}
										flag = false;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									throw new ArgumentException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
								}
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
							}
						}
						finally
						{
							if (directorySearcher != null)
							{
								directorySearcher.Dispose();
							}
							if (directoryEntry1 != null)
							{
								directoryEntry1.Dispose();
							}
						}
					}
					else
					{
						throw new ArgumentException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
					}
					return flag;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		internal override bool IsValidProperty(Principal p, string propertyName)
		{
			return ((Hashtable)ADStoreCtx.propertyMappingTableByProperty[(object)this.MappingTableIndex]).Contains(propertyName);
		}

		protected static string LastLogonConverter(FilterBase filter, string suggestedAdProperty)
		{
			QbeMatchType value = (QbeMatchType)filter.Value;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("(|");
			stringBuilder.Append(ADStoreCtx.DateTimeFilterBuilder("lastLogon", (DateTime)value.Value, LdapConstants.defaultUtcTime, false, value.Match));
			stringBuilder.Append(ADStoreCtx.DateTimeFilterBuilder("lastLogonTimestamp", (DateTime)value.Value, LdapConstants.defaultUtcTime, true, value.Match));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		protected static void LastLogonFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			if (string.Compare(suggestedAdProperty, "lastLogon", StringComparison.OrdinalIgnoreCase) != 0 || properties["lastLogonTimestamp"].Count == 0)
			{
				ADStoreCtx.DateTimeFromLdapConverter(properties, suggestedAdProperty, p, propertyName, false);
				return;
			}
			else
			{
				return;
			}
		}

		internal override void Load(Principal p, string principalPropertyName)
		{
			dSPropertyCollection _dSPropertyCollection;
			SearchResult underlyingSearchObject = (SearchResult)p.UnderlyingSearchObject;
			if (underlyingSearchObject != null)
			{
				_dSPropertyCollection = new dSPropertyCollection(underlyingSearchObject.Properties);
			}
			else
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				_dSPropertyCollection = new dSPropertyCollection(underlyingObject.Properties);
			}
			Hashtable item = (Hashtable)ADStoreCtx.propertyMappingTableByPropertyFull[(object)this.MappingTableIndex];
			ArrayList arrayLists = (ArrayList)item[principalPropertyName];
			if (arrayLists != null)
			{
				try
				{
					foreach (ADStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry in arrayLists)
					{
						if (propertyMappingTableEntry.ldapToPapiConverter == null)
						{
							continue;
						}
						propertyMappingTableEntry.ldapToPapiConverter(_dSPropertyCollection, propertyMappingTableEntry.suggestedADPropertyName, p, propertyMappingTableEntry.propertyName);
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal override void Load(Principal p)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			Hashtable item = (Hashtable)ADStoreCtx.propertyMappingTableByLDAP[(object)this.MappingTableIndex];
			foreach (DictionaryEntry dictionaryEntry in item)
			{
				ArrayList value = (ArrayList)dictionaryEntry.Value;
				try
				{
					foreach (ADStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry in value)
					{
						propertyMappingTableEntry.ldapToPapiConverter(new dSPropertyCollection(underlyingObject.Properties), propertyMappingTableEntry.suggestedADPropertyName, p, propertyMappingTableEntry.propertyName);
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
			p.Loaded = true;
		}

		internal void LoadDirectoryEntryAttributes(DirectoryEntry de)
		{
			try
			{
				de.RefreshCache();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		protected virtual void LoadDomainInfo()
		{
			string str;
			this.dnsHostName = ADUtils.GetServerName(this.ctxBase);
			using (DirectoryEntry directoryEntry = new DirectoryEntry(string.Concat("LDAP://", this.dnsHostName, "/rootDse"), "", "", AuthenticationTypes.Anonymous))
			{
				this.defaultNamingContext = (string)directoryEntry.Properties["defaultNamingContext"][0];
				this.contextBasePartitionDN = this.defaultNamingContext;
				char[] chrArray = new char[1];
				chrArray[0] = ',';
				string[] strArrays = this.defaultNamingContext.Split(chrArray);
				StringBuilder stringBuilder = new StringBuilder();
				string[] strArrays1 = strArrays;
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					string str1 = strArrays1[i];
					if (str1.Length > 3 && string.Compare(str1.Substring(0, 3), "DC=", StringComparison.OrdinalIgnoreCase) == 0)
					{
						stringBuilder.Append(str1.Substring(3));
						stringBuilder.Append(".");
					}
				}
				str = stringBuilder.ToString();
				if (str.Length > 0)
				{
					str = str.Substring(0, str.Length - 1);
				}
				this.domainDnsName = str;
			}
			int num = -2147352304;
			UnsafeNativeMethods.DomainControllerInfo dcName = Utils.GetDcName(null, str, null, num);
			this.domainFlatName = dcName.DomainName;
			this.forestDnsName = dcName.DnsForestName;
			DirectoryEntry directoryEntry1 = SDSUtils.BuildDirectoryEntry(string.Concat("LDAP://", this.dnsHostName, "/", this.defaultNamingContext), this.credentials, this.authTypes);
			string[] strArrays2 = new string[1];
			strArrays2[0] = "lockoutDuration";
			directoryEntry1.RefreshCache(strArrays2);
			if (directoryEntry1.Properties["lockoutDuration"].Count > 0)
			{
				long num1 = ADUtils.LargeIntToInt64((UnsafeNativeMethods.IADsLargeInteger)directoryEntry1.Properties["lockoutDuration"][0]);
				ulong num2 = -num1;
				this.lockoutDuration = num2;
			}
			UnsafeNativeMethods.Pathname pathname = new UnsafeNativeMethods.Pathname();
			UnsafeNativeMethods.IADsPathname aDsPathname = (UnsafeNativeMethods.IADsPathname)pathname;
			aDsPathname.Set(this.ctxBase.Path, 1);
			try
			{
				this.userSuppliedServerName = aDsPathname.Retrieve(9);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				if (cOMException.ErrorCode != -2147463168)
				{
					throw;
				}
				else
				{
					this.userSuppliedServerName = "";
				}
			}
		}

		protected static void LoadFilterMappingTable(int mappingIndex, object[,] rawFilterPropertiesTable)
		{
			if (ADStoreCtx.filterPropertiesTable == null)
			{
				ADStoreCtx.filterPropertiesTable = new Hashtable();
			}
			Hashtable hashtables = new Hashtable();
			for (int i = 0; i < rawFilterPropertiesTable.GetLength(0); i++)
			{
				Type type = rawFilterPropertiesTable[i, 0] as Type;
				string str = rawFilterPropertiesTable[i, 1] as string;
				ADStoreCtx.FilterConverterDelegate filterConverterDelegate = rawFilterPropertiesTable[i, 2] as ADStoreCtx.FilterConverterDelegate;
				ADStoreCtx.FilterPropertyTableEntry filterPropertyTableEntry = new ADStoreCtx.FilterPropertyTableEntry();
				filterPropertyTableEntry.suggestedADPropertyName = str;
				filterPropertyTableEntry.converter = filterConverterDelegate;
				hashtables[type] = filterPropertyTableEntry;
			}
			ADStoreCtx.filterPropertiesTable.Add(mappingIndex, hashtables);
		}

		protected static void LoadPropertyMappingTable(int mappingIndex, object[,] rawPropertyMappingTable)
		{
			string[] strArrays = null;
			string[] strArrays1 = null;
			string[] strArrays2 = null;
			string[] strArrays3 = null;
			string[] strArrays4 = null;
			if (ADStoreCtx.propertyMappingTableByProperty == null)
			{
				ADStoreCtx.propertyMappingTableByProperty = new Hashtable();
			}
			if (ADStoreCtx.propertyMappingTableByLDAP == null)
			{
				ADStoreCtx.propertyMappingTableByLDAP = new Hashtable();
			}
			if (ADStoreCtx.propertyMappingTableByPropertyFull == null)
			{
				ADStoreCtx.propertyMappingTableByPropertyFull = new Hashtable();
			}
			if (ADStoreCtx.TypeToLdapPropListMap == null)
			{
				ADStoreCtx.TypeToLdapPropListMap = new Dictionary<int, Dictionary<Type, StringCollection>>();
			}
			Hashtable hashtables = new Hashtable();
			Hashtable arrayLists = new Hashtable();
			Hashtable hashtables1 = new Hashtable();
			Dictionary<string, string[]> strs = new Dictionary<string, string[]>();
			Dictionary<Type, StringCollection> types = new Dictionary<Type, StringCollection>();
			for (int i = 0; i < ADStoreCtx.propertyMappingTableRaw.GetLength(0); i++)
			{
				string str = rawPropertyMappingTable[i, 0] as string;
				string str1 = rawPropertyMappingTable[i, 1] as string;
				ADStoreCtx.FromLdapConverterDelegate fromLdapConverterDelegate = rawPropertyMappingTable[i, 2] as ADStoreCtx.FromLdapConverterDelegate;
				ADStoreCtx.ToLdapConverterDelegate toLdapConverterDelegate = rawPropertyMappingTable[i, 3] as ADStoreCtx.ToLdapConverterDelegate;
				ADStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry = new ADStoreCtx.PropertyMappingTableEntry();
				propertyMappingTableEntry.propertyName = str;
				propertyMappingTableEntry.suggestedADPropertyName = str1;
				propertyMappingTableEntry.ldapToPapiConverter = fromLdapConverterDelegate;
				propertyMappingTableEntry.papiToLdapConverter = toLdapConverterDelegate;
				if (str1 != null)
				{
					if (!strs.ContainsKey(str))
					{
						string[] strArrays5 = new string[1];
						strArrays5[0] = str1;
						strs.Add(str, strArrays5);
					}
					else
					{
						string[] strArrays6 = new string[(int)strs[str].Length + 1];
						strs[str].CopyTo(strArrays6, 0);
						strArrays6[(int)strs[str].Length] = str1;
						strs[str] = strArrays6;
					}
				}
				if (toLdapConverterDelegate != null)
				{
					if (hashtables[str] == null)
					{
						hashtables[str] = new ArrayList();
					}
					((ArrayList)hashtables[str]).Add(propertyMappingTableEntry);
				}
				if (hashtables1[str] == null)
				{
					hashtables1[str] = new ArrayList();
				}
				((ArrayList)hashtables1[str]).Add(propertyMappingTableEntry);
				if (fromLdapConverterDelegate != null)
				{
					string lower = str1.ToLower(CultureInfo.InvariantCulture);
					if (arrayLists[lower] == null)
					{
						arrayLists[lower] = new ArrayList();
					}
					((ArrayList)arrayLists[lower]).Add(propertyMappingTableEntry);
				}
			}
			ADStoreCtx.propertyMappingTableByProperty.Add(mappingIndex, hashtables);
			ADStoreCtx.propertyMappingTableByLDAP.Add(mappingIndex, arrayLists);
			ADStoreCtx.propertyMappingTableByPropertyFull.Add(mappingIndex, hashtables1);
			StringCollection stringCollections = new StringCollection();
			StringCollection stringCollections1 = new StringCollection();
			StringCollection stringCollections2 = new StringCollection();
			StringCollection stringCollections3 = new StringCollection();
			StringCollection stringCollections4 = new StringCollection();
			string[] strArrays7 = StoreCtx.principalProperties;
			for (int j = 0; j < (int)strArrays7.Length; j++)
			{
				string str2 = strArrays7[j];
				if (strs.TryGetValue(str2, out strArrays))
				{
					string[] strArrays8 = strArrays;
					for (int k = 0; k < (int)strArrays8.Length; k++)
					{
						string str3 = strArrays8[k];
						stringCollections.Add(str3);
						stringCollections1.Add(str3);
						stringCollections2.Add(str3);
						stringCollections3.Add(str3);
						stringCollections4.Add(str3);
					}
				}
			}
			string[] strArrays9 = StoreCtx.authenticablePrincipalProperties;
			int num = 0;
			while (num < (int)strArrays9.Length)
			{
				string str4 = strArrays9[num];
				if (strs.TryGetValue(str4, out strArrays1))
				{
					string[] strArrays10 = strArrays1;
					for (int l = 0; l < (int)strArrays10.Length; l++)
					{
						string str5 = strArrays10[l];
						stringCollections1.Add(str5);
						stringCollections2.Add(str5);
						stringCollections3.Add(str5);
					}
				}
				num++;
			}
			string[] strArrays11 = StoreCtx.groupProperties;
			int num1 = 0;
			while (num1 < (int)strArrays11.Length)
			{
				string str6 = strArrays11[num1];
				if (strs.TryGetValue(str6, out strArrays2))
				{
					string[] strArrays12 = strArrays2;
					for (int m = 0; m < (int)strArrays12.Length; m++)
					{
						string str7 = strArrays12[m];
						stringCollections4.Add(str7);
					}
				}
				num1++;
			}
			string[] strArrays13 = StoreCtx.userProperties;
			int num2 = 0;
			while (num2 < (int)strArrays13.Length)
			{
				string str8 = strArrays13[num2];
				if (strs.TryGetValue(str8, out strArrays3))
				{
					string[] strArrays14 = strArrays3;
					for (int n = 0; n < (int)strArrays14.Length; n++)
					{
						string str9 = strArrays14[n];
						stringCollections2.Add(str9);
					}
				}
				num2++;
			}
			string[] strArrays15 = StoreCtx.computerProperties;
			int num3 = 0;
			while (num3 < (int)strArrays15.Length)
			{
				string str10 = strArrays15[num3];
				if (strs.TryGetValue(str10, out strArrays4))
				{
					string[] strArrays16 = strArrays4;
					for (int o = 0; o < (int)strArrays16.Length; o++)
					{
						string str11 = strArrays16[o];
						stringCollections3.Add(str11);
					}
				}
				num3++;
			}
			stringCollections.Add("objectClass");
			stringCollections1.Add("objectClass");
			stringCollections2.Add("objectClass");
			stringCollections3.Add("objectClass");
			stringCollections4.Add("objectClass");
			types.Add(typeof(Principal), stringCollections);
			types.Add(typeof(GroupPrincipal), stringCollections4);
			types.Add(typeof(AuthenticablePrincipal), stringCollections1);
			types.Add(typeof(UserPrincipal), stringCollections2);
			types.Add(typeof(ComputerPrincipal), stringCollections3);
			ADStoreCtx.TypeToLdapPropListMap.Add(mappingIndex, types);
		}

		protected static string MatchingDateTimeConverter(FilterBase filter, string suggestedAdProperty)
		{
			QbeMatchType value = (QbeMatchType)filter.Value;
			return ADStoreCtx.ExtensionTypeConverter(suggestedAdProperty, value.Value.GetType(), value.Value, value.Match);
		}

		protected static string MatchingIntConverter(FilterBase filter, string suggestedAdProperty)
		{
			QbeMatchType value = (QbeMatchType)filter.Value;
			return ADStoreCtx.ExtensionTypeConverter(suggestedAdProperty, value.Value.GetType(), value.Value, value.Match);
		}

		internal override void Move(StoreCtx originalStore, Principal p)
		{
			bool hasValue;
			string str = null;
			string rdnPrefix = p.ExtensionHelper.RdnPrefix;
			string valueForProperty = null;
			Type type = p.GetType();
			if (rdnPrefix != null)
			{
				if (p.GetChangeStatusForProperty("Principal.Name"))
				{
					str = string.Concat(rdnPrefix, "=", (string)p.GetValueForProperty("Principal.Name"));
					if (type.IsSubclassOf(typeof(GroupPrincipal)) || type.IsSubclassOf(typeof(UserPrincipal)) || type.IsSubclassOf(typeof(ComputerPrincipal)))
					{
						DirectoryRdnPrefixAttribute[] customAttributes = (DirectoryRdnPrefixAttribute[])Attribute.GetCustomAttributes(type.BaseType, typeof(DirectoryRdnPrefixAttribute), false);
						if (customAttributes != null)
						{
							string rdnPrefix1 = null;
							for (int i = 0; i < (int)customAttributes.Length; i++)
							{
								ContextType? context = customAttributes[i].Context;
								if (context.HasValue || rdnPrefix1 != null)
								{
									ContextType contextType = p.ContextType;
									ContextType? nullable = customAttributes[i].Context;
									if (contextType != nullable.GetValueOrDefault())
									{
										hasValue = false;
									}
									else
									{
										hasValue = nullable.HasValue;
									}
									if (!hasValue)
									{
										continue;
									}
								}
								rdnPrefix1 = customAttributes[i].RdnPrefix;

							}
							if (rdnPrefix1 != rdnPrefix)
							{
								valueForProperty = rdnPrefix1;
							}
						}
						else
						{
							throw new InvalidOperationException(StringResources.ExtensionInvalidClassAttributes);
						}
					}
				}
				SDSUtils.MoveDirectoryEntry((DirectoryEntry)p.GetUnderlyingObject(), this.ctxBase, str);
				p.LoadValueIntoProperty("Principal.Name", p.GetValueForProperty("Principal.Name"));
				if (valueForProperty != null)
				{
					((DirectoryEntry)p.GetUnderlyingObject()).Properties[valueForProperty].Value = (string)p.GetValueForProperty("Principal.Name");
				}
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.ExtensionInvalidClassAttributes);
			}
		}

		protected static void MultiStringFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.MultiScalarFromDirectoryEntry<string>(properties, suggestedAdProperty, p, propertyName);
		}

		protected static void MultiStringToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			SDSUtils.MultiStringToDirectoryEntryConverter(p, propertyName, de, suggestedAdProperty);
		}

		internal override Type NativeType(Principal p)
		{
			return typeof(DirectoryEntry);
		}

		protected static void ObjectClassFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			dSPropertyValueCollection item = properties[suggestedAdProperty];
			if (item.Count > 0)
			{
				p.LoadValueIntoProperty(propertyName, (string)item[item.Count - 1]);
			}
		}

		internal override object PushChangesToNative(Principal p)
		{
			string structuralObjectClass;
			ArrayList value = null;
			object obj;
			bool hasValue;
			string str;
			try
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				if (underlyingObject == null)
				{
					Type type = p.GetType();
					string rdnPrefix = p.ExtensionHelper.RdnPrefix;
					string str1 = null;
					string str2 = null;
					if (type != typeof(UserPrincipal))
					{
						if (type != typeof(GroupPrincipal))
						{
							if (type != typeof(ComputerPrincipal))
							{
								structuralObjectClass = p.ExtensionHelper.StructuralObjectClass;
								if (structuralObjectClass == null || rdnPrefix == null)
								{
									throw new InvalidOperationException(StringResources.ExtensionInvalidClassAttributes);
								}
								else
								{
									if (type.IsSubclassOf(typeof(GroupPrincipal)) || type.IsSubclassOf(typeof(UserPrincipal)) || type.IsSubclassOf(typeof(ComputerPrincipal)))
									{
										DirectoryRdnPrefixAttribute[] customAttributes = (DirectoryRdnPrefixAttribute[])Attribute.GetCustomAttributes(type.BaseType, typeof(DirectoryRdnPrefixAttribute), false);
										if (customAttributes != null)
										{
											string rdnPrefix1 = null;
											for (int i = 0; i < (int)customAttributes.Length; i++)
											{
												ContextType? context = customAttributes[i].Context;
												if (context.HasValue || rdnPrefix1 != null)
												{
													ContextType contextType = p.ContextType;
													ContextType? nullable = customAttributes[i].Context;
													if (contextType != nullable.GetValueOrDefault())
													{
														hasValue = false;
													}
													else
													{
														hasValue = nullable.HasValue;
													}
													if (!hasValue)
													{
														continue;
													}
												}
												rdnPrefix1 = customAttributes[i].RdnPrefix;
											}
											if (rdnPrefix1 != rdnPrefix)
											{
												str2 = rdnPrefix1;
											}
										}
										else
										{
											throw new InvalidOperationException(StringResources.ExtensionInvalidClassAttributes);
										}
									}
								}
							}
							else
							{
								structuralObjectClass = "computer";
							}
						}
						else
						{
							structuralObjectClass = "group";
						}
					}
					else
					{
						structuralObjectClass = "user";
					}
					if (p.GetChangeStatusForProperty("Principal.Name"))
					{
						string valueForProperty = (string)p.GetValueForProperty("Principal.Name");
						if (valueForProperty != null && valueForProperty.Length > 0)
						{
							str1 = ADUtils.EscapeDNComponent(valueForProperty);
						}
					}
					if (str1 == null && p.GetChangeStatusForProperty("Principal.SamAccountName"))
					{
						string valueForProperty1 = (string)p.GetValueForProperty("Principal.SamAccountName");
						int num = valueForProperty1.IndexOf('\\');
						if (num != valueForProperty1.Length - 1)
						{
							if (num != -1)
							{
								str = valueForProperty1.Substring(num + 1);
							}
							else
							{
								str = valueForProperty1;
							}
							valueForProperty1 = str;
						}
						if (valueForProperty1 != null && valueForProperty1.Length > 0)
						{
							str1 = ADUtils.EscapeDNComponent(valueForProperty1);
						}
					}
					if (rdnPrefix != null)
					{
						if (str1 != null)
						{
							string str3 = string.Concat(rdnPrefix, "=", str1);
							lock (this.ctxBaseLock)
							{
								underlyingObject = this.ctxBase.Children.Add(str3, structuralObjectClass);
							}
							if (str2 != null)
							{
								underlyingObject.Properties[str2].Value = str1;
							}
							this.InitializeNewDirectoryOptions(underlyingObject);
							p.UnderlyingObject = underlyingObject;
							if (type.IsSubclassOf(typeof(AuthenticablePrincipal)))
							{
								this.InitializeUserAccountControl((AuthenticablePrincipal)p);
							}
						}
						else
						{
							throw new InvalidOperationException(StringResources.NameMustBeSetToPersistPrincipal);
						}
					}
					else
					{
						throw new InvalidOperationException(StringResources.ExtensionInvalidClassAttributes);
					}
				}
				Hashtable item = (Hashtable)ADStoreCtx.propertyMappingTableByProperty[(object)this.MappingTableIndex];
				foreach (DictionaryEntry dictionaryEntry in value)
				{
					value = (ArrayList)dictionaryEntry.Value;
					IEnumerator enumerator = value.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							ADStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry = (ADStoreCtx.PropertyMappingTableEntry)dictionaryEntry;
							if (!p.GetChangeStatusForProperty(propertyMappingTableEntry.propertyName))
							{
								continue;
							}
							propertyMappingTableEntry.papiToLdapConverter(p, propertyMappingTableEntry.propertyName, underlyingObject, propertyMappingTableEntry.suggestedADPropertyName);
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
				obj = underlyingObject;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			return obj;
		}

		internal override object PushFilterToNativeSearcher(PrincipalSearcher ps)
		{
			if (ps.UnderlyingSearcher == null)
			{
				ps.UnderlyingSearcher = new DirectorySearcher(this.ctxBase);
				((DirectorySearcher)ps.UnderlyingSearcher).PageSize = ps.PageSize;
				((DirectorySearcher)ps.UnderlyingSearcher).ServerTimeLimit = new TimeSpan(0, 0, 30);
			}
			DirectorySearcher underlyingSearcher = (DirectorySearcher)ps.UnderlyingSearcher;
			Principal queryFilter = ps.QueryFilter;
			StringBuilder stringBuilder = new StringBuilder();
			if (queryFilter != null)
			{
				stringBuilder.Append(this.GetObjectClassPortion(queryFilter.GetType()));
				QbeFilterDescription qbeFilterDescription = base.BuildQbeFilterDescription(queryFilter);
				Hashtable item = (Hashtable)ADStoreCtx.filterPropertiesTable[(object)this.MappingTableIndex];
				foreach (FilterBase filtersToApply in qbeFilterDescription.FiltersToApply)
				{
					ADStoreCtx.FilterPropertyTableEntry filterPropertyTableEntry = (ADStoreCtx.FilterPropertyTableEntry)item[filtersToApply.GetType()];
					if (filterPropertyTableEntry != null)
					{
						stringBuilder.Append(filterPropertyTableEntry.converter(filtersToApply, filterPropertyTableEntry.suggestedADPropertyName));
					}
					else
					{
						object[] externalForm = new object[1];
						externalForm[0] = PropertyNamesExternal.GetExternalForm(filtersToApply.PropertyName);
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPropertyForQuery, externalForm));
					}
				}
				stringBuilder.Append(")");
			}
			else
			{
				stringBuilder.Append("(|(objectClass=user)(objectClass=computer)(objectClass=group))");
			}
			this.BuildPropertySet(queryFilter.GetType(), underlyingSearcher.PropertiesToLoad);
			underlyingSearcher.Filter = stringBuilder.ToString();
			return underlyingSearcher;
		}

		internal override ResultSet Query(PrincipalSearcher ps, int sizeLimit)
		{
			ResultSet resultSet;
			try
			{
				DirectorySearcher nativeSearcher = (DirectorySearcher)this.PushFilterToNativeSearcher(ps);
				int num = nativeSearcher.SizeLimit;
				if (sizeLimit != -1)
				{
					nativeSearcher.SizeLimit = sizeLimit;
				}
				SearchResultCollection searchResultCollections = nativeSearcher.FindAll();
				ADEntriesSet aDEntriesSet = new ADEntriesSet(searchResultCollections, this, ps.QueryFilter.GetType());
				nativeSearcher.SizeLimit = num;
				resultSet = aDEntriesSet;
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			return resultSet;
		}

		internal override Principal ResolveCrossStoreRefToPrincipal(object o)
		{
			StoreCtx queryCtx;
			string str = null;
			string str1 = null;
			Principal principal;
			try
			{
				DirectoryEntry directoryEntry = (DirectoryEntry)o;
				if (directoryEntry.Properties["objectSid"].Count != 0)
				{
					byte[] value = (byte[])directoryEntry.Properties["objectSid"].Value;
					SidType sidType = Utils.ClassifySID(value);
					if (sidType != SidType.FakeObject)
					{
						if (sidType != SidType.RealObjectFakeDomain)
						{
							UnsafeNativeMethods.IAdsObjectOptions nativeObject = (UnsafeNativeMethods.IAdsObjectOptions)this.ctxBase.NativeObject;
							string option = (string)nativeObject.GetOption(0);
							int num = 0;
							int num1 = Utils.LookupSid(option, this.credentials, value, out str, out str1, out num);
							if (num1 == 0)
							{
								ContextOptions aDDefaultContextOption = DefaultContextOptions.ADDefaultContextOption;
								PrincipalContext context = SDSCache.Domain.GetContext(str1, this.credentials, aDDefaultContextOption);
								queryCtx = context.QueryCtx;
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = num1;
								throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.ADStoreCtxCantResolveSidForCrossStore, objArray));
							}
						}
						else
						{
							queryCtx = this;
						}
						Principal principal1 = queryCtx.FindPrincipalByIdentRef(typeof(Principal), "ms-sid", (new SecurityIdentifier(value, 0)).ToString(), DateTime.UtcNow);
						if (principal1 == null)
						{
							throw new PrincipalOperationException(StringResources.ADStoreCtxFailedFindCrossStoreTarget);
						}
						else
						{
							principal = principal1;
						}
					}
					else
					{
						principal = this.ConstructFakePrincipalFromSID(value);
					}
				}
				else
				{
					throw new PrincipalOperationException(StringResources.ADStoreCtxCantRetrieveObjectSidForCrossStore);
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			return principal;
		}

		protected static void ScanACLForChangePasswordRight(ActiveDirectorySecurity adsSecurity, out bool denySelfFound, out bool denyWorldFound, out bool allowSelfFound, out bool allowWorldFound)
		{
			denySelfFound = false;
			denyWorldFound = false;
			allowSelfFound = false;
			allowWorldFound = false;
			foreach (ActiveDirectoryAccessRule accessRule in adsSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
			{
				SecurityIdentifier identityReference = (SecurityIdentifier)accessRule.IdentityReference;
				string value = identityReference.Value;
				if (accessRule.ObjectType != ADStoreCtx.ChangePasswordGuid)
				{
					continue;
				}
				if (accessRule.AccessControlType != AccessControlType.Deny)
				{
					if (accessRule.AccessControlType != AccessControlType.Allow)
					{
						continue;
					}
					if (value != "S-1-5-10")
					{
						if (value != "S-1-1-0")
						{
							continue;
						}
						allowWorldFound = true;
					}
					else
					{
						allowSelfFound = true;
					}
				}
				else
				{
					if (value != "S-1-5-10")
					{
						if (value != "S-1-1-0")
						{
							continue;
						}
						denyWorldFound = true;
					}
					else
					{
						denySelfFound = true;
					}
				}
			}
		}

		internal override Type SearcherNativeType()
		{
			return typeof(DirectorySearcher);
		}

		protected static bool SecurityIdentityClaimConverterHelper(string urnValue, bool useSidHistory, StringBuilder filter, bool throwOnFail)
		{
			bool flag;
			IntPtr zero = IntPtr.Zero;
			byte[] byteArray = null;
			try
			{
				if (!UnsafeNativeMethods.ConvertStringSidToSid(urnValue, ref zero))
				{
					if (!throwOnFail)
					{
						flag = false;
					}
					else
					{
						throw new ArgumentException(StringResources.StoreCtxSecurityIdentityClaimBadFormat);
					}
				}
				else
				{
					byteArray = Utils.ConvertNativeSidToByteArray(zero);
					if (byteArray != null)
					{
						goto Label0;
					}
					else
					{
						if (!throwOnFail)
						{
							flag = false;
						}
						else
						{
							throw new ArgumentException(StringResources.StoreCtxSecurityIdentityClaimBadFormat);
						}
					}
				}
			}
			finally
			{
				if (IntPtr.Zero != zero)
				{
					UnsafeNativeMethods.LocalFree(zero);
				}
			}
			return flag;
		Label0:
			StringBuilder stringBuilder = new StringBuilder();
			byte[] numArray = byteArray;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				byte num = numArray[i];
				stringBuilder.Append(num.ToString("x2", CultureInfo.InvariantCulture));
			}
			string ldapHexString = ADUtils.HexStringToLdapHexString(stringBuilder.ToString());
			if (ldapHexString != null)
			{
				if (!useSidHistory)
				{
					filter.Append("(objectSid=");
					filter.Append(ldapHexString);
					filter.Append(")");
				}
				else
				{
					filter.Append("(|(objectSid=");
					filter.Append(ldapHexString);
					filter.Append(")(sidHistory=");
					filter.Append(ldapHexString);
					filter.Append("))");
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		protected virtual void SetAuthPrincipalEnableStatus(AuthenticablePrincipal ap, bool enable)
		{
			try
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)ap.UnderlyingObject;
				if (underlyingObject.Properties["userAccountControl"].Count <= 0)
				{
					throw new PrincipalOperationException(StringResources.ADStoreCtxUnableToReadExistingAccountControlFlagsToEnable);
				}
				else
				{
					int item = (int)underlyingObject.Properties["userAccountControl"][0];
					if (!enable || (item & 2) == 0)
					{
						if (!enable && (item & 2) == 0)
						{
							Utils.SetBit(ref item, 2);
							this.WriteAttribute(ap, "userAccountControl", item);
						}
					}
					else
					{
						Utils.ClearBit(ref item, 2);
						this.WriteAttribute(ap, "userAccountControl", item);
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		private static void SetCannotChangePasswordStatus(Principal ap, bool userCannotChangePassword, bool commitChanges)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			DirectoryEntry underlyingObject = (DirectoryEntry)ap.GetUnderlyingObject();
			if (!underlyingObject.Properties.Contains("nTSecurityDescriptor"))
			{
				string[] strArrays = new string[1];
				strArrays[0] = "nTSecurityDescriptor";
				underlyingObject.RefreshCache(strArrays);
			}
			ActiveDirectorySecurity objectSecurity = underlyingObject.ObjectSecurity;
			ADStoreCtx.ScanACLForChangePasswordRight(objectSecurity, out flag, out flag1, out flag2, out flag3);
			ActiveDirectoryAccessRule extendedRightAccessRule = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-5-10"), AccessControlType.Deny, ADStoreCtx.ChangePasswordGuid);
			ActiveDirectoryAccessRule activeDirectoryAccessRule = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-1-0"), AccessControlType.Deny, ADStoreCtx.ChangePasswordGuid);
			ActiveDirectoryAccessRule extendedRightAccessRule1 = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-5-10"), AccessControlType.Allow, ADStoreCtx.ChangePasswordGuid);
			ActiveDirectoryAccessRule activeDirectoryAccessRule1 = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-1-0"), AccessControlType.Allow, ADStoreCtx.ChangePasswordGuid);
			if (!userCannotChangePassword)
			{
				if (flag)
				{
					objectSecurity.RemoveAccessRuleSpecific(extendedRightAccessRule);
				}
				if (flag1)
				{
					objectSecurity.RemoveAccessRuleSpecific(activeDirectoryAccessRule);
				}
				if (!flag2)
				{
					objectSecurity.AddAccessRule(extendedRightAccessRule1);
				}
				if (!flag3)
				{
					objectSecurity.AddAccessRule(activeDirectoryAccessRule1);
				}
			}
			else
			{
				if (!flag)
				{
					objectSecurity.AddAccessRule(extendedRightAccessRule);
				}
				if (!flag1)
				{
					objectSecurity.AddAccessRule(activeDirectoryAccessRule);
				}
				if (flag2)
				{
					objectSecurity.RemoveAccessRuleSpecific(extendedRightAccessRule1);
				}
				if (flag3)
				{
					objectSecurity.RemoveAccessRuleSpecific(activeDirectoryAccessRule1);
				}
			}
			if (commitChanges)
			{
				underlyingObject.CommitChanges();
			}
		}

		internal override void SetPassword(AuthenticablePrincipal p, string newPassword)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			SDSUtils.SetPassword(underlyingObject, newPassword);
		}

		private void SetPasswordSecurityifNeccessary(Principal p)
		{
			if (p.GetChangeStatusForProperty("AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword"))
			{
				ADStoreCtx.SetCannotChangePasswordStatus((AuthenticablePrincipal)p, (bool)p.GetValueForProperty("AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword"), true);
			}
		}

		protected static void SidFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			if (properties["objectSid"].Count <= 0)
			{
				p.LoadValueIntoProperty(propertyName, null);
				return;
			}
			else
			{
				byte[] item = (byte[])properties["objectSid"][0];
				SecurityIdentifier securityIdentifier = new SecurityIdentifier(item, 0);
				p.LoadValueIntoProperty(propertyName, securityIdentifier);
				return;
			}
		}

		protected static string StringConverter(FilterBase filter, string suggestedAdProperty)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (filter.Value == null)
			{
				stringBuilder.Append("(!(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=*))");
			}
			else
			{
				stringBuilder.Append("(");
				stringBuilder.Append(suggestedAdProperty);
				stringBuilder.Append("=");
				stringBuilder.Append(ADUtils.PAPIQueryToLdapQueryString((string)filter.Value));
				stringBuilder.Append(")");
			}
			return stringBuilder.ToString();
		}

		protected static void StringFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.SingleScalarFromDirectoryEntry<string>(properties, suggestedAdProperty, p, propertyName);
		}

		protected static void StringToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			string valueForProperty = (string)p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty != null)
			{
				if (valueForProperty == null || valueForProperty.Length > 0)
				{
					de.Properties[suggestedAdProperty].Value = valueForProperty;
					return;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = propertyName;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidStringValueForStore, objArray));
				}
			}
			else
			{
				return;
			}
		}

		internal override CredentialTypes SupportedCredTypes(AuthenticablePrincipal p)
		{
			if (!p.fakePrincipal)
			{
				return CredentialTypes.Password | CredentialTypes.Certificate;
			}
			else
			{
				return 0;
			}
		}

		internal override bool SupportsAccounts(AuthenticablePrincipal p)
		{
			if (!p.fakePrincipal)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		protected static void UACFromLdapConverter(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName)
		{
			SDSUtils.AccountControlFromDirectoryEntry(properties, suggestedAdProperty, p, propertyName, false);
		}

		protected static void UACToLdapConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty)
		{
			SDSUtils.AccountControlToDirectoryEntry(p, propertyName, de, suggestedAdProperty, false, p.unpersisted);
		}

		internal override void UnexpirePassword(AuthenticablePrincipal p)
		{
			this.WriteAttribute(p, "pwdLastSet", -1);
		}

		internal override void UnlockAccount(AuthenticablePrincipal p)
		{
			this.WriteAttribute(p, "lockoutTime", 0);
		}

		internal override void Update(Principal p)
		{
			try
			{
				SDSUtils.ApplyChangesToDirectory(p, this, new SDSUtils.GroupMembershipUpdater(ADStoreCtx.UpdateGroupMembership), this.credentials, this.authTypes);
				p.ResetAllChangeStatus();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		protected static void UpdateGroupMembership(Principal group, DirectoryEntry de, NetCred credentials, AuthenticationTypes authTypes)
		{
			PrincipalCollection valueForProperty = (PrincipalCollection)group.GetValueForProperty("GroupPrincipal.Members");
			DirectoryEntry directoryEntry = null;
			using (directoryEntry)
			{
				try
				{
					if (valueForProperty.Cleared)
					{
						DirectoryEntry directoryEntry1 = null;
						using (directoryEntry1)
						{
							directoryEntry1 = SDSUtils.BuildDirectoryEntry(de.Path, credentials, authTypes);
							directoryEntry1.Properties["member"].Clear();
							directoryEntry1.CommitChanges();
						}
					}
					List<Principal> inserted = valueForProperty.Inserted;
					List<Principal> removed = valueForProperty.Removed;
					if (inserted.Count > 0 || removed.Count > 0)
					{
						directoryEntry = SDSUtils.BuildDirectoryEntry(de.Path, credentials, authTypes);
					}
					foreach (Principal principal in inserted)
					{
						Type type = principal.GetType();
						if (!(type != typeof(UserPrincipal)) || type.IsSubclassOf(typeof(UserPrincipal)) || !(type != typeof(ComputerPrincipal)) || type.IsSubclassOf(typeof(ComputerPrincipal)) || !(type != typeof(GroupPrincipal)) || type.IsSubclassOf(typeof(GroupPrincipal)) || type.IsSubclassOf(typeof(AuthenticablePrincipal)))
						{
							if (!principal.unpersisted)
							{
								if (principal.ContextType != ContextType.Machine)
								{
									continue;
								}
								throw new InvalidOperationException(StringResources.ADStoreCtxUnsupportedPrincipalContextForGroupInsert);
							}
							else
							{
								throw new InvalidOperationException(StringResources.StoreCtxGroupHasUnpersistedInsertedPrincipal);
							}
						}
						else
						{
							object[] str = new object[1];
							str[0] = type.ToString();
							throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPrincipalTypeForGroupInsert, str));
						}
					}
					foreach (Principal principal1 in inserted)
					{
						if (principal1.fakePrincipal || !ADUtils.ArePrincipalsInSameForest(group, principal1))
						{
							string sidPathFromPrincipal = ADStoreCtx.GetSidPathFromPrincipal(principal1);
							if (sidPathFromPrincipal != null)
							{
								directoryEntry.Properties["member"].Add(sidPathFromPrincipal);
							}
							else
							{
								throw new PrincipalOperationException(StringResources.ADStoreCtxCouldntGetSIDForGroupMember);
							}
						}
						else
						{
							directoryEntry.Properties["member"].Add(principal1.DistinguishedName);
						}
					}
					if (inserted.Count > 0)
					{
						directoryEntry.CommitChanges();
					}
					foreach (Principal principal2 in removed)
					{
						if (principal2.fakePrincipal || !ADUtils.ArePrincipalsInSameForest(group, principal2))
						{
							string sidPathFromPrincipal1 = ADStoreCtx.GetSidPathFromPrincipal(principal2);
							if (sidPathFromPrincipal1 != null)
							{
								directoryEntry.Properties["member"].Remove(sidPathFromPrincipal1);
							}
							else
							{
								throw new PrincipalOperationException(StringResources.ADStoreCtxCouldntGetSIDForGroupMember);
							}
						}
						else
						{
							directoryEntry.Properties["member"].Remove(principal2.DistinguishedName);
						}
					}
					if (removed.Count > 0)
					{
						directoryEntry.CommitChanges();
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
				}
			}
		}

		protected static string UserAccountControlConverter(FilterBase filter, string suggestedAdProperty)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool value = (bool)filter.Value;
			string propertyName = filter.PropertyName;
			string str = propertyName;
			if (propertyName != null)
			{
				switch (str)
				{
					case "AuthenticablePrincipal.Enabled":
					{
						if (!value)
						{
							stringBuilder.Append("(userAccountControl:1.2.840.113556.1.4.803:=2)");
							break;
						}
						else
						{
							stringBuilder.Append("(!(userAccountControl:1.2.840.113556.1.4.803:=2))");
							break;
						}
					}
					case "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired":
					{
						if (!value)
						{
							stringBuilder.Append("(!(userAccountControl:1.2.840.113556.1.4.803:=262144))");
							break;
						}
						else
						{
							stringBuilder.Append("(userAccountControl:1.2.840.113556.1.4.803:=262144)");
							break;
						}
					}
					case "AuthenticablePrincipal.AccountInfo.DelegationPermitted":
					{
						if (!value)
						{
							stringBuilder.Append("(userAccountControl:1.2.840.113556.1.4.803:=1048576)");
							break;
						}
						else
						{
							stringBuilder.Append("(!(userAccountControl:1.2.840.113556.1.4.803:=1048576))");
							break;
						}
					}
					case "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired":
					{
						if (!value)
						{
							stringBuilder.Append("(!(userAccountControl:1.2.840.113556.1.4.803:=32))");
							break;
						}
						else
						{
							stringBuilder.Append("(userAccountControl:1.2.840.113556.1.4.803:=32)");
							break;
						}
					}
					case "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires":
					{
						if (!value)
						{
							stringBuilder.Append("(!(userAccountControl:1.2.840.113556.1.4.803:=65536))");
							break;
						}
						else
						{
							stringBuilder.Append("(userAccountControl:1.2.840.113556.1.4.803:=65536)");
							break;
						}
					}
					case "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword":
					{
						object[] externalForm = new object[1];
						externalForm[0] = PropertyNamesExternal.GetExternalForm(filter.PropertyName);
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPropertyForQuery, externalForm));
					}
					case "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption":
					{
						if (!value)
						{
							stringBuilder.Append("(!(userAccountControl:1.2.840.113556.1.4.803:=128))");
							break;
						}
						else
						{
							stringBuilder.Append("(userAccountControl:1.2.840.113556.1.4.803:=128)");
							break;
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		protected void WriteAttribute(Principal p, string attribute, int value)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			SDSUtils.WriteAttribute(underlyingObject.Path, attribute, value, this.credentials, this.authTypes);
		}

		protected void WriteAttribute<T>(Principal p, string attribute, T value)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			SDSUtils.WriteAttribute<T>(underlyingObject.Path, attribute, value, this.credentials, this.authTypes);
		}

		protected delegate string FilterConverterDelegate(FilterBase filter, string suggestedAdProperty);

		private class FilterPropertyTableEntry
		{
			internal string suggestedADPropertyName;

			internal ADStoreCtx.FilterConverterDelegate converter;

			public FilterPropertyTableEntry()
			{
			}
		}

		protected delegate void FromLdapConverterDelegate(dSPropertyCollection properties, string suggestedAdProperty, Principal p, string propertyName);

		private class PropertyMappingTableEntry
		{
			internal string propertyName;

			internal string suggestedADPropertyName;

			internal ADStoreCtx.FromLdapConverterDelegate ldapToPapiConverter;

			internal ADStoreCtx.ToLdapConverterDelegate papiToLdapConverter;

			public PropertyMappingTableEntry()
			{
			}
		}

		protected enum StoreCapabilityMap
		{
			ASQSearch = 1
		}

		protected delegate void ToLdapConverterDelegate(Principal p, string propertyName, DirectoryEntry de, string suggestedAdProperty);
	}
}