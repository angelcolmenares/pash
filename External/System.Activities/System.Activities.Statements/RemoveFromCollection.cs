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
	[ContentProperty ("Collection")]
	public sealed class RemoveFromCollection<T> : CodeActivity<bool>
	{
		[RequiredArgumentAttribute]
		public InArgument<ICollection<T>> Collection { get; set; }
		[RequiredArgumentAttribute]
		public InArgument<T> Item { get; set; }

		protected override bool Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
