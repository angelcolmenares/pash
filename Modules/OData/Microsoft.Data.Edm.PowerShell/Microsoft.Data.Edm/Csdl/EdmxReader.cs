using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics;
using Microsoft.Data.Edm.Csdl.Internal.Parsing;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl
{
	internal class EdmxReader
	{
		private readonly static Dictionary<string, Action> EmptyParserLookup;

		private readonly Dictionary<string, Action> edmxParserLookup;

		private readonly Dictionary<string, Action> runtimeParserLookup;

		private readonly Dictionary<string, Action> conceptualModelsParserLookup;

		private readonly Dictionary<string, Action> dataServicesParserLookup;

		private readonly XmlReader reader;

		private readonly List<EdmError> errors;

		private readonly CsdlParser csdlParser;

		private Version dataServiceVersion;

		private Version maxDataServiceVersion;

		private bool targetParsed;

		static EdmxReader()
		{
			EdmxReader.EmptyParserLookup = new Dictionary<string, Action>();
		}

		private EdmxReader(XmlReader reader)
		{
			this.reader = reader;
			this.errors = new List<EdmError>();
			this.csdlParser = new CsdlParser();
			Dictionary<string, Action> strs = new Dictionary<string, Action>();
			strs.Add("DataServices", new Action(this.ParseDataServicesElement));
			strs.Add("Runtime", new Action(this.ParseRuntimeElement));
			this.edmxParserLookup = strs;
			Dictionary<string, Action> strs1 = new Dictionary<string, Action>();
			strs1.Add("Schema", new Action(this.ParseCsdlSchemaElement));
			this.dataServicesParserLookup = strs1;
			Dictionary<string, Action> strs2 = new Dictionary<string, Action>();
			strs2.Add("ConceptualModels", new Action(this.ParseConceptualModelsElement));
			this.runtimeParserLookup = strs2;
			Dictionary<string, Action> strs3 = new Dictionary<string, Action>();
			strs3.Add("Schema", new Action(this.ParseCsdlSchemaElement));
			this.conceptualModelsParserLookup = strs3;
		}

		private string GetAttributeValue(string namespaceUri, string localName)
		{
			string namespaceURI = this.reader.NamespaceURI;
			string value = null;
			bool firstAttribute = this.reader.MoveToFirstAttribute();
			while (firstAttribute)
			{
				if ((namespaceUri == null || !(this.reader.NamespaceURI == namespaceUri)) && !string.IsNullOrEmpty(this.reader.NamespaceURI) && !(this.reader.NamespaceURI == namespaceURI) || !(this.reader.LocalName == localName))
				{
					firstAttribute = this.reader.MoveToNextAttribute();
				}
				else
				{
					value = this.reader.Value;
					break;
				}
			}
			this.reader.MoveToElement();
			return value;
		}

		private CsdlLocation Location()
		{
			IXmlLineInfo xmlLineInfo = this.reader as IXmlLineInfo;
			if (xmlLineInfo == null || !xmlLineInfo.HasLineInfo())
			{
				return new CsdlLocation(0, 0);
			}
			else
			{
				return new CsdlLocation(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			}
		}

		private void ParseConceptualModelsElement()
		{
			this.ParseElement("ConceptualModels", this.conceptualModelsParserLookup);
		}

		private void ParseCsdlSchemaElement()
		{
			using (StringReader stringReader = new StringReader(this.reader.ReadOuterXml()))
			{
				XmlReader xmlReader = XmlReader.Create(stringReader);
				using (xmlReader)
				{
					this.csdlParser.AddReader(xmlReader);
				}
			}
		}

		private void ParseDataServicesElement()
		{
			string attributeValue = this.GetAttributeValue("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "DataServiceVersion");
			if (attributeValue != null && !EdmxReader.TryParseVersion(attributeValue, out this.dataServiceVersion))
			{
				this.RaiseError(EdmErrorCode.InvalidVersionNumber, Strings.EdmxParser_EdmxDataServiceVersionInvalid);
			}
			string str = this.GetAttributeValue("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "MaxDataServiceVersion");
			if (str != null && !EdmxReader.TryParseVersion(str, out this.maxDataServiceVersion))
			{
				this.RaiseError(EdmErrorCode.InvalidVersionNumber, Strings.EdmxParser_EdmxMaxDataServiceVersionInvalid);
			}
			this.ParseTargetElement("DataServices", this.dataServicesParserLookup);
		}

		private void ParseEdmxElement(Version edmxVersion)
		{
			Version version = null;
			string attributeValue = this.GetAttributeValue(null, "Version");
			if (attributeValue != null && (!EdmxReader.TryParseVersion(attributeValue, out version) || version != edmxVersion))
			{
				this.RaiseError(EdmErrorCode.InvalidVersionNumber, Strings.EdmxParser_EdmxVersionMismatch);
			}
			this.ParseElement("Edmx", this.edmxParserLookup);
		}

		private void ParseEdmxFile(out Version edmxVersion)
		{
			edmxVersion = null;
			if (this.reader.NodeType != XmlNodeType.Element)
			{
				while (this.reader.Read() && this.reader.NodeType != XmlNodeType.Element)
				{
				}
			}
			if (!this.reader.EOF)
			{
				if (this.reader.LocalName != "Edmx" || !CsdlConstants.SupportedEdmxNamespaces.TryGetValue(this.reader.NamespaceURI, out edmxVersion))
				{
					this.RaiseError(EdmErrorCode.UnexpectedXmlElement, Strings.XmlParser_UnexpectedRootElement(this.reader.Name, "Edmx"));
					return;
				}
				else
				{
					this.ParseEdmxElement(edmxVersion);
					return;
				}
			}
			else
			{
				this.RaiseEmptyFile();
				return;
			}
		}

		private void ParseElement(string elementName, Dictionary<string, Action> elementParsers)
		{
			if (!this.reader.IsEmptyElement)
			{
				this.reader.Read();
				do
				{
				Label0:
					if (this.reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
					if (this.reader.NodeType != XmlNodeType.Element)
					{
						continue;
					}
					if (!elementParsers.ContainsKey(this.reader.LocalName))
					{
						this.ParseElement(this.reader.LocalName, EdmxReader.EmptyParserLookup);
						goto Label0;
					}
					else
					{
						elementParsers[this.reader.LocalName]();
						goto Label0;
					}
				}
				while (this.reader.Read());
				this.reader.Read();
				return;
			}
			else
			{
				this.reader.Read();
				return;
			}
		}

		private void ParseRuntimeElement()
		{
			this.ParseTargetElement("Runtime", this.runtimeParserLookup);
		}

		private void ParseTargetElement(string elementName, Dictionary<string, Action> elementParsers)
		{
			if (this.targetParsed)
			{
				this.RaiseError(EdmErrorCode.UnexpectedXmlElement, Strings.EdmxParser_BodyElement("DataServices"));
				elementParsers = EdmxReader.EmptyParserLookup;
			}
			else
			{
				this.targetParsed = true;
			}
			this.ParseElement(elementName, elementParsers);
		}

		private void RaiseEmptyFile()
		{
			this.RaiseError(EdmErrorCode.EmptyFile, Strings.XmlParser_EmptySchemaTextReader);
		}

		private void RaiseError(EdmErrorCode errorCode, string errorMessage)
		{
			this.errors.Add(new EdmError(this.Location(), errorCode, errorMessage));
		}

		public static bool TryParse(XmlReader reader, out IEdmModel model, out IEnumerable<EdmError> errors)
		{
			EdmxReader edmxReader = new EdmxReader(reader);
			return edmxReader.TryParse(Enumerable.Empty<IEdmModel>(), out model, out errors);
		}

		public static bool TryParse(XmlReader reader, IEdmModel reference, out IEdmModel model, out IEnumerable<EdmError> errors)
		{
			EdmxReader edmxReader = new EdmxReader(reader);
			IEdmModel[] edmModelArray = new IEdmModel[1];
			edmModelArray[0] = reference;
			return edmxReader.TryParse(edmModelArray, out model, out errors);
		}

		public static bool TryParse(XmlReader reader, IEnumerable<IEdmModel> references, out IEdmModel model, out IEnumerable<EdmError> errors)
		{
			EdmxReader edmxReader = new EdmxReader(reader);
			return edmxReader.TryParse(references, out model, out errors);
		}

		private bool TryParse(IEnumerable<IEdmModel> references, out IEdmModel model, out IEnumerable<EdmError> parsingErrors)
		{
			Version version = null;
			CsdlModel csdlModel = null;
			IEnumerable<EdmError> edmErrors = null;
			bool flag;
			try
			{
				this.ParseEdmxFile(out version);
				if (this.errors.Count != 0)
				{
					model = null;
				}
				else
				{
					if (!this.csdlParser.GetResult(out csdlModel, out edmErrors))
					{
						this.errors.AddRange(edmErrors);
						model = null;
					}
					else
					{
						model = new CsdlSemanticsModel(csdlModel, new CsdlSemanticsDirectValueAnnotationsManager(), references);
						model.SetEdmxVersion(version);
						if (this.dataServiceVersion != null)
						{
							model.SetDataServiceVersion(this.dataServiceVersion);
						}
						if (this.maxDataServiceVersion != null)
						{
							model.SetMaxDataServiceVersion(this.maxDataServiceVersion);
						}
					}
				}
				parsingErrors = this.errors;
				return this.errors.Count == 0;
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				model = null;
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(new CsdlLocation(xmlException.LineNumber, xmlException.LinePosition), EdmErrorCode.XmlError, xmlException.Message);
				parsingErrors = edmError;
				flag = false;
			}
			return flag;
		}

		private static bool TryParseVersion(string input, out Version version)
		{
			int num = 0;
			int num1 = 0;
			version = null;
			if (!string.IsNullOrEmpty(input))
			{
				input = input.Trim();
				char[] chrArray = new char[1];
				chrArray[0] = '.';
				string[] strArrays = input.Split(chrArray);
				if ((int)strArrays.Length == 2)
				{
					if (!int.TryParse(strArrays[0], out num) || !int.TryParse(strArrays[1], out num1))
					{
						return false;
					}
					else
					{
						version = new Version(num, num1);
						return true;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}
}