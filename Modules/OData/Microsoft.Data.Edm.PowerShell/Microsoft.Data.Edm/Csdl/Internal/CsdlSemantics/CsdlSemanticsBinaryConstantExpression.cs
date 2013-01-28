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
	internal class CsdlSemanticsBinaryConstantExpression : CsdlSemanticsExpression, IEdmBinaryConstantExpression, IEdmExpression, IEdmBinaryValue, IEdmPrimitiveValue, IEdmValue, IEdmElement, IEdmCheckable
	{
		private readonly CsdlConstantExpression expression;

		private readonly Cache<CsdlSemanticsBinaryConstantExpression, byte[]> valueCache;

		private readonly static Func<CsdlSemanticsBinaryConstantExpression, byte[]> ComputeValueFunc;

		private readonly Cache<CsdlSemanticsBinaryConstantExpression, IEnumerable<EdmError>> errorsCache;

		private readonly static Func<CsdlSemanticsBinaryConstantExpression, IEnumerable<EdmError>> ComputeErrorsFunc;

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
				return this.errorsCache.GetValue(this, CsdlSemanticsBinaryConstantExpression.ComputeErrorsFunc, null);
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.BinaryConstant;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return null;
			}
		}

		public byte[] Value
		{
			get
			{
				return this.valueCache.GetValue(this, CsdlSemanticsBinaryConstantExpression.ComputeValueFunc, null);
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return this.expression.ValueKind;
			}
		}

		static CsdlSemanticsBinaryConstantExpression()
		{
			CsdlSemanticsBinaryConstantExpression.ComputeValueFunc = (CsdlSemanticsBinaryConstantExpression me) => me.ComputeValue();
			CsdlSemanticsBinaryConstantExpression.ComputeErrorsFunc = (CsdlSemanticsBinaryConstantExpression me) => me.ComputeErrors();
		}

		public CsdlSemanticsBinaryConstantExpression(CsdlConstantExpression expression, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.valueCache = new Cache<CsdlSemanticsBinaryConstantExpression, byte[]>();
			this.errorsCache = new Cache<CsdlSemanticsBinaryConstantExpression, IEnumerable<EdmError>>();
			this.expression = expression;
		}

		private IEnumerable<EdmError> ComputeErrors()
		{
			byte[] numArray = null;
			if (EdmValueParser.TryParseBinary(this.expression.Value, out numArray))
			{
				return Enumerable.Empty<EdmError>();
			}
			else
			{
				EdmError[] edmError = new EdmError[1];
				edmError[0] = new EdmError(base.Location, EdmErrorCode.InvalidBinary, Strings.ValueParser_InvalidBinary(this.expression.Value));
				return edmError;
			}
		}

		private byte[] ComputeValue()
		{
			byte[] numArray = null;
			if (EdmValueParser.TryParseBinary(this.expression.Value, out numArray))
			{
				return numArray;
			}
			else
			{
				return new byte[0];
			}
		}
	}
}