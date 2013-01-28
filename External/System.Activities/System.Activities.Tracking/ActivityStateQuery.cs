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
	public class ActivityStateQuery : TrackingQuery
	{
		public ActivityStateQuery ()
		{
			Arguments = new Collection<string> ();
			States = new Collection<string> ();
			Variables = new Collection<string> ();
		}

		public string ActivityName { get; set; }

		public Collection<string> Arguments { get; private set; }

		public Collection<string> States { get; private set; }

		public Collection<string> Variables { get; private set; }
	}
}
