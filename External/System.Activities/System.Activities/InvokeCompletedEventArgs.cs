using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Activities
{
	[MonoTODO ("constructor")]
	public class InvokeCompletedEventArgs : AsyncCompletedEventArgs
	{
		internal InvokeCompletedEventArgs ()
			: base (null, false, null)
		{
		}

		public IDictionary<string, object> Outputs { get; private set; }
	}
}
