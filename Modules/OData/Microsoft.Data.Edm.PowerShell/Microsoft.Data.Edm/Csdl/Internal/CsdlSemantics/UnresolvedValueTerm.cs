using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedValueTerm : UnresolvedVocabularyTerm, IEdmValueTerm, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly UnresolvedValueTerm.UnresolvedValueTermTypeReference type;

		public override EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.ValueTerm;
			}
		}

		public override EdmTermKind TermKind
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

		public UnresolvedValueTerm(string qualifiedName) : base(qualifiedName)
		{
			this.type = new UnresolvedValueTerm.UnresolvedValueTermTypeReference();
		}

		private class UnresolvedValueTermTypeReference : IEdmTypeReference, IEdmElement
		{
			private readonly UnresolvedValueTerm.UnresolvedValueTermTypeReference.UnresolvedValueTermType definition;

			public IEdmType Definition
			{
				get
				{
					return this.definition;
				}
			}

			public bool IsNullable
			{
				get
				{
					return false;
				}
			}

			public UnresolvedValueTermTypeReference()
			{
				this.definition = new UnresolvedValueTerm.UnresolvedValueTermTypeReference.UnresolvedValueTermType();
			}

			private class UnresolvedValueTermType : IEdmType, IEdmElement
			{
				public EdmTypeKind TypeKind
				{
					get
					{
						return EdmTypeKind.None;
					}
				}

				public UnresolvedValueTermType()
				{
				}
			}
		}
	}
}