using System;
using System.Collections;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationFailure
	{
		private string sourceDsaDN;

		private Guid uuidDsaObjGuid;

		private DateTime timeFirstFailure;

		private int numFailures;

		internal int lastResult;

		private DirectoryServer server;

		private string sourceServer;

		private Hashtable nameTable;

		public int ConsecutiveFailureCount
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.numFailures;
			}
		}

		public DateTime FirstFailureTime
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.timeFirstFailure;
			}
		}

		public int LastErrorCode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.lastResult;
			}
		}

		public string LastErrorMessage
		{
			get
			{
				return ExceptionHelper.GetErrorMessage(this.lastResult, false);
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
						if (this.sourceDsaDN != null)
						{
							this.sourceServer = Utils.GetServerNameFromInvocationID(this.sourceDsaDN, this.SourceServerGuid, this.server);
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

		internal ReplicationFailure(IntPtr addr, DirectoryServer server, Hashtable table)
		{
			DS_REPL_KCC_DSA_FAILURE dSREPLKCCDSAFAILURE = new DS_REPL_KCC_DSA_FAILURE();
			Marshal.PtrToStructure(addr, dSREPLKCCDSAFAILURE);
			this.sourceDsaDN = Marshal.PtrToStringUni(dSREPLKCCDSAFAILURE.pszDsaDN);
			this.uuidDsaObjGuid = dSREPLKCCDSAFAILURE.uuidDsaObjGuid;
			this.timeFirstFailure = DateTime.FromFileTime(dSREPLKCCDSAFAILURE.ftimeFirstFailure);
			this.numFailures = dSREPLKCCDSAFAILURE.cNumFailures;
			this.lastResult = dSREPLKCCDSAFAILURE.dwLastResult;
			this.server = server;
			this.nameTable = table;
		}
	}
}