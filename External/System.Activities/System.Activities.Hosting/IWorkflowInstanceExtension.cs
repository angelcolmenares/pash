using System;
using System.Collections.Generic;

namespace System.Activities.Hosting
{
	public interface IWorkflowInstanceExtension
	{
		IEnumerable<object> GetAdditionalExtensions ();
		void SetInstance (WorkflowInstanceProxy instance);
	}
}
