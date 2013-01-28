using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Transactions;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Hosting;
using System.Windows.Markup;

namespace System.Activities.Statements
{
	public sealed class FlowDecision : FlowNode
	{
		public FlowDecision ()
		{
			throw new NotImplementedException ();
		}
		public FlowDecision (Activity<bool> condition)
		{
			throw new NotImplementedException ();
		}
		public FlowDecision (Expression<Func<ActivityContext, bool>> condition)
		{
			throw new NotImplementedException ();
		}
	}
}
