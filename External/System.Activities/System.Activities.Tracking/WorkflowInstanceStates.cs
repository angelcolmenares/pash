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
	[MonoTODO ("Not vetified")]
	public static class WorkflowInstanceStates
	{
		public const string Aborted = "Aborted";
		public const string Canceled = "Canceled";
		public const string Completed = "Completed";
		public const string Deleted = "Deleted";
		public const string Idle = "Idle";
		public const string Persisted = "Persisted";
		public const string Resumed = "Resumed";
		public const string Started = "Started";
		public const string Suspended = "Suspended";
		public const string Terminated = "Terminated";
		public const string UnhandledException = "UnhandledException";
		public const string Unloaded = "Unloaded";
		public const string Unsuspended = "Unsuspended";
	}
}
