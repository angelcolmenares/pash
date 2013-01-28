using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsPropertyReferenceExpression : CsdlSemanticsExpression, IEdmPropertyReferenceExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlPropertyReferenceExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsPropertyReferenceExpression, IEdmExpression> baseCache;

		private readonly static Func<CsdlSemanticsPropertyReferenceExpression, IEdmExpression> ComputeBaseFunc;

		private readonly Cache<CsdlSemanticsPropertyReferenceExpression, IEdmProperty> elementCache;

		private readonly static Func<CsdlSemanticsPropertyReferenceExpression, IEdmProperty> ComputeReferencedFunc;

		public IEdmExpression Base
		{
			get
			{
				return this.baseCache.GetValue(this, CsdlSemanticsPropertyReferenceExpression.ComputeBaseFunc, null);
			}
		}

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
				if (this.ReferencedProperty as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.ReferencedProperty.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.PropertyReference;
			}
		}

		public IEdmProperty ReferencedProperty
		{
			get
			{
				return this.elementCache.GetValue(this, CsdlSemanticsPropertyReferenceExpression.ComputeReferencedFunc, null);
			}
		}

		static CsdlSemanticsPropertyReferenceExpression()
		{
			CsdlSemanticsPropertyReferenceExpression.ComputeBaseFunc = (CsdlSemanticsPropertyReferenceExpression me) => me.ComputeBase();
			CsdlSemanticsPropertyReferenceExpression.ComputeReferencedFunc = (CsdlSemanticsPropertyReferenceExpression me) => me.ComputeReferenced();
		}

		public CsdlSemanticsPropertyReferenceExpression(CsdlPropertyReferenceExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.baseCache = new Cache<CsdlSemanticsPropertyReferenceExpression, IEdmExpression>();
			this.elementCache = new Cache<CsdlSemanticsPropertyReferenceExpression, IEdmProperty>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmExpression ComputeBase()
		{
			if (this.expression.BaseExpression != null)
			{
				return CsdlSemanticsModel.WrapExpression(this.expression.BaseExpression, this.bindingContext, base.Schema);
			}
			else
			{
				return null;
			}
		}

		private IEdmProperty ComputeReferenced()
		{
			IEdmEntityType edmEntityType = this.bindingContext;
			IEdmStructuredType badEntityType = edmEntityType;
			if (edmEntityType == null)
			{
				badEntityType = new BadEntityType("", new EdmError[0]);
			}
			return new UnresolvedProperty(badEntityType, this.expression.Property, base.Location);
		}
	}
}