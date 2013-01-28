using System;
using System.Runtime;

namespace System.Management
{
	public class CompletedEventArgs : ManagementEventArgs
	{
		private readonly int status;

		private readonly ManagementBaseObject wmiObject;

		public ManagementStatus Status
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return (ManagementStatus)this.status;
			}
		}

		public ManagementBaseObject StatusObject
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.wmiObject;
			}
		}

		internal CompletedEventArgs(object context, int status, ManagementBaseObject wmiStatusObject) : base(context)
		{
			this.wmiObject = wmiStatusObject;
			this.status = status;
		}
	}
}