using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsFunctionParameter : CsdlSemanticsElement, IEdmFunctionParameter, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly CsdlSemanticsFunctionBase declaringFunction;

		private readonly CsdlFunctionParameter parameter;

		private readonly Cache<CsdlSemanticsFunctionParameter, IEdmTypeReference> typeCache;

		private readonly static Func<CsdlSemanticsFunctionParameter, IEdmTypeReference> ComputeTypeFunc;

		public IEdmFunctionBase DeclaringFunction
		{
			get
			{
				return this.declaringFunction;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.parameter;
			}
		}

		public EdmFunctionParameterMode Mode
		{
			get
			{
				return this.parameter.Mode;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.declaringFunction.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.parameter.Name;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.typeCache.GetValue(this, CsdlSemanticsFunctionParameter.ComputeTypeFunc, null);
			}
		}

		static CsdlSemanticsFunctionParameter()
		{
			CsdlSemanticsFunctionParameter.ComputeTypeFunc = (CsdlSemanticsFunctionParameter me) => me.ComputeType();
		}

		public CsdlSemanticsFunctionParameter(CsdlSemanticsFunctionBase declaringFunction, CsdlFunctionParameter parameter) : base(parameter)
		{
			this.typeCache = new Cache<CsdlSemanticsFunctionParameter, IEdmTypeReference>();
			this.parameter = parameter;
			this.declaringFunction = declaringFunction;
		}

		protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
		{
			return this.Model.WrapInlineVocabularyAnnotations(this, this.declaringFunction.Context);
		}

		private IEdmTypeReference ComputeType()
		{
			return CsdlSemanticsModel.WrapTypeReference(this.declaringFunction.Context, this.parameter.Type);
		}
	}
}