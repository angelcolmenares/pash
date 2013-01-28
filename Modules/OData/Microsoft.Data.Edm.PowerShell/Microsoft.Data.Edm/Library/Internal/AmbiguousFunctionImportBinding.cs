using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousFunctionImportBinding : AmbiguousBinding<IEdmFunctionImport>, IEdmFunctionImport, IEdmFunctionBase, IEdmEntityContainerElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		public IEdmEntityContainer Container
		{
			get
			{
				IEdmFunctionImport edmFunctionImport = base.Bindings.FirstOrDefault<IEdmFunctionImport>();
				if (edmFunctionImport != null)
				{
					return edmFunctionImport.Container;
				}
				else
				{
					return null;
				}
			}
		}

		public EdmContainerElementKind ContainerElementKind
		{
			get
			{
				return EdmContainerElementKind.FunctionImport;
			}
		}

		public IEdmExpression EntitySet
		{
			get
			{
				return null;
			}
		}

		public bool IsBindable
		{
			get
			{
				return false;
			}
		}

		public bool IsComposable
		{
			get
			{
				return false;
			}
		}

		public bool IsSideEffecting
		{
			get
			{
				return true;
			}
		}

		public IEnumerable<IEdmFunctionParameter> Parameters
		{
			get
			{
				IEdmFunctionImport edmFunctionImport = base.Bindings.FirstOrDefault<IEdmFunctionImport>();
				if (edmFunctionImport != null)
				{
					return edmFunctionImport.Parameters;
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

		public AmbiguousFunctionImportBinding(IEdmFunctionImport first, IEdmFunctionImport second) : base(first, second)
		{
		}

		public IEdmFunctionParameter FindParameter(string name)
		{
			IEdmFunctionImport edmFunctionImport = base.Bindings.FirstOrDefault<IEdmFunctionImport>();
			if (edmFunctionImport != null)
			{
				return edmFunctionImport.FindParameter(name);
			}
			else
			{
				return null;
			}
		}
	}
}