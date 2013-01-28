using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimNewSession : CimSessionBase, IDisposable
	{
		private CimTestSession cimTestSession;

		private bool _disposed;

		protected bool Disposed
		{
			get
			{
				return this._disposed;
			}
		}

		internal CimNewSession()
		{
			this.cimTestSession = new CimTestSession();
			this._disposed = false;
		}

		internal void AddSessionToCache(CimSession cimSession, XOperationContextBase context, CmdletOperationBase cmdlet)
		{
			string str;
			DebugHelper.WriteLogEx();
			CimNewSession.CimTestCimSessionContext cimTestCimSessionContext = context as CimNewSession.CimTestCimSessionContext;
			uint num = this.sessionState.GenerateSessionId();
			string name = cimTestCimSessionContext.CimSessionWrapper.Name;
			if (name != null)
			{
				str = name;
			}
			else
			{
				object[] cimSessionClassName = new object[2];
				cimSessionClassName[0] = CimSessionState.CimSessionClassName;
				cimSessionClassName[1] = num;
				str = string.Format(CultureInfo.CurrentUICulture, "{0}{1}", cimSessionClassName);
			}
			string str1 = str;
			CimSession cimSession1 = cimTestCimSessionContext.Proxy.Detach();
			PSObject cache = this.sessionState.AddObjectToCache(cimSession1, num, cimSession1.InstanceId, str1, cimTestCimSessionContext.CimSessionWrapper.ComputerName, cimTestCimSessionContext.Proxy.Protocol);
			cmdlet.WriteObject(cache, null);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this._disposed && disposing)
			{
				this.cimTestSession.Dispose();
				this._disposed = true;
			}
		}

		internal void NewCimSession(NewCimSessionCommand cmdlet, CimSessionOptions sessionOptions, CimCredential credential)
		{
			string localhostComputerName;
			DebugHelper.WriteLogEx();
			IEnumerable<string> computerNames = ConstValue.GetComputerNames(cmdlet.ComputerName);
			foreach (string computerName in computerNames)
			{
				if (sessionOptions == null)
				{
					DebugHelper.WriteLog("Create CimSessionOption due to NewCimSessionCommand has null sessionoption", 1);
					sessionOptions = CimSessionProxy.CreateCimSessionOption(computerName, cmdlet.OperationTimeoutSec, credential);
				}
				CimSessionProxy cimSessionProxyTestConnection = new CimSessionProxyTestConnection(computerName, sessionOptions);
				if (computerName == ConstValue.NullComputerName)
				{
					localhostComputerName = ConstValue.LocalhostComputerName;
				}
				else
				{
					localhostComputerName = computerName;
				}
				string str = localhostComputerName;
				CimSessionWrapper cimSessionWrapper = new CimSessionWrapper(0, Guid.Empty, cmdlet.Name, str, cimSessionProxyTestConnection.CimSession, cimSessionProxyTestConnection.Protocol);
				CimNewSession.CimTestCimSessionContext cimTestCimSessionContext = new CimNewSession.CimTestCimSessionContext(cimSessionProxyTestConnection, cimSessionWrapper);
				cimSessionProxyTestConnection.ContextObject = cimTestCimSessionContext;
				SwitchParameter skipTestConnection = cmdlet.SkipTestConnection;
				if (!skipTestConnection.IsPresent)
				{
					this.cimTestSession.TestCimSession(computerName, cimSessionProxyTestConnection);
				}
				else
				{
					this.AddSessionToCache(cimSessionProxyTestConnection.CimSession, cimTestCimSessionContext, new CmdletOperationBase(cmdlet));
				}
			}
		}

		public void ProcessActions(CmdletOperationBase cmdletOperation)
		{
			this.cimTestSession.ProcessActions(cmdletOperation);
		}

		public void ProcessRemainActions(CmdletOperationBase cmdletOperation)
		{
			this.cimTestSession.ProcessRemainActions(cmdletOperation);
		}

		internal class CimTestCimSessionContext : XOperationContextBase
		{
			private CimSessionWrapper cimSessionWrapper;

			internal CimSessionWrapper CimSessionWrapper
			{
				get
				{
					return this.cimSessionWrapper;
				}
			}

			internal CimTestCimSessionContext(CimSessionProxy theProxy, CimSessionWrapper wrapper)
			{
				this.proxy = theProxy;
				this.cimSessionWrapper = wrapper;
				this.nameSpace = null;
			}
		}
	}
}