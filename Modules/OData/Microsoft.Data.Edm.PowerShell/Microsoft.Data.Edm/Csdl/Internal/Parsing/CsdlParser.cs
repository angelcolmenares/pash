using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing
{
	internal class CsdlParser
	{
		private readonly List<EdmError> errorsList;

		private readonly CsdlModel result;

		private bool success;

		public CsdlParser()
		{
			this.errorsList = new List<EdmError>();
			this.result = new CsdlModel();
			this.success = true;
		}

		public bool AddReader(XmlReader csdlReader)
		{
			string baseURI = csdlReader.BaseURI;
			string empty = baseURI;
			if (baseURI == null)
			{
				empty = string.Empty;
			}
			string str = empty;
			CsdlDocumentParser csdlDocumentParser = new CsdlDocumentParser(str, csdlReader);
			csdlDocumentParser.ParseDocumentElement();
			CsdlParser hasErrors = this;
			hasErrors.success = hasErrors.success & !csdlDocumentParser.HasErrors;
			this.errorsList.AddRange(csdlDocumentParser.Errors);
			if (csdlDocumentParser.Result != null)
			{
				this.result.AddSchema(csdlDocumentParser.Result.Value);
			}
			return this.success;
		}

		public bool GetResult(out CsdlModel model, out IEnumerable<EdmError> errors)
		{
			model = this.result;
			errors = this.errorsList;
			return this.success;
		}

		public static bool TryParse(IEnumerable<XmlReader> csdlReaders, out CsdlModel entityModel, out IEnumerable<EdmError> errors)
		{
			bool flag;
			EdmUtil.CheckArgumentNull<IEnumerable<XmlReader>>(csdlReaders, "csdlReaders");
			CsdlParser csdlParser = new CsdlParser();
			int num = 0;
			IEnumerator<XmlReader> enumerator = csdlReaders.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					XmlReader current = enumerator.Current;
					if (current == null)
					{
						entityModel = null;
						EdmError[] edmError = new EdmError[1];
						edmError[0] = new EdmError(null, EdmErrorCode.NullXmlReader, Strings.CsdlParser_NullXmlReader);
						errors = edmError;
						flag = false;
						return flag;
					}
					else
					{
						try
						{
							csdlParser.AddReader(current);
						}
						catch (XmlException xmlException1)
						{
							XmlException xmlException = xmlException1;
							entityModel = null;
							EdmError[] edmErrorArray = new EdmError[1];
							edmErrorArray[0] = new EdmError(new CsdlLocation(xmlException.LineNumber, xmlException.LinePosition), EdmErrorCode.XmlError, xmlException.Message);
							errors = edmErrorArray;
							flag = false;
							return flag;
						}
						num++;
					}
				}
				goto Label0;
			}
			return flag;
		Label0:
			if (num != 0)
			{
				bool result = csdlParser.GetResult(out entityModel, out errors);
				if (!result)
				{
					entityModel = null;
				}
				return result;
			}
			else
			{
				entityModel = null;
				EdmError[] edmError1 = new EdmError[1];
				edmError1[0] = new EdmError(null, EdmErrorCode.NoReadersProvided, Strings.CsdlParser_NoReadersProvided);
				errors = edmError1;
				return false;
			}
		}
	}
}