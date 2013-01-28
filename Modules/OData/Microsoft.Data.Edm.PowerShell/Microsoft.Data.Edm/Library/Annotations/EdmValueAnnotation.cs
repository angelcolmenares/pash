using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmValueAnnotation : EdmVocabularyAnnotation, IEdmValueAnnotation, IEdmVocabularyAnnotation, IEdmElement
	{
		private readonly IEdmExpression @value;

		public IEdmExpression Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmValueAnnotation(IEdmVocabularyAnnotatable target, IEdmTerm term, IEdmExpression value) : this(target, term, null, value)
		{
		}

		public EdmValueAnnotation(IEdmVocabularyAnnotatable target, IEdmTerm term, string qualifier, IEdmExpression value) : base(target, term, qualifier)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(value, "value");
			this.@value = value;
		}
	}
}