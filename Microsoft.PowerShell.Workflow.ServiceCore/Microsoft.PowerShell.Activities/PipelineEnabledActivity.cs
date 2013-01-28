using System;
using System.Activities;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PipelineEnabledActivity : NativeActivity
	{
		[BehaviorCategory]
		[DefaultValue(null)]
		public bool? AppendOutput
		{
			get;
			set;
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InArgument<PSDataCollection<PSObject>> Input
		{
			get;
			set;
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InOutArgument<PSDataCollection<PSObject>> Result
		{
			get;
			set;
		}

		[DefaultValue(false)]
		[InputAndOutputCategory]
		public bool UseDefaultInput
		{
			get;
			set;
		}

		protected PipelineEnabledActivity()
		{
		}
	}
}