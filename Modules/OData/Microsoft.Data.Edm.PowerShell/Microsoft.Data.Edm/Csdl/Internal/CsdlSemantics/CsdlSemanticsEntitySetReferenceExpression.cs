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
	internal class CsdlSemanticsEntitySetReferenceExpression : CsdlSemanticsExpression, IEdmEntitySetReferenceExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlEntitySetReferenceExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsEntitySetReferenceExpression, IEdmEntitySet> referencedCache;

		private readonly static Func<CsdlSemanticsEntitySetReferenceExpression, IEdmEntitySet> ComputeReferencedFunc;

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
				if (this.ReferencedEntitySet as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.ReferencedEntitySet.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.EntitySetReference;
			}
		}

		public IEdmEntitySet ReferencedEntitySet
		{
			get
			{
				return this.referencedCache.GetValue(this, CsdlSemanticsEntitySetReferenceExpression.ComputeReferencedFunc, null);
			}
		}

		static CsdlSemanticsEntitySetReferenceExpression()
		{
			CsdlSemanticsEntitySetReferenceExpression.ComputeReferencedFunc = (CsdlSemanticsEntitySetReferenceExpression me) => me.ComputeReferenced();
		}

		public CsdlSemanticsEntitySetReferenceExpression(CsdlEntitySetReferenceExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.referencedCache = new Cache<CsdlSemanticsEntitySetReferenceExpression, IEdmEntitySet>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmEntitySet ComputeReferenced()
		{
			char[] chrArray = new char[1];
			chrArray[0] = '/';
			string[] strArrays = this.expression.EntitySetPath.Split(chrArray);
			return new UnresolvedEntitySet(strArrays[1], new UnresolvedEntityContainer(strArrays[0], base.Location), base.Location);
		}
	}
}