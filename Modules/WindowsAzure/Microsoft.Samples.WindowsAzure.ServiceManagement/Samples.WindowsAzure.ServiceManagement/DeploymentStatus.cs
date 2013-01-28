using System;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public static class DeploymentStatus
	{
		public const string Running = "Running";

		public const string Suspended = "Suspended";

		public const string RunningTransitioning = "RunningTransitioning";

		public const string SuspendedTransitioning = "SuspendedTransitioning";

		public const string Starting = "Starting";

		public const string Suspending = "Suspending";

		public const string Deploying = "Deploying";

		public const string Deleting = "Deleting";

		public const string Unavailable = "Unavailable";

	}
}