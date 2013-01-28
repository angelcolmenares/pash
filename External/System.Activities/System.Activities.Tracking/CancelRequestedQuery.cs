namespace System.Activities.Tracking
{
	public sealed class CancelRequestedQuery : TrackingQuery
	{
		public string ActivityName { get; set; }

		public string ChildActivityName { get; set; }
	}
}
