using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionProxyNewCimInstance : CimSessionProxy
	{
		private CimNewCimInstance newCimInstance;

		internal CimNewCimInstance NewCimInstanceOperation
		{
			get
			{
				return this.newCimInstance;
			}
		}

		public CimSessionProxyNewCimInstance(string computerName, CimNewCimInstance operation) : base(computerName)
		{
			this.newCimInstance = operation;
		}

		public CimSessionProxyNewCimInstance(CimSession session, CimNewCimInstance operation) : base(session)
		{
			this.newCimInstance = operation;
		}

		protected override bool PreNewActionEvent(CmdletActionEventArgs args)
		{
			DebugHelper.WriteLogEx();
			if (args.Action as CimWriteResultObject != null)
			{
				CimWriteResultObject action = args.Action as CimWriteResultObject;
				CimInstance result = action.Result as CimInstance;
				if (result != null)
				{
					object[] className = new object[2];
					className[0] = result.CimSystemProperties.ClassName;
					className[1] = result.CimSystemProperties.Namespace;
					DebugHelper.WriteLog("Going to read CimInstance classname = {0}; namespace = {1}", 1, className);
					this.NewCimInstanceOperation.GetCimInstance(result, base.ContextObject);
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}
	}
}