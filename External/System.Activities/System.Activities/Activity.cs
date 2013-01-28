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
using System.Activities.XamlIntegration;

namespace System.Activities
{
	[ContentProperty ("Implementation")]
	public abstract class Activity
	{
		protected Activity ()
		{
			throw new NotImplementedException ();
			// set: CacheId, Constraints, Id
		}

		protected internal int CacheId { get; private set; }

		protected Collection<Constraint> Constraints { get; private set; }

		public string DisplayName { get; set; }

		public string Id { get; private set; }

		[XamlDeferLoad (typeof (FuncDeferringLoader), typeof (Activity))]
		[Browsable (false)]
		[Ambient]
		protected virtual Func<Activity> Implementation { get; set; }

		protected virtual void CacheMetadata (ActivityMetadata metadata)
		{
			throw new NotImplementedException ();
		}

		public bool ShouldSerializeDisplayName ()
		{
			throw new NotImplementedException ();
		}

		// not verified.
		public override string ToString ()
		{
			return String.Concat ("{0} {1}", Id, DisplayName);
		}
	}

	[TypeConverter (typeof (ActivityWithResultConverter))]
	public abstract class Activity<TResult> : ActivityWithResult
	{
		public static Activity<TResult> FromValue (TResult constValue)
		{
			throw new NotImplementedException ();
		}
		
		public static Activity<TResult> FromVariable (Variable variable)
		{
			throw new NotImplementedException ();
		}

		public static Activity<TResult> FromVariable (Variable<TResult> variable)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator Activity<TResult> (TResult constValue)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator Activity<TResult> (Variable variable)
		{
			throw new NotImplementedException ();
		}

		public static implicit operator Activity<TResult> (Variable<TResult> variable)
		{
			throw new NotImplementedException ();
		}

		// instance members

		protected Activity ()
			: base (typeof (TResult))
		{
			throw new NotImplementedException ();
		}
		
		public new OutArgument<TResult> Result { get; set; }
	}
}
