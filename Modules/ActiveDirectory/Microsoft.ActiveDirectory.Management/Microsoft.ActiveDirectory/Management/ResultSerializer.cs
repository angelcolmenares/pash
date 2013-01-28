using Microsoft.ActiveDirectory;
using System;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management
{
	internal sealed class ResultSerializer
	{
		public ResultSerializer()
		{
		}

		public static void Deserialize(XmlReader reader, ResultSerializer.ResultDialect dialect, out ADWSResultEntry entry)
		{
			if (dialect == ResultSerializer.ResultDialect.WSEnumeration || dialect == ResultSerializer.ResultDialect.WSTransfer)
			{
				ResultSerializer.DeserializeFullResult(reader, dialect, out entry);
				return;
			}
			else
			{
				ResultSerializer.DeserializePartialAttribute(reader, dialect, out entry);
				return;
			}
		}

		private static void DeserializeAttributeElement(XmlReader reader, ResultSerializer.ResultDialect dialect, ADWSResultEntry entry)
		{
			string str = null;
			string str1 = null;
			string str2 = null;
			string str3 = null;
			bool flag = false;
			if (string.IsNullOrEmpty(reader.Prefix))
			{
				if (!string.Equals(reader.NamespaceURI, "http://schemas.microsoft.com/2008/1/ActiveDirectory", StringComparison.Ordinal))
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				string str4 = reader.LookupNamespace(reader.Prefix);
				if (!string.Equals(str4, "http://schemas.microsoft.com/2008/1/ActiveDirectory", StringComparison.Ordinal))
				{
					flag = false;
					//reader.Prefix;
				}
				else
				{
					flag = true;
					//reader.Prefix;
				}
			}
			if (!flag)
			{
				bool flag1 = false;
				string str5 = string.Intern(reader.LocalName);
				if (str5.Equals("objectClass", StringComparison.OrdinalIgnoreCase))
				{
					flag1 = true;
				}
				XmlUtility.DeserializeLdapAttributeOption(reader, ref str5);
				object obj = null;
				reader.Read();
				ADValueSerializer.Deserialize(reader, flag1, out obj);
				entry.DirObject.SetValue(str5, new ADPropertyValueCollection(obj));
				reader.ReadEndElement();
				return;
			}
			else
			{
				string localName = reader.LocalName;
				if (!string.Equals(localName, "container-hierarchy-parent", StringComparison.Ordinal))
				{
					if (!string.Equals(localName, "distinguishedName", StringComparison.Ordinal))
					{
						if (!string.Equals(localName, "relativeDistinguishedName", StringComparison.Ordinal))
						{
							if (!string.Equals(localName, "objectReferenceProperty", StringComparison.Ordinal))
							{
								throw new ArgumentException(string.Format(StringResources.ADWSXmlParserInvalidAttribute, localName));
							}
							else
							{
								reader.ReadStartElement("objectReferenceProperty", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
								ADValueSerializer.DeserializeSingleValue<string>(reader, out str3);
								entry.ObjectReferenceProperty = str3;
								reader.ReadEndElement();
								return;
							}
						}
						else
						{
							reader.ReadStartElement("relativeDistinguishedName", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
							ADValueSerializer.DeserializeSingleValue<string>(reader, out str2);
							entry.RelativeDistinguishedName = str2;
							reader.ReadEndElement();
							return;
						}
					}
					else
					{
						reader.ReadStartElement("distinguishedName", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
						ADValueSerializer.DeserializeSingleValue<string>(reader, out str1);
						entry.DistinguishedName = str1;
						reader.ReadEndElement();
						return;
					}
				}
				else
				{
					reader.ReadStartElement("container-hierarchy-parent", "http://schemas.microsoft.com/2008/1/ActiveDirectory");
					ADValueSerializer.DeserializeSingleValue<string>(reader, out str);
					entry.ParentContainer = str;
					reader.ReadEndElement();
					return;
				}
			}
		}

		private static void DeserializeFullResult(XmlReader reader, ResultSerializer.ResultDialect dialect, out ADWSResultEntry entry)
		{
			entry = new ADWSResultEntry();
			reader.Read();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				ResultSerializer.DeserializeAttributeElement(reader, dialect, entry);
			}
			reader.ReadEndElement();
		}

		private static void DeserializePartialAttribute(XmlReader reader, ResultSerializer.ResultDialect dialect, out ADWSResultEntry entry)
		{
			entry = new ADWSResultEntry();
			while (reader.IsStartElement("PartialAttribute", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess"))
			{
				if (reader.IsEmptyElement)
				{
					reader.Read();
				}
				else
				{
					reader.Read();
					if (reader.NodeType != XmlNodeType.EndElement)
					{
						ResultSerializer.DeserializeAttributeElement(reader, dialect, entry);
					}
					reader.ReadEndElement();
				}
			}
		}

		public enum ResultDialect
		{
			XPath1 = 0,
			WSTransfer = 3,
			WSEnumeration = 4
		}
	}
}