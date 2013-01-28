using Microsoft.Management.Infrastructure;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSubscriptionResultEventArgs : CimSubscriptionEventArgs
	{
		private CimSubscriptionResult result;

		public CimSubscriptionResult Result
		{
			get
			{
				return this.result;
			}
		}

		public CimSubscriptionResultEventArgs(CimSubscriptionResult theResult)
		{
			this.context = null;
			this.result = theResult;
		}
	}
}