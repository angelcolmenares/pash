using Microsoft.Management.Infrastructure;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Threading;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimRegisterCimIndication : CimAsyncOperation
	{
		private Exception exception;

		internal Cmdlet Cmdlet
		{
			get;
			set;
		}

		internal Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		internal string TargetComputerName
		{
			get;
			set;
		}

		public CimRegisterCimIndication()
		{
			this.ackedEvent = new ManualResetEventSlim(false);
		}

		private void CimIndicationHandler(object cimSession, CmdletActionEventArgs actionArgs)
		{
			object[] action = new object[2];
			action[0] = actionArgs.Action;
			action[1] = base.Disposed;
			DebugHelper.WriteLogEx("action is {0}. Disposed {1}", 0, action);
			if (!base.Disposed)
			{
				CimWriteError cimWriteError = actionArgs.Action as CimWriteError;
				if (cimWriteError != null)
				{
					this.exception = cimWriteError.Exception;
					if (this.ackedEvent.IsSet)
					{
						EventHandler<CimSubscriptionEventArgs> eventHandler = this.OnNewSubscriptionResult;
						if (eventHandler != null)
						{
							DebugHelper.WriteLog("Raise an exception event", 2);
							eventHandler(this, new CimSubscriptionExceptionEventArgs(this.exception));
						}
						object[] objArray = new object[1];
						objArray[0] = this.exception;
						DebugHelper.WriteLog("Got an exception: {0}", 2, objArray);
					}
					else
					{
						DebugHelper.WriteLogEx("an exception happened", 0);
						this.ackedEvent.Set();
						return;
					}
				}
				CimWriteResultObject cimWriteResultObject = actionArgs.Action as CimWriteResultObject;
				if (cimWriteResultObject != null)
				{
					CimSubscriptionResult result = cimWriteResultObject.Result as CimSubscriptionResult;
					if (result == null)
					{
						if (this.ackedEvent.IsSet)
						{
							DebugHelper.WriteLogEx("an ack message should not happen here", 0);
						}
						else
						{
							DebugHelper.WriteLogEx("an ack message happened", 0);
							this.ackedEvent.Set();
							return;
						}
					}
					else
					{
						EventHandler<CimSubscriptionEventArgs> eventHandler1 = this.OnNewSubscriptionResult;
						if (eventHandler1 != null)
						{
							DebugHelper.WriteLog("Raise an result event", 2);
							eventHandler1(this, new CimSubscriptionResultEventArgs(result));
							return;
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private CimSessionProxy CreateSessionProxy(string computerName, uint timeout)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(computerName);
			cimSessionProxy.OperationTimeout = timeout;
			return cimSessionProxy;
		}

		private CimSessionProxy CreateSessionProxy(CimSession session, uint timeout)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(session);
			cimSessionProxy.OperationTimeout = timeout;
			return cimSessionProxy;
		}

		public void RegisterCimIndication(string computerName, string nameSpace, string queryDialect, string queryExpression, uint opreationTimeout)
		{
			object[] objArray = new object[2];
			objArray[0] = queryDialect;
			objArray[1] = queryExpression;
			DebugHelper.WriteLogEx("queryDialect = '{0}'; queryExpression = '{1}'", 0, objArray);
			this.TargetComputerName = computerName;
			CimSessionProxy cimSessionProxy = this.CreateSessionProxy(computerName, opreationTimeout);
			cimSessionProxy.SubscribeAsync(nameSpace, queryDialect, queryExpression);
			this.WaitForAckMessage();
		}

		public void RegisterCimIndication(CimSession cimSession, string nameSpace, string queryDialect, string queryExpression, uint opreationTimeout)
		{
			object[] objArray = new object[2];
			objArray[0] = queryDialect;
			objArray[1] = queryExpression;
			DebugHelper.WriteLogEx("queryDialect = '{0}'; queryExpression = '{1}'", 0, objArray);
			if (cimSession != null)
			{
				this.TargetComputerName = cimSession.ComputerName;
				CimSessionProxy cimSessionProxy = this.CreateSessionProxy(cimSession, opreationTimeout);
				cimSessionProxy.SubscribeAsync(nameSpace, queryDialect, queryExpression);
				this.WaitForAckMessage();
				return;
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = "cimSession";
				throw new ArgumentNullException(string.Format(CultureInfo.CurrentUICulture, Strings.NullArgument, objArray1));
			}
		}

		protected override void SubscribeToCimSessionProxyEvent(CimSessionProxy proxy)
		{
			DebugHelper.WriteLog("SubscribeToCimSessionProxyEvent", 4);
			proxy.OnNewCmdletAction += new CimSessionProxy.NewCmdletActionHandler(this.CimIndicationHandler);
			proxy.OnOperationCreated += new CimSessionProxy.OperationEventHandler(this.OperationCreatedHandler);
			proxy.OnOperationDeleted += new CimSessionProxy.OperationEventHandler(this.OperationDeletedHandler);
			proxy.EnableMethodResultStreaming = false;
		}

		private void WaitForAckMessage()
		{
			DebugHelper.WriteLogEx();
			this.ackedEvent.Wait();
			if (this.exception != null)
			{
				DebugHelper.WriteLogEx("error happened", 0);
				if (this.Cmdlet == null)
				{
					DebugHelper.WriteLogEx("Throw exception", 1);
					throw this.exception;
				}
				else
				{
					DebugHelper.WriteLogEx("Throw Terminating error", 1);
					ErrorRecord errorRecord = ErrorToErrorRecord.ErrorRecordFromAnyException(new InvocationContext(this.TargetComputerName, null), this.exception, null);
					this.Cmdlet.ThrowTerminatingError(errorRecord);
				}
			}
			DebugHelper.WriteLogEx("ACK happened", 0);
		}

		public event EventHandler<CimSubscriptionEventArgs> OnNewSubscriptionResult;
	}
}