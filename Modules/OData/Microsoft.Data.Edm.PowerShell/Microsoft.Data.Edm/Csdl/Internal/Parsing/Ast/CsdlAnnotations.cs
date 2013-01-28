using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlAnnotations
	{
		private readonly List<CsdlVocabularyAnnotationBase> annotations;

		private readonly string target;

		private readonly string qualifier;

		public IEnumerable<CsdlVocabularyAnnotationBase> Annotations
		{
			get
			{
				return this.annotations;
			}
		}

		public string Qualifier
		{
			get
			{
				return this.qualifier;
			}
		}

		public string Target
		{
			get
			{
				return this.target;
			}
		}

		public CsdlAnnotations(IEnumerable<CsdlVocabularyAnnotationBase> annotations, string target, string qualifier)
		{
			this.annotations = new List<CsdlVocabularyAnnotationBase>(annotations);
			this.target = target;
			this.qualifier = qualifier;
		}
	}
}