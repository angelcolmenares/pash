using System;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public static class RoleInstanceStatus
	{
		public const string Initializing = "Initializing";

		public const string Ready = "Ready";

		public const string Busy = "Busy";

		public const string Stopping = "Stopping";

		public const string Stopped = "Stopped";

		public const string Unresponsive = "Unresponsive";

		public const string RoleStateUnknown = "RoleStateUnknown";

		public const string CreatingVM = "CreatingVM";

		public const string StartingVM = "StartingVM";

		public const string CreatingRole = "CreatingRole";

		public const string StartingRole = "StartingRole";

		public const string ReadyRole = "ReadyRole";

		public const string BusyRole = "BusyRole";

		public const string StoppingRole = "StoppingRole";

		public const string StoppingVM = "StoppingVM";

		public const string DeletingVM = "DeletingVM";

		public const string StoppedVM = "StoppedVM";

		public const string RestartingRole = "RestartingRole";

		public const string CyclingRole = "CyclingRole";

		public const string FailedStartingRole = "FailedStartingRole";

		public const string FailedStartingVM = "FailedStartingVM";

		public const string UnresponsiveRole = "UnresponsiveRole";

		public const string Provisioning = "Provisioning";

		public const string ProvisioningFailed = "ProvisioningFailed";

	}
}