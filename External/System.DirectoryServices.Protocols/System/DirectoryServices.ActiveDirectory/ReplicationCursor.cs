using System;
using System.DirectoryServices;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationCursor
	{
		private string partition;

		private Guid invocationID;

		private long USN;

		private string serverDN;

		private DateTime syncTime;

		private bool advanced;

		private string sourceServer;

		private DirectoryServer server;

		public DateTime LastSuccessfulSyncTime
		{
			get
			{
				if (!this.advanced)
				{
					if (Environment.OSVersion.Version.Major != 5 || Environment.OSVersion.Version.Minor != 0)
					{
						throw new PlatformNotSupportedException(Res.GetString("DSNotSupportOnDC"));
					}
					else
					{
						throw new PlatformNotSupportedException(Res.GetString("DSNotSupportOnClient"));
					}
				}
				else
				{
					return this.syncTime;
				}
			}
		}

		public string PartitionName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.partition;
			}
		}

		public Guid SourceInvocationId
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.invocationID;
			}
		}

		public string SourceServer
		{
			get
			{
				if (!this.advanced || this.advanced && this.serverDN != null)
				{
					this.sourceServer = Utils.GetServerNameFromInvocationID(this.serverDN, this.SourceInvocationId, this.server);
				}
				return this.sourceServer;
			}
		}

		public long UpToDatenessUsn
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.USN;
			}
		}

		private ReplicationCursor()
		{
		}

		internal ReplicationCursor(DirectoryServer server, string partition, Guid guid, long filter, long time, IntPtr dn)
		{
			this.partition = partition;
			this.invocationID = guid;
			this.USN = filter;
			this.syncTime = DateTime.FromFileTime(time);
			this.serverDN = Marshal.PtrToStringUni(dn);
			this.advanced = true;
			this.server = server;
		}

		internal ReplicationCursor(DirectoryServer server, string partition, Guid guid, long filter)
		{
			this.partition = partition;
			this.invocationID = guid;
			this.USN = filter;
			this.server = server;
		}
	}
}