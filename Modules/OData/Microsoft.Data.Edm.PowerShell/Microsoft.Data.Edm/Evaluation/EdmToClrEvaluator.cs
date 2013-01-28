using Microsoft.Data.Edm;
using Microsoft.Data.Edm.EdmToClrConversion;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Evaluation
{
	internal class EdmToClrEvaluator : EdmExpressionEvaluator
	{
		private EdmToClrConverter edmToClrConverter;

		public EdmToClrConverter EdmToClrConverter
		{
			get
			{
				return this.edmToClrConverter;
			}
			set
			{
				EdmUtil.CheckArgumentNull<EdmToClrConverter>(value, "value");
				this.edmToClrConverter = value;
			}
		}

		public EdmToClrEvaluator(IDictionary<IEdmFunction, Func<IEdmValue[], IEdmValue>> builtInFunctions) : base(builtInFunctions)
		{
			this.edmToClrConverter = new EdmToClrConverter();
		}

		public EdmToClrEvaluator(IDictionary<IEdmFunction, Func<IEdmValue[], IEdmValue>> builtInFunctions, Func<string, IEdmValue[], IEdmValue> lastChanceFunctionApplier) : base(builtInFunctions, lastChanceFunctionApplier)
		{
			this.edmToClrConverter = new EdmToClrConverter();
		}

		public T EvaluateToClrValue<T>(IEdmExpression expression)
		{
			IEdmValue edmValue = base.Evaluate(expression);
			return this.edmToClrConverter.AsClrValue<T>(edmValue);
		}

		public T EvaluateToClrValue<T>(IEdmExpression expression, IEdmStructuredValue context)
		{
			IEdmValue edmValue = base.Evaluate(expression, context);
			return this.edmToClrConverter.AsClrValue<T>(edmValue);
		}

		public T EvaluateToClrValue<T>(IEdmExpression expression, IEdmStructuredValue context, IEdmTypeReference targetType)
		{
			IEdmValue edmValue = base.Evaluate(expression, context, targetType);
			return this.edmToClrConverter.AsClrValue<T>(edmValue);
		}
	}
}