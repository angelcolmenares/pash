using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.TRLParser;
using Microsoft.ActiveDirectory.TRLParser.LanguageParser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADClaimTransformPolicyFactory<T> : ADObjectFactory<T>
	where T : ADClaimTransformPolicy, new()
	{
		private static string RuleXml;

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
				return ADClaimTransformPolicyFactory<T>._identityResolvers;
			}
		}

		internal override string RDNPrefix
		{
			get
			{
				return ADClaimTransformPolicyFactory<T>._rDNPrefix;
			}
		}

		internal override string StructuralObjectClass
		{
			get
			{
				return "msDS-ClaimsTransformationPolicyType";
			}
		}

		internal override IADOPathNode StructuralObjectFilter
		{
			get
			{
				return ADOPathUtil.CreateFilterClause(ADOperator.Eq, "objectClass", this.StructuralObjectClass);
			}
		}

		static ADClaimTransformPolicyFactory()
		{
			ADClaimTransformPolicyFactory<T>.RuleXml = " <ClaimsTransformationPolicy>     <Rules version=\"1\">         <![CDATA[{0}]]>    </Rules></ClaimsTransformationPolicy>";
			ADClaimTransformPolicyFactory<T>._rDNPrefix = "CN";
			string[] strArrays = new string[1];
			strArrays[0] = "name";
			ADClaimTransformPolicyFactory<T>._identityLdapAttributes = strArrays;
			IdentityResolverDelegate[] customIdentityResolver = new IdentityResolverDelegate[2];
			customIdentityResolver[0] = IdentityResolverMethods.GetCustomIdentityResolver(new IdentityResolverDelegate(IdentityResolverMethods.DistinguishedNameIdentityResolver));
			IdentityResolverDelegate[] genericIdentityResolver = new IdentityResolverDelegate[2];
			genericIdentityResolver[0] = IdentityResolverMethods.GetGenericIdentityResolver(ADClaimTransformPolicyFactory<T>._identityLdapAttributes);
			genericIdentityResolver[1] = new IdentityResolverDelegate(IdentityResolverMethods.GuidSearchFilterIdentityResolver);
			customIdentityResolver[1] = IdentityResolverMethods.GetAggregatedIdentityResolver(ADOperator.Or, genericIdentityResolver);
			ADClaimTransformPolicyFactory<T>._identityResolvers = customIdentityResolver;
			AttributeConverterEntry[] attributeConverterEntry = new AttributeConverterEntry[3];
			attributeConverterEntry[0] = new AttributeConverterEntry(ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.IncomingTrust.PropertyName, ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.IncomingTrust.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(ADCBACUtil.ToSearchFromTrustNameToDN));
			attributeConverterEntry[1] = new AttributeConverterEntry(ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.OutgoingTrust.PropertyName, ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.OutgoingTrust.ADAttribute, TypeConstants.String, false, TypeAdapterAccess.Read, false, AttributeSet.Default, new ToExtendedFormatDelegate(AttributeConverters.ToExtendedMultivalueObject), null, new ToSearchFilterDelegate(ADCBACUtil.ToSearchFromTrustNameToDN));
			attributeConverterEntry[2] = new AttributeConverterEntry(ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.Rule.PropertyName, ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.Rule.ADAttribute, TypeConstants.String, true, TypeAdapterAccess.ReadWrite, true, AttributeSet.Default, new ToExtendedFormatDelegate(ADClaimTransformPolicyFactory<T>.ToPolicyRuleStringFromRuleXml), new ToDirectoryFormatDelegate(ADClaimTransformPolicyFactory<T>.ToRuleXmlFromPolicyString), new ToSearchFilterDelegate(SearchConverters.ToSearchNotSupported));
			ADClaimTransformPolicyFactory<T>.ADMappingTable = attributeConverterEntry;
			ADFactoryBase<T>.RegisterMappingTable(ADClaimTransformPolicyFactory<T>.ADMappingTable, ADServerType.ADDS);
		}

		public ADClaimTransformPolicyFactory()
		{
		}

		private static string ParseRules(string RulesXml)
		{
			XmlNode xmlNodes = null;
			bool flag = false;
			string value = null;
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(RulesXml);
				if (string.Compare(xmlDocument.DocumentElement.Name, "ClaimsTransformationPolicy", StringComparison.OrdinalIgnoreCase) == 0)
				{
					XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Rules");
					foreach (XmlNode xmlNodes1 in elementsByTagName)
					{
						XmlAttribute namedItem = (XmlAttribute)xmlNodes1.Attributes.GetNamedItem("version");
						if (string.Compare(namedItem.Value, "1", StringComparison.OrdinalIgnoreCase) != 0 || xmlNodes1.FirstChild.NodeType != XmlNodeType.CDATA)
						{
							continue;
						}
						flag = true;
						xmlNodes = xmlNodes1;
						break;
					}
					if (flag)
					{
						if (xmlNodes != null)
						{
							value = xmlNodes.FirstChild.Value;
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = "RuleNode";
							throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ClaimPolicyXmlNodeError, objArray));
						}
					}
					else
					{
						object[] objArray1 = new object[1];
						objArray1[0] = "version=1";
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ClaimPolicyXmlNodeError, objArray1));
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "ClaimsTransformationPolicy";
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.ClaimPolicyXmlNodeError, objArray2));
				}
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw xmlException;
			}
			return value;
		}

		internal static void ToPolicyRuleStringFromRuleXml(string extendedAttribute, string[] directoryAttributes, ADEntity userObj, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (!directoryObj.Contains(directoryAttributes[0]))
			{
				userObj.Add(extendedAttribute, new ADPropertyValueCollection());
			}
			else
			{
				try
				{
					string str = ADClaimTransformPolicyFactory<T>.ParseRules(directoryObj[directoryAttributes[0]].Value as string);
					ADPropertyValueCollection aDPropertyValueCollection = new ADPropertyValueCollection(str);
					userObj.Add(extendedAttribute, aDPropertyValueCollection);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (exception as ADException != null || exception as XmlException != null)
					{
						object[] value = new object[3];
						value[0] = directoryObj["distinguishedName"].Value;
						value[1] = exception.Message;
						value[2] = directoryObj[directoryAttributes[0]].Value;
						cmdletSessionInfo.CmdletMessageWriter.WriteWarningBuffered(string.Format(CultureInfo.CurrentCulture, StringResources.ClaimPolicyXmlWarning, value));
						userObj.Add(extendedAttribute, new ADPropertyValueCollection());
					}
					else
					{
						throw exception;
					}
				}
			}
		}

		internal static void ToRuleXmlFromPolicyString(string extendedAttribute, string[] directoryAttributes, ADPropertyValueCollection extendedData, ADEntity directoryObj, CmdletSessionInfo cmdletSessionInfo)
		{
			if (extendedData == null || extendedData.Value == null)
			{
				directoryObj.ForceRemove(directoryAttributes[0]);
				return;
			}
			else
			{
				object[] value = new object[1];
				value[0] = extendedData.Value;
				string str = string.Format(CultureInfo.InvariantCulture, ADClaimTransformPolicyFactory<T>.RuleXml, value);
				RulesParser rulesParser = new RulesParser();
				try
				{
					rulesParser.ValidateRules(str);
				}
				catch (XmlParseException xmlParseException1)
				{
					XmlParseException xmlParseException = xmlParseException1;
					object[] message = new object[2];
					message[0] = extendedData.Value;
					message[1] = xmlParseException.Message;
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.XmlFormattingError, message));
				}
				catch (PolicyValidationException policyValidationException1)
				{
					PolicyValidationException policyValidationException = policyValidationException1;
					object[] objArray = new object[2];
					objArray[0] = extendedData.Value;
					objArray[1] = policyValidationException.Message;
					throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.RuleValidationFailure, objArray));
				}
				directoryObj[directoryAttributes[0]].Value = str;
				return;
			}
		}

		internal static class ADClaimTransformPolicyPropertyMap
		{
			public readonly static PropertyMapEntry Rule;

			public readonly static PropertyMapEntry IncomingTrust;

			public readonly static PropertyMapEntry OutgoingTrust;

			static ADClaimTransformPolicyPropertyMap()
			{
				ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.Rule = new PropertyMapEntry("Rule", "msDS-TransformationRules", "msDS-TransformationRules");
				ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.IncomingTrust = new PropertyMapEntry("IncomingTrust", "msDS-TDOEgressBL", "msDS-TDOEgressBL");
				ADClaimTransformPolicyFactory<T>.ADClaimTransformPolicyPropertyMap.OutgoingTrust = new PropertyMapEntry("OutgoingTrust", "msDS-TDOIngressBL", "msDS-TDOIngressBL");
			}
		}
	}
}