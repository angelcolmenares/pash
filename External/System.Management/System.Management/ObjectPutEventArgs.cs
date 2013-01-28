using System;
using System.Runtime;

namespace System.Management
{
	public class ObjectPutEventArgs : ManagementEventArgs
	{
		private ManagementPath wmiPath;

		public ManagementPath Path
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.wmiPath;
			}
		}

		internal ObjectPutEventArgs(object context, ManagementPath path) : base(context)
		{
			this.wmiPath = path;
		}
	}
}