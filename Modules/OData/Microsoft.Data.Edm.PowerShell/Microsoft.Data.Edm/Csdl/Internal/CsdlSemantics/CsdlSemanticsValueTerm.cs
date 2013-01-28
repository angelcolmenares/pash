using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsValueTerm : CsdlSemanticsElement, IEdmValueTerm, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		protected readonly CsdlSemanticsSchema Context;

		protected CsdlValueTerm valueTerm;

		private readonly Cache<CsdlSemanticsValueTerm, IEdmTypeReference> typeCache;

		private readonly static Func<CsdlSemanticsValueTerm, IEdmTypeReference> ComputeTypeFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.valueTerm;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.Context.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.valueTerm.Name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.Context.Namespace;
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
				return this.typeCache.GetValue(this, CsdlSemanticsValueTerm.ComputeTypeFunc, null);
			}
		}

		static CsdlSemanticsValueTerm()
		{
			CsdlSemanticsValueTerm.ComputeTypeFunc = (CsdlSemanticsValueTerm me) => me.ComputeType();
		}

		public CsdlSemanticsValueTerm(CsdlSemanticsSchema context, CsdlValueTerm valueTerm) : base(valueTerm)
		{
			this.typeCache = new Cache<CsdlSemanticsValueTerm, IEdmTypeReference>();
			this.Context = context;
			this.valueTerm = valueTerm;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.Context);
		}

		private IEdmTypeReference ComputeType()
		{
			return CsdlSemanticsModel.WrapTypeReference(this.Context, this.valueTerm.Type);
		}
	}
}