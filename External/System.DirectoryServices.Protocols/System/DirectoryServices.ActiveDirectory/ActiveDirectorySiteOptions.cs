using System;

namespace System.DirectoryServices.ActiveDirectory
{
	[Flags]
	public enum ActiveDirectorySiteOptions
	{
		None = 0,
		AutoTopologyDisabled = 1,
		TopologyCleanupDisabled = 2,
		AutoMinimumHopDisabled = 4,
		StaleServerDetectDisabled = 8,
		AutoInterSiteTopologyDisabled = 16,
		GroupMembershipCachingEnabled = 32,
		ForceKccWindows2003Behavior = 64,
		UseWindows2000IstgElection = 128,
		RandomBridgeHeaderServerSelectionDisabled = 256,
		UseHashingForReplicationSchedule = 512,
		RedundantServerTopologyEnabled = 1024
	}
}