using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Security.Principal;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class SAMStoreCtx : StoreCtx
	{
		private DirectoryEntry ctxBase;

		private object ctxBaseLock;

		private bool ownCtxBase;

		private bool disposed;

		private NetCred credentials;

		private AuthenticationTypes authTypes;

		private ContextOptions contextOptions;

		private object computerInfoLock;

		private bool? isLSAM;

		private string machineUserSuppliedName;

		private string machineFlatName;

		private static object[,] propertyMappingTableRaw;

		private static Hashtable userPropertyMappingTableByProperty;

		private static Hashtable userPropertyMappingTableByWinNT;

		private static Hashtable groupPropertyMappingTableByProperty;

		private static Hashtable groupPropertyMappingTableByWinNT;

		private static Hashtable computerPropertyMappingTableByProperty;

		private static Hashtable computerPropertyMappingTableByWinNT;

		private static Dictionary<string, SAMStoreCtx.ObjectMask> ValidPropertyMap;

		private static Dictionary<Type, SAMStoreCtx.ObjectMask> MaskMap;

		internal AuthenticationTypes AuthTypes
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

		internal NetCred Credentials
		{
			get
			{
				return this.credentials;
			}
		}

		private bool IsLSAM
		{
			get
			{
				if (!this.isLSAM.HasValue)
				{
					lock (this.computerInfoLock)
					{
						if (!this.isLSAM.HasValue)
						{
							this.LoadComputerInfo();
						}
					}
				}
				return this.isLSAM.Value;
			}
		}

		internal string MachineFlatName
		{
			get
			{
				if (this.machineFlatName == null)
				{
					lock (this.computerInfoLock)
					{
						if (this.machineFlatName == null)
						{
							this.LoadComputerInfo();
						}
					}
				}
				return this.machineFlatName;
			}
		}

		internal string MachineUserSuppliedName
		{
			get
			{
				if (this.machineUserSuppliedName == null)
				{
					lock (this.computerInfoLock)
					{
						if (this.machineUserSuppliedName == null)
						{
							this.LoadComputerInfo();
						}
					}
				}
				return this.machineUserSuppliedName;
			}
		}

		internal override bool SupportsNativeMembershipTest
		{
			get
			{
				return false;
			}
		}

		internal override bool SupportsSearchNatively
		{
			get
			{
				return false;
			}
		}

		static SAMStoreCtx()
		{
			SAMStoreCtx.ObjectMask objectMask;
			SAMStoreCtx.ObjectMask objectMask1 = SAMStoreCtx.ObjectMask.None;
			object[,] fromWinNTConverterDelegate = new object[53, 5];
			fromWinNTConverterDelegate[0, 0] = "Principal.DisplayName";
			fromWinNTConverterDelegate[0, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[0, 2] = "FullName";
			fromWinNTConverterDelegate[0, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.StringFromWinNTConverter);
			fromWinNTConverterDelegate[0, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.StringToWinNTConverter);
			fromWinNTConverterDelegate[1, 0] = "Principal.Description";
			fromWinNTConverterDelegate[1, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[1, 2] = "Description";
			fromWinNTConverterDelegate[1, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.StringFromWinNTConverter);
			fromWinNTConverterDelegate[1, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.StringToWinNTConverter);
			fromWinNTConverterDelegate[2, 0] = "Principal.Description";
			fromWinNTConverterDelegate[2, 1] = typeof(GroupPrincipal);
			fromWinNTConverterDelegate[2, 2] = "Description";
			fromWinNTConverterDelegate[2, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.StringFromWinNTConverter);
			fromWinNTConverterDelegate[2, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.StringToWinNTConverter);
			fromWinNTConverterDelegate[3, 0] = "Principal.SamAccountName";
			fromWinNTConverterDelegate[3, 1] = typeof(Principal);
			fromWinNTConverterDelegate[3, 2] = "Name";
			fromWinNTConverterDelegate[3, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.SamAccountNameFromWinNTConverter);
			fromWinNTConverterDelegate[4, 0] = "Principal.Sid";
			fromWinNTConverterDelegate[4, 1] = typeof(Principal);
			fromWinNTConverterDelegate[4, 2] = "objectSid";
			fromWinNTConverterDelegate[4, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.SidFromWinNTConverter);
			fromWinNTConverterDelegate[5, 0] = "Principal.DistinguishedName";
			fromWinNTConverterDelegate[5, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[5, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[6, 0] = "Principal.Guid";
			fromWinNTConverterDelegate[6, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[6, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[7, 0] = "Principal.UserPrincipalName";
			fromWinNTConverterDelegate[7, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[7, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[8, 0] = "Principal.Name";
			fromWinNTConverterDelegate[8, 1] = typeof(Principal);
			fromWinNTConverterDelegate[8, 2] = "Name";
			fromWinNTConverterDelegate[8, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.SamAccountNameFromWinNTConverter);
			fromWinNTConverterDelegate[9, 0] = "AuthenticablePrincipal.Enabled";
			fromWinNTConverterDelegate[9, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[9, 2] = "UserFlags";
			fromWinNTConverterDelegate[9, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[9, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[10, 0] = "AuthenticablePrincipal.Certificates";
			fromWinNTConverterDelegate[10, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[10, 2] = "*******";
			fromWinNTConverterDelegate[10, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.CertFromWinNTConverter);
			fromWinNTConverterDelegate[10, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.CertToWinNT);
			fromWinNTConverterDelegate[11, 0] = "GroupPrincipal.IsSecurityGroup";
			fromWinNTConverterDelegate[11, 1] = typeof(GroupPrincipal);
			fromWinNTConverterDelegate[11, 2] = "*******";
			fromWinNTConverterDelegate[11, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.GroupTypeFromWinNTConverter);
			fromWinNTConverterDelegate[11, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.GroupTypeToWinNTConverter);
			fromWinNTConverterDelegate[12, 0] = "GroupPrincipal.GroupScope";
			fromWinNTConverterDelegate[12, 1] = typeof(GroupPrincipal);
			fromWinNTConverterDelegate[12, 2] = "groupType";
			fromWinNTConverterDelegate[12, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.GroupTypeFromWinNTConverter);
			fromWinNTConverterDelegate[12, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.GroupTypeToWinNTConverter);
			fromWinNTConverterDelegate[13, 0] = "UserPrincipal.EmailAddress";
			fromWinNTConverterDelegate[13, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[13, 2] = "*******";
			fromWinNTConverterDelegate[13, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.EmailFromWinNTConverter);
			fromWinNTConverterDelegate[13, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.EmailToWinNTConverter);
			fromWinNTConverterDelegate[14, 0] = "AuthenticablePrincipal.AccountInfo.LastLogon";
			fromWinNTConverterDelegate[14, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[14, 2] = "LastLogin";
			fromWinNTConverterDelegate[14, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.DateFromWinNTConverter);
			fromWinNTConverterDelegate[15, 0] = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";
			fromWinNTConverterDelegate[15, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[15, 2] = "LoginWorkstations";
			fromWinNTConverterDelegate[15, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.MultiStringFromWinNTConverter);
			fromWinNTConverterDelegate[15, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.MultiStringToWinNTConverter);
			fromWinNTConverterDelegate[16, 0] = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";
			fromWinNTConverterDelegate[16, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[16, 2] = "LoginHours";
			fromWinNTConverterDelegate[16, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.BinaryFromWinNTConverter);
			fromWinNTConverterDelegate[16, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.LogonHoursToWinNTConverter);
			fromWinNTConverterDelegate[17, 0] = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";
			fromWinNTConverterDelegate[17, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[17, 2] = "AccountExpirationDate";
			fromWinNTConverterDelegate[17, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.DateFromWinNTConverter);
			fromWinNTConverterDelegate[17, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.AcctExpirDateToNTConverter);
			fromWinNTConverterDelegate[18, 0] = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";
			fromWinNTConverterDelegate[18, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[18, 2] = "UserFlags";
			fromWinNTConverterDelegate[18, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[18, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[19, 0] = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";
			fromWinNTConverterDelegate[19, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[19, 2] = "UserFlags";
			fromWinNTConverterDelegate[19, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[19, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[20, 0] = "AuthenticablePrincipal.AccountInfo.BadLogonCount";
			fromWinNTConverterDelegate[20, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[20, 2] = "BadPasswordAttempts";
			fromWinNTConverterDelegate[20, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.IntFromWinNTConverter);
			fromWinNTConverterDelegate[21, 0] = "AuthenticablePrincipal.AccountInfo.HomeDirectory";
			fromWinNTConverterDelegate[21, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[21, 2] = "HomeDirectory";
			fromWinNTConverterDelegate[21, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.StringFromWinNTConverter);
			fromWinNTConverterDelegate[21, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.StringToWinNTConverter);
			fromWinNTConverterDelegate[22, 0] = "AuthenticablePrincipal.AccountInfo.HomeDrive";
			fromWinNTConverterDelegate[22, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[22, 2] = "HomeDirDrive";
			fromWinNTConverterDelegate[22, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.StringFromWinNTConverter);
			fromWinNTConverterDelegate[22, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.StringToWinNTConverter);
			fromWinNTConverterDelegate[23, 0] = "AuthenticablePrincipal.AccountInfo.ScriptPath";
			fromWinNTConverterDelegate[23, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[23, 2] = "LoginScript";
			fromWinNTConverterDelegate[23, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.StringFromWinNTConverter);
			fromWinNTConverterDelegate[23, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.StringToWinNTConverter);
			fromWinNTConverterDelegate[24, 0] = "AuthenticablePrincipal.PasswordInfo.LastPasswordSet";
			fromWinNTConverterDelegate[24, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[24, 2] = "PasswordAge";
			fromWinNTConverterDelegate[24, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.ElapsedTimeFromWinNTConverter);
			fromWinNTConverterDelegate[25, 0] = "AuthenticablePrincipal.PasswordInfo.LastBadPasswordAttempt";
			fromWinNTConverterDelegate[25, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[25, 2] = "*******";
			fromWinNTConverterDelegate[25, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.LastBadPwdAttemptFromWinNTConverter);
			fromWinNTConverterDelegate[26, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";
			fromWinNTConverterDelegate[26, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[26, 2] = "UserFlags";
			fromWinNTConverterDelegate[26, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[26, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[27, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";
			fromWinNTConverterDelegate[27, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[27, 2] = "UserFlags";
			fromWinNTConverterDelegate[27, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[27, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[28, 0] = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";
			fromWinNTConverterDelegate[28, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[28, 2] = "UserFlags";
			fromWinNTConverterDelegate[28, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[28, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[29, 0] = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";
			fromWinNTConverterDelegate[29, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[29, 2] = "UserFlags";
			fromWinNTConverterDelegate[29, 3] = new SAMStoreCtx.FromWinNTConverterDelegate(SAMStoreCtx.UserFlagsFromWinNTConverter);
			fromWinNTConverterDelegate[29, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.UserFlagsToWinNTConverter);
			fromWinNTConverterDelegate[30, 0] = "UserPrincipal.GivenName";
			fromWinNTConverterDelegate[30, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[30, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[31, 0] = "UserPrincipal.MiddleName";
			fromWinNTConverterDelegate[31, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[31, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[32, 0] = "UserPrincipal.Surname";
			fromWinNTConverterDelegate[32, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[32, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[33, 0] = "UserPrincipal.VoiceTelephoneNumber";
			fromWinNTConverterDelegate[33, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[33, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[34, 0] = "UserPrincipal.EmployeeId";
			fromWinNTConverterDelegate[34, 1] = typeof(UserPrincipal);
			fromWinNTConverterDelegate[34, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[35, 0] = "Principal.DisplayName";
			fromWinNTConverterDelegate[35, 1] = typeof(GroupPrincipal);
			fromWinNTConverterDelegate[35, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[36, 0] = "Principal.DisplayName";
			fromWinNTConverterDelegate[36, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[36, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[37, 0] = "Principal.Description";
			fromWinNTConverterDelegate[37, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[37, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[38, 0] = "AuthenticablePrincipal.Enabled";
			fromWinNTConverterDelegate[38, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[38, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[39, 0] = "AuthenticablePrincipal.Certificates";
			fromWinNTConverterDelegate[39, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[39, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[40, 0] = "ComputerPrincipal.ServicePrincipalNames";
			fromWinNTConverterDelegate[40, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[40, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[41, 0] = "AuthenticablePrincipal.AccountInfo.PermittedWorkstations";
			fromWinNTConverterDelegate[41, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[41, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[42, 0] = "AuthenticablePrincipal.AccountInfo.PermittedLogonTimes";
			fromWinNTConverterDelegate[42, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[42, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[43, 0] = "AuthenticablePrincipal.AccountInfo.AccountExpirationDate";
			fromWinNTConverterDelegate[43, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[43, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[44, 0] = "AuthenticablePrincipal.AccountInfo.SmartcardLogonRequired";
			fromWinNTConverterDelegate[44, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[44, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[45, 0] = "AuthenticablePrincipal.AccountInfo.DelegationPermitted";
			fromWinNTConverterDelegate[45, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[45, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[46, 0] = "AuthenticablePrincipal.AccountInfo.HomeDirectory";
			fromWinNTConverterDelegate[46, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[46, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[47, 0] = "AuthenticablePrincipal.AccountInfo.HomeDrive";
			fromWinNTConverterDelegate[47, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[47, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[48, 0] = "AuthenticablePrincipal.AccountInfo.ScriptPath";
			fromWinNTConverterDelegate[48, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[48, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[49, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNotRequired";
			fromWinNTConverterDelegate[49, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[49, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[50, 0] = "AuthenticablePrincipal.PasswordInfo.PasswordNeverExpires";
			fromWinNTConverterDelegate[50, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[50, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[51, 0] = "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword";
			fromWinNTConverterDelegate[51, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[51, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			fromWinNTConverterDelegate[52, 0] = "AuthenticablePrincipal.PasswordInfo.AllowReversiblePasswordEncryption";
			fromWinNTConverterDelegate[52, 1] = typeof(ComputerPrincipal);
			fromWinNTConverterDelegate[52, 4] = new SAMStoreCtx.ToWinNTConverterDelegate(SAMStoreCtx.ExceptionToWinNTConverter);
			SAMStoreCtx.propertyMappingTableRaw = fromWinNTConverterDelegate;
			SAMStoreCtx.userPropertyMappingTableByProperty = null;
			SAMStoreCtx.userPropertyMappingTableByWinNT = null;
			SAMStoreCtx.groupPropertyMappingTableByProperty = null;
			SAMStoreCtx.groupPropertyMappingTableByWinNT = null;
			SAMStoreCtx.computerPropertyMappingTableByProperty = null;
			SAMStoreCtx.computerPropertyMappingTableByWinNT = null;
			SAMStoreCtx.ValidPropertyMap = null;
			SAMStoreCtx.MaskMap = null;
			SAMStoreCtx.userPropertyMappingTableByProperty = new Hashtable();
			SAMStoreCtx.userPropertyMappingTableByWinNT = new Hashtable();
			SAMStoreCtx.groupPropertyMappingTableByProperty = new Hashtable();
			SAMStoreCtx.groupPropertyMappingTableByWinNT = new Hashtable();
			SAMStoreCtx.computerPropertyMappingTableByProperty = new Hashtable();
			SAMStoreCtx.computerPropertyMappingTableByWinNT = new Hashtable();
			SAMStoreCtx.ValidPropertyMap = new Dictionary<string, SAMStoreCtx.ObjectMask>();
			SAMStoreCtx.MaskMap = new Dictionary<Type, SAMStoreCtx.ObjectMask>();
			SAMStoreCtx.MaskMap.Add(typeof(UserPrincipal), SAMStoreCtx.ObjectMask.User);
			SAMStoreCtx.MaskMap.Add(typeof(ComputerPrincipal), SAMStoreCtx.ObjectMask.Computer);
			SAMStoreCtx.MaskMap.Add(typeof(GroupPrincipal), SAMStoreCtx.ObjectMask.Group);
			SAMStoreCtx.MaskMap.Add(typeof(Principal), SAMStoreCtx.ObjectMask.Principal);
			for (int i = 0; i < SAMStoreCtx.propertyMappingTableRaw.GetLength(0); i++)
			{
				string arrayLists = SAMStoreCtx.propertyMappingTableRaw[i, 0] as string;
				Type type = SAMStoreCtx.propertyMappingTableRaw[i, 1] as Type;
				string str = SAMStoreCtx.propertyMappingTableRaw[i, 2] as string;
				SAMStoreCtx.FromWinNTConverterDelegate fromWinNTConverterDelegate1 = SAMStoreCtx.propertyMappingTableRaw[i, 3] as SAMStoreCtx.FromWinNTConverterDelegate;
				SAMStoreCtx.ToWinNTConverterDelegate toWinNTConverterDelegate = SAMStoreCtx.propertyMappingTableRaw[i, 4] as SAMStoreCtx.ToWinNTConverterDelegate;
				SAMStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry = new SAMStoreCtx.PropertyMappingTableEntry();
				propertyMappingTableEntry.propertyName = arrayLists;
				propertyMappingTableEntry.suggestedWinNTPropertyName = str;
				propertyMappingTableEntry.winNTToPapiConverter = fromWinNTConverterDelegate1;
				propertyMappingTableEntry.papiToWinNTConverter = toWinNTConverterDelegate;
				List<Hashtable> hashtables = new List<Hashtable>();
				List<Hashtable> hashtables1 = new List<Hashtable>();
				if (type != typeof(UserPrincipal))
				{
					if (type != typeof(ComputerPrincipal))
					{
						if (type != typeof(GroupPrincipal))
						{
							hashtables.Add(SAMStoreCtx.userPropertyMappingTableByProperty);
							hashtables.Add(SAMStoreCtx.computerPropertyMappingTableByProperty);
							hashtables.Add(SAMStoreCtx.groupPropertyMappingTableByProperty);
							hashtables1.Add(SAMStoreCtx.userPropertyMappingTableByWinNT);
							hashtables1.Add(SAMStoreCtx.computerPropertyMappingTableByWinNT);
							hashtables1.Add(SAMStoreCtx.groupPropertyMappingTableByWinNT);
							objectMask = SAMStoreCtx.ObjectMask.Principal;
						}
						else
						{
							hashtables.Add(SAMStoreCtx.groupPropertyMappingTableByProperty);
							hashtables1.Add(SAMStoreCtx.groupPropertyMappingTableByWinNT);
							objectMask = SAMStoreCtx.ObjectMask.Group;
						}
					}
					else
					{
						hashtables.Add(SAMStoreCtx.computerPropertyMappingTableByProperty);
						hashtables1.Add(SAMStoreCtx.computerPropertyMappingTableByWinNT);
						objectMask = SAMStoreCtx.ObjectMask.Computer;
					}
				}
				else
				{
					hashtables.Add(SAMStoreCtx.userPropertyMappingTableByProperty);
					hashtables1.Add(SAMStoreCtx.userPropertyMappingTableByWinNT);
					objectMask = SAMStoreCtx.ObjectMask.User;
				}
				if (str == null || str == "*******")
				{
					objectMask = SAMStoreCtx.ObjectMask.None;
				}
				if (!SAMStoreCtx.ValidPropertyMap.TryGetValue(arrayLists, out objectMask1))
				{
					SAMStoreCtx.ValidPropertyMap.Add(arrayLists, objectMask);
				}
				else
				{
					SAMStoreCtx.ValidPropertyMap[arrayLists] = objectMask1 | objectMask;
				}
				foreach (Hashtable hashtable in hashtables)
				{
					if (hashtable[arrayLists] == null)
					{
						hashtable[arrayLists] = new ArrayList();
					}
					((ArrayList)hashtable[arrayLists]).Add(propertyMappingTableEntry);
				}
				if (fromWinNTConverterDelegate1 != null)
				{
					string lower = str.ToLower(CultureInfo.InvariantCulture);
					foreach (Hashtable hashtable1 in hashtables1)
					{
						if (hashtable1[lower] == null)
						{
							hashtable1[lower] = new ArrayList();
						}
						((ArrayList)hashtable1[lower]).Add(propertyMappingTableEntry);
					}
				}
			}
		}

		public SAMStoreCtx(DirectoryEntry ctxBase, bool ownCtxBase, string username, string password, ContextOptions options)
		{
			this.ctxBaseLock = new object();
			this.computerInfoLock = new object();
			this.isLSAM = null;
			this.ctxBase = ctxBase;
			this.ownCtxBase = ownCtxBase;
			if (username != null && password != null)
			{
				this.credentials = new NetCred(username, password);
			}
			this.contextOptions = options;
			this.authTypes = SDSUtils.MapOptionsToAuthTypes(options);
		}

		internal override bool AccessCheck(Principal p, PrincipalAccessMask targetPermission)
		{
			PrincipalAccessMask principalAccessMask = targetPermission;
			if (principalAccessMask == PrincipalAccessMask.ChangePassword)
			{
				PropertyValueCollection item = ((DirectoryEntry)p.GetUnderlyingObject()).Properties["UserFlags"];
				if (item.Count != 0)
				{
					return SDSUtils.StatusFromAccountControl((int)item[0], "AuthenticablePrincipal.PasswordInfo.UserCannotChangePassword");
				}
			}
			return false;
		}

		private static void AcctExpirDateToNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			DateTime? valueForProperty = (DateTime?)p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty.HasValue)
			{
				if (!valueForProperty.HasValue)
				{
					de.Properties[suggestedWinNTProperty].Value = new DateTime(0x7b2, 1, 1);
					return;
				}
				else
				{
					de.Properties[suggestedWinNTProperty].Value = valueForProperty.Value;
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void BinaryFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			SDSUtils.SingleScalarFromDirectoryEntry<byte[]>(new dSPropertyCollection(de.Properties), suggestedWinNTProperty, p, propertyName);
		}

		internal override bool CanGroupBeCleared(GroupPrincipal g, out string explanationForFailure)
		{
			explanationForFailure = null;
			return true;
		}

		internal override bool CanGroupMemberBeRemoved(GroupPrincipal g, Principal member, out string explanationForFailure)
		{
			explanationForFailure = null;
			return true;
		}

		private static void CertFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
		}

		private static void CertToWinNT(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			if (isLSAM)
			{
				return;
			}
			else
			{
				object[] externalForm = new object[1];
				externalForm[0] = PropertyNamesExternal.GetExternalForm(propertyName);
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.PrincipalUnsupportPropertyForPlatform, externalForm));
			}
		}

		internal override void ChangePassword(AuthenticablePrincipal p, string oldPassword, string newPassword)
		{
			if (p as ComputerPrincipal == null)
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				SDSUtils.ChangePassword(underlyingObject, oldPassword, newPassword);
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.SAMStoreCtxNoComputerPasswordSet);
			}
		}

		internal override Principal ConstructFakePrincipalFromSID(byte[] sid)
		{
			Principal principal = Utils.ConstructFakePrincipalFromSID(sid, base.OwningContext, this.MachineUserSuppliedName, this.credentials, this.MachineUserSuppliedName);
			SAMStoreKey sAMStoreKey = new SAMStoreKey(this.MachineFlatName, sid);
			principal.Key = sAMStoreKey;
			return principal;
		}

		private static void DateFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			PropertyValueCollection item = de.Properties[suggestedWinNTProperty];
			if (item.Count != 0)
			{
				DateTime? nullable = (DateTime?)item[0];
				p.LoadValueIntoProperty(propertyName, nullable);
			}
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

		private static void ElapsedTimeFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			PropertyValueCollection item = de.Properties[suggestedWinNTProperty];
			if (item.Count != 0)
			{
				int num = (int)item[0];
				DateTime? nullable = new DateTime?(DateTime.UtcNow - new TimeSpan(0, 0, num));
				p.LoadValueIntoProperty(propertyName, nullable);
			}
		}

		private static void EmailFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
		}

		private static void EmailToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			if (isLSAM)
			{
				return;
			}
			else
			{
				object[] externalForm = new object[1];
				externalForm[0] = PropertyNamesExternal.GetExternalForm(propertyName);
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.PrincipalUnsupportPropertyForPlatform, externalForm));
			}
		}

		private static void ExceptionToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			object[] str = new object[2];
			str[0] = p.GetType().ToString();
			str[1] = PropertyNamesExternal.GetExternalForm(propertyName);
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.PrincipalUnsupportPropertyForType, str));
		}

		internal override void ExpirePassword(AuthenticablePrincipal p)
		{
			if (p as ComputerPrincipal == null)
			{
				this.WriteAttribute(p, "PasswordExpired", 1);
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.SAMStoreCtxNoComputerPasswordExpire);
			}
		}

		internal override ResultSet FindByBadPasswordAttempt(DateTime dt, MatchType matchType, Type principalType)
		{
			throw new NotSupportedException(StringResources.StoreNotSupportMethod);
		}

		private ResultSet FindByDate(FindByDateMatcher.DateProperty property, MatchType matchType, DateTime value, Type principalType)
		{
			DirectoryEntries children = SDSUtils.BuildDirectoryEntry(this.ctxBase.Path, this.credentials, this.authTypes).Children;
			List<string> schemaFilter = this.GetSchemaFilter(principalType);
			SAMQuerySet sAMQuerySet = new SAMQuerySet(schemaFilter, children, this.ctxBase, -1, this, new FindByDateMatcher(property, matchType, value));
			return sAMQuerySet;
		}

		internal override ResultSet FindByExpirationTime(DateTime dt, MatchType matchType, Type principalType)
		{
			return this.FindByDate(FindByDateMatcher.DateProperty.AccountExpirationTime, matchType, dt, principalType);
		}

		internal override ResultSet FindByLockoutTime(DateTime dt, MatchType matchType, Type principalType)
		{
			throw new NotSupportedException(StringResources.StoreNotSupportMethod);
		}

		internal override ResultSet FindByLogonTime(DateTime dt, MatchType matchType, Type principalType)
		{
			return this.FindByDate(FindByDateMatcher.DateProperty.LogonTime, matchType, dt, principalType);
		}

		internal override ResultSet FindByPasswordSetTime(DateTime dt, MatchType matchType, Type principalType)
		{
			return this.FindByDate(FindByDateMatcher.DateProperty.PasswordSetTime, matchType, dt, principalType);
		}

		private object FindNativeByNT4IdentRef(Type principalType, string urnValue)
		{
			DirectoryEntry directoryEntry;
			object obj;
			string str;
			int num = urnValue.IndexOf('\\');
			if (num != urnValue.Length - 1)
			{
				if (num != -1)
				{
					str = urnValue.Substring(num + 1);
				}
				else
				{
					str = urnValue;
				}
				string str1 = str;
				string str2 = "";
				if (principalType == typeof(UserPrincipal) || principalType.IsSubclassOf(typeof(UserPrincipal)))
				{
					str2 = ",user";
					principalType = typeof(UserPrincipal);
				}
				else
				{
					if (principalType == typeof(GroupPrincipal) || principalType.IsSubclassOf(typeof(GroupPrincipal)))
					{
						str2 = ",group";
						principalType = typeof(GroupPrincipal);
					}
					else
					{
						if (principalType == typeof(ComputerPrincipal) || principalType.IsSubclassOf(typeof(ComputerPrincipal)))
						{
							str2 = ",computer";
							principalType = typeof(ComputerPrincipal);
						}
					}
				}
				string[] machineUserSuppliedName = new string[5];
				machineUserSuppliedName[0] = "WinNT://";
				machineUserSuppliedName[1] = this.MachineUserSuppliedName;
				machineUserSuppliedName[2] = "/";
				machineUserSuppliedName[3] = str1;
				machineUserSuppliedName[4] = str2;
				string str3 = string.Concat(machineUserSuppliedName);
				directoryEntry = SDSUtils.BuildDirectoryEntry(this.credentials, this.authTypes);
				try
				{
					directoryEntry.Path = str3;
					if (directoryEntry.Properties["objectSid"] == null || directoryEntry.Properties["objectSid"].Count == 0)
					{
						obj = (bool)0;
					}
					else
					{
						goto Label0;
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					if (cOMException.ErrorCode == -2147022677 || cOMException.ErrorCode == -2147022676 || cOMException.ErrorCode == -2147024843 || cOMException.ErrorCode == -2147022675)
					{
						obj = null;
					}
					else
					{
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
				}
				return obj;
			}
			else
			{
				throw new ArgumentException(StringResources.StoreCtxNT4IdentityClaimWrongForm);
			}
		Label0:
			bool flag = false;
			if (!(principalType == typeof(UserPrincipal)) || !SAMUtils.IsOfObjectClass(directoryEntry, "User"))
			{
				if (!(principalType == typeof(GroupPrincipal)) || !SAMUtils.IsOfObjectClass(directoryEntry, "Group"))
				{
					if (!(principalType == typeof(ComputerPrincipal)) || !SAMUtils.IsOfObjectClass(directoryEntry, "Computer"))
					{
						if (!(principalType == typeof(AuthenticablePrincipal)) || !SAMUtils.IsOfObjectClass(directoryEntry, "User") && !SAMUtils.IsOfObjectClass(directoryEntry, "Computer"))
						{
							if (SAMUtils.IsOfObjectClass(directoryEntry, "User") || SAMUtils.IsOfObjectClass(directoryEntry, "Group") || SAMUtils.IsOfObjectClass(directoryEntry, "Computer"))
							{
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				return null;
			}
			else
			{
				return directoryEntry;
			}
		}

		private object FindNativeBySIDIdentRef(Type principalType, byte[] sid)
		{
			string str = null;
			string machineFlatName = null;
			int num = 0;
			int num1 = Utils.LookupSid(this.MachineUserSuppliedName, this.credentials, sid, out str, out machineFlatName, out num);
			if (num1 == 0)
			{
				if (Utils.ClassifySID(sid) == SidType.RealObjectFakeDomain)
				{
					machineFlatName = this.MachineFlatName;
				}
				if (string.Compare(machineFlatName, this.MachineFlatName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					string str1 = string.Concat(machineFlatName, "\\", str);
					return this.FindNativeByNT4IdentRef(principalType, str1);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		internal override Principal FindPrincipalByIdentRef(Type principalType, string urnScheme, string urnValue, DateTime referenceDate)
		{
			Principal principal;
			if (urnScheme != "ms-sid")
			{
				if (urnScheme == "ms-nt4account" || urnScheme == "ms-name")
				{
					object obj = this.FindNativeByNT4IdentRef(principalType, urnValue);
					if (obj != null)
					{
						return this.GetAsPrincipal(obj, null);
					}
					else
					{
						return null;
					}
				}
				else
				{
					if (urnScheme != null)
					{
						throw new ArgumentException(StringResources.StoreCtxUnsupportedIdentityClaimForQuery);
					}
					else
					{
						object obj1 = null;
						object obj2 = null;
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
							if (principalType == typeof(Principal) || principalType == typeof(GroupPrincipal) || principalType.IsSubclassOf(typeof(GroupPrincipal)))
							{
								IntPtr zero = IntPtr.Zero;
								try
								{
									zero = Utils.ConvertByteArrayToIntPtr(numArray);
									if (UnsafeNativeMethods.IsValidSid(zero) && Utils.ClassifySID(zero) == SidType.FakeObject)
									{
										principal = this.ConstructFakePrincipalFromSID(numArray);
										return principal;
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
							obj1 = this.FindNativeBySIDIdentRef(principalType, numArray);
						}
						try
						{
							obj2 = this.FindNativeByNT4IdentRef(principalType, urnValue);
						}
						catch (ArgumentException argumentException1)
						{
						}
						if (obj1 == null || obj2 == null)
						{
							if (obj1 != null)
							{
								return this.GetAsPrincipal(obj1, null);
							}
							else
							{
								if (obj2 != null)
								{
									return this.GetAsPrincipal(obj2, null);
								}
								else
								{
									return null;
								}
							}
						}
						else
						{
							throw new MultipleMatchesException(StringResources.MultipleMatchingPrincipals);
						}
					}
				}
			}
			else
			{
				SecurityIdentifier securityIdentifier1 = new SecurityIdentifier(urnValue);
				byte[] numArray1 = new byte[securityIdentifier1.BinaryLength];
				securityIdentifier1.GetBinaryForm(numArray1, 0);
				if (numArray1 != null)
				{
					IntPtr intPtr = IntPtr.Zero;
					try
					{
						intPtr = Utils.ConvertByteArrayToIntPtr(numArray1);
						if (UnsafeNativeMethods.IsValidSid(intPtr) && Utils.ClassifySID(intPtr) == SidType.FakeObject)
						{
							principal = this.ConstructFakePrincipalFromSID(numArray1);
							return principal;
						}
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
					object obj3 = this.FindNativeBySIDIdentRef(principalType, numArray1);
					if (obj3 != null)
					{
						return this.GetAsPrincipal(obj3, null);
					}
					else
					{
						return null;
					}
				}
				else
				{
					throw new ArgumentException(StringResources.StoreCtxSecurityIdentityClaimBadFormat);
				}
			}
			return principal;
		}

		internal override Principal GetAsPrincipal(object storeObject, object discriminant)
		{
			DirectoryEntry directoryEntry = (DirectoryEntry)storeObject;
			Principal principal = SDSUtils.DirectoryEntryToPrincipal(directoryEntry, base.OwningContext, null);
			SAMStoreKey sAMStoreKey = new SAMStoreKey(this.MachineFlatName, (byte[])directoryEntry.Properties["objectSid"].Value);
			principal.Key = sAMStoreKey;
			return principal;
		}

		internal override BookmarkableResultSet GetGroupMembership(GroupPrincipal g, bool recursive)
		{
			if (!g.fakePrincipal)
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)g.UnderlyingObject;
				UnsafeNativeMethods.IADsGroup nativeObject = (UnsafeNativeMethods.IADsGroup)underlyingObject.NativeObject;
				BookmarkableResultSet sAMMembersSet = new SAMMembersSet(underlyingObject.Path, nativeObject, recursive, this, this.ctxBase);
				return sAMMembersSet;
			}
			else
			{
				return new EmptySet();
			}
		}

		internal override ResultSet GetGroupsMemberOf(Principal p)
		{
			if (p.fakePrincipal)
			{
				DirectoryEntries children = SDSUtils.BuildDirectoryEntry(this.ctxBase.Path, this.credentials, this.authTypes).Children;
				List<string> schemaFilter = this.GetSchemaFilter(typeof(GroupPrincipal));
				SecurityIdentifier sid = p.Sid;
				byte[] numArray = new byte[sid.BinaryLength];
				sid.GetBinaryForm(numArray, 0);
				if (sid != null)
				{
					SAMQuerySet sAMQuerySet = new SAMQuerySet(schemaFilter, children, this.ctxBase, -1, this, new GroupMemberMatcher(numArray));
					return sAMQuerySet;
				}
				else
				{
					throw new InvalidOperationException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
				}
			}
			else
			{
				if (p as UserPrincipal != null)
				{
					DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
					UnsafeNativeMethods.IADsMembers aDsMember = (UnsafeNativeMethods.IADsMembers)underlyingObject.Invoke("Groups", new object[0]);
					ResultSet sAMGroupsSet = new SAMGroupsSet(aDsMember, this, this.ctxBase);
					return sAMGroupsSet;
				}
				else
				{
					return new EmptySet();
				}
			}
		}

		internal override ResultSet GetGroupsMemberOf(Principal foreignPrincipal, StoreCtx foreignContext)
		{
			if (!foreignPrincipal.fakePrincipal)
			{
				SecurityIdentifier sid = foreignPrincipal.Sid;
				if (sid != null)
				{
					UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(base.OwningContext, IdentityType.Sid, sid.ToString());
					if (userPrincipal != null)
					{
						return this.GetGroupsMemberOf(userPrincipal);
					}
					else
					{
						return new EmptySet();
					}
				}
				else
				{
					throw new InvalidOperationException(StringResources.StoreCtxNeedValueSecurityIdentityClaimToQuery);
				}
			}
			else
			{
				return this.GetGroupsMemberOf(foreignPrincipal);
			}
		}

		internal override ResultSet GetGroupsMemberOfAZ(Principal p)
		{
			ResultSet authZSet;
			SecurityIdentifier sid = p.Sid;
			if (sid != null)
			{
				byte[] numArray = new byte[sid.BinaryLength];
				sid.GetBinaryForm(numArray, 0);
				if (numArray != null)
				{
					try
					{
						authZSet = new AuthZSet(numArray, this.credentials, this.contextOptions, this.MachineFlatName, this, this.ctxBase);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
					return authZSet;
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

		private string GetSamAccountName(Principal p)
		{
			if (p.GetChangeStatusForProperty("Principal.SamAccountName"))
			{
				string samAccountName = p.SamAccountName;
				if (samAccountName != null)
				{
					int num = samAccountName.IndexOf('\\');
					if (num != samAccountName.Length - 1)
					{
						if (num != -1)
						{
							return samAccountName.Substring(num + 1);
						}
						else
						{
							return samAccountName;
						}
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		private List<string> GetSchemaFilter(Type principalType)
		{
			List<string> strs = new List<string>();
			if (principalType == typeof(UserPrincipal) || principalType.IsSubclassOf(typeof(UserPrincipal)))
			{
				strs.Add("User");
			}
			else
			{
				if (principalType == typeof(GroupPrincipal) || principalType.IsSubclassOf(typeof(GroupPrincipal)))
				{
					strs.Add("Group");
				}
				else
				{
					if (principalType == typeof(ComputerPrincipal) || principalType.IsSubclassOf(typeof(ComputerPrincipal)))
					{
						strs.Add("Computer");
					}
					else
					{
						if (principalType != typeof(Principal))
						{
							if (principalType != typeof(AuthenticablePrincipal))
							{
								object[] str = new object[1];
								str[0] = principalType.ToString();
								throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPrincipalTypeForQuery, str));
							}
							else
							{
								strs.Add("User");
								strs.Add("Computer");
							}
						}
						else
						{
							strs.Add("User");
							strs.Add("Group");
							strs.Add("Computer");
						}
					}
				}
			}
			return strs;
		}

		private static string GetSidADsPathFromPrincipal(Principal p)
		{
			SecurityIdentifier sid = p.Sid;
			if (sid != null)
			{
				string str = sid.ToString();
				if (str != null)
				{
					return string.Concat("WinNT://", str);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		private static void GroupTypeFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			p.LoadValueIntoProperty("GroupPrincipal.IsSecurityGroup", (bool)1);
			if (propertyName != "GroupPrincipal.IsSecurityGroup")
			{
				p.LoadValueIntoProperty(propertyName, GroupScope.Local);
				return;
			}
			else
			{
				return;
			}
		}

		private static void GroupTypeToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			if (propertyName != "GroupPrincipal.IsSecurityGroup")
			{
				GroupScope valueForProperty = (GroupScope)p.GetValueForProperty(propertyName);
				if (valueForProperty != GroupScope.Local)
				{
					throw new InvalidOperationException(StringResources.SAMStoreCtxLocalGroupsOnly);
				}
			}
			else
			{
				if (!isLSAM)
				{
					object[] externalForm = new object[1];
					externalForm[0] = PropertyNamesExternal.GetExternalForm(propertyName);
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.PrincipalUnsupportPropertyForPlatform, externalForm));
				}
			}
		}

		internal override void InitializeUserAccountControl(AuthenticablePrincipal p)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			Type type = p.GetType();
			if (type == typeof(UserPrincipal) || type.IsSubclassOf(typeof(UserPrincipal)))
			{
				underlyingObject.Properties["userFlags"].Value = 0x201;
			}
		}

		internal override void Insert(Principal p)
		{
			try
			{
				SDSUtils.InsertPrincipal(p, this, new SDSUtils.GroupMembershipUpdater(SAMStoreCtx.UpdateGroupMembership), this.credentials, this.authTypes, false);
				((DirectoryEntry)p.UnderlyingObject).RefreshCache();
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				SAMStoreKey sAMStoreKey = new SAMStoreKey(this.MachineFlatName, (byte[])underlyingObject.Properties["objectSid"].Value);
				p.Key = sAMStoreKey;
				p.ResetAllChangeStatus();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		private static void IntFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			SDSUtils.SingleScalarFromDirectoryEntry<int>(new dSPropertyCollection(de.Properties), suggestedWinNTProperty, p, propertyName);
		}

		internal override bool IsLockedOut(AuthenticablePrincipal p)
		{
			bool flag;
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			try
			{
				underlyingObject.RefreshCache();
				flag = (bool)underlyingObject.InvokeGet("IsAccountLocked");
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
			return flag;
		}

		internal override bool IsMemberOfInStore(GroupPrincipal g, Principal p)
		{
			return false;
		}

		internal override bool IsValidProperty(Principal p, string propertyName)
		{
			SAMStoreCtx.ObjectMask objectMask = SAMStoreCtx.ObjectMask.None;
			if (!SAMStoreCtx.ValidPropertyMap.TryGetValue(propertyName, out objectMask))
			{
				return false;
			}
			else
			{
				if ((SAMStoreCtx.MaskMap[p.GetType()] & objectMask) > SAMStoreCtx.ObjectMask.None)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private static void LastBadPwdAttemptFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
		}

		internal override void Load(Principal p, string principalPropertyName)
		{
			Hashtable hashtables;
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			Type type = p.GetType();
			if (type != typeof(UserPrincipal))
			{
				if (type != typeof(GroupPrincipal))
				{
					hashtables = SAMStoreCtx.computerPropertyMappingTableByProperty;
				}
				else
				{
					hashtables = SAMStoreCtx.groupPropertyMappingTableByProperty;
				}
			}
			else
			{
				hashtables = SAMStoreCtx.userPropertyMappingTableByProperty;
			}
			ArrayList item = (ArrayList)hashtables[principalPropertyName];
			if (item != null)
			{
				try
				{
					foreach (SAMStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry in item)
					{
						if (propertyMappingTableEntry.winNTToPapiConverter == null)
						{
							continue;
						}
						propertyMappingTableEntry.winNTToPapiConverter(underlyingObject, propertyMappingTableEntry.suggestedWinNTPropertyName, p, propertyMappingTableEntry.propertyName);
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
			Hashtable hashtables;
			ArrayList item = null;
			try
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				ICollection propertyNames = underlyingObject.Properties.PropertyNames;
				Type type = p.GetType();
				if (type != typeof(UserPrincipal))
				{
					if (type != typeof(GroupPrincipal))
					{
						hashtables = SAMStoreCtx.computerPropertyMappingTableByWinNT;
					}
					else
					{
						hashtables = SAMStoreCtx.groupPropertyMappingTableByWinNT;
					}
				}
				else
				{
					hashtables = SAMStoreCtx.userPropertyMappingTableByWinNT;
				}
				foreach (string str in item)
				{
					item = (ArrayList)hashtables[str.ToLower(CultureInfo.InvariantCulture)];
					if (item == null)
					{
						continue;
					}
					IEnumerator enumerator = item.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							SAMStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry = (SAMStoreCtx.PropertyMappingTableEntry)str;
							propertyMappingTableEntry.winNTToPapiConverter(underlyingObject, propertyMappingTableEntry.suggestedWinNTPropertyName, p, propertyMappingTableEntry.propertyName);
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
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		private void LoadComputerInfo()
		{
			int num = 0;
			int num1 = 0;
			if (SAMUtils.GetOSVersion(this.ctxBase, out num, out num1))
			{
				if (num < 6)
				{
					this.isLSAM = new bool?(false);
				}
				else
				{
					this.isLSAM = new bool?(true);
				}
				if (this.ctxBase.Properties["Name"].Count <= 0)
				{
					throw new PrincipalOperationException(StringResources.SAMStoreCtxUnableToRetrieveMachineName);
				}
				else
				{
					this.machineUserSuppliedName = (string)this.ctxBase.Properties["Name"].Value;
					IntPtr zero = IntPtr.Zero;
					try
					{
						int num2 = UnsafeNativeMethods.NetWkstaGetInfo(this.machineUserSuppliedName, 100, ref zero);
						if (num2 != 0)
						{
							object[] objArray = new object[1];
							objArray[0] = num2;
							throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.SAMStoreCtxUnableToRetrieveFlatMachineName, objArray));
						}
						else
						{
							UnsafeNativeMethods.WKSTA_INFO_100 structure = (UnsafeNativeMethods.WKSTA_INFO_100)Marshal.PtrToStructure(zero, typeof(UnsafeNativeMethods.WKSTA_INFO_100));
							this.machineFlatName = structure.wki100_computername;
						}
					}
					finally
					{
						if (zero != IntPtr.Zero)
						{
							UnsafeNativeMethods.NetApiBufferFree(zero);
						}
					}
					return;
				}
			}
			else
			{
				throw new PrincipalOperationException(StringResources.SAMStoreCtxUnableToRetrieveVersion);
			}
		}

		private static void LogonHoursToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			byte[] valueForProperty = (byte[])p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty != null)
			{
				if (valueForProperty == null || (int)valueForProperty.Length == 0)
				{
					byte[] numArray = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
					byte[] numArray1 = numArray;
					de.Properties[suggestedWinNTProperty].Value = numArray1;
					return;
				}
				else
				{
					de.Properties[suggestedWinNTProperty].Value = valueForProperty;
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal override void Move(StoreCtx originalStore, Principal p)
		{
		}

		private static void MultiStringFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			SDSUtils.MultiScalarFromDirectoryEntry<string>(new dSPropertyCollection(de.Properties), suggestedWinNTProperty, p, propertyName);
		}

		private static void MultiStringToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			SDSUtils.MultiStringToDirectoryEntryConverter(p, propertyName, de, suggestedWinNTProperty);
		}

		internal override Type NativeType(Principal p)
		{
			return typeof(DirectoryEntry);
		}

		internal override object PushChangesToNative(Principal p)
		{
			string str;
			ArrayList value = null;
			object obj;
			try
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				Type type = p.GetType();
				if (underlyingObject == null)
				{
					if (type == typeof(UserPrincipal) || type.IsSubclassOf(typeof(UserPrincipal)))
					{
						str = "user";
					}
					else
					{
						if (type == typeof(GroupPrincipal) || type.IsSubclassOf(typeof(GroupPrincipal)))
						{
							str = "group";
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = type.ToString();
							throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPrincipalTypeForSave, objArray));
						}
					}
					string samAccountName = this.GetSamAccountName(p);
					if (samAccountName != null)
					{
						lock (this.ctxBaseLock)
						{
							underlyingObject = this.ctxBase.Children.Add(samAccountName, str);
						}
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
				if (type != typeof(UserPrincipal))
				{
					if (type != typeof(GroupPrincipal))
					{
					}
					else
					{
					}
				}
				else
				{
				}
				foreach (DictionaryEntry dictionaryEntry in value)
				{
					value = (ArrayList)dictionaryEntry.Value;
					IEnumerator enumerator = value.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							SAMStoreCtx.PropertyMappingTableEntry propertyMappingTableEntry = (SAMStoreCtx.PropertyMappingTableEntry)dictionaryEntry;
							if (propertyMappingTableEntry.papiToWinNTConverter == null || !p.GetChangeStatusForProperty(propertyMappingTableEntry.propertyName))
							{
								continue;
							}
							propertyMappingTableEntry.papiToWinNTConverter(p, propertyMappingTableEntry.propertyName, underlyingObject, propertyMappingTableEntry.suggestedWinNTPropertyName, this.IsLSAM);
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
				if (p.GetChangeStatusForProperty("AuthenticablePrincipal.PasswordInfo.Password"))
				{
					string valueForProperty = (string)p.GetValueForProperty("AuthenticablePrincipal.PasswordInfo.Password");
					SDSUtils.SetPassword(underlyingObject, valueForProperty);
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
			QbeFilterDescription qbeFilterDescription;
			Principal queryFilter = ps.QueryFilter;
			if (queryFilter == null)
			{
				qbeFilterDescription = new QbeFilterDescription();
			}
			else
			{
				qbeFilterDescription = base.BuildQbeFilterDescription(queryFilter);
			}
			return qbeFilterDescription;
		}

		internal override ResultSet Query(PrincipalSearcher ps, int sizeLimit)
		{
			QbeFilterDescription nativeSearcher = (QbeFilterDescription)this.PushFilterToNativeSearcher(ps);
			DirectoryEntries children = SDSUtils.BuildDirectoryEntry(this.ctxBase.Path, this.credentials, this.authTypes).Children;
			Type type = typeof(Principal);
			if (ps.QueryFilter != null)
			{
				type = ps.QueryFilter.GetType();
			}
			List<string> schemaFilter = this.GetSchemaFilter(type);
			SAMQuerySet sAMQuerySet = new SAMQuerySet(schemaFilter, children, this.ctxBase, sizeLimit, this, new QbeMatcher(nativeSearcher));
			return sAMQuerySet;
		}

		internal override Principal ResolveCrossStoreRefToPrincipal(object o)
		{
			string str = null;
			string str1 = null;
			DirectoryEntry directoryEntry = (DirectoryEntry)o;
			if (directoryEntry.Properties["objectSid"].Count != 0)
			{
				byte[] value = (byte[])directoryEntry.Properties["objectSid"].Value;
				int num = 0;
				int num1 = Utils.LookupSid(this.MachineUserSuppliedName, this.credentials, value, out str, out str1, out num);
				if (num1 == 0)
				{
					PrincipalContext context = SDSCache.Domain.GetContext(str1, this.credentials, DefaultContextOptions.ADDefaultContextOption);
					SecurityIdentifier securityIdentifier = new SecurityIdentifier(value, 0);
					Principal principal = context.QueryCtx.FindPrincipalByIdentRef(typeof(Principal), "ms-sid", securityIdentifier.ToString(), DateTime.UtcNow);
					if (principal == null)
					{
						throw new PrincipalOperationException(StringResources.SAMStoreCtxFailedFindCrossStoreTarget);
					}
					else
					{
						return principal;
					}
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = num1;
					throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.SAMStoreCtxCantResolveSidForCrossStore, objArray));
				}
			}
			else
			{
				throw new PrincipalOperationException(StringResources.SAMStoreCtxCantRetrieveObjectSidForCrossStore);
			}
		}

		private static void SamAccountNameFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			string item = (string)de.Properties["Name"][0];
			p.LoadValueIntoProperty(propertyName, item);
		}

		internal override Type SearcherNativeType()
		{
			throw new InvalidOperationException(StringResources.PrincipalSearcherNoUnderlying);
		}

		internal override void SetPassword(AuthenticablePrincipal p, string newPassword)
		{
			if (p as ComputerPrincipal == null)
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				SDSUtils.SetPassword(underlyingObject, newPassword);
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.SAMStoreCtxNoComputerPasswordSet);
			}
		}

		private static void SidFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			byte[] item = (byte[])de.Properties["objectSid"][0];
			Utils.ByteArrayToString(item);
			string sDDL = Utils.ConvertSidToSDDL(item);
			SecurityIdentifier securityIdentifier = new SecurityIdentifier(sDDL);
			p.LoadValueIntoProperty(propertyName, securityIdentifier);
		}

		private static void StringFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			PropertyValueCollection item = de.Properties[suggestedWinNTProperty];
			if (item.Count <= 0 || ((string)item[0]).Length != 0)
			{
				SDSUtils.SingleScalarFromDirectoryEntry<string>(new dSPropertyCollection(de.Properties), suggestedWinNTProperty, p, propertyName);
				return;
			}
			else
			{
				return;
			}
		}

		private static void StringToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			string valueForProperty = (string)p.GetValueForProperty(propertyName);
			if (!p.unpersisted || valueForProperty != null)
			{
				if (valueForProperty == null || valueForProperty.Length <= 0)
				{
					de.Properties[suggestedWinNTProperty].Value = "";
					return;
				}
				else
				{
					de.Properties[suggestedWinNTProperty].Value = valueForProperty;
					return;
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
				CredentialTypes credentialType = CredentialTypes.Password;
				if (this.IsLSAM)
				{
					credentialType = credentialType | CredentialTypes.Certificate;
				}
				return credentialType;
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

		internal override void UnexpirePassword(AuthenticablePrincipal p)
		{
			if (p as ComputerPrincipal == null)
			{
				this.WriteAttribute(p, "PasswordExpired", 0);
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.SAMStoreCtxNoComputerPasswordExpire);
			}
		}

		internal override void UnlockAccount(AuthenticablePrincipal p)
		{
			if (p as ComputerPrincipal == null)
			{
				DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
				DirectoryEntry directoryEntry = null;
				using (directoryEntry)
				{
					try
					{
						directoryEntry = SDSUtils.BuildDirectoryEntry(underlyingObject.Path, this.credentials, this.authTypes);
						object[] objArray = new object[1];
						objArray[0] = (bool)0;
						directoryEntry.InvokeSet("IsAccountLocked", objArray);
						directoryEntry.CommitChanges();
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal override void Update(Principal p)
		{
			try
			{
				SDSUtils.ApplyChangesToDirectory(p, this, new SDSUtils.GroupMembershipUpdater(SAMStoreCtx.UpdateGroupMembership), this.credentials, this.authTypes);
				p.ResetAllChangeStatus();
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		private static void UpdateGroupMembership(Principal group, DirectoryEntry de, NetCred credentials, AuthenticationTypes authTypes)
		{
			int num;
			PrincipalCollection valueForProperty = (PrincipalCollection)group.GetValueForProperty("GroupPrincipal.Members");
			UnsafeNativeMethods.IADsGroup nativeObject = (UnsafeNativeMethods.IADsGroup)de.NativeObject;
			try
			{
				if (valueForProperty.Cleared)
				{
					UnsafeNativeMethods.IADsMembers aDsMember = nativeObject.Members();
					IEnumVARIANT enumVARIANT = (IEnumVARIANT)aDsMember._NewEnum;
					object[] objArray = new object[1];
					do
					{
						num = enumVARIANT.Next(1, objArray, IntPtr.Zero);
						if (num != 0)
						{
							continue;
						}
						UnsafeNativeMethods.IADs aD = (UnsafeNativeMethods.IADs)objArray[0];
						nativeObject.Remove(aD.ADsPath);
					}
					while (num == 0);
					if (num != 1)
					{
						object[] str = new object[1];
						str[0] = num.ToString(CultureInfo.InvariantCulture);
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.SAMStoreCtxFailedToClearGroup, str));
					}
				}
				List<Principal> inserted = valueForProperty.Inserted;
				foreach (Principal principal in inserted)
				{
					Type type = principal.GetType();
					if (!(type != typeof(UserPrincipal)) || type.IsSubclassOf(typeof(UserPrincipal)) || !(type != typeof(ComputerPrincipal)) || type.IsSubclassOf(typeof(ComputerPrincipal)) || !(type != typeof(GroupPrincipal)) || type.IsSubclassOf(typeof(GroupPrincipal)))
					{
						if (!principal.unpersisted)
						{
							continue;
						}
						throw new InvalidOperationException(StringResources.StoreCtxGroupHasUnpersistedInsertedPrincipal);
					}
					else
					{
						object[] str1 = new object[1];
						str1[0] = type.ToString();
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.StoreCtxUnsupportedPrincipalTypeForGroupInsert, str1));
					}
				}
				foreach (Principal principal1 in inserted)
				{
					string sidADsPathFromPrincipal = SAMStoreCtx.GetSidADsPathFromPrincipal(principal1);
					if (sidADsPathFromPrincipal != null)
					{
						nativeObject.Add(sidADsPathFromPrincipal);
					}
					else
					{
						throw new InvalidOperationException(StringResources.SAMStoreCtxCouldntGetSIDForGroupMember);
					}
				}
				List<Principal> removed = valueForProperty.Removed;
				foreach (Principal principal2 in removed)
				{
					string sidADsPathFromPrincipal1 = SAMStoreCtx.GetSidADsPathFromPrincipal(principal2);
					if (sidADsPathFromPrincipal1 != null)
					{
						nativeObject.Remove(sidADsPathFromPrincipal1);
					}
					else
					{
						throw new InvalidOperationException(StringResources.SAMStoreCtxCouldntGetSIDForGroupMember);
					}
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(cOMException);
			}
		}

		private static void UserFlagsFromWinNTConverter(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName)
		{
			SDSUtils.AccountControlFromDirectoryEntry(new dSPropertyCollection(de.Properties), suggestedWinNTProperty, p, propertyName, true);
		}

		private static void UserFlagsToWinNTConverter(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM)
		{
			SDSUtils.AccountControlToDirectoryEntry(p, propertyName, de, suggestedWinNTProperty, true, p.unpersisted);
		}

		private void WriteAttribute(AuthenticablePrincipal p, string attribute, int value)
		{
			DirectoryEntry underlyingObject = (DirectoryEntry)p.UnderlyingObject;
			SDSUtils.WriteAttribute(underlyingObject.Path, attribute, value, this.credentials, this.authTypes);
		}

		private delegate void FromWinNTConverterDelegate(DirectoryEntry de, string suggestedWinNTProperty, Principal p, string propertyName);

		[Flags]
		private enum ObjectMask
		{
			None = 0,
			User = 1,
			Computer = 2,
			Group = 4,
			Principal = 7
		}

		private class PropertyMappingTableEntry
		{
			internal string propertyName;

			internal string suggestedWinNTPropertyName;

			internal SAMStoreCtx.FromWinNTConverterDelegate winNTToPapiConverter;

			internal SAMStoreCtx.ToWinNTConverterDelegate papiToWinNTConverter;

			public PropertyMappingTableEntry()
			{
			}
		}

		private delegate void ToWinNTConverterDelegate(Principal p, string propertyName, DirectoryEntry de, string suggestedWinNTProperty, bool isLSAM);
	}
}