using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Windows.Markup;

namespace System.Activities.Expressions
{
	public sealed class ValueTypeFieldReference<TOperand, TResult> : CodeActivity<Location<TResult>>
	{
		public string FieldName { get; set; }
		public InOutArgument<TOperand> OperandLocation { get; set; }

		protected override Location<TResult> Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
