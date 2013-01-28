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
	public sealed class ArgumentValue<T> : CodeActivity<T>
	{
		public ArgumentValue ()
		{
			throw new NotImplementedException ();
		}
		public ArgumentValue (string argumentName)
		{
			throw new NotImplementedException ();
		}

		public string ArgumentName { get; set; }
		
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		protected override T Execute (CodeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
