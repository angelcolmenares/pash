using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimWriteMessage : CimBaseAction
	{
		private uint channel;

		private string message;

		public CimWriteMessage(uint channel, string message)
		{
			this.channel = channel;
			this.message = message;
		}

		public override void Execute(CmdletOperationBase cmdlet)
		{
			ValidationHelper.ValidateNoNullArgument(cmdlet, "cmdlet");
			CimWriteMessageChannel cimWriteMessageChannel = (CimWriteMessageChannel)this.channel;
			switch (cimWriteMessageChannel)
			{
				case CimWriteMessageChannel.Warning:
				{
					cmdlet.WriteWarning(this.message);
					return;
				}
				case CimWriteMessageChannel.Verbose:
				{
					cmdlet.WriteVerbose(this.message);
					return;
				}
				case CimWriteMessageChannel.Debug:
				{
					cmdlet.WriteDebug(this.message);
					return;
				}
				default:
				{
					return;
				}
			}
		}
	}
}