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
	public sealed class CancelRequestedRecord : TrackingRecord
	{
		public CancelRequestedRecord (Guid instanceId, long recordNumber, ActivityInfo activity, ActivityInfo child)
			: base (instanceId, recordNumber)
		{
			Activity = activity;
			Child = child;
		}

		[DataMember]
		public ActivityInfo Activity { get; private set; }

		[DataMember]
		public ActivityInfo Child { get; private set; }

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
