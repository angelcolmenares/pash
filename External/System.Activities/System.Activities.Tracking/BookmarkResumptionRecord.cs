using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Activities;
using System.Windows.Markup;

namespace System.Activities.Tracking
{
	[DataContract]
	public sealed class BookmarkResumptionRecord : TrackingRecord
	{
		public BookmarkResumptionRecord (Guid instanceId, long recordNumber, Guid bookmarkScope, string bookmarkName, ActivityInfo owner)
			: base (instanceId, recordNumber)
		{
			BookmarkName = bookmarkName;
			BookmarkScope = bookmarkScope;
			Owner = owner;
		}

		[DataMemberAttribute(EmitDefaultValue = false)]
		public string BookmarkName { get; private set; }

		[DataMemberAttribute(EmitDefaultValue = false)]
		public Guid BookmarkScope { get; private set; }

		[DataMember]
		public ActivityInfo Owner { get; private set; }

		[DataMember]
		public object Payload { get; internal set; }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		protected internal override TrackingRecord Clone ()
		{
			throw new NotImplementedException ();
		}
	}
}
