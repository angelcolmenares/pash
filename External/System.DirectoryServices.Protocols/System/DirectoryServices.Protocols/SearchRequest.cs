using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class SearchRequest : DirectoryRequest
	{
		private string dn;

		private StringCollection directoryAttributes;

		private object directoryFilter;

		private SearchScope directoryScope;

		private DereferenceAlias directoryRefAlias;

		private int directorySizeLimit;

		private TimeSpan directoryTimeLimit;

		private bool directoryTypesOnly;

		public DereferenceAlias Aliases
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryRefAlias;
			}
			set
			{
				if (value < DereferenceAlias.Never || value > DereferenceAlias.Always)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(DereferenceAlias));
				}
				else
				{
					this.directoryRefAlias = value;
					return;
				}
			}
		}

		public StringCollection Attributes
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryAttributes;
			}
		}

		public string DistinguishedName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dn;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.dn = value;
			}
		}

		public object Filter
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryFilter;
			}
			set
			{
				if (value as string != null || value as XmlDocument != null || value == null)
				{
					this.directoryFilter = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidFilterType"), "value");
				}
			}
		}

		public SearchScope Scope
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryScope;
			}
			set
			{
				if (value < SearchScope.Base || value > SearchScope.Subtree)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(SearchScope));
				}
				else
				{
					this.directoryScope = value;
					return;
				}
			}
		}

		public int SizeLimit
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directorySizeLimit;
			}
			set
			{
				if (value >= 0)
				{
					this.directorySizeLimit = value;
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeSizeLimit"), "value");
				}
			}
		}

		public TimeSpan TimeLimit
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryTimeLimit;
			}
			set
			{
				if (value >= TimeSpan.Zero)
				{
					if (value.TotalSeconds <= 2147483647)
					{
						this.directoryTimeLimit = value;
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
				}
			}
		}

		public bool TypesOnly
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.directoryTypesOnly;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.directoryTypesOnly = value;
			}
		}

		public SearchRequest()
		{
			this.directoryAttributes = new StringCollection();
			this.directoryScope = SearchScope.Subtree;
			this.directoryTimeLimit = new TimeSpan((long)0);
			this.directoryAttributes = new StringCollection();
		}

		public SearchRequest(string distinguishedName, XmlDocument filter, SearchScope searchScope, string[] attributeList) : this()
		{
			this.dn = distinguishedName;
			if (attributeList != null)
			{
				for (int i = 0; i < (int)attributeList.Length; i++)
				{
					this.directoryAttributes.Add(attributeList[i]);
				}
			}
			this.Scope = searchScope;
			this.Filter = filter;
		}

		public SearchRequest(string distinguishedName, string ldapFilter, SearchScope searchScope, string[] attributeList) : this()
		{
			this.dn = distinguishedName;
			if (attributeList != null)
			{
				for (int i = 0; i < (int)attributeList.Length; i++)
				{
					this.directoryAttributes.Add(attributeList[i]);
				}
			}
			this.Scope = searchScope;
			this.Filter = ldapFilter;
		}

		private void CopyFilter(XmlNode node, XmlTextWriter writer)
		{
			for (XmlNode i = node.FirstChild; i != null; i = i.NextSibling)
			{
				if (i != null)
				{
					this.CopyXmlTree(i, writer);
				}
			}
		}

		private void CopyXmlTree(XmlNode node, XmlTextWriter writer)
		{
			XmlNodeType nodeType = node.NodeType;
			if (nodeType != XmlNodeType.Element)
			{
				writer.WriteRaw(node.OuterXml);
				return;
			}
			else
			{
				writer.WriteStartElement(node.LocalName, "urn:oasis:names:tc:DSML:2:0:core");
				foreach (XmlAttribute attribute in node.Attributes)
				{
					writer.WriteAttributeString(attribute.LocalName, attribute.Value);
				}
				for (XmlNode i = node.FirstChild; i != null; i = i.NextSibling)
				{
					this.CopyXmlTree(i, writer);
				}
				writer.WriteEndElement();
				return;
			}
		}

		protected override XmlElement ToXmlNode(XmlDocument doc)
		{
			string str;
			XmlElement xmlElement = base.CreateRequestElement(doc, "searchRequest", true, this.dn);
			XmlAttribute xmlAttribute = doc.CreateAttribute("scope", null);
			SearchScope searchScope = this.directoryScope;
			switch (searchScope)
			{
				case SearchScope.Base:
				{
					xmlAttribute.InnerText = "baseObject";
					break;
				}
				case SearchScope.OneLevel:
				{
					xmlAttribute.InnerText = "singleLevel";
					break;
				}
				case SearchScope.Subtree:
				{
					xmlAttribute.InnerText = "wholeSubtree";
					break;
				}
			}
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute1 = doc.CreateAttribute("derefAliases", null);
			DereferenceAlias dereferenceAlia = this.directoryRefAlias;
			switch (dereferenceAlia)
			{
				case DereferenceAlias.Never:
				{
					xmlAttribute1.InnerText = "neverDerefAliases";
					break;
				}
				case DereferenceAlias.InSearching:
				{
					xmlAttribute1.InnerText = "derefInSearching";
					break;
				}
				case DereferenceAlias.FindingBaseObject:
				{
					xmlAttribute1.InnerText = "derefFindingBaseObj";
					break;
				}
				case DereferenceAlias.Always:
				{
					xmlAttribute1.InnerText = "derefAlways";
					break;
				}
			}
			xmlElement.Attributes.Append(xmlAttribute1);
			XmlAttribute str1 = doc.CreateAttribute("sizeLimit", null);
			str1.InnerText = this.directorySizeLimit.ToString(CultureInfo.InvariantCulture);
			xmlElement.Attributes.Append(str1);
			XmlAttribute str2 = doc.CreateAttribute("timeLimit", null);
			long ticks = this.directoryTimeLimit.Ticks / (long)0x989680;
			str2.InnerText = ticks.ToString(CultureInfo.InvariantCulture);
			xmlElement.Attributes.Append(str2);
			XmlAttribute xmlAttribute2 = doc.CreateAttribute("typesOnly", null);
			XmlAttribute xmlAttribute3 = xmlAttribute2;
			if (this.directoryTypesOnly)
			{
				str = "true";
			}
			else
			{
				str = "false";
			}
			xmlAttribute3.InnerText = str;
			xmlElement.Attributes.Append(xmlAttribute2);
			XmlElement outerXml = doc.CreateElement("filter", "urn:oasis:names:tc:DSML:2:0:core");
			if (this.Filter == null)
			{
				outerXml.InnerXml = "<present name='objectClass' xmlns=\"urn:oasis:names:tc:DSML:2:0:core\"/>";
			}
			else
			{
				StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
				XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
				try
				{
					if (this.Filter as XmlDocument == null)
					{
						if (this.Filter as string != null)
						{
							string filter = (string)this.Filter;
							if (!filter.StartsWith("(", StringComparison.Ordinal) && !filter.EndsWith(")", StringComparison.Ordinal))
							{
								filter = filter.Insert(0, "(");
								filter = string.Concat(filter, ")");
							}
							ADFilter aDFilter = FilterParser.ParseFilterString(filter);
							if (aDFilter != null)
							{
								DSMLFilterWriter dSMLFilterWriter = new DSMLFilterWriter();
								dSMLFilterWriter.WriteFilter(aDFilter, false, xmlTextWriter, "urn:oasis:names:tc:DSML:2:0:core");
								outerXml.InnerXml = stringWriter.ToString();
							}
							else
							{
								throw new ArgumentException(Res.GetString("BadSearchLDAPFilter"));
							}
						}
					}
					else
					{
						if (((XmlDocument)this.Filter).NamespaceURI.Length != 0)
						{
							outerXml.InnerXml = ((XmlDocument)this.Filter).OuterXml;
						}
						else
						{
							this.CopyFilter((XmlDocument)this.Filter, xmlTextWriter);
							outerXml.InnerXml = stringWriter.ToString();
						}
					}
				}
				finally
				{
					xmlTextWriter.Close();
				}
			}
			xmlElement.AppendChild(outerXml);
			if (this.directoryAttributes != null && this.directoryAttributes.Count != 0)
			{
				XmlElement xmlElement1 = doc.CreateElement("attributes", "urn:oasis:names:tc:DSML:2:0:core");
				xmlElement.AppendChild(xmlElement1);
				foreach (string directoryAttribute in this.directoryAttributes)
				{
					DirectoryAttribute directoryAttribute1 = new DirectoryAttribute();
					directoryAttribute1.Name = directoryAttribute;
					XmlElement xmlNode = directoryAttribute1.ToXmlNode(doc, "attribute");
					xmlElement1.AppendChild(xmlNode);
				}
			}
			return xmlElement;
		}
	}
}