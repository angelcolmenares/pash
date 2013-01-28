using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlVocabularyAnnotationBase : CsdlElement
	{
		private readonly string qualifier;

		private readonly string term;

		public string Qualifier
		{
			get
			{
				return this.qualifier;
			}
		}

		public string Term
		{
			get
			{
				return this.term;
			}
		}

		protected CsdlVocabularyAnnotationBase(string term, string qualifier, CsdlLocation location) : base(location)
		{
			this.qualifier = qualifier;
			this.term = term;
		}
	}
}