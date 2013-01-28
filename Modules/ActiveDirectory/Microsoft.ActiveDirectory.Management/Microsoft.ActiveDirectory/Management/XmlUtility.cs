using Microsoft.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal class XmlUtility
	{
		private XmlUtility()
		{
		}

		public static string AddPrefix(string prefix, string element)
		{
			object[] objArray = new object[2];
			objArray[0] = prefix;
			objArray[1] = element;
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", objArray);
		}

		public static string ConvertADAttributeValToXml(object value)
		{
			if (value as byte[] == null)
			{
				return (string)value;
			}
			else
			{
				return Convert.ToBase64String((byte[])value);
			}
		}

		public static string DeserializeEunmerationContext(XmlDictionaryReader reader)
		{
			if (!reader.IsStartElement("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
			{
				return null;
			}
			else
			{
				return reader.ReadElementString("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
			}
		}

		public static void DeserializeExpiresIfNeeded(XmlDictionaryReader reader, ref DateTime? dateTimeFormat, ref TimeSpan? timeSpanFormat)
		{
			if (reader.IsStartElement("Expires", "http://schemas.xmlsoap.org/ws/2004/09/enumeration"))
			{
				string str = reader.ReadElementString();
				try
				{
					if (!str.StartsWith("P", StringComparison.OrdinalIgnoreCase))
					{
						dateTimeFormat = new DateTime?(XmlConvert.ToDateTime(str, XmlDateTimeSerializationMode.Utc));
					}
					else
					{
						timeSpanFormat = new TimeSpan?(XmlConvert.ToTimeSpan(str));
					}
				}
				catch (RemotingException remotingException)
				{
					throw;
				}
			}
		}

		public static bool DeserializeLdapAttributeOption(XmlReader reader, ref string ldapAttribute)
		{
			if (reader.AttributeCount > 1)
			{
				string attribute = reader.GetAttribute("RangeLow", string.Empty);
				if (!string.IsNullOrEmpty(attribute))
				{
					string str = reader.GetAttribute("RangeHigh", string.Empty);
					if (string.IsNullOrEmpty(str))
					{
						throw new ADException();
					}
					else
					{
						ldapAttribute = string.Format(LdapOptionConstants.RangeOptionFormatString, ldapAttribute, attribute, str);
						return true;
					}
				}
			}
			return false;
		}

		public static void DeserializeObjectReference(XmlReader reader, out string objectReference)
		{
			if (reader.IsStartElement("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory"))
			{
				string str = reader.ReadString();
				string str1 = str;
				objectReference = str;
				if (str1 != null)
				{
					reader.Read();
					return;
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = "ad";
					objArray[1] = "objectReferenceProperty";
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADWSXmlParserInvalidelement, objArray));
				}
			}
			else
			{
				object[] objArray1 = new object[2];
				objArray1[0] = "ad";
				objArray1[1] = "objectReferenceProperty";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ADWSXmlParserInvalidelement, objArray1));
			}
		}

		public static void MarkHeaderAsUnderstood(MessageHeaders headers, string localName, string ns)
		{
			int num = headers.FindHeader(localName, ns);
			if (-1 != num)
			{
				if (!headers.UnderstoodHeaders.Contains(headers[num]))
				{
					headers.UnderstoodHeaders.Add(headers[num]);
				}
				return;
			}
			else
			{
				return;
			}
		}

		public static string RemovePrefix(string prefix, string element)
		{
			if (!string.IsNullOrEmpty(element))
			{
				string str = string.Concat(prefix, ":");
				if (element.StartsWith(str, StringComparison.Ordinal))
				{
					return element.Substring(str.Length);
				}
			}
			return element;
		}

		public static void SerializeAttributeList(XmlDictionaryWriter writer, string xmlAttribute, string ns, string syntheticPrefix, string attrPrefix, IList<string> attributes)
		{
			string str;
			bool flag = false;
			foreach (string attribute in attributes)
			{
				if (!AttributeNs.IsSynthetic(attribute, SyntheticAttributeOperation.Read, ref flag))
				{
					string str1 = null;
					string str2 = null;
					if (!XmlUtility.SplitLdapAttributeOnOption(attribute, ref str1, ref str2))
					{
						writer.WriteElementString(xmlAttribute, ns, XmlUtility.AddPrefix(attrPrefix, attribute));
					}
					else
					{
						writer.WriteStartElement(xmlAttribute, ns);
						XmlUtility.SerializeLdapAttributeOption(writer, str2);
						writer.WriteValue(XmlUtility.AddPrefix(attrPrefix, str1));
						writer.WriteEndElement();
					}
				}
				else
				{
					XmlDictionaryWriter xmlDictionaryWriter = writer;
					string str3 = xmlAttribute;
					string str4 = ns;
					if (flag)
					{
						str = attribute;
					}
					else
					{
						str = XmlUtility.AddPrefix(syntheticPrefix, attribute);
					}
					xmlDictionaryWriter.WriteElementString(str3, str4, str);
				}
			}
		}

		public static void SerializeEnumerationContext(XmlDictionaryWriter writer, string context)
		{
			writer.WriteElementString("EnumerationContext", "http://schemas.xmlsoap.org/ws/2004/09/enumeration", context);
		}

		public static void SerializeExpires(XmlDictionaryWriter writer, DateTime? dateTimeFormat, TimeSpan? timeSpanFormat)
		{
			if (dateTimeFormat.HasValue || timeSpanFormat.HasValue)
			{
				writer.WriteStartElement("Expires", "http://schemas.xmlsoap.org/ws/2004/09/enumeration");
				if (!dateTimeFormat.HasValue)
				{
					writer.WriteValue(XmlConvert.ToString(timeSpanFormat.Value));
				}
				else
				{
					writer.WriteValue(XmlConvert.ToString(dateTimeFormat.Value, XmlDateTimeSerializationMode.Utc));
				}
				writer.WriteEndElement();
			}
		}

		public static void SerializeLdapAttributeOption(XmlDictionaryWriter writer, string ldapOptions)
		{
			Match match = LdapOptionConstants.RangeOptionRegex.Match(ldapOptions);
			if (!match.Success || !match.Groups[LdapOptionConstants.HighRangeIndex].Success || !match.Groups[LdapOptionConstants.LowRangeIndex].Success)
			{
				throw new ArgumentException("Invalid Format");
			}
			else
			{
				writer.WriteAttributeString("RangeLow", string.Empty, match.Groups[LdapOptionConstants.LowRangeIndex].Value);
				writer.WriteAttributeString("RangeHigh", string.Empty, match.Groups[LdapOptionConstants.HighRangeIndex].Value);
				return;
			}
		}

		public static bool SplitLdapAttributeOnOption(string ldapAttribute, ref string splitAttribute, ref string splitOption)
		{
			int num = ldapAttribute.IndexOf(LdapOptionConstants.LdapOptionSeperator, StringComparison.Ordinal);
			if (-1 != num)
			{
				splitAttribute = ldapAttribute.Substring(0, num);
				splitOption = ldapAttribute.Substring(num + 1);
				if (!string.IsNullOrEmpty(splitOption))
				{
					return true;
				}
				else
				{
					throw new ArgumentException("Invalid Format");
				}
			}
			else
			{
				return false;
			}
		}

		public static void SplitPrefix(string name, out string prefix, out string localName)
		{
			char[] chrArray = new char[1];
			chrArray[0] = ':';
			string[] strArrays = name.Split(chrArray);
			if ((int)strArrays.Length != 1)
			{
				prefix = strArrays[0];
				localName = strArrays[1];
				return;
			}
			else
			{
				localName = strArrays[0];
				prefix = null;
				return;
			}
		}

		public static bool ValidateNamespace(XmlReader reader, string value, string ns)
		{
			char[] chrArray = new char[1];
			chrArray[0] = ':';
			string[] strArrays = value.Split(chrArray);
			if ((int)strArrays.Length != 1)
			{
				string str = strArrays[0];
				return string.Equals(reader.LookupNamespace(str), ns, StringComparison.Ordinal);
			}
			else
			{
				return 0 == reader.NamespaceURI.CompareTo(ns);
			}
		}

		public static void WriteXsiTypeAttribute(XmlDictionaryWriter writer, string xsdType)
		{
			string str = writer.LookupPrefix("http://www.w3.org/2001/XMLSchema");
			object[] objArray = new object[2];
			objArray[0] = str;
			objArray[1] = xsdType;
			writer.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", string.Format(CultureInfo.CurrentCulture, "{0}:{1}", objArray));
		}
	}
}