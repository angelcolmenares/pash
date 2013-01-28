using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousValueTermBinding : AmbiguousBinding<IEdmValueTerm>, IEdmValueTerm, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEdmValueTerm first;

		private readonly Cache<AmbiguousValueTermBinding, IEdmTypeReference> type;

		private readonly static Func<AmbiguousValueTermBinding, IEdmTypeReference> ComputeTypeFunc;

		public string Namespace
		{
			get
			{
				string @namespace = this.first.Namespace;
				string empty = @namespace;
				if (@namespace == null)
				{
					empty = string.Empty;
				}
				return empty;
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
				return this.type.GetValue(this, AmbiguousValueTermBinding.ComputeTypeFunc, null);
			}
		}

		static AmbiguousValueTermBinding()
		{
			AmbiguousValueTermBinding.ComputeTypeFunc = (AmbiguousValueTermBinding me) => me.ComputeType();
		}

		public AmbiguousValueTermBinding(IEdmValueTerm first, IEdmValueTerm second) : base(first, second)
		{
			this.type = new Cache<AmbiguousValueTermBinding, IEdmTypeReference>();
			this.first = first;
		}

		private IEdmTypeReference ComputeType()
		{
			return new BadTypeReference(new BadType(base.Errors), true);
		}
	}
}