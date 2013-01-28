using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADResourcePropertyListFactory<T> : ADObjectFactory<T>
	where T : ADResourcePropertyList, new()
	{
		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

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
				return ADResourcePropertyListFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADResourcePropertyListFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-ResourcePropertyList";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADResourcePropertyListFactory()
		{
			ADResourcePropertyListFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADResourcePropertyListFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADResourcePropertyListFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADResourcePropertyListFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[1];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADResourcePropertyListFactory<T>.ADResourcePropertyListPropertyMap.Members.PropertyName, ADResourcePropertyListFactory<T>.ADResourcePropertyListPropertyMap.Members.ADAttribute, TypeConstants.ADResourceProperty, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(ADCBACUtil.ToSearchFromResourcePropertyNameToDN));
			ADResourcePropertyListFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADResourcePropertyListFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADResourcePropertyListFactory()
		{
		}

		internal static class ADResourcePropertyListPropertyMap
		{
			public readonly static PropertyMapEntry Members;

			static ADResourcePropertyListPropertyMap()
			{
				ADResourcePropertyListFactory<T>.ADResourcePropertyListPropertyMap.Members = new PropertyMapEntry("Members", "msDS-MembersOfResourcePropertyList", "msDS-MembersOfResourcePropertyList");
			}
		}
	}
}