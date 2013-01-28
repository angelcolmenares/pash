using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADTopologyUtil
	{
		internal static IADOPathNode BuildSearchFilter(IADOPathNode filter, MappingTable<AttributeConverterEntry> attrMapBaseObj, MappingTable<AttributeConverterEntry> attrMapChildObj, CmdletSessionInfo sessionInfo)
		{
			BinaryADOPathNode binaryADOPathNode = filter as BinaryADOPathNode;
			if (binaryADOPathNode == null)
			{
				return null;
			}
			else
			{
				string ldapFilterString = binaryADOPathNode.LeftNode.GetLdapFilterString();
				AttributeConverterEntry attributeConverterEntry = null;
				if (!attrMapBaseObj.TryGetValue(ldapFilterString, out attributeConverterEntry))
				{
					if (!attrMapChildObj.TryGetValue(ldapFilterString, out attributeConverterEntry))
					{
						string[] strArrays = new string[1];
						strArrays[0] = ldapFilterString;
						return SearchConverters.ToSearchObjectClientSideFilter(ldapFilterString, strArrays, filter, sessionInfo);
					}
					else
					{
						return attributeConverterEntry.InvokeToSearcherConverter(filter, sessionInfo);
					}
				}
				else
				{
					return attributeConverterEntry.InvokeToSearcherConverter(filter, sessionInfo);
				}
			}
		}

		internal static void ConstructAggregateObject<F, O>(string childDN, ADObject parentObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			F f = Activator.CreateInstance<F>();
			f.SetCmdletSessionInfo(cmdletSessionInfo);
			O o = Activator.CreateInstance<O>();
			o.Identity = childDN;
			O extendedObjectFromIdentity = f.GetExtendedObjectFromIdentity(o, identityQueryPath, propertiesToFetch, showDeleted);
			if (extendedObjectFromIdentity != null)
			{
				ADTopologyUtil.MergeADObjectProperties(parentObj, extendedObjectFromIdentity);
			}
		}

		internal static string CreateISTPPath(ADInterSiteTransportProtocolType type, string ConfigurationNC)
		{
			string str;
			string str1 = ADPathModule.MakePath(ConfigurationNC, "CN=Inter-Site Transports,CN=Sites,", ADPathFormat.X500);
			if (type != ADInterSiteTransportProtocolType.SMTP)
			{
				str = "CN=IP,";
			}
			else
			{
				str = "CN=SMTP,";
			}
			return ADPathModule.MakePath(str1, str, ADPathFormat.X500);
		}

		internal static object FromNameToTopologyDN<F, O>(object entity, string extendedAttribute, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
			return AttributeConverters.GetAttributeValueFromObjectName<F, O>(entity, str, null, extendedAttribute, cmdletSessionInfo);
		}

		internal static ICollection<string> GetChildAttributes(MappingTable<AttributeConverterEntry> attrMapBaseObj, MappingTable<AttributeConverterEntry> attrMapChildObj, ICollection<string> propertiesToFetch)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (propertiesToFetch != null)
			{
				if (!propertiesToFetch.Contains("*"))
				{
					foreach (string str in propertiesToFetch)
					{
						if (!attrMapChildObj.TryGetValue(str, out attributeConverterEntry) || attrMapBaseObj.TryGetValue(str, out attributeConverterEntry))
						{
							continue;
						}
						strs.Add(str);
					}
				}
				else
				{
					foreach (AttributeConverterEntry value in attrMapChildObj.Values)
					{
						strs.Add(value.ExtendedAttribute);
					}
				}
			}
			return strs;
		}

		internal static ICollection<string> GetDefaultPropertyFromMappingTable(MappingTable<AttributeConverterEntry> attrMapTable)
		{
			List<string> strs = new List<string>();
			foreach (AttributeConverterEntry value in attrMapTable.Values)
			{
				if (value.AttributeSet != AttributeSet.Default)
				{
					continue;
				}
				strs.Add(value.ExtendedAttribute);
			}
			return strs;
		}

		internal static ICollection<string> GetParentAttributes(MappingTable<AttributeConverterEntry> attrMapBaseObj, MappingTable<AttributeConverterEntry> attrMapChildObj, ICollection<string> propertiesToFetch)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (propertiesToFetch != null)
			{
				foreach (string str in propertiesToFetch)
				{
					if (!attrMapBaseObj.TryGetValue(str, out attributeConverterEntry) && attrMapChildObj.TryGetValue(str, out attributeConverterEntry))
					{
						continue;
					}
					strs.Add(str);
				}
			}
			return strs;
		}

		internal static void MergeADObjectProperties(ADEntity parentObj, ADEntity childObj)
		{
			parentObj.TrackChanges = false;
			foreach (string propertyName in childObj.PropertyNames)
			{
				if (parentObj.Contains(propertyName))
				{
					continue;
				}
				parentObj.Add(propertyName, childObj[propertyName].Value);
			}
			parentObj.TrackChanges = true;
		}

		internal static void RemoveChildObjectAttributes(ADObject directoryObj, MappingTable<AttributeConverterEntry> attrMapBaseObj, MappingTable<AttributeConverterEntry> attrMapChildObj)
		{
			HashSet<string> strs = new HashSet<string>(attrMapChildObj.Keys, StringComparer.OrdinalIgnoreCase);
			strs.ExceptWith(attrMapBaseObj.Keys);
			foreach (string str in strs)
			{
				directoryObj.Remove(str);
				directoryObj.AddedProperties.Remove(str);
				directoryObj.RemovedProperties.Remove(str);
			}
		}

		internal static IEnumerable<O> RemoveExtraPropertiesFromADAggregateObject<O>(MappingTable<AttributeConverterEntry> attrMapBaseObj, MappingTable<AttributeConverterEntry> attrMapChildObj, ICollection<string> propertiesToFetch, IEnumerable<O> aggregateObjectList)
		where O : ADEntity, new()
		{
			List<string> strs = null;
			if (propertiesToFetch == null || !propertiesToFetch.Contains("*"))
			{
				IEnumerable<string> defaultPropertyFromMappingTable = ADTopologyUtil.GetDefaultPropertyFromMappingTable(attrMapBaseObj);
				IEnumerable<string> defaultPropertyFromMappingTable1 = ADTopologyUtil.GetDefaultPropertyFromMappingTable(attrMapChildObj);
				HashSet<string> strs1 = new HashSet<string>(defaultPropertyFromMappingTable, StringComparer.OrdinalIgnoreCase);
				strs1.UnionWith(defaultPropertyFromMappingTable1);
				if (propertiesToFetch != null)
				{
					strs1.UnionWith(propertiesToFetch);
				}
				foreach (O o in aggregateObjectList)
				{
					o.TrackChanges = false;
					strs = new List<string>();
					foreach (string propertyName in o.PropertyNames)
					{
						if (strs1.Contains(propertyName))
						{
							continue;
						}
						strs.Add(propertyName);
					}
					List<string>.Enumerator enumerator = strs.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							string str = enumerator.Current;
							o.Remove(str);
						}
					}
					finally
					{
						enumerator.Dispose();
					}
					o.TrackChanges = true;
				}
				return aggregateObjectList;
			}
			else
			{
				return aggregateObjectList;
			}
		}

		internal static void ToDirectoryFromServerNameToNTDSSettings(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToDirectoryFromADEntityToAttributeValue<ADDirectoryServerFactory<ADDirectoryServer>, ADDirectoryServer>(cmdletSessionInfo.ADRootDSE.DefaultNamingContext, "NTDSSettingsObjectDN", extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryFromSiteNameToDN(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
			AttributeConverters.ToDirectoryFromADEntityToAttributeValue<ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>(str, null, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryFromTopologyObjectNameListToDNList<F, O>(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			AttributeConverters.ToDirectoryMultivalueObjectConvertor(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo, new MultiValueAttributeConvertorDelegate(ADTopologyUtil.FromNameToTopologyDN<F, O>));
		}

		internal static void ToExtendedFromDNToISTPEnum(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = (string)directoryObj[directoryAttributes[0]].Value;
			value = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
			string childName = ADPathModule.GetChildName(value, ADPathFormat.X500);
			ADInterSiteTransportProtocolType aDInterSiteTransportProtocolType = ADInterSiteTransportProtocolType.IP;
			if (string.Compare("CN=IP", childName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				aDInterSiteTransportProtocolType = ADInterSiteTransportProtocolType.SMTP;
			}
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection((object)aDInterSiteTransportProtocolType);
			userObj.Add(extendedAttribute, aDPropertyValueCollection);
		}

		internal static IADOPathNode ToSearchFromTopologyObjectNameToDN<F, O>(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
			return SearchConverters.ToSearchFromADEntityToAttributeValue<F, O>(str, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}
	}
}