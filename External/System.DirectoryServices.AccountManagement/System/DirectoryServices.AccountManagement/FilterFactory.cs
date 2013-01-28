using System;
using System.Collections;

namespace System.DirectoryServices.AccountManagement
{
	internal class FilterFactory
	{
		private static Hashtable subclasses;

		static FilterFactory()
		{
			FilterFactory.subclasses = new Hashtable();
			FilterFactory.subclasses["Principal.Description"] = typeof(DescriptionFilter);
			FilterFactory.subclasses["Principal.DisplayName"] = typeof(DisplayNameFilter);
			FilterFactory.subclasses["Principal.IdentityClaims"] = typeof(IdentityClaimFilter);
			FilterFactory.subclasses["Principal.SamAccountName"] = typeof(SamAccountNameFilter);
			FilterFactory.subclasses["Principal.DistinguishedName"] = typeof(DistinguishedNameFilter);
			FilterFactory.subclasses["Principal.Guid"] = typeof(GuidFilter);
			FilterFactory.subclasses["Principal.UserPrincipalName"] = typeof(UserPrincipalNameFilter);
			FilterFactory.subclasses["Principal.StructuralObjectClass"] = typeof(StructuralObjectClassFilter);
			FilterFactory.subclasses["Principal.Name"] = typeof(NameFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.Certificates"] = typeof(CertificateFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.Enabled"] = typeof(AuthPrincEnabledFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.PermittedWorkstations"] = typeof(PermittedWorkstationFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.PermittedLogonTimes"] = typeof(PermittedLogonTimesFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.AccountExpirationDate"] = typeof(ExpirationDateFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired"] = typeof(SmartcardLogonRequiredFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.DelegationPermitted"] = typeof(DelegationPermittedFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.HomeDirectory"] = typeof(HomeDirectoryFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.HomeDrive"] = typeof(HomeDriveFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.ScriptPath"] = typeof(ScriptPathFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.PasswordInfo.PasswordNotRequired"] = typeof(PasswordNotRequiredFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires"] = typeof(PasswordNeverExpiresFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword"] = typeof(CannotChangePasswordFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption"] = typeof(AllowReversiblePasswordEncryptionFilter);
			FilterFactory.subclasses["UserPrincipal.GivenName"] = typeof(GivenNameFilter);
			FilterFactory.subclasses["UserPrincipal.MiddleName"] = typeof(MiddleNameFilter);
			FilterFactory.subclasses["UserPrincipal.Surname"] = typeof(SurnameFilter);
			FilterFactory.subclasses["UserPrincipal.EmailAddress"] = typeof(EmailAddressFilter);
			FilterFactory.subclasses["UserPrincipal.VoiceTelephoneNumber"] = typeof(VoiceTelephoneNumberFilter);
			FilterFactory.subclasses["UserPrincipal.EmployeeId"] = typeof(EmployeeIDFilter);
			FilterFactory.subclasses["GroupPrincipal.IsSecurityGroup"] = typeof(GroupIsSecurityGroupFilter);
			FilterFactory.subclasses["GroupPrincipal.GroupScope"] = typeof(GroupScopeFilter);
			FilterFactory.subclasses["ComputerPrincipal.ServicePrincipalNames"] = typeof(ServicePrincipalNameFilter);
			FilterFactory.subclasses["Principal.ExtensionCache"] = typeof(ExtensionCacheFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt"] = typeof(BadPasswordAttemptFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfoExpired"] = typeof(ExpiredAccountFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.LastLogon"] = typeof(LastLogonTimeFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.AccountLockoutTime"] = typeof(LockoutTimeFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.PasswordInfo.LastPasswordSet"] = typeof(PasswordSetTimeFilter);
			FilterFactory.subclasses["AuthenticablePrincipal.AccountInfo.BadLogonCount"] = typeof(BadLogonCountFilter);
		}

		private FilterFactory()
		{
		}

		public static object CreateFilter(string propertyName)
		{
			Type item = (Type)FilterFactory.subclasses[propertyName];
			return Activator.CreateInstance(item);
		}
	}
}