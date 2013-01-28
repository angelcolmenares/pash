using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics;
using Microsoft.Data.Edm.Csdl.Internal.Parsing;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Data.Edm.Csdl
{
	internal static class CsdlReader
	{
		public static bool TryParse(IEnumerable<XmlReader> readers, out IEdmModel model, out IEnumerable<EdmError> errors)
		{
			return CsdlReader.TryParse(readers, Enumerable.Empty<IEdmModel>(), out model, out errors);
		}

		public static bool TryParse(IEnumerable<XmlReader> readers, IEdmModel reference, out IEdmModel model, out IEnumerable<EdmError> errors)
		{
			IEdmModel[] edmModelArray = new IEdmModel[1];
			edmModelArray[0] = reference;
			return CsdlReader.TryParse(readers, edmModelArray, out model, out errors);
		}

		public static bool TryParse(IEnumerable<XmlReader> readers, IEnumerable<IEdmModel> references, out IEdmModel model, out IEnumerable<EdmError> errors)
		{
			CsdlModel csdlModel = null;
			if (!CsdlParser.TryParse(readers, out csdlModel, out errors))
			{
				model = null;
				return false;
			}
			else
			{
				model = new CsdlSemanticsModel(csdlModel, new CsdlSemanticsDirectValueAnnotationsManager(), references);
				return true;
			}
		}
	}
}