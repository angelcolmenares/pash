using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Common
{
	internal abstract class XmlDocumentParser
	{
		private readonly string docPath;

		private readonly Stack<XmlDocumentParser.ElementScope> currentBranch;

		private XmlReader reader;

		private IXmlLineInfo xmlLineInfo;

		private List<EdmError> errors;

		private StringBuilder currentText;

		private CsdlLocation currentTextLocation;

		private XmlDocumentParser.ElementScope currentScope;

		internal CsdlLocation DocumentElementLocation
		{
			get;
			private set;
		}

		internal string DocumentNamespace
		{
			get;
			private set;
		}

		internal string DocumentPath
		{
			get
			{
				return this.docPath;
			}
		}

		internal Version DocumentVersion
		{
			get;
			private set;
		}

		internal IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errors;
			}
		}

		internal bool HasErrors
		{
			get;
			private set;
		}

		private bool IsTextNode
		{
			get
			{
				XmlNodeType nodeType = this.reader.NodeType;
				switch (nodeType)
				{
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					{
						return true;
					}
					default:
					{
						if (nodeType == XmlNodeType.SignificantWhitespace)
						{
							return true;
						}
						return false;
					}
				}
			}
		}

		internal CsdlLocation Location
		{
			get
			{
				if (this.xmlLineInfo == null || !this.xmlLineInfo.HasLineInfo())
				{
					return new CsdlLocation(0, 0);
				}
				else
				{
					return new CsdlLocation(this.xmlLineInfo.LineNumber, this.xmlLineInfo.LinePosition);
				}
			}
		}

		internal XmlElementValue Result
		{
			get;
			private set;
		}

		protected XmlDocumentParser(XmlReader underlyingReader, string documentPath)
		{
			this.currentBranch = new Stack<XmlDocumentParser.ElementScope>();
			this.reader = underlyingReader;
			this.docPath = documentPath;
			this.errors = new List<EdmError>();
		}

		private void BeginElement(XmlElementParser elementParser, XmlElementInfo element)
		{
			XmlDocumentParser.ElementScope elementScope = new XmlDocumentParser.ElementScope(elementParser, element);
			this.currentBranch.Push(elementScope);
			this.currentScope = elementScope;
		}

		protected virtual XmlElementParser<TResult> Element<TResult>(string elementName, Func<XmlElementInfo, XmlElementValueCollection, TResult> parserFunc, XmlElementParser[] childParsers)
		{
			return XmlElementParser.Create<TResult>(elementName, parserFunc, childParsers, null);
		}

		private void EndElement()
		{
			XmlTextValue xmlTextValue;
			XmlDocumentParser.ElementScope elementScope;
			XmlDocumentParser.ElementScope elementScope1 = this.currentBranch.Pop();
			XmlDocumentParser xmlDocumentParser = this;
			if (this.currentBranch.Count > 0)
			{
				elementScope = this.currentBranch.Peek();
			}
			else
			{
				elementScope = null;
			}
			xmlDocumentParser.currentScope = elementScope;
			XmlElementParser parser = elementScope1.Parser;
			XmlElementValue xmlElementValue = parser.Parse(elementScope1.Element, elementScope1.ChildValues);
			if (xmlElementValue != null)
			{
				if (this.currentScope == null)
				{
					this.Result = xmlElementValue;
				}
				else
				{
					this.currentScope.AddChildValue(xmlElementValue);
				}
			}
			foreach (XmlAttributeInfo unused in elementScope1.Element.Attributes.Unused)
			{
				this.ReportUnexpectedAttribute(unused.Location, unused.Name);
			}
			IList<XmlElementValue> childValues = elementScope1.ChildValues;
			IEnumerable<XmlElementValue> xmlElementValues = childValues.Where<XmlElementValue>((XmlElementValue v) => v.IsText);
			IEnumerable<XmlElementValue> xmlElementValues1 = xmlElementValues;
			IEnumerable<XmlElementValue> xmlElementValues2 = xmlElementValues1.Where<XmlElementValue>((XmlElementValue t) => !t.IsUsed);
			if (xmlElementValues2.Any<XmlElementValue>())
			{
				if (xmlElementValues2.Count<XmlElementValue>() != xmlElementValues.Count<XmlElementValue>())
				{
					xmlTextValue = (XmlTextValue)xmlElementValues2.First<XmlElementValue>();
				}
				else
				{
					xmlTextValue = (XmlTextValue)xmlElementValues.First<XmlElementValue>();
				}
				this.ReportTextNotAllowed(xmlTextValue.Location, xmlTextValue.Value);
			}
			IList<XmlElementValue> childValues1 = elementScope1.ChildValues;
			foreach (XmlElementValue xmlElementValue1 in childValues1.Where<XmlElementValue>((XmlElementValue v) => {
				if (v.IsText)
				{
					return false;
				}
				else
				{
					return !v.IsUsed;
				}
			}
			))
			{
				this.ReportUnusedElement(xmlElementValue1.Location, xmlElementValue1.Name);
			}
		}

		protected abstract XmlReader InitializeReader(XmlReader inputReader);

		protected virtual bool IsOwnedNamespace(string namespaceName)
		{
			return this.DocumentNamespace.EqualsOrdinal(namespaceName);
		}

		private void Parse()
		{
			while (this.currentBranch.Count > 0 && this.reader.Read())
			{
				this.ProcessNode();
			}
			if (!this.reader.EOF)
			{
				this.reader.Read();
				return;
			}
			else
			{
				return;
			}
		}

		internal void ParseDocumentElement()
		{
			Version version = null;
			string[] strArrays = null;
			XmlElementParser xmlElementParser = null;
			this.reader = this.InitializeReader(this.reader);
			this.xmlLineInfo = this.reader as IXmlLineInfo;
			if (this.reader.NodeType != XmlNodeType.Element)
			{
				while (this.reader.Read() && this.reader.NodeType != XmlNodeType.Element)
				{
				}
			}
			if (!this.reader.EOF)
			{
				this.DocumentNamespace = this.reader.NamespaceURI;
				if (!this.TryGetDocumentVersion(this.DocumentNamespace, out version, out strArrays))
				{
					this.ReportUnexpectedRootNamespace(this.reader.LocalName, this.DocumentNamespace, strArrays);
					return;
				}
				else
				{
					this.DocumentVersion = version;
					this.DocumentElementLocation = this.Location;
					bool isEmptyElement = this.reader.IsEmptyElement;
					XmlElementInfo xmlElementInfo = this.ReadElement(this.reader.LocalName, this.DocumentElementLocation);
					if (this.TryGetRootElementParser(this.DocumentVersion, xmlElementInfo, out xmlElementParser))
					{
						this.BeginElement(xmlElementParser, xmlElementInfo);
						if (!isEmptyElement)
						{
							this.Parse();
							return;
						}
						else
						{
							this.EndElement();
							return;
						}
					}
					else
					{
						this.ReportUnexpectedRootElement(xmlElementInfo.Location, xmlElementInfo.Name, this.DocumentNamespace);
						return;
					}
				}
			}
			else
			{
				this.ReportEmptyFile();
				return;
			}
		}

		private void ProcessElement()
		{
			XmlElementParser xmlElementParser = null;
			bool isEmptyElement = this.reader.IsEmptyElement;
			string namespaceURI = this.reader.NamespaceURI;
			string localName = this.reader.LocalName;
			if (namespaceURI != this.DocumentNamespace)
			{
				if (string.IsNullOrEmpty(namespaceURI) || this.IsOwnedNamespace(namespaceURI))
				{
					this.ReportUnexpectedElement(this.Location, this.reader.Name);
					this.reader.Skip();
					return;
				}
				else
				{
					XmlReader xmlReader = this.reader.ReadSubtree();
					xmlReader.MoveToContent();
					string str = xmlReader.ReadOuterXml();
					this.currentScope.Element.AddAnnotation(new XmlAnnotationInfo(this.Location, namespaceURI, localName, str, false));
				}
			}
			else
			{
				if (this.currentScope.Parser.TryGetChildElementParser(localName, out xmlElementParser))
				{
					XmlElementInfo xmlElementInfo = this.ReadElement(localName, this.Location);
					this.BeginElement(xmlElementParser, xmlElementInfo);
					if (isEmptyElement)
					{
						this.EndElement();
						return;
					}
				}
				else
				{
					this.ReportUnexpectedElement(this.Location, this.reader.Name);
					if (!isEmptyElement)
					{
						this.reader.Read();
					}
					return;
				}
			}
		}

		private void ProcessNode()
		{
			if (!this.IsTextNode)
			{
				if (this.currentText != null)
				{
					string str = this.currentText.ToString();
					CsdlLocation csdlLocation = this.currentTextLocation;
					this.currentText = null;
					this.currentTextLocation = null;
					if (!EdmUtil.IsNullOrWhiteSpaceInternal(str) && !string.IsNullOrEmpty(str))
					{
						this.currentScope.AddChildValue(new XmlTextValue(csdlLocation, str));
					}
				}
				XmlNodeType nodeType = this.reader.NodeType;
				switch (nodeType)
				{
					case XmlNodeType.Element:
					{
						this.ProcessElement();
						return;
					}
					case XmlNodeType.Attribute:
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
					case XmlNodeType.Entity:
					case XmlNodeType.Document:
					case XmlNodeType.DocumentFragment:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.EndEntity:
					{
						this.ReportUnexpectedNodeType(this.reader.NodeType);
						this.reader.Skip();
						return;
					}
					case XmlNodeType.EntityReference:
					case XmlNodeType.DocumentType:
					{
						this.reader.Skip();
						return;
					}
					case XmlNodeType.ProcessingInstruction:
					case XmlNodeType.Comment:
					case XmlNodeType.Notation:
					case XmlNodeType.Whitespace:
					case XmlNodeType.XmlDeclaration:
					{
						return;
					}
					case XmlNodeType.EndElement:
					{
						this.EndElement();
						return;
					}
					default:
					{
						this.ReportUnexpectedNodeType(this.reader.NodeType);
						this.reader.Skip();
						return;
					}
				}
			}
			else
			{
				if (this.currentText == null)
				{
					this.currentText = new StringBuilder();
					this.currentTextLocation = this.Location;
				}
				this.currentText.Append(this.reader.Value);
				return;
			}
		}

		private XmlElementInfo ReadElement(string elementName, CsdlLocation elementLocation)
		{
			List<XmlAttributeInfo> xmlAttributeInfos = null;
			List<XmlAnnotationInfo> xmlAnnotationInfos = null;
			for (bool i = this.reader.MoveToFirstAttribute(); i; i = this.reader.MoveToNextAttribute())
			{
				string namespaceURI = this.reader.NamespaceURI;
				if (string.IsNullOrEmpty(namespaceURI) || namespaceURI.EqualsOrdinal(this.DocumentNamespace))
				{
					if (xmlAttributeInfos == null)
					{
						xmlAttributeInfos = new List<XmlAttributeInfo>();
					}
					xmlAttributeInfos.Add(new XmlAttributeInfo(this.reader.LocalName, this.reader.Value, this.Location));
				}
				else
				{
					if (!this.IsOwnedNamespace(namespaceURI))
					{
						if (xmlAnnotationInfos == null)
						{
							xmlAnnotationInfos = new List<XmlAnnotationInfo>();
						}
						xmlAnnotationInfos.Add(new XmlAnnotationInfo(this.Location, this.reader.NamespaceURI, this.reader.LocalName, this.reader.Value, true));
					}
					else
					{
						this.ReportUnexpectedAttribute(this.Location, this.reader.Name);
					}
				}
			}
			return new XmlElementInfo(elementName, elementLocation, xmlAttributeInfos, xmlAnnotationInfos);
		}

		private void ReportEmptyFile()
		{
			string xmlParserEmptySchemaTextReader;
			if (this.DocumentPath == null)
			{
				xmlParserEmptySchemaTextReader = Strings.XmlParser_EmptySchemaTextReader;
			}
			else
			{
				xmlParserEmptySchemaTextReader = Strings.XmlParser_EmptyFile(this.DocumentPath);
			}
			string str = xmlParserEmptySchemaTextReader;
			this.ReportError(this.Location, EdmErrorCode.EmptyFile, str);
		}

		protected void ReportError(CsdlLocation errorLocation, EdmErrorCode errorCode, string errorMessage)
		{
			this.errors.Add(new EdmError(errorLocation, errorCode, errorMessage));
			this.HasErrors = true;
		}

		private void ReportTextNotAllowed(CsdlLocation errorLocation, string textValue)
		{
			this.ReportError(errorLocation, EdmErrorCode.TextNotAllowed, Strings.XmlParser_TextNotAllowed(textValue));
		}

		private void ReportUnexpectedAttribute(CsdlLocation errorLocation, string attributeName)
		{
			this.ReportError(errorLocation, EdmErrorCode.UnexpectedXmlAttribute, Strings.XmlParser_UnexpectedAttribute(attributeName));
		}

		private void ReportUnexpectedElement(CsdlLocation errorLocation, string elementName)
		{
			this.ReportError(errorLocation, EdmErrorCode.UnexpectedXmlElement, Strings.XmlParser_UnexpectedElement(elementName));
		}

		private void ReportUnexpectedNodeType(XmlNodeType nodeType)
		{
			this.ReportError(this.Location, EdmErrorCode.UnexpectedXmlNodeType, Strings.XmlParser_UnexpectedNodeType(nodeType));
		}

		private void ReportUnexpectedRootElement(CsdlLocation elementLocation, string elementName, string expectedNamespace)
		{
			this.ReportError(elementLocation, EdmErrorCode.UnexpectedXmlElement, Strings.XmlParser_UnexpectedRootElement(elementName, "Schema"));
		}

		private void ReportUnexpectedRootNamespace(string elementName, string namespaceUri, string[] expectedNamespaces)
		{
			string str;
			string str1 = string.Join(", ", expectedNamespaces);
			if (string.IsNullOrEmpty(namespaceUri))
			{
				str = Strings.XmlParser_UnexpectedRootElementNoNamespace(str1);
			}
			else
			{
				str = Strings.XmlParser_UnexpectedRootElementWrongNamespace(namespaceUri, str1);
			}
			string str2 = str;
			this.ReportError(this.Location, EdmErrorCode.UnexpectedXmlElement, str2);
		}

		private void ReportUnusedElement(CsdlLocation errorLocation, string elementName)
		{
			this.ReportError(errorLocation, EdmErrorCode.UnexpectedXmlElement, Strings.XmlParser_UnusedElement(elementName));
		}

		protected abstract bool TryGetDocumentVersion(string xmlNamespaceName, out Version version, out string[] expectedNamespaces);

		protected abstract bool TryGetRootElementParser(Version artifactVersion, XmlElementInfo rootElement, out XmlElementParser parser);

		private class ElementScope
		{
			private readonly static IList<XmlElementValue> EmptyValues;

			private List<XmlElementValue> childValues;

			internal IList<XmlElementValue> ChildValues
			{
				get
				{
					List<XmlElementValue> xmlElementValues = this.childValues;
					IList<XmlElementValue> emptyValues = xmlElementValues;
					if (xmlElementValues == null)
					{
						emptyValues = XmlDocumentParser.ElementScope.EmptyValues;
					}
					return emptyValues;
				}
			}

			internal XmlElementInfo Element
			{
				get;
				private set;
			}

			internal XmlElementParser Parser
			{
				get;
				private set;
			}

			static ElementScope()
			{
				XmlDocumentParser.ElementScope.EmptyValues = new ReadOnlyCollection<XmlElementValue>(new XmlElementValue[0]);
			}

			internal ElementScope(XmlElementParser parser, XmlElementInfo element)
			{
				this.Parser = parser;
				this.Element = element;
			}

			internal void AddChildValue(XmlElementValue value)
			{
				if (this.childValues == null)
				{
					this.childValues = new List<XmlElementValue>();
				}
				this.childValues.Add(value);
			}
		}
	}
}