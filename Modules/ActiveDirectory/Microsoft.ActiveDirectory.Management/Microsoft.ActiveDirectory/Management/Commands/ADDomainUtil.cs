using Microsoft.ActiveDirectory.Management;
using System;
using System.Management;
using System.Security.Principal;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADDomainUtil
	{
		private const string _debugCategory = "ADDomainUtil";

		internal const string IdentityDataKey = "IdentityData";

		public static ADSessionInfo ConstructSessionFromIdentity<P, T>(ADCmdletBase<P> cmdletInstance, ADSessionInfo baseSessionInfo, bool ignoreNonDomainIdentity)
		where P : ADParameterSet, new()
		where T : ADEntity, new()
		{
			string str = null;
			string str1;
			string item = cmdletInstance._cmdletParameters["Server"] as string;
			object obj = cmdletInstance._cmdletParameters["Identity"];
			if (item != null || obj == null)
			{
				return baseSessionInfo;
			}
			else
			{
				if (((ADEntity)obj).Identity == null)
				{
					str1 = null;
				}
				else
				{
					str1 = ((ADEntity)obj).Identity.ToString();
				}
				string str2 = str1;
				string str3 = ADDomainUtil.DiscoverDCFromIdentity<T>(obj, out str);
				if (str3 != null)
				{
					ADSessionInfo aDSessionInfo = baseSessionInfo.Copy();
					aDSessionInfo.Server = str;
					return aDSessionInfo;
				}
				else
				{
					if (!ignoreNonDomainIdentity)
					{
						ArgumentException argumentException = new ArgumentException();
						argumentException.Data.Add("IdentityData", str2);
						throw argumentException;
					}
					else
					{
						return baseSessionInfo;
					}
				}
			}
		}

		public static IADOPathNode CreateSidFilterClause(SecurityIdentifier identitySid)
		{
			byte[] numArray = new byte[identitySid.BinaryLength];
			identitySid.GetBinaryForm(numArray, 0);
			return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectSid", numArray);
		}

		public static string DiscoverDCFromIdentity<T>(object identity, out string domainDNSFromIdentity)
		where T : ADEntity, new()
		{
			return ADDomainUtil.DiscoverDCFromIdentity<T>(identity, false, out domainDNSFromIdentity);
		}

		public static string DiscoverDCFromIdentity<T>(object identity, bool forceDiscover, out string domainDNSFromIdentity)
		where T : ADEntity, new()
		{
			string str;
			ADEntity aDEntity;
			domainDNSFromIdentity = null;
			if (identity as T == null)
			{
				if (identity as string == null)
				{
					str = identity.ToString();
				}
				else
				{
					str = identity as string;
				}
			}
			else
			{
				T t = (T)identity;
				if (!t.IsSearchResult)
				{
					if (t.Identity as ADDomain == null || !((ADDomain)t.Identity).IsSearchResult)
					{
						str = t.Identity.ToString();
					}
					else
					{
						str = ((ADDomain)t.Identity).DNSRoot;
					}
				}
				else
				{
					if ((object)t as ADDomain == null)
					{
						if ((object)t as ADForest == null)
						{
							str = t["Name"].Value as string;
						}
						else
						{
							str = t["RootDomain"].Value as string;
						}
					}
					else
					{
						str = t["DNSRoot"].Value as string;
					}
				}
			}
			ADDiscoverableService[] aDDiscoverableServiceArray = new ADDiscoverableService[1];
			aDDiscoverableServiceArray[0] = ADDiscoverableService.ADWS;
			try
			{
				if (!forceDiscover)
				{
					aDEntity = DomainControllerUtil.DiscoverDomainController(null, str, aDDiscoverableServiceArray, ADDiscoverDomainControllerOptions.ReturnDnsName, new ADMinimumDirectoryServiceVersion?(ADMinimumDirectoryServiceVersion.Windows2000));
				}
				else
				{
					aDEntity = DomainControllerUtil.DiscoverDomainController(null, str, aDDiscoverableServiceArray, ADDiscoverDomainControllerOptions.ForceDiscover | ADDiscoverDomainControllerOptions.ReturnDnsName, new ADMinimumDirectoryServiceVersion?(ADMinimumDirectoryServiceVersion.Windows2000));
				}
				if (aDEntity != null)
				{
					domainDNSFromIdentity = str;
					string value = (string)aDEntity["HostName"].Value;
					return value;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				DebugLogger.LogError("ADDomainUtil", string.Concat("DiscoverDCFromIdentity: ", exception.ToString()));
			}
			return null;
		}

		public static string FindDomainNCHead(string identity, CmdletSessionInfo session)
		{
			ADObject aDObject;
			using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(session.ADSessionInfo))
			{
				aDObjectSearcher.SearchRoot = string.Concat("CN=Partitions,", session.ADRootDSE.ConfigurationNamingContext);
				aDObjectSearcher.Scope = ADSearchScope.OneLevel;
				IADOPathNode[] aDOPathNodeArray = new IADOPathNode[3];
				aDOPathNodeArray[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectCategory", "crossRef");
				aDOPathNodeArray[1] = ADOPathUtil.CreateFilterClause(ADOperator.Band, "systemFlags", 3);
				IADOPathNode[] aDOPathNodeArray1 = new IADOPathNode[3];
				aDOPathNodeArray1[0] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "nCName", identity);
				aDOPathNodeArray1[1] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "nETBIOSName", identity);
				aDOPathNodeArray1[2] = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "dnsRoot", identity);
				aDOPathNodeArray[2] = ADOPathUtil.CreateOrClause(aDOPathNodeArray1);
				aDObjectSearcher.Filter = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
				aDObjectSearcher.Properties.Add("nCName");
				aDObject = aDObjectSearcher.FindOne();
			}
			if (aDObject != null)
			{
				return aDObject.GetValue("nCName") as string;
			}
			else
			{
				return null;
			}
		}

		public static string GetLocalComputerDomain()
		{
			string item;
			SelectQuery selectQuery = new SelectQuery(WMIConstants.ComputerSystem);
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(selectQuery);
			ManagementObjectCollection managementObjectCollections = managementObjectSearcher.Get();
			ManagementObjectCollection.ManagementObjectEnumerator enumerator = managementObjectCollections.GetEnumerator();
			using (enumerator)
			{
				if (enumerator.MoveNext())
				{
					ManagementObject current = (ManagementObject)enumerator.Current;
					item = current[WMIConstants.Domain] as string;
				}
				else
				{
					return null;
				}
			}
			return item;
		}
	}
}