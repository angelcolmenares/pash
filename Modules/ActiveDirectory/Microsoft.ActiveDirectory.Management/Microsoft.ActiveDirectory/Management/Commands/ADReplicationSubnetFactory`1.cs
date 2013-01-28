using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADReplicationSubnetFactory<T> : ADObjectFactory<T>
	where T : ADReplicationSubnet, new()
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
				return ADReplicationSubnetFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADReplicationSubnetFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "subnet";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADReplicationSubnetFactory()
		{
			ADReplicationSubnetFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADReplicationSubnetFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADReplicationSubnetFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADReplicationSubnetFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[2];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADReplicationSubnetFactory<T>.ADReplicationSubnetPropertyMap.Location.PropertyName, ADReplicationSubnetFactory<T>.ADReplicationSubnetPropertyMap.Location.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADReplicationSubnetFactory<T>.ADReplicationSubnetPropertyMap.Site.PropertyName, ADReplicationSubnetFactory<T>.ADReplicationSubnetPropertyMap.Site.ADAttribute, TypeConstants.ADReplicationSite, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADTopologyUtil.ToDirectoryFromSiteNameToDN), new ToSearchFilterDelegate(ADTopologyUtil.ToSearchFromTopologyObjectNameToDN<ADReplicationSiteFactory<ADReplicationSite>, ADReplicationSite>));
			ADReplicationSubnetFactory<T>.ADMappingTable = attributeConverterEntry;
			ADReplicationSubnetFactory<T>.ADAMMappingTable = ADReplicationSubnetFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSubnetFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADReplicationSubnetFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADReplicationSubnetFactory()
		{
		}

		internal static class ADReplicationSubnetPropertyMap
		{
			public readonly static PropertyMapEntry Location;

			public readonly static PropertyMapEntry Site;

			static ADReplicationSubnetPropertyMap()
			{
				ADReplicationSubnetFactory<T>.ADReplicationSubnetPropertyMap.Location = new PropertyMapEntry("Location", "location", "location");
				ADReplicationSubnetFactory<T>.ADReplicationSubnetPropertyMap.Site = new PropertyMapEntry("Site", "siteObject", "siteObject");
			}
		}
	}
}