using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADNtdsSettingFactory<T> : ADObjectFactory<T>
	where T : ADNtdsSetting, new()
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
				return ADNtdsSettingFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADNtdsSettingFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "nTDSDSA";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADNtdsSettingFactory()
		{
			ADNtdsSettingFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADNtdsSettingFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADNtdsSettingFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADNtdsSettingFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[2];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.ReplicatedNamingContexts.PropertyName, ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.ReplicatedNamingContexts.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueStringFromMultiAttribute), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.PartiallyReplicatedNamingContexts.PropertyName, ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.PartiallyReplicatedNamingContexts.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchObjectClientSideFilter));
			ADNtdsSettingFactory<T>.ADMappingTable = attributeConverterEntry;
			ADNtdsSettingFactory<T>.ADAMMappingTable = ADNtdsSettingFactory<T>.ADMappingTable;
			ADFactoryBase<T>.RegisterMappingTable(ADNtdsSettingFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADNtdsSettingFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADNtdsSettingFactory()
		{
		}

		internal override AttributeSetRequest ConstructAttributeSetRequest(ICollection<string> requestedExtendedAttr)
		{
			AttributeSetRequest attributeSetRequest = base.ConstructAttributeSetRequest(requestedExtendedAttr);
			ADSchema aDSchema = new ADSchema(base.CmdletSessionInfo.ADSessionInfo);
			if (!aDSchema.SchemaProperties.ContainsKey("msDS-hasFullReplicaNCs"))
			{
				attributeSetRequest.DirectoryAttributes.Remove("msDS-hasFullReplicaNCs");
			}
			return attributeSetRequest;
		}

		internal static class ADNtdsSettingPropertyMap
		{
			private readonly static string[] ReplicatedNamingContextsAttributes;

			public readonly static PropertyMapEntry ReplicatedNamingContexts;

			public readonly static PropertyMapEntry PartiallyReplicatedNamingContexts;

			static ADNtdsSettingPropertyMap()
			{
				string[] strArrays = new string[2];
				strArrays[0] = "msDS-hasMasterNCs";
				strArrays[1] = "msDS-hasFullReplicaNCs";
				ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.ReplicatedNamingContextsAttributes = strArrays;
				ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.ReplicatedNamingContexts = new PropertyMapEntry("ReplicatedNamingContexts", ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.ReplicatedNamingContextsAttributes, ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.ReplicatedNamingContextsAttributes);
				ADNtdsSettingFactory<T>.ADNtdsSettingPropertyMap.PartiallyReplicatedNamingContexts = new PropertyMapEntry("PartiallyReplicatedNamingContexts", "hasPartialReplicaNCs", "hasPartialReplicaNCs");
			}
		}
	}
}