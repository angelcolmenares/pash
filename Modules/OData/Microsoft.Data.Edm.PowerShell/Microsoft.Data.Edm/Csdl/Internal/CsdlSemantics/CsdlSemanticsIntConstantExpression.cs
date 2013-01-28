using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsIntConstantExpression : CsdlSemanticsExpression, IEdmIntegerConstantExpression, IEdmExpression, IEdmIntegerValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsIntConstantExpression, long> valueCache;

		private readonly static Func<CsdlSemanticsIntConstantExpression, long> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsIntConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsIntConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

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
				return this.errorsCache.GetValue(this, CsdlSemanticsIntConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.IntegerConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public long Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsIntConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsIntConstantExpression()
		{
			CsdlSemanticsIntConstantExpression.ComputeValueFunc = (CsdlSemanticsIntConstantExpression me) => me.ComputeValue();
			CsdlSemanticsIntConstantExpression.ComputeErrorsFunc = (CsdlSemanticsIntConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsIntConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsIntConstantExpression, long>();
			this.errorsCache = new Cache<CsdlSemanticsIntConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			long? nullable;
			if (EdmValueParser.TryParseLong(this.expression.Value, out nullable))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidInteger, Strings.ValueParser_InvalidInteger(this.expression.Value));
				return edmError;
			}
		}

		private long ComputeValue()
		{
			long? nullable;
			if (EdmValueParser.TryParseLong(this.expression.Value, out nullable))
			{
				return nullable.Value;
			}
			else
			{
				return (long)0;
			}
		}
	}
}