using System;
using System.Diagnostics;
using System.Diagnostics.Eventing;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class EtwEventCorrelator : IEtwEventCorrelator
	{
		private readonly EventProvider _transferProvider;

		private readonly EventDescriptor _transferEvent;

		public Guid CurrentActivityId
		{
			get
			{
				return Trace.CorrelationManager.ActivityId;
			}
			set
			{
				EventProvider.SetActivityId(ref value);
			}
		}

		internal object TestOnlyTransferEvent
		{
			get
			{
				return this._transferEvent;
			}
		}

		internal object TestOnlyTransferProvider
		{
			get
			{
				return this._transferProvider;
			}
		}

		public EtwEventCorrelator(EventProvider transferProvider, EventDescriptor transferEvent)
		{
			if (transferProvider != null)
			{
				this._transferProvider = transferProvider;
				this._transferEvent = transferEvent;
				return;
			}
			else
			{
				throw new ArgumentNullException("transferProvider");
			}
		}

		public IEtwActivity StartActivity(Guid relatedActivityId)
		{
			EtwActivity etwActivity = new EtwActivity(this);
			this.CurrentActivityId = EventProvider.CreateActivityId();
			if (relatedActivityId != Guid.Empty)
			{
				EventDescriptor eventDescriptor = this._transferEvent;
				this._transferProvider.WriteTransferEvent(ref eventDescriptor, relatedActivityId, new object[0]);
			}
			return etwActivity;
		}

		public IEtwActivity StartActivity()
		{
			return this.StartActivity(this.CurrentActivityId);
		}
	}
}