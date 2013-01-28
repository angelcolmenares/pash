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
	public sealed class FaultPropagationRecord : TrackingRecord
	{
		public FaultPropagationRecord (Guid instanceId, long recordNumber, ActivityInfo faultSource, ActivityInfo faultHandler, bool isFaultSource, Exception fault)
			: base (instanceId, recordNumber)
		{
			FaultSource = faultSource;
			FaultHandler = faultHandler;
			IsFaultSource = isFaultSource;
			Fault = fault;
		}


		[DataMember]
		public Exception Fault { get; private set; }

		[DataMember]
		public ActivityInfo FaultHandler { get; private set; }

		[DataMember]
		public ActivityInfo FaultSource { get; private set; }

		[DataMember(EmitDefaultValue = false)]
		public bool IsFaultSource { get; private set; }

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