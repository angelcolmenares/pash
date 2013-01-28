using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADResourcePropertyFactory<T> : ADClaimTypeBaseFactory<T>
	where T : ADResourceProperty, new()
	{
		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private static AttributeConverterEntry[] ADMappingTable;

		internal static IDictionary<ADServerType, MappingTable<AttributeConverterEntry>> AttributeTable
		{
			get
			{
				return ADClaimTypeBaseFactory<T>.AttributeTable;
			}
		}

		internal override IdentityResolverDelegate[] IdentityResolvers
		{
			get
			{
				return ADResourcePropertyFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADResourcePropertyFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-ResourceProperty";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADResourcePropertyFactory()
		{
			ADResourcePropertyFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[2];
			strArrays[0] = "name";
			strArrays[1] = "displayName";
			ADResourcePropertyFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADResourcePropertyFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADResourcePropertyFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[5];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.IsSecured.PropertyName, ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.IsSecured.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.SharesValuesWith.PropertyName, ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.SharesValuesWith.ADAttribute, TypeConstants.ADClaimType, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADResourcePropertyFactory<T>.ToDirectoryFromSPCTNameToDN), new ToSearchFilterDelegate(ADResourcePropertyFactory<T>.ToSearchFromSPCTNameToDN));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.MemberOf.PropertyName, ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.MemberOf.ADAttribute, TypeConstants.ADResourcePropertyList, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.ResourcePropertyValueType.PropertyName, ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.ResourcePropertyValueType.ADAttribute, TypeConstants.ADResourcePropertyValueType, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADResourcePropertyFactory<T>.ToDirectoryFromResourcePropertyValueTypeNameToDN), new ToSearchFilterDelegate(ADResourcePropertyFactory<T>.ToSearchFromResourcePropertyValueTypeNameToDN));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.AppliesToResourceTypes.PropertyName, ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.AppliesToResourceTypes.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryMultivalueObject), new ToSearchFilterDelegate(SearchConverters.ToSearchMultivalueObject));
			ADResourcePropertyFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADResourcePropertyFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADResourcePropertyFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADResourcePropertyPreCommitFSRoutine));
		}

		private bool ADResourcePropertyPreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			if (operation == ADFactory<T>.DirectoryOperation.Create || ADFactory<T>.DirectoryOperation.Update == operation)
			{
				bool flag = true;
				ADObject aDObject = null;
				StringBuilder stringBuilder = new StringBuilder();
				flag = flag & ADResourcePropertyFactory<T>.VerifyResourcePropertyValueType(directoryObj, base.CmdletSessionInfo, out aDObject, stringBuilder);
				if (aDObject != null && aDObject.Contains("ValueType"))
				{
					ADClaimValueType? value = (ADClaimValueType?)(aDObject["ValueType"].Value as ADClaimValueType?);
					flag = flag & ADCBACUtil.VerifyAndSetPossibleValues(directoryObj, value.Value, stringBuilder);
				}
				flag = flag & ADResourcePropertyFactory<T>.VerifySharesPossibleValueWithAndPossibleValueExclusiveness(directoryObj, operation, stringBuilder);
				flag = flag & ADResourcePropertyFactory<T>.VerifyIsSuggestedValuePresentAttribute(directoryObj, stringBuilder, aDObject);
				flag = flag & ADResourcePropertyFactory<T>.VerifySharesValuesWith(directoryObj, stringBuilder, aDObject, base.CmdletSessionInfo);
				if (flag)
				{
					return false;
				}
				else
				{
					stringBuilder.AppendLine(StringResources.CTParameterValidationFailure);
					throw new ADException(stringBuilder.ToString());
				}
			}
			else
			{
				return false;
			}
		}

		internal override string GenerateObjectName(ADParameterSet parameters)
		{
			if (parameters.Contains("ID"))
			{
				ADCBACUtil.ValidateResourceID(parameters["ID"] as string);
				return parameters["ID"] as string;
			}
			else
			{
				return ADCBACUtil.GenerateResourceID(parameters["DisplayName"] as string);
			}
		}

		internal static void ToDirectoryFromResourcePropertyValueTypeNameToDN(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Value Types,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			AttributeConverters.ToDirectoryFromADEntityToAttributeValue<ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType>, ADResourcePropertyValueType>(str, null, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
		}

		internal static void ToDirectoryFromSPCTNameToDN(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			str = ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
			try
			{
				AttributeConverters.ToDirectoryFromADEntityToAttributeValue<ADClaimTypeFactory<ADClaimType>, ADClaimType>(str, null, extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo);
			}
			catch (ADIdentityResolutionException aDIdentityResolutionException1)
			{
				ADIdentityResolutionException aDIdentityResolutionException = aDIdentityResolutionException1;
				object[] message = new object[1];
				message[0] = aDIdentityResolutionException.Message;
				throw new ADIdentityResolutionException(string.Format(CultureInfo.CurrentCulture, StringResources.SharesValueWithIdentityError, message), aDIdentityResolutionException);
			}
		}

		internal static IADOPathNode ToSearchFromResourcePropertyValueTypeNameToDN(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Value Types,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			return SearchConverters.ToSearchFromADEntityToAttributeValue<ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType>, ADResourcePropertyValueType>(str, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchFromSPCTNameToDN(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			str = ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
			return SearchConverters.ToSearchFromADEntityToAttributeValue<ADClaimTypeFactory<ADClaimType>, ADClaimType>(str, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		private static bool VerifyIsSuggestedValuePresentAttribute(ADObject directoryObj, StringBuilder errorBuffer, ADObject resourcePropertyValueTypeObj)
		{
			bool flag = false;
			if (!directoryObj.Contains("msDS-ClaimSharesPossibleValuesWith") && resourcePropertyValueTypeObj != null && resourcePropertyValueTypeObj.Contains("IsSuggestedValuesPresent") && resourcePropertyValueTypeObj["IsSuggestedValuesPresent"].Value != null)
			{
				bool? value = (bool?)(resourcePropertyValueTypeObj["IsSuggestedValuesPresent"].Value as bool?);
				if (!value.Value || directoryObj.Contains("msDS-ClaimPossibleValues"))
				{
					if (!value.Value && directoryObj.Contains("msDS-ClaimPossibleValues"))
					{
						flag = true;
						errorBuffer.AppendLine(StringResources.RCTSuggestedValuePresentError);
					}
				}
				else
				{
					flag = true;
					errorBuffer.AppendLine(StringResources.RCTSuggestedValueNotPresentError);
				}
			}
			return !flag;
		}

		private static bool VerifyResourcePropertyValueType(ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo, out ADObject resourcePropertyValueTypeObj, StringBuilder errorBuffer)
		{
			bool flag = false;
			resourcePropertyValueTypeObj = null;
			if (directoryObj.Contains("msDS-ValueTypeReference"))
			{
				string value = directoryObj["msDS-ValueTypeReference"].Value as string;
				ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType> aDResourcePropertyValueTypeFactory = new ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType>();
				aDResourcePropertyValueTypeFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				ADResourcePropertyValueType aDResourcePropertyValueType = new ADResourcePropertyValueType();
				aDResourcePropertyValueType.Identity = value;
				string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Value Types,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
				resourcePropertyValueTypeObj = aDResourcePropertyValueTypeFactory.GetExtendedObjectFromIdentity(aDResourcePropertyValueType, str);
			}
			else
			{
				flag = true;
				errorBuffer.AppendLine(StringResources.RCTNoResourcePropertyValueTypeError);
			}
			return !flag;
		}

		private static bool VerifySharesPossibleValueWithAndPossibleValueExclusiveness(ADObject directoryObj, ADFactory<T>.DirectoryOperation operation, StringBuilder errorBuffer)
		{
			bool flag;
			bool flag1 = false;
			if (directoryObj.ModifiedProperties.Contains("msDS-ClaimSharesPossibleValuesWith"))
			{
				flag = true;
			}
			else
			{
				flag = directoryObj.AddedProperties.Contains("msDS-ClaimSharesPossibleValuesWith");
			}
			bool flag2 = flag;
			bool flag3 = directoryObj.InternalProperties.Contains("SuggestedValues");
			if (directoryObj.Contains("msDS-ClaimSharesPossibleValuesWith") && directoryObj.Contains("msDS-ClaimPossibleValues"))
			{
				if (operation != ADFactory<T>.DirectoryOperation.Create)
				{
					if (!flag2 || !flag3)
					{
						if (!flag2)
						{
							if (flag3)
							{
								directoryObj.ForceRemove("msDS-ClaimSharesPossibleValuesWith");
							}
						}
						else
						{
							directoryObj.ForceRemove("msDS-ClaimPossibleValues");
						}
					}
					else
					{
						flag1 = true;
						errorBuffer.AppendLine(StringResources.CTBothPossibleValuesShareValueWarning);
					}
				}
				else
				{
					flag1 = true;
					errorBuffer.AppendLine(StringResources.CTBothPossibleValuesShareValueWarning);
				}
			}
			return !flag1;
		}

		private static bool VerifySharesValuesWith(ADObject directoryObj, StringBuilder errorBuffer, ADObject resourcePropertyValueTypeObj, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag = false;
			if (directoryObj.Contains("msDS-ClaimSharesPossibleValuesWith") && resourcePropertyValueTypeObj != null)
			{
				ADClaimTypeFactory<ADClaimType> aDClaimTypeFactory = new ADClaimTypeFactory<ADClaimType>();
				aDClaimTypeFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				ADClaimType aDClaimType = new ADClaimType();
				aDClaimType.Identity = directoryObj["msDS-ClaimSharesPossibleValuesWith"].Value as string;
				string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
				str = ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
				ADObject extendedObjectFromIdentity = aDClaimTypeFactory.GetExtendedObjectFromIdentity(aDClaimType, str);
				if (!extendedObjectFromIdentity.Contains("CompatibleResourceTypes") || extendedObjectFromIdentity["CompatibleResourceTypes"].Value == null)
				{
					flag = true;
					errorBuffer.AppendLine(StringResources.ResourcePropertySharesValueWithValueTypeError);
				}
				else
				{
					List<string> value = extendedObjectFromIdentity["CompatibleResourceTypes"].Value as List<string>;
					if (!value.Contains(resourcePropertyValueTypeObj.Name))
					{
						flag = true;
						errorBuffer.AppendLine(StringResources.ResourcePropertySharesValueWithValueTypeError);
					}
				}
			}
			return !flag;
		}

		internal static class ADResourcePropertyPropertyMap
		{
			public readonly static PropertyMapEntry SharesValuesWith;

			public readonly static PropertyMapEntry IsSecured;

			public readonly static PropertyMapEntry MemberOf;

			public readonly static PropertyMapEntry ResourcePropertyValueType;

			public readonly static PropertyMapEntry AppliesToResourceTypes;

			static ADResourcePropertyPropertyMap()
			{
				ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.SharesValuesWith = new PropertyMapEntry("SharesValuesWith", "msDS-ClaimSharesPossibleValuesWith", "msDS-ClaimSharesPossibleValuesWith");
				ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.IsSecured = new PropertyMapEntry("IsSecured", "msDS-IsUsedAsResourceSecurityAttribute", "msDS-IsUsedAsResourceSecurityAttribute");
				ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.MemberOf = new PropertyMapEntry("MemberOf", "msDS-MembersOfResourcePropertyListBL", "msDS-MembersOfResourcePropertyListBL");
				ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.ResourcePropertyValueType = new PropertyMapEntry("ResourcePropertyValueType", "msDS-ValueTypeReference", "msDS-ValueTypeReference");
				ADResourcePropertyFactory<T>.ADResourcePropertyPropertyMap.AppliesToResourceTypes = new PropertyMapEntry("AppliesToResourceTypes", "msDS-AppliesToResourceTypes", "msDS-AppliesToResourceTypes");
			}
		}
	}
}