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
	public abstract class TrackingRecord
	{
		protected TrackingRecord (Guid instanceId)
		{
			throw new NotImplementedException ();
		}

		protected TrackingRecord (TrackingRecord record)
		{
			throw new NotImplementedException ();
		}

		protected TrackingRecord (Guid instanceId, long recordNumber)
		{
			throw new NotImplementedException ();
		}

		public IDictionary<string, string> Annotations { get; internal set; }

		[DataMember]
		[MonoTODO]
		public DateTime EventTime { get; private set; }

		[DataMember]
		[MonoTODO]
		public Guid InstanceId { get; internal set; }

		[DataMember]
		[MonoTODO]
		public TraceLevel Level { get; protected set; }

		[DataMember]
		[MonoTODO]
		public long RecordNumber { get; internal set; }

		protected internal abstract TrackingRecord Clone ();

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
