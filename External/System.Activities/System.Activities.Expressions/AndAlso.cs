using System;
using System.Activities;

namespace System.Activities.Expressions
{
	public sealed class AndAlso : Activity<bool>
	{
		public Activity<bool> Left { get; set; }
		public Activity<bool> Right { get; set; }
	}
}
