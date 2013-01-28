using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmValueTerm : EdmNamedElement, IEdmValueTerm, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		private readonly IEdmTypeReference type;

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
				return EdmSchemaElementKind.ValueTerm;
			}
		}

		public EdmTermKind TermKind
		{
			get
			{
				return EdmTermKind.Value;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public EdmValueTerm(string namespaceName, string name, EdmPrimitiveTypeKind type) : this(namespaceName, name, EdmCoreModel.Instance.GetPrimitive(type, true))
		{
		}

		public EdmValueTerm(string namespaceName, string name, IEdmTypeReference type) : base(name)
		{
			EdmUtil.CheckArgumentNull<string>(namespaceName, "namespaceName");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			this.namespaceName = namespaceName;
			this.type = type;
		}
	}
}