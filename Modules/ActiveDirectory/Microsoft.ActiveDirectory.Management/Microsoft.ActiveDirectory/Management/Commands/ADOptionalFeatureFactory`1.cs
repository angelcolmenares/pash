using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADOptionalFeatureFactory<T> : ADObjectFactory<T>
	where T : ADOptionalFeature, new()
	{
		private static IADOPathNode _ofStructuralFilter;

		private static string[] _ofIdentityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		private static AttributeConverterEntry[] ADAMMappingTable;

		internal override IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADOptionalFeatureFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return "CN";
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-OptionalFeature";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOptionalFeatureFactory<T>._ofStructuralFilter;
			}
		}

		static ADOptionalFeatureFactory()
		{
			ADOptionalFeatureFactory<T>._ofStructuralFilter = null;
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADOptionalFeatureFactory<T>._ofIdentityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[3];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			customIdentityResolver[1] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(ADOptionalFeatureFactory<T>.ADOFGuidIdentityResolver));
			customIdentityResolver[2] = IdentityResolverMethods.GetGenericIdentityResolver(ADOptionalFeatureFactory<T>._ofIdentityLdapAttributes);
			ADOptionalFeatureFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[6];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.EnabledScopes.PropertyName, ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.EnabledScopes.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.FeatureScope.PropertyName, ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.FeatureScope.ADAttribute, TypeConstants.Int, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(ADOptionalFeatureFactory<T>.ToExtendedFeatureScope), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.IsDisableable.PropertyName, ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.IsDisableable.ADAttribute, TypeConstants.Bool, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADOptionalFeatureFactory<T>.ToExtendedIsDisableable), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.FeatureGUID.PropertyName, ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.FeatureGUID.ADAttribute, TypeConstants.Guid, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedGuid), null, new ToSearchFilterDelegate(SearchConverters.ToSearchGuid));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.RequiredDomainMode.PropertyName, ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.RequiredDomainMode.ADAttribute, TypeConstants.ADDomainMode, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObjectWithCast<ADDomainMode>), null, new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADDomainMode>));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.RequiredForestMode.PropertyName, ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.RequiredForestMode.ADAttribute, TypeConstants.ADForestMode, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObjectWithCast<ADForestMode>), null, new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADForestMode>));
			ADOptionalFeatureFactory<T>.ADMappingTable = attributeConverterEntry;
			ADOptionalFeatureFactory<T>.ADAMMappingTable = ADOptionalFeatureFactory<T>.ADMappingTable;
			ADOptionalFeatureFactory<T>._ofStructuralFilter = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", "msDS-OptionalFeature");
			ADFactoryBase<T>.RegisterMappingTable(ADOptionalFeatureFactory<T>.ADMappingTable, ADServerType.ADDS);
			ADFactoryBase<T>.RegisterMappingTable(ADOptionalFeatureFactory<T>.ADAMMappingTable, ADServerType.ADLDS);
		}

		public ADOptionalFeatureFactory()
		{
		}

		internal static ADObjectSearcher ADOFGuidIdentityResolver(object identityObject, string searchBase, CmdletSessionInfo cmdletSessionInfo, out bool useSearchFilter)
		{
			useSearchFilter = true;
			if (identityObject != null)
			{
				SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchBase, ADSearchScope.Subtree);
				Guid? nullable = null;
				string str = identityObject as string;
				if (string.IsNullOrEmpty(str) || !Utils.TryParseGuid(str, out nullable))
				{
					if (!(identityObject is Guid))
					{
						ADOptionalFeature aDOptionalFeature = identityObject as ADOptionalFeature;
						if (aDOptionalFeature != null)
						{
							Guid? objectGuid = aDOptionalFeature.ObjectGuid;
							if (!objectGuid.HasValue)
							{
								Guid? featureGUID = aDOptionalFeature.FeatureGUID;
								if (featureGUID.HasValue)
								{
									Guid? featureGUID1 = aDOptionalFeature.FeatureGUID;
									ADOptionalFeatureFactory<T>.BuildADOFGuidSearcher(featureGUID1.Value, searchBase, cmdletSessionInfo);
								}
							}
							else
							{
								useSearchFilter = true;
								Guid? objectGuid1 = aDOptionalFeature.ObjectGuid;
								return IdentityResolverMethods.BuildGuidBaseSearcher(new Guid?(objectGuid1.Value), cmdletSessionInfo);
							}
						}
						return null;
					}
					else
					{
						return ADOptionalFeatureFactory<T>.BuildADOFGuidSearcher((Guid)identityObject, searchBase, cmdletSessionInfo);
					}
				}
				else
				{
					return ADOptionalFeatureFactory<T>.BuildADOFGuidSearcher(nullable.Value, searchBase, cmdletSessionInfo);
				}
			}
			else
			{
				throw new ArgumentNullException("identityObject");
			}
		}

		private static ADObjectSearcher BuildADOFGuidSearcher(Guid guidObject, string searchBase, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, searchBase, ADSearchScope.Subtree);
			IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectGUID", guidObject.ToByteArray());
			IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "msDS-OptionalFeatureGUID", guidObject.ToByteArray());
			IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
			aDOPathNodeArray[0] = aDOPathNode;
			aDOPathNodeArray[1] = aDOPathNode1;
			aDObjectSearcher.Filter = ADOPathUtil.CreateOrClause(aDOPathNodeArray);
			return aDObjectSearcher;
		}

		internal override IADOPathNode IdentitySearchConverter(object identity)
		{
			if (identity != null)
			{
				return base.IdentitySearchConverter(identity);
			}
			else
			{
				throw new ArgumentNullException("Identity");
			}
		}

		internal static void ToExtendedFeatureScope(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection();
			int value = (int)directoryObj[directoryAttributes[0]].Value;
			foreach (ADOptionalFeatureScope aDOptionalFeatureScope in Enum.GetValues(typeof(ADOptionalFeatureScope)))
			{
				if ((aDOptionalFeatureScope & (ADOptionalFeatureScope)value) <= ADOptionalFeatureScope.Unknown)
				{
					continue;
				}
				aDPropertyValueCollection.Add(aDOptionalFeatureScope);
			}
			userObj.Add(extendedAttribute, aDPropertyValueCollection);
		}

		internal static void ToExtendedIsDisableable(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool value = ((int)directoryObj[directoryAttributes[0]].Value & ADOptionalFeature.FeatureDisableableBit) > 0;
			userObj.Add(extendedAttribute, value);
		}

		internal static class ADOptionalFeaturePropertyMap
		{
			internal readonly static PropertyMapEntry EnabledScopes;

			internal readonly static PropertyMapEntry FeatureScope;

			internal readonly static PropertyMapEntry FeatureGUID;

			internal readonly static PropertyMapEntry RequiredDomainMode;

			internal readonly static PropertyMapEntry RequiredForestMode;

			internal readonly static PropertyMapEntry IsDisableable;

			static ADOptionalFeaturePropertyMap()
			{
				ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.EnabledScopes = new PropertyMapEntry("EnabledScopes", "msDS-EnabledFeatureBL", "msDS-EnabledFeatureBL");
				ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.FeatureScope = new PropertyMapEntry("FeatureScope", "msDS-OptionalFeatureFlags", "msDS-OptionalFeatureFlags");
				ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.FeatureGUID = new PropertyMapEntry("FeatureGUID", "msDS-OptionalFeatureGUID", "msDS-OptionalFeatureGUID");
				ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.RequiredDomainMode = new PropertyMapEntry("RequiredDomainMode", "msDS-RequiredDomainBehaviorVersion", "msDS-RequiredDomainBehaviorVersion");
				ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.RequiredForestMode = new PropertyMapEntry("RequiredForestMode", "msDS-RequiredForestBehaviorVersion", "msDS-RequiredForestBehaviorVersion");
				ADOptionalFeatureFactory<T>.ADOptionalFeaturePropertyMap.IsDisableable = new PropertyMapEntry("IsDisableable", "msDS-OptionalFeatureFlags", "msDS-OptionalFeatureFlags");
			}
		}
	}
}