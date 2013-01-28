using System;
using System.Runtime;

namespace System.Management
{
	public class StoppedEventArgs : ManagementEventArgs
	{
		private int status;

		public ManagementStatus Status
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return (ManagementStatus)this.status;
			}
		}

		internal StoppedEventArgs(object context, int status) : base(context)
		{
			this.status = status;
		}
	}
}