using Microsoft.Management.Infrastructure;
using System;
using System.ComponentModel;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	public class CimIndicationWatcher
	{
		private const string cimSessionParameterName = "cimSession";

		private const string queryExpressionParameterName = "queryExpression";

		private bool enableRaisingEvents;

		private CimRegisterCimIndication cimRegisterCimIndication;

		private CimIndicationWatcher.Status status;

		private object myLock;

		private string computerName;

		private CimSession cimSession;

		private string nameSpace;

		private string queryDialect;

		private string queryExpression;

		private uint opreationTimeout;

		[Browsable(false)]
		public bool EnableRaisingEvents
		{
			get
			{
				return this.enableRaisingEvents;
			}
			set
			{
				DebugHelper.WriteLogEx();
				if (value && !this.enableRaisingEvents)
				{
					this.enableRaisingEvents = value;
					this.Start();
				}
			}
		}

		public CimIndicationWatcher(string computerName, string theNamespace, string queryDialect, string queryExpression, uint operationTimeout)
		{
			ValidationHelper.ValidateNoNullorWhiteSpaceArgument(queryExpression, "queryExpression");
			computerName = ConstValue.GetComputerName(computerName);
			theNamespace = ConstValue.GetNamespace(theNamespace);
			this.Initialize(computerName, null, theNamespace, queryDialect, queryExpression, operationTimeout);
		}

		public CimIndicationWatcher(CimSession cimSession, string theNamespace, string queryDialect, string queryExpression, uint operationTimeout)
		{
			ValidationHelper.ValidateNoNullorWhiteSpaceArgument(queryExpression, "queryExpression");
			ValidationHelper.ValidateNoNullArgument(cimSession, "cimSession");
			theNamespace = ConstValue.GetNamespace(theNamespace);
			this.Initialize(null, cimSession, theNamespace, queryDialect, queryExpression, operationTimeout);
		}

		private void Initialize(string theComputerName, CimSession theCimSession, string theNameSpace, string theQueryDialect, string theQueryExpression, uint theOpreationTimeout)
		{
			this.enableRaisingEvents = false;
			this.status = CimIndicationWatcher.Status.Default;
			this.myLock = new object();
			this.cimRegisterCimIndication = new CimRegisterCimIndication();
			this.cimRegisterCimIndication.OnNewSubscriptionResult += new EventHandler<CimSubscriptionEventArgs>(this.NewSubscriptionResultHandler);
			this.cimSession = theCimSession;
			this.nameSpace = theNameSpace;
			this.queryDialect = ConstValue.GetQueryDialectWithDefault(theQueryDialect);
			this.queryExpression = theQueryExpression;
			this.opreationTimeout = theOpreationTimeout;
			this.computerName = theComputerName;
		}

		private void NewSubscriptionResultHandler(object src, CimSubscriptionEventArgs args)
		{
			EventHandler<CimIndicationEventArgs> eventHandler = this.CimIndicationArrived;
			if (eventHandler != null)
			{
				CimSubscriptionResultEventArgs cimSubscriptionResultEventArg = args as CimSubscriptionResultEventArgs;
				if (cimSubscriptionResultEventArg == null)
				{
					CimSubscriptionExceptionEventArgs cimSubscriptionExceptionEventArg = args as CimSubscriptionExceptionEventArgs;
					if (cimSubscriptionExceptionEventArg != null)
					{
						eventHandler(this, new CimIndicationEventExceptionEventArgs(cimSubscriptionExceptionEventArg.Exception));
					}
				}
				else
				{
					eventHandler(this, new CimIndicationEventInstanceEventArgs(cimSubscriptionResultEventArg.Result));
					return;
				}
			}
		}

		internal void SetCmdlet(Cmdlet cmdlet)
		{
			if (this.cimRegisterCimIndication != null)
			{
				this.cimRegisterCimIndication.Cmdlet = cmdlet;
			}
		}

		public void Start()
		{
			DebugHelper.WriteLogEx();
			lock (this.myLock)
			{
				if (this.status == CimIndicationWatcher.Status.Default)
				{
					if (this.cimSession != null)
					{
						this.cimRegisterCimIndication.RegisterCimIndication(this.cimSession, this.nameSpace, this.queryDialect, this.queryExpression, this.opreationTimeout);
					}
					else
					{
						this.cimRegisterCimIndication.RegisterCimIndication(this.computerName, this.nameSpace, this.queryDialect, this.queryExpression, this.opreationTimeout);
					}
					this.status = CimIndicationWatcher.Status.Started;
				}
			}
		}

		public void Stop()
		{
			object[] objArray = new object[1];
			objArray[0] = this.status;
			DebugHelper.WriteLogEx("Status = {0}", 0, objArray);
			lock (this.myLock)
			{
				if (this.status == CimIndicationWatcher.Status.Started)
				{
					if (this.cimRegisterCimIndication != null)
					{
						DebugHelper.WriteLog("Dispose CimRegisterCimIndication object", 4);
						this.cimRegisterCimIndication.Dispose();
					}
					this.status = CimIndicationWatcher.Status.Stopped;
				}
			}
		}

		public event EventHandler<CimIndicationEventArgs> CimIndicationArrived;
		internal enum Status
		{
			Default,
			Started,
			Stopped
		}
	}
}