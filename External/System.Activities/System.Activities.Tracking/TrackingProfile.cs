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
	[ContentPropertyAttribute("Queries")]
	public class TrackingProfile
	{
		public TrackingProfile ()
		{
			Queries = new Collection<TrackingQuery> ();
		}

		public string ActivityDefinitionId { get; set; }

		public ImplementationVisibility ImplementationVisibility { get; set; }

		public string Name { get; set; }

		public Collection<TrackingQuery> Queries { get; private set; }
	}
}
