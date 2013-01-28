using System;
using System.Runtime;

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public class IgnoreMemberAttribute : Attribute
	{
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public IgnoreMemberAttribute()
		{
		}
	}
}