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
	public sealed class BookmarkScopeInfo
	{
		internal BookmarkScopeInfo ()
		{
			throw new NotImplementedException ();
		}

		[DataMember (EmitDefaultValue = false)]
		public Guid Id { get; private set; }
		public bool IsInitialized { get { throw new NotImplementedException (); } }
		[DataMember (EmitDefaultValue = false)]
		public string TemporaryId { get; private set; }
	}
}
