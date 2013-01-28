using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsPathExpression : CsdlSemanticsExpression, IEdmPathExpression, IEdmExpression, IEdmElement
	{
		private readonly CsdlPathExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsPathExpression, IEnumerable<string>> pathCache;

		private readonly static Func<CsdlSemanticsPathExpression, IEnumerable<string>> ComputePathFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.expression;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Path;
			}
		}

		public IEnumerable<string> Path
		{
			get
			{
				return this.pathCache.GetValue(this, CsdlSemanticsPathExpression.ComputePathFunc, null);
			}
		}

		static CsdlSemanticsPathExpression()
		{
			CsdlSemanticsPathExpression.ComputePathFunc = (CsdlSemanticsPathExpression me) => me.ComputePath();
		}

		public CsdlSemanticsPathExpression(CsdlPathExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.pathCache = new Cache<CsdlSemanticsPathExpression, IEnumerable<string>>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEnumerable<string> ComputePath()
		{
			char[] chrArray = new char[1];
			chrArray[0] = '/';
			return this.expression.Path.Split(chrArray, StringSplitOptions.None);
		}
	}
}