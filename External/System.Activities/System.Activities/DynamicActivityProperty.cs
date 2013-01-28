using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Transactions;
using System.Windows.Markup;
using System.Xaml;
using System.Xml.Linq;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Activities.Validation;

namespace System.Activities
{
	public class DynamicActivityProperty
	{
		public Collection<Attribute> Attributes { get { throw new NotImplementedException (); } }
		public string Name { get; set; }
		public Type Type { get; set; }
		public object Value { get; set; }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
