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
	public sealed class ExecutionProperties : IEnumerable<KeyValuePair<string, Object>>, IEnumerable
	{
		internal ExecutionProperties ()
		{
		}
		
		public bool IsEmpty { get { throw new NotImplementedException (); } }

		public void Add (string name, object property)
		{
			throw new NotImplementedException ();
		}
		public void Add (string name, object property, bool onlyVisibleToPublicChildren)
		{
			throw new NotImplementedException ();
		}
		public object Find (string name)
		{
			throw new NotImplementedException ();
		}
		public IEnumerator<KeyValuePair<string, Object>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		public bool Remove (string name)
		{
			throw new NotImplementedException ();
		}
	}
}
