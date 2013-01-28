using System;

namespace System.Activities.Tracking
{
	public sealed class EtwTrackingParticipant : TrackingParticipant
	{
		public string ApplicationReference { get; set; }

		public Guid EtwProviderId { get; set; }


		protected internal override void Track (TrackingRecord record, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}
