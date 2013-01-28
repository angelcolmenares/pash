using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	public sealed class Assign : CodeActivity
	{
		[RequiredArgumentAttribute]
		public OutArgument To { get; set; }
		[RequiredArgumentAttribute]
		public InArgument Value { get; set; }

		protected override void Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	public sealed class Assign<T> : CodeActivity
	{
		[RequiredArgumentAttribute]
		public OutArgument<T> To { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T> Value { get; set; }

		protected override void Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
