using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Management.Automation.Runspaces;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSessionBase
	{
		internal static ConcurrentDictionary<Guid, CimSessionState> cimSessions;

		internal static Guid defaultRunspaceId;

		internal CimSessionState sessionState;

		private static Guid CurrentRunspaceId
		{
			get
			{
				if (Runspace.DefaultRunspace == null)
				{
					return CimSessionBase.defaultRunspaceId;
				}
				else
				{
					return Runspace.DefaultRunspace.InstanceId;
				}
			}
		}

		static CimSessionBase()
		{
			CimSessionBase.cimSessions = new ConcurrentDictionary<Guid, CimSessionState>();
			CimSessionBase.defaultRunspaceId = Guid.Empty;
		}

		public CimSessionBase()
		{
			CimSessionBase orAdd = this;
			ConcurrentDictionary<Guid, CimSessionState> guids = CimSessionBase.cimSessions;
			Guid currentRunspaceId = CimSessionBase.CurrentRunspaceId;
			orAdd.sessionState = guids.GetOrAdd(currentRunspaceId, (Guid instanceId) => {
				if (Runspace.DefaultRunspace != null)
				{
					Runspace.DefaultRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(CimSessionBase.DefaultRunspace_StateChanged);
				}
				return new CimSessionState();
			}
			);
		}

		private static void DefaultRunspace_StateChanged(object sender, RunspaceStateEventArgs e)
		{
			CimSessionState cimSessionState = null;
			Runspace runspace = (Runspace)sender;
			RunspaceState state = e.RunspaceStateInfo.State;
			switch (state)
			{
				case RunspaceState.Closed:
				case RunspaceState.Broken:
				{
					if (CimSessionBase.cimSessions.TryRemove(runspace.InstanceId, out cimSessionState))
					{
						object[] instanceId = new object[2];
						instanceId[0] = runspace.InstanceId;
						instanceId[1] = e.RunspaceStateInfo.State;
						DebugHelper.WriteLog(string.Format(CultureInfo.CurrentUICulture, DebugHelper.runspaceStateChanged, instanceId));
						cimSessionState.Dispose();
					}
					runspace.StateChanged -= new EventHandler<RunspaceStateEventArgs>(CimSessionBase.DefaultRunspace_StateChanged);
					return;
				}
				case RunspaceState.Closing:
				{
					return;
				}
				default:
				{
					return;
				}
			}
		}

		public static CimSessionState GetCimSessionState()
		{
			CimSessionState cimSessionState = null;
			CimSessionBase.cimSessions.TryGetValue(CimSessionBase.CurrentRunspaceId, out cimSessionState);
			return cimSessionState;
		}
	}
}