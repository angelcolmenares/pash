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
	[DataContract]
	public sealed class BookmarkInfo
	{
		internal BookmarkInfo ()
		{
			throw new NotImplementedException ();
		}

		[DataMemberAttribute]
		public string BookmarkName { get; private set; }

		[DataMember (EmitDefaultValue = false)]
		public string OwnerDisplayName { get; private set; }

		[DataMember (EmitDefaultValue = false)]
		public BookmarkScopeInfo ScopeInfo { get; private set; }
	}
}
