using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmFunction : IEdmFunctionBase, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		string DefiningExpression
		{
			get;
		}

	}
}