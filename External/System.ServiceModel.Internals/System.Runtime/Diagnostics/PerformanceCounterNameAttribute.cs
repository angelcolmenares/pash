using System;
using System.Runtime;

namespace System.Runtime.Diagnostics
{
	[AttributeUsage(AttributeTargets.Field, Inherited=false)]
	internal sealed class PerformanceCounterNameAttribute : Attribute
	{
		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get;
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set;
		}

		public PerformanceCounterNameAttribute(string name)
		{
			this.Name = name;
		}
	}
}