using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	public class CimIndicationEventInstanceEventArgs : CimIndicationEventArgs
	{
		private CimSubscriptionResult result;

		public string Bookmark
		{
			get
			{
				if (this.result == null)
				{
					return null;
				}
				else
				{
					return this.result.Bookmark;
				}
			}
		}

		public string MachineId
		{
			get
			{
				if (this.result == null)
				{
					return null;
				}
				else
				{
					return this.result.MachineId;
				}
			}
		}

		public CimInstance NewEvent
		{
			get
			{
				if (this.result == null)
				{
					return null;
				}
				else
				{
					return this.result.Instance;
				}
			}
		}

		public CimIndicationEventInstanceEventArgs(CimSubscriptionResult result)
		{
			this.context = null;
			this.result = result;
		}
	}
}