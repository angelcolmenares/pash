using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal abstract class EdmProperty : EdmNamedElement, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly IEdmStructuredType declaringType;

		private readonly HashSetInternal<IDependent> dependents;

		private readonly IEdmTypeReference type;

		public IEdmStructuredType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public abstract EdmPropertyKind PropertyKind
		{
			get;
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		protected EdmProperty(IEdmStructuredType declaringType, string name, IEdmTypeReference type) : base(name)
		{
			this.dependents = new HashSetInternal<IDependent>();
			EdmUtil.CheckArgumentNull<IEdmStructuredType>(declaringType, "declaringType");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			this.declaringType = declaringType;
			this.type = type;
		}
	}
}