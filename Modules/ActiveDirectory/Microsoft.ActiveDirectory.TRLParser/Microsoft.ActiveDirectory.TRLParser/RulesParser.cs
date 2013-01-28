using Microsoft.ActiveDirectory.TRLParser.LanguageParser;
using Microsoft.ActiveDirectory.TRLParser.LanguageParser.Diagnostics;
using System;
using System.Xml;

namespace Microsoft.ActiveDirectory.TRLParser
{
	public class RulesParser
	{
		private const string RulesXmlRootNode = "ClaimsTransformationPolicy";

		private const string RulesXmlSubNode = "Rules";

		private const string RulesXmlVersionAttribute = "version";

		private const string RulesXmlVersionValue = "1";

		public RulesParser()
		{

		}

		[CLSCompliant(false)]
		public void ParseRules(string rulesXml, out PolicyRuleSetStruct policyRuleSetStruct)
		{
			string str = null;
			XmlNode xmlNodes = null;
			bool flag = false;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(rulesXml);
			if (xmlDocument.DocumentElement.Name == null || string.Compare(xmlDocument.DocumentElement.Name, "ClaimsTransformationPolicy", StringComparison.OrdinalIgnoreCase) != 0)
			{
				object[] name = new object[2];
				name[0] = xmlDocument.DocumentElement.Name;
				name[1] = "ClaimsTransformationPolicy";
				str = SR.GetString("POLICY0400", name);
				DebugLog.PolicyEngineTraceLog.ErrorSafe(string.Concat("Error:", str), new object[0]);
				throw new XmlParseException(str);
			}
			else
			{
				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("Rules");
				if (elementsByTagName != null)
				{
					foreach (XmlNode xmlNodes1 in elementsByTagName)
					{
						XmlAttribute namedItem = (XmlAttribute)xmlNodes1.Attributes.GetNamedItem("version");
						if (namedItem == null || string.Compare(namedItem.Value, "1", StringComparison.OrdinalIgnoreCase) != 0 || xmlNodes1.FirstChild == null || xmlNodes1.FirstChild.NodeType != XmlNodeType.CDATA)
						{
							continue;
						}
						flag = true;
						xmlNodes = xmlNodes1;
						break;
					}
					if (flag)
					{
						if (xmlNodes == null || xmlNodes.FirstChild.Value == null)
						{
							object[] objArray = new object[3];
							objArray[0] = "Rules";
							objArray[1] = "version";
							objArray[2] = "1";
							str = SR.GetString("POLICY0403", objArray);
							str = string.Concat(str, SR.GetString("INFO0001", new object[0]));
							DebugLog.PolicyEngineTraceLog.ErrorSafe(string.Concat("Error:", str), new object[0]);
							throw new XmlParseException(str);
						}
						else
						{
							string value = xmlNodes.FirstChild.Value;
							PolicyRuleSet policyRuleSet = new PolicyRuleSet();
							try
							{
								policyRuleSet.Initialize(value);
								DebugLog.PolicyEngineTraceLog.InfoSafe(string.Concat("Parsed rules:", policyRuleSet.ToString()), new object[0]);
								policyRuleSetStruct = policyRuleSet.GetStruct();
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								str = string.Concat(exception.Message, SR.GetString("INFO0001", new object[0]));
								DebugLog.PolicyEngineTraceLog.ErrorSafe(string.Concat("Error:", str), new object[0]);
								throw new Exception(str, exception);
							}
							return;
						}
					}
					else
					{
						object[] objArray1 = new object[3];
						objArray1[0] = "Rules";
						objArray1[1] = "version";
						objArray1[2] = "1";
						str = SR.GetString("POLICY0402", objArray1);
						str = string.Concat(str, SR.GetString("INFO0001", new object[0]));
						DebugLog.PolicyEngineTraceLog.ErrorSafe(string.Concat("Error:", str), new object[0]);
						throw new XmlParseException(str);
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = "Rules";
					str = SR.GetString("POLICY0401", objArray2);
					DebugLog.PolicyEngineTraceLog.ErrorSafe(string.Concat("Error:", str), new object[0]);
					throw new XmlParseException(str);
				}
			}
		}

		public void ValidateRules(string rulesXml)
		{
			PolicyRuleSetStruct policyRuleSetStruct;
			this.ParseRules(rulesXml, out policyRuleSetStruct);
		}
	}
}