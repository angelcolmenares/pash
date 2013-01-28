using System.Runtime.DurableInstancing;

namespace System.Activities.DurableInstancing
{
	public sealed class HasRunnableWorkflowEvent : InstancePersistenceEvent<HasRunnableWorkflowEvent>
	{
		public HasRunnableWorkflowEvent() : base(InstancePersistence.ActivitiesEventNamespace.GetName("HasRunnableWorkflow"))
		{
		}
	}
}