using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADAccountFactory<T> : ADPrincipalFactory<T>
	where T : ADAccount, new()
	{
		internal static int DefaultUserAccessControl;

		protected static bool UseComputerPasswordGeneration;

		private bool _delayEnableUntilSetPassword;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADPrincipalFactory<T>.AttributeTable;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return "CN";
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "user";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "user");
				return aDOPathNode;
			}
		}

		static ADAccountFactory()
		{
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[27];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.UserPrincipalName.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.UserPrincipalName.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AccountExpirationDate.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AccountExpirationDate.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AccountExpiresUtils.ToExtendedAccountExpirationDate), new ToDirectoryFormatDelegate(AccountExpiresUtils.ToDirectoryAccountExpirationDate), new ToSearchFilterDelegate(AccountExpiresUtils.ToSearchAccountExpirationDate));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AccountLockoutTime.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AccountLockoutTime.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryDateTime), new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AllowReversiblePasswordEncryption.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AllowReversiblePasswordEncryption.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.BadLogonCount.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.BadLogonCount.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedCannotChangePassword), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.Certificates.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.Certificates.ADAttribute, TypeConstants.X509Certificate, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueCertificate), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueCertificate), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueCertificate));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AccountNotDelegated.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AccountNotDelegated.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.Enabled.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.Enabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.LastBadPasswordAttempt.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.LastBadPasswordAttempt.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), null, new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.LastLogonDate.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.LastLogonDate.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), null, new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.LockedOut.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.LockedOut.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryLockedOut), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchLockedOut));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordLastSet.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordLastSet.ADAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryDateTime), new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordNeverExpires.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordNeverExpires.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[14] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordNotRequired.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordNotRequired.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[15] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PrimaryGroup.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PrimaryGroup.ADAttribute, TypeConstants.ADGroup, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedPrimaryGroup), null, new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchPrimaryGroup));
			attributeConverterEntry[16] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.ServicePrincipalNames.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.ServicePrincipalNames.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[17] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.TrustedForDelegation.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.TrustedForDelegation.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[18] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.TrustedToAuthForDelegation.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.TrustedToAuthForDelegation.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[19] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.MNSLogonAccount.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.MNSLogonAccount.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[20] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.DoesNotRequirePreAuth.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.DoesNotRequirePreAuth.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[21] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordExpired.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordExpired.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[22] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.HomedirRequired.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.HomedirRequired.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[23] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.UseDESKeyOnly.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.UseDESKeyOnly.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedUserAccountControl), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryUserAccountControl), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchUserAccountControl));
			attributeConverterEntry[24] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.KerberosEncryptionType.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.KerberosEncryptionType.ADAttribute, TypeConstants.ADKerberosEncryptionType, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedFlagEnumerationFromInt<ADKerberosEncryptionType>), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryIntFromFlagEnumeration), new ToSearchFilterDelegate(SearchConverters.ToSearchFlagEnumerationInInt<ADKerberosEncryptionType>));
			attributeConverterEntry[25] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.CompoundIdentitySupported.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.CompoundIdentitySupported.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x20000, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x20000, false).Invoke), new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(0x20000, false).Invoke));
			attributeConverterEntry[26] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PrincipalsAllowedToDelegateToAccount.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PrincipalsAllowedToDelegateToAccount.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedPrincipalFromSecDesc), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectorySecDescFromPrincipal), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			ADAccountFactory<T>.ADMappingTable = attributeConverterEntry;
			AttributeConverterEntry[] attributeConverterEntryArray = new AttributeConverterEntry[18];
			attributeConverterEntryArray[0] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.UserPrincipalName.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.UserPrincipalName.ADAMAttribute, TypeConstants.String, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[1] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AccountExpirationDate.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AccountExpirationDate.ADAMAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AccountExpiresUtils.ToExtendedAccountExpirationDate), new ToDirectoryFormatDelegate(AccountExpiresUtils.ToDirectoryAccountExpirationDate), new ToSearchFilterDelegate(AccountExpiresUtils.ToSearchAccountExpirationDate));
			attributeConverterEntryArray[2] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AccountLockoutTime.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AccountLockoutTime.ADAMAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryDateTime), new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntryArray[3] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.AllowReversiblePasswordEncryption.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.AllowReversiblePasswordEncryption.ADAMAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[4] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.BadLogonCount.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.BadLogonCount.ADAMAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[5] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword.ADAMAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADAccountFactory<T>.ToExtendedCannotChangePassword), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntryArray[6] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.Certificates.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.Certificates.ADAMAttribute, TypeConstants.X509Certificate, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueCertificate), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueCertificate), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueCertificate));
			attributeConverterEntryArray[7] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.Enabled.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.Enabled.ADAMAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedInvertBool), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryInvertBool), new ToSearchFilterDelegate(SearchConverters.ToSearchInvertBool));
			attributeConverterEntryArray[8] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.LastBadPasswordAttempt.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.LastBadPasswordAttempt.ADAMAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), null, new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntryArray[9] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.LastLogonDate.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.LastLogonDate.ADAMAttribute, TypeConstants.DateTime, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), null, new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntryArray[10] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.LockedOut.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.LockedOut.ADAMAttribute, TypeConstants.Bool, false, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedFromFirstAttributeOnly), new ToDirectoryFormatDelegate(ADAccountFactory<T>.ToDirectoryLockedOut), new ToSearchFilterDelegate(ADAccountFactory<T>.ToSearchLockedOut));
			attributeConverterEntryArray[11] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordExpired.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordExpired.ADAMAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntryArray[12] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordLastSet.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordLastSet.ADAMAttribute, TypeConstants.DateTime, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedDateTimeFromLong), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryDateTime), new ToSearchFilterDelegate(SearchConverters.ToSearchDateTimeUsingSchemaInfo));
			attributeConverterEntryArray[13] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordNeverExpires.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordNeverExpires.ADAMAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[14] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PasswordNotRequired.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PasswordNotRequired.ADAMAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntryArray[15] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.KerberosEncryptionType.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.KerberosEncryptionType.ADAttribute, TypeConstants.ADKerberosEncryptionType, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedFlagEnumerationFromInt<ADKerberosEncryptionType>), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryIntFromFlagEnumeration), new ToSearchFilterDelegate(SearchConverters.ToSearchFlagEnumerationInInt<ADKerberosEncryptionType>));
			attributeConverterEntryArray[16] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.CompoundIdentitySupported.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.CompoundIdentitySupported.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x20000, false).Invoke), new ToDirectoryFormatDelegate(AttributeConverters.GetDelegateToDirectoryIntFromFlag(0x20000, false).Invoke), new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(0x20000, false).Invoke));
			attributeConverterEntryArray[17] = new AttributeConverterEntry(ADAccountFactory<T>.ADAccountPropertyMap.PrincipalsAllowedToDelegateToAccount.PropertyName, ADAccountFactory<T>.ADAccountPropertyMap.PrincipalsAllowedToDelegateToAccount.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedPrincipalFromSecDesc), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectorySecDescFromPrincipal), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			ADAccountFactory<T>.ADAMMappingTable = attributeConverterEntryArray;
			ADFactoryBase<T>.RegisterMappingTable(ADAccountFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
			ADFactoryBase<T>.RegisterMappingTable(ADAccountFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADAccountFactory<T>.DefaultUserAccessControl = 0;
			ADAccountFactory<T>.UseComputerPasswordGeneration = false;
		}

		public ADAccountFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADAccountPreCommitChangePasswordAtLogonFSRoutine));
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADAccountPreCommitNewPasswordSetFSRoutine));
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADAccountPreCommitUpdateCannotChangePasswordFSRoutine));
			base.PostCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADAccountPostCommitFSRoutine));
		}

		private bool ADAccountPostCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			int defaultUserAccessControl;
			if (operation != ADFactory<T>.DirectoryOperation.Create)
			{
				return false;
			}
			else
			{
				bool flag = this.CalculateUpdateCannotChangePassword(operation, instance, parameters, directoryObj);
				directoryObj.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
				SecureString singleValueProperty = base.GetSingleValueProperty<SecureString>("AccountPassword", instance, parameters, operation);
				if (singleValueProperty == null)
				{
					if (ADAccountFactory<T>.UseComputerPasswordGeneration)
					{
						RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
						byte[] numArray = new byte[240];
						singleValueProperty = new SecureString();
						rNGCryptoServiceProvider.GetBytes(numArray);
						byte[] numArray1 = numArray;
						for (int i = 0; i < (int)numArray1.Length; i++)
						{
							byte num = numArray1[i];
							if (num == 0)
							{
								singleValueProperty.AppendChar('\u0001');
							}
							else
							{
								singleValueProperty.AppendChar(Convert.ToChar (num));
							}
						}
						ADPasswordUtil.PerformSetPassword(base.CmdletSessionInfo.DefaultPartitionPath, directoryObj, singleValueProperty);
					}
				}
				else
				{
					ADPasswordUtil.PerformSetPassword(base.CmdletSessionInfo.DefaultPartitionPath, directoryObj, singleValueProperty);
				}
				if (this._delayEnableUntilSetPassword)
				{
					flag = true;
					if (base.ConnectedStore != ADServerType.ADDS || !flag)
					{
						if (base.ConnectedStore == ADServerType.ADLDS && flag)
						{
							directoryObj.SetValue("msDS-UserAccountDisabled", false);
						}
					}
					else
					{
						if (!directoryObj.Contains("userAccountControl"))
						{
							defaultUserAccessControl = ADAccountFactory<T>.DefaultUserAccessControl;
						}
						else
						{
							int value = 0;
							int value1 = 0;
							if (directoryObj.Contains("userAccountControl"))
							{
								value = (int)directoryObj["userAccountControl"].Value;
							}
							if (directoryObj.Contains("msDS-User-Account-Control-Computed"))
							{
								value1 = (int)directoryObj["msDS-User-Account-Control-Computed"].Value;
							}
							defaultUserAccessControl = value | value1;
						}
						int bit = UserAccountControlUtil.StringToBit("Enabled");
						defaultUserAccessControl = defaultUserAccessControl & ~bit;
						if (!directoryObj.Contains("userAccountControl"))
						{
							directoryObj.Add("userAccountControl", defaultUserAccessControl);
						}
						else
						{
							directoryObj["userAccountControl"].Value = defaultUserAccessControl;
						}
					}
				}
				return flag;
			}
		}

		private bool ADAccountPreCommitChangePasswordAtLogonFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			object[] objArray;
			bool hasValue;
			bool flag;
			bool hasValue1;
			if (operation == ADFactory<T>.DirectoryOperation.Create || operation == ADFactory<T>.DirectoryOperation.Update)
			{
				bool? item = (bool?)(parameters["ChangePasswordAtLogon"] as bool?);
				if (item.HasValue)
				{
					bool? nullable = item;
					if (!nullable.GetValueOrDefault())
					{
						hasValue = false;
					}
					else
					{
						hasValue = nullable.HasValue;
					}
					if (!hasValue)
					{
						directoryObj["pwdLastSet"].Value = -1;
					}
					else
					{
						bool flag1 = this.IsAccountPasswordNeverExpires(operation, directoryObj);
						bool? item1 = (bool?)(parameters["PasswordNeverExpires"] as bool?);
						bool? nullable1 = item1;
						if (!nullable1.GetValueOrDefault())
						{
							flag = false;
						}
						else
						{
							flag = nullable1.HasValue;
						}
						if (!flag)
						{
							if (flag1)
							{
								bool? nullable2 = item1;
								if (nullable2.GetValueOrDefault())
								{
									hasValue1 = true;
								}
								else
								{
									hasValue1 = !nullable2.HasValue;
								}
								if (hasValue1)
								{
									objArray = new object[1];
									objArray[0] = "PasswordNeverExpires";
									throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.AcctChangePwdNotWorksWhenPwdNotExpires, objArray));
								}
							}
							directoryObj["pwdLastSet"].Value = 0;
							return true;
						}
						objArray = new object[1];
						objArray[0] = "PasswordNeverExpires";
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.AcctChangePwdNotWorksWhenPwdNotExpires, objArray));
					}
				}
				else
				{
					return true;
				}
			}
			return true;
		}

		private bool ADAccountPreCommitNewPasswordSetFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			bool hasValue;
			if (operation == ADFactory<T>.DirectoryOperation.Create)
			{
				bool flag = false;
				bool flag1 = false;
				if (base.ConnectedStore == ADServerType.ADDS && !directoryObj.Contains("userAccountControl"))
				{
					directoryObj["userAccountControl"].Value = ADAccountFactory<T>.DefaultUserAccessControl;
					if ((ADAccountFactory<T>.DefaultUserAccessControl & 32) == 32)
					{
						flag = true;
					}
					if ((ADAccountFactory<T>.DefaultUserAccessControl & 2) == 0)
					{
						flag1 = true;
					}
				}
				base.GetSingleValueProperty<SecureString>("AccountPassword", instance, parameters, operation);
				bool? singleValueProperty = base.GetSingleValueProperty<bool?>(ADAccountFactory<T>.ADAccountPropertyMap.Enabled.PropertyName, instance, parameters, operation);
				if (ADAccountFactory<T>.UseComputerPasswordGeneration)
				{
					if (singleValueProperty.HasValue)
					{
						bool? nullable = singleValueProperty;
						if (!nullable.GetValueOrDefault())
						{
							hasValue = false;
						}
						else
						{
							hasValue = nullable.HasValue;
						}
						if (!hasValue)
						{
							goto Label0;
						}
					}
					this.DelayEnableUntilSetPassword(instance, parameters, directoryObj);
					return true;
				}
			Label0:
				if (base.PropertyHasChange("AccountPassword", instance, parameters, operation) && (!singleValueProperty.HasValue && flag1 || singleValueProperty.HasValue && singleValueProperty.Value))
				{
					bool? singleValueProperty1 = base.GetSingleValueProperty<bool?>(ADAccountFactory<T>.ADAccountPropertyMap.PasswordNotRequired.PropertyName, instance, parameters, operation);
					if (!singleValueProperty1.HasValue && !flag || singleValueProperty1.HasValue && !singleValueProperty1.Value)
					{
						this.DelayEnableUntilSetPassword(instance, parameters, directoryObj);
						return true;
					}
				}
			}
			return false;
		}

		private bool ADAccountPreCommitUpdateCannotChangePasswordFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (operation != ADFactory<T>.DirectoryOperation.Update)
			{
				return false;
			}
			else
			{
				return this.CalculateUpdateCannotChangePassword(operation, instance, parameters, directoryObj);
			}
		}

		private bool CalculateUpdateCannotChangePassword(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (base.PropertyHasChange(ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword.PropertyName, instance, parameters, operation))
			{
				bool? singleValueProperty = base.GetSingleValueProperty<bool?>(ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword.PropertyName, instance, parameters, operation);
				if (singleValueProperty.HasValue)
				{
					return this.UpdateCannotChangePassword(directoryObj, singleValueProperty.Value);
				}
			}
			return false;
		}

		internal override AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			AttributeSetRequest attributeSetRequest = base.ConstructAttributeSetRequest(requestedExtendedAttr);
			if (base.CmdletSessionInfo.ADRootDSE.ServerType != ADServerType.ADLDS)
			{
				if (base.CmdletSessionInfo.ADRootDSE.ServerType == ADServerType.ADDS)
				{
					attributeSetRequest.DirectoryAttributes.Add("userAccountControl");
				}
			}
			else
			{
				attributeSetRequest.DirectoryAttributes.Add("msDS-User-Account-Control-Computed");
			}
			ADSchema aDSchema = new ADSchema(base.CmdletSessionInfo.ADSessionInfo);
			if (aDSchema.SchemaProperties.ContainsKey("msDS-SupportedEncryptionTypes"))
			{
				attributeSetRequest.DirectoryAttributes.Add("msDS-SupportedEncryptionTypes");
			}
			else
			{
				attributeSetRequest.DirectoryAttributes.Remove("msDS-SupportedEncryptionTypes");
			}
			if (!aDSchema.SchemaProperties.ContainsKey("msDS-AllowedToActOnBehalfOfOtherIdentity"))
			{
				attributeSetRequest.DirectoryAttributes.Remove("msDS-AllowedToActOnBehalfOfOtherIdentity");
			}
			return attributeSetRequest;
		}

		private void DelayEnableUntilSetPassword(T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			base.ClearProperty(ADAccountFactory<T>.ADAccountPropertyMap.Enabled.PropertyName, instance, parameters);
			parameters["Enabled"] = false;
			if (base.ConnectedStore != ADServerType.ADDS)
			{
				if (base.ConnectedStore == ADServerType.ADLDS)
				{
					directoryObj.SetValue("msDS-UserAccountDisabled", true);
				}
			}
			else
			{
				directoryObj["userAccountControl"].Value = (int)directoryObj["userAccountControl"].Value | 2;
			}
			this._delayEnableUntilSetPassword = true;
		}

		private bool IsAccountPasswordNeverExpires(ADFactory<T>.DirectoryOperation operation, ADObject acctDirObj)
		{
			if (operation != ADFactory<T>.DirectoryOperation.Create)
			{
				int value = 0;
				int num = 0;
				if (acctDirObj.Contains("userAccountControl"))
				{
					value = (int)acctDirObj["userAccountControl"].Value;
				}
				if (acctDirObj.Contains("msDS-User-Account-Control-Computed"))
				{
					num = (int)acctDirObj["msDS-User-Account-Control-Computed"].Value;
				}
				int num1 = value | num;
				if ((num1 & 0x10000) != 0x10000)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		private static void ScanACLForChangePasswordRight(ActiveDirectorySecurity adsSecurity, out bool denySelfFound, out bool denyWorldFound, out bool allowSelfFound, out bool allowWorldFound)
		{
			denySelfFound = false;
			denyWorldFound = false;
			allowSelfFound = false;
			allowWorldFound = false;
			foreach (ActiveDirectoryAccessRule accessRule in adsSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
			{
				SecurityIdentifier identityReference = (SecurityIdentifier)accessRule.IdentityReference;
				string value = identityReference.Value;
				if (accessRule.ObjectType != ACLConstants.ChangePasswordGuid)
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

		private static bool SetCannotChangePassword(ActiveDirectorySecurity adsSecurity, bool userCannotChangePassword)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			ActiveDirectoryAccessRule extendedRightAccessRule = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-5-10"), AccessControlType.Deny, ACLConstants.ChangePasswordGuid);
			ActiveDirectoryAccessRule activeDirectoryAccessRule = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-1-0"), AccessControlType.Deny, ACLConstants.ChangePasswordGuid);
			ActiveDirectoryAccessRule extendedRightAccessRule1 = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-5-10"), AccessControlType.Allow, ACLConstants.ChangePasswordGuid);
			ActiveDirectoryAccessRule activeDirectoryAccessRule1 = new ExtendedRightAccessRule(new SecurityIdentifier("S-1-1-0"), AccessControlType.Allow, ACLConstants.ChangePasswordGuid);
			foreach (ActiveDirectoryAccessRule accessRule in adsSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
			{
				SecurityIdentifier identityReference = (SecurityIdentifier)accessRule.IdentityReference;
				string value = identityReference.Value;
				if (accessRule.ObjectType != ACLConstants.ChangePasswordGuid)
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
						if (userCannotChangePassword)
						{
							adsSecurity.RemoveAccessRuleSpecific(accessRule);
							flag4 = true;
						}
						flag3 = true;
					}
					else
					{
						if (userCannotChangePassword)
						{
							adsSecurity.RemoveAccessRuleSpecific(accessRule);
							flag4 = true;
						}
						flag2 = true;
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
						if (!userCannotChangePassword)
						{
							adsSecurity.RemoveAccessRuleSpecific(accessRule);
							flag4 = true;
						}
						flag1 = true;
					}
					else
					{
						if (!userCannotChangePassword)
						{
							adsSecurity.RemoveAccessRuleSpecific(accessRule);
							flag4 = true;
						}
						flag = true;
					}
				}
			}
			if (!userCannotChangePassword)
			{
				if (!flag2)
				{
					adsSecurity.AddAccessRule(extendedRightAccessRule1);
					flag4 = true;
				}
				if (!flag3)
				{
					adsSecurity.AddAccessRule(activeDirectoryAccessRule1);
					flag4 = true;
				}
			}
			else
			{
				if (!flag)
				{
					adsSecurity.AddAccessRule(extendedRightAccessRule);
					flag4 = true;
				}
				if (!flag1)
				{
					adsSecurity.AddAccessRule(activeDirectoryAccessRule);
					flag4 = true;
				}
			}
			return flag4;
		}

		internal static void ToDirectoryLockedOut(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool item = (bool)extendedData[0];
			if (!item)
			{
				directoryObj.SetValue("lockoutTime", 0);
			}
		}

		internal static void ToDirectoryUserAccountControl(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			int defaultUserAccessControl;
			if (!directoryObj.Contains("userAccountControl"))
			{
				defaultUserAccessControl = ADAccountFactory<T>.DefaultUserAccessControl;
			}
			else
			{
				int value = 0;
				int num = (int)directoryObj["userAccountControl"].Value;
				if (directoryObj.Contains("msDS-User-Account-Control-Computed"))
				{
					value = (int)directoryObj["msDS-User-Account-Control-Computed"].Value;
				}
				defaultUserAccessControl = num | value;
			}
			bool item = (bool)extendedData[0];
			int bit = UserAccountControlUtil.StringToBit(extendedAttribute);
			if (UserAccountControlUtil.IsInverseBit(bit))
			{
				item = !item;
			}
			if (!item)
			{
				defaultUserAccessControl = defaultUserAccessControl & ~bit;
			}
			else
			{
				defaultUserAccessControl = defaultUserAccessControl | bit;
			}
			directoryObj.SetValue("userAccountControl", defaultUserAccessControl);
		}

		internal static void ToExtendedCannotChangePassword(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4;
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				ActiveDirectorySecurity value = directoryObj[directoryAttributes[0]].Value as ActiveDirectorySecurity;
				if (value != null)
				{
					ADAccountFactory<T>.ScanACLForChangePasswordRight(value, out flag, out flag1, out flag2, out flag3);
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
					userObj.Add(extendedAttribute, flag4);
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal static void ToExtendedPrimaryGroup(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObject aDObject;
			string str = directoryObj["primaryGroupID"].Value.ToString();
			string str1 = directoryObj["objectSid"].Value.ToString();
			using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(cmdletSessionInfo.ADSessionInfo))
			{
				aDObjectSearcher.SearchRoot = cmdletSessionInfo.DefaultPartitionPath;
				aDObjectSearcher.Scope = ADSearchScope.Subtree;
				aDObjectSearcher.Properties.Add("distinguishedName");
				aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", string.Concat(str1.Substring(0, str1.LastIndexOf("-") + 1), str));
				aDObject = aDObjectSearcher.FindOne();
			}
			userObj.Add(extendedAttribute, aDObject.DistinguishedName);
		}

		internal static void ToExtendedUserAccountControl(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag;
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				int value = 0;
				int num = 0;
				if (directoryObj.Contains("userAccountControl"))
				{
					value = (int)directoryObj["userAccountControl"].Value;
				}
				if (directoryObj.Contains("msDS-User-Account-Control-Computed"))
				{
					num = (int)directoryObj["msDS-User-Account-Control-Computed"].Value;
				}
				int num1 = value | num;
				int bit = UserAccountControlUtil.StringToBit(extendedAttribute);
				if (!UserAccountControlUtil.IsInverseBit(bit))
				{
					flag = (num1 & bit) != 0;
				}
				else
				{
					flag = (num1 & bit) == 0;
				}
				userObj.Add(extendedAttribute, flag);
				return;
			}
			else
			{
				return;
			}
		}

		internal static IADOPathNode ToSearchCannotChangePassword(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return ADAccountFactory<T>.ToSearchUserAccountControl(extendedAttribute, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchLockedOut(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			object[] objArray = new object[1];
			objArray[0] = extendedAttribute;
			throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterUseSearchADAccount, objArray));
		}

		internal static IADOPathNode ToSearchPrimaryGroup(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			string value;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				if (binaryADOPathNode.Operator == ADOperator.Eq || binaryADOPathNode.Operator == ADOperator.Ne)
				{
					IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
					if (rightNode == null)
					{
						throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
					}
					else
					{
						if (rightNode.DataObject as string == null)
						{
							if (rightNode.DataObject as ADGroup == null)
							{
								throw new ArgumentException(StringResources.SearchConverterRHSInvalidType);
							}
							else
							{
								value = ((ADGroup)rightNode.DataObject).SID.Value;
							}
						}
						else
						{
							string dataObject = rightNode.DataObject as string;
							using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(cmdletSessionInfo.ADSessionInfo))
							{
								aDObjectSearcher.SearchRoot = dataObject;
								aDObjectSearcher.Scope = ADSearchScope.Subtree;
								aDObjectSearcher.Properties.Add("objectSid");
								aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "distinguishedName", dataObject);
								value = aDObjectSearcher.FindOne()["objectSid"].Value.ToString();
							}
						}
						value = value.Substring(value.LastIndexOf("-") + 1, value.Length - value.LastIndexOf("-") - 1);
						return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "primaryGroupID", value);
					}
				}
				else
				{
					object[] str = new object[2];
					ADOperator[] aDOperatorArray = new ADOperator[2];
					aDOperatorArray[1] = ADOperator.Ne;
					str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
					str[1] = extendedAttribute;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
				}
			}
		}

		internal static IADOPathNode ToSearchUserAccountControl(string extendedAttribute, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			bool dataObject;
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
			else
			{
				if (binaryADOPathNode.Operator == ADOperator.Eq || binaryADOPathNode.Operator == ADOperator.Ne)
				{
					IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
					if (rightNode == null)
					{
						throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
					}
					else
					{
						if (!(rightNode.DataObject is bool))
						{
							if (rightNode.DataObject as string == null)
							{
								object[] str = new object[2];
								str[0] = rightNode.DataObject.GetType().ToString();
								str[1] = extendedAttribute;
								throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterRHSInvalidType, str));
							}
							else
							{
								dataObject = bool.Parse((string)rightNode.DataObject);
							}
						}
						else
						{
							dataObject = (bool)rightNode.DataObject;
						}
						if (binaryADOPathNode.Operator == ADOperator.Ne)
						{
							dataObject = !dataObject;
						}
						if (extendedAttribute != "LockedOut")
						{
							int bit = UserAccountControlUtil.StringToBit(extendedAttribute);
							if (UserAccountControlUtil.IsInverseBit(bit))
							{
								dataObject = !dataObject;
							}
							IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Band, "userAccountControl", bit);
							if (!dataObject)
							{
								return ADOPathUtil.CreateNotClause(aDOPathNode);
							}
							else
							{
								return aDOPathNode;
							}
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = extendedAttribute;
							throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterUseSearchADAccount, objArray));
						}
					}
				}
				else
				{
					object[] str1 = new object[2];
					ADOperator[] aDOperatorArray = new ADOperator[2];
					aDOperatorArray[1] = ADOperator.Ne;
					str1[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
					str1[1] = extendedAttribute;
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str1));
				}
			}
		}

		private bool UpdateCannotChangePassword(ADObject directoryObj, bool cannotChangePassword)
		{
			ADObject aDObject;
			if (!directoryObj.Contains("nTSecurityDescriptor") || directoryObj["nTSecurityDescriptor"].Value == null)
			{
				using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(base.CmdletSessionInfo.ADSessionInfo))
				{
					string value = directoryObj["distinguishedName"].Value as string;
					aDObjectSearcher.SearchRoot = value;
					aDObjectSearcher.Scope = ADSearchScope.Base;
					aDObjectSearcher.Properties.Add("nTSecurityDescriptor");
					aDObject = aDObjectSearcher.FindOne();
					if (aDObject == null)
					{
						object[] searchRoot = new object[2];
						searchRoot[0] = value;
						searchRoot[1] = aDObjectSearcher.SearchRoot;
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.IdentityNotFound, searchRoot));
					}
				}
				if (!aDObject.Contains("nTSecurityDescriptor"))
				{
					return false;
				}
				else
				{
					ActiveDirectorySecurity activeDirectorySecurity = aDObject["nTSecurityDescriptor"].Value as ActiveDirectorySecurity;
					if (activeDirectorySecurity != null)
					{
						directoryObj.Add("nTSecurityDescriptor", activeDirectorySecurity);
					}
					else
					{
						return false;
					}
				}
			}
			ActiveDirectorySecurity value1 = directoryObj["nTSecurityDescriptor"].Value as ActiveDirectorySecurity;
			return ADAccountFactory<T>.SetCannotChangePassword(value1, cannotChangePassword);
		}

		internal static class ADAccountPropertyMap
		{
			public readonly static PropertyMapEntry PrimaryGroup;

			public readonly static PropertyMapEntry UserPrincipalName;

			public readonly static PropertyMapEntry AccountExpirationDate;

			public readonly static PropertyMapEntry AccountLockoutTime;

			public readonly static PropertyMapEntry AllowReversiblePasswordEncryption;

			public readonly static PropertyMapEntry BadLogonCount;

			public readonly static PropertyMapEntry CannotChangePassword;

			public readonly static PropertyMapEntry Certificates;

			public readonly static PropertyMapEntry AccountNotDelegated;

			public readonly static PropertyMapEntry Enabled;

			public readonly static PropertyMapEntry LastBadPasswordAttempt;

			public readonly static PropertyMapEntry LastLogonDate;

			public readonly static PropertyMapEntry LockedOut;

			public readonly static PropertyMapEntry PasswordLastSet;

			public readonly static PropertyMapEntry PasswordNeverExpires;

			public readonly static PropertyMapEntry PasswordNotRequired;

			public readonly static PropertyMapEntry ServicePrincipalNames;

			public readonly static PropertyMapEntry TrustedForDelegation;

			public readonly static PropertyMapEntry KerberosEncryptionType;

			public readonly static PropertyMapEntry CompoundIdentitySupported;

			public readonly static PropertyMapEntry PrincipalsAllowedToDelegateToAccount;

			public readonly static PropertyMapEntry TrustedToAuthForDelegation;

			public readonly static PropertyMapEntry MNSLogonAccount;

			public readonly static PropertyMapEntry DoesNotRequirePreAuth;

			public readonly static PropertyMapEntry PasswordExpired;

			public readonly static PropertyMapEntry HomedirRequired;

			public readonly static PropertyMapEntry UseDESKeyOnly;

			static ADAccountPropertyMap()
			{
				string[] strArrays = new string[2];
				strArrays[0] = "primaryGroupID";
				strArrays[1] = "objectSid";
				ADAccountFactory<T>.ADAccountPropertyMap.PrimaryGroup = new PropertyMapEntry("PrimaryGroup", strArrays, null);
				ADAccountFactory<T>.ADAccountPropertyMap.UserPrincipalName = new PropertyMapEntry("UserPrincipalName", "userPrincipalName", "userPrincipalName");
				ADAccountFactory<T>.ADAccountPropertyMap.AccountExpirationDate = new PropertyMapEntry("AccountExpirationDate", "accountExpires", "accountExpires");
				ADAccountFactory<T>.ADAccountPropertyMap.AccountLockoutTime = new PropertyMapEntry("AccountLockoutTime", "lockoutTime", "lockoutTime");
				ADAccountFactory<T>.ADAccountPropertyMap.AllowReversiblePasswordEncryption = new PropertyMapEntry("AllowReversiblePasswordEncryption", "userAccountControl", "ms-DS-UserEncryptedTextPasswordAllowed");
				ADAccountFactory<T>.ADAccountPropertyMap.BadLogonCount = new PropertyMapEntry("BadLogonCount", "badPwdCount", "badPwdCount");
				ADAccountFactory<T>.ADAccountPropertyMap.CannotChangePassword = new PropertyMapEntry("CannotChangePassword", "nTSecurityDescriptor", "nTSecurityDescriptor");
				ADAccountFactory<T>.ADAccountPropertyMap.Certificates = new PropertyMapEntry("Certificates", "userCertificate", "userCertificate");
				string[] strArrays1 = new string[2];
				strArrays1[0] = "userAccountControl";
				strArrays1[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.AccountNotDelegated = new PropertyMapEntry("AccountNotDelegated", strArrays1, null);
				string[] strArrays2 = new string[2];
				strArrays2[0] = "userAccountControl";
				strArrays2[1] = "msDS-User-Account-Control-Computed";
				string[] strArrays3 = new string[1];
				strArrays3[0] = "msDS-UserAccountDisabled";
				ADAccountFactory<T>.ADAccountPropertyMap.Enabled = new PropertyMapEntry("Enabled", strArrays2, strArrays3);
				ADAccountFactory<T>.ADAccountPropertyMap.LastBadPasswordAttempt = new PropertyMapEntry("LastBadPasswordAttempt", "badPasswordTime", "badPasswordTime");
				ADAccountFactory<T>.ADAccountPropertyMap.LastLogonDate = new PropertyMapEntry("LastLogonDate", "lastLogonTimestamp", "lastLogonTimestamp");
				string[] strArrays4 = new string[3];
				strArrays4[0] = "userAccountControl";
				strArrays4[1] = "msDS-User-Account-Control-Computed";
				strArrays4[2] = "lockoutTime";
				string[] strArrays5 = new string[2];
				strArrays5[0] = "ms-DS-UserAccountAutoLocked";
				strArrays5[1] = "lockoutTime";
				ADAccountFactory<T>.ADAccountPropertyMap.LockedOut = new PropertyMapEntry("LockedOut", strArrays4, strArrays5);
				ADAccountFactory<T>.ADAccountPropertyMap.PasswordLastSet = new PropertyMapEntry("PasswordLastSet", "pwdLastSet", "pwdLastSet");
				string[] strArrays6 = new string[2];
				strArrays6[0] = "userAccountControl";
				strArrays6[1] = "msDS-User-Account-Control-Computed";
				string[] strArrays7 = new string[1];
				strArrays7[0] = "msDS-UserDontExpirePassword";
				ADAccountFactory<T>.ADAccountPropertyMap.PasswordNeverExpires = new PropertyMapEntry("PasswordNeverExpires", strArrays6, strArrays7);
				string[] strArrays8 = new string[2];
				strArrays8[0] = "userAccountControl";
				strArrays8[1] = "msDS-User-Account-Control-Computed";
				string[] strArrays9 = new string[1];
				strArrays9[0] = "ms-DS-UserPasswordNotRequired";
				ADAccountFactory<T>.ADAccountPropertyMap.PasswordNotRequired = new PropertyMapEntry("PasswordNotRequired", strArrays8, strArrays9);
				ADAccountFactory<T>.ADAccountPropertyMap.ServicePrincipalNames = new PropertyMapEntry("ServicePrincipalNames", "servicePrincipalName", null);
				string[] strArrays10 = new string[2];
				strArrays10[0] = "userAccountControl";
				strArrays10[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.TrustedForDelegation = new PropertyMapEntry("TrustedForDelegation", strArrays10, null);
				ADAccountFactory<T>.ADAccountPropertyMap.KerberosEncryptionType = new PropertyMapEntry("KerberosEncryptionType", "msDS-SupportedEncryptionTypes", null);
				ADAccountFactory<T>.ADAccountPropertyMap.CompoundIdentitySupported = new PropertyMapEntry("CompoundIdentitySupported", "msDS-SupportedEncryptionTypes", null);
				ADAccountFactory<T>.ADAccountPropertyMap.PrincipalsAllowedToDelegateToAccount = new PropertyMapEntry("PrincipalsAllowedToDelegateToAccount", "msDS-AllowedToActOnBehalfOfOtherIdentity", null);
				string[] strArrays11 = new string[2];
				strArrays11[0] = "userAccountControl";
				strArrays11[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.TrustedToAuthForDelegation = new PropertyMapEntry("TrustedToAuthForDelegation", strArrays11, null);
				string[] strArrays12 = new string[2];
				strArrays12[0] = "userAccountControl";
				strArrays12[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.MNSLogonAccount = new PropertyMapEntry("MNSLogonAccount", strArrays12, null);
				string[] strArrays13 = new string[2];
				strArrays13[0] = "userAccountControl";
				strArrays13[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.DoesNotRequirePreAuth = new PropertyMapEntry("DoesNotRequirePreAuth", strArrays13, null);
				string[] strArrays14 = new string[2];
				strArrays14[0] = "userAccountControl";
				strArrays14[1] = "msDS-User-Account-Control-Computed";
				string[] strArrays15 = new string[1];
				strArrays15[0] = "msDS-UserPasswordExpired";
				ADAccountFactory<T>.ADAccountPropertyMap.PasswordExpired = new PropertyMapEntry("PasswordExpired", strArrays14, strArrays15);
				string[] strArrays16 = new string[2];
				strArrays16[0] = "userAccountControl";
				strArrays16[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.HomedirRequired = new PropertyMapEntry("HomedirRequired", strArrays16, null);
				string[] strArrays17 = new string[2];
				strArrays17[0] = "userAccountControl";
				strArrays17[1] = "msDS-User-Account-Control-Computed";
				ADAccountFactory<T>.ADAccountPropertyMap.UseDESKeyOnly = new PropertyMapEntry("UseDESKeyOnly", strArrays17, null);
			}
		}
	}
}