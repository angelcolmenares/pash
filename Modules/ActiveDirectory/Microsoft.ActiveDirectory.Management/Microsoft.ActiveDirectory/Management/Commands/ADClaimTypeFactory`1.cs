using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADClaimTypeFactory<T> : ADClaimTypeBaseFactory<T>
	where T : ADClaimType, new()
	{
		private readonly static string _rDNPrefix;

		private readonly static string[] _identityLdapAttributes;

		private readonly static IdentityResolverDelegate[] _identityResolvers;

		private readonly static string[] ValidAppliesToClasses;

		private readonly static string[] BlockedAttributes;

		private static int FLAG_ATTR_NOT_REPLICATED;

		private static int RODCFilteredAttribute;

		private readonly static string[] ValidAttributeSyntaxString;

		private readonly static string[] ValidAttributeSyntaxInt;

		private readonly static string ValidAttributeSyntaxUInt;

		private static string ValidBooleanAttributeSyntax;

		private readonly static string[] SchemaAttributeProperties;

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
				return ADClaimTypeFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADClaimTypeFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-ClaimType";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADClaimTypeFactory()
		{
			ADClaimTypeFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[2];
			strArrays[0] = "name";
			strArrays[1] = "displayName";
			ADClaimTypeFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADClaimTypeFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADClaimTypeFactory<T>._identityResolvers = customIdentityResolver;
			string[] strArrays1 = new string[4];
			strArrays1[0] = "msDS-ManagedServiceAccount";
			strArrays1[1] = "user";
			strArrays1[2] = "computer";
			strArrays1[3] = "inetOrgPerson";
			ADClaimTypeFactory<T>.ValidAppliesToClasses = strArrays1;
			string[] strArrays2 = new string[4];
			strArrays2[0] = "dBCSPwd";
			strArrays2[1] = "lmPwdHistory";
			strArrays2[2] = "dBCSPwd";
			strArrays2[3] = "unicodePwd";
			ADClaimTypeFactory<T>.BlockedAttributes = strArrays2;
			ADClaimTypeFactory<T>.FLAG_ATTR_NOT_REPLICATED = 1;
			ADClaimTypeFactory<T>.RODCFilteredAttribute = 0x200;
			string[] strArrays3 = new string[3];
			strArrays3[0] = "2.5.5.1";
			strArrays3[1] = "2.5.5.12";
			strArrays3[2] = "2.5.5.15";
			ADClaimTypeFactory<T>.ValidAttributeSyntaxString = strArrays3;
			string[] strArrays4 = new string[2];
			strArrays4[0] = "2.5.5.9";
			strArrays4[1] = "2.5.5.16";
			ADClaimTypeFactory<T>.ValidAttributeSyntaxInt = strArrays4;
			ADClaimTypeFactory<T>.ValidAttributeSyntaxUInt = "2.5.5.2";
			ADClaimTypeFactory<T>.ValidBooleanAttributeSyntax = "2.5.5.8";
			string[] strArrays5 = new string[5];
			strArrays5[0] = "lDAPDisplayName";
			strArrays5[1] = "systemFlags";
			strArrays5[2] = "attributeSyntax";
			strArrays5[3] = "isDefunct";
			strArrays5[4] = "searchFlags";
			ADClaimTypeFactory<T>.SchemaAttributeProperties = strArrays5;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[8];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.SourceAttribute.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.SourceAttribute.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(ADClaimTypeFactory<T>.ToDirectoryFromSourceAttributeToDN), new ToSearchFilterDelegate(ADCBACUtil.ToSearchFromSchemaObjectNameToDN));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.SourceOID.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.SourceOID.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.AppliesToClasses.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.AppliesToClasses.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), new ToDirectoryFormatDelegate(ADCBACUtil.ToDirectoryFromSchemaObjectNameListToDNList), new ToSearchFilterDelegate(ADCBACUtil.ToSearchFromSchemaObjectNameToDN));
			attributeConverterEntry[3] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.RestrictValues.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.RestrictValues.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[4] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.ValueType.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.ValueType.ADAttribute, TypeConstants.ADClaimValueType, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADClaimTypeBaseFactory<ADClaimTypeBase>.ToExtendedFromIntToValueTypeEnum), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObjectWithCast<long>), new ToSearchFilterDelegate(SearchConverters.ToSearchEnum<ADClaimValueType>));
			attributeConverterEntry[5] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.IsSingleValued.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.IsSingleValued.ADAttribute, TypeConstants.Bool, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), new ToDirectoryFormatDelegate(AttributeConverters.ToDirectoryObject), new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			attributeConverterEntry[6] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.CompatibleResourceTypes.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.CompatibleResourceTypes.ADAttribute, TypeConstants.ADResourcePropertyValueType, true, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(ADClaimTypeFactory<T>.ToCompatibleResourceTypes), null, new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			attributeConverterEntry[7] = new AttributeConverterEntry(ADClaimTypeFactory<T>.ADClaimTypePropertyMap.ClaimSourceType.PropertyName, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.ClaimSourceType.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.Read, true, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedObject), null, new ToSearchFilterDelegate(SearchConverters.ToSearchUsingSchemaInfo));
			ADClaimTypeFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADClaimTypeFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADClaimTypeFactory()
		{
			base.PreCommitPipeline.InsertAtEnd(new ADFactory<T>.FactoryCommitSubroutine(this.ADClaimTypePreCommitFSRoutine));
		}

		private bool ADClaimTypePreCommitFSRoutine(ADFactory<T>.DirectoryOperation operation, T instance, ADParameterSet parameters, ADObject directoryObj)
		{
			bool classes = true;
			StringBuilder stringBuilder = new StringBuilder();
			if (operation == ADFactory<T>.DirectoryOperation.Create || ADFactory<T>.DirectoryOperation.Update == operation)
			{
				bool switchParameterBooleanValue = parameters.GetSwitchParameterBooleanValue("SourceTransformPolicy");
				if (switchParameterBooleanValue)
				{
					directoryObj.SetValue("msDS-ClaimSourceType", "TransformPolicy");
					directoryObj.ForceRemove("msDS-ClaimAttributeSource");
					directoryObj.ForceRemove("msDS-ClaimSource");
				}
				classes = classes & ADClaimTypeFactory<T>.VerifyClaimSourceAndAttributeExclusiveness(directoryObj, stringBuilder, operation);
				classes = classes & ADClaimTypeFactory<T>.VerifyClaimSourceAndPossibleValueExclusiveness(directoryObj, stringBuilder);
				classes = classes & ADClaimTypeFactory<T>.SetAndValidateClaimSourceType(directoryObj, stringBuilder);
				classes = classes & ADClaimTypeFactory<T>.VerifyRestrictValues(directoryObj, stringBuilder, base.CmdletSessionInfo, operation);
				if (directoryObj.Contains("msDS-ClaimValueType"))
				{
					ADClaimValueType num = (ADClaimValueType)((long)Convert.ToInt32(directoryObj["msDS-ClaimValueType"].Value, CultureInfo.InvariantCulture));
					classes = classes & ADCBACUtil.VerifyAndSetPossibleValues(directoryObj, num, stringBuilder);
				}
				classes = classes & ADClaimTypeFactory<T>.VerifyAppliesToClasses(directoryObj, stringBuilder, base.CmdletSessionInfo);
				if (classes)
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
				ADCBACUtil.ValidateClaimID(parameters["ID"] as string);
				return parameters["ID"] as string;
			}
			else
			{
				return ADCBACUtil.GenerateClaimID(parameters["DisplayName"] as string);
			}
		}

		private static bool SetAndValidateClaimSourceType(ADObject directoryObj, StringBuilder errorBuffer)
		{
			bool flag = false;
			if (!directoryObj.Contains("msDS-ClaimSource"))
			{
				if (directoryObj.Contains("msDS-ClaimAttributeSource"))
				{
					directoryObj.SetValue("msDS-ClaimSourceType", "AD");
				}
			}
			else
			{
				if (!directoryObj.Contains("msDS-ClaimValueType"))
				{
					directoryObj.Add("msDS-ClaimValueType", (long)6);
				}
				else
				{
					long? value = (long?)(directoryObj["msDS-ClaimValueType"].Value as long?);
					if (value.Value != (long)6)
					{
						flag = true;
						errorBuffer.AppendLine(StringResources.CTSourceOIDValueTypeError);
					}
				}
				directoryObj.SetValue("msDS-ClaimSourceType", "Certificate");
			}
			return !flag;
		}

		internal static void ToCompatibleResourceTypes(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains("msDS-ClaimValueType") || !directoryObj.Contains("msDS-ClaimIsValueSpaceRestricted"))
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
				return;
			}
			else
			{
				long value = (long)directoryObj["msDS-ClaimValueType"].Value;
				bool flag = directoryObj.Contains("msDS-ClaimPossibleValues");
				IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "msDS-ClaimValueType", value);
				IADOPathNode aDOPathNode1 = ADOPathUtil.CreateFilterClause(ADOperator.Eq, "msDS-IsPossibleValuesPresent", flag);
				IADOPathNode[] aDOPathNodeArray = new IADOPathNode[2];
				aDOPathNodeArray[0] = aDOPathNode;
				aDOPathNodeArray[1] = aDOPathNode1;
				IADOPathNode aDOPathNode2 = ADOPathUtil.CreateAndClause(aDOPathNodeArray);
				ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType> aDResourcePropertyValueTypeFactory = new ADResourcePropertyValueTypeFactory<ADResourcePropertyValueType>();
				aDResourcePropertyValueTypeFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Value Types,CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
				int? nullable = null;
				int? nullable1 = null;
				IEnumerable<ADResourcePropertyValueType> extendedObjectFromFilter = aDResourcePropertyValueTypeFactory.GetExtendedObjectFromFilter(aDOPathNode2, str, ADSearchScope.OneLevel, null, nullable, nullable1, false);
				List<string> strs = new List<string>();
				foreach (ADResourcePropertyValueType aDResourcePropertyValueType in extendedObjectFromFilter)
				{
					strs.Add(aDResourcePropertyValueType.Name);
				}
				userObj.Add(extendedAttribute, new ADPropertyValueCollection(strs));
				return;
			}
		}

		internal static void ToDirectoryFromSourceAttributeToDN(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				ADSchemaObjectFactory<ADSchemaObject> aDSchemaObjectFactory = new ADSchemaObjectFactory<ADSchemaObject>();
				aDSchemaObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
				ADSchemaObject aDSchemaObject = new ADSchemaObject();
				aDSchemaObject.Identity = extendedData.Value;
				ADObject extendedObjectFromIdentity = aDSchemaObjectFactory.GetExtendedObjectFromIdentity(aDSchemaObject, cmdletSessionInfo.ADRootDSE.SchemaNamingContext, ADClaimTypeFactory<T>.SchemaAttributeProperties);
				if (extendedObjectFromIdentity != null)
				{
					if (!extendedObjectFromIdentity.Contains("lDAPDisplayName") || !extendedObjectFromIdentity.Contains("attributeSyntax"))
					{
						object[] value = new object[1];
						value[0] = extendedData.Value;
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.SPCTInvalidSourceAttribute, value));
					}
					else
					{
						string str = (string)extendedObjectFromIdentity["lDAPDisplayName"].Value;
						string value1 = (string)extendedObjectFromIdentity["attributeSyntax"].Value;
						HashSet<string> strs = new HashSet<string>(ADClaimTypeFactory<T>.BlockedAttributes, StringComparer.OrdinalIgnoreCase);
						if (!strs.Contains(str))
						{
							if (!extendedObjectFromIdentity.Contains("systemFlags") || ((int)extendedObjectFromIdentity["systemFlags"].Value & ADClaimTypeFactory<T>.FLAG_ATTR_NOT_REPLICATED) == 0)
							{
								if (!extendedObjectFromIdentity.Contains("searchFlags") || ((int)extendedObjectFromIdentity["searchFlags"].Value & ADClaimTypeFactory<T>.RODCFilteredAttribute) == 0)
								{
									if (!extendedObjectFromIdentity.Contains("isDefunct") || !(bool)extendedObjectFromIdentity["isDefunct"].Value)
									{
										ADClaimValueType aDClaimValueType = ADClaimValueType.Invalid;
										HashSet<string> strs1 = new HashSet<string>(ADClaimTypeFactory<T>.ValidAttributeSyntaxInt, StringComparer.OrdinalIgnoreCase);
										HashSet<string> strs2 = new HashSet<string>(ADClaimTypeFactory<T>.ValidAttributeSyntaxString, StringComparer.OrdinalIgnoreCase);
										if (!strs2.Contains(value1))
										{
											if (!strs1.Contains(value1))
											{
												if (string.Compare(value1, ADClaimTypeFactory<T>.ValidAttributeSyntaxUInt, true, CultureInfo.InvariantCulture) != 0)
												{
													if (string.Compare(value1, ADClaimTypeFactory<T>.ValidBooleanAttributeSyntax, true, CultureInfo.InvariantCulture) != 0)
													{
														throw new ADException(StringResources.SPCTInvalidAttributeSyntax);
													}
													else
													{
														aDClaimValueType = ADClaimValueType.Boolean;
													}
												}
												else
												{
													aDClaimValueType = ADClaimValueType.UInt64;
												}
											}
											else
											{
												aDClaimValueType = ADClaimValueType.Int64;
											}
										}
										else
										{
											aDClaimValueType = ADClaimValueType.String;
										}
										directoryObj[directoryAttributes[0]].Value = extendedObjectFromIdentity.DistinguishedName;
										if (!directoryObj.Contains("msDS-ClaimValueType"))
										{
											directoryObj.Add("msDS-ClaimValueType", (long)aDClaimValueType);
										}
										else
										{
											long? nullable = (long?)(directoryObj["msDS-ClaimValueType"].Value as long?);
											if ((ADClaimValueType)nullable.Value != aDClaimValueType)
											{
												throw new ADException(StringResources.CTSourceAttributeValueTypeError);
											}
										}
										directoryObj.InternalProperties[directoryAttributes[0]].Value = str;
										return;
									}
									else
									{
										throw new ADException(StringResources.SPCTDefuctSourceAttr);
									}
								}
								else
								{
									throw new ADException(StringResources.SPCTRODCFilteredSourceAttr);
								}
							}
							else
							{
								throw new ADException(StringResources.SPCTNonREPLSourceAttrError);
							}
						}
						else
						{
							throw new ADException(StringResources.SPCTBlockedSourceAttribute);
						}
					}
				}
				else
				{
					throw new ADException(StringResources.SPCTInvalidSourceAttributeName);
				}
			}
		}

		private static bool VerifyAppliesToClasses(ADObject directoryObj, StringBuilder errorBuffer, CmdletSessionInfo cmdletSessionInfo)
		{
			bool flag = false;
			if (directoryObj.Contains("msDS-ClaimTypeAppliesToClass"))
			{
				ADSchemaUtil aDSchemaUtil = new ADSchemaUtil(cmdletSessionInfo.ADSessionInfo);
				string[] valueList = directoryObj["msDS-ClaimTypeAppliesToClass"].ValueList as string[];
				string[] strArrays = valueList;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					HashSet<string> allParentClassesForSchemaClassDN = aDSchemaUtil.GetAllParentClassesForSchemaClassDN(str);
					allParentClassesForSchemaClassDN.IntersectWith(ADClaimTypeFactory<T>.ValidAppliesToClasses);
					if (allParentClassesForSchemaClassDN.Count == 0)
					{
						flag = true;
						object[] objArray = new object[1];
						objArray[0] = str;
						errorBuffer.AppendLine(string.Format(CultureInfo.CurrentCulture, StringResources.SPCTInvalidAppliesToClassWarning, objArray));
					}
				}
				if (directoryObj.Contains("msDS-ClaimAttributeSource"))
				{
					string value = directoryObj["msDS-ClaimAttributeSource"].Value as string;
					ADSchemaObjectFactory<ADSchemaObject> aDSchemaObjectFactory = new ADSchemaObjectFactory<ADSchemaObject>();
					aDSchemaObjectFactory.SetCmdletSessionInfo(cmdletSessionInfo);
					ADSchemaObject aDSchemaObject = new ADSchemaObject();
					aDSchemaObject.Identity = value;
					string[] strArrays1 = new string[1];
					strArrays1[0] = "lDAPDisplayName";
					ADObject extendedObjectFromIdentity = aDSchemaObjectFactory.GetExtendedObjectFromIdentity(aDSchemaObject, cmdletSessionInfo.ADRootDSE.SchemaNamingContext, strArrays1);
					if (extendedObjectFromIdentity.Contains("lDAPDisplayName"))
					{
						string value1 = extendedObjectFromIdentity["lDAPDisplayName"].Value as string;
						string[] strArrays2 = valueList;
						for (int j = 0; j < (int)strArrays2.Length; j++)
						{
							string str1 = strArrays2[j];
							HashSet<string> attributeListForSchemaClassDN = aDSchemaUtil.GetAttributeListForSchemaClassDN(str1);
							if (!attributeListForSchemaClassDN.Contains(value1))
							{
								flag = true;
								object[] objArray1 = new object[2];
								objArray1[0] = str1;
								objArray1[1] = value1;
								errorBuffer.AppendLine(string.Format(CultureInfo.CurrentCulture, StringResources.SPCTAttributeNotFoundInSchemaClass, objArray1));
							}
						}
					}
					else
					{
						flag = true;
						object[] objArray2 = new object[1];
						objArray2[0] = value;
						errorBuffer.AppendLine(string.Format(CultureInfo.CurrentCulture, StringResources.SPCTSourceAttributeLdapDisplayNameError, objArray2));
					}
				}
			}
			return !flag;
		}

		private static bool VerifyClaimSourceAndAttributeExclusiveness(ADObject directoryObj, StringBuilder errorBuffer, ADFactory<T>.DirectoryOperation operation)
		{
			bool flag;
			bool flag1;
			bool flag2 = false;
			if (directoryObj.ModifiedProperties.Contains("msDS-ClaimSource"))
			{
				flag = true;
			}
			else
			{
				flag = directoryObj.AddedProperties.Contains("msDS-ClaimSource");
			}
			bool flag3 = flag;
			if (directoryObj.ModifiedProperties.Contains("msDS-ClaimAttributeSource"))
			{
				flag1 = true;
			}
			else
			{
				flag1 = directoryObj.AddedProperties.Contains("msDS-ClaimAttributeSource");
			}
			bool flag4 = flag1;
			if (directoryObj.Contains("msDS-ClaimSource") && directoryObj.Contains("msDS-ClaimAttributeSource"))
			{
				if (operation != ADFactory<T>.DirectoryOperation.Create)
				{
					if (!flag3 || !flag4)
					{
						if (!flag3)
						{
							if (flag4)
							{
								directoryObj.ForceRemove("msDS-ClaimSource");
							}
						}
						else
						{
							directoryObj.ForceRemove("msDS-ClaimAttributeSource");
						}
					}
					else
					{
						flag2 = true;
						errorBuffer.AppendLine(StringResources.SPCTBothSourceWarning);
					}
				}
				else
				{
					flag2 = true;
					errorBuffer.AppendLine(StringResources.SPCTBothSourceWarning);
				}
			}
			return !flag2;
		}

		private static bool VerifyClaimSourceAndPossibleValueExclusiveness(ADObject directoryObj, StringBuilder errorBuffer)
		{
			bool flag = false;
			if (directoryObj.Contains("msDS-ClaimSource") && directoryObj.InternalProperties.Contains("SuggestedValues"))
			{
				flag = true;
				errorBuffer.AppendLine(StringResources.SPCTBothSourceOIDPossibleValuesWarning);
			}
			return !flag;
		}

		private static bool VerifyRestrictValues(ADObject directoryObj, StringBuilder errorBuffer, CmdletSessionInfo cmdletSessionInfo, ADFactory<T>.DirectoryOperation operation)
		{
			bool flag;
			bool value;
			bool value1;
			bool flag1 = false;
			if (directoryObj.ModifiedProperties.Contains("msDS-ClaimIsValueSpaceRestricted"))
			{
				flag = true;
			}
			else
			{
				flag = directoryObj.AddedProperties.Contains("msDS-ClaimIsValueSpaceRestricted");
			}
			bool flag2 = flag;
			if (!directoryObj.Contains("msDS-ClaimPossibleValues"))
			{
				value = false;
			}
			else
			{
				value = directoryObj["msDS-ClaimPossibleValues"].Value != null;
			}
			bool flag3 = value;
			if (!directoryObj.InternalProperties.Contains("SuggestedValues"))
			{
				value1 = false;
			}
			else
			{
				value1 = directoryObj.InternalProperties["SuggestedValues"].Value != null;
			}
			bool flag4 = value1;
			if (operation == ADFactory<T>.DirectoryOperation.Create && !directoryObj.Contains("msDS-ClaimIsValueSpaceRestricted"))
			{
				directoryObj.SetValue("msDS-ClaimIsValueSpaceRestricted", flag4);
			}
			if (operation == ADFactory<T>.DirectoryOperation.Update && !flag2)
			{
				if (!flag3)
				{
					directoryObj.SetValue("msDS-ClaimIsValueSpaceRestricted", false);
				}
				if (flag4 && !flag3)
				{
					directoryObj.SetValue("msDS-ClaimIsValueSpaceRestricted", true);
				}
			}
			if (!flag4 && !flag3 && directoryObj.Contains("msDS-ClaimIsValueSpaceRestricted") && (bool)directoryObj["msDS-ClaimIsValueSpaceRestricted"].Value)
			{
				flag1 = true;
				errorBuffer.AppendLine(StringResources.ClaimTypeRestrictValueError);
			}
			return !flag1;
		}

		internal static class ADClaimTypePropertyMap
		{
			public readonly static PropertyMapEntry SourceAttribute;

			public readonly static PropertyMapEntry SourceOID;

			public readonly static PropertyMapEntry AppliesToClasses;

			public readonly static PropertyMapEntry RestrictValues;

			public readonly static PropertyMapEntry ValueType;

			public readonly static PropertyMapEntry IsSingleValued;

			private readonly static string[] comapitableValueType;

			public readonly static PropertyMapEntry CompatibleResourceTypes;

			public readonly static PropertyMapEntry ClaimSourceType;

			static ADClaimTypePropertyMap()
			{
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.SourceAttribute = new PropertyMapEntry("SourceAttribute", "msDS-ClaimAttributeSource", "msDS-ClaimAttributeSource");
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.SourceOID = new PropertyMapEntry("SourceOID", "msDS-ClaimSource", "msDS-ClaimSource");
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.AppliesToClasses = new PropertyMapEntry("AppliesToClasses", "msDS-ClaimTypeAppliesToClass", "msDS-ClaimTypeAppliesToClass");
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.RestrictValues = new PropertyMapEntry("RestrictValues", "msDS-ClaimIsValueSpaceRestricted", "msDS-ClaimIsValueSpaceRestricted");
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.ValueType = new PropertyMapEntry("ValueType", "msDS-ClaimValueType", "msDS-ClaimValueType");
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.IsSingleValued = new PropertyMapEntry("IsSingleValued", "msDS-ClaimIsSingleValued", "msDS-ClaimIsSingleValued");
				string[] strArrays = new string[2];
				strArrays[0] = "msDS-ClaimValueType";
				strArrays[1] = "msDS-ClaimIsValueSpaceRestricted";
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.comapitableValueType = strArrays;
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.CompatibleResourceTypes = new PropertyMapEntry("CompatibleResourceTypes", ADClaimTypeFactory<T>.ADClaimTypePropertyMap.comapitableValueType, ADClaimTypeFactory<T>.ADClaimTypePropertyMap.comapitableValueType);
				ADClaimTypeFactory<T>.ADClaimTypePropertyMap.ClaimSourceType = new PropertyMapEntry("ClaimSourceType", "msDS-ClaimSourceType", "msDS-ClaimSourceType");
			}
		}
	}
}