using System;

namespace Microsoft.PowerShell.Workflow
{
	internal static class PSWorkflowPerformanceCounterIds
	{
		internal const int FailedWorkflowJobsCount = 1;

		internal const int FailedWorkflowJobsPerSec = 2;

		internal const int ResumedWorkflowJobsCount = 3;

		internal const int ResumedWorkflowJobsPerSec = 4;

		internal const int RunningWorkflowJobsCount = 5;

		internal const int RunningWorkflowJobsPerSec = 6;

		internal const int StoppedWorkflowJobsCount = 7;

		internal const int StoppedWorkflowJobsPerSec = 8;

		internal const int SucceededWorkflowJobsCount = 9;

		internal const int SucceededWorkflowJobsPerSec = 10;

		internal const int SuspendedWorkflowJobsCount = 11;

		internal const int SuspendedWorkflowJobsPerSec = 12;

		internal const int TerminatedWorkflowJobsCount = 13;

		internal const int TerminatedWorkflowJobsPerSec = 14;

		internal const int WaitingWorkflowJobsCount = 15;

		internal const int ActivityHostMgrBusyProcessesCount = 16;

		internal const int ActivityHostMgrFailedRequestsPerSec = 17;

		internal const int ActivityHostMgrFailedRequestsQueueLength = 18;

		internal const int ActivityHostMgrIncomingRequestsPerSec = 19;

		internal const int ActivityHostMgrPendingRequestsQueueLength = 20;

		internal const int ActivityHostMgrCreatedProcessesCount = 21;

		internal const int ActivityHostMgrDisposedProcessesCount = 22;

		internal const int ActivityHostMgrProcessesPoolSize = 23;

		internal const int PSRemotingPendingRequestsQueueLength = 24;

		internal const int PSRemotingRequestsBeingServicedCount = 25;

		internal const int PSRemotingForcedToWaitRequestsQueueLength = 26;

		internal const int PSRemotingConnectionsCreatedCount = 27;

		internal const int PSRemotingConnectionsDisposedCount = 28;

		internal const int PSRemotingConnectionsClosedReopendCount = 29;

	}
}