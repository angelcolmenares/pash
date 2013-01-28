namespace System.Activities.Tracking
{
	public sealed class FaultPropagationQuery : TrackingQuery
	{
		public string FaultHandlerActivityName { get; set; }

		public string FaultSourceActivityName { get; set; }
	}
}
