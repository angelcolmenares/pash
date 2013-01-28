using System;
using System.Runtime;

namespace System.DirectoryServices.ActiveDirectory
{
	public class SyncFromAllServersErrorInformation
	{
		private SyncFromAllServersErrorCategory category;

		private int errorCode;

		private string errorMessage;

		private string sourceServer;

		private string targetServer;

		public SyncFromAllServersErrorCategory ErrorCategory
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.category;
			}
		}

		public int ErrorCode
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorCode;
			}
		}

		public string ErrorMessage
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.errorMessage;
			}
		}

		public string SourceServer
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.sourceServer;
			}
		}

		public string TargetServer
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.targetServer;
			}
		}

		internal SyncFromAllServersErrorInformation(SyncFromAllServersErrorCategory category, int errorCode, string errorMessage, string sourceServer, string targetServer)
		{
			this.category = category;
			this.errorCode = errorCode;
			this.errorMessage = errorMessage;
			this.sourceServer = sourceServer;
			this.targetServer = targetServer;
		}
	}
}