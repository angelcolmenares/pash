using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADResourcePropertyValueTypeFactory<T> : ADObjectFactory<T>
	where T : ADResourcePropertyValueType, new()
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
				return ADResourcePropertyValueTypeFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADResourcePropertyValueTypeFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-ValueType";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADResourcePropertyValueTypeFactory()
		{
			ADResourcePropertyValueTypeFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADResourcePropertyValueTypeFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADResourcePropertyValueTypeFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADResourcePropertyValueTypeFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[5];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.RestrictValues.PropertyName, ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.RestrictValues.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.ValueType.PropertyName, ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.ValueType.ADAttribute, TypeConstants.ADClaimValueType, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADClaimTypeBaseFactory<ADClaimTypeBase>.ToExtendedFromIntToValueTypeEnum), null, new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADClaimValueType>));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.IsSingleValued.PropertyName, ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.IsSingleValued.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.IsSuggestedValuesPresent.PropertyName, ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.IsSuggestedValuesPresent.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.ResourceProperties.PropertyName, ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.ResourceProperties.ADAttribute, TypeConstants.ADResourceProperty, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(ADCBACUtil.ToSearchFromResourcePropertyNameToDN));
			ADResourcePropertyValueTypeFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADResourcePropertyValueTypeFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADResourcePropertyValueTypeFactory()
		{
		}

		internal static class ADResourcePropertyValueTypePropertyMap
		{
			public readonly static PropertyMapEntry RestrictValues;

			public readonly static PropertyMapEntry ValueType;

			public readonly static PropertyMapEntry IsSingleValued;

			public readonly static PropertyMapEntry IsSuggestedValuesPresent;

			public readonly static PropertyMapEntry ResourceProperties;

			static ADResourcePropertyValueTypePropertyMap()
			{
				ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.RestrictValues = new PropertyMapEntry("RestrictValues", "msDS-ClaimIsValueSpaceRestricted", "msDS-ClaimIsValueSpaceRestricted");
				ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.ValueType = new PropertyMapEntry("ValueType", "msDS-ClaimValueType", "msDS-ClaimValueType");
				ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.IsSingleValued = new PropertyMapEntry("IsSingleValued", "msDS-ClaimIsSingleValued", "msDS-ClaimIsSingleValued");
				ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.IsSuggestedValuesPresent = new PropertyMapEntry("IsSuggestedValuesPresent", "msDS-IsPossibleValuesPresent", "msDS-IsPossibleValuesPresent");
				ADResourcePropertyValueTypeFactory<T>.ADResourcePropertyValueTypePropertyMap.ResourceProperties = new PropertyMapEntry("ResourceProperties", "msDS-ValueTypeReferenceBL", "msDS-ValueTypeReferenceBL");
			}
		}
	}
}