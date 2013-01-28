using System;
using System.Runtime;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ReplicationOperationInformation
	{
		internal DateTime startTime;

		internal ReplicationOperation currentOp;

		internal ReplicationOperationCollection collection;

		public ReplicationOperation CurrentOperation
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.currentOp;
			}
		}

		public DateTime OperationStartTime
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.startTime;
			}
		}

		public ReplicationOperationCollection PendingOperations
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.collection;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ReplicationOperationInformation()
		{
		}
	}
}