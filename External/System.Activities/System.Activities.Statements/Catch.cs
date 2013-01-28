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
	public abstract class Catch
	{
		public abstract Type ExceptionType { get; }
	}
	
	[ContentProperty ("Action")]
	public sealed class Catch<TException> : Catch
		where TException : Exception
	{
		public ActivityAction<TException> Action { get; set; }
		public override Type ExceptionType { get { throw new NotImplementedException (); } }
	}
}
