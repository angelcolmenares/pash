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
	public sealed class BookmarkScopeHandle : Handle
	{
		public static BookmarkScopeHandle Default { get; private set; }
		
		static BookmarkScopeHandle ()
		{
			Default = new BookmarkScopeHandle (BookmarkScope.Default);
		}

		internal BookmarkScopeHandle (BookmarkScope scope)
		{
			BookmarkScope = scope;
		}

		public BookmarkScope BookmarkScope { get; private set; }

		public void CreateBookmarkScope (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
		public void CreateBookmarkScope (NativeActivityContext context, Guid scopeId)
		{
			throw new NotImplementedException ();
		}
		public void Initialize (NativeActivityContext context, Guid scope)
		{
			throw new NotImplementedException ();
		}
	}
}
