using System;

namespace System.Activities.Debugger
{
	public interface IDebuggableWorkflowTree
	{
		Activity GetWorkflowRoot ();
	}
}
