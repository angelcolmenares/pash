using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmFunctionImport : IEdmFunctionBase, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEdmExpression EntitySet
		{
			get;
		}

		bool IsBindable
		{
			get;
		}

		bool IsComposable
		{
			get;
		}

		bool IsSideEffecting
		{
			get;
		}

	}
}