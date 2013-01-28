using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationConnectionFactory<T> : ADObjectFactory<T>
	where T : ADReplicationConnection, new()
	{
		private readonly static string _rDNPrefix;

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
				return ADReplicationConnectionFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADReplicationConnectionFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "nTDSConnection";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADReplicationConnectionFactory()
		{
			ADReplicationConnectionFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADReplicationConnectionFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADReplicationConnectionFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADReplicationConnectionFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[6];
			attributeConverterEntry[0] = new AttributeConverterEntry("Name", "name", TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.InterSiteTransportProtocol.PropertyName, ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.InterSiteTransportProtocol.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationConnectionFactory<T>.ToExtendedFromTransportTypeToISTPEnum), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicateFromDirectoryServer.PropertyName, ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicateFromDirectoryServer.ADAttribute, TypeConstants.ADDirectoryServer, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADTopologyUtil.ToDirectoryFromServerNameToNTDSSettings), new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicateToDirectoryServer.PropertyName, ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicateToDirectoryServer.ADAttribute, TypeConstants.ADDirectoryServer, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADReplicationConnectionFactory<T>.ToExtendedFromDNToDirectoryServerDN), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicationSchedule.PropertyName, ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicationSchedule.ADAttribute, TypeConstants.ByteArray, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedADReplicationScheduleFromBlob), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryBlobFromADReplicationSchedule), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.AutoGenerated.PropertyName, ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.AutoGenerated.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(1, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADReplicationConnectionFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationConnectionFactory<T>.ADAMMappingTable = ADReplicationConnectionFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationConnectionFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationConnectionFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADReplicationConnectionFactory()
		{
			this._debugCategory = "ADReplicationConnectionFactory";
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADReplicationConnectionPreCommitFSRoutine));
		}

		private bool ADReplicationConnectionPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (ADFactory<T>.DirectoryOperation.Update == operation)
			{
				MappingTable<AttributeConverterEntry> item = ADNtdsSettingFactory<ADNtdsSetting>.AttributeTable[base.ConnectedStore];
				MappingTable<AttributeConverterEntry> mappingTable = ADReplicationConnectionFactory<T>.AttributeTable[base.ConnectedStore];
				ADTopologyUtil.RemoveChildObjectAttributes(directoryObj, mappingTable, item);
				return true;
			}
			else
			{
				return false;
			}
		}

		internal override IADOPathNode BuildSearchFilter(IADOPathNode filter)
		{
			MappingTable<AttributeConverterEntry> item = ADNtdsSettingFactory<ADNtdsSetting>.AttributeTable[base.ConnectedStore];
			MappingTable<AttributeConverterEntry> mappingTable = ADReplicationConnectionFactory<T>.AttributeTable[base.ConnectedStore];
			return ADTopologyUtil.BuildSearchFilter(filter, mappingTable, item, base.CmdletSessionInfo);
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
				MappingTable<AttributeConverterEntry> item = ADNtdsSettingFactory<ADNtdsSetting>.AttributeTable[base.ConnectedStore];
				MappingTable<AttributeConverterEntry> mappingTable = ADReplicationConnectionFactory<T>.AttributeTable[base.ConnectedStore];
				ICollection<string> parentAttributes = ADTopologyUtil.GetParentAttributes(mappingTable, item, propertiesToFetch);
				ICollection<string> childAttributes = ADTopologyUtil.GetChildAttributes(mappingTable, item, propertiesToFetch);
				if (!parentAttributes.Contains("*"))
				{
					parentAttributes.Add("*");
				}
				IEnumerable<T> extendedObjectFromFilter = base.GetExtendedObjectFromFilter(this.StructuralObjectFilter, searchBase, searchScope, parentAttributes, resultSetSize, pageSize, showDeleted);
				ADNtdsSettingFactory<ADNtdsSetting> aDNtdsSettingFactory = new ADNtdsSettingFactory<ADNtdsSetting>();
				aDNtdsSettingFactory.SetCmdletSessionInfo(base.CmdletSessionInfo);
				IEnumerable<ADNtdsSetting> aDNtdsSettings = aDNtdsSettingFactory.GetExtendedObjectFromFilter(aDNtdsSettingFactory.StructuralObjectFilter, searchBase, searchScope, childAttributes, resultSetSize, pageSize, showDeleted);
				Dictionary<string, ADNtdsSetting> strs = new Dictionary<string, ADNtdsSetting>();
				foreach (ADNtdsSetting aDNtdsSetting in aDNtdsSettings)
				{
					strs.Add(aDNtdsSetting.DistinguishedName, aDNtdsSetting);
				}
				List<T> ts = new List<T>();
				foreach (T t in extendedObjectFromFilter)
				{
					string parentPath = ADPathModule.GetParentPath(t.DistinguishedName, null, ADPathFormat.X500);
					if (strs.ContainsKey(parentPath))
					{
						ADNtdsSetting item1 = strs[parentPath];
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
				MappingTable<AttributeConverterEntry> item = ADNtdsSettingFactory<ADNtdsSetting>.AttributeTable[base.ConnectedStore];
				MappingTable<AttributeConverterEntry> mappingTable = ADReplicationConnectionFactory<T>.AttributeTable[base.ConnectedStore];
				ICollection<string> parentAttributes = ADTopologyUtil.GetParentAttributes(mappingTable, item, propertiesToFetch);
				ICollection<string> childAttributes = ADTopologyUtil.GetChildAttributes(mappingTable, item, propertiesToFetch);
				T extendedObjectFromIdentity = base.GetExtendedObjectFromIdentity(identityObj, identityQueryPath, parentAttributes, showDeleted);
				string parentPath = ADPathModule.GetParentPath(extendedObjectFromIdentity.DistinguishedName, null, ADPathFormat.X500);
				try
				{
					ADTopologyUtil.ConstructAggregateObject<ADNtdsSettingFactory<ADNtdsSetting>, ADNtdsSetting>(parentPath, extendedObjectFromIdentity, identityQueryPath, childAttributes, showDeleted, base.CmdletSessionInfo);
				}
				catch (ADIdentityNotFoundException aDIdentityNotFoundException)
				{
					DebugLogger.LogInfo(this._debugCategory, string.Format("ADReplicationConnectionFactory: Ntds-Setting object not found for the connection {0}, while constructing ADReplicationConnection", extendedObjectFromIdentity.DistinguishedName));
				}
				return extendedObjectFromIdentity;
			}
			else
			{
				throw new ArgumentNullException(StringResources.SessionRequired);
			}
		}

		internal static void ToExtendedFromDNToDirectoryServerDN(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = (string)directoryObj[directoryAttributes[0]].Value;
			value = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
			value = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(value);
			userObj.Add(extendedAttribute, aDPropertyValueCollection);
		}

		internal static void ToExtendedFromTransportTypeToISTPEnum(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]) || directoryObj[directoryAttributes[0]].Value == null)
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
				return;
			}
			else
			{
				string value = (string)directoryObj[directoryAttributes[0]].Value;
				string childName = ADPathModule.GetChildName(value, ADPathFormat.X500);
				ADInterSiteTransportProtocolType aDInterSiteTransportProtocolType = ADInterSiteTransportProtocolType.IP;
				if (string.Compare("CN=IP", childName, StringComparison.OrdinalIgnoreCase) != 0)
				{
					aDInterSiteTransportProtocolType = ADInterSiteTransportProtocolType.SMTP;
				}
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection((object)aDInterSiteTransportProtocolType);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
		}

		internal static class ADReplicationConnectionPropertyMap
		{
			public readonly static PropertyMapEntry InterSiteTransportProtocol;

			public readonly static PropertyMapEntry ReplicateFromDirectoryServer;

			public readonly static PropertyMapEntry ReplicateToDirectoryServer;

			public readonly static PropertyMapEntry ReplicationSchedule;

			public readonly static PropertyMapEntry AutoGenerated;

			static ADReplicationConnectionPropertyMap()
			{
				ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.InterSiteTransportProtocol = new PropertyMapEntry("InterSiteTransportProtocol", "transportType", "transportType");
				ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicateFromDirectoryServer = new PropertyMapEntry("ReplicateFromDirectoryServer", "fromServer", "fromServer");
				ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicateToDirectoryServer = new PropertyMapEntry("ReplicateToDirectoryServer", "distinguishedName", "distinguishedName");
				ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.ReplicationSchedule = new PropertyMapEntry("ReplicationSchedule", "schedule", "schedule");
				ADReplicationConnectionFactory<T>.ADReplicationConnectionPropertyMap.AutoGenerated = new PropertyMapEntry("AutoGenerated", "Options", "Options");
			}
		}
	}
}