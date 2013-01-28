using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedFunction : BadElement, IEdmFunction, IEdmFunctionBase, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement, IUnresolvedElement
	{
		private readonly string namespaceName;

		private readonly string name;

		private readonly IEdmTypeReference returnType;

		public string DefiningExpression
		{
			get
			{
				return null;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public IEnumerable<IEdmFunctionParameter> Parameters
		{
			get
			{
				return Enumerable.Empty<IEdmFunctionParameter>();
			}
		}

		public IEdmTypeReference ReturnType
		{
			get
			{
				return this.returnType;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.Function;
			}
		}

		public UnresolvedFunction(string qualifiedName, string errorMessage, EdmLocation location)
			: base(new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedFunction, errorMessage) })
		{
			string str = qualifiedName;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			qualifiedName = empty;
			EdmUtil.TryGetNamespaceNameFromQualifiedName(qualifiedName, out this.namespaceName, out this.name);
			this.returnType = new BadTypeReference(new BadType(base.Errors), true);
		}

		public IEdmFunctionParameter FindParameter(string name)
		{
			return null;
		}
	}
}