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
	public abstract class TrackingParticipant
	{
		public virtual TrackingProfile TrackingProfile { get; set; }

		protected internal virtual IAsyncResult BeginTrack (TrackingRecord record, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual void EndTrack (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected internal abstract void Track (TrackingRecord record, TimeSpan timeout);
	}
}
