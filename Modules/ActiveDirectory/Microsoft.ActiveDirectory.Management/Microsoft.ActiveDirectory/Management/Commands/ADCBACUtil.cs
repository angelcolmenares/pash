using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ADCBACUtil
	{
		private const string possibleValuesXsd = "ADClaimPossibleValues.xsd";

		private static string AllowAll;

		private static string DenyAll;

		private static string AllowAllExceptSingleClaimRule;

		private static string AllowAllExcept;

		private static string DenyAllExceptSingleClaimRule;

		private static char ResourceIDDelimeter;

		private static int ClaimIDPrefixLength;

		private static int ClaimIDSuffixLength;

		private static string ClaimIDPrefix;

		private static char ColonDelimeter;

		private static char SlashDelimeter;

		private static int MaxClaimIDLength;

		private static int MinClaimIDLength;

		internal static List<char> InvalidClaimIDCharacters;

		static ADCBACUtil()
		{
			ADCBACUtil.AllowAll = "C1:[]=>Issue(claim=C1);";
			ADCBACUtil.DenyAll = "C1:[Type !~ \"*\"]=>Issue(claim=C1);";
			ADCBACUtil.AllowAllExceptSingleClaimRule = "Type !=\"{0}\"";
			ADCBACUtil.AllowAllExcept = "C1:[{0}]=>Issue(claim=C1);";
			ADCBACUtil.DenyAllExceptSingleClaimRule = "C1:[Type==\"{0}\"]=>Issue(claim=C1);";
			ADCBACUtil.ResourceIDDelimeter = '\u005F';
			ADCBACUtil.ClaimIDPrefixLength = 15;
			ADCBACUtil.ClaimIDSuffixLength = 16;
			ADCBACUtil.ClaimIDPrefix = "ad://ext/";
			ADCBACUtil.ColonDelimeter = ':';
			ADCBACUtil.SlashDelimeter = '/';
			ADCBACUtil.MaxClaimIDLength = ADCBACUtil.ClaimIDPrefix.Length + 32;
			ADCBACUtil.MinClaimIDLength = ADCBACUtil.ClaimIDPrefix.Length + 1;
			char[] chrArray = new char[1];
			chrArray[0] = ' ';
			ADCBACUtil.InvalidClaimIDCharacters = new List<char>(Path.GetInvalidFileNameChars().Concat<char>(chrArray));
		}

		internal static void AddSuggestedValueXmlFromADSuggestedValueEntryList(ADSuggestedValueEntry[] claimValues, ADEntity directoryObj, ADClaimValueType valueType)
		{
			if (valueType != ADClaimValueType.Invalid)
			{
				HashSet<string> strs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				ADSuggestedValueEntry[] aDSuggestedValueEntryArray = claimValues;
				int num = 0;
				while (num < (int)aDSuggestedValueEntryArray.Length)
				{
					ADSuggestedValueEntry aDSuggestedValueEntry = aDSuggestedValueEntryArray[num];
					if (!strs.Contains(aDSuggestedValueEntry.Value.ToString()))
					{
						strs.Add(aDSuggestedValueEntry.Value.ToString());
						num++;
					}
					else
					{
						object[] str = new object[1];
						str[0] = aDSuggestedValueEntry.Value.ToString();
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.SuggestedValueNotUniqueError, str));
					}
				}
				string suggestedValueXmlFromSuggestedValueEntryList = ADCBACUtil.GetSuggestedValueXmlFromSuggestedValueEntryList(claimValues, ADCBACUtil.GetsuggestedValueTypeFromClaimType(valueType));
				directoryObj["msDS-ClaimPossibleValues"].Value = suggestedValueXmlFromSuggestedValueEntryList;
				return;
			}
			else
			{
				throw new ADException(StringResources.InvalidValueTypeForPossibleValueXml);
			}
		}

		internal static List<ADSuggestedValueEntry> ConvertSuggestedValueXmlToSuggestedValueEntryList(string suggestedValuesXmlBlob, out bool hasUnknownElements)
		{
			ADCBACUtil.VerifyADSuggestedValuesXml(suggestedValuesXmlBlob);
			List<ADSuggestedValueEntry> aDSuggestedValueEntries = new List<ADSuggestedValueEntry>();
			hasUnknownElements = false;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PossibleClaimValuesType));
			PossibleClaimValuesType possibleClaimValuesType = (PossibleClaimValuesType)xmlSerializer.Deserialize(new StringReader(suggestedValuesXmlBlob));
			if (possibleClaimValuesType.Item as StringListType == null)
			{
				if (possibleClaimValuesType.Item as IntegerListType == null)
				{
					if (possibleClaimValuesType.Item as UnsignedIntegerListType == null)
					{
						hasUnknownElements = true;
					}
					else
					{
						UnsignedIntegerListType item = (UnsignedIntegerListType)possibleClaimValuesType.Item;
						UnsignedIntegerItemType[] unsignedIntegerItemTypeArray = item.Item;
						for (int i = 0; i < (int)unsignedIntegerItemTypeArray.Length; i++)
						{
							UnsignedIntegerItemType unsignedIntegerItemType = unsignedIntegerItemTypeArray[i];
							ADSuggestedValueEntry aDSuggestedValueEntry = new ADSuggestedValueEntry((object)unsignedIntegerItemType.Value, unsignedIntegerItemType.ValueDisplayName, unsignedIntegerItemType.ValueDescription, unsignedIntegerItemType.ValueGUID);
							aDSuggestedValueEntries.Add(aDSuggestedValueEntry);
							if (unsignedIntegerItemType.Any != null)
							{
								hasUnknownElements = true;
							}
						}
					}
				}
				else
				{
					IntegerListType integerListType = (IntegerListType)possibleClaimValuesType.Item;
					IntegerItemType[] integerItemTypeArray = integerListType.Item;
					for (int j = 0; j < (int)integerItemTypeArray.Length; j++)
					{
						IntegerItemType integerItemType = integerItemTypeArray[j];
						ADSuggestedValueEntry aDSuggestedValueEntry1 = new ADSuggestedValueEntry((object)integerItemType.Value, integerItemType.ValueDisplayName, integerItemType.ValueDescription, integerItemType.ValueGUID);
						aDSuggestedValueEntries.Add(aDSuggestedValueEntry1);
						if (integerItemType.Any != null)
						{
							hasUnknownElements = true;
						}
					}
				}
			}
			else
			{
				StringListType stringListType = (StringListType)possibleClaimValuesType.Item;
				StringItemType[] stringItemTypeArray = stringListType.Item;
				for (int k = 0; k < (int)stringItemTypeArray.Length; k++)
				{
					StringItemType stringItemType = stringItemTypeArray[k];
					ADSuggestedValueEntry aDSuggestedValueEntry2 = new ADSuggestedValueEntry(stringItemType.Value, stringItemType.ValueDisplayName, stringItemType.ValueDescription, stringItemType.ValueGUID);
					aDSuggestedValueEntries.Add(aDSuggestedValueEntry2);
					if (stringItemType.Any != null)
					{
						hasUnknownElements = true;
					}
				}
			}
			return aDSuggestedValueEntries;
		}

		internal static string CreateClaimTransformAllowRule(string[] claims)
		{
			if (claims == null || (int)claims.Length == 0)
			{
				return ADCBACUtil.AllowAll;
			}
			else
			{
				string empty = string.Empty;
				for (int i = 0; i < (int)claims.Length; i++)
				{
					object[] objArray = new object[1];
					objArray[0] = claims[i];
					string str = string.Format(CultureInfo.InvariantCulture, ADCBACUtil.AllowAllExceptSingleClaimRule, objArray);
					empty = string.Concat(empty, str);
					if (i != (int)claims.Length - 1)
					{
						empty = string.Concat(empty, ",");
					}
				}
				object[] objArray1 = new object[1];
				objArray1[0] = empty;
				return string.Format(CultureInfo.InvariantCulture, ADCBACUtil.AllowAllExcept, objArray1);
			}
		}

		internal static string CreateClaimTransformDenyRule(string[] claims)
		{
			if (claims == null || (int)claims.Length == 0)
			{
				return ADCBACUtil.DenyAll;
			}
			else
			{
				string empty = string.Empty;
				for (int i = 0; i < (int)claims.Length; i++)
				{
					object[] objArray = new object[1];
					objArray[0] = claims[i];
					string str = string.Format(CultureInfo.InvariantCulture, ADCBACUtil.DenyAllExceptSingleClaimRule, objArray);
					empty = string.Concat(empty, str);
				}
				return empty;
			}
		}

		internal static object FromNameToSchemaDN(object entity, string extendedAttribute, CmdletSessionInfo cmdletSessionInfo)
		{
			return AttributeConverters.GetAttributeValueFromObjectName<ADSchemaObjectFactory<ADSchemaObject>, ADSchemaObject>(entity, cmdletSessionInfo.ADRootDSE.SchemaNamingContext, null, extendedAttribute, cmdletSessionInfo);
		}

		internal static string GenerateClaimID(string displayName)
		{
			foreach (char invalidClaimIDCharacter in ADCBACUtil.InvalidClaimIDCharacters)
			{
				displayName = displayName.Replace(invalidClaimIDCharacter.ToString(), "");
			}
			if (displayName.Length > ADCBACUtil.ClaimIDPrefixLength)
			{
				displayName = displayName.Remove(ADCBACUtil.ClaimIDPrefixLength);
			}
			DateTime now = DateTime.Now;
			long binary = now.ToBinary();
			string str = Convert.ToString(binary, 16);
			object[] claimIDPrefix = new object[4];
			claimIDPrefix[0] = ADCBACUtil.ClaimIDPrefix;
			claimIDPrefix[1] = displayName;
			claimIDPrefix[2] = ADCBACUtil.ColonDelimeter;
			claimIDPrefix[3] = str;
			displayName = string.Concat(claimIDPrefix);
			return displayName;
		}

		internal static string GenerateResourceID(string displayName)
		{
			foreach (char invalidClaimIDCharacter in ADCBACUtil.InvalidClaimIDCharacters)
			{
				displayName = displayName.Replace(invalidClaimIDCharacter.ToString(), "");
			}
			displayName = displayName.Replace(ADCBACUtil.ResourceIDDelimeter.ToString(), "");
			if (displayName.Length > ADCBACUtil.ClaimIDPrefixLength)
			{
				displayName = displayName.Remove(ADCBACUtil.ClaimIDPrefixLength);
			}
			DateTime now = DateTime.Now;
			long binary = now.ToBinary();
			string str = Convert.ToString(binary, 16);
			displayName = string.Concat(displayName, ADCBACUtil.ResourceIDDelimeter, str);
			return displayName;
		}

		internal static string[] GetClaimTypeName(ADClaimType[] claims, CmdletSessionInfo cmdletSessionInfo, string extendedAttribute)
		{
			List<string> strs = new List<string>();
			ADClaimType[] aDClaimTypeArray = claims;
			for (int i = 0; i < (int)aDClaimTypeArray.Length; i++)
			{
				ADClaimType aDClaimType = aDClaimTypeArray[i];
				string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
				str = ADPathModule.MakePath(str, "CN=Claim Types,", ADPathFormat.X500);
				string attributeValueFromObjectName = (string)AttributeConverters.GetAttributeValueFromObjectName<ADClaimTypeFactory<ADClaimType>, ADClaimType>(aDClaimType, str, "Name", extendedAttribute, cmdletSessionInfo);
				strs.Add(attributeValueFromObjectName);
			}
			return strs.ToArray();
		}

		internal static ADSuggestedValueType GetsuggestedValueTypeFromClaimType(ADClaimValueType claimValaueType)
		{
			ADSuggestedValueType aDSuggestedValueType = ADSuggestedValueType.Invalid;
			ADClaimValueType aDClaimValueType = claimValaueType;
			if (aDClaimValueType <= ADClaimValueType.SID)
			{
				if (aDClaimValueType >= ADClaimValueType.Int64)
				{
					if ((int)(aDClaimValueType - ADClaimValueType.Int64) == 0)
					{
						aDSuggestedValueType = ADSuggestedValueType.Int64;
						return aDSuggestedValueType;
					}
					else if ((int)(aDClaimValueType - ADClaimValueType.Int64) == 1)
					{
						aDSuggestedValueType = ADSuggestedValueType.UInt64;
						return aDSuggestedValueType;
					}
					else if ((int)(aDClaimValueType - ADClaimValueType.Int64) == 2 || (int)(aDClaimValueType - ADClaimValueType.Int64) == 3 || (int)(aDClaimValueType - ADClaimValueType.Int64) == 4)
					{
						aDSuggestedValueType = ADSuggestedValueType.String;
						return aDSuggestedValueType;
					}
				}
				else
				{
					return aDSuggestedValueType;
				}
			}
			if (aDClaimValueType != ADClaimValueType.OctetString)
			{
				return aDSuggestedValueType;
			}
			aDSuggestedValueType = ADSuggestedValueType.String;
			return aDSuggestedValueType;
		}

		internal static string GetSuggestedValueXmlFromSuggestedValueEntryList(ADSuggestedValueEntry[] claimValues, ADSuggestedValueType valueType)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PossibleClaimValuesType));
			PossibleClaimValuesType possibleClaimValuesType = new PossibleClaimValuesType();
			ADSuggestedValueType aDSuggestedValueType = valueType;
			if (aDSuggestedValueType == ADSuggestedValueType.Int64)
			{
				IntegerListType integerListType = new IntegerListType();
				List<IntegerItemType> integerItemTypes = new List<IntegerItemType>();
				ADSuggestedValueEntry[] aDSuggestedValueEntryArray = claimValues;
				for (int i = 0; i < (int)aDSuggestedValueEntryArray.Length; i++)
				{
					ADSuggestedValueEntry aDSuggestedValueEntry = aDSuggestedValueEntryArray[i];
					IntegerItemType integerItemType = new IntegerItemType();
					integerItemType.Value = Convert.ToInt64(aDSuggestedValueEntry.Value, CultureInfo.InvariantCulture);
					integerItemType.ValueDescription = aDSuggestedValueEntry.ValueDescription;
					integerItemType.ValueDisplayName = aDSuggestedValueEntry.ValueDisplayName;
					integerItemType.ValueGUID = aDSuggestedValueEntry.ValueGUID;
					integerItemTypes.Add(integerItemType);
				}
				integerListType.Item = integerItemTypes.ToArray();
				possibleClaimValuesType.Item = integerListType;
			}
			else if (aDSuggestedValueType == ADSuggestedValueType.UInt64)
			{
				UnsignedIntegerListType unsignedIntegerListType = new UnsignedIntegerListType();
				List<UnsignedIntegerItemType> unsignedIntegerItemTypes = new List<UnsignedIntegerItemType>();
				ADSuggestedValueEntry[] aDSuggestedValueEntryArray1 = claimValues;
				for (int j = 0; j < (int)aDSuggestedValueEntryArray1.Length; j++)
				{
					ADSuggestedValueEntry aDSuggestedValueEntry1 = aDSuggestedValueEntryArray1[j];
					UnsignedIntegerItemType unsignedIntegerItemType = new UnsignedIntegerItemType();
					unsignedIntegerItemType.Value = Convert.ToUInt64(aDSuggestedValueEntry1.Value, CultureInfo.InvariantCulture);
					unsignedIntegerItemType.ValueDescription = aDSuggestedValueEntry1.ValueDescription;
					unsignedIntegerItemType.ValueDisplayName = aDSuggestedValueEntry1.ValueDisplayName;
					unsignedIntegerItemType.ValueGUID = aDSuggestedValueEntry1.ValueGUID;
					unsignedIntegerItemTypes.Add(unsignedIntegerItemType);
				}
				unsignedIntegerListType.Item = unsignedIntegerItemTypes.ToArray();
				possibleClaimValuesType.Item = unsignedIntegerListType;
			}
			else if (aDSuggestedValueType == ADSuggestedValueType.String)
			{
				StringListType stringListType = new StringListType();
				List<StringItemType> stringItemTypes = new List<StringItemType>();
				ADSuggestedValueEntry[] aDSuggestedValueEntryArray2 = claimValues;
				for (int k = 0; k < (int)aDSuggestedValueEntryArray2.Length; k++)
				{
					ADSuggestedValueEntry aDSuggestedValueEntry2 = aDSuggestedValueEntryArray2[k];
					StringItemType stringItemType = new StringItemType();
					stringItemType.Value = (string)aDSuggestedValueEntry2.Value;
					stringItemType.ValueDescription = aDSuggestedValueEntry2.ValueDescription;
					stringItemType.ValueDisplayName = aDSuggestedValueEntry2.ValueDisplayName;
					stringItemType.ValueGUID = aDSuggestedValueEntry2.ValueGUID;
					stringItemTypes.Add(stringItemType);
				}
				stringListType.Item = stringItemTypes.ToArray();
				possibleClaimValuesType.Item = stringListType;
			}
			else
			{
				throw new ADException("Unable to create Suggested Values Xml for the given Claim Type");
			}
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			xmlSerializer.Serialize(stringWriter, possibleClaimValuesType);
			return stringWriter.ToString();
		}

		internal static void InsertClaimTransformRule(ADParameterSet parameters, CmdletSessionInfo cmdletSessionInfo)
		{
			if (parameters.GetSwitchParameterBooleanValue("DenyAll"))
			{
				parameters["Rule"] = ADCBACUtil.CreateClaimTransformDenyRule(null);
			}
			if (!parameters.GetSwitchParameterBooleanValue("AllowAll"))
			{
				if (!parameters.Contains("AllowAllExcept") || parameters["AllowAllExcept"] == null)
				{
					if (parameters.Contains("DenyAllExcept") && parameters["DenyAllExcept"] != null)
					{
						string[] claimTypeName = ADCBACUtil.GetClaimTypeName((ADClaimType[])parameters["DenyAllExcept"], cmdletSessionInfo, "DenyAllExcept");
						parameters["Rule"] = ADCBACUtil.CreateClaimTransformDenyRule(claimTypeName);
					}
					return;
				}
				else
				{
					string[] strArrays = ADCBACUtil.GetClaimTypeName((ADClaimType[])parameters["AllowAllExcept"], cmdletSessionInfo, "AllowAllExcept");
					parameters["Rule"] = ADCBACUtil.CreateClaimTransformAllowRule(strArrays);
					return;
				}
			}
			else
			{
				parameters["Rule"] = ADCBACUtil.CreateClaimTransformAllowRule(null);
				return;
			}
		}

		internal static bool IsAttributeValueUsed<F, O>(string attributeName, string attributeValue, CmdletSessionInfo cmdletSessionInfo, string searchPath)
		where F : ADFactory<O>, new()
		where O : ADEntity, new()
		{
			bool flag;
			IADOPathNode aDOPathNode = ADOPathUtil.CreateFilterClause(ADOperator.Eq, attributeName, attributeValue);
			F f = Activator.CreateInstance<F>();
			f.SetCmdletSessionInfo(cmdletSessionInfo);
			int? nullable = null;
			int? nullable1 = null;
			IEnumerable<O> extendedObjectFromFilter = f.GetExtendedObjectFromFilter(aDOPathNode, searchPath, ADSearchScope.Subtree, null, nullable, nullable1, false);
			IEnumerator<O> enumerator = extendedObjectFromFilter.GetEnumerator();
			using (enumerator)
			{
				if (enumerator.MoveNext())
				{
					//TODO: Review: enumerator.Current;
					flag = true;
				}
				else
				{
					return false;
				}
			}
			return flag;
		}

		internal static void ToDirectoryFromSchemaObjectNameListToDNList(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			AttributeConverters.ToDirectoryMultivalueObjectConvertor(extendedAttribute, directoryAttributes, extendedData, directoryObj, cmdletSessionInfo, new MultiValueAttributeConvertorDelegate(ADCBACUtil.FromNameToSchemaDN));
		}

		internal static void ToDirectoryXmlFromADSuggestedValueEntryList(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				ADSuggestedValueEntry[] valueList = extendedData.ValueList as ADSuggestedValueEntry[];
				directoryObj.InternalProperties["SuggestedValues"].Value = valueList;
				return;
			}
		}

		internal static void ToExtendedADSuggestedValueEntryListFromXml(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(null);
				userObj.Add(extendedAttribute, aDPropertyValueCollection);
			}
			else
			{
				string value = (string)directoryObj[directoryAttributes[0]].Value;
				try
				{
					bool flag = false;
					List<ADSuggestedValueEntry> suggestedValueEntryList = ADCBACUtil.ConvertSuggestedValueXmlToSuggestedValueEntryList(value, out flag);
					ADPropertyValueCollection aDPropertyValueCollection1 = new ADPropertyValueCollection(suggestedValueEntryList.ToArray());
					userObj.Add(extendedAttribute, aDPropertyValueCollection1);
					if (flag)
					{
						object[] objArray = new object[2];
						objArray[0] = directoryObj["name"].Value;
						objArray[1] = "msDS-ClaimPossibleValues";
						cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.NextVersionPossibleValuesXml, objArray));
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (exception as XmlException != null || exception as XmlSchemaException != null || exception as InvalidOperationException != null)
					{
						object[] value1 = new object[1];
						value1[0] = directoryObj["distinguishedName"].Value;
						cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.InvalidPossibleValuesXml, value1));
						ADPropertyValueCollection aDPropertyValueCollection2 = new ADPropertyValueCollection(null);
						userObj.Add(extendedAttribute, aDPropertyValueCollection2);
					}
					else
					{
						throw;
					}
				}
			}
		}

		internal static IADOPathNode ToSearchFromResourcePropertyNameToDN(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			string str = ADPathModule.MakePath(cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext, "CN=Claims Configuration,CN=Services,", ADPathFormat.X500);
			str = ADPathModule.MakePath(str, "CN=Resource Properties,", ADPathFormat.X500);
			return SearchConverters.ToSearchFromADEntityToAttributeValue<ADResourcePropertyFactory<ADResourceProperty>, ADResourceProperty>(str, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchFromSchemaObjectNameToDN(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			return SearchConverters.ToSearchFromADEntityToAttributeValue<ADSchemaObjectFactory<ADSchemaObject>, ADSchemaObject>(cmdletSessionInfo.ADRootDSE.SchemaNamingContext, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static IADOPathNode ToSearchFromTrustNameToDN(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo)
		{
			string wellKnownGuidDN = Utils.GetWellKnownGuidDN(cmdletSessionInfo.ADSessionInfo, cmdletSessionInfo.ADRootDSE.DefaultNamingContext, WellKnownGuids.SystemsContainerGuid);
			return SearchConverters.ToSearchFromADEntityToAttributeValue<ADTrustFactory<ADTrust>, ADTrust>(wellKnownGuidDN, null, extendedAttributeName, directoryAttributes, filterClause, cmdletSessionInfo);
		}

		internal static void ValidateClaimID(string claimID)
		{
			if (claimID.Length > ADCBACUtil.MaxClaimIDLength || claimID.Length < ADCBACUtil.MinClaimIDLength)
			{
				throw new ADException(StringResources.ClaimIDValidationError);
			}
			else
			{
				if (string.Compare(ADCBACUtil.ClaimIDPrefix, 0, claimID, 0, ADCBACUtil.ClaimIDPrefix.Length, StringComparison.OrdinalIgnoreCase) == 0)
				{
					int num = claimID.IndexOf('/', ADCBACUtil.ClaimIDPrefix.Length);
					while (num != -1)
					{
						if (num == 0 || ADCBACUtil.InvalidClaimIDCharacters.Contains(claimID[num - 1]))
						{
							throw new ADException(StringResources.ClaimIDValidationError);
						}
						else
						{
							if (claimID.Length == num + 1 || ADCBACUtil.InvalidClaimIDCharacters.Contains(claimID[num + 1]))
							{
								throw new ADException(StringResources.ClaimIDValidationError);
							}
							else
							{
								num = claimID.IndexOf('/', num + 1);
							}
						}
					}
					string str = claimID.Replace(ADCBACUtil.ColonDelimeter.ToString(), "");
					str = str.Replace(ADCBACUtil.SlashDelimeter.ToString(), "");
					if (-1 == str.IndexOfAny(ADCBACUtil.InvalidClaimIDCharacters.ToArray()))
					{
						return;
					}
					else
					{
						throw new ADException(StringResources.ClaimIDValidationError);
					}
				}
				else
				{
					throw new ADException(StringResources.ClaimIDValidationError);
				}
			}
		}

		internal static void ValidateResourceID(string resourceID)
		{
			char[] resourceIDDelimeter = new char[1];
			resourceIDDelimeter[0] = ADCBACUtil.ResourceIDDelimeter;
			string[] strArrays = resourceID.Split(resourceIDDelimeter);
			if ((int)strArrays.Length != 2 || strArrays[0].Length > ADCBACUtil.ClaimIDPrefixLength || strArrays[0].Length == 0 || strArrays[1].Length > ADCBACUtil.ClaimIDSuffixLength || strArrays[1].Length == 0)
			{
				throw new ADException(StringResources.ResourceIDValidationError);
			}
			else
			{
				string[] strArrays1 = strArrays;
				int num = 0;
				while (num < (int)strArrays1.Length)
				{
					string str = strArrays1[num];
					if (-1 == str.IndexOfAny(ADCBACUtil.InvalidClaimIDCharacters.ToArray()))
					{
						num++;
					}
					else
					{
						throw new ADException(StringResources.ResourceIDValidationError);
					}
				}
				return;
			}
		}

		private static void VerifyADSuggestedValuesXml(string suggestedValuesXmlBlob)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Stream manifestResourceStream = executingAssembly.GetManifestResourceStream("ADClaimPossibleValues.xsd");
			if (manifestResourceStream == null)
			{
				throw new ADException("Couldn't get ADClaimPossibleValues.xsd from resources");
			}
			else
			{
				using (manifestResourceStream)
				{
					XmlTextReader xmlTextReader = new XmlTextReader(manifestResourceStream);
					XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
					xmlReaderSetting.ValidationType = ValidationType.Schema;
					xmlReaderSetting.Schemas.Add(null, xmlTextReader);
					xmlReaderSetting.CloseInput = true;
					XmlReader xmlReader = XmlReader.Create(new StringReader(suggestedValuesXmlBlob), xmlReaderSetting);
					using (xmlReader)
					{
						while (xmlReader.Read())
						{
						}
					}
				}
				return;
			}
		}

		internal static bool VerifyAndSetPossibleValues(ADObject directoryObj, ADClaimValueType valueType, StringBuilder errorBuffer)
		{
			bool flag = false;
			if (directoryObj.InternalProperties.Contains("SuggestedValues") && directoryObj.InternalProperties["SuggestedValues"].Value != null)
			{
				if (directoryObj.Contains("msDS-ClaimPossibleValues") && directoryObj["msDS-ClaimPossibleValues"].Value != null)
				{
					bool flag1 = false;
					try
					{
						ADCBACUtil.ConvertSuggestedValueXmlToSuggestedValueEntryList(directoryObj["msDS-ClaimPossibleValues"].Value as string, out flag1);
						if (flag1)
						{
							flag = true;
							object[] value = new object[2];
							value[0] = directoryObj["name"].Value as string;
							value[1] = "msDS-ClaimPossibleValues";
							errorBuffer.AppendLine(string.Format(CultureInfo.CurrentCulture, StringResources.CannotOverwriteNextVersionXml, value));
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						if (exception as XmlException == null && exception as XmlSchemaException == null && exception as InvalidOperationException == null)
						{
							throw;
						}
					}
				}
				if (!flag)
				{
					ADSuggestedValueEntry[] valueList = directoryObj.InternalProperties["SuggestedValues"].ValueList as ADSuggestedValueEntry[];
					ADCBACUtil.AddSuggestedValueXmlFromADSuggestedValueEntryList(valueList, directoryObj, valueType);
				}
			}
			return !flag;
		}
	}
}