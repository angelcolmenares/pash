using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmFunctionBase : IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEnumerable<IEdmFunctionParameter> Parameters
		{
			get;
		}

		IEdmTypeReference ReturnType
		{
			get;
		}

		IEdmFunctionParameter FindParameter(string name);
	}
}