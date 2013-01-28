using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousPropertyBinding : AmbiguousBinding<IEdmProperty>, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEdmStructuredType declaringType;

		private readonly Cache<AmbiguousPropertyBinding, IEdmTypeReference> type;

		private readonly static Func<AmbiguousPropertyBinding, IEdmTypeReference> ComputeTypeFunc;

		public IEdmStructuredType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public EdmPropertyKind PropertyKind
		{
			get
			{
				return EdmPropertyKind.None;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type.GetValue(this, AmbiguousPropertyBinding.ComputeTypeFunc, null);
			}
		}

		static AmbiguousPropertyBinding()
		{
			AmbiguousPropertyBinding.ComputeTypeFunc = (AmbiguousPropertyBinding me) => me.ComputeType();
		}

		public AmbiguousPropertyBinding(IEdmStructuredType declaringType, IEdmProperty first, IEdmProperty second) : base(first, second)
		{
			this.type = new Cache<AmbiguousPropertyBinding, IEdmTypeReference>();
			this.declaringType = declaringType;
		}

		private IEdmTypeReference ComputeType()
		{
			return new BadTypeReference(new BadType(base.Errors), true);
		}
	}
}