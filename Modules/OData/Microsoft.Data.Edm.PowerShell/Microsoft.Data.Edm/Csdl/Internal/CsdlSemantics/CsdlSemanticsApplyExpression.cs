using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Expressions;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsApplyExpression : CsdlSemanticsExpression, IEdmApplyExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlApplyExpression expression;

		private readonly CsdlSemanticsSchema schema;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsApplyExpression, IEdmExpression> appliedFunctionCache;

		private readonly static Func<CsdlSemanticsApplyExpression, IEdmExpression> ComputeAppliedFunctionFunc;

		private readonly Cache<CsdlSemanticsApplyExpression, IEnumerable<IEdmExpression>> argumentsCache;

		private readonly static Func<CsdlSemanticsApplyExpression, IEnumerable<IEdmExpression>> ComputeArgumentsFunc;

		public IEdmExpression AppliedFunction
		{
			get
			{
				return this.appliedFunctionCache.GetValue(this, CsdlSemanticsApplyExpression.ComputeAppliedFunctionFunc, null);
			}
		}

		public IEnumerable<IEdmExpression> Arguments
		{
			get
			{
				return this.argumentsCache.GetValue(this, CsdlSemanticsApplyExpression.ComputeArgumentsFunc, null);
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
				if (this.AppliedFunction as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.AppliedFunction.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.FunctionApplication;
			}
		}

		static CsdlSemanticsApplyExpression()
		{
			CsdlSemanticsApplyExpression.ComputeAppliedFunctionFunc = (CsdlSemanticsApplyExpression me) => me.ComputeAppliedFunction();
			CsdlSemanticsApplyExpression.ComputeArgumentsFunc = (CsdlSemanticsApplyExpression me) => me.ComputeArguments();
		}

		public CsdlSemanticsApplyExpression(CsdlApplyExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.appliedFunctionCache = new Cache<CsdlSemanticsApplyExpression, IEdmExpression>();
			this.argumentsCache = new Cache<CsdlSemanticsApplyExpression, IEnumerable<IEdmExpression>>();
			this.expression = expression;
			this.bindingContext = bindingContext;
			this.schema = schema;
		}

		private IEdmExpression ComputeAppliedFunction()
		{
			IEdmFunction unresolvedFunction;
			if (this.expression.Function != null)
			{
				IEnumerable<IEdmFunction> edmFunctions = this.schema.FindFunctions(this.expression.Function);
				if (edmFunctions.Count<IEdmFunction>() != 0)
				{
					edmFunctions = edmFunctions.Where<IEdmFunction>(new Func<IEdmFunction, bool>(this.IsMatchingFunction));
					if (edmFunctions.Count<IEdmFunction>() <= 1)
					{
						if (edmFunctions.Count<IEdmFunction>() != 0)
						{
							unresolvedFunction = edmFunctions.Single<IEdmFunction>();
						}
						else
						{
							unresolvedFunction = new UnresolvedFunction(this.expression.Function, Strings.Bad_FunctionParametersDontMatch(this.expression.Function), base.Location);
						}
					}
					else
					{
						unresolvedFunction = new UnresolvedFunction(this.expression.Function, Strings.Bad_AmbiguousFunction(this.expression.Function), base.Location);
					}
				}
				else
				{
					unresolvedFunction = new UnresolvedFunction(this.expression.Function, Strings.Bad_UnresolvedFunction(this.expression.Function), base.Location);
				}
				return new EdmFunctionReferenceExpression(unresolvedFunction);
			}
			else
			{
				return CsdlSemanticsModel.WrapExpression(this.expression.Arguments.FirstOrDefault<CsdlExpressionBase>(null), this.bindingContext, this.schema);
			}
		}

		private IEnumerable<IEdmExpression> ComputeArguments()
		{
			bool function = this.expression.Function == null;
			List<IEdmExpression> edmExpressions = new List<IEdmExpression>();
			foreach (CsdlExpressionBase argument in this.expression.Arguments)
			{
				if (!function)
				{
					edmExpressions.Add(CsdlSemanticsModel.WrapExpression(argument, this.bindingContext, this.schema));
				}
				else
				{
					function = false;
				}
			}
			return edmExpressions;
		}

		private bool IsMatchingFunction(IEdmFunction function)
		{
			IEnumerable<EdmError> edmErrors = null;
			bool flag;
			if (function.Parameters.Count<IEdmFunctionParameter>() == this.Arguments.Count<IEdmExpression>())
			{
				IEnumerator<IEdmExpression> enumerator = this.Arguments.GetEnumerator();
				IEnumerator<IEdmFunctionParameter> enumerator1 = function.Parameters.GetEnumerator();
				using (enumerator1)
				{
					while (enumerator1.MoveNext())
					{
						IEdmFunctionParameter current = enumerator1.Current;
						enumerator.MoveNext();
						if (enumerator.Current.TryAssertType(current.Type, out edmErrors))
						{
							continue;
						}
						flag = false;
						return flag;
					}
					return true;
				}
				return flag;
			}
			else
			{
				return false;
			}
		}
	}
}