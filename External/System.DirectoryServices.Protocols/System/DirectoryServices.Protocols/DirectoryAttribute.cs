using System;
using System.Collections;
using System.Runtime;
using System.Text;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	public class DirectoryAttribute : CollectionBase
	{
		private string attributeName;

		internal bool isSearchResult;

		private static UTF8Encoding utf8EncoderWithErrorDetection;

		private static UTF8Encoding encoder;

		public object this[int index]
		{
			get
			{
				object str;
				if (this.isSearchResult)
				{
					byte[] item = base.List[index] as byte[];
					if (item == null)
					{
						return base.List[index];
					}
					else
					{
						try
						{
							str = DirectoryAttribute.utf8EncoderWithErrorDetection.GetString(item);
						}
						catch (ArgumentException argumentException)
						{
							str = base.List[index];
						}
						return str;
					}
				}
				else
				{
					return base.List[index];
				}
			}
			set
			{
				if (value != null)
				{
					if (value as string != null || value as byte[] != null || value as Uri != null)
					{
						base.List[index] = value;
						return;
					}
					else
					{
						throw new ArgumentException(Res.GetString("ValidValueType"), "value");
					}
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.attributeName;
			}
			set
			{
				if (value != null)
				{
					this.attributeName = value;
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		static DirectoryAttribute()
		{
			DirectoryAttribute.utf8EncoderWithErrorDetection = new UTF8Encoding(false, true);
			DirectoryAttribute.encoder = new UTF8Encoding();
		}

		public DirectoryAttribute()
		{
			this.attributeName = "";
			Utility.CheckOSVersion();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryAttribute(string name, string value) : this(name, (object)value)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryAttribute(string name, byte[] value) : this(name, (object)value)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DirectoryAttribute(string name, Uri value) : this(name, (object)value)
		{
		}

		internal DirectoryAttribute(string name, object value) : this()
		{
			if (name != null)
			{
				if (value != null)
				{
					this.Name = name;
					this.Add(value);
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
			else
			{
				throw new ArgumentNullException("name");
			}
		}

		public DirectoryAttribute(string name, object[] values) : this()
		{
			if (name != null)
			{
				if (values != null)
				{
					this.Name = name;
					for (int i = 0; i < (int)values.Length; i++)
					{
						this.Add(values[i]);
					}
					return;
				}
				else
				{
					throw new ArgumentNullException("values");
				}
			}
			else
			{
				throw new ArgumentNullException("name");
			}
		}

		internal DirectoryAttribute(XmlElement node)
		{
			byte[] numArray;
			this.attributeName = "";
			string str = "@dsml:name";
			string str1 = "@name";
			XmlNamespaceManager dsmlNamespaceManager = NamespaceUtils.GetDsmlNamespaceManager();
			XmlAttribute xmlAttribute = (XmlAttribute)node.SelectSingleNode(str, dsmlNamespaceManager);
			if (xmlAttribute != null)
			{
				this.attributeName = xmlAttribute.Value;
			}
			else
			{
				xmlAttribute = (XmlAttribute)node.SelectSingleNode(str1, dsmlNamespaceManager);
				if (xmlAttribute != null)
				{
					this.attributeName = xmlAttribute.Value;
				}
				else
				{
					throw new DsmlInvalidDocumentException(Res.GetString("MissingSearchResultEntryAttributeName"));
				}
			}
			XmlNodeList xmlNodeLists = node.SelectNodes("dsml:value", dsmlNamespaceManager);
			if (xmlNodeLists.Count != 0)
			{
				foreach (XmlNode xmlNodes in xmlNodeLists)
				{
					XmlAttribute xmlAttribute1 = (XmlAttribute)xmlNodes.SelectSingleNode("@xsi:type", dsmlNamespaceManager);
					if (xmlAttribute1 != null)
					{
						if (string.Compare(xmlAttribute1.Value, "xsd:string", StringComparison.OrdinalIgnoreCase) != 0)
						{
							if (string.Compare(xmlAttribute1.Value, "xsd:base64Binary", StringComparison.OrdinalIgnoreCase) != 0)
							{
								if (string.Compare(xmlAttribute1.Value, "xsd:anyURI", StringComparison.OrdinalIgnoreCase) != 0)
								{
									continue;
								}
								Uri uri = new Uri(xmlNodes.InnerText);
								this.Add(uri);
							}
							else
							{
								string innerText = xmlNodes.InnerText;
								try
								{
									numArray = Convert.FromBase64String(innerText);
								}
								catch (FormatException formatException)
								{
									throw new DsmlInvalidDocumentException(Res.GetString("BadBase64Value"));
								}
								this.Add(numArray);
							}
						}
						else
						{
							this.Add(xmlNodes.InnerText);
						}
					}
					else
					{
						this.Add(xmlNodes.InnerText);
					}
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public int Add(byte[] value)
		{
			return this.Add(value);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public int Add(string value)
		{
			return this.Add(value);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public int Add(Uri value)
		{
			return this.Add(value);
		}

		internal int Add(object value)
		{
			if (value != null)
			{
				if (value as string != null || value as byte[] != null || value as Uri != null)
				{
					return base.List.Add(value);
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValueType"), "value");
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void AddRange(object[] values)
		{
			if (values != null)
			{
				if (values as string[] != null || values as byte[][] != null || values as Uri[] != null)
				{
					int num = 0;
					while (num < (int)values.Length)
					{
						if (values[num] != null)
						{
							num++;
						}
						else
						{
							throw new ArgumentException(Res.GetString("NullValueArray"), "values");
						}
					}
					base.InnerList.AddRange(values);
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValuesType"), "values");
				}
			}
			else
			{
				throw new ArgumentNullException("values");
			}
		}

		public bool Contains(object value)
		{
			return base.List.Contains(value);
		}

		public void CopyTo(object[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public object[] GetValues(Type valuesType)
		{
			if (valuesType != typeof(byte[]))
			{
				if (valuesType != typeof(string))
				{
					throw new ArgumentException(Res.GetString("ValidDirectoryAttributeType"), "valuesType");
				}
				else
				{
					int count = base.List.Count;
					string[] str = new string[count];
					for (int i = 0; i < count; i++)
					{
						if (base.List[i] as string == null)
						{
							if (base.List[i] as byte[] == null)
							{
								throw new NotSupportedException(Res.GetString("DirectoryAttributeConversion"));
							}
							else
							{
								str[i] = DirectoryAttribute.encoder.GetString((byte[])base.List[i]);
							}
						}
						else
						{
							str[i] = (string)base.List[i];
						}
					}
					return str;
				}
			}
			else
			{
				int num = base.List.Count;
				byte[][] item = new byte[num][];
				for (int j = 0; j < num; j++)
				{
					if (base.List[j] as string == null)
					{
						if (base.List[j] as byte[] == null)
						{
							throw new NotSupportedException(Res.GetString("DirectoryAttributeConversion"));
						}
						else
						{
							item[j] = (byte[])base.List[j];
						}
					}
					else
					{
						item[j] = DirectoryAttribute.encoder.GetBytes((string)base.List[j]);
					}
				}
				return item;
			}
		}

		public int IndexOf(object value)
		{
			return base.List.IndexOf(value);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Insert(int index, byte[] value)
		{
			this.Insert(index, value);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Insert(int index, string value)
		{
			this.Insert(index, value);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Insert(int index, Uri value)
		{
			this.Insert(index, value);
		}

		private void Insert(int index, object value)
		{
			if (value != null)
			{
				base.List.Insert(index, value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		protected override void OnValidate(object value)
		{
			if (value != null)
			{
				if (value as string != null || value as byte[] != null || value as Uri != null)
				{
					return;
				}
				else
				{
					throw new ArgumentException(Res.GetString("ValidValueType"), "value");
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Remove(object value)
		{
			base.List.Remove(value);
		}

		internal XmlElement ToXmlNode(XmlDocument doc, string elementName)
		{
			XmlElement xmlElement = doc.CreateElement(elementName, "urn:oasis:names:tc:DSML:2:0:core");
			this.ToXmlNodeCommon(xmlElement);
			return xmlElement;
		}

		internal void ToXmlNodeCommon(XmlElement elemBase)
		{
			XmlDocument ownerDocument = elemBase.OwnerDocument;
			XmlAttribute name = ownerDocument.CreateAttribute("name", null);
			name.InnerText = this.Name;
			elemBase.Attributes.Append(name);
			if (base.Count != 0)
			{
				foreach (object innerList in base.InnerList)
				{
					XmlElement str = ownerDocument.CreateElement("value", "urn:oasis:names:tc:DSML:2:0:core");
					if (innerList as byte[] == null)
					{
						if (innerList as Uri == null)
						{
							str.InnerText = innerList.ToString();
							if (str.InnerText.StartsWith(" ", StringComparison.Ordinal) || str.InnerText.EndsWith(" ", StringComparison.Ordinal))
							{
								XmlAttribute xmlAttribute = ownerDocument.CreateAttribute("xml:space");
								xmlAttribute.InnerText = "preserve";
								str.Attributes.Append(xmlAttribute);
							}
						}
						else
						{
							str.InnerText = innerList.ToString();
							XmlAttribute xmlAttribute1 = ownerDocument.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
							xmlAttribute1.InnerText = "xsd:anyURI";
							str.Attributes.Append(xmlAttribute1);
						}
					}
					else
					{
						str.InnerText = Convert.ToBase64String((byte[])innerList);
						XmlAttribute xmlAttribute2 = ownerDocument.CreateAttribute("xsi:type", "http://www.w3.org/2001/XMLSchema-instance");
						xmlAttribute2.InnerText = "xsd:base64Binary";
						str.Attributes.Append(xmlAttribute2);
					}
					elemBase.AppendChild(str);
				}
			}
		}
	}
}