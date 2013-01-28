using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionProxySetCimInstance : CimSessionProxy
	{
		private bool passThru;

		public CimSessionProxySetCimInstance(CimSessionProxy originalProxy, bool passThru) : base(originalProxy)
		{
			this.passThru = passThru;
		}

		public CimSessionProxySetCimInstance(string computerName, CimInstance cimInstance, bool passThru) : base(computerName, cimInstance)
		{
			this.passThru = passThru;
		}

		public CimSessionProxySetCimInstance(CimSession session, bool passThru) : base(session)
		{
			this.passThru = passThru;
		}

		protected override bool PreNewActionEvent(CmdletActionEventArgs args)
		{
			DebugHelper.WriteLogEx();
			if (this.passThru || args.Action as CimWriteResultObject == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}