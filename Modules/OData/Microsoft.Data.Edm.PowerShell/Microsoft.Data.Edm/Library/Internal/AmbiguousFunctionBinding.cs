using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousFunctionBinding : AmbiguousBinding<IEdmFunction>, IEdmFunction, IEdmFunctionBase, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		public string DefiningExpression
		{
			get
			{
				return null;
			}
		}

		public string Namespace
		{
			get
			{
				IEdmFunction edmFunction = base.Bindings.FirstOrDefault<IEdmFunction>();
				if (edmFunction != null)
				{
					return edmFunction.Namespace;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public IEnumerable<IEdmFunctionParameter> Parameters
		{
			get
			{
				IEdmFunction edmFunction = base.Bindings.FirstOrDefault<IEdmFunction>();
				if (edmFunction != null)
				{
					return edmFunction.Parameters;
				}
				else
				{
					return Enumerable.Empty<IEdmFunctionParameter>();
				}
			}
		}

		public IEdmTypeReference ReturnType
		{
			get
			{
				return null;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.Function;
			}
		}

		public AmbiguousFunctionBinding(IEdmFunction first, IEdmFunction second) : base(first, second)
		{
		}

		public IEdmFunctionParameter FindParameter(string name)
		{
			IEdmFunction edmFunction = base.Bindings.FirstOrDefault<IEdmFunction>();
			if (edmFunction != null)
			{
				return edmFunction.FindParameter(name);
			}
			else
			{
				return null;
			}
		}
	}
}