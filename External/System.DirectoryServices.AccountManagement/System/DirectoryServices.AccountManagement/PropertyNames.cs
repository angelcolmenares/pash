using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class PropertyNames
	{
		internal const string PrincipalDisplayName = "Principal.DisplayName";

		internal const string PrincipalDescription = "Principal.Description";

		internal const string PrincipalSamAccountName = "Principal.SamAccountName";

		internal const string PrincipalUserPrincipalName = "Principal.UserPrincipalName";

		internal const string PrincipalGuid = "Principal.Guid";

		internal const string PrincipalSid = "Principal.Sid";

		internal const string PrincipalIdentityClaims = "Principal.IdentityClaims";

		internal const string PrincipalDistinguishedName = "Principal.DistinguishedName";

		internal const string PrincipalStructuralObjectClass = "Principal.StructuralObjectClass";

		internal const string PrincipalName = "Principal.Name";

		internal const string PrincipalExtensionCache = "Principal.ExtensionCache";

		internal const string AuthenticablePrincipalEnabled = "AuthenticablePrincipal.Enabled";

		internal const string AuthenticablePrincipalCertificates = "AuthenticablePrincipal.Certificates";

		internal const string GroupIsSecurityGroup = "GroupPrincipal.IsSecurityGroup";

		internal const string GroupGroupScope = "GroupPrincipal.GroupScope";

		internal const string GroupMembers = "GroupPrincipal.Members";

		internal const string UserGivenName = "UserPrincipal.GivenName";

		internal const string UserMiddleName = "UserPrincipal.MiddleName";

		internal const string UserSurname = "UserPrincipal.Surname";

		internal const string UserEmailAddress = "UserPrincipal.EmailAddress";

		internal const string UserVoiceTelephoneNumber = "UserPrincipal.VoiceTelephoneNumber";

		internal const string UserEmployeeID = "UserPrincipal.EmployeeId";

		internal const string ComputerServicePrincipalNames = "ComputerPrincipal.ServicePrincipalNames";

		internal const string AcctInfoPrefix = "AuthenticablePrincipal.AccountInfo";

		internal const string AcctInfoAcctLockoutTime = "AuthenticablePrincipal.AccountInfo.AccountLockoutTime";

		internal const string AcctInfoLastLogon = "AuthenticablePrincipal.AccountInfo.LastLogon";

		internal const string AcctInfoPermittedWorkstations = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";

		internal const string AcctInfoPermittedLogonTimes = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";

		internal const string AcctInfoExpirationDate = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";

		internal const string AcctInfoSmartcardRequired = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";

		internal const string AcctInfoDelegationPermitted = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";

		internal const string AcctInfoBadLogonCount = "AuthenticablePrincipal.AccountInfo.BadLogonCount";

		internal const string AcctInfoHomeDirectory = "AuthenticablePrincipal.AccountInfo.HomeDirectory";

		internal const string AcctInfoHomeDrive = "AuthenticablePrincipal.AccountInfo.HomeDrive";

		internal const string AcctInfoScriptPath = "AuthenticablePrincipal.AccountInfo.ScriptPath";

		internal const string AcctInfoExpiredAccount = "AuthenticablePrincipal.AccountInfoExpired";

		internal const string PwdInfoPrefix = "AuthenticablePrincipal.PasswordInfo";

		internal const string PwdInfoLastPasswordSet = "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";

		internal const string PwdInfoLastBadPasswordAttempt = "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";

		internal const string PwdInfoPasswordNotRequired = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";

		internal const string PwdInfoPasswordNeverExpires = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";

		internal const string PwdInfoCannotChangePassword = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";

		internal const string PwdInfoAllowReversiblePasswordEncryption = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";

		internal const string PwdInfoPassword = "AuthenticablePrincipal.PasswordInfo.Password";

		internal const string PwdInfoExpireImmediately = "AuthenticablePrincipal.PasswordInfo.ExpireImmediately";

		private PropertyNames()
		{
		}
	}
}