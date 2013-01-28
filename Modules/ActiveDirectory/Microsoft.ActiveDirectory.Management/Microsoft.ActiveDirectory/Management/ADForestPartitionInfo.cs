using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADForestPartitionInfo
	{
		private const string _partitionsDNPrefix = "CN=Partitions,";

		private const string _debugCategory = "ADForestPartitionInfo";

		private static Dictionary<string, ADForestPartitionInfo> _forestInfoTable;

		private static object _forestInfoTableLock;

		private string _forestId;

		private ReadOnlyCollection<string> _forestPartitionList;

		public string ForestId
		{
			get
			{
				return this._forestId;
			}
		}

		private IList<string> PartitionList
		{
			get
			{
				return this._forestPartitionList;
			}
		}

		static ADForestPartitionInfo()
		{
			ADForestPartitionInfo._forestInfoTable = new Dictionary<string, ADForestPartitionInfo>(StringComparer.OrdinalIgnoreCase);
			ADForestPartitionInfo._forestInfoTableLock = new object();
		}

		private ADForestPartitionInfo()
		{
			throw new NotImplementedException();
		}

		private ADForestPartitionInfo(ADRootDSE rootDSE)
		{
			this._forestId = rootDSE.SubSchemaSubEntry;
			this.Refresh(rootDSE);
		}

		private static string ConcatList(IEnumerable<string> stringList, string delimiter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string str in stringList)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(delimiter);
				}
				stringBuilder.Append(str);
			}
			return stringBuilder.ToString();
		}

		internal static IEnumerable<string> ConstructPartitionList(ADRootDSE rootDSE, IEnumerable<string> partitionList, bool refreshForestPartitionList)
		{
			IEnumerable<string> validPartitionList;
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IEnumerator<string> enumerator = partitionList.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					string str = null;
					try
					{
						str = ADForestPartitionInfo.ConvertFriendlyPartition(rootDSE, current);
					}
					catch (ADIdentityNotFoundException aDIdentityNotFoundException)
					{
						continue;
					}
					if (str == null)
					{
						if (!string.Equals(current, "*", StringComparison.OrdinalIgnoreCase))
						{
							if (string.IsNullOrEmpty(current) || !ADForestPartitionInfo.IsValidPartitionDN(rootDSE, current, refreshForestPartitionList))
							{
								continue;
							}
							strs.Add(current);
						}
						else
						{
							validPartitionList = ADForestPartitionInfo.GetValidPartitionList(rootDSE, refreshForestPartitionList);
							return validPartitionList;
						}
					}
					else
					{
						strs.Add(str);
					}
				}
				return strs;
			}
			return validPartitionList;
		}

		internal static string ConvertFriendlyPartition(ADRootDSE rootDSE, string partition)
		{
			if (!string.Equals(partition, "Default", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.Equals(partition, "Domain", StringComparison.OrdinalIgnoreCase))
				{
					if (string.Equals(partition, "Config", StringComparison.OrdinalIgnoreCase) || string.Equals(partition, "Configuration", StringComparison.OrdinalIgnoreCase))
					{
						if (string.IsNullOrEmpty(rootDSE.ConfigurationNamingContext))
						{
							object[] dNSHostName = new object[2];
							dNSHostName[0] = rootDSE.DNSHostName;
							dNSHostName[1] = partition;
							throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDoesNotHaveFriendlyPartition, dNSHostName));
						}
						else
						{
							return rootDSE.ConfigurationNamingContext;
						}
					}
					else
					{
						if (!string.Equals(partition, "Schema", StringComparison.OrdinalIgnoreCase))
						{
							return null;
						}
						else
						{
							if (string.IsNullOrEmpty(rootDSE.SchemaNamingContext))
							{
								object[] objArray = new object[2];
								objArray[0] = rootDSE.DNSHostName;
								objArray[1] = partition;
								throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDoesNotHaveFriendlyPartition, objArray));
							}
							else
							{
								return rootDSE.SchemaNamingContext;
							}
						}
					}
				}
				else
				{
					if (string.IsNullOrEmpty(rootDSE.RootDomainNamingContext))
					{
						object[] dNSHostName1 = new object[2];
						dNSHostName1[0] = rootDSE.DNSHostName;
						dNSHostName1[1] = partition;
						throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDoesNotHaveFriendlyPartition, dNSHostName1));
					}
					else
					{
						return rootDSE.RootDomainNamingContext;
					}
				}
			}
			else
			{
				if (string.IsNullOrEmpty(rootDSE.DefaultNamingContext))
				{
					object[] objArray1 = new object[2];
					objArray1[0] = rootDSE.DNSHostName;
					objArray1[1] = partition;
					throw new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerDoesNotHaveFriendlyPartition, objArray1));
				}
				else
				{
					return rootDSE.DefaultNamingContext;
				}
			}
		}

		public static string ExtractAndValidatePartitionInfo(ADRootDSE rootDSE, string objectDN)
		{
			IEnumerable<string> partitionList;
			string str = ADForestPartitionInfo.ExtractPartitionInfo(rootDSE, objectDN, false);
			if (str == null)
			{
				str = ADForestPartitionInfo.ExtractPartitionInfo(rootDSE, objectDN, true);
				if (str == null)
				{
					CultureInfo currentCulture = CultureInfo.CurrentCulture;
					string invalidDNMustBelongToValidPartitionSet = StringResources.InvalidDNMustBelongToValidPartitionSet;
					object[] objArray = new object[1];
					object[] objArray1 = objArray;
					int num = 0;
					if (rootDSE.SessionInfo.ConnectedToGC)
					{
						partitionList = ADForestPartitionInfo.GetForestPartitionInfo(rootDSE).PartitionList;
					}
					else
					{
						partitionList = rootDSE.NamingContexts;
					}
					objArray1[num] = ADForestPartitionInfo.ConcatList(partitionList, " , ");
					throw new ArgumentException(string.Format(currentCulture, invalidDNMustBelongToValidPartitionSet, objArray));
				}
			}
			return str;
		}

		internal static string ExtractPartitionInfo(ADRootDSE rootDSE, string objectDN, bool refreshForestPartitionList)
		{
			string str = null;
			if (rootDSE != null)
			{
				if (objectDN != null)
				{
					if (rootDSE.SessionInfo == null || !rootDSE.SessionInfo.ConnectedToGC || !objectDN.Equals(string.Empty))
					{
						IEnumerable<string> validPartitionList = ADForestPartitionInfo.GetValidPartitionList(rootDSE, refreshForestPartitionList);
						int length = -1;
						foreach (string str1 in validPartitionList)
						{
							if (str1.Length <= length || !ADPathModule.IsChildPath(objectDN, str1, true, ADPathFormat.X500))
							{
								continue;
							}
							length = str1.Length;
							str = str1;
						}
						return str;
					}
					else
					{
						return string.Empty;
					}
				}
				else
				{
					throw new ArgumentNullException("objectDN");
				}
			}
			else
			{
				throw new ArgumentNullException("rootDSE");
			}
		}

		private static ADForestPartitionInfo GetForestPartitionInfo(ADSessionInfo sessionInfo)
		{
			ADForestPartitionInfo forestPartitionInfo;
			using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(sessionInfo))
			{
				forestPartitionInfo = ADForestPartitionInfo.GetForestPartitionInfo(aDObjectSearcher.GetRootDSE());
			}
			return forestPartitionInfo;
		}

		private static ADForestPartitionInfo GetForestPartitionInfo(ADRootDSE rootDSE)
		{
			if (rootDSE != null)
			{
				string subSchemaSubEntry = rootDSE.SubSchemaSubEntry;
				ADForestPartitionInfo aDForestPartitionInfo = null;
				lock (ADForestPartitionInfo._forestInfoTableLock)
				{
					ADForestPartitionInfo._forestInfoTable.TryGetValue(subSchemaSubEntry, out aDForestPartitionInfo);
				}
				if (aDForestPartitionInfo == null)
				{
					object[] objArray = new object[1];
					objArray[0] = rootDSE.SubSchemaSubEntry;
					DebugLogger.LogInfo("ADForestPartitionInfo", "Getting forest info from server for Forest: {0}", objArray);
					aDForestPartitionInfo = new ADForestPartitionInfo(rootDSE);
					lock (ADForestPartitionInfo._forestInfoTableLock)
					{
						if (ADForestPartitionInfo._forestInfoTable.ContainsKey(subSchemaSubEntry))
						{
							ADForestPartitionInfo._forestInfoTable.Remove(subSchemaSubEntry);
						}
						ADForestPartitionInfo._forestInfoTable.Add(subSchemaSubEntry, aDForestPartitionInfo);
					}
					return aDForestPartitionInfo;
				}
				else
				{
					object[] subSchemaSubEntry1 = new object[1];
					subSchemaSubEntry1[0] = rootDSE.SubSchemaSubEntry;
					DebugLogger.LogInfo("ADForestPartitionInfo", "Found ADForestPartitionInfo for Forest: {0} in Cache", subSchemaSubEntry1);
					return aDForestPartitionInfo;
				}
			}
			else
			{
				throw new ArgumentNullException("rootDSE");
			}
		}

		private static IEnumerable<string> GetValidPartitionList(ADRootDSE rootDSE, bool refreshForestPartitionList)
		{
			if (rootDSE.SessionInfo == null || !rootDSE.SessionInfo.ConnectedToGC)
			{
				return rootDSE.NamingContexts;
			}
			else
			{
				if (refreshForestPartitionList)
				{
					ADForestPartitionInfo.GetForestPartitionInfo(rootDSE).Refresh(rootDSE);
				}
				return ADForestPartitionInfo.GetForestPartitionInfo(rootDSE).PartitionList;
			}
		}

		internal static bool IsDNUnderPartition(ADRootDSE rootDSE, string objectDN, bool refreshForestPartitionList)
		{
			bool flag;
			if (rootDSE != null)
			{
				if (!string.IsNullOrEmpty(objectDN))
				{
					IEnumerable<string> validPartitionList = ADForestPartitionInfo.GetValidPartitionList(rootDSE, refreshForestPartitionList);
					IEnumerator<string> enumerator = validPartitionList.GetEnumerator();
					using (enumerator)
					{
						while (enumerator.MoveNext())
						{
							string current = enumerator.Current;
							if (!ADPathModule.IsChildPath(objectDN, current, true, ADPathFormat.X500))
							{
								continue;
							}
							flag = true;
							return flag;
						}
						return false;
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
				throw new ArgumentNullException("rootDSE");
			}
		}

		private static bool IsValidPartitionDN(ADRootDSE rootDSE, string partitionDN, bool refreshForestPartitionList)
		{
			bool flag;
			if (rootDSE != null)
			{
				if (partitionDN != null)
				{
					if (rootDSE.SessionInfo == null || !rootDSE.SessionInfo.ConnectedToGC || !partitionDN.Equals(string.Empty))
					{
						IEnumerable<string> validPartitionList = ADForestPartitionInfo.GetValidPartitionList(rootDSE, refreshForestPartitionList);
						IEnumerator<string> enumerator = validPartitionList.GetEnumerator();
						using (enumerator)
						{
							while (enumerator.MoveNext())
							{
								string current = enumerator.Current;
								if (!ADPathModule.ComparePath(partitionDN, current, ADPathFormat.X500))
								{
									continue;
								}
								flag = true;
								return flag;
							}
							return false;
						}
						return flag;
					}
					else
					{
						return true;
					}
				}
				else
				{
					throw new ArgumentNullException("partitionDN");
				}
			}
			else
			{
				throw new ArgumentNullException("rootDSE");
			}
		}

		private void Refresh(ADRootDSE rootDSE)
		{
			if (rootDSE != null)
			{
				if (rootDSE.SessionInfo != null)
				{
					if (this._forestId.Equals(rootDSE.SubSchemaSubEntry, StringComparison.OrdinalIgnoreCase))
					{
						object[] objArray = new object[1];
						objArray[0] = this._forestId;
						DebugLogger.LogInfo("ADForestPartitionInfo", "Refreshing PartitionList of Forest: {0}", objArray);
						List<string> strs = new List<string>();
						ADSessionInfo sessionInfo = rootDSE.SessionInfo;
						if (rootDSE.ServerType == ADServerType.ADDS && sessionInfo.ConnectedToGC)
						{
							sessionInfo = sessionInfo.Copy();
							sessionInfo.SetEffectivePort(LdapConstants.LDAP_PORT);
						}
						using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(sessionInfo))
						{
							aDObjectSearcher.SchemaTranslation = false;
							aDObjectSearcher.SearchRoot = string.Concat("CN=Partitions,", rootDSE.ConfigurationNamingContext);
							aDObjectSearcher.Properties.Add("nCName");
							aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "crossRef");
							foreach (ADObject aDObject in aDObjectSearcher.FindAll())
							{
								if (aDObject["nCName"] == null || aDObject["nCName"].Count <= 0)
								{
									continue;
								}
								strs.Add((string)aDObject["nCName"][0]);
							}
							this._forestPartitionList = new ReadOnlyCollection<string>(strs);
						}
						return;
					}
					else
					{
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ForestIdDoesNotMatch, new object[0]));
					}
				}
				else
				{
					throw new ArgumentNullException("rootDSE.SessionInfo");
				}
			}
			else
			{
				throw new ArgumentNullException("rootDSE");
			}
		}

		public static void ValidatePartitionDN(ADRootDSE rootDSE, string partitionDN)
		{
			IEnumerable<string> partitionList;
			if (ADForestPartitionInfo.IsValidPartitionDN(rootDSE, partitionDN, false) || ADForestPartitionInfo.IsValidPartitionDN(rootDSE, partitionDN, true))
			{
				return;
			}
			else
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				string invalidPartitionMustBelongToValidSet = StringResources.InvalidPartitionMustBelongToValidSet;
				object[] objArray = new object[1];
				object[] objArray1 = objArray;
				int num = 0;
				if (rootDSE.SessionInfo.ConnectedToGC)
				{
					partitionList = ADForestPartitionInfo.GetForestPartitionInfo(rootDSE).PartitionList;
				}
				else
				{
					partitionList = rootDSE.NamingContexts;
				}
				objArray1[num] = ADForestPartitionInfo.ConcatList(partitionList, " , ");
				throw new ArgumentException(string.Format(currentCulture, invalidPartitionMustBelongToValidSet, objArray));
			}
		}
	}
}