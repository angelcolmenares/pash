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
	[ContentProperty ("Implementation")]
	public sealed class ActivityBuilder : IDebuggableWorkflowTree
	{
		public static ActivityPropertyReference GetPropertyReference (object target)
		{
			throw new NotImplementedException ();
		}

		public static void SetPropertyReference (object target, ActivityPropertyReference value)
		{
			throw new NotImplementedException ();
		}

		public ActivityBuilder ()
		{
			Attributes = new Collection<Attribute> ();
			Constraints = new Collection<Constraint> ();
		}

		public Collection<Attribute> Attributes { get; private set; }
		[Browsable (false)]
		public Collection<Constraint> Constraints { get; private set; }
		[Browsable (false)]
		public Activity Implementation { get; set; }
		public string Name { get; set; }
		[Browsable (false)]
		public KeyedCollection<string, DynamicActivityProperty> Properties {
			get { throw new NotImplementedException (); }
		}

		Activity IDebuggableWorkflowTree.GetWorkflowRoot ()
		{
			throw new NotImplementedException ();
		}
	}

	[ContentProperty ("Implementation")]
	public sealed class ActivityBuilder<TResult> : IDebuggableWorkflowTree
	{
		public ActivityBuilder ()
		{
			Attributes = new Collection<Attribute> ();
			Constraints = new Collection<Constraint> ();
		}

		public Collection<Attribute> Attributes { get; private set; }
		[Browsable (false)]
		public Collection<Constraint> Constraints { get; private set; }
		[Browsable (false)]
		public Activity Implementation { get; set; }
		public string Name { get; set; }
		[Browsable (false)]
		public KeyedCollection<string, DynamicActivityProperty> Properties {
			get { throw new NotImplementedException (); }
		}

		Activity IDebuggableWorkflowTree.GetWorkflowRoot ()
		{
			throw new NotImplementedException ();
		}
	}

}
