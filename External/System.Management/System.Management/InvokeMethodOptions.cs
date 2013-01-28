using System;
using System.Runtime;

namespace System.Management
{
	public class InvokeMethodOptions : ManagementOptions
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InvokeMethodOptions()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InvokeMethodOptions(ManagementNamedValueCollection context, TimeSpan timeout) : base(context, timeout)
		{
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new InvokeMethodOptions(managementNamedValueCollection, base.Timeout);
		}
	}
}