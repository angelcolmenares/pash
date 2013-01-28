using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Threading;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.Tracking;

namespace System.Activities.Hosting
{
	public sealed class SymbolResolver : IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		public int Count { get { throw new NotImplementedException (); } }
		public bool IsReadOnly { get { throw new NotImplementedException (); } }
		public object this [string key] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public ICollection<string> Keys { get { throw new NotImplementedException (); } }
		public ICollection<object> Values { get { throw new NotImplementedException (); } }

		public void Add (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException ();
		}
		public void Add (string key, object value)
		{
			throw new NotImplementedException ();
		}
		public void Add (string key,Type type)
		{
			throw new NotImplementedException ();
		}
		public void Add (string key, object value, Type type)
		{
			throw new NotImplementedException ();
		}
		
		public LocationReferenceEnvironment AsLocationReferenceEnvironment ()
		{
			throw new NotImplementedException ();
		}
		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		public bool Contains (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException ();
		}
		public bool ContainsKey (string key)
		{
			throw new NotImplementedException ();
		}
		public void CopyTo (KeyValuePair<string, object>[] array,int arrayIndex)
		{
			throw new NotImplementedException ();
		}
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		public bool Remove (KeyValuePair<string, object> item)
		{
			throw new NotImplementedException ();
		}
		public bool Remove (string key)
		{
			throw new NotImplementedException ();
		}
		public bool TryGetValue (string key,out object value)
		{
			throw new NotImplementedException ();
		}
	}
}
