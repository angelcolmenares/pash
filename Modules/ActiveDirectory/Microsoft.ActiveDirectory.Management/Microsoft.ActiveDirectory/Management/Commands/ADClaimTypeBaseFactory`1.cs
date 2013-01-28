using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADClaimTypeBaseFactory<T> : ADObjectFactory<T>
	where T : ADClaimTypeBase, new()
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
				return ADClaimTypeBaseFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADClaimTypeBaseFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-ClaimTypePropertyBase";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADClaimTypeBaseFactory()
		{
			ADClaimTypeBaseFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADClaimTypeBaseFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADClaimTypeBaseFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADClaimTypeBaseFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[4];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.SuggestedValues.PropertyName, ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.SuggestedValues.ADAttribute, TypeConstants.ByteArray, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(ADCBACUtil.ToExtendedADSuggestedValueEntryListFromXml), new ToDirectoryFormatDelegate(ADCBACUtil.ToDirectoryXmlFromADSuggestedValueEntryList), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.ID.PropertyName, ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.ID.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.DisplayName.PropertyName, ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.DisplayName.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.Enabled.PropertyName, ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.Enabled.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADClaimTypeBaseFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADClaimTypeBaseFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADClaimTypeBaseFactory()
		{
		}

		internal override ADObject GetDirectoryObjectFromIdentity(T identityObj, string searchRoot, bool showDeleted)
		{
			ADObject aDObject = identityObj;
			if (aDObject != null)
			{
				if (base.CmdletSessionInfo != null)
				{
					if (aDObject.IsSearchResult && aDObject.Contains("isDeleted"))
					{
						showDeleted = true;
					}
					ADObjectSearcher aDObjectSearcherFromIdentity = ADFactoryUtil.GetADObjectSearcherFromIdentity(identityObj, searchRoot, showDeleted, this.StructuralObjectFilter, this.BuildIdentityFilter(identityObj), this.IdentityResolvers, base.CmdletSessionInfo);
					AttributeSetRequest attributeSetRequest = this.ConstructAttributeSetRequest(null);
					string[] strArrays = new string[0];
					ADObject objectFromIdentitySearcher = ADFactoryUtil.GetObjectFromIdentitySearcher(aDObjectSearcherFromIdentity, identityObj, searchRoot, attributeSetRequest, base.CmdletSessionInfo, out strArrays);
					objectFromIdentitySearcher.TrackChanges = true;
					objectFromIdentitySearcher.SessionInfo = base.CmdletSessionInfo.ADSessionInfo;
					string[] strArrays1 = strArrays;
					for (int i = 0; i < (int)strArrays1.Length; i++)
					{
						string str = strArrays1[i];
						base.CmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(str);
					}
					return objectFromIdentitySearcher;
				}
				else
				{
					throw new ArgumentNullException(StringResources.SessionRequired);
				}
			}
			else
			{
				object[] type = new object[2];
				type[0] = "GetDirectoryObjectFromIdentity";
				type[1] = identityObj.GetType();
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, StringResources.MethodNotSupportedForObjectType, type));
			}
		}

		internal static void ToExtendedFromIntToValueTypeEnum(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (directoryObj.Contains(directoryAttributes[0]))
			{
				ADClaimValueType aDClaimValueType = ADClaimValueType.Invalid;
				long value = (long)directoryObj[directoryAttributes[0]].Value;
				if (!Enum.IsDefined(typeof(ADClaimValueType), value))
				{
					object[] objArray = new object[2];
					objArray[0] = directoryObj["distinguishedName"].Value;
					objArray[1] = value;
					cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidClaimValueType, objArray));
				}
				else
				{
					aDClaimValueType = (ADClaimValueType)value;
				}
				userObj.Add(extendedAttribute, new ADPropertyValueCollection((object)aDClaimValueType));
				return;
			}
			else
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
		}

		internal override void UpdateFromObject(T modifiedObject, ADObject directoryObj)
		{
			AttributeConverterEntry attributeConverterEntry = null;
			if (modifiedObject.Contains("SuggestedValues"))
			{
				MappingTable<AttributeConverterEntry> item = ADClaimTypeBaseFactory<T>.AttributeTable[base.ConnectedStore];
				if (item.TryGetValue("SuggestedValues", out attributeConverterEntry))
				{
					attributeConverterEntry.InvokeToDirectoryConverter(modifiedObject["SuggestedValues"], directoryObj, base.CmdletSessionInfo);
				}
			}
			base.UpdateFromObject(modifiedObject, directoryObj);
		}

		internal static class ADClaimTypeBasePropertyMap
		{
			public readonly static PropertyMapEntry SuggestedValues;

			public readonly static PropertyMapEntry DisplayName;

			public readonly static PropertyMapEntry Enabled;

			public readonly static PropertyMapEntry ID;

			static ADClaimTypeBasePropertyMap()
			{
				ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.SuggestedValues = new PropertyMapEntry("SuggestedValues", "msDS-ClaimPossibleValues", "msDS-ClaimPossibleValues");
				ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.DisplayName = new PropertyMapEntry("DisplayName", "displayName", "displayName");
				ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.Enabled = new PropertyMapEntry("Enabled", "Enabled", "Enabled");
				ADClaimTypeBaseFactory<T>.ADClaimTypeBasePropertyMap.ID = new PropertyMapEntry("ID", "name", "name");
			}
		}
	}
}