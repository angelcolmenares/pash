namespace System.Activities
{
	public interface IExecutionProperty
	{
		void CleanupWorkflowThread ();
		void SetupWorkflowThread ();
	}
}
