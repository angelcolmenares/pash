using System.Runtime.DurableInstancing;

namespace System.Activities.DurableInstancing
{
	public sealed class HasActivatableWorkflowEvent : InstancePersistenceEvent<HasActivatableWorkflowEvent>
	{
		public HasActivatableWorkflowEvent() : base(InstancePersistence.ActivitiesEventNamespace.GetName("HasActivatableWorkflow"))
		{
		}
	}
}