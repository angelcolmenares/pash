using System;
using System.Collections.Generic;

namespace System.Activities.Tracking
{
	public abstract class TrackingQuery
	{
		protected TrackingQuery ()
		{
			QueryAnnotations = new Dictionary<string, string> ();
		}

		public IDictionary<string, string> QueryAnnotations { get; private set; }
	}
}
