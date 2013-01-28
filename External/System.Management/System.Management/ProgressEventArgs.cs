using System;
using System.Runtime;

namespace System.Management
{
	public class ProgressEventArgs : ManagementEventArgs
	{
		private int upperBound;

		private int current;

		private string message;

		public int Current
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.current;
			}
		}

		public string Message
		{
			get
			{
				if (this.message != null)
				{
					return this.message;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public int UpperBound
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.upperBound;
			}
		}

		internal ProgressEventArgs(object context, int upperBound, int current, string message) : base(context)
		{
			this.upperBound = upperBound;
			this.current = current;
			this.message = message;
		}
	}
}