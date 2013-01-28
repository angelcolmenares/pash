using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADTrustFactory<T> : ADObjectFactory<T>
	where T : ADTrust, new()
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
				return ADTrustFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADTrustFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "trustedDomain";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADTrustFactory()
		{
			ADTrustFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADTrustFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADTrustFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADTrustFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[19];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.Source.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.Source.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADTrustFactory<T>.ToExtendedSourceNameFromDN), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.Target.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.Target.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.TrustDirection.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.TrustDirection.ADAttribute, TypeConstants.ADTrustDirection, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObjectWithCast<ADTrustDirection>), null, new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADTrustDirection>));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.TrustType.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.TrustType.ADAttribute, TypeConstants.ADTrustType, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObjectWithCast<ADTrustType>), null, new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADTrustType>));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.TrustAttributes.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.TrustAttributes.ADAttribute, TypeConstants.Int, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.TrustingPolicy.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.TrustingPolicy.ADAttribute, TypeConstants.ADClaimTransformPolicy, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(ADTrustFactory<T>.ToSearchFromClaimTransformPolicyToDN));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.TrustedPolicy.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.TrustedPolicy.ADAttribute, TypeConstants.ADClaimTransformPolicy, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(ADTrustFactory<T>.ToSearchFromClaimTransformPolicyToDN));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.DisallowTransivity.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.DisallowTransivity.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(1, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(1, false).Invoke));
			attributeConverterEntry[8] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.UplevelOnly.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.UplevelOnly.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(2, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(2, false).Invoke));
			attributeConverterEntry[9] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.SIDFilteringQuarantined.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.SIDFilteringQuarantined.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(4, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(4, false).Invoke));
			attributeConverterEntry[10] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.ForestTransitive.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.ForestTransitive.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(8, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(8, false).Invoke));
			attributeConverterEntry[11] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.SelectiveAuthentication.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.SelectiveAuthentication.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(16, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(16, false).Invoke));
			attributeConverterEntry[12] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.IntraForest.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.IntraForest.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(32, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(32, false).Invoke));
			attributeConverterEntry[13] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.SIDFilteringForestAware.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.SIDFilteringForestAware.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(64, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(64, false).Invoke));
			attributeConverterEntry[14] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.UsesRC4Encryption.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.UsesRC4Encryption.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(128, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(128, false).Invoke));
			attributeConverterEntry[15] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.UsesAESKeys.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.UsesAESKeys.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x100, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(0x100, false).Invoke));
			attributeConverterEntry[16] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.IsTreeParent.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.IsTreeParent.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x400000, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(0x400000, false).Invoke));
			attributeConverterEntry[17] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.IsTreeRoot.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.IsTreeRoot.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x800000, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(0x800000, false).Invoke));
			attributeConverterEntry[18] = new AttributeConverterEntry(ADTrustFactory<T>.ADTrustPropertyMap.TGTDelegation.PropertyName, ADTrustFactory<T>.ADTrustPropertyMap.TGTDelegation.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.GetDelegateToExtendedFlagFromInt(0x200, false).Invoke), null, new ToSearchFilterDelegate(SearchConverters.GetDelegateToSearchFlagInInt(0x200, false).Invoke));
			ADTrustFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADTrustFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADTrustFactory()
		{
		}

		internal static void ToExtendedSourceNameFromDN(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string value = (string)directoryObj[directoryAttributes[0]].Value;
			value = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
			value = ADPathModule.GetParentPath(value, null, ADPathFormat.X500);
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(value);
			userObj.Add(extendedAttribute, aDPropertyValueCollection);
		}

		internal static IADOPathNode ToSearchFromClaimTransformPolicyToDN(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Transformation Policies,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return SearchConverters.ToSearchFromADEntityToAttributeValue<ADClaimTransformPolicyFactory<ADClaimTransformPolicy>, ADClaimTransformPolicy>(str, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static class ADTrustPropertyMap
		{
			public readonly static PropertyMapEntry Source;

			public readonly static PropertyMapEntry Target;

			public readonly static PropertyMapEntry TrustDirection;

			public readonly static PropertyMapEntry TrustType;

			public readonly static PropertyMapEntry TrustAttributes;

			public readonly static PropertyMapEntry TrustingPolicy;

			public readonly static PropertyMapEntry TrustedPolicy;

			public readonly static PropertyMapEntry DisallowTransivity;

			public readonly static PropertyMapEntry UplevelOnly;

			public readonly static PropertyMapEntry SIDFilteringQuarantined;

			public readonly static PropertyMapEntry ForestTransitive;

			public readonly static PropertyMapEntry SelectiveAuthentication;

			public readonly static PropertyMapEntry IntraForest;

			public readonly static PropertyMapEntry SIDFilteringForestAware;

			public readonly static PropertyMapEntry UsesRC4Encryption;

			public readonly static PropertyMapEntry UsesAESKeys;

			public readonly static PropertyMapEntry IsTreeParent;

			public readonly static PropertyMapEntry IsTreeRoot;

			public readonly static PropertyMapEntry TGTDelegation;

			static ADTrustPropertyMap()
			{
				ADTrustFactory<T>.ADTrustPropertyMap.Source = new PropertyMapEntry("Source", "distinguishedName", "distinguishedName");
				ADTrustFactory<T>.ADTrustPropertyMap.Target = new PropertyMapEntry("Target", "trustPartner", "trustPartner");
				ADTrustFactory<T>.ADTrustPropertyMap.TrustDirection = new PropertyMapEntry("Direction", "trustDirection", "trustDirection");
				ADTrustFactory<T>.ADTrustPropertyMap.TrustType = new PropertyMapEntry("TrustType", "trustType", "trustType");
				ADTrustFactory<T>.ADTrustPropertyMap.TrustAttributes = new PropertyMapEntry("TrustAttributes", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.TrustingPolicy = new PropertyMapEntry("TrustingPolicy", "msDS-IngressClaimsTransformationPolicy", "msDS-IngressClaimsTransformationPolicy");
				ADTrustFactory<T>.ADTrustPropertyMap.TrustedPolicy = new PropertyMapEntry("TrustedPolicy", "msDS-EgressClaimsTransformationPolicy", "msDS-EgressClaimsTransformationPolicy");
				ADTrustFactory<T>.ADTrustPropertyMap.DisallowTransivity = new PropertyMapEntry("DisallowTransivity", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.UplevelOnly = new PropertyMapEntry("UplevelOnly", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.SIDFilteringQuarantined = new PropertyMapEntry("SIDFilteringQuarantined", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.ForestTransitive = new PropertyMapEntry("ForestTransitive", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.SelectiveAuthentication = new PropertyMapEntry("SelectiveAuthentication", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.IntraForest = new PropertyMapEntry("IntraForest", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.SIDFilteringForestAware = new PropertyMapEntry("SIDFilteringForestAware", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.UsesRC4Encryption = new PropertyMapEntry("UsesRC4Encryption", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.UsesAESKeys = new PropertyMapEntry("UsesAESKeys", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.IsTreeParent = new PropertyMapEntry("IsTreeParent", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.IsTreeRoot = new PropertyMapEntry("IsTreeRoot", "trustAttributes", "trustAttributes");
				ADTrustFactory<T>.ADTrustPropertyMap.TGTDelegation = new PropertyMapEntry("TGTDelegation", "trustAttributes", "trustAttributes");
			}
		}
	}
}