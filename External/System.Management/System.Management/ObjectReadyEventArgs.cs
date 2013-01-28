using System;
using System.Runtime;

namespace System.Management
{
	public class ObjectReadyEventArgs : ManagementEventArgs
	{
		private ManagementBaseObject wmiObject;

		public ManagementBaseObject NewObject
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.wmiObject;
			}
		}

		internal ObjectReadyEventArgs(object context, ManagementBaseObject wmiObject) : base(context)
		{
			this.wmiObject = wmiObject;
		}
	}
}