using Microsoft.Data.Edm;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Annotations
{
	internal interface IEdmTypeAnnotation : IEdmVocabularyAnnotation, IEdmElement
	{
		IEnumerable<IEdmPropertyValueBinding> PropertyValueBindings
		{
			get;
		}

	}
}