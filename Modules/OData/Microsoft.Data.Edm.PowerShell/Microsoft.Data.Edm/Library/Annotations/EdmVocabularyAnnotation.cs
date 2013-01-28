using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal abstract class EdmVocabularyAnnotation : EdmElement, IEdmVocabularyAnnotation, IEdmElement
	{
		private readonly IEdmVocabularyAnnotatable target;

		private readonly IEdmTerm term;

		private readonly string qualifier;

		public string Qualifier
		{
			get
			{
				return this.qualifier;
			}
		}

		public IEdmVocabularyAnnotatable Target
		{
			get
			{
				return this.target;
			}
		}

		public IEdmTerm Term
		{
			get
			{
				return this.term;
			}
		}

		protected EdmVocabularyAnnotation(IEdmVocabularyAnnotatable target, IEdmTerm term, string qualifier)
		{
			EdmUtil.CheckArgumentNull<IEdmVocabularyAnnotatable>(target, "target");
			EdmUtil.CheckArgumentNull<IEdmTerm>(term, "term");
			this.target = target;
			this.term = term;
			this.qualifier = qualifier;
		}
	}
}