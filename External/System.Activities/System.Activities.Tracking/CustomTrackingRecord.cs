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
	public class CustomTrackingRecord : TrackingRecord
	{
		protected CustomTrackingRecord (CustomTrackingRecord record)
			: base (record)
		{
			Data = new Dictionary<string, Object> ();
		}

		[MonoTODO]
		public CustomTrackingRecord (string name)
			: base (null)
		{
			Data = new Dictionary<string, Object> ();
			Name = name;
		}

		[MonoTODO]
		public CustomTrackingRecord (string name, TraceLevel level)
			: base (null)
		{
			Data = new Dictionary<string, Object> ();
			Level = level;
		}

		public CustomTrackingRecord (Guid instanceId, string name, TraceLevel level)
			: base (instanceId)
		{
			Data = new Dictionary<string, Object> ();
			Level = level;
			Name = name;
		}

		[DataMember]
		public ActivityInfo Activity { get; internal set; }

		public IDictionary<string, Object> Data { get; private set; }

		[DataMember]
		public string Name { get; private set; }

		protected internal override TrackingRecord Clone ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
