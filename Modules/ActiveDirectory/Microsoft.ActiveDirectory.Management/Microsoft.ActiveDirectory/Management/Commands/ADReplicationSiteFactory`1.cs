using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationSiteFactory<T> : ADObjectFactory<T>
	where T : ADReplicationSite, new()
	{
		private readonly static IADOPathNode _structuralObjectFilter;

		private readonly static string _rDNPrefix;

		private readonly static string _structuralObjectClass;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private string _debugCategory;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADObjectFactory<T>.AttributeTable;
			}
		}

		internal override IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADReplicationSiteFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADReplicationSiteFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return ADReplicationSiteFactory<T>._structuralObjectClass;
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADReplicationSiteFactory<T>._structuralObjectFilter;
			}
		}

		static ADReplicationSiteFactory()
		{
			ADReplicationSiteFactory<T>._structuralObjectFilter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "site");
			ADReplicationSiteFactory<T>._rDNPrefix = "CN";
			ADReplicationSiteFactory<T>._structuralObjectClass = "site";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADReplicationSiteFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADReplicationSiteFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADReplicationSiteFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[4];
			attributeConverterEntry[0] = new AttributeConverterEntry("Name", "name", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.Description.PropertyName, ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.Description.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.Subnet.PropertyName, ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.Subnet.ADAttribute, TypeConstants.ADReplicationSubnet, true, TypeAdapterAccess.Read, false, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.ManagedBy.PropertyName, ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.ManagedBy.ADAttribute, TypeConstants.ADPrincipal, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADReplicationSiteFactory<T>.ToDirectoryFromNameToManagedByDN), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationSiteFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationSiteFactory<T>.ADAMMappingTable = ADReplicationSiteFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSiteFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSiteFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADReplicationSiteFactory()
		{
			this._debugCategory = "ADReplicationSiteFactory";
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADReplicationSitePreCommitFSRoutine));
			base.PostCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADReplicationSitePostCommitFSRoutine));
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADReplicationSiteRemovePreCommitFSRoutine));
		}

		private bool ADReplicationSitePostCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (ADFactory<T>.DirectoryOperation.Update == operation || operation == ADFactory<T>.DirectoryOperation.Create)
			{
				try
				{
					this.CreateSiteChildObjects(operation, instance, parameters, directoryObj);
				}
				catch (Exception exception3)
				{
					Exception exception = exception3;
					if (operation == ADFactory<T>.DirectoryOperation.Create && (exception as ADException != null || exception as ADInvalidOperationException != null || exception as ADIdentityResolutionException != null || exception as UnauthorizedAccessException != null || exception as ArgumentException != null))
					{
						try
						{
							this.RemoveADReplicationSite(directoryObj);
						}
						catch (Exception exception2)
						{
							Exception exception1 = exception2;
							DebugLogger.LogWarning(this._debugCategory, string.Format("ADReplicationSiteFactory: Unable to delete the Site {0}. Deletion failed with error {1}.", directoryObj.DistinguishedName, exception1.Message));
						}
					}
					throw;
				}
				return false;
			}
			else
			{
				return false;
			}
		}

		private bool ADReplicationSitePreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (ADFactory<T>.DirectoryOperation.Update == operation || operation == ADFactory<T>.DirectoryOperation.Create)
			{
				MappingTable<AttributeConverterEntry> item = ADNtdsSiteSettingFactory<ADNtdsSiteSetting>.AttributeTable[base.ConnectedStore];
				MappingTable<AttributeConverterEntry> mappingTable = ADReplicationSiteFactory<T>.AttributeTable[base.ConnectedStore];
				ADTopologyUtil.RemoveChildObjectAttributes(directoryObj, mappingTable, item);
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool ADReplicationSiteRemovePreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (ADFactory<T>.DirectoryOperation.Delete == operation)
			{
				ADObjectFactory<ADObject> aDObjectFactory = new ADObjectFactory<ADObject>();
				aDObjectFactory.SetCmdletSessionInfo(base.CmdletSessionInfo);
				IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "server");
				int? nullable = null;
				int? nullable1 = null;
				IEnumerable<ADObject> extendedObjectFromFilter = aDObjectFactory.GetExtendedObjectFromFilter(aDOPathNode, directoryObj.DistinguishedName, ADSearchScope.Subtree, null, nullable, nullable1, false);
				IEnumerator<ADObject> enumerator = extendedObjectFromFilter.GetEnumerator();
				using (enumerator)
				{
					if (enumerator.MoveNext())
					{
						//TODO: Review: URGENT!! : enumerator.Current;
						object[] distinguishedName = new object[1];
						distinguishedName[0] = directoryObj.DistinguishedName;
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerContainerNotEmpty, distinguishedName));
					}
				}
				return false;
			}
			else
			{
				return false;
			}
		}

		internal override IADOPathNode BuildSearchFilter(IADOPathNode filter)
		{
			MappingTable<AttributeConverterEntry> item = ADNtdsSiteSettingFactory<ADNtdsSiteSetting>.AttributeTable[base.ConnectedStore];
			MappingTable<AttributeConverterEntry> mappingTable = ADReplicationSiteFactory<T>.AttributeTable[base.ConnectedStore];
			return ADTopologyUtil.BuildSearchFilter(filter, mappingTable, item, base.CmdletSessionInfo);
		}

		private void CreateServerContainer(string siteDN)
		{
			string str = ADPathModule.MakePath(siteDN, "CN=Servers,", ADPathFormat.X500);
			ADObject aDObject = new ADObject(str, "serversContainer");
			using (ADActiveObject aDActiveObject = new ADActiveObject(base.CmdletSessionInfo.ADSessionInfo, aDObject))
			{
				aDActiveObject.Create();
			}
		}

		private void CreateSiteChildObjects(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			bool flag = ADFactory<T>.DirectoryOperation.Create == operation;
			MappingTable<AttributeConverterEntry> item = ADNtdsSiteSettingFactory<ADNtdsSiteSetting>.AttributeTable[base.ConnectedStore];
			MappingTable<AttributeConverterEntry> mappingTable = ADReplicationSiteFactory<T>.AttributeTable[base.ConnectedStore];
			IDictionary<string, ADPropertyValueCollection> strs = new Dictionary<string, ADPropertyValueCollection>();
			if (instance != null)
			{
				foreach (string propertyName in instance.PropertyNames)
				{
					if (flag && instance[propertyName].Value == null || mappingTable.TryGetValue(propertyName, out attributeConverterEntry) || !item.TryGetValue(propertyName, out attributeConverterEntry))
					{
						continue;
					}
					strs.Add(propertyName, instance[propertyName]);
				}
			}
			IDictionary<string, ADPropertyValueCollection> aDPVCDictionary = parameters.GetADPVCDictionary();
			foreach (string key in aDPVCDictionary.Keys)
			{
				if (mappingTable.TryGetValue(key, out attributeConverterEntry) || !item.TryGetValue(key, out attributeConverterEntry))
				{
					continue;
				}
				if (!strs.ContainsKey(key))
				{
					strs.Add(key, aDPVCDictionary[key]);
				}
				else
				{
					strs[key] = aDPVCDictionary[key];
				}
			}
			string str = ADPathModule.MakePath(directoryObj.DistinguishedName, "CN=NTDS Site Settings,", ADPathFormat.X500);
			ADNtdsSiteSettingFactory<ADNtdsSiteSetting> aDNtdsSiteSettingFactory = new ADNtdsSiteSettingFactory<ADNtdsSiteSetting>();
			aDNtdsSiteSettingFactory.SetCmdletSessionInfo(base.CmdletSessionInfo);
			ADObject directoryObjectFromIdentity = null;
			if (!flag)
			{
				try
				{
					ADNtdsSiteSetting aDNtdsSiteSetting = new ADNtdsSiteSetting(str);
					directoryObjectFromIdentity = aDNtdsSiteSettingFactory.GetDirectoryObjectFromIdentity(aDNtdsSiteSetting, directoryObj.DistinguishedName);
				}
				catch (ADIdentityNotFoundException aDIdentityNotFoundException)
				{
					DebugLogger.LogInfo(this._debugCategory, string.Format("ADReplicationSiteFactory: Ntds-Site-Setting object not found for the site {0}, while updating the properties of the ntds-site-settings", directoryObj.DistinguishedName));
				}
			}
			if (directoryObjectFromIdentity == null)
			{
				flag = true;
				directoryObjectFromIdentity = new ADObject(str, aDNtdsSiteSettingFactory.StructuralObjectClass);
			}
			foreach (string key1 in strs.Keys)
			{
				if (!item.TryGetValue(key1, out attributeConverterEntry) || !attributeConverterEntry.IsDirectoryConverterDefined)
				{
					continue;
				}
				attributeConverterEntry.InvokeToDirectoryConverter(strs[key1], directoryObjectFromIdentity, base.CmdletSessionInfo);
			}
			using (ADActiveObject aDActiveObject = new ADActiveObject(base.CmdletSessionInfo.ADSessionInfo, directoryObjectFromIdentity))
			{
				if (!flag)
				{
					aDActiveObject.Update();
				}
				else
				{
					aDActiveObject.Create();
				}
			}
			if (operation == ADFactory<T>.DirectoryOperation.Create)
			{
				this.CreateServerContainer(directoryObj.DistinguishedName);
			}
		}

		internal override T GetExtendedObjectFromDN(string distinguishedName, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			T t = Activator.CreateInstance<T>();
			t.Identity = distinguishedName;
			return base.GetExtendedObjectFromIdentity(t, distinguishedName, propertiesToFetch);
		}

		internal override IEnumerable<T> GetExtendedObjectFromFilter(IADOPathNode filter, string searchBase, ADSearchScope searchScope, ICollection<string> propertiesToFetch, int? resultSetSize, int? pageSize, bool showDeleted)
		{
			if (base.CmdletSessionInfo != null)
			{
				MappingTable<AttributeConverterEntry> item = ADNtdsSiteSettingFactory<ADNtdsSiteSetting>.AttributeTable[base.ConnectedStore];
				MappingTable<AttributeConverterEntry> mappingTable = ADReplicationSiteFactory<T>.AttributeTable[base.ConnectedStore];
				ICollection<string> parentAttributes = ADTopologyUtil.GetParentAttributes(mappingTable, item, propertiesToFetch);
				ICollection<string> childAttributes = ADTopologyUtil.GetChildAttributes(mappingTable, item, propertiesToFetch);
				if (!parentAttributes.Contains("*"))
				{
					parentAttributes.Add("*");
				}
				IEnumerable<T> extendedObjectFromFilter = base.GetExtendedObjectFromFilter(this.StructuralObjectFilter, searchBase, searchScope, parentAttributes, resultSetSize, pageSize, showDeleted);
				ADNtdsSiteSettingFactory<ADNtdsSiteSetting> aDNtdsSiteSettingFactory = new ADNtdsSiteSettingFactory<ADNtdsSiteSetting>();
				aDNtdsSiteSettingFactory.SetCmdletSessionInfo(base.CmdletSessionInfo);
				IEnumerable<ADNtdsSiteSetting> aDNtdsSiteSettings = aDNtdsSiteSettingFactory.GetExtendedObjectFromFilter(aDNtdsSiteSettingFactory.StructuralObjectFilter, searchBase, searchScope, childAttributes, resultSetSize, pageSize, showDeleted);
				Dictionary<string, ADNtdsSiteSetting> strs = new Dictionary<string, ADNtdsSiteSetting>();
				foreach (ADNtdsSiteSetting aDNtdsSiteSetting in aDNtdsSiteSettings)
				{
					strs.Add(aDNtdsSiteSetting.DistinguishedName, aDNtdsSiteSetting);
				}
				List<T> ts = new List<T>();
				foreach (T t in extendedObjectFromFilter)
				{
					string str = ADPathModule.MakePath(t.DistinguishedName, "CN=NTDS Site Settings,", ADPathFormat.X500);
					if (strs.ContainsKey(str))
					{
						ADNtdsSiteSetting item1 = strs[str];
						ADTopologyUtil.MergeADObjectProperties(t, item1);
					}
					ts.Add(t);
				}
				IEnumerable<T> ts1 = this.ApplyClientSideFilter(ts);
				return ADTopologyUtil.RemoveExtraPropertiesFromADAggregateObject<T>(mappingTable, item, propertiesToFetch, ts1);
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal override T GetExtendedObjectFromIdentity(T identityObj, string identityQueryPath, ICollection<string> propertiesToFetch, bool showDeleted)
		{
			if (base.CmdletSessionInfo != null)
			{
				MappingTable<AttributeConverterEntry> item = ADNtdsSiteSettingFactory<ADNtdsSiteSetting>.AttributeTable[base.ConnectedStore];
				MappingTable<AttributeConverterEntry> mappingTable = ADReplicationSiteFactory<T>.AttributeTable[base.ConnectedStore];
				ICollection<string> parentAttributes = ADTopologyUtil.GetParentAttributes(mappingTable, item, propertiesToFetch);
				ICollection<string> childAttributes = ADTopologyUtil.GetChildAttributes(mappingTable, item, propertiesToFetch);
				T extendedObjectFromIdentity = base.GetExtendedObjectFromIdentity(identityObj, identityQueryPath, parentAttributes, showDeleted);
				string str = ADPathModule.MakePath(extendedObjectFromIdentity.DistinguishedName, "CN=NTDS Site Settings,", ADPathFormat.X500);
				try
				{
					ADTopologyUtil.ConstructAggregateObject<ADNtdsSiteSettingFactory<ADNtdsSiteSetting>, ADNtdsSiteSetting>(str, extendedObjectFromIdentity, identityQueryPath, childAttributes, showDeleted, base.CmdletSessionInfo);
				}
				catch (ADIdentityNotFoundException aDIdentityNotFoundException)
				{
					DebugLogger.LogInfo(this._debugCategory, string.Format("ADReplicationSiteFactory: Ntds-Site-Setting object not found for the site {0}, while constructing ADReplicationSite", extendedObjectFromIdentity.DistinguishedName));
				}
				return extendedObjectFromIdentity;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		private void RemoveADReplicationSite(ADObject directoryObj)
		{
			using (ADActiveObject aDActiveObject = new ADActiveObject(base.CmdletSessionInfo.ADSessionInfo, directoryObj))
			{
				aDActiveObject.DeleteTree(false);
			}
		}

		internal static void ToDirectoryFromNameToManagedByDN(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToDirectoryFromADEntityToAttributeValue<ADPrincipalFactory<ADPrincipal>, ADPrincipal>(cmdletSessionInfo.ADRootDSE.DefaultNamingContext, null, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static class ADReplicationSitePropertyMap
		{
			public readonly static PropertyMapEntry Description;

			public readonly static PropertyMapEntry Subnet;

			public readonly static PropertyMapEntry ManagedBy;

			static ADReplicationSitePropertyMap()
			{
				ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.Description = new PropertyMapEntry("Description", "description", "description");
				ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.Subnet = new PropertyMapEntry("Subnets", "siteObjectBL", "siteObjectBL");
				ADReplicationSiteFactory<T>.ADReplicationSitePropertyMap.ManagedBy = new PropertyMapEntry("ManagedBy", "managedBy", "managedBy");
			}
		}
	}
}