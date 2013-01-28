using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class DirectoryServerUtil
	{
		private const string _debugCategory = "DirectoryServerUtil";

		public DirectoryServerUtil()
		{
		}

		internal static void CheckIfObjectsRefersToSingleDirectoryServer(ADSessionInfo sessionInfo, ICollection<ADObject> objectList, bool checkForDCs, out string referredDirectoryServerDN, out ADObject computerObj, out ADObject serverObj, out ADObject ntdsDSAObj)
		{
			referredDirectoryServerDN = null;
			computerObj = null;
			serverObj = null;
			ntdsDSAObj = null;
			foreach (ADObject aDObject in objectList)
			{
				bool? nullable = aDObject.IsOfType("server");
				if (!nullable.Value)
				{
					bool? nullable1 = aDObject.IsOfType("computer");
					if (!nullable1.Value)
					{
						bool? nullable2 = aDObject.IsOfType("nTDSDSA");
						if (!nullable2.Value)
						{
							object[] objectClass = new object[1];
							objectClass[0] = aDObject.ObjectClass;
							DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Unknown object of type: '{0}' found in directory server list", objectClass));
							throw new NotSupportedException(string.Format(StringResources.UnsupportedObjectClass, aDObject.ObjectClass));
						}
						else
						{
							if (ntdsDSAObj != null)
							{
								object[] objArray = new object[1];
								objArray[0] = "nTDSDSA";
								DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Duplicate object of type: '{0}' found in directory server list", objArray));
								throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
							}
							else
							{
								ntdsDSAObj = aDObject;
							}
						}
					}
					else
					{
						if (checkForDCs)
						{
							if (computerObj != null)
							{
								object[] objArray1 = new object[1];
								objArray1[0] = "computer";
								DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Duplicate object of type: '{0}' found in directory server list", objArray1));
								throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
							}
							else
							{
								computerObj = aDObject;
							}
						}
						else
						{
							object[] objectClass1 = new object[1];
							objectClass1[0] = aDObject.ObjectClass;
							DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Unknown object of type: '{0}' found in directory server list", objectClass1));
							throw new NotSupportedException(string.Format(StringResources.UnsupportedObjectClass, aDObject.ObjectClass));
						}
					}
				}
				else
				{
					if (serverObj != null)
					{
						if (!DirectoryServerUtil.IsStaleServerObject(sessionInfo, aDObject.DistinguishedName))
						{
							object[] objArray2 = new object[1];
							objArray2[0] = "server";
							DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Duplicate object of type: '{0}' found in directory server list", objArray2));
							throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
						}
						else
						{
							object[] distinguishedName = new object[1];
							distinguishedName[0] = aDObject.DistinguishedName;
							DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Stale server object : '{0}' found in directory server list", distinguishedName));
						}
					}
					else
					{
						if (DirectoryServerUtil.IsStaleServerObject(sessionInfo, aDObject.DistinguishedName))
						{
							object[] distinguishedName1 = new object[1];
							distinguishedName1[0] = aDObject.DistinguishedName;
							DebugLogger.LogInfo("DirectoryServerUtil", string.Format(CultureInfo.CurrentCulture, "Stale server object : '{0}' found in directory server list", distinguishedName1));
						}
						else
						{
							serverObj = aDObject;
						}
					}
				}
			}
			if (computerObj != null)
			{
				referredDirectoryServerDN = computerObj["serverReferenceBL"].Value as string;
			}
			if (serverObj != null)
			{
				if (referredDirectoryServerDN == null || referredDirectoryServerDN.Equals(serverObj.DistinguishedName, StringComparison.OrdinalIgnoreCase))
				{
					referredDirectoryServerDN = serverObj.DistinguishedName;
				}
				else
				{
					throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
				}
			}
			if (ntdsDSAObj != null)
			{
				string str = ntdsDSAObj.DistinguishedName.Substring("CN=NTDS Settings,".Length);
				if (referredDirectoryServerDN == null || referredDirectoryServerDN.Equals(str, StringComparison.OrdinalIgnoreCase))
				{
					referredDirectoryServerDN = str;
				}
				else
				{
					throw new ADMultipleMatchingIdentitiesException(StringResources.MultipleMatches);
				}
			}
		}

		internal static bool IsStaleServerObject(ADSessionInfo sessionInfo, string serverObjectDN)
		{
			bool flag;
			if (serverObjectDN != null)
			{
				ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(sessionInfo, serverObjectDN, ADSearchScope.OneLevel);
				using (aDObjectSearcher)
				{
					aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "nTDSDSA");
					ADObject aDObject = aDObjectSearcher.FindOne();
					flag = aDObject == null;
				}
				return flag;
			}
			else
			{
				throw new ArgumentNullException("serverObjectDN");
			}
		}
	}
}