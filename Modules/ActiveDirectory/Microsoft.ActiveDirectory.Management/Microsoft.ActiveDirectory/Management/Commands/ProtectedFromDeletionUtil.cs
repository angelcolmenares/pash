using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ProtectedFromDeletionUtil
	{
		private const int WriteDaclPermissionBit = 4;

		private const string CmdletCacheCategory = "ProtectedFromDeletionCache";

		private readonly static string[] AttributesToFetchOnObject;

		private readonly static string[] AttributesToFetchOnParent;

		static ProtectedFromDeletionUtil()
		{
			string[] strArrays = new string[4];
			strArrays[0] = "nTSecurityDescriptor";
			strArrays[1] = "sdRightsEffective";
			strArrays[2] = "instanceType";
			strArrays[3] = "isDeleted";
			ProtectedFromDeletionUtil.AttributesToFetchOnObject = strArrays;
			string[] strArrays1 = new string[2];
			strArrays1[0] = "nTSecurityDescriptor";
			strArrays1[1] = "sdRightsEffective";
			ProtectedFromDeletionUtil.AttributesToFetchOnParent = strArrays1;
		}

		private static void AddObjectToCache(ADObject directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			IDictionary<string, ADObject> protectedFromDeletionCache = ProtectedFromDeletionUtil.GetProtectedFromDeletionCache(cmdletSessionInfo);
			protectedFromDeletionCache[directoryObj.DistinguishedName] = directoryObj;
		}

		private static CmdletSessionInfo BuildCmdletSessionInfo(ADSessionInfo sessionInfo)
		{
			return new CmdletSessionInfo(sessionInfo, null, null, null, null, ADServerType.Unknown, new ADCmdletCache(), null, null, null);
		}

		private static bool EveryoneDeniedDeleteAndDeleteTree(ADEntity directoryObj)
		{
			bool flag;
			ActiveDirectorySecurity value = (ActiveDirectorySecurity)directoryObj["nTSecurityDescriptor"].Value;
			ActiveDirectoryAccessRule deleteAndDeleteTreeAccessRule = ProtectedFromDeletionUtil.ACEConstants.DeleteAndDeleteTreeAccessRule;
			IEnumerator enumerator = value.GetAccessRules(true, true, typeof(SecurityIdentifier)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ActiveDirectoryAccessRule current = (ActiveDirectoryAccessRule)enumerator.Current;
					if (current.AccessControlType != AccessControlType.Allow || !Utils.HasFlagsSet((int)current.ActiveDirectoryRights, 0x10000) && !Utils.HasFlagsSet((int)current.ActiveDirectoryRights, 64))
					{
						if (current.InheritanceFlags != deleteAndDeleteTreeAccessRule.InheritanceFlags || !(current.IdentityReference == deleteAndDeleteTreeAccessRule.IdentityReference) || current.AccessControlType != deleteAndDeleteTreeAccessRule.AccessControlType || !Utils.HasFlagsSet((int)current.ActiveDirectoryRights, (int)deleteAndDeleteTreeAccessRule.ActiveDirectoryRights))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					else
					{
						flag = false;
						return flag;
					}
				}
				return false;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return flag;
		}

		private static bool EveryoneDeniedDeleteChild(ADEntity directoryObj)
		{
			bool flag;
			ActiveDirectorySecurity value = (ActiveDirectorySecurity)directoryObj["nTSecurityDescriptor"].Value;
			ActiveDirectoryAccessRule deleteChildAccessRule = ProtectedFromDeletionUtil.ACEConstants.DeleteChildAccessRule;
			IEnumerator enumerator = value.GetAccessRules(true, true, typeof(SecurityIdentifier)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ActiveDirectoryAccessRule current = (ActiveDirectoryAccessRule)enumerator.Current;
					if (current.AccessControlType != AccessControlType.Allow || !Utils.HasFlagsSet((int)current.ActiveDirectoryRights, 2))
					{
						if (current.InheritanceFlags != deleteChildAccessRule.InheritanceFlags || !(current.IdentityReference == deleteChildAccessRule.IdentityReference) || current.AccessControlType != deleteChildAccessRule.AccessControlType || !Utils.HasFlagsSet((int)current.ActiveDirectoryRights, (int)deleteChildAccessRule.ActiveDirectoryRights))
						{
							continue;
						}
						flag = true;
						return flag;
					}
					else
					{
						flag = false;
						return flag;
					}
				}
				return false;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return flag;
		}

		private static ADObject GetCachedObject(string DN, string[] directoryAttributes, CmdletSessionInfo cmdletSessionInfo)
		{
			IDictionary<string, ADObject> protectedFromDeletionCache = ProtectedFromDeletionUtil.GetProtectedFromDeletionCache(cmdletSessionInfo);
			if (!protectedFromDeletionCache.ContainsKey(DN))
			{
				protectedFromDeletionCache.Add(DN, Utils.GetDirectoryObject(DN, directoryAttributes, cmdletSessionInfo));
			}
			return protectedFromDeletionCache[DN];
		}

		private static IDictionary<string, ADObject> GetProtectedFromDeletionCache(CmdletSessionInfo cmdletSessionInfo)
		{
			if (!cmdletSessionInfo.CmdletSessionCache.ContainsSubcache("ProtectedFromDeletionCache"))
			{
				cmdletSessionInfo.CmdletSessionCache.SetSubcache("ProtectedFromDeletionCache", new Dictionary<string, ADObject>());
			}
			return cmdletSessionInfo.CmdletSessionCache.GetSubcache("ProtectedFromDeletionCache") as Dictionary<string, ADObject>;
		}

		internal static bool? IsProtectedFromDeletion(ADObject directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag;
			ProtectedFromDeletionUtil.AddObjectToCache(directoryObj, cmdletSessionInfo);
			if (!directoryObj.Contains("nTSecurityDescriptor") || directoryObj["nTSecurityDescriptor"].Value == null)
			{
				bool? nullable = null;
				return nullable;
			}
			else
			{
				bool flag1 = ProtectedFromDeletionUtil.EveryoneDeniedDeleteAndDeleteTree(directoryObj);
				if (!Utils.IsNamingContext(directoryObj) && !Utils.IsDeleted(directoryObj))
				{
					string parentPath = ADPathModule.GetParentPath(directoryObj.DistinguishedName, null, ADPathFormat.X500);
					ADObject cachedObject = ProtectedFromDeletionUtil.GetCachedObject(parentPath, ProtectedFromDeletionUtil.AttributesToFetchOnParent, cmdletSessionInfo);
					if (cachedObject != null)
					{
						if (!cachedObject.Contains("nTSecurityDescriptor") || cachedObject["nTSecurityDescriptor"].Value == null)
						{
							bool? nullable1 = null;
							return nullable1;
						}
						else
						{
							if (!ProtectedFromDeletionUtil.EveryoneDeniedDeleteChild(cachedObject))
							{
								flag = false;
							}
							else
							{
								flag = flag1;
							}
							return new bool?(flag);
						}
					}
				}
				return new bool?(flag1);
			}
		}

		internal static bool? IsProtectedFromDeletion(ADObject directoryObj, ADSessionInfo sessionInfo)
		{
			return ProtectedFromDeletionUtil.IsProtectedFromDeletion(directoryObj, ProtectedFromDeletionUtil.BuildCmdletSessionInfo(sessionInfo));
		}

		internal static bool ProtectFromAccidentalDeletion(ADObject directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObject cachedObject;
			string value = directoryObj["distinguishedName"].Value as string;
			if (!directoryObj.Contains("nTSecurityDescriptor") || !directoryObj.Contains("sdRightsEffective"))
			{
				cachedObject = ProtectedFromDeletionUtil.GetCachedObject(value, ProtectedFromDeletionUtil.AttributesToFetchOnObject, cmdletSessionInfo);
			}
			else
			{
				cachedObject = directoryObj;
				ProtectedFromDeletionUtil.AddObjectToCache(directoryObj, cmdletSessionInfo);
			}
			if (cachedObject == null || !cachedObject.Contains("nTSecurityDescriptor") || cachedObject["nTSecurityDescriptor"].Value == null)
			{
				object[] objArray = new object[1];
				objArray[0] = directoryObj["distinguishedName"].Value;
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.InsufficientPermissionsToProtectObject, objArray));
			}
			else
			{
				if (ProtectedFromDeletionUtil.EveryoneDeniedDeleteAndDeleteTree(cachedObject) || Utils.HasFlagsSet((int)cachedObject["sdRightsEffective"].Value, 4))
				{
					if (!Utils.IsNamingContext(cachedObject) && !Utils.IsDeleted(cachedObject))
					{
						string parentPath = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
						ADObject aDObject = ProtectedFromDeletionUtil.GetCachedObject(parentPath, ProtectedFromDeletionUtil.AttributesToFetchOnParent, cmdletSessionInfo);
						if (aDObject != null && !ProtectedFromDeletionUtil.EveryoneDeniedDeleteChild(aDObject))
						{
							if (Utils.HasFlagsSet((int)aDObject["sdRightsEffective"].Value, 4))
							{
								ActiveDirectorySecurity activeDirectorySecurity = (ActiveDirectorySecurity)aDObject["nTSecurityDescriptor"].Value;
								activeDirectorySecurity.AddAccessRule(ProtectedFromDeletionUtil.ACEConstants.DeleteChildAccessRule);
								using (ADActiveObject aDActiveObject = new ADActiveObject(cmdletSessionInfo.ADSessionInfo, aDObject))
								{
									aDObject.TrackChanges = true;
									aDObject["nTSecurityDescriptor"].Value = activeDirectorySecurity;
									aDActiveObject.Update();
								}
							}
							else
							{
								object[] value1 = new object[2];
								value1[0] = directoryObj["distinguishedName"].Value;
								value1[1] = aDObject["distinguishedName"].Value;
								throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.InsufficientPermissionsToProtectObjectParent, value1));
							}
						}
					}
					ActiveDirectorySecurity activeDirectorySecurity1 = (ActiveDirectorySecurity)cachedObject["nTSecurityDescriptor"].Value;
					if (ProtectedFromDeletionUtil.EveryoneDeniedDeleteAndDeleteTree(cachedObject))
					{
						return false;
					}
					else
					{
						activeDirectorySecurity1.AddAccessRule(ProtectedFromDeletionUtil.ACEConstants.DeleteAndDeleteTreeAccessRule);
						if (!directoryObj.Contains("nTSecurityDescriptor"))
						{
							directoryObj.Add("nTSecurityDescriptor", activeDirectorySecurity1);
						}
						else
						{
							directoryObj["nTSecurityDescriptor"].Value = activeDirectorySecurity1;
						}
						return true;
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = directoryObj["distinguishedName"].Value;
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.InsufficientPermissionsToProtectObject, objArray1));
				}
			}
		}

		internal static bool ProtectFromAccidentalDeletion(ADObject directoryObj, ADSessionInfo sessionInfo)
		{
			return ProtectedFromDeletionUtil.ProtectFromAccidentalDeletion(directoryObj, ProtectedFromDeletionUtil.BuildCmdletSessionInfo(sessionInfo));
		}

		internal static bool ShouldProtectByDefault(string objectClass)
		{
			if (objectClass.Equals("organizationalUnit", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else
			{
				return objectClass.Equals("2.5.6.5", StringComparison.OrdinalIgnoreCase);
			}
		}

		internal static bool UnprotectFromAccidentalDeletion(ADObject directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObject cachedObject;
			bool hasValue;
			string value = directoryObj["distinguishedName"].Value as string;
			if (!directoryObj.Contains("nTSecurityDescriptor") || !directoryObj.Contains("sdRightsEffective"))
			{
				cachedObject = ProtectedFromDeletionUtil.GetCachedObject(value, ProtectedFromDeletionUtil.AttributesToFetchOnObject, cmdletSessionInfo);
			}
			else
			{
				cachedObject = directoryObj;
				ProtectedFromDeletionUtil.AddObjectToCache(directoryObj, cmdletSessionInfo);
			}
			if (cachedObject != null)
			{
				bool? nullable = ProtectedFromDeletionUtil.IsProtectedFromDeletion(cachedObject, cmdletSessionInfo);
				if (nullable.HasValue)
				{
					bool? nullable1 = nullable;
					if (nullable1.GetValueOrDefault())
					{
						hasValue = false;
					}
					else
					{
						hasValue = nullable1.HasValue;
					}
					if (!hasValue)
					{
						if (Utils.HasFlagsSet((int)cachedObject["sdRightsEffective"].Value, 4))
						{
							ActiveDirectorySecurity activeDirectorySecurity = (ActiveDirectorySecurity)cachedObject["nTSecurityDescriptor"].Value;
							activeDirectorySecurity.RemoveAccessRule(ProtectedFromDeletionUtil.ACEConstants.DeleteAndDeleteTreeAccessRule);
							if (!directoryObj.Contains("nTSecurityDescriptor"))
							{
								directoryObj.Add("nTSecurityDescriptor", activeDirectorySecurity);
							}
							else
							{
								directoryObj["nTSecurityDescriptor"].Value = activeDirectorySecurity;
							}
							return true;
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = directoryObj["distinguishedName"].Value;
							throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.InsufficientPermissionsToProtectObject, objArray));
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					object[] value1 = new object[1];
					value1[0] = directoryObj["distinguishedName"].Value;
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.InsufficientPermissionsToProtectObject, value1));
				}
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = directoryObj["distinguishedName"].Value;
				throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.InsufficientPermissionsToProtectObject, objArray1));
			}
		}

		internal static bool UnprotectFromAccidentalDeletion(ADObject directoryObj, ADSessionInfo sessionInfo)
		{
			return ProtectedFromDeletionUtil.UnprotectFromAccidentalDeletion(directoryObj, ProtectedFromDeletionUtil.BuildCmdletSessionInfo(sessionInfo));
		}

		private class ACEConstants
		{
			public readonly static ActiveDirectoryAccessRule DeleteAndDeleteTreeAccessRule;

			public readonly static ActiveDirectoryAccessRule DeleteChildAccessRule;

			static ACEConstants()
			{
				ProtectedFromDeletionUtil.ACEConstants.DeleteAndDeleteTreeAccessRule = new ActiveDirectoryAccessRule(new SecurityIdentifier("S-1-1-0"), ActiveDirectoryRights.Delete | ActiveDirectoryRights.DeleteTree, AccessControlType.Deny, ActiveDirectorySecurityInheritance.None);
				ProtectedFromDeletionUtil.ACEConstants.DeleteChildAccessRule = new ActiveDirectoryAccessRule(new SecurityIdentifier("S-1-1-0"), ActiveDirectoryRights.DeleteChild, AccessControlType.Deny, ActiveDirectorySecurityInheritance.None);
			}

			public ACEConstants()
			{
			}
		}
	}
}