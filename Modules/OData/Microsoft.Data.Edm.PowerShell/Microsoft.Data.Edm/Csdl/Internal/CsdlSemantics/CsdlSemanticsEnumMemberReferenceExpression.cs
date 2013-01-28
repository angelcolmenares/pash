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
	internal class CsdlSemanticsEnumMemberReferenceExpression : CsdlSemanticsExpression, IEdmEnumMemberReferenceExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlEnumMemberReferenceExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsEnumMemberReferenceExpression, IEdmEnumMember> referencedCache;

		private readonly static Func<CsdlSemanticsEnumMemberReferenceExpression, IEdmEnumMember> ComputeReferencedFunc;

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
				if (this.ReferencedEnumMember as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.ReferencedEnumMember.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.EnumMemberReference;
			}
		}

		public IEdmEnumMember ReferencedEnumMember
		{
			get
			{
				return this.referencedCache.GetValue(this, CsdlSemanticsEnumMemberReferenceExpression.ComputeReferencedFunc, null);
			}
		}

		static CsdlSemanticsEnumMemberReferenceExpression()
		{
			CsdlSemanticsEnumMemberReferenceExpression.ComputeReferencedFunc = (CsdlSemanticsEnumMemberReferenceExpression me) => me.ComputeReferenced();
		}

		public CsdlSemanticsEnumMemberReferenceExpression(CsdlEnumMemberReferenceExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.referencedCache = new Cache<CsdlSemanticsEnumMemberReferenceExpression, IEdmEnumMember>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmEnumMember ComputeReferenced()
		{
			char[] chrArray = new char[1];
			chrArray[0] = '/';
			string[] strArrays = this.expression.EnumMemberPath.Split(chrArray);
			return new UnresolvedEnumMember(strArrays[1], new UnresolvedEnumType(strArrays[0], base.Location), base.Location);
		}
	}
}