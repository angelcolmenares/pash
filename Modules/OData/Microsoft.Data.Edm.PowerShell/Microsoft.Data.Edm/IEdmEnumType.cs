using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmEnumType : IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
	{
		bool IsFlags
		{
			get;
		}

		IEnumerable<IEdmEnumMember> Members
		{
			get;
		}

		IEdmPrimitiveType UnderlyingType
		{
			get;
		}

	}
}