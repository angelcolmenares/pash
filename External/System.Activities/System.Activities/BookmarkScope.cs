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
	[DataContract]
	public sealed class BookmarkScope : IEquatable<BookmarkScope>
	{
		static BookmarkScope ()
		{
			// FIXME: verify
			Default = new BookmarkScope (Guid.Empty);
		}

		public static BookmarkScope Default { get; private set; }

		public BookmarkScope (Guid id)
		{
			Id = id;
		}

		public Guid Id { get; internal set; }
		public bool IsInitialized { get; private set; }
		
		public bool Equals (BookmarkScope other)
		{
			throw new NotImplementedException ();
		}
		
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
		
		public void Initialize (NativeActivityContext context, Guid id)
		{
			throw new NotImplementedException ();
		}
	}
}
