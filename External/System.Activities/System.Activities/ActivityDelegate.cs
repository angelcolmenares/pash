using System;
using System.Windows.Markup;

namespace System.Activities
{
	[ContentProperty ("Handler")]
	public abstract class ActivityDelegate
	{
		public string DisplayName { get; set; }
		public Activity Handler { get; set; }
	}
}
