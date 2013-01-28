using System;
using System.Runtime;

namespace System.Management
{
	public abstract class ManagementEventArgs : EventArgs
	{
		private object context;

		public object Context
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.context;
			}
		}

		internal ManagementEventArgs(object context)
		{
			this.context = context;
		}
	}
}