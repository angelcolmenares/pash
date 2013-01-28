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
	[ContentProperty ("Exception")]
	public sealed class Throw : CodeActivity
	{
		[RequiredArgumentAttribute]
		public InArgument<Exception> Exception { get; set; }

		protected override void Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
