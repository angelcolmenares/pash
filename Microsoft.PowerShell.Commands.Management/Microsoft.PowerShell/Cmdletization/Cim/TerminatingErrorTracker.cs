using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal class TerminatingErrorTracker
	{
		private readonly static ConditionalWeakTable<InvocationInfo, TerminatingErrorTracker> InvocationToTracker;

		private readonly int _numberOfSessions;

		private int _numberOfReportedSessionTerminatingErrors;

		private readonly ConcurrentDictionary<CimSession, bool> _sessionToIsConnected;

		private readonly ConcurrentDictionary<CimSession, bool> _sessionToIsTerminated;

		static TerminatingErrorTracker()
		{
			TerminatingErrorTracker.InvocationToTracker = new ConditionalWeakTable<InvocationInfo, TerminatingErrorTracker>();
		}

		private TerminatingErrorTracker(int numberOfSessions)
		{
			this._sessionToIsConnected = new ConcurrentDictionary<CimSession, bool>();
			this._sessionToIsTerminated = new ConcurrentDictionary<CimSession, bool>();
			this._numberOfSessions = numberOfSessions;
		}

		internal bool DidSessionAlreadyPassedConnectivityTest(CimSession session)
		{
			bool flag = false;
			if (!this._sessionToIsConnected.TryGetValue(session, out flag))
			{
				return false;
			}
			else
			{
				return flag;
			}
		}

        internal CmdletMethodInvoker<bool> GetErrorReportingDelegate(ErrorRecord errorRecord)
        {
            ManualResetEventSlim slim = new ManualResetEventSlim();
            object obj2 = new object();
            Func<Cmdlet, bool> func = delegate(Cmdlet cmdlet)
            {
                this._numberOfReportedSessionTerminatingErrors++;
                if (this._numberOfReportedSessionTerminatingErrors >= this._numberOfSessions)
                {
                    cmdlet.ThrowTerminatingError(errorRecord);
                }
                else
                {
                    cmdlet.WriteError(errorRecord);
                }
                return false;
            };
            return new CmdletMethodInvoker<bool> { Action = func, Finished = slim, SyncObject = obj2 };
        }

		internal Exception GetExceptionIfBrokenSession(CimSession potentiallyBrokenSession, bool skipTestConnection, out bool sessionWasAlreadyTerminated)
		{
			CimInstance cimInstance = null;
			CimException cimException = null;
			if (!this.IsSessionTerminated(potentiallyBrokenSession))
			{
				Exception exception = null;
				if (!skipTestConnection && !this.DidSessionAlreadyPassedConnectivityTest(potentiallyBrokenSession))
				{
					try
					{
						potentiallyBrokenSession.TestConnection(out cimInstance, out cimException);
						exception = cimException;
						if (exception == null)
						{
							this.MarkSessionAsConnected(potentiallyBrokenSession);
						}
					}
					catch (InvalidOperationException invalidOperationException1)
					{
						InvalidOperationException invalidOperationException = invalidOperationException1;
						exception = invalidOperationException;
					}
				}
				if (exception == null)
				{
					sessionWasAlreadyTerminated = false;
					return exception;
				}
				else
				{
					this.MarkSessionAsTerminated(potentiallyBrokenSession, out sessionWasAlreadyTerminated);
					return exception;
				}
			}
			else
			{
				sessionWasAlreadyTerminated = true;
				return null;
			}
		}

		private static int GetNumberOfSessions(InvocationInfo invocationInfo)
		{
			object obj = null;
			if (!invocationInfo.BoundParameters.TryGetValue("CimSession", out obj))
			{
				if (!invocationInfo.ExpectingInput)
				{
					int num = 1;
					foreach (object value in invocationInfo.BoundParameters.Values)
					{
						CimInstance[] cimInstanceArray = value as CimInstance[];
						if (cimInstanceArray == null)
						{
							continue;
						}
						int num1 = cimInstanceArray.Select<CimInstance, CimSession>(new Func<CimInstance, CimSession>(CimCmdletAdapter.GetSessionOfOriginFromCimInstance)).Distinct<CimSession>().Count<CimSession>();
						num = Math.Max(num, num1);
					}
					return num;
				}
				else
				{
					return 0x7fffffff;
				}
			}
			else
			{
				IList lists = (IList)obj;
				return lists.Count;
			}
		}

		internal static TerminatingErrorTracker GetTracker(InvocationInfo invocationInfo, bool isStaticCmdlet)
		{
			TerminatingErrorTracker value = TerminatingErrorTracker.InvocationToTracker.GetValue(invocationInfo, (InvocationInfo argument0) => new TerminatingErrorTracker(TerminatingErrorTracker.GetNumberOfSessions(invocationInfo)));
			return value;
		}

		internal static TerminatingErrorTracker GetTracker(InvocationInfo invocationInfo)
		{
			TerminatingErrorTracker terminatingErrorTracker = null;
			TerminatingErrorTracker.InvocationToTracker.TryGetValue(invocationInfo, out terminatingErrorTracker);
			return terminatingErrorTracker;
		}

		internal bool IsSessionTerminated(CimSession session)
		{
			bool orAdd = this._sessionToIsTerminated.GetOrAdd(session, false);
			return orAdd;
		}

		internal void MarkSessionAsConnected(CimSession connectedSession)
		{
			this._sessionToIsConnected.TryAdd(connectedSession, true);
		}

		internal void MarkSessionAsTerminated(CimSession terminatedSession, out bool sessionWasAlreadyTerminated)
		{
            bool flag = false;
			this._sessionToIsTerminated.AddOrUpdate(terminatedSession, true, (CimSession key, bool isTerminatedValueInDictionary) => {
				flag = isTerminatedValueInDictionary;
				return true;
			}
			);
			sessionWasAlreadyTerminated = flag;
		}
	}
}