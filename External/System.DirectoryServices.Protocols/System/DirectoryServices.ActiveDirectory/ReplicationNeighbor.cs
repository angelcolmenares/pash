using System;
using System.Collections;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationNeighbor
	{
		private string namingContext;

		private string sourceServerDN;

		private ActiveDirectoryTransportType transportType;

		private ReplicationNeighbor.ReplicationNeighborOptions replicaFlags;

		private Guid uuidSourceDsaInvocationID;

		private long usnLastObjChangeSynced;

		private long usnAttributeFilter;

		private DateTime timeLastSyncSuccess;

		private DateTime timeLastSyncAttempt;

		private int lastSyncResult;

		private int consecutiveSyncFailures;

		private DirectoryServer server;

		private string sourceServer;

		private Hashtable nameTable;

		public int ConsecutiveFailureCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.consecutiveSyncFailures;
			}
		}

		public DateTime LastAttemptedSync
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.timeLastSyncAttempt;
			}
		}

		public DateTime LastSuccessfulSync
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.timeLastSyncSuccess;
			}
		}

		public string LastSyncMessage
		{
			get
			{
				return ExceptionHelper.GetErrorMessage(this.lastSyncResult, false);
			}
		}

		public int LastSyncResult
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.lastSyncResult;
			}
		}

		public string PartitionName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.namingContext;
			}
		}

		public ReplicationNeighbor.ReplicationNeighborOptions ReplicationNeighborOption
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.replicaFlags;
			}
		}

		public Guid SourceInvocationId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.uuidSourceDsaInvocationID;
			}
		}

		public string SourceServer
		{
			get
			{
				if (this.sourceServer == null)
				{
					if (!this.nameTable.Contains(this.SourceInvocationId))
					{
						if (this.sourceServerDN != null)
						{
							this.sourceServer = Utils.GetServerNameFromInvocationID(this.sourceServerDN, this.SourceInvocationId, this.server);
							this.nameTable.Add(this.SourceInvocationId, this.sourceServer);
						}
					}
					else
					{
						this.sourceServer = (string)this.nameTable[(object)this.SourceInvocationId];
					}
				}
				return this.sourceServer;
			}
		}

		public ActiveDirectoryTransportType TransportType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.transportType;
			}
		}

		public long UsnAttributeFilter
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.usnAttributeFilter;
			}
		}

		public long UsnLastObjectChangeSynced
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.usnLastObjChangeSynced;
			}
		}

		internal ReplicationNeighbor(IntPtr addr, DirectoryServer server, Hashtable table)
		{
			DS_REPL_NEIGHBOR dSREPLNEIGHBOR = new DS_REPL_NEIGHBOR();
			Marshal.PtrToStructure(addr, dSREPLNEIGHBOR);
			this.namingContext = Marshal.PtrToStringUni(dSREPLNEIGHBOR.pszNamingContext);
			this.sourceServerDN = Marshal.PtrToStringUni(dSREPLNEIGHBOR.pszSourceDsaDN);
			string stringUni = Marshal.PtrToStringUni(dSREPLNEIGHBOR.pszAsyncIntersiteTransportDN);
			if (stringUni != null)
			{
				string rdnFromDN = Utils.GetRdnFromDN(stringUni);
				string value = Utils.GetDNComponents(rdnFromDN)[0].Value;
				if (string.Compare(value, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
				{
					this.transportType = ActiveDirectoryTransportType.Rpc;
				}
				else
				{
					this.transportType = ActiveDirectoryTransportType.Smtp;
				}
			}
			this.replicaFlags = (ReplicationNeighbor.ReplicationNeighborOptions)((long)dSREPLNEIGHBOR.dwReplicaFlags);
			this.uuidSourceDsaInvocationID = dSREPLNEIGHBOR.uuidSourceDsaInvocationID;
			this.usnLastObjChangeSynced = dSREPLNEIGHBOR.usnLastObjChangeSynced;
			this.usnAttributeFilter = dSREPLNEIGHBOR.usnAttributeFilter;
			this.timeLastSyncSuccess = DateTime.FromFileTime(dSREPLNEIGHBOR.ftimeLastSyncSuccess);
			this.timeLastSyncAttempt = DateTime.FromFileTime(dSREPLNEIGHBOR.ftimeLastSyncAttempt);
			this.lastSyncResult = dSREPLNEIGHBOR.dwLastSyncResult;
			this.consecutiveSyncFailures = dSREPLNEIGHBOR.cNumConsecutiveSyncFailures;
			this.server = server;
			this.nameTable = table;
		}

		[Flags]
		public enum ReplicationNeighborOptions : long
		{
			Writeable = 16,
			SyncOnStartup = 32,
			ScheduledSync = 64,
			UseInterSiteTransport = 128,
			TwoWaySync = 512,
			ReturnObjectParent = 2048,
			FullSyncInProgress = 65536,
			FullSyncNextPacket = 131072,
			NeverSynced = 2097152,
			Preempted = 16777216,
			IgnoreChangeNotifications = 67108864,
			DisableScheduledSync = 134217728,
			CompressChanges = 268435456,
			NoChangeNotifications = 536870912,
			PartialAttributeSet = 1073741824
		}
	}
}