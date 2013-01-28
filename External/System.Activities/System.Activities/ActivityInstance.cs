using System;
using System.Runtime.Serialization;

namespace System.Activities
{
	[DataContract (Name = "ActivityInstance", Namespace = "http://schemas.datacontract.org/2010/02/System.Activities")]
	public sealed class ActivityInstance
	{
		internal ActivityInstance (string id, bool isCompleted, ActivityInstanceState state)
		{
			Id = id;
			IsCompleted = isCompleted;
			State = state;
		}

		public Activity Activity { get; internal set; }
		public string Id { get; private set; }
		public bool IsCompleted { get; private set; }
		public ActivityInstanceState State { get; private set; }
	}
}
