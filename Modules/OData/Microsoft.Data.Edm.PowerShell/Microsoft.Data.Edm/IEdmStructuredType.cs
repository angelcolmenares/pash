using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmStructuredType : IEdmType, IEdmElement
	{
		IEdmStructuredType BaseType
		{
			get;
		}

		IEnumerable<IEdmProperty> DeclaredProperties
		{
			get;
		}

		bool IsAbstract
		{
			get;
		}

		bool IsOpen
		{
			get;
		}

		IEdmProperty FindProperty(string name);
	}
}