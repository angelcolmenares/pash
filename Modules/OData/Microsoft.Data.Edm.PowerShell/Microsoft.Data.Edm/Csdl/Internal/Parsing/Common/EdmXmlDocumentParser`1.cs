using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal abstract class EdmXmlDocumentParser<TResult> : XmlDocumentParser<TResult>
	{
		protected XmlElementInfo currentElement;

		private readonly Stack<XmlElementInfo> elementStack;

		private HashSetInternal<string> edmNamespaces;

		internal abstract IEnumerable<KeyValuePair<Version, string>> SupportedVersions
		{
			get;
		}

		internal EdmXmlDocumentParser(string artifactLocation, XmlReader reader) : base(reader, artifactLocation)
		{
			this.elementStack = new Stack<XmlElementInfo>();
		}

		protected abstract void AnnotateItem(object result, XmlElementValueCollection childValues);

		protected void BeginItem(XmlElementInfo element)
		{
			this.elementStack.Push(element);
			this.currentElement = element;
		}

		protected XmlElementParser<TItem> CsdlElement<TItem>(string elementName, Func<XmlElementInfo, XmlElementValueCollection, TItem> initializer, XmlElementParser[] childParsers)
		where TItem : class
		{
			return this.Element<TItem>(elementName, (XmlElementInfo element, XmlElementValueCollection childValues) => {
				this.BeginItem(element);
				TItem tItem = initializer(element, childValues);
				this.AnnotateItem(tItem, childValues);
				this.EndItem();
				return tItem;
			}
			, childParsers);
		}

		protected void EndItem()
		{
			XmlElementInfo xmlElementInfo;
			this.elementStack.Pop();
			EdmXmlDocumentParser<TResult> edmXmlDocumentParser = this;
			if (this.elementStack.Count == 0)
			{
				xmlElementInfo = null;
			}
			else
			{
				xmlElementInfo = this.elementStack.Peek();
			}
			edmXmlDocumentParser.currentElement = xmlElementInfo;
		}

		internal XmlAttributeInfo GetOptionalAttribute(XmlElementInfo element, string attributeName)
		{
			return element.Attributes[attributeName];
		}

		internal XmlAttributeInfo GetRequiredAttribute(XmlElementInfo element, string attributeName)
		{
			XmlAttributeInfo item = element.Attributes[attributeName];
			if (!item.IsMissing)
			{
				return item;
			}
			else
			{
				base.ReportError(element.Location, EdmErrorCode.MissingAttribute, Strings.XmlParser_MissingAttribute(attributeName, element.Name));
				return item;
			}
		}

		protected override XmlReader InitializeReader(XmlReader reader)
		{
			XmlReaderSettings xmlReaderSetting = new XmlReaderSettings();
			xmlReaderSetting.CheckCharacters = true;
			xmlReaderSetting.CloseInput = false;
			xmlReaderSetting.IgnoreWhitespace = true;
			xmlReaderSetting.ConformanceLevel = ConformanceLevel.Auto;
			xmlReaderSetting.IgnoreComments = true;
			xmlReaderSetting.IgnoreProcessingInstructions = true;
			xmlReaderSetting.DtdProcessing = DtdProcessing.Prohibit;
			XmlReaderSettings xmlReaderSetting1 = xmlReaderSetting;
			return XmlReader.Create(reader, xmlReaderSetting1);
		}

		private bool IsEdmNamespace(string xmlNamespaceUri)
		{
			if (this.edmNamespaces == null)
			{
				this.edmNamespaces = new HashSetInternal<string>();
				foreach (string[] value in CsdlConstants.SupportedVersions.Values)
				{
					string[] strArrays = value;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						this.edmNamespaces.Add(str);
					}
				}
			}
			return this.edmNamespaces.Contains(xmlNamespaceUri);
		}

		protected override bool IsOwnedNamespace(string namespaceName)
		{
			return this.IsEdmNamespace(namespaceName);
		}

		protected string Optional(string attributeName)
		{
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (!optionalAttribute.IsMissing)
			{
				return optionalAttribute.Value;
			}
			else
			{
				return null;
			}
		}

		protected string OptionalAlias(string attributeName)
		{
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateAlias(optionalAttribute.Value);
			}
		}

		protected bool? OptionalBoolean(string attributeName)
		{
			bool? nullable;
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				bool? nullable1 = null;
				return nullable1;
			}
			else
			{
				if (!EdmValueParser.TryParseBool(optionalAttribute.Value, out nullable))
				{
					base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidBoolean, Strings.ValueParser_InvalidBoolean(optionalAttribute.Value));
				}
				return nullable;
			}
		}

		protected EdmConcurrencyMode? OptionalConcurrencyMode(string attributeName)
		{
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (!optionalAttribute.IsMissing)
			{
				string value = optionalAttribute.Value;
				string str = value;
				if (value != null)
				{
					if (str == "None")
					{
						return new EdmConcurrencyMode?(EdmConcurrencyMode.None);
					}
					else
					{
						if (str == "Fixed")
						{
							return new EdmConcurrencyMode?(EdmConcurrencyMode.Fixed);
						}
					}
				}
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidConcurrencyMode, Strings.CsdlParser_InvalidConcurrencyMode(optionalAttribute.Value));
			}
			EdmConcurrencyMode? nullable = null;
			return nullable;
		}

		protected EdmFunctionParameterMode? OptionalFunctionParameterMode(string attributeName)
		{
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				EdmFunctionParameterMode? nullable = null;
				return nullable;
			}
			else
			{
				string value = optionalAttribute.Value;
				string str = value;
				if (value != null)
				{
					if (str == "In")
					{
						return new EdmFunctionParameterMode?(EdmFunctionParameterMode.In);
					}
					else
					{
						if (str == "InOut")
						{
							return new EdmFunctionParameterMode?(EdmFunctionParameterMode.InOut);
						}
						else
						{
							if (str == "Out")
							{
								return new EdmFunctionParameterMode?(EdmFunctionParameterMode.Out);
							}
						}
					}
				}
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidParameterMode, Strings.CsdlParser_InvalidParameterMode(optionalAttribute.Value));
				return new EdmFunctionParameterMode?(EdmFunctionParameterMode.None);
			}
		}

		protected int? OptionalInteger(string attributeName)
		{
			int? nullable;
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				int? nullable1 = null;
				return nullable1;
			}
			else
			{
				if (!EdmValueParser.TryParseInt(optionalAttribute.Value, out nullable))
				{
					base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidInteger, Strings.ValueParser_InvalidInteger(optionalAttribute.Value));
				}
				return nullable;
			}
		}

		protected long? OptionalLong(string attributeName)
		{
			long? nullable;
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				long? nullable1 = null;
				return nullable1;
			}
			else
			{
				if (!EdmValueParser.TryParseLong(optionalAttribute.Value, out nullable))
				{
					base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidLong, Strings.ValueParser_InvalidLong(optionalAttribute.Value));
				}
				return nullable;
			}
		}

		protected int? OptionalMaxLength(string attributeName)
		{
			int? nullable;
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				int? nullable1 = null;
				return nullable1;
			}
			else
			{
				if (!EdmValueParser.TryParseInt(optionalAttribute.Value, out nullable))
				{
					base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidMaxLength, Strings.ValueParser_InvalidMaxLength(optionalAttribute.Value));
				}
				return nullable;
			}
		}

		protected string OptionalQualifiedName(string attributeName)
		{
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateQualifiedName(optionalAttribute.Value);
			}
		}

		protected int? OptionalSrid(string attributeName, int defaultSrid)
		{
			int? nullable;
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				return new int?(defaultSrid);
			}
			else
			{
				if (!optionalAttribute.Value.EqualsOrdinalIgnoreCase("Variable"))
				{
					if (!EdmValueParser.TryParseInt(optionalAttribute.Value, out nullable))
					{
						base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidSrid, Strings.ValueParser_InvalidSrid(optionalAttribute.Value));
					}
				}
				else
				{
					nullable = null;
				}
				return nullable;
			}
		}

		protected string OptionalType(string attributeName)
		{
			XmlAttributeInfo optionalAttribute = this.GetOptionalAttribute(this.currentElement, attributeName);
			if (optionalAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateTypeName(optionalAttribute.Value);
			}
		}

		protected string Required(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (!requiredAttribute.IsMissing)
			{
				return requiredAttribute.Value;
			}
			else
			{
				return string.Empty;
			}
		}

		protected string RequiredAlias(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (requiredAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateAlias(requiredAttribute.Value);
			}
		}

		protected string RequiredEntitySetPath(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (requiredAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateEntitySetPath(requiredAttribute.Value);
			}
		}

		protected string RequiredEnumMemberPath(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (requiredAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateEnumMemberPath(requiredAttribute.Value);
			}
		}

		protected EdmMultiplicity RequiredMultiplicity(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (!requiredAttribute.IsMissing)
			{
				string value = requiredAttribute.Value;
				string str = value;
				if (value != null)
				{
					if (str == "1")
					{
						return EdmMultiplicity.One;
					}
					else
					{
						if (str == "0..1")
						{
							return EdmMultiplicity.ZeroOrOne;
						}
						else
						{
							if (str == "*")
							{
								return EdmMultiplicity.Many;
							}
						}
					}
				}
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidMultiplicity, Strings.CsdlParser_InvalidMultiplicity(requiredAttribute.Value));
			}
			return EdmMultiplicity.One;
		}

		protected EdmOnDeleteAction RequiredOnDeleteAction(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (!requiredAttribute.IsMissing)
			{
				string value = requiredAttribute.Value;
				string str = value;
				if (value != null)
				{
					if (str == "None")
					{
						return EdmOnDeleteAction.None;
					}
					else
					{
						if (str == "Cascade")
						{
							return EdmOnDeleteAction.Cascade;
						}
					}
				}
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidOnDelete, Strings.CsdlParser_InvalidDeleteAction(requiredAttribute.Value));
			}
			return EdmOnDeleteAction.None;
		}

		protected string RequiredQualifiedName(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (requiredAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateQualifiedName(requiredAttribute.Value);
			}
		}

		protected string RequiredType(string attributeName)
		{
			XmlAttributeInfo requiredAttribute = this.GetRequiredAttribute(this.currentElement, attributeName);
			if (requiredAttribute.IsMissing)
			{
				return null;
			}
			else
			{
				return this.ValidateTypeName(requiredAttribute.Value);
			}
		}

		protected override bool TryGetDocumentVersion(string xmlNamespaceName, out Version version, out string[] expectedNamespaces)
		{
			expectedNamespaces = (from v in this.SupportedVersions select v.Value).ToArray<string>();
			version = (from v in this.SupportedVersions
			           where v.Value == xmlNamespaceName
			           select v.Key).FirstOrDefault<Version>();
			return (version != null);
		}

		private string ValidateAlias(string name)
		{
			if (!EdmUtil.IsValidUndottedName(name))
			{
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidQualifiedName, Strings.CsdlParser_InvalidAlias(name));
			}
			return name;
		}

		private string ValidateEntitySetPath(string path)
		{
			char[] chrArray = new char[1];
			chrArray[0] = '/';
			string[] strArrays = path.Split(chrArray);
			if (strArrays.Count<string>() != 2 || !EdmUtil.IsValidDottedName(strArrays[0]) || !EdmUtil.IsValidUndottedName(strArrays[1]))
			{
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidEntitySetPath, Strings.CsdlParser_InvalidEntitySetPath(path));
			}
			return path;
		}

		private string ValidateEnumMemberPath(string path)
		{
			char[] chrArray = new char[1];
			chrArray[0] = '/';
			string[] strArrays = path.Split(chrArray);
			if (strArrays.Count<string>() != 2 || !EdmUtil.IsValidDottedName(strArrays[0]) || !EdmUtil.IsValidUndottedName(strArrays[1]))
			{
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidEnumMemberPath, Strings.CsdlParser_InvalidEnumMemberPath(path));
			}
			return path;
		}

		private string ValidateQualifiedName(string qualifiedName)
		{
			if (!EdmUtil.IsQualifiedName(qualifiedName))
			{
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidQualifiedName, Strings.CsdlParser_InvalidQualifiedName(qualifiedName));
			}
			return qualifiedName;
		}

		private string ValidateTypeName(string name)
		{
			char[] chrArray = new char[2];
			chrArray[0] = '(';
			chrArray[1] = ')';
			string[] strArrays = name.Split(chrArray);
			string str = strArrays[0];
			string str1 = str;
			string str2 = str1;
			if (str1 != null)
			{
				if (str2 == "Collection")
				{
					if (strArrays.Count<string>() != 1)
					{
						str = strArrays[1];
					}
					else
					{
						return name;
					}
				}
				else
				{
					if (str2 == "Ref")
					{
						if (strArrays.Count<string>() != 1)
						{
							str = strArrays[1];
						}
						else
						{
							base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidTypeName, Strings.CsdlParser_InvalidTypeName(name));
							return name;
						}
					}
				}
			}
			if (EdmUtil.IsQualifiedName(str) || EdmCoreModel.Instance.GetPrimitiveTypeKind(str) != EdmPrimitiveTypeKind.None)
			{
				return name;
			}
			else
			{
				base.ReportError(this.currentElement.Location, EdmErrorCode.InvalidTypeName, Strings.CsdlParser_InvalidTypeName(name));
				return name;
			}
		}
	}
}