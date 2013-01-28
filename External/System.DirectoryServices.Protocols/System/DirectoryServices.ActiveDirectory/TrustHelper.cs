using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace System.DirectoryServices.ActiveDirectory
{
	internal class TrustHelper
	{
		private static int STATUS_OBJECT_NAME_NOT_FOUND;

		internal static int ERROR_NOT_FOUND;

		internal static int NETLOGON_QUERY_LEVEL;

		internal static int NETLOGON_CONTROL_REDISCOVER;

		private static int NETLOGON_CONTROL_TC_VERIFY;

		private static int NETLOGON_VERIFY_STATUS_RETURNED;

		private static int PASSWORD_LENGTH;

		private static int TRUST_AUTH_TYPE_CLEAR;

		private static int PolicyDnsDomainInformation;

		private static int TRUSTED_SET_POSIX;

		private static int TRUSTED_SET_AUTH;

		internal static int TRUST_TYPE_DOWNLEVEL;

		internal static int TRUST_TYPE_UPLEVEL;

		internal static int TRUST_TYPE_MIT;

		private static int ERROR_ALREADY_EXISTS;

		private static int ERROR_INVALID_LEVEL;

		private static char[] punctuations;

		static TrustHelper()
		{
			TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND = 2;
			TrustHelper.ERROR_NOT_FOUND = 0x490;
			TrustHelper.NETLOGON_QUERY_LEVEL = 2;
			TrustHelper.NETLOGON_CONTROL_REDISCOVER = 5;
			TrustHelper.NETLOGON_CONTROL_TC_VERIFY = 10;
			TrustHelper.NETLOGON_VERIFY_STATUS_RETURNED = 128;
			TrustHelper.PASSWORD_LENGTH = 15;
			TrustHelper.TRUST_AUTH_TYPE_CLEAR = 2;
			TrustHelper.PolicyDnsDomainInformation = 12;
			TrustHelper.TRUSTED_SET_POSIX = 16;
			TrustHelper.TRUSTED_SET_AUTH = 32;
			TrustHelper.TRUST_TYPE_DOWNLEVEL = 1;
			TrustHelper.TRUST_TYPE_UPLEVEL = 2;
			TrustHelper.TRUST_TYPE_MIT = 3;
			TrustHelper.ERROR_ALREADY_EXISTS = 183;
			TrustHelper.ERROR_INVALID_LEVEL = 124;
			TrustHelper.punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
		}

		private TrustHelper()
		{
		}

		internal static void CreateTrust(DirectoryContext sourceContext, string sourceName, DirectoryContext targetContext, string targetName, bool isForest, TrustDirection direction, string password)
		{
			IntPtr intPtr = (IntPtr)0;
			IntPtr hGlobalUni = (IntPtr)0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			bool flag = false;
			IntPtr trustedDomainInfo = TrustHelper.GetTrustedDomainInfo(targetContext, targetName, isForest);
			try
			{
				try
				{
					POLICY_DNS_DOMAIN_INFO pOLICYDNSDOMAININFO = new POLICY_DNS_DOMAIN_INFO();
					Marshal.PtrToStructure(trustedDomainInfo, pOLICYDNSDOMAININFO);
					LSA_AUTH_INFORMATION lSAAUTHINFORMATION = new LSA_AUTH_INFORMATION();
					intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
					UnsafeNativeMethods.GetSystemTimeAsFileTime(intPtr);
					FileTime fileTime = new FileTime();
					Marshal.PtrToStructure(intPtr, fileTime);
					lSAAUTHINFORMATION.LastUpdateTime = new LARGE_INTEGER();
					lSAAUTHINFORMATION.LastUpdateTime.lowPart = fileTime.lower;
					lSAAUTHINFORMATION.LastUpdateTime.highPart = fileTime.higher;
					lSAAUTHINFORMATION.AuthType = TrustHelper.TRUST_AUTH_TYPE_CLEAR;
					hGlobalUni = Marshal.StringToHGlobalUni(password);
					lSAAUTHINFORMATION.AuthInfo = hGlobalUni;
					lSAAUTHINFORMATION.AuthInfoLength = password.Length * 2;
					intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_AUTH_INFORMATION)));
					Marshal.StructureToPtr(lSAAUTHINFORMATION, intPtr2, false);
					TRUSTED_DOMAIN_AUTH_INFORMATION tRUSTEDDOMAINAUTHINFORMATION = new TRUSTED_DOMAIN_AUTH_INFORMATION();
					if ((direction & TrustDirection.Inbound) != 0)
					{
						tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthInfos = 1;
						tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthenticationInformation = intPtr2;
						tRUSTEDDOMAINAUTHINFORMATION.IncomingPreviousAuthenticationInformation = (IntPtr)0;
					}
					if ((direction & TrustDirection.Outbound) != 0)
					{
						tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthInfos = 1;
						tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthenticationInformation = intPtr2;
						tRUSTEDDOMAINAUTHINFORMATION.OutgoingPreviousAuthenticationInformation = (IntPtr)0;
					}
					TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX = new TRUSTED_DOMAIN_INFORMATION_EX();
					tRUSTEDDOMAININFORMATIONEX.FlatName = pOLICYDNSDOMAININFO.Name;
					tRUSTEDDOMAININFORMATIONEX.Name = pOLICYDNSDOMAININFO.DnsDomainName;
					tRUSTEDDOMAININFORMATIONEX.Sid = pOLICYDNSDOMAININFO.Sid;
					tRUSTEDDOMAININFORMATIONEX.TrustType = TrustHelper.TRUST_TYPE_UPLEVEL;
					tRUSTEDDOMAININFORMATIONEX.TrustDirection = (int)direction;
					if (!isForest)
					{
						tRUSTEDDOMAININFORMATIONEX.TrustAttributes = TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN;
					}
					else
					{
						tRUSTEDDOMAININFORMATIONEX.TrustAttributes = TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE;
					}
					string policyServerName = Utils.GetPolicyServerName(sourceContext, isForest, false, sourceName);
					flag = Utils.Impersonate(sourceContext);
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					int winError = UnsafeNativeMethods.LsaCreateTrustedDomainEx(policySafeHandle, tRUSTEDDOMAININFORMATIONEX, tRUSTEDDOMAINAUTHINFORMATION, TrustHelper.TRUSTED_SET_POSIX | TrustHelper.TRUSTED_SET_AUTH, out intPtr1);
					if (winError != 0)
					{
						winError = UnsafeNativeMethods.LsaNtStatusToWinError(winError);
						if (winError != TrustHelper.ERROR_ALREADY_EXISTS)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
						else
						{
							if (!isForest)
							{
								object[] objArray = new object[2];
								objArray[0] = sourceName;
								objArray[1] = targetName;
								throw new ActiveDirectoryObjectExistsException(Res.GetString("AlreadyExistingDomainTrust", objArray));
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = sourceName;
								objArray1[1] = targetName;
								throw new ActiveDirectoryObjectExistsException(Res.GetString("AlreadyExistingForestTrust", objArray1));
							}
						}
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (intPtr != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr);
					}
					if (intPtr1 != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaClose(intPtr1);
					}
					if (trustedDomainInfo != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(trustedDomainInfo);
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
					if (intPtr2 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr2);
					}
				}
			}
			catch
			{
				throw;
			}
		}

		internal static string CreateTrustPassword()
		{
			byte[] numArray = new byte[TrustHelper.PASSWORD_LENGTH];
			char[] chrArray = new char[TrustHelper.PASSWORD_LENGTH];
			(new RNGCryptoServiceProvider()).GetBytes(numArray);
			for (int i = 0; i < TrustHelper.PASSWORD_LENGTH; i++)
			{
				int num = numArray[i] % 87;
				if (num >= 10)
				{
					if (num >= 36)
					{
						if (num >= 62)
						{
							chrArray[i] = TrustHelper.punctuations[num - 62];
						}
						else
						{
							chrArray[i] = (char)((ushort)(97 + num - 36));
						}
					}
					else
					{
						chrArray[i] = (char)((ushort)(65 + num - 10));
					}
				}
				else
				{
					chrArray[i] = (char)((ushort)(48 + num));
				}
			}
			string str = new string(chrArray);
			return str;
		}

		internal static void DeleteTrust(DirectoryContext sourceContext, string sourceName, string targetName, bool isForest)
		{
			int winError;
			IntPtr hGlobalUni = (IntPtr)0;
			IntPtr intPtr = (IntPtr)0;
			string policyServerName = Utils.GetPolicyServerName(sourceContext, isForest, false, sourceName);
			bool flag = Utils.Impersonate(sourceContext);
			try
			{
				try
				{
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni = Marshal.StringToHGlobalUni(targetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni);
					int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref intPtr);
					if (num == 0)
					{
						try
						{
							TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX = new TRUSTED_DOMAIN_INFORMATION_EX();
							Marshal.PtrToStructure(intPtr, tRUSTEDDOMAININFORMATIONEX);
							TrustHelper.ValidateTrustAttribute(tRUSTEDDOMAININFORMATIONEX, isForest, sourceName, targetName);
							num = UnsafeNativeMethods.LsaDeleteTrustedDomain(policySafeHandle, tRUSTEDDOMAININFORMATIONEX.Sid);
							if (num != 0)
							{
								winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
								throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
							}
						}
						finally
						{
							if (intPtr != (IntPtr)0)
							{
								UnsafeNativeMethods.LsaFreeMemory(intPtr);
							}
						}
					}
					else
					{
						winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
						if (winError != TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
						else
						{
							if (!isForest)
							{
								object[] objArray = new object[2];
								objArray[0] = sourceName;
								objArray[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", objArray), typeof(TrustRelationshipInformation), null);
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = sourceName;
								objArray1[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray1), typeof(ForestTrustRelationshipInformation), null);
							}
						}
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
				}
			}
			catch
			{
				throw;
			}
		}

		private static IntPtr GetTrustedDomainInfo(DirectoryContext targetContext, string targetName, bool isForest)
		{
			PolicySafeHandle policySafeHandle;
			IntPtr intPtr;
			IntPtr intPtr1 = (IntPtr)0;
			bool flag = false;
			try
			{
				try
				{
					string policyServerName = Utils.GetPolicyServerName(targetContext, isForest, false, targetName);
					flag = Utils.Impersonate(targetContext);
					try
					{
						policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					}
					catch (ActiveDirectoryOperationException activeDirectoryOperationException)
					{
						if (flag)
						{
							Utils.Revert();
							flag = false;
						}
						Utils.ImpersonateAnonymous();
						flag = true;
						policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					}
					catch (UnauthorizedAccessException unauthorizedAccessException)
					{
						if (flag)
						{
							Utils.Revert();
							flag = false;
						}
						Utils.ImpersonateAnonymous();
						flag = true;
						policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					}
					int num = UnsafeNativeMethods.LsaQueryInformationPolicy(policySafeHandle, TrustHelper.PolicyDnsDomainInformation, out intPtr1);
					if (num == 0)
					{
						intPtr = intPtr1;
					}
					else
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num), policyServerName);
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
				}
			}
			catch
			{
				throw;
			}
			return intPtr;
		}

		internal static bool GetTrustedDomainInfoStatus(DirectoryContext context, string sourceName, string targetName, TRUST_ATTRIBUTE attribute, bool isForest)
		{
			bool flag;
			IntPtr intPtr = (IntPtr)0;
			IntPtr hGlobalUni = (IntPtr)0;
			string policyServerName = Utils.GetPolicyServerName(context, isForest, false, sourceName);
			bool flag1 = Utils.Impersonate(context);
			try
			{
				try
				{
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni = Marshal.StringToHGlobalUni(targetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni);
					int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref intPtr);
					if (num == 0)
					{
						TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX = new TRUSTED_DOMAIN_INFORMATION_EX();
						Marshal.PtrToStructure(intPtr, tRUSTEDDOMAININFORMATIONEX);
						TrustHelper.ValidateTrustAttribute(tRUSTEDDOMAININFORMATIONEX, isForest, sourceName, targetName);
						if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION)
						{
							if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL)
							{
								if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN)
								{
									throw new ArgumentException("attribute");
								}
								else
								{
									if ((tRUSTEDDOMAININFORMATIONEX.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN) != 0)
									{
										flag = true;
									}
									else
									{
										flag = false;
									}
								}
							}
							else
							{
								if ((tRUSTEDDOMAININFORMATIONEX.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL) != 0)
								{
									flag = false;
								}
								else
								{
									flag = true;
								}
							}
						}
						else
						{
							if ((tRUSTEDDOMAININFORMATIONEX.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION) != 0)
							{
								flag = true;
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						int winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
						if (winError != TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
						else
						{
							if (!isForest)
							{
								object[] objArray = new object[2];
								objArray[0] = sourceName;
								objArray[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", objArray), typeof(TrustRelationshipInformation), null);
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = sourceName;
								objArray1[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray1), typeof(ForestTrustRelationshipInformation), null);
							}
						}
					}
				}
				finally
				{
					if (flag1)
					{
						Utils.Revert();
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr);
					}
				}
			}
			catch
			{
				throw;
			}
			return flag;
		}

		internal static void SetTrustedDomainInfoStatus(DirectoryContext context, string sourceName, string targetName, TRUST_ATTRIBUTE attribute, bool status, bool isForest)
		{
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr hGlobalUni = (IntPtr)0;
			string policyServerName = Utils.GetPolicyServerName(context, isForest, false, sourceName);
			bool flag = Utils.Impersonate(context);
			try
			{
				try
				{
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni = Marshal.StringToHGlobalUni(targetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni);
					int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref intPtr);
					if (num == 0)
					{
						TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX = new TRUSTED_DOMAIN_INFORMATION_EX();
						Marshal.PtrToStructure(intPtr, tRUSTEDDOMAININFORMATIONEX);
						TrustHelper.ValidateTrustAttribute(tRUSTEDDOMAININFORMATIONEX, isForest, sourceName, targetName);
						if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION)
						{
							if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL)
							{
								if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN)
								{
									throw new ArgumentException("attribute");
								}
								else
								{
									if (!status)
									{
										TRUSTED_DOMAIN_INFORMATION_EX trustAttributes = tRUSTEDDOMAININFORMATIONEX;
										trustAttributes.TrustAttributes = trustAttributes.TrustAttributes & (TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_NON_TRANSITIVE | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_UPLEVEL_ONLY | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_WITHIN_FOREST | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL);
									}
									else
									{
										TRUSTED_DOMAIN_INFORMATION_EX trustAttributes1 = tRUSTEDDOMAININFORMATIONEX;
										trustAttributes1.TrustAttributes = trustAttributes1.TrustAttributes | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN;
									}
								}
							}
							else
							{
								if (!status)
								{
									TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX1 = tRUSTEDDOMAININFORMATIONEX;
									tRUSTEDDOMAININFORMATIONEX1.TrustAttributes = tRUSTEDDOMAININFORMATIONEX1.TrustAttributes | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL;
								}
								else
								{
									TRUSTED_DOMAIN_INFORMATION_EX trustAttributes2 = tRUSTEDDOMAININFORMATIONEX;
									trustAttributes2.TrustAttributes = trustAttributes2.TrustAttributes & (TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_NON_TRANSITIVE | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_UPLEVEL_ONLY | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_WITHIN_FOREST);
								}
							}
						}
						else
						{
							if (!status)
							{
								TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX2 = tRUSTEDDOMAININFORMATIONEX;
								tRUSTEDDOMAININFORMATIONEX2.TrustAttributes = tRUSTEDDOMAININFORMATIONEX2.TrustAttributes & (TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_NON_TRANSITIVE | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_UPLEVEL_ONLY | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_WITHIN_FOREST | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL);
							}
							else
							{
								TRUSTED_DOMAIN_INFORMATION_EX trustAttributes3 = tRUSTEDDOMAININFORMATIONEX;
								trustAttributes3.TrustAttributes = trustAttributes3.TrustAttributes | TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION;
							}
						}
						intPtr1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TRUSTED_DOMAIN_INFORMATION_EX)));
						Marshal.StructureToPtr(tRUSTEDDOMAININFORMATIONEX, intPtr1, false);
						num = UnsafeNativeMethods.LsaSetTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, intPtr1);
						if (num != 0)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num), policyServerName);
						}
					}
					else
					{
						int winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
						if (winError != TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
						else
						{
							if (!isForest)
							{
								object[] objArray = new object[2];
								objArray[0] = sourceName;
								objArray[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", objArray), typeof(TrustRelationshipInformation), null);
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = sourceName;
								objArray1[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray1), typeof(ForestTrustRelationshipInformation), null);
							}
						}
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr);
					}
					if (intPtr1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr1);
					}
				}
			}
			catch
			{
				throw;
			}
		}

		internal static string UpdateTrust(DirectoryContext context, string sourceName, string targetName, string password, bool isForest)
		{
			string str;
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			IntPtr hGlobalUni = (IntPtr)0;
			IntPtr intPtr3 = (IntPtr)0;
			IntPtr hGlobalUni1 = (IntPtr)0;
			string policyServerName = Utils.GetPolicyServerName(context, isForest, false, sourceName);
			bool flag = Utils.Impersonate(context);
			try
			{
				try
				{
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni1 = Marshal.StringToHGlobalUni(targetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni1);
					int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, ref intPtr);
					if (num == 0)
					{
						TRUSTED_DOMAIN_FULL_INFORMATION tRUSTEDDOMAINFULLINFORMATION = new TRUSTED_DOMAIN_FULL_INFORMATION();
						Marshal.PtrToStructure(intPtr, tRUSTEDDOMAINFULLINFORMATION);
						TrustHelper.ValidateTrustAttribute(tRUSTEDDOMAINFULLINFORMATION.Information, isForest, sourceName, targetName);
						TrustDirection trustDirection = (TrustDirection)tRUSTEDDOMAINFULLINFORMATION.Information.TrustDirection;
						LSA_AUTH_INFORMATION lSAAUTHINFORMATION = new LSA_AUTH_INFORMATION();
						intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
						UnsafeNativeMethods.GetSystemTimeAsFileTime(intPtr2);
						FileTime fileTime = new FileTime();
						Marshal.PtrToStructure(intPtr2, fileTime);
						lSAAUTHINFORMATION.LastUpdateTime = new LARGE_INTEGER();
						lSAAUTHINFORMATION.LastUpdateTime.lowPart = fileTime.lower;
						lSAAUTHINFORMATION.LastUpdateTime.highPart = fileTime.higher;
						lSAAUTHINFORMATION.AuthType = TrustHelper.TRUST_AUTH_TYPE_CLEAR;
						hGlobalUni = Marshal.StringToHGlobalUni(password);
						lSAAUTHINFORMATION.AuthInfo = hGlobalUni;
						lSAAUTHINFORMATION.AuthInfoLength = password.Length * 2;
						intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_AUTH_INFORMATION)));
						Marshal.StructureToPtr(lSAAUTHINFORMATION, intPtr3, false);
						TRUSTED_DOMAIN_AUTH_INFORMATION tRUSTEDDOMAINAUTHINFORMATION = new TRUSTED_DOMAIN_AUTH_INFORMATION();
						if ((trustDirection & TrustDirection.Inbound) != 0)
						{
							tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthInfos = 1;
							tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthenticationInformation = intPtr3;
							tRUSTEDDOMAINAUTHINFORMATION.IncomingPreviousAuthenticationInformation = (IntPtr)0;
						}
						if ((trustDirection & TrustDirection.Outbound) != 0)
						{
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthInfos = 1;
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthenticationInformation = intPtr3;
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingPreviousAuthenticationInformation = (IntPtr)0;
						}
						tRUSTEDDOMAINFULLINFORMATION.AuthInformation = tRUSTEDDOMAINAUTHINFORMATION;
						intPtr1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TRUSTED_DOMAIN_FULL_INFORMATION)));
						Marshal.StructureToPtr(tRUSTEDDOMAINFULLINFORMATION, intPtr1, false);
						num = UnsafeNativeMethods.LsaSetTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, intPtr1);
						if (num == 0)
						{
							str = policyServerName;
						}
						else
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num), policyServerName);
						}
					}
					else
					{
						int winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
						if (winError != TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
						else
						{
							if (!isForest)
							{
								object[] objArray = new object[2];
								objArray[0] = sourceName;
								objArray[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", objArray), typeof(TrustRelationshipInformation), null);
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = sourceName;
								objArray1[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray1), typeof(ForestTrustRelationshipInformation), null);
							}
						}
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (hGlobalUni1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni1);
					}
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr);
					}
					if (intPtr1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr1);
					}
					if (intPtr2 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr2);
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
					if (intPtr3 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr3);
					}
				}
			}
			catch
			{
				throw;
			}
			return str;
		}

		internal static void UpdateTrustDirection(DirectoryContext context, string sourceName, string targetName, string password, bool isForest, TrustDirection newTrustDirection)
		{
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			IntPtr hGlobalUni = (IntPtr)0;
			IntPtr intPtr3 = (IntPtr)0;
			IntPtr hGlobalUni1 = (IntPtr)0;
			string policyServerName = Utils.GetPolicyServerName(context, isForest, false, sourceName);
			bool flag = Utils.Impersonate(context);
			try
			{
				try
				{
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni1 = Marshal.StringToHGlobalUni(targetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni1);
					int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, ref intPtr);
					if (num == 0)
					{
						TRUSTED_DOMAIN_FULL_INFORMATION tRUSTEDDOMAINFULLINFORMATION = new TRUSTED_DOMAIN_FULL_INFORMATION();
						Marshal.PtrToStructure(intPtr, tRUSTEDDOMAINFULLINFORMATION);
						TrustHelper.ValidateTrustAttribute(tRUSTEDDOMAINFULLINFORMATION.Information, isForest, sourceName, targetName);
						LSA_AUTH_INFORMATION lSAAUTHINFORMATION = new LSA_AUTH_INFORMATION();
						intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
						UnsafeNativeMethods.GetSystemTimeAsFileTime(intPtr2);
						FileTime fileTime = new FileTime();
						Marshal.PtrToStructure(intPtr2, fileTime);
						lSAAUTHINFORMATION.LastUpdateTime = new LARGE_INTEGER();
						lSAAUTHINFORMATION.LastUpdateTime.lowPart = fileTime.lower;
						lSAAUTHINFORMATION.LastUpdateTime.highPart = fileTime.higher;
						lSAAUTHINFORMATION.AuthType = TrustHelper.TRUST_AUTH_TYPE_CLEAR;
						hGlobalUni = Marshal.StringToHGlobalUni(password);
						lSAAUTHINFORMATION.AuthInfo = hGlobalUni;
						lSAAUTHINFORMATION.AuthInfoLength = password.Length * 2;
						intPtr3 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_AUTH_INFORMATION)));
						Marshal.StructureToPtr(lSAAUTHINFORMATION, intPtr3, false);
						TRUSTED_DOMAIN_AUTH_INFORMATION tRUSTEDDOMAINAUTHINFORMATION = new TRUSTED_DOMAIN_AUTH_INFORMATION();
						if ((newTrustDirection & TrustDirection.Inbound) == 0)
						{
							tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthInfos = 0;
							tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthenticationInformation = (IntPtr)0;
							tRUSTEDDOMAINAUTHINFORMATION.IncomingPreviousAuthenticationInformation = (IntPtr)0;
						}
						else
						{
							tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthInfos = 1;
							tRUSTEDDOMAINAUTHINFORMATION.IncomingAuthenticationInformation = intPtr3;
							tRUSTEDDOMAINAUTHINFORMATION.IncomingPreviousAuthenticationInformation = (IntPtr)0;
						}
						if ((newTrustDirection & TrustDirection.Outbound) == 0)
						{
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthInfos = 0;
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthenticationInformation = (IntPtr)0;
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingPreviousAuthenticationInformation = (IntPtr)0;
						}
						else
						{
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthInfos = 1;
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingAuthenticationInformation = intPtr3;
							tRUSTEDDOMAINAUTHINFORMATION.OutgoingPreviousAuthenticationInformation = (IntPtr)0;
						}
						tRUSTEDDOMAINFULLINFORMATION.AuthInformation = tRUSTEDDOMAINAUTHINFORMATION;
						tRUSTEDDOMAINFULLINFORMATION.Information.TrustDirection = (int)newTrustDirection;
						intPtr1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TRUSTED_DOMAIN_FULL_INFORMATION)));
						Marshal.StructureToPtr(tRUSTEDDOMAINFULLINFORMATION, intPtr1, false);
						num = UnsafeNativeMethods.LsaSetTrustedDomainInfoByName(policySafeHandle, lSAUNICODESTRING, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, intPtr1);
						if (num != 0)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num), policyServerName);
						}
					}
					else
					{
						int winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
						if (winError != TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(winError, policyServerName);
						}
						else
						{
							if (!isForest)
							{
								object[] objArray = new object[2];
								objArray[0] = sourceName;
								objArray[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", objArray), typeof(TrustRelationshipInformation), null);
							}
							else
							{
								object[] objArray1 = new object[2];
								objArray1[0] = sourceName;
								objArray1[1] = targetName;
								throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray1), typeof(ForestTrustRelationshipInformation), null);
							}
						}
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (hGlobalUni1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni1);
					}
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr);
					}
					if (intPtr1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr1);
					}
					if (intPtr2 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr2);
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
					if (intPtr3 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr3);
					}
				}
			}
			catch
			{
				throw;
			}
		}

		private static void ValidateTrust(PolicySafeHandle handle, LSA_UNICODE_STRING trustedDomainName, string sourceName, string targetName, bool isForest, int direction, string serverName)
		{
			IntPtr intPtr = (IntPtr)0;
			int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, trustedDomainName, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref intPtr);
			if (num == 0)
			{
				try
				{
					TRUSTED_DOMAIN_INFORMATION_EX tRUSTEDDOMAININFORMATIONEX = new TRUSTED_DOMAIN_INFORMATION_EX();
					Marshal.PtrToStructure(intPtr, tRUSTEDDOMAININFORMATIONEX);
					TrustHelper.ValidateTrustAttribute(tRUSTEDDOMAININFORMATIONEX, isForest, sourceName, targetName);
					if (direction != 0 && (direction & tRUSTEDDOMAININFORMATIONEX.TrustDirection) == 0)
					{
						if (!isForest)
						{
							object[] objArray = new object[3];
							objArray[0] = sourceName;
							objArray[1] = targetName;
							objArray[2] = (TrustDirection)direction;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", objArray), typeof(TrustRelationshipInformation), null);
						}
						else
						{
							object[] objArray1 = new object[3];
							objArray1[0] = sourceName;
							objArray1[1] = targetName;
							objArray1[2] = (TrustDirection)direction;
							throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", objArray1), typeof(ForestTrustRelationshipInformation), null);
						}
					}
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.LsaFreeMemory(intPtr);
					}
				}
				return;
			}
			else
			{
				int winError = UnsafeNativeMethods.LsaNtStatusToWinError(num);
				if (winError != TrustHelper.STATUS_OBJECT_NAME_NOT_FOUND)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(winError, serverName);
				}
				else
				{
					if (!isForest)
					{
						object[] objArray2 = new object[2];
						objArray2[0] = sourceName;
						objArray2[1] = targetName;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", objArray2), typeof(TrustRelationshipInformation), null);
					}
					else
					{
						object[] objArray3 = new object[2];
						objArray3[0] = sourceName;
						objArray3[1] = targetName;
						throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray3), typeof(ForestTrustRelationshipInformation), null);
					}
				}
			}
		}

		private static void ValidateTrustAttribute(TRUSTED_DOMAIN_INFORMATION_EX domainInfo, bool isForest, string sourceName, string targetName)
		{
			if (!isForest)
			{
				if ((domainInfo.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE) == 0)
				{
					if (domainInfo.TrustType != TrustHelper.TRUST_TYPE_DOWNLEVEL)
					{
						if (domainInfo.TrustType == TrustHelper.TRUST_TYPE_MIT)
						{
							throw new InvalidOperationException(Res.GetString("KerberosNotSupported"));
						}
					}
					else
					{
						throw new InvalidOperationException(Res.GetString("NT4NotSupported"));
					}
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = sourceName;
					objArray[1] = targetName;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongForestTrust", objArray), typeof(TrustRelationshipInformation), null);
				}
			}
			else
			{
				if ((domainInfo.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE) == 0)
				{
					object[] objArray1 = new object[2];
					objArray1[0] = sourceName;
					objArray1[1] = targetName;
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", objArray1), typeof(ForestTrustRelationshipInformation), null);
				}
			}
		}

		internal static void VerifyTrust(DirectoryContext context, string sourceName, string targetName, bool isForest, TrustDirection direction, bool forceSecureChannelReset, string preferredTargetServer)
		{
			int num;
			IntPtr hGlobalUni = (IntPtr)0;
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr1 = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			IntPtr hGlobalUni1 = (IntPtr)0;
			string policyServerName = Utils.GetPolicyServerName(context, isForest, false, sourceName);
			bool flag = Utils.Impersonate(context);
			try
			{
				try
				{
					PolicySafeHandle policySafeHandle = new PolicySafeHandle(Utils.GetPolicyHandle(policyServerName));
					LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
					hGlobalUni1 = Marshal.StringToHGlobalUni(targetName);
					UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni1);
					TrustHelper.ValidateTrust(policySafeHandle, lSAUNICODESTRING, sourceName, targetName, isForest, (int)direction, policyServerName);
					if (preferredTargetServer != null)
					{
						hGlobalUni = Marshal.StringToHGlobalUni(string.Concat(targetName, "\\", preferredTargetServer));
					}
					else
					{
						hGlobalUni = Marshal.StringToHGlobalUni(targetName);
					}
					intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
					Marshal.WriteIntPtr(intPtr, hGlobalUni);
					if (forceSecureChannelReset)
					{
						num = UnsafeNativeMethods.I_NetLogonControl2(policyServerName, TrustHelper.NETLOGON_CONTROL_REDISCOVER, TrustHelper.NETLOGON_QUERY_LEVEL, intPtr, out intPtr2);
						if (num != 0)
						{
							throw ExceptionHelper.GetExceptionFromErrorCode(num);
						}
					}
					else
					{
						num = UnsafeNativeMethods.I_NetLogonControl2(policyServerName, TrustHelper.NETLOGON_CONTROL_TC_VERIFY, TrustHelper.NETLOGON_QUERY_LEVEL, intPtr, out intPtr1);
						if (num != 0)
						{
							if (num != TrustHelper.ERROR_INVALID_LEVEL)
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(num);
							}
							else
							{
								throw new NotSupportedException(Res.GetString("TrustVerificationNotSupport"));
							}
						}
						else
						{
							NETLOGON_INFO_2 nETLOGONINFO2 = new NETLOGON_INFO_2();
							Marshal.PtrToStructure(intPtr1, nETLOGONINFO2);
							if ((nETLOGONINFO2.netlog2_flags & TrustHelper.NETLOGON_VERIFY_STATUS_RETURNED) == 0)
							{
								int netlog2TcConnectionStatus = nETLOGONINFO2.netlog2_tc_connection_status;
								throw ExceptionHelper.GetExceptionFromErrorCode(netlog2TcConnectionStatus);
							}
							else
							{
								int netlog2PdcConnectionStatus = nETLOGONINFO2.netlog2_pdc_connection_status;
								if (netlog2PdcConnectionStatus != 0)
								{
									throw ExceptionHelper.GetExceptionFromErrorCode(netlog2PdcConnectionStatus);
								}
								else
								{
									return;
								}
							}
						}
					}
				}
				finally
				{
					if (flag)
					{
						Utils.Revert();
					}
					if (hGlobalUni1 != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni1);
					}
					if (intPtr != (IntPtr)0)
					{
						Marshal.FreeHGlobal(intPtr);
					}
					if (hGlobalUni != (IntPtr)0)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
					if (intPtr1 != (IntPtr)0)
					{
						UnsafeNativeMethods.NetApiBufferFree(intPtr1);
					}
					if (intPtr2 != (IntPtr)0)
					{
						UnsafeNativeMethods.NetApiBufferFree(intPtr2);
					}
				}
			}
			catch
			{
				throw;
			}
		}
	}
}