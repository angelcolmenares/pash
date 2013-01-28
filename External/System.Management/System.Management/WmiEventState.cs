using System;
using System.Threading;

namespace System.Management
{
	internal class WmiEventState
	{
		private Delegate d;

		private ManagementEventArgs args;

		private AutoResetEvent h;

		public ManagementEventArgs Args
		{
			get
			{
				return this.args;
			}
		}

		public AutoResetEvent AutoResetEvent
		{
			get
			{
				return this.h;
			}
		}

		public Delegate Delegate
		{
			get
			{
				return this.d;
			}
		}

		internal WmiEventState(Delegate d, ManagementEventArgs args, AutoResetEvent h)
		{
			this.d = d;
			this.args = args;
			this.h = h;
		}
	}
}