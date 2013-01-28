using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Transactions;
using System.Windows.Markup;
using System.Xaml;
using System.Xml.Linq;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Activities.Statements;
using System.Activities.Tracking;
using System.Activities.Validation;

namespace System.Activities
{
	public static class WorkflowInspectionServices
	{
		public static void CacheMetadata (Activity rootActivity)
		{
			throw new NotImplementedException ();
		}
		public static void CacheMetadata (Activity rootActivity, LocationReferenceEnvironment hostEnvironment)
		{
			throw new NotImplementedException ();
		}
		public static IEnumerable<Activity> GetActivities (Activity activity)
		{
			throw new NotImplementedException ();
		}
		public static Activity Resolve (Activity root, string id)
		{
			throw new NotImplementedException ();
		}
	}
}
