using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsTypeAnnotation : CsdlSemanticsVocabularyAnnotation, IEdmTypeAnnotation, IEdmVocabularyAnnotation, IEdmElement
	{
		private readonly Cache<CsdlSemanticsTypeAnnotation, IEnumerable<IEdmPropertyValueBinding>> propertiesCache;

		private readonly static Func<CsdlSemanticsTypeAnnotation, IEnumerable<IEdmPropertyValueBinding>> ComputePropertiesFunc;

		public IEnumerable<IEdmPropertyValueBinding> PropertyValueBindings
		{
			get
			{
				return this.propertiesCache.GetValue(this, CsdlSemanticsTypeAnnotation.ComputePropertiesFunc, null);
			}
		}

		static CsdlSemanticsTypeAnnotation()
		{
			CsdlSemanticsTypeAnnotation.ComputePropertiesFunc = (CsdlSemanticsTypeAnnotation me) => me.ComputeProperties();
		}

		public CsdlSemanticsTypeAnnotation(CsdlSemanticsSchema schema, IEdmVocabularyAnnotatable targetContext, CsdlSemanticsAnnotations annotationsContext, CsdlTypeAnnotation annotation, string externalQualifier) : base(schema, targetContext, annotationsContext, annotation, externalQualifier)
		{
			this.propertiesCache = new Cache<CsdlSemanticsTypeAnnotation, IEnumerable<IEdmPropertyValueBinding>>();
		}

		private IEnumerable<IEdmPropertyValueBinding> ComputeProperties()
		{
			List<IEdmPropertyValueBinding> edmPropertyValueBindings = new List<IEdmPropertyValueBinding>();
			foreach (CsdlPropertyValue property in ((CsdlTypeAnnotation)this.Annotation).Properties)
			{
				edmPropertyValueBindings.Add(new CsdlSemanticsPropertyValueBinding(this, property));
			}
			return edmPropertyValueBindings;
		}

		protected override IEdmTerm ComputeTerm()
		{
			IEdmStructuredType edmStructuredType = base.Schema.FindType(this.Annotation.Term) as IEdmStructuredType;
			object unresolvedTypeTerm = edmStructuredType;
			if (edmStructuredType == null)
			{
				unresolvedTypeTerm = new UnresolvedTypeTerm(base.Schema.UnresolvedName(this.Annotation.Term));
			}
			return (IEdmTerm)unresolvedTypeTerm;
		}
	}
}