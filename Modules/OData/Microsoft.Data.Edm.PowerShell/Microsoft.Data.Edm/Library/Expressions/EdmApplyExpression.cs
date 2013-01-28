using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmApplyExpression : EdmElement, IEdmApplyExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmExpression appliedFunction;

		private readonly IEnumerable<IEdmExpression> arguments;

		public IEdmExpression AppliedFunction
		{
			get
			{
				return this.appliedFunction;
			}
		}

		public IEnumerable<IEdmExpression> Arguments
		{
			get
			{
				return this.arguments;
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.FunctionApplication;
			}
		}

		public EdmApplyExpression(IEdmFunction appliedFunction, IEdmExpression[] arguments) : this(appliedFunction, (IEnumerable<IEdmExpression>)arguments)
		{
		}

		public EdmApplyExpression(IEdmFunction appliedFunction, IEnumerable<IEdmExpression> arguments) : this(new EdmFunctionReferenceExpression(EdmUtil.CheckArgumentNull<IEdmFunction>(appliedFunction, "appliedFunction")), arguments)
		{
		}

		public EdmApplyExpression(IEdmExpression appliedFunction, IEnumerable<IEdmExpression> arguments)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(appliedFunction, "appliedFunction");
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmExpression>>(arguments, "arguments");
			this.appliedFunction = appliedFunction;
			this.arguments = arguments;
		}
	}
}