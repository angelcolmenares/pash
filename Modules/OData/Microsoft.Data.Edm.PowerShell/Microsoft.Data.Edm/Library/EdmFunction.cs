using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmFunction : EdmFunctionBase, IEdmFunction, IEdmFunctionBase, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		private readonly string definingExpression;

		public string DefiningExpression
		{
			get
			{
				return this.definingExpression;
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.Function;
			}
		}

		public EdmFunction(string namespaceName, string name, IEdmTypeReference returnType) : this(namespaceName, name, returnType, null)
		{
		}

		public EdmFunction(string namespaceName, string name, IEdmTypeReference returnType, string definingExpression) : base(name, returnType)
		{
			EdmUtil.CheckArgumentNull<string>(namespaceName, "namespaceName");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(returnType, "returnType");
			this.namespaceName = namespaceName;
			this.definingExpression = definingExpression;
		}
	}
}