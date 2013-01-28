using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class ADAMStoreCtx : ADStoreCtx
	{
		private const int mappingIndex = 1;

		private List<string> cachedBindableObjectList;

		private string cachedBindableObjectFilter;

		private object objectListLock;

		private static object[,] PresenceStateTable;

		private static object[,] propertyMappingTableRaw;

		private static object[,] filterPropertiesTableRaw;

		protected override int MappingTableIndex
		{
			get
			{
				return 1;
			}
		}

		static ADAMStoreCtx()
		{
			bool flag;
			object[,] objArray = new object[3, 2];
			objArray[0, 0] = "ms-DS-UserPasswordNotRequired";
			objArray[0, 1] = "FALSE";
			objArray[1, 0] = "msDS-UserDontExpirePassword";
			objArray[1, 1] = "FALSE";
			objArray[2, 0] = "ms-DS-UserEncryptedTextPasswordAllowed";
			objArray[2, 1] = "FALSE";
			ADAMStoreCtx.PresenceStateTable = objArray;
			object[,] fromLdapConverterDelegate = new object[39, 4];
			fromLdapConverterDelegate[0, 0] = "Principal.Description";
			fromLdapConverterDelegate[0, 1] = "description";
			fromLdapConverterDelegate[0, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[0, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[1, 0] = "Principal.DisplayName";
			fromLdapConverterDelegate[1, 1] = "displayName";
			fromLdapConverterDelegate[1, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[1, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[2, 0] = "Principal.DistinguishedName";
			fromLdapConverterDelegate[2, 1] = "distinguishedName";
			fromLdapConverterDelegate[2, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[2, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[3, 0] = "Principal.Sid";
			fromLdapConverterDelegate[3, 1] = "objectSid";
			fromLdapConverterDelegate[3, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.SidFromLdapConverter);
			fromLdapConverterDelegate[4, 0] = "Principal.SamAccountName";
			fromLdapConverterDelegate[4, 1] = "name";
			fromLdapConverterDelegate[5, 0] = "Principal.UserPrincipalName";
			fromLdapConverterDelegate[5, 1] = "userPrincipalName";
			fromLdapConverterDelegate[5, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[5, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[6, 0] = "Principal.Guid";
			fromLdapConverterDelegate[6, 1] = "objectGuid";
			fromLdapConverterDelegate[6, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GuidFromLdapConverter);
			fromLdapConverterDelegate[7, 0] = "Principal.StructuralObjectClass";
			fromLdapConverterDelegate[7, 1] = "objectClass";
			fromLdapConverterDelegate[7, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.ObjectClassFromLdapConverter);
			fromLdapConverterDelegate[8, 0] = "Principal.Name";
			fromLdapConverterDelegate[8, 1] = "name";
			fromLdapConverterDelegate[8, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[8, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[9, 0] = "Principal.ExtensionCache";
			fromLdapConverterDelegate[9, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.ExtensionCacheToLdapConverter);
			fromLdapConverterDelegate[10, 0] = "AuthenticablePrincipal.Enabled";
			fromLdapConverterDelegate[10, 1] = "msDS-UserAccountDisabled";
			fromLdapConverterDelegate[10, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.AcctDisabledFromLdapConverter);
			fromLdapConverterDelegate[10, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.AcctDisabledToLdapConverter);
			fromLdapConverterDelegate[11, 0] = "AuthenticablePrincipal.Certificates";
			fromLdapConverterDelegate[11, 1] = "userCertificate";
			fromLdapConverterDelegate[11, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.CertFromLdapConverter);
			fromLdapConverterDelegate[11, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.CertToLdap);
			fromLdapConverterDelegate[12, 0] = "GroupPrincipal.IsSecurityGroup";
			fromLdapConverterDelegate[12, 1] = "groupType";
			fromLdapConverterDelegate[12, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GroupTypeFromLdapConverter);
			fromLdapConverterDelegate[12, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.GroupTypeToLdapConverter);
			fromLdapConverterDelegate[13, 0] = "GroupPrincipal.GroupScope";
			fromLdapConverterDelegate[13, 1] = "groupType";
			fromLdapConverterDelegate[13, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GroupTypeFromLdapConverter);
			fromLdapConverterDelegate[13, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.GroupTypeToLdapConverter);
			fromLdapConverterDelegate[14, 0] = "UserPrincipal.GivenName";
			fromLdapConverterDelegate[14, 1] = "givenName";
			fromLdapConverterDelegate[14, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[14, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[15, 0] = "UserPrincipal.MiddleName";
			fromLdapConverterDelegate[15, 1] = "middleName";
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
			fromLdapConverterDelegate[18, 1] = "telephoneNumber";
			fromLdapConverterDelegate[18, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[18, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[19, 0] = "UserPrincipal.EmployeeId";
			fromLdapConverterDelegate[19, 1] = "employeeID";
			fromLdapConverterDelegate[19, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.StringFromLdapConverter);
			fromLdapConverterDelegate[19, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.StringToLdapConverter);
			fromLdapConverterDelegate[20, 0] = "ComputerPrincipal.ServicePrincipalNames";
			fromLdapConverterDelegate[20, 1] = "servicePrincipalName";
			fromLdapConverterDelegate[20, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.MultiStringFromLdapConverter);
			fromLdapConverterDelegate[20, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.MultiStringToLdapConverter);
			fromLdapConverterDelegate[21, 0] = "AuthenticablePrincipal.AccountInfo.AccountLockoutTime";
			fromLdapConverterDelegate[21, 1] = "lockoutTime";
			fromLdapConverterDelegate[21, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GenericDateTimeFromLdapConverter);
			fromLdapConverterDelegate[22, 0] = "AuthenticablePrincipal.AccountInfo.LastLogon";
			fromLdapConverterDelegate[22, 1] = "lastLogon";
			fromLdapConverterDelegate[22, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.LastLogonFromLdapConverter);
			fromLdapConverterDelegate[23, 0] = "AuthenticablePrincipal.AccountInfo.LastLogon";
			fromLdapConverterDelegate[23, 1] = "lastLogonTimestamp";
			fromLdapConverterDelegate[23, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.LastLogonFromLdapConverter);
			fromLdapConverterDelegate[24, 0] = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";
			fromLdapConverterDelegate[24, 1] = "userWorkstations";
			fromLdapConverterDelegate[25, 0] = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";
			fromLdapConverterDelegate[25, 1] = "logonHours";
			fromLdapConverterDelegate[26, 0] = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";
			fromLdapConverterDelegate[26, 1] = "accountExpires";
			fromLdapConverterDelegate[26, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.AcctExpirFromLdapConverter);
			fromLdapConverterDelegate[26, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.AcctExpirToLdapConverter);
			fromLdapConverterDelegate[27, 0] = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";
			fromLdapConverterDelegate[27, 1] = "userAccountControl";
			fromLdapConverterDelegate[28, 0] = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";
			fromLdapConverterDelegate[28, 1] = "userAccountControl";
			fromLdapConverterDelegate[29, 0] = "AuthenticablePrincipal.AccountInfo.BadLogonCount";
			fromLdapConverterDelegate[29, 1] = "badPwdCount";
			fromLdapConverterDelegate[29, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.IntFromLdapConverter);
			fromLdapConverterDelegate[30, 0] = "AuthenticablePrincipal.AccountInfo.HomeDirectory";
			fromLdapConverterDelegate[30, 1] = "homeDirectory";
			fromLdapConverterDelegate[31, 0] = "AuthenticablePrincipal.AccountInfo.HomeDrive";
			fromLdapConverterDelegate[31, 1] = "homeDrive";
			fromLdapConverterDelegate[32, 0] = "AuthenticablePrincipal.AccountInfo.ScriptPath";
			fromLdapConverterDelegate[32, 1] = "scriptPath";
			fromLdapConverterDelegate[33, 0] = "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";
			fromLdapConverterDelegate[33, 1] = "pwdLastSet";
			fromLdapConverterDelegate[33, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GenericDateTimeFromLdapConverter);
			fromLdapConverterDelegate[34, 0] = "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";
			fromLdapConverterDelegate[34, 1] = "badPasswordTime";
			fromLdapConverterDelegate[34, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.GenericDateTimeFromLdapConverter);
			fromLdapConverterDelegate[35, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";
			fromLdapConverterDelegate[35, 1] = "ms-DS-UserPasswordNotRequired";
			fromLdapConverterDelegate[35, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.BoolFromLdapConverter);
			fromLdapConverterDelegate[35, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.BoolToLdapConverter);
			fromLdapConverterDelegate[36, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";
			fromLdapConverterDelegate[36, 1] = "msDS-UserDontExpirePassword";
			fromLdapConverterDelegate[36, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.BoolFromLdapConverter);
			fromLdapConverterDelegate[36, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.BoolToLdapConverter);
			fromLdapConverterDelegate[37, 0] = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";
			fromLdapConverterDelegate[37, 1] = "ntSecurityDescriptor";
			fromLdapConverterDelegate[37, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.CannotChangePwdToLdapConverter);
			fromLdapConverterDelegate[38, 0] = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";
			fromLdapConverterDelegate[38, 1] = "ms-DS-UserEncryptedTextPasswordAllowed";
			fromLdapConverterDelegate[38, 2] = new ADStoreCtx.FromLdapConverterDelegate(ADStoreCtx.BoolFromLdapConverter);
			fromLdapConverterDelegate[38, 3] = new ADStoreCtx.ToLdapConverterDelegate(ADStoreCtx.BoolToLdapConverter);
			ADAMStoreCtx.propertyMappingTableRaw = fromLdapConverterDelegate;
			object[,] filterConverterDelegate = new object[37, 3];
			filterConverterDelegate[0, 0] = typeof(DescriptionFilter);
			filterConverterDelegate[0, 1] = "description";
			filterConverterDelegate[0, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[1, 0] = typeof(DisplayNameFilter);
			filterConverterDelegate[1, 1] = "displayName";
			filterConverterDelegate[1, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[2, 0] = typeof(IdentityClaimFilter);
			filterConverterDelegate[2, 1] = "";
			filterConverterDelegate[2, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.IdentityClaimConverter);
			filterConverterDelegate[3, 0] = typeof(DistinguishedNameFilter);
			filterConverterDelegate[3, 1] = "distinguishedName";
			filterConverterDelegate[3, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[4, 0] = typeof(GuidFilter);
			filterConverterDelegate[4, 1] = "objectGuid";
			filterConverterDelegate[4, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.GuidConverter);
			filterConverterDelegate[5, 0] = typeof(UserPrincipalNameFilter);
			filterConverterDelegate[5, 1] = "userPrincipalName";
			filterConverterDelegate[5, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[6, 0] = typeof(StructuralObjectClassFilter);
			filterConverterDelegate[6, 1] = "objectClass";
			filterConverterDelegate[6, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[7, 0] = typeof(NameFilter);
			filterConverterDelegate[7, 1] = "name";
			filterConverterDelegate[7, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[8, 0] = typeof(CertificateFilter);
			filterConverterDelegate[8, 1] = "";
			filterConverterDelegate[8, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.CertificateConverter);
			filterConverterDelegate[9, 0] = typeof(AuthPrincEnabledFilter);
			filterConverterDelegate[9, 1] = "msDS-UserAccountDisabled";
			filterConverterDelegate[9, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.AcctDisabledConverter);
			filterConverterDelegate[10, 0] = typeof(PermittedWorkstationFilter);
			filterConverterDelegate[10, 1] = "userWorkstations";
			filterConverterDelegate[10, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[11, 0] = typeof(PermittedLogonTimesFilter);
			filterConverterDelegate[11, 1] = "logonHours";
			filterConverterDelegate[11, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.BinaryConverter);
			filterConverterDelegate[12, 0] = typeof(ExpirationDateFilter);
			filterConverterDelegate[12, 1] = "accountExpires";
			filterConverterDelegate[12, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.ExpirationDateConverter);
			filterConverterDelegate[13, 0] = typeof(SmartcardLogonRequiredFilter);
			filterConverterDelegate[13, 1] = "userAccountControl";
			filterConverterDelegate[13, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[14, 0] = typeof(DelegationPermittedFilter);
			filterConverterDelegate[14, 1] = "userAccountControl";
			filterConverterDelegate[14, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[15, 0] = typeof(HomeDirectoryFilter);
			filterConverterDelegate[15, 1] = "homeDirectory";
			filterConverterDelegate[15, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[16, 0] = typeof(HomeDriveFilter);
			filterConverterDelegate[16, 1] = "homeDrive";
			filterConverterDelegate[16, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[17, 0] = typeof(ScriptPathFilter);
			filterConverterDelegate[17, 1] = "scriptPath";
			filterConverterDelegate[17, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[18, 0] = typeof(PasswordNotRequiredFilter);
			filterConverterDelegate[18, 1] = "ms-DS-UserPasswordNotRequired";
			filterConverterDelegate[18, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValueBoolConverter);
			filterConverterDelegate[19, 0] = typeof(PasswordNeverExpiresFilter);
			filterConverterDelegate[19, 1] = "msDS-UserDontExpirePassword";
			filterConverterDelegate[19, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValueBoolConverter);
			filterConverterDelegate[20, 0] = typeof(CannotChangePasswordFilter);
			filterConverterDelegate[20, 1] = "userAccountControl";
			filterConverterDelegate[20, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.UserAccountControlConverter);
			filterConverterDelegate[21, 0] = typeof(AllowReversiblePasswordEncryptionFilter);
			filterConverterDelegate[21, 1] = "ms-DS-UserEncryptedTextPasswordAllowed";
			filterConverterDelegate[21, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValueBoolConverter);
			filterConverterDelegate[22, 0] = typeof(GivenNameFilter);
			filterConverterDelegate[22, 1] = "givenName";
			filterConverterDelegate[22, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[23, 0] = typeof(MiddleNameFilter);
			filterConverterDelegate[23, 1] = "middleName";
			filterConverterDelegate[23, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[24, 0] = typeof(SurnameFilter);
			filterConverterDelegate[24, 1] = "sn";
			filterConverterDelegate[24, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[25, 0] = typeof(EmailAddressFilter);
			filterConverterDelegate[25, 1] = "mail";
			filterConverterDelegate[25, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[26, 0] = typeof(VoiceTelephoneNumberFilter);
			filterConverterDelegate[26, 1] = "telephoneNumber";
			filterConverterDelegate[26, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[27, 0] = typeof(EmployeeIDFilter);
			filterConverterDelegate[27, 1] = "employeeID";
			filterConverterDelegate[27, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[28, 0] = typeof(GroupIsSecurityGroupFilter);
			filterConverterDelegate[28, 1] = "groupType";
			filterConverterDelegate[28, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.GroupTypeConverter);
			filterConverterDelegate[29, 0] = typeof(GroupScopeFilter);
			filterConverterDelegate[29, 1] = "groupType";
			filterConverterDelegate[29, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.GroupTypeConverter);
			filterConverterDelegate[30, 0] = typeof(ServicePrincipalNameFilter);
			filterConverterDelegate[30, 1] = "servicePrincipalName";
			filterConverterDelegate[30, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.StringConverter);
			filterConverterDelegate[31, 0] = typeof(ExtensionCacheFilter);
			filterConverterDelegate[31, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.ExtensionCacheConverter);
			filterConverterDelegate[32, 0] = typeof(BadPasswordAttemptFilter);
			filterConverterDelegate[32, 1] = "badPasswordTime";
			filterConverterDelegate[32, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValutMatchingDateTimeConverter);
			filterConverterDelegate[33, 0] = typeof(ExpiredAccountFilter);
			filterConverterDelegate[33, 1] = "accountExpires";
			filterConverterDelegate[33, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.MatchingDateTimeConverter);
			filterConverterDelegate[34, 0] = typeof(LastLogonTimeFilter);
			filterConverterDelegate[34, 1] = "lastLogonTimestamp";
			filterConverterDelegate[34, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValutMatchingDateTimeConverter);
			filterConverterDelegate[35, 0] = typeof(LockoutTimeFilter);
			filterConverterDelegate[35, 1] = "lockoutTime";
			filterConverterDelegate[35, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValutMatchingDateTimeConverter);
			filterConverterDelegate[36, 0] = typeof(PasswordSetTimeFilter);
			filterConverterDelegate[36, 1] = "pwdLastSet";
			filterConverterDelegate[36, 2] = new ADStoreCtx.FilterConverterDelegate(ADStoreCtx.DefaultValutMatchingDateTimeConverter);
			ADAMStoreCtx.filterPropertiesTableRaw = filterConverterDelegate;
			ADStoreCtx.LoadFilterMappingTable(1, ADAMStoreCtx.filterPropertiesTableRaw);
			ADStoreCtx.LoadPropertyMappingTable(1, ADAMStoreCtx.propertyMappingTableRaw);
			if (ADStoreCtx.NonPresentAttrDefaultStateMapping == null)
			{
				ADStoreCtx.NonPresentAttrDefaultStateMapping = new Dictionary<string, bool>();
			}
			for (int i = 0; i < ADAMStoreCtx.PresenceStateTable.GetLength(0); i++)
			{
				string presenceStateTable = ADAMStoreCtx.PresenceStateTable[i, 0] as string;
				string str = ADAMStoreCtx.PresenceStateTable[i, 1] as string;
				Dictionary<string, bool> nonPresentAttrDefaultStateMapping = ADStoreCtx.NonPresentAttrDefaultStateMapping;
				string str1 = presenceStateTable;
				if (str == "FALSE")
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
				nonPresentAttrDefaultStateMapping.Add(str1, flag);
			}
		}

		public ADAMStoreCtx(DirectoryEntry ctxBase, bool ownCtxBase, string username, string password, string serverName, ContextOptions options) : base(ctxBase, ownCtxBase, username, password, options)
		{
			this.objectListLock = new object();
			this.userSuppliedServerName = serverName;
		}

		internal override void ChangePassword(AuthenticablePrincipal p, string oldPassword, string newPassword)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			this.SetupPasswordModification(p);
			SDSUtils.ChangePassword(underlyingObject, oldPassword, newPassword);
		}

		internal override ResultSet GetGroupsMemberOfAZ(Principal p)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			string value = (string)underlyingObject.Properties["distinguishedName"].Value;
			return new TokenGroupSet(value, this, true);
		}

		protected override string GetObjectClassPortion(Type principalType)
		{
			string str;
			if (principalType == typeof(AuthenticablePrincipal) || principalType == typeof(Principal))
			{
				lock (this.objectListLock)
				{
					if (this.cachedBindableObjectList == null)
					{
						this.cachedBindableObjectList = this.PopulatAuxObjectList("msDS-BindableObject");
					}
					if (this.cachedBindableObjectFilter == null)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append("(&(|");
						foreach (string str1 in this.cachedBindableObjectList)
						{
							stringBuilder.Append("(objectClass=");
							stringBuilder.Append(str1);
							stringBuilder.Append(")");
						}
						this.cachedBindableObjectFilter = stringBuilder.ToString();
					}
					if (principalType != typeof(Principal))
					{
						str = string.Concat(this.cachedBindableObjectFilter, ")");
					}
					else
					{
						str = string.Concat(this.cachedBindableObjectFilter, "(objectClass=group))");
					}
				}
				return str;
			}
			else
			{
				return base.GetObjectClassPortion(principalType);
			}
		}

		protected internal override void InitializeNewDirectoryOptions(DirectoryEntry newDeChild)
		{
			newDeChild.Options.PasswordPort = this.ctxBase.Options.PasswordPort;
		}

		internal override void InitializeUserAccountControl(AuthenticablePrincipal p)
		{
		}

		protected override void LoadDomainInfo()
		{
			this.dnsHostName = ADUtils.GetServerName(this.ctxBase);
			this.domainFlatName = this.userSuppliedServerName;
			this.forestDnsName = this.userSuppliedServerName;
			this.domainDnsName = this.userSuppliedServerName;
			using (DirectoryEntry directoryEntry = new DirectoryEntry(string.Concat("LDAP://", this.userSuppliedServerName, "/rootDse"), "", "", AuthenticationTypes.Anonymous))
			{
				string item = (string)this.ctxBase.Properties["distinguishedName"][0];
				int length = -1;
				foreach (string str in directoryEntry.Properties["namingContexts"])
				{
					if (str.Length <= length || !item.EndsWith(str, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					length = str.Length;
					this.contextBasePartitionDN = str;
				}
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

		private List<string> PopulatAuxObjectList(string auxClassName)
		{
			string value;
			List<string> strs;
			string userName;
			string password;
			string str;
			string password1;
			try
			{
				string str1 = string.Concat("LDAP://", this.userSuppliedServerName, "/rootDSE");
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
				using (DirectoryEntry directoryEntry = new DirectoryEntry(str1, userName, password, this.authTypes))
				{
					if (directoryEntry.Properties["schemaNamingContext"].Count != 0)
					{
						value = (string)directoryEntry.Properties["schemaNamingContext"].Value;
					}
					else
					{
						throw new PrincipalOperationException(StringResources.ADAMStoreUnableToPopulateSchemaList);
					}
				}
				string str2 = string.Concat("LDAP://", this.userSuppliedServerName, "/", value);
				if (this.credentials == null)
				{
					str = null;
				}
				else
				{
					str = this.credentials.UserName;
				}
				if (this.credentials == null)
				{
					password1 = null;
				}
				else
				{
					password1 = this.credentials.Password;
				}
				using (DirectoryEntry directoryEntry1 = new DirectoryEntry(str2, str, password1, this.authTypes))
				{
					using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry1))
					{
						directorySearcher.Filter = string.Concat("(&(objectClass=classSchema)(systemAuxiliaryClass=", auxClassName, "))");
						directorySearcher.PropertiesToLoad.Add("ldapDisplayName");
						List<string> strs1 = new List<string>();
						SearchResultCollection searchResultCollections = directorySearcher.FindAll();
						using (searchResultCollections)
						{
							foreach (SearchResult searchResult in searchResultCollections)
							{
								if (searchResult.Properties["ldapDisplayName"] != null)
								{
									strs1.Add(searchResult.Properties["ldapDisplayName"][0].ToString());
								}
								else
								{
									throw new PrincipalOperationException(StringResources.ADAMStoreUnableToPopulateSchemaList);
								}
							}
						}
						strs1.Add(auxClassName);
						strs = strs1;
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
			return strs;
		}

		protected override void SetAuthPrincipalEnableStatus(AuthenticablePrincipal ap, bool enable)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)ap.UnderlyingObject;
			if (underlyingObject.Properties["msDS-UserAccountDisabled"].Count <= 0)
			{
				throw new PrincipalOperationException(StringResources.ADStoreCtxUnableToReadExistingAccountControlFlagsToEnable);
			}
			else
			{
				bool item = (bool)underlyingObject.Properties["msDS-UserAccountDisabled"][0];
				if (enable && item || !enable && !item)
				{
					base.WriteAttribute<bool>(ap, "msDS-UserAccountDisabled", !enable);
				}
				return;
			}
		}

		internal override void SetPassword(AuthenticablePrincipal p, string newPassword)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			this.SetupPasswordModification(p);
			SDSUtils.SetPassword(underlyingObject, newPassword);
		}

		private void SetupPasswordModification(AuthenticablePrincipal p)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			if ((this.contextOptions & ContextOptions.Signing) != 0 && (this.contextOptions & ContextOptions.Sealing) != 0)
			{
				try
				{
					object[] objArray = new object[2];
					objArray[0] = UnsafeNativeMethods.ADS_OPTION_ENUM.ADS_OPTION_PASSWORD_METHOD;
					objArray[1] = UnsafeNativeMethods.ADS_PASSWORD_ENCODING_ENUM.ADS_PASSWORD_ENCODE_CLEAR;
					underlyingObject.Invoke("SetOption", objArray);
					underlyingObject.Options.PasswordPort = p.Context.ServerInformation.portLDAP;
				}
				catch (TargetInvocationException targetInvocationException1)
				{
					TargetInvocationException targetInvocationException = targetInvocationException1;
					if (targetInvocationException.InnerException as COMException == null)
					{
						throw;
					}
					else
					{
						throw ExceptionHelper.GetExceptionFromCOMException((COMException)targetInvocationException.InnerException);
					}
				}
			}
		}
	}
}