using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmValueAnnotation : IEdmVocabularyAnnotation, IEdmElement
	{
		IEdmExpression Value
		{
			get;
		}

	}
}