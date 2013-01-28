using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsValueAnnotation : CsdlSemanticsVocabularyAnnotation, IEdmValueAnnotation, IEdmVocabularyAnnotation, IEdmElement
	{
		private readonly Cache<CsdlSemanticsValueAnnotation, IEdmExpression> valueCache;

		private readonly static Func<CsdlSemanticsValueAnnotation, IEdmExpression> ComputeValueFunc;

		public IEdmExpression Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsValueAnnotation.ComputeValueFunc, null);
			}
		}

		static CsdlSemanticsValueAnnotation()
		{
			CsdlSemanticsValueAnnotation.ComputeValueFunc = (CsdlSemanticsValueAnnotation me) => me.ComputeValue();
		}

		public CsdlSemanticsValueAnnotation(CsdlSemanticsSchema schema, IEdmVocabularyAnnotatable targetContext, CsdlSemanticsAnnotations annotationsContext, CsdlValueAnnotation annotation, string externalQualifier) : base(schema, targetContext, annotationsContext, annotation, externalQualifier)
		{
			this.valueCache = new Cache<CsdlSemanticsValueAnnotation, IEdmExpression>();
		}

		protected override IEdmTerm ComputeTerm()
		{
			IEdmValueTerm edmValueTerm = base.Schema.FindValueTerm(this.Annotation.Term);
			IEdmTerm unresolvedValueTerm = edmValueTerm;
			if (edmValueTerm == null)
			{
				unresolvedValueTerm = new UnresolvedValueTerm(base.Schema.UnresolvedName(this.Annotation.Term));
			}
			return unresolvedValueTerm;
		}

		private IEdmExpression ComputeValue()
		{
			return CsdlSemanticsModel.WrapExpression(((CsdlValueAnnotation)this.Annotation).Expression, base.TargetBindingContext, base.Schema);
		}
	}
}