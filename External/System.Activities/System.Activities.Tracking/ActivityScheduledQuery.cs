using System;

namespace System.Activities.Tracking
{
	public sealed class ActivityScheduledQuery : TrackingQuery
	{
		public string ActivityName { get; set; }

		public string ChildActivityName { get; set; }
	}
}
