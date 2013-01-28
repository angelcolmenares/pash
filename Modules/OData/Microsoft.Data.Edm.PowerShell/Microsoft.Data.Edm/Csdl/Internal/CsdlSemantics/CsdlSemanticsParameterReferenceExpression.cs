using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsParameterReferenceExpression : CsdlSemanticsExpression, IEdmParameterReferenceExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlParameterReferenceExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsParameterReferenceExpression, IEdmFunctionParameter> referencedCache;

		private readonly static Func<CsdlSemanticsParameterReferenceExpression, IEdmFunctionParameter> ComputeReferencedFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.expression;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				if (this.ReferencedParameter as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.ReferencedParameter.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.ParameterReference;
			}
		}

		public IEdmFunctionParameter ReferencedParameter
		{
			get
			{
				return this.referencedCache.GetValue(this, CsdlSemanticsParameterReferenceExpression.ComputeReferencedFunc, null);
			}
		}

		static CsdlSemanticsParameterReferenceExpression()
		{
			CsdlSemanticsParameterReferenceExpression.ComputeReferencedFunc = (CsdlSemanticsParameterReferenceExpression me) => me.ComputeReferenced();
		}

		public CsdlSemanticsParameterReferenceExpression(CsdlParameterReferenceExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.referencedCache = new Cache<CsdlSemanticsParameterReferenceExpression, IEdmFunctionParameter>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmFunctionParameter ComputeReferenced()
		{
			return new UnresolvedParameter(new UnresolvedFunction(string.Empty, Strings.Bad_UnresolvedFunction(string.Empty), base.Location), this.expression.Parameter, base.Location);
		}
	}
}