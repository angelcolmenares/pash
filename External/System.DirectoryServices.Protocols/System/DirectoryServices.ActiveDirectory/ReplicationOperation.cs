using System;
using System.Collections;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationOperation
	{
		private DateTime timeEnqueued;

		private int serialNumber;

		private int priority;

		private ReplicationOperationType operationType;

		private string namingContext;

		private string dsaDN;

		private Guid uuidDsaObjGuid;

		private DirectoryServer server;

		private string sourceServer;

		private Hashtable nameTable;

		public int OperationNumber
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.serialNumber;
			}
		}

		public ReplicationOperationType OperationType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.operationType;
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

		public int Priority
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.priority;
			}
		}

		public string SourceServer
		{
			get
			{
				if (this.sourceServer == null)
				{
					if (!this.nameTable.Contains(this.SourceServerGuid))
					{
						if (this.dsaDN != null)
						{
							this.sourceServer = Utils.GetServerNameFromInvocationID(this.dsaDN, this.SourceServerGuid, this.server);
							this.nameTable.Add(this.SourceServerGuid, this.sourceServer);
						}
					}
					else
					{
						this.sourceServer = (string)this.nameTable[(object)this.SourceServerGuid];
					}
				}
				return this.sourceServer;
			}
		}

		private Guid SourceServerGuid
		{
			get
			{
				return this.uuidDsaObjGuid;
			}
		}

		public DateTime TimeEnqueued
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.timeEnqueued;
			}
		}

		internal ReplicationOperation(IntPtr addr, DirectoryServer server, Hashtable table)
		{
			DS_REPL_OP dSREPLOP = new DS_REPL_OP();
			Marshal.PtrToStructure(addr, dSREPLOP);
			this.timeEnqueued = DateTime.FromFileTime(dSREPLOP.ftimeEnqueued);
			this.serialNumber = dSREPLOP.ulSerialNumber;
			this.priority = dSREPLOP.ulPriority;
			this.operationType = dSREPLOP.OpType;
			this.namingContext = Marshal.PtrToStringUni(dSREPLOP.pszNamingContext);
			this.dsaDN = Marshal.PtrToStringUni(dSREPLOP.pszDsaDN);
			this.uuidDsaObjGuid = dSREPLOP.uuidDsaObjGuid;
			this.server = server;
			this.nameTable = table;
		}
	}
}