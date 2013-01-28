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
	public class WorkflowInstanceRecord : TrackingRecord
	{
		protected WorkflowInstanceRecord (WorkflowInstanceRecord record)
			: base (record)
		{
		}

		public WorkflowInstanceRecord (Guid instanceId, string activityDefinitionId, string state)
			: base (instanceId)
		{
			ActivityDefinitionId = activityDefinitionId;
			State = state;
		}

		public WorkflowInstanceRecord (Guid instanceId, long recordNumber, string activityDefinitionId, string state)
			: base (instanceId, recordNumber)
		{
			ActivityDefinitionId = activityDefinitionId;
			State = state;
		}

		[DataMember]
		public string ActivityDefinitionId { get; private set; }

		[DataMember]
		public string State { get; private set; }

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
