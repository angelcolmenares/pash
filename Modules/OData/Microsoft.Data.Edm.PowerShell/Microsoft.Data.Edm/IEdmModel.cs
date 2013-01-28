using Microsoft.Data.Edm.Annotations;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmModel : IEdmElement
	{
		IEdmDirectValueAnnotationsManager DirectValueAnnotationsManager
		{
			get;
		}

		IEnumerable<IEdmModel> ReferencedModels
		{
			get;
		}

		IEnumerable<IEdmSchemaElement> SchemaElements
		{
			get;
		}

		IEnumerable<IEdmVocabularyAnnotation> VocabularyAnnotations
		{
			get;
		}

		IEdmEntityContainer FindDeclaredEntityContainer(string name);

		IEnumerable<IEdmFunction> FindDeclaredFunctions(string qualifiedName);

		IEdmSchemaType FindDeclaredType(string qualifiedName);

		IEdmValueTerm FindDeclaredValueTerm(string qualifiedName);

		IEnumerable<IEdmVocabularyAnnotation> FindDeclaredVocabularyAnnotations(IEdmVocabularyAnnotatable element);

		IEnumerable<IEdmStructuredType> FindDirectlyDerivedTypes(IEdmStructuredType baseType);
	}
}