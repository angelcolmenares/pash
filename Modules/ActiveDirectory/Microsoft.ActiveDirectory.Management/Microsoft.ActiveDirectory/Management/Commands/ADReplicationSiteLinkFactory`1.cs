using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationSiteLinkFactory<T> : ADObjectFactory<T>
	where T : ADReplicationSiteLink, new()
	{
		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

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
				return ADReplicationSiteLinkFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADReplicationSiteLinkFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "siteLink";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADReplicationSiteLinkFactory()
		{
			ADReplicationSiteLinkFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADReplicationSiteLinkFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADReplicationSiteLinkFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADReplicationSiteLinkFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[5];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationSchedule.PropertyName, ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationSchedule.ADAttribute, TypeConstants.ByteArray, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedADReplicationScheduleFromBlob), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryBlobFromADReplicationSchedule), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationCost.PropertyName, ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationCost.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationFrequencyInMinutes.PropertyName, ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationFrequencyInMinutes.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.InterSiteTransportProtocol.PropertyName, ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.InterSiteTransportProtocol.ADAttribute, typeof(ADInterSiteTransportProtocolType), true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Extended, new ToExtendedFormatDelegate(ADTopologyUtil.ToExtendedFromDNToISTPEnum), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.SitesIncluded.PropertyName, ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.SitesIncluded.ADAttribute, TypeConstants.ADReplicationSite, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(ADTopologyUtil.ToDirectoryFromTopologyObjectNameListToDNList<ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>), new ToSearchFilterDelegate(ADTopologyUtil.ToSearchFromTopologyObjectNameToDN<ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>));
			ADReplicationSiteLinkFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationSiteLinkFactory<T>.ADAMMappingTable = ADReplicationSiteLinkFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSiteLinkFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSiteLinkFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADReplicationSiteLinkFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADReplicationSiteLinkPreCommitFSRoutine));
		}

		private bool ADReplicationSiteLinkPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (operation == ADFactory<T>.DirectoryOperation.Create)
			{
				if (!directoryObj.Contains("siteList") || directoryObj["siteList"].Count <= 1)
				{
					object[] objArray = new object[4];
					objArray[0] = "SiteLink";
					objArray[1] = 2;
					objArray[2] = "Sites";
					objArray[3] = "SitesIncluded";
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ADInvalidAttributeValueCount, objArray));
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		internal static class ADReplicationSiteLinkPropertyMap
		{
			public readonly static PropertyMapEntry ReplicationSchedule;

			public readonly static PropertyMapEntry ReplicationFrequencyInMinutes;

			public readonly static PropertyMapEntry ReplicationCost;

			public readonly static PropertyMapEntry SitesIncluded;

			public readonly static PropertyMapEntry InterSiteTransportProtocol;

			static ADReplicationSiteLinkPropertyMap()
			{
				ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationSchedule = new PropertyMapEntry("ReplicationSchedule", "schedule", "schedule");
				ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationFrequencyInMinutes = new PropertyMapEntry("ReplicationFrequencyInMinutes", "replInterval", "replInterval");
				ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.ReplicationCost = new PropertyMapEntry("Cost", "cost", "cost");
				ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.SitesIncluded = new PropertyMapEntry("SitesIncluded", "siteList", "siteList");
				ADReplicationSiteLinkFactory<T>.ADReplicationSiteLinkPropertyMap.InterSiteTransportProtocol = new PropertyMapEntry("InterSiteTransportProtocol", "distinguishedName", "distinguishedName");
			}
		}
	}
}