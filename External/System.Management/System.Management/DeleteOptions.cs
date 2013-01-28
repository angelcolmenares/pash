using System;
using System.Runtime;

namespace System.Management
{
	public class DeleteOptions : ManagementOptions
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DeleteOptions()
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public DeleteOptions(ManagementNamedValueCollection context, TimeSpan timeout) : base(context, timeout)
		{
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new DeleteOptions(managementNamedValueCollection, base.Timeout);
		}
	}
}