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
	internal class CsdlSemanticsFunctionReferenceExpression : CsdlSemanticsExpression, IEdmFunctionReferenceExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlFunctionReferenceExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsFunctionReferenceExpression, IEdmFunction> referencedCache;

		private readonly static Func<CsdlSemanticsFunctionReferenceExpression, IEdmFunction> ComputeReferencedFunc;

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
				if (this.ReferencedFunction as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.ReferencedFunction.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.FunctionReference;
			}
		}

		public IEdmFunction ReferencedFunction
		{
			get
			{
				return this.referencedCache.GetValue(this, CsdlSemanticsFunctionReferenceExpression.ComputeReferencedFunc, null);
			}
		}

		static CsdlSemanticsFunctionReferenceExpression()
		{
			CsdlSemanticsFunctionReferenceExpression.ComputeReferencedFunc = (CsdlSemanticsFunctionReferenceExpression me) => me.ComputeReferenced();
		}

		public CsdlSemanticsFunctionReferenceExpression(CsdlFunctionReferenceExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.referencedCache = new Cache<CsdlSemanticsFunctionReferenceExpression, IEdmFunction>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmFunction ComputeReferenced()
		{
			return new UnresolvedFunction(this.expression.Function, Strings.Bad_UnresolvedFunction(this.expression.Function), base.Location);
		}
	}
}