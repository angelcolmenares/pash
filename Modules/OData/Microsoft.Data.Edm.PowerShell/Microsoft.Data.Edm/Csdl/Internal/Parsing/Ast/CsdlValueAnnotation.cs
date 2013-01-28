using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlValueAnnotation : CsdlVocabularyAnnotationBase
	{
		private readonly CsdlExpressionBase expression;

		public CsdlExpressionBase Expression
		{
			get
			{
				return this.expression;
			}
		}

		public CsdlValueAnnotation(string term, string qualifier, CsdlExpressionBase expression, CsdlLocation location) : base(term, qualifier, location)
		{
			this.expression = expression;
		}
	}
}