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
	public sealed class WorkflowInstanceUnhandledExceptionRecord : WorkflowInstanceRecord
	{
		[MonoTODO ("reason argument")]
		public WorkflowInstanceUnhandledExceptionRecord (Guid instanceId, string activityDefinitionId, ActivityInfo faultSource, Exception exception)
			: base (instanceId, activityDefinitionId, null)
		{
			FaultSource = faultSource;
			UnhandledException = exception;
		}

		[MonoTODO ("reason argument")]
		public WorkflowInstanceUnhandledExceptionRecord (Guid instanceId, long recordNumber, string activityDefinitionId, ActivityInfo faultSource, Exception exception)
			: base (instanceId, recordNumber, activityDefinitionId, null)
		{
			FaultSource = faultSource;
			UnhandledException = exception;
		}

		[DataMember]
		public ActivityInfo FaultSource { get; private set; }

		[DataMember]
		public Exception UnhandledException { get; private set; }

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
