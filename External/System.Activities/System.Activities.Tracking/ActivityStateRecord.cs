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
	public sealed class ActivityStateRecord : TrackingRecord
	{
		public ActivityStateRecord (Guid instanceId, long recordNumber, ActivityInfo activity, string state)
			: base (instanceId, recordNumber)
		{
			Activity = activity;
			State = state;
		}
		
		[DataMember]
		public ActivityInfo Activity { get; private set; }

		public IDictionary<string, Object> Arguments { get; internal set; }

		[DataMember]
		public string State { get; private set; }

		public IDictionary<string, Object> Variables { get; internal set; }

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
