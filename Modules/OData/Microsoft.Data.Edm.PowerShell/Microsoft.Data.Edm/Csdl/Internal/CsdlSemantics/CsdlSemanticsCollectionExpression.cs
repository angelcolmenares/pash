using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsCollectionExpression : CsdlSemanticsExpression, IEdmCollectionExpression, IEdmExpression, IEdmElement
	{
		private readonly CsdlCollectionExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsCollectionExpression, IEdmTypeReference> declaredTypeCache;

		private readonly static Func<CsdlSemanticsCollectionExpression, IEdmTypeReference> ComputeDeclaredTypeFunc;

		private readonly Cache<CsdlSemanticsCollectionExpression, IEnumerable<IEdmExpression>> elementsCache;

		private readonly static Func<CsdlSemanticsCollectionExpression, IEnumerable<IEdmExpression>> ComputeElementsFunc;

		public IEdmTypeReference DeclaredType
		{
			get
			{
				return this.declaredTypeCache.GetValue(this, CsdlSemanticsCollectionExpression.ComputeDeclaredTypeFunc, null);
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.expression;
			}
		}

		public IEnumerable<IEdmExpression> Elements
		{
			get
			{
				return this.elementsCache.GetValue(this, CsdlSemanticsCollectionExpression.ComputeElementsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Collection;
			}
		}

		static CsdlSemanticsCollectionExpression()
		{
			CsdlSemanticsCollectionExpression.ComputeDeclaredTypeFunc = (CsdlSemanticsCollectionExpression me) => me.ComputeDeclaredType();
			CsdlSemanticsCollectionExpression.ComputeElementsFunc = (CsdlSemanticsCollectionExpression me) => me.ComputeElements();
		}

		public CsdlSemanticsCollectionExpression(CsdlCollectionExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.declaredTypeCache = new Cache<CsdlSemanticsCollectionExpression, IEdmTypeReference>();
			this.elementsCache = new Cache<CsdlSemanticsCollectionExpression, IEnumerable<IEdmExpression>>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmTypeReference ComputeDeclaredType()
		{
			if (this.expression.Type != null)
			{
				return CsdlSemanticsModel.WrapTypeReference(base.Schema, this.expression.Type);
			}
			else
			{
				return null;
			}
		}

		private IEnumerable<IEdmExpression> ComputeElements()
		{
			List<IEdmExpression> edmExpressions = new List<IEdmExpression>();
			foreach (CsdlExpressionBase elementValue in this.expression.ElementValues)
			{
				edmExpressions.Add(CsdlSemanticsModel.WrapExpression(elementValue, this.bindingContext, base.Schema));
			}
			return edmExpressions;
		}
	}
}