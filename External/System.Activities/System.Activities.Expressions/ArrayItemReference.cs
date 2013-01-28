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
	public sealed class ArrayItemReference<TItem> : CodeActivity<Location<TItem>>
	{
		[RequiredArgumentAttribute]
		public InArgument<TItem[]> Array { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<int> Index { get; set; }

		protected override Location<TItem> Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
