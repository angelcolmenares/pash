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
	public class ExclusiveHandle : Handle
	{
		public ReadOnlyCollection<BookmarkScopeHandle> RegisteredBookmarkScopes { get { throw new NotImplementedException (); } }

		protected override void OnInitialize (HandleInitializationContext context)
		{
			throw new NotImplementedException ();
		}
		public void RegisterBookmarkScope (NativeActivityContext context, BookmarkScopeHandle bookmarkScopeHandle)
		{
			throw new NotImplementedException ();
		}
		public void Reinitialize (NativeActivityContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
