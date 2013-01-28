using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Threading;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.Tracking;

namespace System.Activities.Hosting
{
	public class WorkflowInstanceExtensionManager
	{
		public virtual void Add<T> (Func<T> extensionCreationFunction) where T : class
		{
			throw new NotImplementedException ();
		}
		public virtual void Add (object singletonExtension)
		{
			throw new NotImplementedException ();
		}
		public void MakeReadOnly ()
		{
			throw new NotImplementedException ();
		}
	}
}
